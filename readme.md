Project done at CEERI, Pilani.
----------------------------------
Works with 3+ Microsoft Kinect sensors simultaneously, to detect the speaking person.

Include MS Kinect.Toolit and Kinect.Toolkit.Facetracking in the VS Project Solution.

## Things Done

* Initialise multiple Kinects and run them concurrently. 
* Checking combined audiostream, and monitoring audio levels.
* Switching to the camera with highest associated input sound intensity (w/o background noise).
* Display the correct person on screen.

## Possible enhancements

* Create a wrapper so that a new Kinect can be initialised with one command.
* [Optional]: Automatic addition of sensor if a new Kinect is connected to the computer.
* <del> [Optional]: Improve live camera switching to make it seamless, and instantaneous. (done!) </del>
* [Optional]: Lip tracking on face for better match.
* Extract faces of people facing the different Kinect sensors and display their profile view.

[![Bitdeli Badge](https://d2weczhvl823v0.cloudfront.net/uvcyclotron/kinect-speaker-detection/trend.png)](https://bitdeli.com/free "Bitdeli Badge")