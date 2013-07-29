using System;
using System.Threading;

namespace BlinkySimulator
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
        byte _numLEDs = 32;
        byte _numStrips = 5;
        int _mode = 9;
        int _numModes = 10;
        bool _changeMode = false;
        SPI _spi;
        LedStripUtils _utils;

        /// <summary>
        /// Default constructor starts the demo which cannot be cancelled.
        /// </summary>
        public Demo( SPI spi)
        {
            _spi = spi;

            _spi.SetupVirtualDisplay();

            _utils = new LedStripUtils(_numLEDs, _numStrips, _spi);

            Timer t = new Timer(new TimerCallback(loop));
            t.Change(0, 5000);
        }
        bool started = false;
        public void loop(object sender)
        {
            if (!started)
            {
                started = true;
                _changeMode = false;

                switch (_mode)
                {
                    case 0:
                        rainbowCycle(10);
                        break;
                    case 1:
                        StringTest(false);
                        break;
                    case 2:
                        LettersTest();
                        break;
                    case 3:
                        AdvancedMaskTest();
                        break;
                    case 4:
                        MaskTest();
                        break;
                    case 5:
                        RGBTest();
                        break;
                    case 6:
                        RainbowStringTest(false);
                        break;
                    case 7:
                        cylonCycle(100, 20);
                        break;
                    case 8:
                        EqualizerTest();
                        break;
                    case 9:
                        rainbowCylon(1000);
                        break;
                    default:
                        _mode = 0;
                        rainbowCycle(10);
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
        public void ChangeMode(uint data1, uint data2, DateTime time)
        {
            _mode++;
            if (_mode > _numModes - 1) _mode = 0;
            _changeMode = true;
            _utils.BlankAll();
        }

        /// <summary>
        /// Tests rendering a "mask" which is a template that contains
        /// booleans indicating the pixel should be drawn.
        /// </summary>
        public void MaskTest()
        {
            // Allocate the "mask" boolean arrays.
            byte[][] maskOdd = new byte[_numStrips][];
            byte[][] maskEven = new byte[_numStrips][];

            for (int strip = 0; strip < _numStrips; strip++)
            {
                maskOdd[strip] = new byte[_numLEDs];
                maskEven[strip] = new byte[_numLEDs];
                for (int led = 0; led < _numLEDs; led++)
                {
                    maskOdd[strip][led] = (byte)(((led & 1) == 0) ? 0 : 1);
                }
            }

            byte[] strips = new byte[3 * _numLEDs * _numStrips];

            for (int strip = 0; strip < _numStrips; strip++)
            {
                for (int i = 0; i < _numLEDs; i++)
                {
                    _utils.SetColor(i + (_numLEDs * strip), _utils.RGB(255, 0, 0), strips);
                }
            }

            bool odd = true;
            while (!_changeMode)
            {
                byte[] toWrite;
                if (!odd)
                {
                    toWrite = _utils.PixelsFromMask(maskEven, strips);
                    odd = true;
                }
                else
                {
                    toWrite = _utils.PixelsFromMask(maskOdd, strips);
                    odd = false;
                }
                _spi.Write(toWrite);
                Thread.Sleep(500);
            }
            started = false;
        }

        /// <summary>
        /// Test simply writing letters to the display. This makes use of 
        /// both masks and the letters utility functions.
        /// </summary>
        public void StringTest(bool isVertical)
        {
            // An empty mask
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};

            byte[] strips = new byte[_numLEDs * _numStrips * 3];

            for (int i = 0; i < strips.Length; i++ )
            {
                if (i % 3 == 0)
                {
                    strips[i] = 127;
                }
            }

            string toWrite = "abcdefghijklmnopqrstuvwxyz1234567890 ";

            int stringIndex = toWrite.Length - 1;

            while (!_changeMode)
            {
                byte[][] nextChar = _utils.CharAsMask(toWrite[stringIndex]);
                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        if (isVertical)
                        {
                            mask[col][0] = nextChar[row][col];
                        }
                        else
                        {
                            mask[col][0] = nextChar[col][4 - row];
                        }
                    }
                    _spi.Write(_utils.PixelsFromMask(mask, strips));
                    mask = _utils.RotateMask(mask, false);
                    Thread.Sleep(100);
                    Thread.Yield();
                }
                // Draw line between letters
                mask = _utils.RotateMask(mask, false);

                stringIndex--;
                if (stringIndex < 0)
                {
                    stringIndex = toWrite.Length - 1;
                }
            }

            started = false;
        }

        /// <summary>
        /// Creates a byte array representing a rainbow rotated by the passed in value.
        /// </summary>
        /// <param name="offset">The value to rotate. Should be less than #leds.</param>
        /// <returns>A byte with pixels set in the rotated rainbow.</returns>
        public byte[] GetRainbow(int offset)
        {
            byte[] toReturn = new byte[_numLEDs * _numStrips * 3];

            // A nice set of red pixels.
            for (int strip = 0; strip < _numStrips; strip++)
            {
                for (int i = 0; i < _numLEDs; i++)
                {
                    int pos = ((i + offset) >= _numLEDs) ? offset - _numLEDs : offset;
                    _utils.SetColor((i + (_numLEDs * strip) + pos), _utils.Wheel((255 / _numLEDs) * i), toReturn);
                }
            }


            return toReturn;
        }

        /// <summary>
        /// Test simply writing letters to the display. This makes use of 
        /// both masks and the letters utility functions.
        /// </summary>
        public void RainbowStringTest(bool isVertical)
        {

            // An empty mask
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};

            string toWrite = "gus pwnz LED awesome lulz ";
            int stringIndex = toWrite.Length - 1;
            int offset = 0;

            while (!_changeMode)
            {
                byte[][] nextChar = _utils.CharAsMask(toWrite[stringIndex]);
                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        if (isVertical)
                        {
                            mask[col][0] = nextChar[row][col];
                        }
                        else
                        {
                            mask[col][0] = nextChar[col][4-row];
                        }
                    }
                    byte[] strips = GetRainbow(offset);
                    _spi.Write(_utils.PixelsFromMask(mask, strips));
                    mask = _utils.RotateMask(mask, false);
                    Thread.Sleep(100);
                    offset++;
                    if (offset > _numLEDs)
                    {
                        offset = 0;
                    }
                }
                // Draw line between letters
                mask = _utils.RotateMask(mask, false);

                stringIndex--;
                if (stringIndex < 0)
                {
                    stringIndex = toWrite.Length - 1;
                }
            }

            started = false;
        }

        /// <summary>
        /// Test simply writing letters to the display. This makes use of 
        /// both masks and the letters utility functions.
        /// </summary>
        public void LettersTest()
        {
            // An empty mask
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};

            byte[] strips = new byte[_numLEDs * _numStrips * 3];

            // A nice set of red pixels.
            for (int strip = 0; strip < _numStrips; strip++)
            {
                for (int i = 0; i < _numLEDs; i++)
                {
                    _utils.SetColor(i + (_numLEDs * strip), _utils.RGB(255, 0, 0), strips);
                }
            }

            _utils.WriteLetter('z', 0, mask);
            _utils.WriteLetter('z', 1, mask);
            _utils.WriteLetter('y', 2, mask);
            _utils.WriteLetter('z', 3, mask);
            _utils.WriteLetter('x', 4, mask);

            while (!_changeMode)
            {
                _spi.Write(_utils.PixelsFromMask(mask, strips));
                mask = _utils.RotateMask(mask, true);
                Thread.Sleep(300);
                Thread.Yield();
            }

            started = false;
        }

        /// <summary>
        /// Tests rendering a "mask" which is a template that contains
        /// booleans indicating the pixel should be drawn.
        /// </summary>
        public void AdvancedMaskTest()
        {
            byte[] strips = new byte[_numLEDs * _numStrips * 3];


            // Some X's
            byte[][] mask = new byte[][]{ 
                // A simple guide, assuming 32 LEDs in a strip
                // ------------------------------------------------------------------------------------------------------------
                // --------  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 ---
                // ------------------------------------------------------------------------------------------------------------
                new byte[] { 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] { 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0},
                new byte[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0},
                new byte[] { 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0},
                new byte[] { 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0}};

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
            for (int strip = 0; strip < _numStrips; strip++)
            {
                for (int i = 0; i < _numLEDs; i++)
                {
                    _utils.SetColor(i + (_numLEDs * strip), _utils.RGB(255, 0, 0), strips);
                }
            }

            while (!_changeMode)
            {
                _spi.Write(_utils.PixelsFromMask(mask, strips));
                mask = _utils.RotateMask(mask, true);
                Thread.Sleep(100);
            }

            started = false;
        }


        /// <summary>
        /// A test that covers the ability to use LED strips. Draws
        /// a rainbow up and down each LED strip.
        /// </summary>
        public void EqualizerTest()
        {
        
            byte[] strip = new byte[3 * _numLEDs];
            byte[] strips = new byte[3 * _numLEDs * _numStrips];
            int pos = 0;
            bool up = true;
            while (!_changeMode)
            {
                if (pos == _numLEDs)
                {
                    up = false;
                    pos = _numLEDs - 1;
                }
                if (pos < 0)
                {
                    up = true;
                    pos = 0;
                }

                byte i = 0;
                while (i < pos)
                {

                    _utils.SetColor(i, _utils.Wheel(255 - ((255 / _numLEDs) * i)), strip);
                    i++;
                }
                while (i < _numLEDs)
                {
                    _utils.SetColor(i, _utils.RGB(0, 0, 0), strip);
                    i++;
                }
                for (byte stripNumber = 0; stripNumber < _numStrips; stripNumber++)
                {
                    ///bool reverse = ((stripNumber & 1) == 1);
                    bool reverse = false;
                    _utils.WriteStrip(stripNumber, strip, strips, reverse);
                }
                _spi.Write(strips);
                if (up)
                {
                    pos++;
                }
                else
                {
                    pos--;
                }
                Thread.Sleep(10);
            }

            started = false;
            _changeMode = false;
        }

        /// <summary>
        /// Fade a cylon pattern along the chain. [Yeah BSG :)]
        /// </summary>
        /// <param name="wait">Time to wait before updating position (milliseconds).</param>
        /// <param name="width">Width of the Cylon bar (pixels).</param>
        public void cylonCycle(int wait, int width)
        {
            var colors = new byte[3 * _numLEDs * _numStrips];

            int pos = 0;

            int offsetLimit = 255;
            int offset = offsetLimit;
            int steps = _numLEDs;
            while (!_changeMode)
            {
                while (pos <= _numLEDs)
                {

                    if (offset > offsetLimit)
                    {
                        offset = 0;
                    }

                    // Write colors
                    for (byte i = 0; i < _numLEDs; ++i)
                    {
                        if (i >= pos & (i < pos + width))
                        {
                            byte[] color = _utils.Wheel(i + offset);
                            _utils.SetColor(i, color, colors);
                        }
                        else
                        {
                            byte[] color = _utils.RGB(0, 0, 0);
                            _utils.SetColor(i, color, colors);
                        }
                    }
                    Thread.Sleep(wait);
                    _spi.Write(colors);
                    Thread.Yield(); // march at 32 pixels per second
                    offset += offsetLimit / steps;
                    pos++;
                }
                while (pos >= 0)
                {
                    // Write colors
                    for (byte i = 0; i < _numLEDs; ++i)
                    {
                        if (i >= pos & (i < pos + width))
                        {
                            byte[] color = _utils.Wheel(i + offset);
                            _utils.SetColor(i, color, colors);
                        }
                        else
                        {
                            byte[] color = _utils.RGB(0, 0, 0);
                            _utils.SetColor(i, color, colors);
                        }
                    }
                    offset -= offsetLimit / steps;
                    _spi.Write(colors);
                    Thread.Yield(); // march at 32 pixels per second
                    Thread.Sleep(wait);
                    pos--;
                }
            }

            started = false;
            _changeMode = false;
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

            while (!_changeMode)
            {
                // Write colors
                for (byte i = 0; i < _numLEDs; ++i)
                {
                    byte[] thisColor = _utils.Wheel(i + offset);
                    colors[(i * 3)] = thisColor[0];
                    colors[(i * 3) + 1] = thisColor[1];
                    colors[(i * 3) + 2] = thisColor[2];
                }
                _spi.Write(colors);
                offset += offsetLimit / steps;
                Thread.Sleep(wait / steps); // march at 32 pixels per second
            }

            started = false;
            _changeMode = false;
        }

        /// <summary>
        /// Fade a rainbow along the chain
        /// </summary>
        /// <param name="wait">Time to wait between updates to the rainbow.</param
        public void rainbowCycle(int wait)
        {
            int offsetLimit = 255;
            int offset = offsetLimit;
            int steps = 255;
            int numPixels = (_numLEDs * _numStrips);
            int numBytes = 3 * numPixels;
            var colors = new byte[numBytes];
            while (!_changeMode)
            {
                if (offset > offsetLimit)
                {
                    offset = 0;
                }

                // Write colors
                for (byte i = 0; i < numPixels; ++i)
                {
                    byte[] color = _utils.Wheel(i + offset);
                    colors[(i * 3)] = color[0];
                    colors[(i * 3) + 1] = color[1];
                    colors[(i * 3) + 2] = color[2];
                }
                _spi.Write(colors);
                offset += offsetLimit / steps;
                Thread.Sleep(wait);
                Thread.Yield();
            }

            started = false;
            _changeMode = false;
        }

        /// <summary>
        /// Fade a rainbow along the chain
        /// </summary>
        /// <param name="wait">Time to wait between rainbow cycles.</param>
        public void rainbowCylon(int wait)
        {
            var colors = new byte[3 * _numLEDs * _numStrips];
            var zeros = new byte[3 * _numLEDs * _numStrips];

            int offsetLimit = 255;
            int offset = offsetLimit;
            int steps = 255;
            while (!_changeMode)
            {
                if (offset > offsetLimit)
                {
                    offset = 0;
                }

                // Write colors
                for (byte i = 0; i < 32; ++i)
                {
                    byte[] color = _utils.Wheel(i + offset);
                    colors[(i * 3)] = color[0];
                    colors[(i * 3) + 1] = color[1];
                    colors[(i * 3) + 2] = color[2];
                }
                _spi.Write(colors);
                offset += offsetLimit / steps;
                Thread.Sleep(wait / steps); // march at 32 pixels per second
                Thread.Yield();
            }

            started = false;
            _changeMode = false;
        }

        /// <summary>
        /// Just testing splitting the strip to R / G / B colors.
        /// </summary>
        public void RGBTest()
        {
            var colors = new byte[3 * _numLEDs * _numStrips];

            byte offset = 255;
            while (!_changeMode)
            {
                if (offset > 255)
                {
                    offset = 0;
                }
                // all pixels off
                for (int i = 0; i < colors.Length; ++i) colors[i] = (byte)(0);
                // a progressive yellow/red blend
                for (byte i = 0; i < _numLEDs; ++i)
                {
                    byte[] color = new byte[3];
                    if (i < (_numLEDs / 3))
                    {
                        color = _utils.RGB(offset, 0, 0);
                    }
                    else if (i < ((_numLEDs * 2) / 3))
                    {
                        color = _utils.RGB(0, offset, 0);
                    }
                    else
                    {
                        color = _utils.RGB(0, 0, offset);
                    }
                    for (byte j = 0; j < 3; j++)
                    {
                        colors[(3 * i) + j] = color[j];
                    }
                }
                _spi.Write(colors);

                Thread.Sleep(1000 / _numLEDs); // march at 32 pixels per second
            }

            started = false;
            _changeMode = false;
        }

    }
}
