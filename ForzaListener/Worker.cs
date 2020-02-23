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

    public static class MetricsExtensions
    {
        public static void Gauge(this IMetrics metrics, string name, f32_wheel wheel, float scale=1)
        {
            metrics.Gauge($"wheels.front_left.{name}", wheel.FrontLeft*scale );
            metrics.Gauge($"wheels.front_right.{name}", wheel.FrontRight*scale);
            metrics.Gauge($"wheels.rear_left.{name}", wheel.RearLeft*scale);
            metrics.Gauge($"wheels.rear_right.{name}", wheel.RearRight*scale);
        }

        public static void Gauge(this IMetrics metrics, string name, f32_vec vec, float scale = 1)
        {
            metrics.Gauge($"{name}.x", vec.X * scale);
            metrics.Gauge($"{name}.y", vec.Y * scale);
            metrics.Gauge($"{name}.z", vec.Z * scale);
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

                metrics.Gauge("race.is_race_on", packet.isRaceOn);
                metrics.Gauge("timeStampMS", packet.timeStampMS);

                metrics.Gauge("dash.rpm", packet.currentEngineRpm);
                metrics.Gauge("dash.rpm_idle", packet.engineIdleRpm);
                metrics.Gauge("dash.rpm_max", packet.engineMaxRpm);

                metrics.Gauge("physics.velocity", packet.velocity);
                metrics.Gauge("physics.pitchyawroll", packet.pitch);
                metrics.Gauge("physics.angular_velocity", packet.angularVelocity);
                metrics.Gauge("physics.acceleration", packet.acceleration);

                metrics.Gauge("car.driveTrainType", packet.driveTrainType);
                metrics.Gauge("car.ordinal", packet.carOrdinal);
                metrics.Gauge("car.class", packet.carClass);
                metrics.Gauge("car.PI", packet.carPerfomanceIndex);
                metrics.Gauge("car.cylinders", packet.numCylinders);

                // Wheel and Suspension.
                metrics.Gauge("rotation_speed", packet.wheelRotationSpeed);
                metrics.Gauge("slip_ratio", packet.tireSlipRatio);
                metrics.Gauge("slip_angle", packet.tireSlipAngle);
                metrics.Gauge("slip_combined", packet.tireCombinedSlip);
                metrics.Gauge("on_rumblestrip", packet.wheelOnRumbleStrip);
                metrics.Gauge("in_puddle", packet.wheelInPuddleDepth);
                metrics.Gauge("suspension_travel_mm", packet.suspensionTravelMeters, 1000);
                metrics.Gauge("suspension_travel", packet.normalizedSuspensionTravel);

                if (packet.hasDashData)
                {
                    metrics.Gauge("world.position", packet.position);

                    metrics.Gauge("dash.speed", packet.speed);
                    metrics.Gauge("dash.power", packet.power);
                    metrics.Gauge("dash.torque", packet.torque);
                    metrics.Gauge("temp", packet.tireTemp);
                    metrics.Gauge("dash.boost", packet.boost);
                    metrics.Gauge("dash.fuel", packet.fuel);
                    metrics.Gauge("dash.distanceTravelled", packet.distanceTraveled);
                    metrics.Gauge("race.timings.best", packet.bestLap);
                    metrics.Gauge("race.timings.last", packet.lastLap);
                    metrics.Gauge("race.timings.current", packet.currentLap);
                    metrics.Gauge("race.time.current", packet.currentRaceTime);
                    metrics.Gauge("race.lap_number", packet.lapNumber);
                    metrics.Gauge("race.position", packet.racePosition);
                    metrics.Gauge("input.accel", packet.accel);
                    metrics.Gauge("input.brake", packet.brake);
                    metrics.Gauge("input.clutch", packet.clutch);
                    metrics.Gauge("input.hand_brake", packet.handBrake);
                    metrics.Gauge("input.gear", packet.gear);
                    metrics.Gauge("input.steer", packet.steer);
                    metrics.Gauge("assist.aiBrakeDifference", packet.normalizedAIBrakeDifference);
                    metrics.Gauge("assist.drivingLine", packet.normalizedDrivingLine);
                }




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
                metrics.Gauge("unknown.b343", packet.last);

                if (random.NextDouble() <= 0.001)
                  _logger.LogInformation($"{packet.timeStampMS}: receive {data.Length} bytes from {address}");
            }
        }
    }
}
