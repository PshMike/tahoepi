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



I2cConnectionSettings i2CConnectionSettings = new(1, GrovePi.DefaultI2cAddress);
using GrovePi grovePi = new GrovePi(I2cDevice.Create(i2CConnectionSettings));

GrovePort portTempSensor  = GrovePort.AnalogPin0;
GrovePort portLightSensor = GrovePort.AnalogPin1;

GroveTemperatureSensor s1 = new GroveTemperatureSensor(grovePi,portTempSensor);
LightSensor s2 = new LightSensor(grovePi,portLightSensor);


Console.WriteLine($"Manufacturer :{grovePi.GrovePiInfo.Manufacturer}");
Console.WriteLine($"Board: {grovePi.GrovePiInfo.Board}");
Console.WriteLine($"Firmware version: {grovePi.GrovePiInfo.SoftwareVersion}");



Log.Logger = new LoggerConfiguration()
.WriteTo.File("tahoe-logs.log")
.CreateLogger();







while (true)
{
    //sensorReading = grovePi.AnalogRead(tempsensorport);
    int lightLevel = s2.ValueAsPercent;

    double R0 = 100000;
    
    double R = 1023.0 / s1.Value - 1.0;
    R = R0 * R;

    int B = 4275;

    double temp = 1 / (Math.Log( R/R0 )/B+1/298.15) -273.15;
    temp = temp * 1.8 + 32;

    Log.Information("Temp is {0}", temp.ToString());
    Log.Information("LightLevel is {0}", lightLevel.ToString());

    Console.WriteLine(temp.ToString());
    Console.WriteLine(lightLevel);
    Console.WriteLine();

    Thread.Sleep(30000);
}

