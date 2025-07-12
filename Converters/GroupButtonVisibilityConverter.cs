using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

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
                return Visibility.Collapsed; // �e�L�X�g������ꍇ�̓v���[�X�z���_�[���B��
            }
            return Visibility.Visible; // �e�L�X�g���Ȃ��ꍇ�̓v���[�X�z���_�[��\��
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
                return "??"; // �t�H���_�A�C�R��
            }
            return "??"; // �t�@�C��/�v���W�F�N�g�A�C�R��
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}