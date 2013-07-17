using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Blinky
{
    class LedStripUtils
    {
        int _numLEDs, _numStrips;
        SPI _spi;

        public LedStripUtils(int numLEDs, byte numStrips, SPI spi)
        {
            _numLEDs = numLEDs;
            _numStrips = numStrips;
            _spi = spi;
        }

        /// <summary>
        /// Set a specific pixel color on a strip.
        /// </summary>
        /// <param name="pixel">The pixel (0 ... numLEDs).</param>
        /// <param name="color">The color to set the pixel e.g. (RBG(255,0,0).</param>
        /// <param name="colors">The byte array representing all of the colors.</param>
        public void SetColor(int pixel, byte[] color, byte[] colors)
        {
            colors[(pixel * 3)] = color[0];
            colors[(pixel * 3) + 1] = color[1];
            colors[(pixel * 3) + 2] = color[2];
        }

        /// <summary>
        /// Gets the byte array representing the color at a pixel index.
        /// </summary>
        /// <param name="pixel">The pixel to get the color from.</param>
        /// <param name="strips">The byte array representing the strip colors.</param>
        /// <returns></returns>
        public byte[] GetColor(int pixel, byte[] strips)
        {
            byte[] toReturn = new byte[3];
            int offset = pixel * 3;
            toReturn[0] = strips[offset];
            toReturn[1] = strips[offset + 1];
            toReturn[2] = strips[offset + 2];
            return toReturn;
        }

        /// <summary>
        /// Get an array representing an RGB color.
        /// </summary>
        /// <param name="r">The red value (0..255).</param>
        /// <param name="g">The green value (0..255).</param>
        /// <param name="b">The blue value (0..255).</param>
        /// <returns></returns>
        public byte[] RGB(byte r, byte g, byte b)
        {
            byte[] toReturn = new byte[3];
            toReturn[2] = r;
            toReturn[1] = g;
            toReturn[0] = b;
            return toReturn;
        }

        /// <summary>
        /// Based on the Adafruit utility function for generating a color
        /// on a "color wheel" based on a value.
        /// </summary>
        /// <param name="WheelPos">The position (0-255) on the color wheel.</param>
        /// <returns>A byte array representing the color.</returns>
        public byte[] Wheel(int WheelPos)
        {
            byte[] toReturn = new byte[3];
            switch (WheelPos / 128)
            {
                case 0:
                    toReturn[0] = (byte)(127 - WheelPos % 128); // red down
                    toReturn[1] = (byte)(WheelPos % 128);       // green up
                    toReturn[2] = 0;                            // blue off
                    break;
                case 1:
                    toReturn[0] = 0;                            // red off
                    toReturn[1] = (byte)(127 - WheelPos % 128); // green down
                    toReturn[2] = (byte)(WheelPos % 128);       // blue up

                    break;
                case 2:
                    toReturn[0] = (byte)(WheelPos % 128);       // red up
                    toReturn[1] = 0;                            // green off
                    toReturn[2] = (byte)(127 - WheelPos % 128); // blue down 
                    break;
            }
            return toReturn;
        }

        /// <summary>
        /// Renders a mask over a set of pixels to occlude some of the pixels.
        /// </summary>
        /// <param name="mask">A 2D array of booleans representing the mask where
        /// true will render the pixel, false will not.</param>
        /// <param name="strips">The colors that will be masked.</param>
        /// <returns>A byte array that can be written to SPI.</returns>
        public byte[] PixelsFromMask(byte[][] mask, byte[] strips)
        {
            byte[] colors = new byte[3 * _numLEDs * _numStrips];
            byte[] thisStrip = new byte[3 * _numLEDs];
            //BlankAll();
            for (int strip = 0; strip < _numStrips; strip++)
            {
                for (int led = 0; led < _numLEDs; led++)
                {
                    if ((mask[strip][led] & 1) == 1)
                    {
                        SetColor(led, GetColor(led + (strip * _numLEDs), strips), thisStrip);
                    }
                    else
                    {
                        SetColor(led, RGB(0, 0, 0), thisStrip);
                    }
                }
                WriteStrip(strip, thisStrip, colors, ((strip & 1) == 1));
            }
            return colors;
        }

        public byte[][] RotateMask(byte[][] mask)
        {
            byte[][] newMask = new byte[_numStrips][];

            byte[] colors = new byte[3 * _numLEDs * _numStrips];
            byte[] thisStrip = new byte[3 * _numLEDs];
            for (int strip = 0; strip < _numStrips; strip++)
            {
                newMask[strip] = new byte[_numLEDs];
                newMask[strip][0] = mask[strip][mask.Length - 1];
                for (int led = 0; led < (_numLEDs -1); led++)
                {
                    newMask[strip][led+1] = mask[strip][led];
                }
            }
            return newMask;
        }

        /// <summary>
        /// When using strips of LEDs, it can be convenient to write to one strip at a time.
        /// This function will write an array of bytes to a given strip taking into account
        /// whether the strip is reversed in your larger LED chain.
        /// </summary>
        /// <param name="stripNumber">The position to write to (starts at 0).</param>
        /// <param name="strip">An array representing the colors to write for this strip.</param>
        /// <param name="strips">An array representing the whole LED display values.</param>
        /// <param name="reverse">Indicates whether you are reversing this strip. For example,
        /// if you were reversing the odd strips, you could use:
        /// WriteStrip(..., ((stripNumber & 1) == 1)</param>
        public void WriteStrip(int stripNumber, byte[] strip, byte[] strips, bool reverse)
        {
            int offset = stripNumber * 3 * _numLEDs;
            if (!reverse)
            {
                // e.g. i = 1 ... 32
                for (int i = 0; i < _numLEDs; ++i)
                {
                    strips[offset + (3 * i)] = strip[(3 * i)];
                    strips[(offset + (3 * i)) + 1] = strip[(3 * i) + 1];
                    strips[(offset + (3 * i)) + 2] = strip[(3 * i) + 2];
                }
            }
            else
            {
                for (int i = (_numLEDs - 1); i >= 0; i--)
                {
                    int lastIndex =
                    strips[offset + (3 * i)] = strip[(((_numLEDs) * 3) - ((3 * i) + 2) - 1)];
                    strips[(offset + (3 * i)) + 1] = strip[(((_numLEDs) * 3) - ((3 * i) + 1) - 1)];
                    strips[(offset + (3 * i)) + 2] = strip[(((_numLEDs) * 3) - (3 * i)) - 1];
                }
            }
        }

        /// <summary>
        /// Blanks all of the strips.
        /// </summary>
        public void BlankAll()
        {
            int numBytes = 3 * _numLEDs * _numStrips;
            byte[] zeros = new byte[numBytes];
            for (int i = 0; i < numBytes; i++)
            {
                zeros[i] = 0;
            }
            _spi.Write(zeros);
        }
    }
}
