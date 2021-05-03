using System;
using System.Collections.Generic;
using System.Text;

namespace NearSatMobile
{
    public class Satellite
    {

        public string name = "";
        public double distance;

        public double x;
        public double y;
        public double z;

        public Satellite(string varName, double distNew)
        {
            name = varName;
            distance = distNew;
        }

    }
}
