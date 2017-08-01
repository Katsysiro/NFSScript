// [----------------------------------------------------------------------------------------------------------------------------------------------]
// This Quaternion class has been re-written in C# from SlimDX's C++ code.
// Without SlimDX I'd be stuck infront of mathhelp and mathworks asking myself why it was a good idea to start this project in the first place.
// So big thanks to SlimDX's Group <3
// [----------------------------------------------------------------------------------------------------------------------------------------------]
/*
 * Copyright (C) 2007-2010 SlimDX Group
 *
 * Permission is hereby granted, free  of charge, to any person obtaining a copy of this software  and
 * associated  documentation  files (the  "Software"), to deal  in the Software  without  restriction,
 * including  without  limitation  the  rights  to use,  copy,  modify,  merge,  publish,  distribute,
 * sublicense, and/or sell  copies of the  Software,  and to permit  persons to whom  the Software  is
 * furnished to do so, subject to the following conditions:
 *
 * The  above  copyright  notice  and this  permission  notice shall  be included  in  all  copies  or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS",  WITHOUT WARRANTY OF  ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
 * NOT  LIMITED  TO  THE  WARRANTIES  OF  MERCHANTABILITY,  FITNESS  FOR  A   PARTICULAR  PURPOSE  AND
 * NONINFRINGEMENT.  IN  NO  EVENT SHALL THE  AUTHORS  OR COPYRIGHT HOLDERS  BE LIABLE FOR  ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  OUT
 * OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using Maths = System.Math;

namespace NFSScript.Math
{
    /// <summary>
    /// Quaternions are used to represent rotations.
    /// </summary>
    public struct Quaternion
    {
        /// <summary>
        /// Returns a <see cref="Quaternion"/> representing no rotation.
        /// </summary>
        public static Quaternion Identity => new Quaternion(0, 0, 0, 1);

        /// <summary>
        /// W component of the <see cref="Quaternion"/>.
        /// </summary>
        public float W { get; private set; }

        /// <summary>
        /// X component of the <see cref="Quaternion"/>.
        /// </summary>
        public float X { get; private set; }

        /// <summary>
        /// Y component of the <see cref="Quaternion"/>.
        /// </summary>
        public float Y { get; private set; }

        /// <summary>
        /// Z component of the <see cref="Quaternion"/>.
        /// </summary>
        public float Z { get; private set; }

        /// <summary>
        /// Returns the length of this <see cref="Quaternion"/>.
        /// </summary>
        public float Magnitude => (float)Maths.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));

        /// <summary>
        /// Returns the squared length of this <see cref="Quaternion"/>.
        /// </summary>
        public float MagnitudeSquared => ((X * X) + (Y * Y) + (Z * Z) + (W * W));

        /// <summary>
        /// Axis of this <see cref="Quaternion"/>.
        /// </summary>
        public Vector3 Axis
        {
            get
            {
                float x, y, z;
                if (Maths.Abs(Magnitude) > float.Epsilon)
                {
                    var inverseLength = 1.0f / Magnitude;
                    x = X * inverseLength;
                    y = Y * inverseLength;
                    z = Z * inverseLength;
                }
                else
                {
                    x = 1.0f;
                    y = 0.0f;
                    z = 0.0f;
                }

                return new Vector3(x, y, z);                
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quaternion"/> class with the specified x,y,z,w components.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quaternion"/> class using components specified by an Vector3 value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="w"></param>
        public Quaternion(Vector3 value, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        /// <summary>
        /// Set the XYZW values of this <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public void Set(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Returns a quaternion with the same rotation as the specified <see cref="Quaternion"/>, but with a length of one.
        /// </summary>
        /// <param name="quat"></param>
        /// <returns></returns>
        public static Quaternion Normalize(Quaternion quat)
        {
            var length = 1.0f / quat.Magnitude;
            var x = quat.X * length;
            var y = quat.Y * length;
            var z = quat.Z * length;
            var w = quat.W * length;

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Dot product between two rotations.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static float Dot(Quaternion left, Quaternion right)
        {
            return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

        /// <summary>
        /// Returns the angle between a and b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float AngleBetween(Quaternion a, Quaternion b)
        {
            var dot = Dot(a, b);
            return (float)((Maths.Acos(Maths.Min(Maths.Abs(dot), 1.0f)) * 2.0 * (180.0f / Mathf.PI)));
        }

        /// <summary>
        /// Interpolates between a and b by a factor and normalizes the result afterwards.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Quaternion Lerp(Quaternion a, Quaternion b, float factor)
        {
            float x, y, z, w;

            var inverse = 1.0f - factor;
            var dot = Dot(a, b);

            if (dot >= 0.0f)
            {
                x = (inverse * a.X) + (factor * b.X);
                y = (inverse * a.Y) + (factor * b.Y);
                z = (inverse * a.Z) + (factor * b.Z);
                w = (inverse * a.W) + (factor * b.W);
            }
            else
            {
                x = (inverse * a.X) - (factor * b.X);
                y = (inverse * a.Y) - (factor * b.Y);
                z = (inverse * a.Z) - (factor * b.Z);
                w = (inverse * a.W) - (factor * b.W);
            }

            var inverseLength = 1.0f / new Quaternion(x, y, z, w).Magnitude;

            x *= inverseLength;
            y *= inverseLength;
            z *= inverseLength;
            w *= inverseLength;

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Spherically interpolates between a and b by a factor.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Quaternion Slerp(Quaternion a, Quaternion b, float factor)
        {
            float x, y, z, w;
            float opposite, inverse;
            var dot = Dot(a, b);

            if (Maths.Abs(dot) > (1.0 - Mathf.Epsilon))
            {
                inverse = 1.0f - factor;
                opposite = factor * Maths.Sign(dot);
            }
            else
            {
                var acos = (float)Maths.Acos(Maths.Abs(dot));
                var inverseSin = (float)(1.0 / Maths.Sin(acos));

                inverse = (float)(Maths.Sin((1.0f - factor) * acos) * inverseSin);
                opposite = (float)(Maths.Sin(factor * acos) * inverseSin * Maths.Sign(dot));
            }

            x = (inverse * a.X) + (opposite * b.X);
            y = (inverse * a.Y) + (opposite * b.Y);
            z = (inverse * a.Z) + (opposite * b.Z);
            w = (inverse * a.W) + (opposite * b.W);

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Spherically interpolates between a and b by factor. The parameter factor is not clamped.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, float factor)
        {
            if (Maths.Abs(a.MagnitudeSquared) < float.Epsilon)
            {
                return Maths.Abs(b.MagnitudeSquared) < float.Epsilon ? Identity : b;
            }
            if (Maths.Abs(b.MagnitudeSquared) < float.Epsilon)
            {
                return a;
            }

            var cosHalfAngle = a.W * b.W + Vector3.Dot(a.Axis, b.Axis);
            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
                return a;
            if (cosHalfAngle < 0.0f)
            {
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA, blendB;

            if (cosHalfAngle < 0.99f)
            {
                var halfAngle = (float)Maths.Acos(cosHalfAngle);
                var sinHalfAngle = (float)Maths.Sin(halfAngle);
                var oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (float)Maths.Sin(halfAngle * (1.0f - factor)) * oneOverSinHalfAngle;
                blendB = (float)Maths.Sin(halfAngle * factor) * oneOverSinHalfAngle;
            }
            else
            {
                blendA = 1.0f - factor;
                blendB = factor;
            }

            var result = new Quaternion(blendA * a.Axis + blendB, blendA * a.W + blendB + b.W);

            if (result.MagnitudeSquared > 0.0f)
                return Normalize(result);
            return Identity;
        }

        /// <summary>
        /// Returns the Inverse of rotation.
        /// </summary>
        /// <param name="quat"></param>
        /// <returns></returns>
        public static Quaternion Inverse(Quaternion quat)
        {
            float x, y, z, w;
            var lengthSqr = quat.MagnitudeSquared;

            x = -quat.X * lengthSqr;
            y = -quat.Y * lengthSqr;
            z = -quat.Z * lengthSqr;
            w = -quat.W * lengthSqr;

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Rotates a rotation from towards to.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="maxDegressDelta"></param>
        /// <returns></returns>
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegressDelta)
        {
            var angle = AngleBetween(from, to);
            if (Maths.Abs(angle) < float.Epsilon)
                return to;

            var t = Maths.Min(1.0f, maxDegressDelta / angle);
            return SlerpUnclamped(from, to, t);
        }

        /// <summary>
        /// Returns a quaternion with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <param name="roll"></param>
        /// <returns></returns>
        public static Quaternion RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            var halfRoll = roll * 0.5f;
            var sinRoll = (float)Maths.Sin(halfRoll);
            var cosRoll = (float)Maths.Cos(halfRoll);
            var halfPitch = pitch * 0.5f;
            var sinPitch = (float)Maths.Sin(halfPitch);
            var cosPitch = (float)Maths.Cos(halfPitch);
            var halfYaw = yaw * 0.5f;
            var sinYaw = (float)Maths.Sin(halfYaw);
            var cosYaw = (float)Maths.Cos(halfYaw);

            var x = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
            var y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
            var z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
            var w = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Quaternion Euler(float x, float y, float z)
        {
            return RotationYawPitchRoll(x * Mathf.Deg2Rad, y * Mathf.Deg2Rad, z * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis.
        /// </summary>
        /// <param name="euler"></param>
        /// <returns></returns>
        public static Quaternion Euler(Vector3 euler)
        {
            var eulerRad = euler * Mathf.Deg2Rad;
            return RotationYawPitchRoll(eulerRad.X, eulerRad.Y, eulerRad.Z);
        }

        /// <summary>
        /// Creates a rotation which rotates from fromDirection to toDirection.
        /// </summary>
        /// <param name="fromDir"></param>
        /// <param name="toDir"></param>
        /// <returns></returns>
        public static Quaternion FromToRotation(Vector3 fromDir, Vector3 toDir)
        {
            var abNorm = (float)Maths.Sqrt(fromDir.MagnitudeSquared * fromDir.MagnitudeSquared);
            var w = abNorm + Vector3.Dot(fromDir, toDir);

            Quaternion result;
            if (w >= 1e-6f * abNorm)
                result = new Quaternion(Vector3.Cross(fromDir, toDir), w);
            else
            {
                w = 0.0f;
                result = (Maths.Abs(fromDir.X) > Maths.Abs(fromDir.Y) ? new Quaternion(-fromDir.Z, 0.0f, fromDir.X, w) : new Quaternion(0.0f, -fromDir.Z, fromDir.Y, w));
            }

            return Normalize(result);
        }

        /// <summary>
        /// Returns a formmated <see cref="Quaternion"/> string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"W = {W} X = {X} Y = {Y} Z = {Z}";
        }

        /// <summary>
        /// Returns a formmated <see cref="Quaternion"/> string with a specified number format.
        /// </summary>
        /// <returns></returns>
        public string ToString(string numberFormat)
        {
            return
                $"W = {W.ToString(numberFormat)} X = {X.ToString(numberFormat)} Y = {Y.ToString(numberFormat)} Z = {Z.ToString(numberFormat)}";
        }
    }
}
