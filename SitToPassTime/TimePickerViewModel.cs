using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StardewValley;

namespace SitToPassTime
{
    internal class TimePickerViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        private int _hour;   // 1–12
        private int _minute; // 0 or 30 (only half-hour steps — matches game's 10-min tick system rounded up)
        private bool _isPm;

        private readonly Action<int> _onConfirm;

        private readonly Action _onCancel;
        
        public string HourDisplay   => _hour.ToString("D2");
        
        public string MinuteDisplay => _minute.ToString("D2");

        public string AmPmLabel => _isPm ? "PM" : "AM";

        public bool ShowWarning => SelectedGameTime() <= Game1.timeOfDay;

        public TimePickerViewModel(Action<int> onConfirm, Action onCancel)
        {
            _onConfirm = onConfirm;
            _onCancel  = onCancel;
            
            int curMins  = Utility.ConvertTimeToMinutes(Game1.timeOfDay);
            int nextHour = ((curMins / 60) + 1) * 60;
            nextHour = Math.Min(nextHour, 25 * 60);

            int totalHour24 = nextHour / 60;
            _minute = 0;
            
            _isPm  = totalHour24 >= 12;
            int h12 = totalHour24 % 12;
            _hour  = h12 == 0 ? 12 : h12;
        }

        public void HourUp()
        {
            _hour = (_hour % 12) + 1;
            Notify(nameof(HourDisplay));
            Notify(nameof(ShowWarning));
        }

        public void HourDown()
        {
            _hour = _hour == 1 ? 12 : _hour - 1;
            Notify(nameof(HourDisplay));
            Notify(nameof(ShowWarning));
        }

        public void MinuteUp()
        {
            _minute = (_minute + 10) % 60;
            Notify(nameof(MinuteDisplay));
            Notify(nameof(ShowWarning));
        }

        public void MinuteDown()
        {
            _minute = _minute == 0 ? 50 : _minute - 10;
            Notify(nameof(MinuteDisplay));
            Notify(nameof(ShowWarning));
        }

        public void ToggleAmPm()
        {
            _isPm = !_isPm;
            Notify(nameof(AmPmLabel));
            Notify(nameof(ShowWarning));
        }

        public void Confirm()
        {
            int target = SelectedGameTime();
            if (target <= Game1.timeOfDay) return;
            _onConfirm(target);
        }

        public void Cancel() => _onCancel();
        
        private int SelectedGameTime()
        {
            int hour24 = (_hour % 12) + (_isPm ? 12 : 0);
            
            if (hour24 == 0) hour24 = 24;   // 12 AM → 2400
            if (hour24 == 1) hour24 = 25;   // 1 AM  → 2500
            if (hour24 == 2) hour24 = 26;   // 2 AM  → 2600

            return hour24 * 100 + _minute;
        }

        private void Notify([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
