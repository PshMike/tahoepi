using System.Device.Gpio;
using System.Collections.Generic;
using Iot.Device.GrovePiDevice.Models;
using Iot.Device.GrovePiDevice;
using Iot.Device.GrovePiDevice.Sensors;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Mcp25xxx.Register.MessageReceive;




// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
Console.WriteLine("Line 2");



I2cConnectionSettings i2CConnectionSettings = new(1, GrovePi.DefaultI2cAddress);
using GrovePi grovePi = new GrovePi(I2cDevice.Create(i2CConnectionSettings));

GrovePort portTempSensor  = GrovePort.AnalogPin0;
GrovePort portLightSensor = GrovePort.AnalogPin1;
GrovePort portRelay = GrovePort.DigitalPin4;
GrovePort portLed = GrovePort.DigitalPin3;

GroveTemperatureSensor s1 = new GroveTemperatureSensor(grovePi,portTempSensor);
Relay r1 = new Relay(grovePi,portRelay);
LightSensor s2 = new LightSensor(grovePi,portLightSensor);
DigitalOutput led = new DigitalOutput(grovePi,portLed);


Console.WriteLine($"Manufacturer :{grovePi.GrovePiInfo.Manufacturer}");
Console.WriteLine($"Board: {grovePi.GrovePiInfo.Board}");
Console.WriteLine($"Firmware version: {grovePi.GrovePiInfo.SoftwareVersion}");



bool relayClosed = false;

while (true)
{
    Console.Clear();
    //sensorReading = grovePi.AnalogRead(tempsensorport);
    string tempC = s1.Value.ToString();
    int lightLevel = s2.ValueAsPercent;

    double R0 = 100000;
    
    double R = 1023.0 / s1.Value - 1.0;
    R = R0 * R;

    int B = 4275;

    double temp = 1 / (Math.Log( R/R0 )/B+1/298.15) -273.15;
    temp = temp * 1.8 + 32;


    Console.WriteLine(temp.ToString());
    Console.WriteLine(tempC);
    Console.WriteLine(lightLevel);

    if ( relayClosed ) 
    {
        r1.Off();
    }
    else 
    {
        r1.On();
    }

    if ( 75 < temp ) 
    {
        led.Value = PinValue.High;
    }
    else
    {
        led.Value = PinValue.Low;
    }



    Thread.Sleep(2000);
}

