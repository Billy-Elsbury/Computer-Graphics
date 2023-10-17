using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outcode
{
    bool up, down, left, right;

    public Outcode(bool up, bool down, bool left, bool right)
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
    }

    public Outcode(Vector2 point)
    {
        up = (point.y > 1);
        down = (point.y < -1);
        left = (point.x < -1);
        right = (point.x > 1);
    }

    public Outcode()
    {
        up = false;
        down = false;
        left = false;
        right = false;
    }

    public static Outcode operator * (Outcode outcode1, Outcode outcode2)
    {
        return new Outcode(outcode1.up && outcode2.up, outcode1.down && outcode2.down, outcode1.left && outcode2.left, outcode1.right && outcode2.right);

    }

    public static Outcode operator + (Outcode outcode1, Outcode outcode2)
    {
        return new Outcode(outcode1.up || outcode2.up, outcode1.down || outcode2.down, outcode1.left || outcode2.left, outcode1.right || outcode2.right);

    }

    public static bool operator == (Outcode outcode1, Outcode outcode2) 
    { 
    
        return (outcode1.up == outcode2.up) && (outcode1.down == outcode2.down) && (outcode1.left == outcode2.left) && (outcode1.right == outcode2.right);
    }

    public static bool operator !=(Outcode outcode1, Outcode outcode2)
    {
        return !(outcode1 == outcode2);
    }

    public string outcodeString()
    {
        return (up ? "1" : "0") + (down ? "1" : "0") + (left ? "1" : "0") + (right ? "1" : "0");
    }

    /*Get the outcodes of the two coordinates,
         * If both outcodes are 0000 we can 'trivial accept' the coords
         * If the 'AND' of the two outcodes IS NOT 0000 we can 'trivial reject'
         * If the 'AND' of the coords IS 0000 there is more work to do...*/

}
