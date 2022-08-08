﻿using System.Text.RegularExpressions;

namespace Interchangeable;

/// <summary>
/// Provides instruments to check if input strings are valid to certain predefined RegExes.
/// </summary>
public static class RegexPresetValues
{
    public static Regex OnlyNumeric = new Regex("[^0-9]+");
    public static Regex OnlyNumericWithCommas = new Regex("^[0-9,\\s]*$");
    public static Regex OnlyLettersWithCommas = new Regex("^[a-z,\\s]*$");
    public static Regex OnlyOneOrTwoLimitedToSixWithCommas = new Regex("^[0-1,\\s]{0,6}$");
    public static Regex OnlyIpv4Address = new Regex("^(?=.*[^\\.]$)((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.?){4}$");
}
