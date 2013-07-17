using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Blinky
{
    /// <summary>
    /// The LPD8806-based RGB Strip from Adafruit
    /// </summary>
    public class RGBStrip
    {
        SPI _spi;
        InterruptPort _switchPort;
        int _ledCount;
        byte[] _bytes;
        //byte[] _zeros = new byte[3];
        float[] _colorValues;

        /// <summary>
        /// Create a new instance of the RGBStrip.
        /// </summary>
        /// <param name="ledCount">Number of pixels (leds) in the strip.</param>
        /// <param name="spi">The spi object to use</param>
        public RGBStrip(int ledCount, SPI spi)
        {
            _spi = spi;
            DimFactor = 1;

            CreateBuffers(ledCount);
            SetupDimValueSwitch();
        }

        /// <summary>
        /// Create a new instance of the RGBStrip.
        /// </summary>
        /// <param name="ledCount">Number of pixels (leds) in the strip.</param>
        public RGBStrip(int ledCount)
        {
            DimFactor = 1;

            _spi = new SPI(new SPI.Configuration(Cpu.Pin.GPIO_NONE,
                false, 0, 0, false, true, 10000, SPI.SPI_module.SPI1));

            CreateBuffers(ledCount);
            SetupDimValueSwitch();
        }

        void CreateBuffers(int ledcount)
        {
            _ledCount = ledcount;
            _bytes = new byte[3 * ledcount + 3];    // 3 extra bytes as an end-of-message mark...
            _colorValues = new float[3 * ledcount];
        }

        void SetupDimValueSwitch()
        {
            _switchPort = new InterruptPort(
                Pins.ONBOARD_SW1,
                true,
                Port.ResistorMode.Disabled,
                Port.InterruptMode.InterruptEdgeLow);

            _switchPort.OnInterrupt += new NativeEventHandler(switchPort_OnInterrupt);
        }

        /// Event handler for the onboard switch.    
        /// </summary>    
        /// <param name="port">The port for which the event occurs.    
        /// <param name="data">The state of the switch.    
        /// <param name="time">Time of the event.    
        private void switchPort_OnInterrupt(uint port, uint data, DateTime time)
        {
            DimFactor++;
            if (DimFactor == 5)
            {
                DimFactor = 0;
            }
        }

        /// <summary>
        /// Reduces the intensity for debugging purposes.
        /// </summary>
        /// <remarks>
        /// Sets the number of times we divide the intensity value by 2. Try starting at 1 and moving to 2 if that is
        /// still too bright. 
        /// 
        /// Using DimFactor will result in some jerkiness in color transitions. 
        /// </remarks>
        public int DimFactor { get; set; }

        /// <summary>
        /// Set the color values for a specific LED
        /// </summary>
        /// <param name="ledNumber">The LED to set</param>
        /// <param name="red">The red value.</param>
        /// <param name="green">The green value.</param>
        /// <param name="blue">The blue value.</param>
        public void SetLedColorsRaw(int ledNumber, int red, int green, int blue)
        {
            ledNumber = ledNumber * 3;

            _bytes[ledNumber] = (byte)(0x80 | green);
            _bytes[ledNumber + 1] = (byte)(0x80 | red);
            _bytes[ledNumber + 2] = (byte)(0x80 | blue);
        }

        /// <summary>
        /// Set the color values for a specific LED, including DimFactor support.
        /// </summary>
        /// <remarks>
        /// The DimFactor dims down the LEDs so they aren't so bright when you are developing your
        /// animations. In Raw mode, dimming only happens on newly-written values; existing values are
        /// not affected.
        /// </remarks>
        /// <param name="ledNumber">The LED to set</param>
        /// <param name="red">The red value.</param>
        /// <param name="green">The green value.</param>
        /// <param name="blue">The blue value.</param>
        public void SetLedColorsRawWithDim(int ledNumber, int red, int green, int blue)
        {
            ledNumber = ledNumber * 3;

            _bytes[ledNumber] = (byte)(0x80 | (green >> DimFactor));
            _bytes[ledNumber + 1] = (byte)(0x80 | (red >> DimFactor));
            _bytes[ledNumber + 2] = (byte)(0x80 | (blue >> DimFactor));
        }

        /// <summary>
        /// Do one animation step for a specific animation.
        /// </summary>
        /// <param name="animation">The animation.</param>
        public void DoAnimationStep(Animation animation)
        {
            int index = animation.LedNumber * 3;

            float greenCurrent = _colorValues[index];
            float redCurrent = _colorValues[index + 1];
            float blueCurrent = _colorValues[index + 2];

            if (animation.Current == 0)
            {
                animation.RedIncrement = (animation.RedTarget - redCurrent) / animation.Steps;
                animation.GreenIncrement = (animation.GreenTarget - greenCurrent) / animation.Steps;
                animation.BlueIncrement = (animation.BlueTarget - blueCurrent) / animation.Steps;
            }

            _colorValues[index] = greenCurrent + animation.GreenIncrement;
            _colorValues[index + 1] = redCurrent + animation.RedIncrement;
            _colorValues[index + 2] = blueCurrent + animation.BlueIncrement;
        }

        /// <summary>
        /// Write the raw color values to the strip.
        /// </summary>
        public void WriteToStripRaw()
        {
            _spi.Write(_bytes);
        }

        /// <summary>
        /// Write Animator-based color values to the strip.
        /// </summary>
        public void WriteToStrip()
        {
            //Stopwatch stopwatch = new Stopwatch();
            //Stopwatch stopwatch2 = new Stopwatch();
            //stopwatch.Start();
            int end = _ledCount * 3;

            for (int i = 0; i < end; i++)
            {
                // OPT: Measure the speed of each section of this...
                //stopwatch2.Start();
                _bytes[i] = (byte)(0x80 | ((int)_colorValues[i]) >> DimFactor);
                //stopwatch2.StopAndPrint("expression");

                //stopwatch2.Start();
                //float f = _colorValues[i];
                ///stopwatch2.StopAndPrint("float array index");
                //stopwatch2.Start();
                //int v = (int)f;
                //stopwatch2.StopAndPrint("float to int");
                //stopwatch2.Start();
                //int d = v >> DimFactor;
                //stopwatch2.StopAndPrint("shift by dimfactor");
                //stopwatch2.Start();
                //byte b = (byte)(0x80 | d);
                //stopwatch2.StopAndPrint("OR with 0x80");
                //stopwatch2.Start();
                //_bytes[i] = b;
                //stopwatch2.StopAndPrint("store into byte array");
            }
            //stopwatch.StopAndPrint("Copy");

            //stopwatch.Start();
            _spi.Write(_bytes);
            //stopwatch.StopAndPrint("SPI write:");
            //_spi.Write(_zeros);
        }

        /// <summary>
        ///  Clear all the raw-values, and update the strip.
        /// </summary>
        public void ClearAllRaw()
        {
            for (int i = 0; i < _ledCount * 3; i++)
            {
                _bytes[i] = 0x80;
            }
            WriteToStripRaw();
        }

        /// <summary>
        /// Clear all the animator-based values, and update the strip.
        /// </summary>
        public void ClearAll()
        {
            for (int i = 0; i < _ledCount * 3; i++)
            {
                _colorValues[i] = 0;
            }
            WriteToStrip();
        }
    }
}