using UnityEngine;
using System.Collections;

namespace VoxelEngine.DoubleKeyDictionary
{
    public class IntPair
    {

        public IntPair(int first, int second)
        {
            this.first = first;
            this.second = second;
        }

        public IntPair(float first, float second)
        {
            this.first = Mathf.FloorToInt(first);
            this.second = Mathf.FloorToInt(second);
        }

        public IntPair(IntPair x)
        {
            this.first = x.first;
            this.second = x.second;
        }

        public int first { get; set; }
        public int second { get; set; }

        public void Set(int first, int second)
        {
            this.first = first;
            this.second = second;
        }

        public void Set(float first, float second)
        {
            this.first = Mathf.FloorToInt(first);
            this.second = Mathf.FloorToInt(second);
        }

        #region Comparison
        public static bool operator ==(IntPair x, IntPair y)
        {
            return (x.first == y.first) && (x.second == y.second);
        }

        public static bool operator !=(IntPair x, IntPair y)
        {
            return !(x == y);
        }

        #endregion

        #region Arithmetic operators
        public static IntPair operator - (IntPair x)
        {
            return new IntPair(-x.first, -x.second);
        }
        public static IntPair operator + (IntPair x, IntPair y)
        {
            return new IntPair(x.first + y.first, x.second + y.second);
        }
        public static IntPair operator + (IntPair x, int y)
        {
            return new IntPair(x.first + y, x.second + y);
        }
        public static IntPair operator - (IntPair x, IntPair y)
        {
            return new IntPair(x.first - y.first, x.second - y.second);
        }
        public static IntPair operator - (IntPair x, int y)
        {
            return new IntPair(x.first - y, x.second - y);
        }
        public static IntPair operator * (IntPair x, int y)
        {
            return new IntPair(x.first * y, x.second * y);
        }
        public static IntPair operator * (int y, IntPair x)
        {
            return new IntPair(x.first * y, x.second * y);
        }
        public static IntPair operator ++(IntPair x)
        {
            return new IntPair(x + 1);
        }
        public static IntPair operator -- (IntPair x)
        {
            return new IntPair(x - 1);
        }
        #endregion

        #region String Conversion
        public override string ToString()
        {
            return first + " , " + second;
        }
        #endregion
    }
}
