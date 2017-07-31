using UnityEngine;
using System.Collections;

namespace GRTK
{
    /// <summary>
    /// A class to represent a line segment and handle various interactions with line segments
    /// </summary>
    public class Line2D
    {
        public static float LEFT_EPS = 0.0001f;

        // The two points defining the line segment
        public Vector2 p1;
        public Vector2 p2;

        // Main constructor
        public Line2D(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        // Tests if a line segment intersects with another line segment
        // Warning: double precision errors can affect intersection near the boundaries of the segments
        public bool Intersect(Line2D other)
        {
            // Convert the lines into a mathematical representation
            // A = y2 - y1
            // B = x2 - x1
            // C = A*x1 + B*y1
            double A1 = p2.y - p1.y;
            double B1 = p1.x - p2.x;
            double C1 = (A1 * p1.x) + (B1 * p1.y);

            double A2 = other.p2.y - other.p1.y;
            double B2 = other.p1.x - other.p2.x;
            double C2 = (A2 * other.p1.x) + (B2 * other.p1.y);

            // Test intersection on infinite line
            double det = (A1 * B2) - (A2 * B1);
            if (det == 0)
                return false; // If lines are parallel return false
            double interX = ((B2 * C1) - (B1 * C2)) / det;
            double interY = ((A1 * C2) - (A2 * C1)) / det;

            // Verify the intersection is on both segments of the infinite line
            // To do this we compare distances. If dist(p1, intersect) + dist(intersect, p2) = dist(p1, p2) then it's on the segment
            double epsilon = 0.00001;
            bool onLine1 = false;
            bool onLine2 = false;

            // Line 1
            double AB = Mathf.Sqrt(((p2.x - p1.x) * (p2.x - p1.x)) + ((p2.y - p1.y) * (p2.y - p1.y)));
            double AP = Mathf.Sqrt((((float)interX - p1.x) * ((float)interX - p1.x)) + (((float)interY - p1.y) * ((float)interY - p1.y)));
            double PB = Mathf.Sqrt(((p2.x - (float)interX) * (p2.x - (float)interX)) + ((p2.y - (float)interY) * (p2.y - (float)interY)));

            if (Mathf.Abs((float)(AB - (AP + PB))) < epsilon)
                onLine1 = true;

            // Line 2
            double AB2 = Mathf.Sqrt(((other.p2.x - other.p1.x) * (other.p2.x - other.p1.x)) + ((other.p2.y - other.p1.y) * (other.p2.y - other.p1.y)));
            double AP2 = Mathf.Sqrt((((float)interX - other.p1.x) * ((float)interX - other.p1.x)) + (((float)interY - other.p1.y) * ((float)interY - other.p1.y)));
            double PB2 = Mathf.Sqrt(((other.p2.x - (float)interX) * (other.p2.x - (float)interX)) + ((other.p2.y - (float)interY) * (other.p2.y - (float)interY)));

            if (Mathf.Abs((float)(AB2 - (AP2 + PB2))) < epsilon)
                onLine2 = true;

            return onLine1 && onLine2;
        }

        // Override returning intersection point
        public bool Intersect(Line2D other, out Vector2 intersectionPoint)
        {
            intersectionPoint = new Vector2();
            // Convert the lines into a mathematical representation
            // A = y2 - y1
            // B = x2 - x1
            // C = A*x1 + B*y1
            double A1 = p2.y - p1.y;
            double B1 = p1.x - p2.x;
            double C1 = (A1 * p1.x) + (B1 * p1.y);

            double A2 = other.p2.y - other.p1.y;
            double B2 = other.p1.x - other.p2.x;
            double C2 = (A2 * other.p1.x) + (B2 * other.p1.y);

            // Test intersection on infinite line
            double det = (A1 * B2) - (A2 * B1);
            if (det == 0)
                return false; // If lines are parallel return false
            double interX = ((B2 * C1) - (B1 * C2)) / det;
            double interY = ((A1 * C2) - (A2 * C1)) / det;
            intersectionPoint.x = (float)interX;
            intersectionPoint.y = (float)interY;

            // Verify the intersection is on both segments of the infinite line
            // To do this we compare distances. If dist(p1, intersect) + dist(intersect, p2) = dist(p1, p2) then it's on the segment
            double epsilon = 0.00001;
            bool onLine1 = false;
            bool onLine2 = false;

            // Line 1
            double AB = Mathf.Sqrt(((p2.x - p1.x) * (p2.x - p1.x)) + ((p2.y - p1.y) * (p2.y - p1.y)));
            double AP = Mathf.Sqrt((((float)interX - p1.x) * ((float)interX - p1.x)) + (((float)interY - p1.y) * ((float)interY - p1.y)));
            double PB = Mathf.Sqrt(((p2.x - (float)interX) * (p2.x - (float)interX)) + ((p2.y - (float)interY) * (p2.y - (float)interY)));

            if (Mathf.Abs((float)(AB - (AP + PB))) < epsilon)
                onLine1 = true;

            // Line 2
            double AB2 = Mathf.Sqrt(((other.p2.x - other.p1.x) * (other.p2.x - other.p1.x)) + ((other.p2.y - other.p1.y) * (other.p2.y - other.p1.y)));
            double AP2 = Mathf.Sqrt((((float)interX - other.p1.x) * ((float)interX - other.p1.x)) + (((float)interY - other.p1.y) * ((float)interY - other.p1.y)));
            double PB2 = Mathf.Sqrt(((other.p2.x - (float)interX) * (other.p2.x - (float)interX)) + ((other.p2.y - (float)interY) * (other.p2.y - (float)interY)));

            if (Mathf.Abs((float)(AB2 - (AP2 + PB2))) < epsilon)
                onLine2 = true;

            return onLine1 && onLine2;
        }

        // Check if a point is to the left of the line (assuming orientation of line A to B means the line is going towards B)
        public bool Left(Vector2 point)
        {
            // Find the determinate of the point and the line
            // A = -(y2 - y1)
            // B = x2 - x1
            // C = -(A * x1 + B * y1)
            // D = A * xp + B * yp + C
            double A = -(p2.y - p1.y);
            double B = p2.x - p1.x;
            double C = -(A * p1.x + B * p1.y);
            double D = A * point.x + B * point.y + C;

            // if D > 0 then it's on the left
            return D > LEFT_EPS;
        }
    }
}