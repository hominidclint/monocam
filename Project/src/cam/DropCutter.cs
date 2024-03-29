//this file is the correct one
using System;
using System.Collections.Generic;
using System.Text;

// This is the Drop Cutter algorithm
// For a given (x,y) point and a given triangle
// It drops the cutter down along the z-axis until it touches the triangle
// 
// There are three tests:
// 1. cutter vs. vertex
// 2. cutter vs. facet
// 3. cutter vs. edge


namespace monoCAM
{
    class Cutter
    {
        public double R; // shaft radius
        public double r; // corner radius
        public Cutter(double Rset, double rset)
        {
            if (Rset > 0)
            {
                R = Rset;
            }
            else
            {
                // ERROR!
                System.Console.WriteLine("Cutter: ERROR R<0!");
                R = 1;
            }

            if ((rset >= 0) && (rset <= R))
            {
                r = rset;
            }
            else
            {
                // ERROR!
                // Throw an exception or something
                System.Console.WriteLine("Cutter: ERROR r<0 or r>R!");
                r = 0;
            }
        }

        // TODO:
        // - bounding box is probably useful for orhtogonal range search

    } // end Cutter class

    class DropCutter
    {
        
       
        public static double? VertexTest(Cutter c,Geo.Point e, Geo.Point p)
       {
           // c.R and c.r define the cutter
           // e.x and e.y is the xy-position of the cutter (e.z is ignored)
           // p is the vertex tested against

           // q is the distance along xy-plane from e to vertex
           double q = Math.Sqrt(Math.Pow(e.x - p.x, 2) + Math.Pow((e.y - p.y), 2));

           if (q > c.R)
           { 
               // vertex is outside cutter. no need to do anything!
               return null;
           }
           else if (q <= (c.R - c.r))
           { 
                // vertex is in the cylindical/flat part of the cutter
               return p.z;
           }
           else if ((q > (c.R - c.r)) && (q <= c.R))
           {
               // vertex is in the toroidal part of the cutter
               double h2 = Math.Sqrt(Math.Pow(c.r, 2) - Math.Pow((q - (c.R - c.r)), 2));
               double h1 = c.r - h2;
               return p.z - h1;
           }
           else
           {
               // SERIOUS ERROR, we should not be here!
               System.Console.WriteLine("DropCutter: VertexTest: ERROR!");
               return null;
           }

       } // end VertexTest

        public static double? FacetTest(Cutter cu, Geo.Point e, Geo.Tri t)
        { 
            // local copy of the surface normal

            t.recalc_normals(); // don't trust the pre-calculated normal! calculate it separately here.

            Vector n = new Vector(t.n.x, t.n.y, t.n.z);
            Geo.Point cc;

            if (n.z == 0)
            {
                // vertical plane, can't touch cutter against that!
                return null;
            }
            else if (n.z < 0)
            {
                // flip the normal so it points up (? is this always required?)
                n = -1*n;
            }

            // define plane containing facet
            double a = n.x;
            double b = n.y;
            double c = n.z;
            double d = - n.x * t.p[0].x - n.y * t.p[0].y - n.z * t.p[0].z;

            // the z-direction normal is a special case (?required?)
            // in debug phase, see if this is a useful case!
            if ((a == 0) && (b == 0))
            {
                // System.Console.WriteLine("facet-test:z-dir normal case!");
                e.z = t.p[0].z;
                cc = new Geo.Point(e.x,e.y,e.z);
                if (isinside(t, cc))
                {
                    // System.Console.WriteLine("facet-test:z-dir normal case!, returning {0}",e.z);
                    // System.Console.ReadKey();
                    return e.z;
                }
                else
                    return null;
            }

            // System.Console.WriteLine("facet-test:general case!");
            // facet test general case
            // uses trigonometry, so might be too slow?

            // flat endmill and ballnose should be simple to do without trig
            // toroidal case might require offset-ellipse idea?

            /*
            theta = asin(c);
            zf= -d/c - (a*xe+b*ye)/c+ (R-r)/tan(theta) + r/sin(theta) -r;
            e=[xe ye zf];
            u=[0  0  1];
            rc=e + ((R-r)*tan(theta)+r)*u - ((R-r)/cos(theta) + r)*n;
            t=isinside(p1,p2,p3,rc);
            */

            double theta = Math.Asin(c);
            double zf = -d/c - (a*e.x+b*e.y)/c + (cu.R-cu.r)/Math.Tan(theta) + cu.r/Math.Sin(theta) - cu.r;
            Vector ve = new Vector(e.x,e.y,zf);
            Vector u = new Vector(0,0,1);
            Vector rc = new Vector();
            rc = ve +((cu.R-cu.r)*Math.Tan(theta)+cu.r)*u - ((cu.R-cu.r)/Math.Cos(theta)+cu.r)*n;

            /*
            if (rc.z > 1000)
                System.Console.WriteLine("z>1000 !");
             */

            cc = new Geo.Point(rc.x, rc.y, rc.z);

            // check that CC lies in plane:
            // a*rc(1)+b*rc(2)+c*rc(3)+d
            double test = a * cc.x + b * cc.y + c * cc.z + d;
            if (test > 0.000001)
                System.Console.WriteLine("FacetTest ERROR! CC point not in plane");

            if (isinside(t, cc))
            {
                if (Math.Abs(zf) > 100)
                {
                    System.Console.WriteLine("serious problem... at" +e.x + "," + e.y);
                }
                return zf;
            }
            else
                return null;

        } // end FacetTest

