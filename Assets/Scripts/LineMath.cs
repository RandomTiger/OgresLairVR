using System;
using UnityEngine;

namespace OgreMaths
{

    public class Point
    {
        public float x, y;

        public Point(Point p) { x = p.x; y = p.y; }
        public Point(Point2 point2) { x = point2.X; y = point2.Y; }
        public Point(Vector3 vec3) { x = vec3.x; y = vec3.y; }
        public Point(Vector2 vec2) { x = vec2.x; y = vec2.y; }
	    public Point() {x=0; y =0;}
	    public Point(float x, float y) {this.x = x; this.y = y;}

        static public Point operator -(Point a, Point b) 
	    {
		    return new Point(a.x - b.x, a.y - b.y);
	    }

        static public Point operator +(Point a, Point b) 
	    {
		    return new Point(a.x + b.x, a.y + b.y);
	    }

	    public Point Mult(float multipler) 
	    {
		    return new Point(this.x * multipler, this.y * multipler);
	    }

	    public Point Div(float divider) 
	    {
		    return new Point(this.x / divider, this.y / divider);
	    }

	    public float Length()
	    {
		    return (float) Math.Sqrt(LengthSquared());
	    }

        public float LengthSquared()
	    {
		    return x*x + y*y;
	    }

	    Point GetNormalisedVector()
	    {
		    Point vector = this;
		    float length = vector.Length();
		    vector.x =  vector.x / length;
		    vector.y =  vector.y / length;
		    return vector;
	    }

        public void Normalise()
        {
            float length = Length();
            x = x / length;
            y = y / length;
        }

        static public float DistanceSquared(Point a, Point b)
        {
            return (float)(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
        }

        static public float Distance(Point a, Point b)
        {
            return (float)Math.Sqrt(DistanceSquared(a, b));
        }
    };

    class Line
    {

	    public Point a;
	    public Point b;

        public Line() { a = new Point(); b = new Point(); }
	    public Line(Point a, Point b) {this.a = a; this.b = b;}

	    public Point GetNormal() 
	    {
		    Point newPoint = b - a;
		    float t = newPoint.y;
		    newPoint.y = -newPoint.x;
		    newPoint.x = t;

		    return newPoint;
	    }

	    public Point GetVector() 
	    {
		    return b - a;
	    }

	    public Point GetNormalisedVector()
	    {
		    Point vector = GetVector();
		    float length = vector.Length();
            if(length <= 0)
            {
                return vector;
            }

		    vector.x =  vector.x / length;
		    vector.y =  vector.y / length;
		    return vector;
	    }

	    public float GetGradient() 
	    {
		    return (b.y - a.y) / (b.x - a.x); 
	    }

	    public float GetLength()
	    {
		    Point vector = GetVector();
		    return vector.Length();
	    }



        
        // calculates intersection and checks for parallel lines.  
        // also checks that the intersection point is actually on  
        // the line segment
        public static bool CalcIntersection(Line l1, Line l2, Point outputIntersection)
        {
	        Point p1 = l1.a;
	        Point p2 = l1.b;
            Point p3 = l2.a;
	        Point p4 = l2.b;

            float xD1,yD1,xD2,yD2,xD3,yD3;  
            float dot,deg,len1,len2;  
            float segmentLen1,segmentLen2;  
            float ua,ub,div;  
            
            // calculate differences  
            xD1=p2.x-p1.x;  
            xD2=p4.x-p3.x;  
            yD1=p2.y-p1.y;  
            yD2=p4.y-p3.y;  
            xD3=p1.x-p3.x;  
            yD3=p1.y-p3.y;    
            
            // calculate the lengths of the two lines  
            len1 = (float)Math.Sqrt(xD1 * xD1 + yD1 * yD1);
            len2 = (float)Math.Sqrt(xD2 * xD2 + yD2 * yD2);  
            
            // calculate angle between the two lines.  
            dot=(xD1*xD2+yD1*yD2); // dot product  
            deg=dot/(len1*len2);  
            
            // if abs(angle)==1 then the lines are parallell,  
            // so no intersection is possible  
            if(Math.Abs(deg)==1) return false;  
            
            // find intersection Pt between two lines  
            Point pt = outputIntersection;  

            div=yD2*xD1-xD2*yD1;  
            ua=(xD2*yD3-yD2*xD3)/div;  
            ub=(xD1*yD3-yD1*xD3)/div;  
            pt.x=p1.x+ua*xD1;  
            pt.y=p1.y+ua*yD1;  
            
            // calculate the combined length of the two segments  
            // between Pt-p1 and Pt-p2  
            xD1=pt.x-p1.x;  
            xD2=pt.x-p2.x;  
            yD1=pt.y-p1.y;  
            yD2=pt.y-p2.y;
            segmentLen1 = (float)(Math.Sqrt(xD1 * xD1 + yD1 * yD1) + Math.Sqrt(xD2 * xD2 + yD2 * yD2));  
            
            // calculate the combined length of the two segments  
            // between Pt-p3 and Pt-p4  
            xD1=pt.x-p3.x;  
            xD2=pt.x-p4.x;  
            yD1=pt.y-p3.y;  
            yD2=pt.y-p4.y;  
            segmentLen2=(float)(Math.Sqrt(xD1*xD1+yD1*yD1)+Math.Sqrt(xD2*xD2+yD2*yD2));  
            
            // if the lengths of both sets of segments are the same as  
            // the lenghts of the two lines the point is actually  
            // on the line segment.  
            
            // if the point isn't on the line, return null  
            if(Math.Abs(len1-segmentLen1)>0.01 || Math.Abs(len2-segmentLen2)>0.01)  
              return false;  
            
            // return the valid intersection  
            return true;  
        }  


    };

