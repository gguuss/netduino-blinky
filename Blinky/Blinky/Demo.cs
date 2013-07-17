using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;

namespace Blinky
{
    /// <summary>
    /// This demo class tests the various features of this project.
    /// When the class is constructed, it starts the demo, which can not
    /// be cancelled. 
    /// Pressing the button on the Netduino will change the mode to render
    /// various tests / patterns.
    /// </summary>
    class Demo
    {
        byte numLEDs = 32;
        byte numStrips = 5;
        int mode = 0;
        int numModes = 7;
        bool changeMode = false;
        SPI spi;
        LedStripUtils utils;

        /// <summary>
        /// Default constructor starts the demo which cannot be cancelled.
        /// </summary>
        public Demo()
        {
            var button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, InterruptModes.InterruptEdgeHigh);
            spi = new SPI(new SPI.Configuration(Cpu.Pin.GPIO_NONE, false, 0, 0, false, true, 10000, SPI.SPI_module.SPI1));
            button.OnInterrupt += ChangeMode;

            utils = new LedStripUtils(numLEDs, numStrips, spi);

            while (true)
            {
                switch (mode)
                {
                    case 0:
                        AdvancedMaskTest();
                        break;
                    case 1:
                        MaskTest();
                        break;
                    case 2:
                        RGBTest();
                        break;
                    case 3:
                        rainbowCycle(10000);
                        break;
                    case 4:
                        cylonCycle(1000, 20);
                        break;
                    case 5:
                        EqualizerTest();
                        break;
                    case 6:
                        rainbowCylon(100);
                        break;
                    default:
                        mode = 0;
                        break;
                }
            }
        }

        /// <summary>
        /// Changes the mode of the demo.
        /// </summary>
        /// <param name="data1">Unused.</param>
        /// <param name="data2">Unused.</param>
        /// <param name="time">Unused.</param>
        void ChangeMode(uint data1, uint data2, DateTime time)
        {
            mode++;
            if (mode > numModes) mode = 0;
            changeMode = true;
            utils.BlankAll();
        }

