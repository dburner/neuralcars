using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticCars
{
    static class Functions
    {
        public static Random rand = new Random();

        public static double DegreeToRadian(float angle)
        {
            double rad = angle * (Math.PI / 180);
            return rad;
        }
    }
}
