using System;
using System.Windows.Data;

namespace fname2timestamp
{
    public class MyBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = System.Convert.ToBoolean(value);
            if (b)
            {
                return "Yes";
            }

            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyDateTimeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime d = new DateTime();
            DateTime dt = System.Convert.ToDateTime(value);
            if (dt != d)
            {
                return value;
            }

            return "--------------";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}