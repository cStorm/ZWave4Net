﻿using Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave.CommandClasses;

namespace ZWave.Devices.Fibaro
{
    public class MotionSensor : BatteryDevice
    {
        public event AsyncEventHandler<EventArgs> MotionDetected;
        public event AsyncEventHandler<EventArgs> MotionCancelled;
        public event AsyncEventHandler<EventArgs> TamperDetected;
        public event AsyncEventHandler<EventArgs> TamperCancelled;
        public event AsyncEventHandler<MeasureEventArgs> TemperatureMeasured;
        public event AsyncEventHandler<MeasureEventArgs> LuminanceMeasured;

        public MotionSensor(Node node)
            : base(node)
        {
            node.GetCommandClass<Basic>().Changed += Basic_Changed;
            node.GetCommandClass<SensorMultiLevel>().Changed += SensorMultiLevel_Changed;
            node.GetCommandClass<Alarm>().Changed += Alarm_Changed;
        }

        private async Task SensorMultiLevel_Changed(object sender, ReportEventArgs<SensorMultiLevelReport> e)
        {
            if (e.Report.Type == SensorType.Temperature)
            {
                await OnTemperatureMeasured(new MeasureEventArgs(new Measure(e.Report.Value, Unit.Celsius)));
            }
            if (e.Report.Type == SensorType.Luminance)
            {
                await OnLuminanceMeasured(new MeasureEventArgs(new Measure(e.Report.Value, Unit.Lux)));
            }
        }

        private async Task Basic_Changed(object sender, ReportEventArgs<BasicReport> e)
        {
            if (e.Report.Value == 0x00)
            {
                await OnMotionCancelled(EventArgs.Empty);
                return;
            }
            if (e.Report.Value == 0xFF)
            {
                await OnMotionDetected(EventArgs.Empty);
                return;
            }
        }

        public async Task AddAssociation(AssociationGroup group, Node node)
        {
            await Node.GetCommandClass<Association>().Add(Convert.ToByte(group), node.NodeID);
        }

        public async Task RemoveAssociation(AssociationGroup group, Node node)
        {
            await Node.GetCommandClass<Association>().Remove(Convert.ToByte(group), node.NodeID);
        }

        protected virtual async Task OnMotionDetected(EventArgs e)
        {
            await MotionDetected?.Invoke(this, e);
        }

        protected virtual async Task OnMotionCancelled(EventArgs e)
        {
            await MotionCancelled?.Invoke(this, e);
        }

        protected virtual async Task OnTemperatureMeasured(MeasureEventArgs e)
        {
            await TemperatureMeasured?.Invoke(this, e);
        }

        protected virtual async Task OnLuminanceMeasured(MeasureEventArgs e)
        {
            await LuminanceMeasured?.Invoke(this, e);
        }

        private async Task Alarm_Changed(object sender, ReportEventArgs<AlarmReport> e)
        {
            if (e.Report.Type == AlarmType.General)
            {
                if (e.Report.Level == 0x00)
                {
                    await OnTamperCancelled(EventArgs.Empty);
                    return;
                }
                if (e.Report.Level == 0xFF)
                {
                    await OnTamperDetected(EventArgs.Empty);
                    return;
                }
            }
        }

        protected virtual async Task OnTamperDetected(EventArgs e)
        {
            await TamperDetected?.Invoke(this, e);
        }

        protected virtual async Task OnTamperCancelled(EventArgs e)
        {
            await TamperCancelled?.Invoke(this, e);
        }
    }
}
