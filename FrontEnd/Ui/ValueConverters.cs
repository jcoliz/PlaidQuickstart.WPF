// Copyright (C) 2024 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FrontEnd.Ui.ValueConverters;

/// <summary>
/// Generic value converter for converting booleans into other typs
/// </summary>
/// <typeparam name="T">Which type to convert into</typeparam>
public class BooleanConverter<T>(T trueValue, T falseValue) : IValueConverter
{
    public T True
    {
        get; set;
    }
    = trueValue;

    public T False
    {
        get; set;
    }
    = falseValue;

    public virtual object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            true => True!,
            false => False!,
            null => False!,
            _ => True!
        };
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is T tvalue && EqualityComparer<T>.Default.Equals(tvalue, True);
    }
}

public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
{
    public BooleanToVisibilityConverter() :
        base(Visibility.Visible, Visibility.Collapsed)
    {
    }
}

public sealed class BooleanToIntConverter : BooleanConverter<int>
{
    public BooleanToIntConverter() :
        base(1, 0)
    {
    }
}
