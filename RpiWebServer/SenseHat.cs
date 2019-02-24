using System;
using System.Threading;
using System.Threading.Tasks;
using Emmellsoft.IoT.Rpi.SenseHat;
using Windows.UI;

namespace RpiWebServer
{
    class SenseHat : IDisposable
    {
        private ISenseHat _senseHat { get; set; }

        public ISenseHatSensors GetSensors()
        {
            return _senseHat.Sensors;
        }

        public async Task Activate()
        {
            _senseHat = await SenseHatFactory.GetSenseHat().ConfigureAwait(false);
            
            _senseHat.Display.Clear();
            _senseHat.Display.Update();
        }

        public void Dispose()
        {
            _senseHat.Dispose();
        }

        public Double GetTemperatureCelsius()
        {
            while (true)
            {
                _senseHat.Sensors.HumiditySensor.Update();

                if (_senseHat.Sensors.Temperature.HasValue)
                {
                    return Math.Round(_senseHat.Sensors.Temperature.Value, 2);
                }
                else new ManualResetEventSlim(false).Wait(TimeSpan.FromSeconds(0.5));
            }
        }

        public Double GetTemperatureFahrenheit()
        {
            while (true)
            {
                _senseHat.Sensors.HumiditySensor.Update();

                if (_senseHat.Sensors.Temperature.HasValue)
                {
                    return Math.Round((_senseHat.Sensors.Temperature.Value * 9 / 5) + 32, 2);
                }
                else new ManualResetEventSlim(false).Wait(TimeSpan.FromSeconds(0.5));
            }
        }

        public Double GetHumidity()
        {
            while (true)
            {
                _senseHat.Sensors.HumiditySensor.Update();
                if (_senseHat.Sensors.Humidity.HasValue)
                {
                    return Math.Round(_senseHat.Sensors.Humidity.Value, 2);
                }
                else new ManualResetEventSlim(false).Wait(TimeSpan.FromSeconds(0.5));
            }
        }

        public Double GetPressure()
        {
            while (true)
            {
                _senseHat.Sensors.PressureSensor.Update();
                if (_senseHat.Sensors.Pressure.HasValue)
                {
                    return Math.Round(_senseHat.Sensors.Pressure.Value, 2);
                }
                else new ManualResetEventSlim(false).Wait(TimeSpan.FromSeconds(0.5));
            }
        }

        public void ClearDisplay() { _senseHat.Display.Clear(); }
        public void UpdateDisplay() { _senseHat.Display.Update(); }

        public void SetPixle(int x, int y, Color color)
        {
            _senseHat.Display.Screen[x, y] = color;
        }

        public void FillDisplay(Color color)
        {
            _senseHat.Display.Fill(color);
        }

        public void ChaseDisplay(Color color)
        {
            int row = 0;
            int col = 0;

            _senseHat.Display.Clear();

            for (row = 0; row < 8; row++)
                for (col = 0; col < 8; col++)
                {
                    _senseHat.Display.Screen[row, col] = color;
                    _senseHat.Display.Update();
                }

            for (row = 0; row < 8; row++)
                for (col = 7; col >= 0; col--)
                {
                    _senseHat.Display.Screen[row, col] = Color.FromArgb(0, 0, 0, 0);
                    _senseHat.Display.Update();
                }
        }
    }
}
