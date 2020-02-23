using System;

namespace ForzaListner
{
    public class ForzaPacket
    {

        class Parser
        {
            byte[] data;
            int pos;
            public Parser(byte[] data)
            {
                this.data = data;
                pos = 0;
            }
            public Int32 s32()
            {
                pos += 4;
                return BitConverter.ToInt32(data, pos - 4);
            }

            public float f32()
            {
                pos += 4;
                return BitConverter.ToSingle(data, pos - 4);
            }

            public Byte u8()
            {
                pos += 1;
                return data[pos - 1];
            }

            public SByte s8()
            {
                pos += 1;
                return (SByte)data[pos - 1];
            }

            public UInt16 u16()
            {
                pos += 2;
                return BitConverter.ToUInt16(data, pos - 2);
            }

            public UInt32 u32()
            {
                pos += 4;
                return BitConverter.ToUInt32(data, pos - 4);
            }

            public f32_wheel f32_Wheel()
            {
                f32_wheel wheel;
                wheel.FrontLeft = f32();
                wheel.FrontRight = f32();
                wheel.RearLeft = f32();
                wheel.RearRight = f32();
                return wheel;
            }
            public f32_vec f32_Vec()
            {
                f32_vec vec;
                vec.X = f32();
                vec.Y = f32();
                vec.Z = f32();
                return vec;
            }
        }

        public struct f32_wheel
        {
            public float FrontLeft;
            public float FrontRight;
            public float RearLeft;
            public float RearRight;
        }

        public struct f32_vec
        {
            public float X;
            public float Y;
            public float Z;
        }

        public Int32 isRaceOn;
        public UInt32 timeStampMS;
        public float engineMaxRpm;
        public float engineIdleRpm;
        public float currentEngineRpm;
        public f32_vec acceleration;
        public f32_vec velocity;
        public f32_vec angularVelocity;
        public float pitch;
        public float yaw;
        public float roll;
        public f32_wheel normalizedSuspensionTravel;
        public f32_wheel tireSlipRatio;
        public f32_wheel wheelRotationSpeed;
        public f32_wheel wheelOnRumbleStrip;
        public f32_wheel wheelInPuddleDepth;
        public f32_wheel surfaceRumble;
        public f32_wheel tireSlipAngle;
        public f32_wheel tireCombinedSlip;
        public f32_wheel suspensionTravelMeters;

        public Int32 carOrdinal;
        public Int32 carClass;
        public Int32 carPerfomanceIndex;
        public Int32 driveTrainType;
        public Int32 numCylinders;

        /// If packet 
        public f32_vec position;
        public float speed;
        public float power;
        public float torque;

        public f32_wheel tireTemp;

        public float boost;
        public float fuel;
        public float distanceTraveled;
        public float bestLap;
        public float lastLap;
        public float currentLap;
        public float currentRaceTime;

        public UInt16 lapNumber;
        public Byte racePosition;

        public Byte accel;
        public Byte brake;
        public Byte clutch;
        public Byte handBrake;
        public Byte gear;
        public Byte steer;

        public SByte normalizedDrivingLine;
        public SByte normalizedAIBrakeDifference;
        public SByte last;

        public Byte b1;
        public Byte b2;
        public Byte b3;
        public Byte b4;
        public Byte b5;
        public Byte b6;
        public Byte b7;
        public Byte b8;
        public Byte b9;
        public Byte ba;
        public Byte bb;
        public Byte bc;

        public bool hasDashData;

        public ForzaPacket(byte[] data)
        {
            var read = new Parser(data);

            isRaceOn = read.s32();
            timeStampMS = read.u32();
            engineMaxRpm = read.f32();
            engineIdleRpm = read.f32();
            currentEngineRpm = read.f32();
            acceleration = read.f32_Vec();
            velocity = read.f32_Vec();
            angularVelocity = read.f32_Vec();
            pitch = read.f32();
            yaw = read.f32();
            roll = read.f32();
            normalizedSuspensionTravel = read.f32_Wheel();
            tireSlipRatio = read.f32_Wheel();
            wheelRotationSpeed = read.f32_Wheel();
            wheelOnRumbleStrip = read.f32_Wheel();
            wheelInPuddleDepth = read.f32_Wheel();
            surfaceRumble = read.f32_Wheel();
            tireSlipAngle = read.f32_Wheel();
            tireCombinedSlip = read.f32_Wheel();
            suspensionTravelMeters = read.f32_Wheel();
            carOrdinal = read.s32();
            carClass = read.s32();
            carPerfomanceIndex = read.s32();
            driveTrainType = read.s32();
            numCylinders = read.s32();

            hasDashData = data.Length >= 232;
            //
            if (data.Length >= 324)
            {
                b1 = read.u8();
                b2 = read.u8();
                b3 = read.u8();
                b4 = read.u8();
                b5 = read.u8();
                b6 = read.u8();
                b7 = read.u8();
                b8 = read.u8();
                b9 = read.u8();
                ba = read.u8();
                bb = read.u8();
                bc = read.u8();
            }
            if (hasDashData)
            {

                // Dash data
                position = read.f32_Vec();
                speed = read.f32();
                power = read.f32();
                torque = read.f32();
                tireTemp = read.f32_Wheel();
                boost = read.f32();
                fuel = read.f32();
                distanceTraveled = read.f32();
                bestLap = read.f32();
                lastLap = read.f32();
                currentLap = read.f32();
                currentRaceTime = read.f32();
                lapNumber = read.u16();
                racePosition = read.u8();
                accel = read.u8();
                brake = read.u8();
                clutch = read.u8();
                handBrake = read.u8();
                gear = read.u8();
                steer = read.u8();
                normalizedDrivingLine = read.s8();
                normalizedAIBrakeDifference = read.s8();
            }
            if (data.Length >= 324)
                last = read.s8();
        }
    }
}
