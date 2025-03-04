using System.Device.Gpio;
using System.Collections.Generic;
using Iot.Device.GrovePiDevice.Models;
using Iot.Device.GrovePiDevice;
using Iot.Device.GrovePiDevice.Sensors;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Iot.Device.CharacterLcd;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text.Json;
using Jenne.TahoePI;

Guid sessionGUID = Guid.NewGuid();
DateTime sessionStart = DateTime.Now;

string LogDirectoryName = System.IO.Path.Join(System.IO.Directory.GetCurrentDirectory(), "TahoePi-Logs");

try
{
    // Check if directory already exists
    if (!Directory.Exists(LogDirectoryName))
    {
        // Create the directory
        Directory.CreateDirectory(LogDirectoryName);
        Console.WriteLine("Directory created successfully at {0}.", LogDirectoryName);
    }
    else
    {
        Console.WriteLine("Directory already exists at {0}.", LogDirectoryName);
    }
}
catch (Exception e)
{
    Console.WriteLine("The process failed: {0}", e.ToString());
}



I2cConnectionSettings i2CConnectionSettings = new(1, GrovePi.DefaultI2cAddress);
using GrovePi grovePi = new GrovePi(I2cDevice.Create(i2CConnectionSettings));

GrovePort portTempSensor = GrovePort.AnalogPin0;
GrovePort portLightSensor = GrovePort.AnalogPin1;

GroveTemperatureSensor s1 = new GroveTemperatureSensor(grovePi, portTempSensor);
LightSensor s2 = new LightSensor(grovePi, portLightSensor);


Console.WriteLine($"Manufacturer :{grovePi.GrovePiInfo.Manufacturer}");
Console.WriteLine($"Board: {grovePi.GrovePiInfo.Board}");
Console.WriteLine($"Firmware version: {grovePi.GrovePiInfo.SoftwareVersion}");

var loopStart = DateTime.Now;
System.Collections.ArrayList Data = new System.Collections.ArrayList();

while (true)
{
    loopStart = DateTime.Now;

    var entry = new Jenne.TahoePI.Telemetry(sessionGUID, sessionStart);
    entry.TimeGenerated = DateTime.Now;
    TimeSpan timeSpan = DateTime.Now - entry.sessionStart;
    entry.sessionUptime = timeSpan.TotalSeconds;


    int lightLevel = s2.ValueAsPercent;


    entry.Samples.Add("Light", lightLevel);

    double R0 = 100000;
    double R = 1023.0 / s1.Value - 1.0;
    R = R0 * R;
    int B = 4275;
    double temp = 1 / (Math.Log(R / R0) / B + 1 / 298.15) - 273.15;
    temp = temp * 1.8 + 32;
    entry.Samples.Add("Temp", temp);
    Data.Add(entry);

    if (Data.Count > 300)
    {
            string logFilename = DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json";
            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;

            string jsonString = JsonSerializer.Serialize(Data, jso);

            string LogFileFullName = System.IO.Path.Join(LogDirectoryName, logFilename);
            File.WriteAllText(LogFileFullName, jsonString);
            Data.Clear();
            loopStart = DateTime.Now;

    }

    Thread.Sleep(1000);
}

