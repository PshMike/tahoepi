using System.Device.Gpio;
using System.Collections.Generic;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Iot.Device.CharacterLcd;
using Iot.Device.Ads1115;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text.Json;
using Jenne.TahoePI;
using Microsoft.Extensions.Configuration;
using Iot.Device.Usb;


var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfigurationRoot configuration = builder.Build();





Guid sessionGUID = Guid.NewGuid();
DateTime sessionStart = DateTime.Now;

string LogDirectoryRoot = System.IO.Path.Join(System.IO.Directory.GetCurrentDirectory(), "TahoePi-Logs");

try
{
    // Check if directory already exists
    if (!Directory.Exists(LogDirectoryRoot))
    {
        // Create the directory
        Directory.CreateDirectory(LogDirectoryRoot);
        Console.WriteLine("Directory created successfully at {0}.", LogDirectoryRoot);
    }
    else
    {
        Console.WriteLine("Directory already exists at {0}.", LogDirectoryRoot);
    }
}
catch (Exception e)
{
    Console.WriteLine("The process failed: {0}", e.ToString());
}


I2cConnectionSettings i2CConnectionSettings = new(1, 1);


// set I2C bus ID: 1
// ADS1115 Addr Pin connect to GND
I2cConnectionSettings settings0 = new I2cConnectionSettings(1, (int)I2cAddress.GND);
I2cDevice adc0 = I2cDevice.Create(settings0);

I2cConnectionSettings settings1 = new I2cConnectionSettings(1, (int)I2cAddress.VCC);
I2cDevice adc1 = I2cDevice.Create(settings1);




// pass in I2cDevice
// measure the voltage AIN0
// set the maximum range to 6.144V

Ads1115 voltagesensor0 = new Ads1115(adc0, InputMultiplexer.AIN0, MeasuringRange.FS4096);
voltagesensor0.DataRate = DataRate.SPS008;

Ads1115 voltagesensor1 = new Ads1115(adc1, InputMultiplexer.AIN0, MeasuringRange.FS4096);
voltagesensor1.DataRate = DataRate.SPS008;



var loopStart = DateTime.Now;
System.Collections.ArrayList Data = new System.Collections.ArrayList();

while (true)
{
    double voltagesensorfactor = 1580.0;
    double referencevoltage = 5.0;
    var str = configuration["5vBusVoltage"];
    Double.TryParse(str, out referencevoltage);

    int LogfileTimespan = 600;
    var str2 = configuration["LogfileTimespanSeconds"];
    int.TryParse(str2, out LogfileTimespan);

    int SamplesPerSecond = 1;
    var str3 = configuration["SamplesPerSecond"];
    int.TryParse(str3, out SamplesPerSecond);
    int sleepDuration = 1000 / SamplesPerSecond;





    var entry = new Jenne.TahoePI.Telemetry(sessionGUID, sessionStart);
    entry.TimeGenerated = DateTime.Now;
    TimeSpan timeSpan = DateTime.Now - entry.sessionStart;
    entry.sessionUptime = timeSpan.TotalSeconds;





    short rawvoltagesignal0 = 0;
    short rawvoltagesignal1 = 0;
    short rawvoltagesignal2 = 0;
    short rawvoltagesignal3 = 0;
    short rawvoltagesignal4 = 0;
    short rawvoltagesignal5 = 0;

    try
    {
        rawvoltagesignal0 = voltagesensor1.ReadRaw();
        voltagesensorfactor = rawvoltagesignal0 / referencevoltage;
        var voltage0 = Math.Round(1.0000 / voltagesensorfactor * rawvoltagesignal0, 3, MidpointRounding.AwayFromZero);
        Console.WriteLine($"ADS1115 Voltage Sensor 5v Bus Raw Data: {rawvoltagesignal0}  5v Bus Voltage: {voltage0}");
        Console.WriteLine($"voltagesensorfactor is {voltagesensorfactor}");
        entry.Samples.Add("5voltbus", voltage0);

    }
    catch
    {
        Console.WriteLine("unable to read voltage0");
    }

    try
    {
        rawvoltagesignal1 = voltagesensor0.ReadRaw();
        var voltage1 = Math.Round(1.0000 / voltagesensorfactor * rawvoltagesignal1, 3, MidpointRounding.AwayFromZero);
        Console.WriteLine($"ADS1115 Voltage Sensor 3.3v Bus Raw Data: {rawvoltagesignal1}  3.3v Bus Voltage: {voltage1}");
        entry.Samples.Add("cell1", voltage1);

    }
    catch
    {
        Console.WriteLine("unable to read voltage1");
    }





    Data.Add(entry);

    if (Data.Count > (LogfileTimespan * SamplesPerSecond))
    {
        var timestamp = DateTime.Now;
        string logFilename = timestamp.ToString("yyyyMMdd-HHmmss") + ".json";
        string logdirectory = timestamp.ToString("yyyyMMdd-HH00");

        string LogDirectoryName = System.IO.Path.Join(LogDirectoryRoot, logdirectory);
        if (!Directory.Exists(LogDirectoryName))
        {
            // Create the directory
            Directory.CreateDirectory(LogDirectoryName);
        }



        JsonSerializerOptions jso = new JsonSerializerOptions();
        jso.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;

        string jsonString = JsonSerializer.Serialize(Data, jso);

        string LogFileFullName = System.IO.Path.Join(LogDirectoryName, logFilename);
        File.WriteAllText(LogFileFullName, jsonString);
        Data.Clear();
        loopStart = DateTime.Now;

    }

    Thread.Sleep(sleepDuration);
}

