﻿using System;
using System.Diagnostics;
using Eco2.Models;
using System.Text;
using System.Linq;

namespace Eco2.Parsing
{
    public class ParsedThermostat
    {
        public enum ScheduleModes
        {
            MANUAL,
            SCHEDULED,
            VACATION,
            UNKNOWN
        }

        readonly Thermostat thermostat;
        readonly Encryption.Encryption encryption;
        readonly byte[] batteryLevelBytes;
        readonly byte[] deviceNameBytes;
        readonly byte[] settingsBytes;
        readonly byte[] temperatureBytes;
        readonly byte[] schedule1Bytes;
        readonly byte[] schedule2Bytes;
        readonly byte[] schedule3Bytes;

        public ParsedThermostat(Thermostat thermostat)
        {
            this.thermostat = thermostat;
            this.encryption = new Encryption.Encryption(thermostat.SecretKey);

            batteryLevelBytes = Conversion.HexStringToByteArray(thermostat.BatteryLevel);
            Trace.Assert(batteryLevelBytes.Length == 1, "Expected battery level to be 1 byte, got " + batteryLevelBytes.Length);

            deviceNameBytes = encryption.Decrypt(thermostat.Name);

            settingsBytes = encryption.Decrypt(thermostat.Settings);
            Trace.Assert(settingsBytes.Length == 16, "Expected settings to be 16 bytes, got " + settingsBytes.Length);

            temperatureBytes = encryption.Decrypt(thermostat.Temperature);
            Trace.Assert(temperatureBytes.Length == 8, "Expected temperature to be 8 bytes, got " + temperatureBytes.Length);

            schedule1Bytes = encryption.Decrypt(thermostat.Schedule1);
            Trace.Assert(schedule1Bytes.Length == 20, "Expected schedule1 to be 20 bytes, got " + schedule1Bytes.Length);
            schedule2Bytes = encryption.Decrypt(thermostat.Schedule2);
            Trace.Assert(schedule2Bytes.Length == 12, "Expected schedule2 to be 12 bytes, got " + schedule2Bytes.Length);
            schedule3Bytes = encryption.Decrypt(thermostat.Schedule3);
            Trace.Assert(schedule3Bytes.Length == 12, "Expected schedule3 to be 12 bytes, got " + schedule3Bytes.Length);
        }

        public int BatteryLevelPercent
        {
            get { return batteryLevelBytes [0]; }
        }

        public string DeviceName
        {
            get { return Encoding.ASCII.GetString((byte[])(Array)deviceNameBytes); }
        }

        public Temperature FrostProtectionTemperature
        {
            get { return new Temperature(settingsBytes[3]); }
        }

        public Temperature VacationTemperature
        {
            get { return new Temperature(settingsBytes[5]); }
        }

        public ScheduleModes ScheduleMode
        {
            get
            {
                switch (settingsBytes[4])
                {
                    case 0:
                        return ScheduleModes.MANUAL;
                    case 1:
                        return ScheduleModes.SCHEDULED;
                    case 3:
                        return ScheduleModes.VACATION;
                    default:
                        return ScheduleModes.UNKNOWN;
                }
            }
        }

        public DateTime? VacationFrom
        {
            get { return Timestamp.Parse(settingsBytes.Skip(6).Take(4)); }
        }

        public DateTime? VacationTo
        {
            get { return Timestamp.Parse(settingsBytes.Skip(10).Take(4)); }
        }

        public Temperature SetPointTemperature
        {
            get { return new Temperature(temperatureBytes[0]); }
        }

        public Temperature RoomTemperature
        {
            get { return new Temperature(temperatureBytes[1]); }
        }

        public Temperature HomeTemperature
        {
            get { return new Temperature(schedule1Bytes[0]); }
        }

        public Temperature AwayTemperature
        {
            get { return new Temperature(schedule1Bytes[1]); }
        }

        public DailySchedule MondaySchedule
        {
            get { return DailySchedule.Parse(schedule1Bytes.Skip(2).Take(6)); }
        }

        public DailySchedule TuesdaySchedule
        {
            get { return DailySchedule.Parse(schedule1Bytes.Skip(8).Take(6)); }
        }

        public DailySchedule WednesdaySchedule
        {
            get { return DailySchedule.Parse(schedule1Bytes.Skip(14).Take(6)); }
        }

        public DailySchedule ThursdaySchedule
        {
            get { return DailySchedule.Parse(schedule2Bytes.Take(6)); }
        }

        public DailySchedule FridaySchedule
        {
            get { return DailySchedule.Parse(schedule2Bytes.Skip(6).Take(6)); }
        }

        public DailySchedule SaturdaySchedule
        {
            get { return DailySchedule.Parse(schedule3Bytes.Take(6)); }
        }

        public DailySchedule SundaySchedule
        {
            get { return DailySchedule.Parse(schedule3Bytes.Skip(6).Take(6)); }
        }
    }
}
