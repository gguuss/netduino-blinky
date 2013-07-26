netduino-blinky
===============

netduino-blinky

This project is a port of functionality provided in the Adafruit library into 
Netduino code. 

I'm assuming that you are connecting LED strips such as the Adafruit one:
http://learn.adafruit.com/digital-led-strip

To run the program:

1. Connect the CLK pin to 13
2. Connect the MOSI pin to 11
3. Deploy the program to your Netduino

When running the demo, you can press the button on the device to 
switch the mode between the various demos included.

A few demos of the patterns:

* https://plus.google.com/+GusClass/posts/TzfUe9kzsTx
* https://plus.google.com/+GusClass/posts/FiphDznHLBe

Most of the LED strips I have seen use 5V, while the typical input for a 
Netduino will be 12V. Make sure to supply the correct voltage or you will
most likely break your LED strip.

License
=======

[MIT license](http://opensource.org/licenses/MIT)
