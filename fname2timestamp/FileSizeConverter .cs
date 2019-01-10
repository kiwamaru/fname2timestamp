using System.Windows.Data;

namespace BindingTest.Converters
{
  　class FileSizeConverter : IValueConverter
    {
        static readonly string[] Suffix = new string[] { "", "K", "M", "G", "T" };
        static readonly double Unit = 1024;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is long)) throw new ArgumentException("long型じゃないよ。", "value");
            double size = (double)((long)value);

            int i;
            for (i = 0; i < Suffix.Length - 1; i++)
            {
                if (size < Unit) break;
                size /= Unit;
            }

            return string.Format("{0:" + (i == 0 ? "0" : "0.0") + "} {1}B", size, Suffix[i]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}