using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static ForzaListner.ForzaPacket;

namespace ForzaListner
{


    public class ForzaConfig
    {
        public int port { get; set; } = 5200;
    }

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
                return BitConverter.ToInt32(data, pos -4);
            }

            public float f32()
            {
                pos += 4;
                return BitConverter.ToSingle(data, pos -4);
            }

            public Byte u8()
            {
                pos += 1;
                return data[pos - 1];
            }

            public SByte s8()
            {
                pos += 1;
                return (SByte)data[pos-1];
            }

            public UInt16 u16()
            {
                pos+=2;
                return BitConverter.ToUInt16(data, pos - 2);
            }

            public UInt32 u32()
            {
                pos += 4;
                return BitConverter.ToUInt32(data, pos-4);
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
            //
            if(data.Length >= 324)
            {
                b1= read.u8();
                b2= read.u8();
                b3=read.u8();
                b4=read.u8();
                b5=read.u8();
                b6=read.u8();
                b7=read.u8();
                b8=read.u8();
                b9=read.u8();
                ba=read.u8();
                bb=read.u8();
                bc=read.u8();
            }
            if (data.Length >= 232)
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
            if(data.Length >= 324)
              last = read.s8();
        }
    }

    public static class MetricsExtensions
    {
        public static void Gauge(this IMetrics metrics, string name, f32_wheel wheel, float scale=1)
        {
            metrics.Gauge($"wheels.front_left.{name}", wheel.FrontLeft*scale );
            metrics.Gauge($"wheels.front_right.{name}", wheel.FrontRight*scale);
            metrics.Gauge($"wheels.rear_left.{name}", wheel.RearLeft*scale);
            metrics.Gauge($"wheels.rear_right.{name}", wheel.RearRight*scale);
        }

        public static void Gauge(this IMetrics metrics, string name, SByte value)
        {
            metrics.Gauge(name, (uint)(value) & 0xff);
        }

        public static void Gauge(this IMetrics metrics, string name, Int32 value)
        {
            metrics.Gauge(name, (uint)value);
        }

        public static void Gauge(this IMetrics metrics, string name, float value)
        {
            metrics.Gauge(name, (uint)Math.Abs(value));
        }
    }

    class ForzaState
    {


    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ForzaConfig config;
        private readonly IMetrics metrics;

        public Worker(ILogger<Worker> logger, IOptions<ForzaConfig> options, IMetrics metrics)
        {
            _logger = logger;
            config = options.Value;
            this.metrics = metrics;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            UdpClient server = new UdpClient(config.port);

            _logger.LogInformation($"Listening on port: {config.port}");
            var result = await server.ReceiveAsync();
            var address = result.RemoteEndPoint;
            var data = result.Buffer;
            _logger.LogInformation($"Receive {data.Length} bytes from {address}");

            Random random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                address = new IPEndPoint(0,0);
                data = server.Receive(ref address);

                var packet = new ForzaPacket(data);

                metrics.Gauge("timeStampMS", packet.timeStampMS);
                metrics.Gauge("dash.rpm", packet.currentEngineRpm);
                metrics.Gauge("dash.rpm_idle", packet.engineIdleRpm);
                metrics.Gauge("dash.rpm_max", packet.engineMaxRpm);
                metrics.Gauge("temp",packet.tireTemp);
                metrics.Gauge("rotation_speed", packet.wheelRotationSpeed);
                metrics.Gauge("slip_ratio", packet.tireSlipRatio);
                metrics.Gauge("slip_angle", packet.tireSlipAngle);
                metrics.Gauge("slip_combined", packet.tireCombinedSlip);
                metrics.Gauge("on_rumblestrip", packet.wheelOnRumbleStrip);
                metrics.Gauge("in_puddle", packet.wheelInPuddleDepth);

                metrics.Gauge("physics.velocity.x", packet.velocity.X);
                metrics.Gauge("physics.velocity.y", packet.velocity.Y);
                metrics.Gauge("physics.velocity.z", packet.velocity.Z);
                metrics.Gauge("physics.pitchyawroll.x", packet.pitch);
                metrics.Gauge("physics.pitchyawroll.y", packet.yaw);
                metrics.Gauge("physics.pitchyawroll.z", packet.roll);
                metrics.Gauge("physics.angular_velocity.x", packet.angularVelocity.X);
                metrics.Gauge("physics.angular_velocity.y", packet.angularVelocity.Y);
                metrics.Gauge("physics.angular_velocity.z", packet.angularVelocity.Z);
                metrics.Gauge("physics.acceleration.x", packet.acceleration.X);
                metrics.Gauge("physics.acceleration.y", packet.acceleration.Y);
                metrics.Gauge("physics.acceleration.z", packet.acceleration.Z);

                metrics.Gauge("dash.speed", packet.speed);
                metrics.Gauge("dash.power", packet.power);
                metrics.Gauge("dash.torque", packet.torque);

                metrics.Gauge("race.is_race_on", packet.isRaceOn);
                metrics.Gauge("race.lap_number", packet.lapNumber);
                metrics.Gauge("race.position", packet.racePosition);
                metrics.Gauge("race.timings.best", packet.bestLap);
                metrics.Gauge("race.timings.last", packet.lastLap);
                metrics.Gauge("race.timings.current", packet.currentLap);
                metrics.Gauge("race.boost", packet.boost);
                metrics.Gauge("race.fuel", packet.fuel);
                metrics.Gauge("race.distanceTravelled", packet.distanceTraveled);

                metrics.Gauge("suspension_travel_cm", packet.suspensionTravelMeters, 100);

                metrics.Gauge("car.driveTrainType", packet.driveTrainType);
                metrics.Gauge("car.ordinal", packet.carOrdinal);
                metrics.Gauge("car.class", packet.carClass);
                metrics.Gauge("car.cylinders", packet.numCylinders);

                metrics.Gauge("input.brake", packet.brake);
                metrics.Gauge("input.hand_brake",packet.handBrake);
                metrics.Gauge("input.accel", packet.accel);
                metrics.Gauge("input.steer", packet.steer);
                metrics.Gauge("input.clutch", packet.clutch);
                metrics.Gauge("input.gear", packet.gear);

                metrics.Gauge("assist.aiBrakeDifference", packet.normalizedAIBrakeDifference);
                metrics.Gauge("assist.drivingLine", packet.normalizedDrivingLine);

                metrics.Gauge("unknown.b232", packet.b1);
                metrics.Gauge("unknown.b233", packet.b2);
                metrics.Gauge("unknown.b234", packet.b3);
                metrics.Gauge("unknown.b235", packet.b4);
                metrics.Gauge("unknown.b236", packet.b5);
                metrics.Gauge("unknown.b237", packet.b6);
                metrics.Gauge("unknown.b238", packet.b7);
                metrics.Gauge("unknown.b239", packet.b8);
                metrics.Gauge("unknown.b240", packet.b9);
                metrics.Gauge("unknown.b241", packet.ba);
                metrics.Gauge("unknown.b242", packet.bb);
                metrics.Gauge("unknown.b243", packet.bc);

                if(random.NextDouble() <= 0.001)
                  _logger.LogInformation($"{packet.timeStampMS}: receive {data.Length} bytes from {address}");
            }
        }
    }
}
