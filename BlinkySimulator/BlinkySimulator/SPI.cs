// -----------------------------------------------------------------------
// <copyright file="SPI.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace BlinkySimulator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SPI
    {
        Canvas _canvas;
        MainWindow _window;

        private int _numLEDs, _numStrips;
        private Rectangle[][] _squares;
        private SolidColorBrush[][] _brushes;
        private int wheelColor = 0;

        private double _width, _height, _margin, _squareSize, _squareMargin, _leftBoardBound,
            _topBoardBound;

        bool _changeMode = false;

        public SPI(MainWindow window, Canvas canvas, int x, int y)
        {
            _window = window;
            _canvas = canvas;
            _numLEDs = x;
            _numStrips = y;

            // setup canvas objects
            _squares = new Rectangle[_numLEDs][];
            _brushes = new SolidColorBrush[_numLEDs][];

            for (int row = 0; row < _numLEDs; row++)
            {
                _squares[row] = new Rectangle[_numStrips];
                _brushes[row] = new SolidColorBrush[_numStrips];
            }

        }
        public void Write(byte[] pixels)
        {
            _window.Dispatcher.Invoke((Action)(() =>
            {
                byte[] colorBytes = new byte[3];
                for (int x = 0; x < _numLEDs; x++)
                {
                    for (int y = 0; y < _numStrips; y++)
                    {
                        colorBytes[0] = pixels[3 * (x + (y * _numLEDs))];
                        colorBytes[1] = pixels[3 * (x + (y * _numLEDs)) + 1];
                        colorBytes[2] = pixels[3 * (x + (y * _numLEDs)) + 2];
                        SetPixel(x, y, colorBytes);
                    }
                }
            }));
        }

        public void SetPixel(int x, int y, byte[] color)
        {
            _squares[x][y].Fill = new SolidColorBrush(Color.FromRgb(color[0], color[1], color[2]));
        }


        public void SetupVirtualDisplay()
        {
            _height = _canvas.Height;
            _width = _canvas.Width;

            // evenly divide screen by rows / cols
            _margin = _height < _width ? _height / 10 : _width / 10;

            _height -= _margin;
            _width -= _margin;

            double rowSize = _width / _numLEDs;
            double colSize = _height / _numStrips;

            _squareSize = rowSize < colSize ? rowSize : colSize;

            _squareMargin = _squareSize / 50;

            //_squareSize -= _squareMargin;

            double bigSidePad = (_height > _width) ? ((_height - _width) / 2) : ((_width - _height) / 2);

            // which is the smaller one? used to make grid square vs screen-bound geometry            
            _leftBoardBound = _margin / 2 + (_height < _width ? bigSidePad : 0);
            _topBoardBound = _margin / 2 + (_width < _height ? bigSidePad : 0);

            for (int row = 0; row < _numLEDs; row++)
            {
                for (int col = 0; col < _numStrips; col++)
                {
                    SetupSquare(row, col);
                    //updateColor(_brushes[row][col], Colors.Purple);
                }
            }
        }

        void SetupSquare(int row, int col)
        {
            // calculate and set left, top, and size 
            double left = (_squareMargin + _squareSize) + ((_squareSize + _squareMargin) * row);
            double top = 10 + _squareMargin + (_squareSize * col) + (_squareMargin * col);

            // Create the rect
            Rectangle toAdd = new Rectangle();
            _brushes[row][col] = new SolidColorBrush(Colors.Red);
            toAdd.Fill = _brushes[row][col];
            toAdd.Width = _squareSize;
            toAdd.Height = _squareSize;

            _squares[row][col] = toAdd;

            _canvas.Children.Add(toAdd);
            Canvas.SetLeft(toAdd, left);
            Canvas.SetTop(toAdd, top);
        }
    }
}
