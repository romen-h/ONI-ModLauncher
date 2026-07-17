using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ONIModLauncher.Core;
using ONIModLauncher.Core.Interfaces;

namespace ONIModLauncher.App.Common
{
	public class InverseBooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
			{
				return b ? Visibility.Collapsed : Visibility.Visible;
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ColorToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is System.Drawing.Color color)
			{
				return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is SolidColorBrush brush)
			{
				return brush.Color;
			}

			return null;
		}
	}
	
	public class NegateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value)
			{
				case int intValue:
					return -intValue;
				case double doubleValue:
					return -doubleValue;
				default:
					return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value)
			{
				case int intValue:
					return -intValue;
				case double doubleValue:
					return -doubleValue;
				default:
					return null;
			}
		}
	}
	
	public class PresetColorToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is PresetColor color)
			{
				return color switch
				{
					PresetColor.Red => App.Current.Resources["RedBrush"],
					PresetColor.Yellow => App.Current.Resources["YellowBrush"],
					PresetColor.Orange => App.Current.Resources["OrangeBrush"],
					PresetColor.Blue => App.Current.Resources["BlueBrush"],
					_ => App.Current.Resources["GrayBrush"],
				};
			}
			if (value is ModType modType)
			{
				
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

}