        /// <summary>
        /// Tests rendering a "mask" which is a template that contains
        /// booleans indicating the pixel should be drawn.
        /// </summary>
        public void MaskTest()
        {
            // Allocate the "mask" boolean arrays.
            byte[][] maskOdd = new byte[numStrips][];
            byte[][] maskEven = new byte[numStrips][];

            for (int strip = 0; strip < numStrips; strip++)
            {
                maskOdd[strip] = new byte[numLEDs];
                maskEven[strip] = new byte[numLEDs];
                for (int led = 0; led < numLEDs; led++)
                {
                    maskOdd[strip][led] = (byte)(((led & 1) == 0) ? 0 : 1);
                }
            }

            byte[] strips = new byte[3 * numLEDs * numStrips];

            for (int strip = 0; strip < numStrips; strip++)
            {
                for (int i = 0; i < numLEDs; i++)
                {
                    utils.SetColor(i + (numLEDs * strip), utils.RGB(255, 0, 0), strips);
                }
            }

            bool odd = true;
            while (!changeMode)
            {
                byte[] toWrite;
                if (!odd)
                {
                    toWrite = utils.PixelsFromMask(maskEven, strips);
                    odd = true;
                }
                else
                {
                    toWrite = utils.PixelsFromMask(maskOdd, strips);
                    odd = false;
                }
                spi.Write(toWrite);
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Tests rendering a "mask" which is a template that contains
        /// booleans indicating the pixel should be drawn.
        /// </summary>
        public void AdvancedMaskTest()
        {
            byte[] strips = new byte[numLEDs * numStrips * 3];

            /*
            // GUS PWNS
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 1, 0},
                new byte[] { 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0},
                new byte[] { 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0},
                new byte[] { 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0},
                new byte[] { 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0}};
            */
            
            /*
            // Chevrons, 5 strips
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0},
                new byte[] { 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0},
                new byte[] { 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0},
                new byte[] { 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0},
                new byte[] { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0}};
             */
            // Stripes, 5 strips
            /*
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0},
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0},
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0},
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0},
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0}};
             */
            // Allocate the "mask" boolean arrays.
            /*
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0},
                new byte[] { 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1},
                new byte[] { 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0},
                new byte[] { 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0}};
            */

            /*
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0 },
                new byte[] { 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1 },
                new byte[] { 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1 },
                new byte[] { 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0 }};
             */

            /*
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 0, 1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0 },
                new byte[] { 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 0, 1 },
                new byte[] { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1 },
                new byte[] { 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0 }};
            */
            for (int strip = 0; strip < numStrips; strip++)
            {
                for (int i = 0; i < numLEDs; i++)
                {
                    utils.SetColor(i + (numLEDs * strip), utils.RGB(255, 0, 0), strips);
                }
            }

            while (!changeMode)
            {
                spi.Write(utils.PixelsFromMask(mask,strips));
                mask = utils.RotateMask(mask);
                Thread.Sleep(100);
            }
        }


        /// <summary>
        /// A test that covers the ability to use LED strips. Draws
        /// a rainbow up and down each LED strip.
        /// </summary>
        public void EqualizerTest()
        {
        
            byte[] strip = new byte[3 * numLEDs];
            byte[] strips = new byte[3 * numLEDs * numStrips];
            int pos = 0;
            bool up = true;
            while (!changeMode)
            {
                if (pos == numLEDs)
                {
                    up = false;
                    pos = numLEDs - 1;
                }
                if (pos < 0)
                {
                    up = true;
                    pos = 0;
                }

                byte i = 0;
                while (i < pos)
                {

                    utils.SetColor(i, utils.Wheel(255 - ((255 / numLEDs) * i)), strip);
                    i++;
                }
                while (i < numLEDs)
                {
                    utils.SetColor(i, utils.RGB(0, 0, 0), strip);
                    i++;
                }
                for (byte stripNumber = 0; stripNumber < numStrips; stripNumber++)
                {
                    bool reverse = ((stripNumber & 1) == 1);
                    utils.WriteStrip(stripNumber, strip, strips, reverse);
                }
                spi.Write(strips);
                if (up)
                {
                    pos++;
                }
                else
                {
                    pos--;
                }
            }
            changeMode = false;
        }

        /// <summary>
        /// Fade a cylon pattern along the chain. [Yeah BSG :)]
        /// </summary>
        /// <param name="wait">Time to wait before updating position (milliseconds).</param>
        /// <param name="width">Width of the Cylon bar (pixels).</param>
        public void cylonCycle(int wait, int width)
        {
            var colors = new byte[3 * numLEDs];

            int pos = 0;

            int offsetLimit = 255;
            int offset = offsetLimit;
            int steps = numLEDs;
            while (!changeMode)
            {
                while (pos <= numLEDs)
                {

                    if (offset > offsetLimit)
                    {
                        offset = 0;
                    }

                    // Write colors
                    for (byte i = 0; i < numLEDs; ++i)
                    {
                        if (i >= pos & (i < pos + width))
                        {
                            byte[] color = utils.Wheel(i + offset);
                            utils.SetColor(i, color, colors);
                        }
                        else
                        {
                            byte[] color = utils.RGB(0, 0, 0);
                            utils.SetColor(i, color, colors);
                        }
                    }
                    spi.Write(colors);
                    offset += offsetLimit / steps;
                    Thread.Sleep(wait / steps); // march at 32 pixels per second
                    pos++;
                }
                while (pos >= 0)
                {
                    // Write colors
                    for (byte i = 0; i < numLEDs; ++i)
                    {
                        if (i >= pos & (i < pos + width))
                        {
                            byte[] color = utils.Wheel(i + offset);
                            utils.SetColor(i, color, colors);
                        }
                        else
                        {
                            byte[] color = utils.RGB(0, 0, 0);
                            utils.SetColor(i, color, colors);
                        }
                    }
                    spi.Write(colors);
                    offset -= offsetLimit / steps;
                    Thread.Sleep(wait / steps); // march at 32 pixels per second
                    pos--;
                }
            }
            changeMode = false;
        }

        /// <summary>
        /// Test cycles of colors generated using the Wheel function.
        /// </summary>
        /// <param name="wait">Time to wait between updates.</param>
        public void CycleTest(int wait)
        {
            var colors = new byte[3 * 32];

            int offsetLimit = 255;
            int offset = offsetLimit;
            int steps = 255;

            while (!changeMode)
            {
                // Write colors
                for (byte i = 0; i < numLEDs; ++i)
                {
                    byte[] thisColor = utils.Wheel(i + offset);
                    colors[(i * 3)] = thisColor[0];
                    colors[(i * 3) + 1] = thisColor[1];
                    colors[(i * 3) + 2] = thisColor[2];
                }
                spi.Write(colors);
                offset += offsetLimit / steps;
                Thread.Sleep(wait / steps); // march at 32 pixels per second
            }
            changeMode = false;
        }

        /// <summary>
        /// Fade a rainbow along the chain
        /// </summary>
        /// <param name="wait">Time to wait between updates to the rainbow.</param>
        public void rainbowCycle(int wait)
        {
            var colors = new byte[3 * 32];
            var zeros = new byte[3 * ((32 + 63) / 64)];

            int offsetLimit = 255;
            int offset = offsetLimit;
            int steps = 255;
            while (!changeMode)
            {
                if (offset > offsetLimit)
                {
                    offset = 0;
                }

                // Write colors
                for (byte i = 0; i < 32; ++i)
                {
                    byte[] color = utils.Wheel(i + offset);
                    colors[(i * 3)] = color[0];
                    colors[(i * 3) + 1] = color[1];
                    colors[(i * 3) + 2] = color[2];
                }
                spi.Write(colors);
                offset += offsetLimit / steps;
                Thread.Sleep(wait / steps); // march at 32 pixels per second
            }
            changeMode = false;
        }

        /// <summary>
        /// Fade a rainbow along the chain
        /// </summary>
        /// <param name="wait">Time to wait between rainbow cycles.</param>
        public void rainbowCylon(int wait)
        {
            var colors = new byte[3 * 32];
            var zeros = new byte[3 * ((32 + 63) / 64)];

            int offsetLimit = 255;
            int offset = offsetLimit;
            int steps = 255;
            while (!changeMode)
            {
                if (offset > offsetLimit)
                {
                    offset = 0;
                }

                // Write colors
                for (byte i = 0; i < 32; ++i)
                {
                    byte[] color = utils.Wheel(i + offset);
                    colors[(i * 3)] = color[0];
                    colors[(i * 3) + 1] = color[1];
                    colors[(i * 3) + 2] = color[2];
                }
                spi.Write(colors);
                offset += offsetLimit / steps;
                Thread.Sleep(wait / steps); // march at 32 pixels per second
            }
            changeMode = false;
        }

        /// <summary>
        /// Just testing splitting the strip to R / G / B colors.
        /// </summary>
        public void RGBTest()
        {
            var colors = new byte[3 * numLEDs];

            byte offset = 255;
            while (!changeMode)
            {
                if (offset > 255)
                {
                    offset = 0;
                }
                // all pixels off
                for (int i = 0; i < colors.Length; ++i) colors[i] = (byte)(0);
                // a progressive yellow/red blend
                for (byte i = 0; i < numLEDs; ++i)
                {
                    byte[] color = new byte[3];
                    if (i < (numLEDs / 3))
                    {
                        color = utils.RGB(offset, 0, 0);
                    }
                    else if (i < ((numLEDs * 2) / 3))
                    {
                        color = utils.RGB(0, offset, 0);
                    }
                    else
                    {
                        color = utils.RGB(0, 0, offset);
                    }
                    for (byte j = 0; j < 3; j++)
                    {
                        colors[(3 * i) + j] = color[j];
                    }
                }
                spi.Write(colors);

                Thread.Sleep(1000 / numLEDs); // march at 32 pixels per second
            }
            changeMode = false;
        }

    }
}
