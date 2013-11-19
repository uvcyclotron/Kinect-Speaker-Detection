using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
namespace KSpeakerDetec
{
    public sealed class KinectDevice
    {
        public string DeviceName { get; set; }

        public Runtime KinectRuntime { get; set; }

    }
}
