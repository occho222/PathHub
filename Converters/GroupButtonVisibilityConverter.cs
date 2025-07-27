using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ModernLauncher.Models;

namespace ModernLauncher.Converters
{
    public class GroupButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string id && id == "all")
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AllGroupVisibilityConverter : IValueConverter
    {
        public static readonly AllGroupVisibilityConverter Instance = new AllGroupVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ItemGroupオブジェクトまたはstring IDを処理
            if (value is ItemGroup group)
            {
                return group.Id != "all";
            }
            else if (value is string id)
            {
                return id != "all";
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GroupLaunchButtonVisibilityConverter : IValueConverter
    {
        public static readonly GroupLaunchButtonVisibilityConverter Instance = new GroupLaunchButtonVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // グループが選択されており、「すべて」グループではない場合に表示
            if (value is ItemGroup group && group.Id != "all")
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AllGroupLaunchButtonVisibilityConverter : IValueConverter
    {
        public static readonly AllGroupLaunchButtonVisibilityConverter Instance = new AllGroupLaunchButtonVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 「すべて」グループが選択されている場合に表示
            if (value is ItemGroup group && group.Id == "all")
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToBoolConverter : IValueConverter
    {
        public static readonly NullToBoolConverter Instance = new NullToBoolConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public static readonly StringToVisibilityConverter Instance = new StringToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return Visibility.Collapsed; // テキストがある場合はプレースホルダーを隠す
            }
            return Visibility.Visible; // テキストがない場合はプレースホルダーを表示
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToIconConverter : IValueConverter
    {
        public static readonly BoolToIconConverter Instance = new BoolToIconConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFolder && isFolder)
            {
                return "📁"; // フォルダアイコン
            }
            return "📄"; // ファイル/プロジェクトアイコン
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CategoryColorConverter : IValueConverter
    {
        public static readonly CategoryColorConverter Instance = new CategoryColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category)
            {
                return CategoryColorSettings.Instance.GetBrushForCategory(category);
            }
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CategoryColorIndicatorConverter : IValueConverter
    {
        public static readonly CategoryColorIndicatorConverter Instance = new CategoryColorIndicatorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category)
            {
                var brush = CategoryColorSettings.Instance.GetBrushForCategory(category);
                return brush;
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public static readonly DateTimeToStringConverter Instance = new DateTimeToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // DateTime.MinValueの場合は未アクセスと表示
                if (dateTime == DateTime.MinValue)
                {
                    return "-";
                }
                return dateTime.ToString("yyyy/MM/dd HH:mm");
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}