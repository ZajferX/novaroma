﻿using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class ObjectToVisibilityConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public ObjectToVisibilityConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
