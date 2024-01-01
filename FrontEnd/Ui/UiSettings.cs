// Copyright (C) 2024 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace FrontEnd.Ui;

/// <summary>
/// Settings which affect how the UI is presented
/// </summary>
public class UiSettings
{
    public static string Section => "Ui";

    /// <summary>
    /// Where to look for the Link web page
    /// </summary>
    public string? WebAddress { get; set; }
}