        public static double? EdgeTest(Cutter cu, Geo.Point e, Geo.Point p1, Geo.Point p2)
        { 
            // contact cutter against edge from p1 to p2

            // translate segment so that cutter is at (0,0)
            Geo.Point start = new Geo.Point(p1.x - e.x, p1.y - e.y, p1.z);
            Geo.Point end = new Geo.Point(p2.x - e.x, p2.y - e.y, p2.z);

            // find angle btw. segment and X-axis
            double dx = end.x - start.x;
            double dy = end.y - start.y;
            double alfa;
            if (dx != 0)
                alfa = Math.Atan(dy / dx);
            else
                alfa = Math.PI / 2;

            //alfa = -alfa;
            // rotation matrix for rotation around z-axis:
            // should probably implement a matrix class later

            // rotate by angle alfa
            // need copy of data that does not change as we go through each line:
            double sx = start.x, sy = start.y, ex = end.x, ey = end.y;
            start.x = sx * Math.Cos(alfa) + sy * Math.Sin(alfa);
            start.y = -sx * Math.Sin(alfa) + sy * Math.Cos(alfa);
            end.x = ex * Math.Cos(alfa) + ey * Math.Sin(alfa);
            end.y = -ex * Math.Sin(alfa) + ey * Math.Cos(alfa);


            // check if segment is below cutter
            
            if (start.y > 0)
            {   
                alfa = alfa+Math.PI;
                start.x = sx * Math.Cos(alfa) + sy * Math.Sin(alfa);
                start.y = -sx * Math.Sin(alfa) + sy * Math.Cos(alfa);
                end.x = ex * Math.Cos(alfa) + ey * Math.Sin(alfa);
                end.y = -ex * Math.Sin(alfa) + ey * Math.Cos(alfa);
            }

            if (Math.Abs(start.y-end.y)>0.0000001)
            {
                System.Console.WriteLine("EdgeTest ERROR! (start.y - end.y) = " +(start.y-end.y));
                return null;
            }

            double l = -start.y; // distance from cutter to edge
            if (l < 0)
                System.Console.WriteLine("EdgeTest ERROR! l<0 !");

 
                
            
            // System.Console.WriteLine("l=" + l+" start.y="+start.y+" end.y="+end.y);
            

            // now we have two different algorithms depending on the cutter:
            if (cu.r == 0)
            {
                // this is the flat endmill case
                // it is easier and faster than the general case, so we handle it separately
                if (l > cu.R) // edge is outside of the cutter
                    return null;
                else // we are inside the cutter
                {   // so calculate CC point
                    double xc1 = Math.Sqrt(Math.Pow(cu.R, 2) - Math.Pow(l, 2));
                    double xc2 = -xc1;
                    double zc1 = ((xc1 - start.x) / (end.x - start.x)) * (end.z - start.z) + start.z;
                    double zc2 = ((xc2 - start.x) / (end.x - start.x)) * (end.z - start.z) + start.z;

                    // choose the higher point
                    double zc,xc;
                    if (zc1 > zc2)
                    {
                        zc = zc1;
                        xc = xc1;
                    }
                    else
                    {
                        zc = zc2;
                        xc = xc2;
                    }

                    // now that we have a CC point, check if it's in the edge
                    if ((start.x > xc) && (xc < end.x))
                        return null;
                    else if ((end.x < xc) && (xc > start.x))
                        return null;
                    else
                        return zc;

                }
                // unreachable place (according to compiler)
            } // end of flat endmill (r=0) case
            else if (cu.r > 0)
            {
                // System.Console.WriteLine("edgetest r>0 case!");

                // this is the general case (r>0)   ball-nose or bull-nose (spherical or toroidal)
                // later a separate case for the ball-cutter might be added (for performance)

                double xd=0, w=0, h=0, xd1=0, xd2=0, xc=0 , ze=0, zc=0;

                if (l > cu.R) // edge is outside of the cutter
                    return null;
                else if (((cu.R-cu.r)<l)&&(l<=cu.R))
                {    // toroidal case
                    xd=0; // center of ellipse
                    w=Math.Sqrt(Math.Pow(cu.R,2)-Math.Pow(l,2)); // width of ellipse
                    h=Math.Sqrt(Math.Pow(cu.r,2)-Math.Pow((l-(cu.R-cu.r)),2)); // height of ellipse
                }
                else if ((cu.R-cu.r)>=l)
                {
                    // quarter ellipse case
                    xd1=Math.Sqrt( Math.Pow((cu.R-cu.r),2)-Math.Pow(l,2));
                    xd2=-xd1;
                    h=cu.r; // ellipse height
                    w=Math.Sqrt( Math.Pow(cu.R,2)-Math.Pow(l,2) )- Math.Sqrt( Math.Pow((cu.R-cu.r),2)-Math.Pow(l,2) ); // ellipse height
                }

                // now there is a special case where the theta calculation will fail if
                // the segment is horziontal, i.e. start.z==end.z  so we need to catch that here
                if (start.z==end.z)
                {
                    if ((cu.R-cu.r)<l) 
                    {
                        // half-ellipse case
                        xc=0;
                        h=Math.Sqrt(Math.Pow(cu.r,2)-Math.Pow((l-(cu.R-cu.r)),2));
                        ze = start.z + h - cu.r;
                    }
                    else if ((cu.R - cu.r) > l)
                    {
                        // quarter ellipse case
                        xc = 0;
                        ze = start.z;
                    }

                    // now we have a CC point
                    // so we need to check if the CC point is in the edge
                    if (isinrange(start.x, end.x, xc))
                        return ze;
                    else
                        return null;

                } // end horizontal edge special case


                // now the general case where the theta calculation works
                double theta = Math.Atan( h*(start.x-end.x)/(w*(start.z-end.z))  );

                // based on this calculate the CC point
                if (((cu.R - cu.r) < l) && (cu.R <= l))
                {
                    // half-ellipse case
                    double xc1 = xd + Math.Abs(w * Math.Cos(theta));
                    double xc2 = xd - Math.Abs(w * Math.Cos(theta));
                    double zc1 = ((xc1 - start.x) / (end.x - start.x)) * (end.z - start.z) + start.z;
                    double zc2 = ((xc2 - start.x) / (end.x - start.x)) * (end.z - start.z) + start.z;
                    // select the higher point:
                    if (zc1 > zc2)
                    {
                        zc = zc1;
                        xc = xc1;
                    }
                    else
                    {
                        zc = zc2;
                        xc = xc2;
                    }

                }
                else if ((cu.R - cu.r) > l)
                { 
                    // quarter ellipse case
                    double xc1 = xd1 + Math.Abs(w * Math.Cos(theta));
                    double xc2 = xd2 - Math.Abs(w * Math.Cos(theta));
                    double zc1 = ((xc1 - start.x) / (end.x - start.x)) * (end.z - start.z) + start.z;
                    double zc2 = ((xc2 - start.x) / (end.x - start.x)) * (end.z - start.z) + start.z;
                    // select the higher point:
                    if (zc1 > zc2)
                    {
                        zc = zc1;
                        xc = xc1;
                    }
                    else
                    {
                        zc = zc2;
                        xc = xc2;
                    }
                }

                // now we have a valid xc value, so calculate the ze value:
                ze = zc + Math.Abs(h * Math.Sin(theta)) - cu.r;

                // finally, check that the CC point is in the edge
                if (isinrange(start.x,end.x,xc))
                    return ze;
                else
                    return null;

     


                // this line is unreachable (according to compiler)
                
            } // end of toroidal/spherical case
 
            
            // if we ever get here it is probably a serious error!
            System.Console.WriteLine("EdgeTest: ERROR: no case returned a valid ze!");
            return null;

        } // end of EdgeTest method

