using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Project2 // http://csharphelper.com/blog/2014/07/triangulate-a-polygon-in-c/
{
    public class Triangle : Polygon2
    {
        public Triangle(PointF p0, PointF p1, PointF p2)
        {
            Points = new PointF[] { p0, p1, p2 };
        }
    }
}