    class Box
    {
        public enum Corner
        {
            TOP_LEFT,
            TOP_RIGHT,
            BOTTOM_RIGHT,
            BOTTOM_LEFT,
        };

	    public Point [] corners = new Point[4];
        private Point centre;

	    public Box(){}
	    public Box(Point centre, float width)
	    {
		    corners[0] = new Point(centre.x - width / 2, centre.y - width / 2);
		    corners[1] = new Point(centre.x + width / 2, centre.y - width / 2);
		    corners[2] = new Point(centre.x + width / 2, centre.y + width / 2);
		    corners[3] = new Point(centre.x - width / 2, centre.y + width / 2);

		    this.centre = centre;
	    }

	    public void MoveCentre(float x, float y)
	    {
		    centre.x += x;
		    centre.y += y;

		    for(int i = 0; i < 4; i++)
		    {
			    corners[i].x += x;
			    corners[i].y += y;
		    }
	    }

	    public Line GetLine(int index)
	    {
		    return new Line(corners[index],corners[(index + 1) % 4]);
	    }

	    public bool GetClosestIntersectionWithLine(Line vector, ref Point finalIntersectionPoint, ref Line outPutLineFromBox)
	    {
		    float closestDistance = -1.0f;
		    int closestIndex    = 0;
    		

		    for(int i = 0; i < 4; i++)
		    {
			    Line boxLine = GetLine(i);
    			
			    Point intersectionPoint = new Point();
			    bool intersection = Line.CalcIntersection(vector, boxLine, intersectionPoint);

			    if(intersection == false)
			    {
				    continue;
			    }
    			
			    float distance = Point.Distance(vector.a, intersectionPoint);

			    if(closestDistance < 0 || closestDistance > distance)
			    {
				    finalIntersectionPoint = intersectionPoint;
				    closestDistance = distance;
				    closestIndex = i;
				    outPutLineFromBox = boxLine;
			    }
		    }

		    return closestDistance >= 0.0f;
	    }

        public static bool CalcReflectVectorWithBox(Point boxCentre, float boxSize, Line vector, ref Point reflectionPoint, ref Point intersect)
        {
            return CalcReflectVectorWithBox(boxCentre, boxSize, vector.a, vector.b, ref reflectionPoint, ref intersect);
        }

        public static bool CalcReflectVectorWithBox(Point boxCentre, float boxSize, Point vectorStart, Point vectorEnd, ref Point reflectionPoint, ref Point intersect)
        {
            Box l_tile = new Box(boxCentre, boxSize);
            Line l_vectorIn = new Line(vectorStart, vectorEnd);

		    Line lineChosen = new Line();
            bool intersectionHappened = l_tile.GetClosestIntersectionWithLine(l_vectorIn, ref intersect, ref lineChosen);

		    if(intersectionHappened)
		    {
			    Line vectorInCapped = l_vectorIn;
                vectorInCapped.b = intersect;

                Line boxNormal = new Line(intersect, intersect + lineChosen.GetNormal());

                Point a = vectorInCapped.GetVector();// NormalisedVector();
			    Point b = boxNormal.GetNormalisedVector();

			    float dotProduct = (a.x * b.x + a.y * b.y);// / (a.Length() * b.Length());
			    Point result = a - (b.Mult(2.0f * dotProduct));

                reflectionPoint = intersect + result;//.Mult(vectorInCapped.GetLength());
			  //  Line vectorOut = Line(intersectionOnTile,intersectionOnTile + result * vectorInCapped.GetLength());
		    }

		    return intersectionHappened;
        }

    };

}