        public static bool isinrange(double start, double end, double x)
        {
            // order input
            double s_tmp = start;
            //double e_tmp = end;
            if (start > end)
            {
                start = end;
                end = s_tmp;
            }

            if ((start < x) && (x < end))
                return true;
            else
                return false;
        }

        public static bool isinside(Geo.Tri t, Geo.Point p)
        {
            // point in triangle test

            // a new Tri projected onto the xy plane:
            Geo.Point p1 = new Geo.Point(t.p[0].x, t.p[0].y, 0);
            Geo.Point p2 = new Geo.Point(t.p[1].x, t.p[1].y, 0);
            Geo.Point p3 = new Geo.Point(t.p[2].x, t.p[2].y, 0);
            Geo.Point pt = new Geo.Point(p.x, p.y, 0);

            bool b1 = isright(p1, p2, pt);
            bool b2 = isright(p3, p1, pt);
            bool b3 = isright(p2, p3, pt);

            if ((b1) && (b2) && (b3))
            {
                return true;
            }
            else if ((!b1) && (!b2) && (!b3))
            {
                return true;
            }
            else
            {
                return false;
            }

        } // end isinside()

        public static bool isright(Geo.Point p1, Geo.Point p2, Geo.Point p)
        {
            // is point p right of line through points p1 and p2 ?

            // this is an ugly way of doing a determinant
            // should be prettyfied sometime...
            double a1 = p2.x - p1.x;
            double a2 = p2.y - p1.y;
            double t1 = a2;
            double t2 = -a1;
            double b1 = p.x - p1.x;
            double b2 = p.y - p1.y;

            double t = t1 * b1 + t2 * b2;
            if (t > 0)
                return true;
            else
                return false;
        } // end isright()


   } // end DropCutter class


} // end namespace monoCAM