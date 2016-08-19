using System;
using System.Collections.Generic;
using System.ComponentModel;
using Slackbot_Traffic.Libraries;
/// <summary>
/// ConvertNice converts data types and returns either the converted value or a default value Without Producing Exceptions.
/// </summary>
public static class ConvertNice
{
	#region Boolean Conversion

	/// <summary>
	/// Returns true if the numeric value is > 0, else returns false
	/// </summary>
	public static bool ToBoolean(decimal value)
	{
		return (value != 0);
	}

	/// <summary>
	/// Returns true if the numeric value is > 0, else returns false
	/// </summary>
	public static bool ToBoolean(float value)
	{
		return (value != 0);
	}

	/// <summary>
	/// Returns true if the integer is > 0, else returns false
	/// </summary>
	public static bool ToBoolean(int value)
	{
		return (value != 0);
	}

	/// <summary>
	/// returns true if the string contans the text 'True' or a numeric value '1'
	/// </summary>
	public static bool ToBoolean(string value)
	{
		if (string.IsNullOrEmpty(value)) return false;
		return StringHelper.IsTrueString(value);
	}

	/// <summary>
	/// Convert the object to boolean using Convert.ToBoolean(), returns false on exceptions
	/// </summary>
	public static bool ToBoolean(object value)
	{
		if (value == null)
		{
			return false;
		}

		if (Utilities.IsNumeric(value))
		{
			return ToBoolean(ConvertNice.ToDecimal(value));
		}
		else
		{
			return ToBoolean(value.ToString());
		}
	}

	#endregion Boolean Conversion

	#region Integer Conversion

	/// <summary>
	/// Converts the object to an integer if possible, else returns 0
	/// </summary>
	public static int ToInt32(object value)
	{
		return ToInteger(value, 0);
	}

	/// <summary>
	/// Converts the object to an integer if possible, else returns 0
	/// </summary>
	public static int ToInteger(object value)
	{
		return ToInteger(value, 0);
	}

	/// <summary>
	/// converts the object to an integer if possible, else returns the defaultValue
	/// </summary>
	public static int ToInteger(object value, int defaultValue)
	{
		if (value == null)
		{
			return defaultValue;
		}

		int returnValue = 0;
		bool success = false;

		if (!(success = int.TryParse(value.ToString(), out returnValue)))
		{
			//decimal value would have failed integer parse
			decimal f;
			if (success = decimal.TryParse(value.ToString(), out f))
			{
				if ((f <= int.MaxValue) && (f >= int.MinValue))
				{
					//convert 12.04 to 12
					returnValue = (int)f;
				}
				else
				{
					success = false;
				}
			}
		}

		return success ? returnValue : defaultValue;
	}

	#endregion Integer Conversion

	#region Long Conversion

	/// <summary>
	/// Converts the object to an integer if possible, else returns 0
	/// </summary>
	public static long ToInt64(object value)
	{
		return ToLong(value, 0);
	}

	/// <summary>
	/// Converts the object to an integer if possible, else returns 0
	/// </summary>
	public static long ToLong(object value)
	{
		return ToLong(value, 0);
	}

	/// <summary>
	/// converts the object to an integer if possible, else returns the defaultValue
	/// </summary>
	public static long ToLong(object value, long defaultValue)
	{
		if (value == null)
		{
			return defaultValue;
		}

		long returnValue = 0;
		bool success = false;

		if (!(success = long.TryParse(value.ToString(), out returnValue)))
		{
			//decimal value would have failed long parse
			decimal dbl;
			if (success = decimal.TryParse(value.ToString(), out dbl))
			{
				if ((dbl <= long.MaxValue) && (dbl >= long.MinValue))
				{
					//convert 12.04 to 12
					returnValue = (long)dbl;
				}
				else
				{
					success = false;
				}
			}
		}

		return success ? returnValue : defaultValue;
	}

	#endregion Long Conversion

	#region Single Conversion

	/// <summary>
	/// Converts the object to an Single if possible, else returns 0
	/// </summary>
	public static Single ToSingle(object value)
	{
		return ToSingle(value, 0);
	}

	/// <summary>
	/// converts the object to an Single if possible, else returns the defaultValue
	/// </summary>
	public static Single ToSingle(object value, Single defaultValue)
	{
		if (value == null)
			return defaultValue;

		Single returnValue = 0;
		bool success = Single.TryParse(value.ToString(), out returnValue);
		if (success)
			return returnValue;
		else
			return defaultValue;
	}

	#endregion Single Conversion

	#region Decimal Conversion

	/// <summary>
	/// Converts the object to an Single if possible, else returns 0
	/// </summary>
	public static decimal ToDecimal(object value)
	{
		return ToDecimal(value, 0);
	}

	/// <summary>
	/// converts the object to an Single if possible, else returns the defaultValue
	/// </summary>
	public static decimal ToDecimal(object value, decimal defaultValue)
	{
		return ToDecimal(value, defaultValue, false);
	}

	/// <summary>
	/// Converts the object to a double, if possible, also allowing for currency symbols, otherwise returns zero.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	/// <param name="tryCurrency">Whether to allow for currency or not.</param>
	/// <returns>The converted value, or the default.</returns>
	public static decimal ToDecimal(object value, bool tryCurrency)
	{
		return ToDecimal(value, decimal.Zero, tryCurrency);
	}

	/// <summary>
	/// Converts the object to a double, if possible, also allowing for currency symbols, otherwise returns the defaultValue.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	/// <param name="defaultValue">Default to use if conversion fails.</param>
	/// <param name="tryCurrency">Whether to allow for currency or not.</param>
	/// <returns>The converted value, or the default.</returns>
	public static decimal ToDecimal(object value, decimal defaultValue, bool tryCurrency)
	{
		if (value == null) return defaultValue;

		Decimal returnValue;
		if (Decimal.TryParse(value.ToString(), out returnValue)) return returnValue;
		if (tryCurrency && Decimal.TryParse(value.ToString(), System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.CurrentCulture.NumberFormat, out returnValue)) return returnValue;
		return defaultValue;
	}

	#endregion Decimal Conversion

	#region Date conversion

	/// <summary>
	/// Converts the object to an Single if possible, else returns DateTime.MinValue
	/// </summary>
	public static DateTime ToDateTime(object value)
	{
		return ToDateTime(value, DateTime.MinValue);
	}

	/// <summary>
	/// converts the object to DateTime if possible, else returns the defaultValue
	/// </summary>
	public static DateTime ToDateTime(object value, DateTime defaultValue)
	{
		if (value == null)
		{
			return defaultValue;
		}

		string strVal = value.ToString();
		string shortFormat = DateHelper.ShortDateFormat;
		DateTime returnValue;

		// Try parsing with the short date format setting (with and without time), then the normal parse, then try a few standard formats
		//  - The problem is, in the US a "d/M/yy" date won't work with TryParse, even if its 25/2/2011 or something unambiguous
		//  - Annoyingly we have to try all of the year options too
		//  - Short circuiting will make this as efficient as it can be, and mean that the "returnValue" will have the last good result
		bool success = DateTime.TryParseExact(strVal, shortFormat, null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, shortFormat + " h:mm tt", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, shortFormat + " h:mm:ss", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParse(strVal, out returnValue) ||
			DateTime.TryParseExact(strVal, "d/M/yyyy", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "M/d/yyyy", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "d/M/yyyy h:mm tt", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "M/d/yyyy h:mm tt", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "d/M/yyyy h:mm:ss", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "M/d/yyyy h:mm:ss", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "d/M/yy", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "M/d/yy", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "d/M/yy h:mm tt", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "M/d/yy h:mm tt", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "d/M/yy h:mm:ss", null, System.Globalization.DateTimeStyles.None, out returnValue) ||
			DateTime.TryParseExact(strVal, "M/d/yy h:mm:ss", null, System.Globalization.DateTimeStyles.None, out returnValue);
		if (success)
		{
			return returnValue;
		}
		else
		{
			return defaultValue;
		}
	}

	/// <summary>
	/// Converts the object to DateTime if possible, else returns null
	/// </summary>
	/// <param name="value">The object to convert</param>
	/// <returns>The object as a DateTime, or null if conversion wasn't possible</returns>
	public static DateTime? ToNullableDateTime(object value)
	{
		DateTime? returnValue = null;
		DateTime dateValue = DateTime.MinValue;

		if (value != null)
		{
			dateValue = ToDateTime(value, DateTime.MinValue);  // This will convert more formats that DateTime.TryParse

			// Check that the conversion did not return the default value, or if it did, that the date can actually be converted normally to that value
			if ((dateValue != DateTime.MinValue) ||
				((dateValue == DateTime.MinValue) && DateTime.TryParse(value.ToString(), out dateValue)))
			{
				returnValue = dateValue;
			}
		}
		return returnValue;
	}

	#endregion Date conversion

	#region IntArray Methods

	/// <summary>
	/// Converts a comma seperated list of ints in a string to an int array.
	/// </summary>
	/// <param name="input">The input string.</param>
	/// <returns></returns>
	public static int[] ToIntArray(string input)
	{
		List<int> result = new List<int>();
		if (!string.IsNullOrEmpty(input))
		{
			foreach (string s in input.Split(','))
			{
				int i = ConvertNice.ToInteger(s, -4000);
				if (i != -4000) result.Add(i);
			}
		}
		return result.ToArray();
	}

	#endregion IntArray Methods

	#region ToString methods

	/// <summary>
	/// Converts an int array to a comma seperated string
	/// </summary>
	/// <param name="intArray">The int array.</param>
	/// <returns></returns>
	public static string ToString(int[] intArray)
	{
		string result = "";
		if (intArray != null)
		{
			foreach (int i in intArray)
			{
				if (result.Length > 0) result += ",";
				result += i;
			}
		}
		return result;
	}

	/// <summary>
	/// Converts the boolean value to integer string '1' if True and '0' if false
	/// (instead of the default .NET of a 'True' and 'False' string)
	/// </summary>
	public static string ToString(bool value)
	{
		return value ? "1" : "0";
	}

	#endregion ToString methods

	#region ToEnum methods

	/// <summary>
	/// Converts an int value into the corresponding enum value. If the conversion is
	/// unsuccessful it returns the supplied fallback.
	/// </summary>
	/// <typeparam name="T">The enum type</typeparam>
	/// <param name="value">The int value to convert to the enum value</param>
	/// <param name="fallback">The fallback value to use when unsuccessful</param>
	/// <returns>The corresponding enum value, or the supplied fallback</returns>
	public static T ToEnum<T>(object value, T fallback)
	{
		return (T)ToEnum(value, typeof(T), fallback);
	}

	/// <summary>
	/// Converts an int value into the corresponding enum value. If the conversion is
	/// unsuccessful it returns the supplied fallback.
	/// </summary>
	public static object ToEnum(object value, Type enumType, object fallback)
	{
		if (value == null) return fallback;
		// convert an integer as a string to an integer
		int intValue = ConvertNice.ToInteger(value, -5000);
		if (intValue != -5000)
		{
			value = intValue;
		}
		if (value is int && Enum.IsDefined(enumType, value))
		{
			return Enum.ToObject(enumType, (int)value);
		}
		else if (value is string)
		{
			try
			{
				return Enum.Parse(enumType, value as string, true);
			}
			catch
			{
				return fallback;
			}
		}
		else if (value.GetType() == enumType)
		{
			return value;
		}
		else
		{
			return fallback;
		}
	}

	#endregion ToEnum methods

	#region ChangeType Methods

	/// <summary>
	/// Converts the current object to the type of T.
	/// </summary>
	/// <typeparam name="T">The type to convert the object to.</typeparam>
	/// <param name="value">The object to convert.</param>
	/// <returns></returns>
	public static T ChangeType<T>(object value)
	{
		// early exit!
		if (value is T) return (T)value;

		string error;
		return (T)ChangeType(value, typeof(T), out error);
	}

	/// <summary>
	/// Converts the current object to the supplied type.
	/// </summary>
	/// <param name="value">The object to convert.</param>
	/// <param name="conversionType">The type to convert to.</param>
	/// <returns></returns>
	public static object ChangeType(object value, Type conversionType)
	{
		string error;
		return ChangeType(value, conversionType, out error);
	}

	/// <summary>
	/// Changes the type.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="destinationType">Type of the destination.</param>
	/// <param name="defaultValue">The default value.</param>
	/// <returns></returns>
	public static object ChangeType(object value, Type destinationType, object defaultValue)
	{
		string error = "";
		object result = ChangeType(value, destinationType, out error);
		if (error.IsNotWhitespace())
		{
			return defaultValue;
		}
		else
		{
			return result;
		}
	}

	/// <summary>
	/// Changes the type.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="destinationType">Type of the destination.</param>
	/// <param name="error">The error.</param>
	/// <returns></returns>
	public static object ChangeType(object value, Type destinationType, out string error)
	{
		// minor performance improvements for the win!
		Type stringType = typeof(string);

		error = "";
		if (destinationType == null) throw new ArgumentNullException("destinationType", "destinationType must not be null");

		Type valueType = value?.GetType();

		if (destinationType.IsEnum)
		{
			object fallback = new object();
			object result = ConvertNice.ToEnum(value, destinationType, fallback);
			if (result == fallback)
			{
				error = "Could not map value '" + value + "' to a valid value of type " + destinationType.Name + ": Requested value '" + value + "' was not found.";
				return GetDefault(destinationType);
			}
			return result;
		}
		else if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
		{
			// Apparently this is required for nullable types
			if (value == null)
			{
				return GetDefault(destinationType);
			}

			// At this point if the value is DBNull.Value the conversion will error
			// In all of our item bases we set DBNull.Value to null, but we don't do this for GenericDataItems
			// It would be nice to put some code in GenericDataItem.SetValue to set DBNull.Value to
			// null but we use DBNull.Value occasionally to indicate state, so I'm just going to do
			// the conversion manually here to prevent the exception
			if (value == DBNull.Value)
			{
				return GetDefault(destinationType);
			}

			if (valueType == stringType && ((string)value).Length == 0)
			{
				return GetDefault(destinationType);
			}

			object result = ChangeType(value, Nullable.GetUnderlyingType(destinationType), out error);
			if (error.IsNotWhitespace())
			{
				return GetDefault(destinationType);
			}
			return result;
		}
		else if (destinationType.IsValueType && (value == null || value == DBNull.Value))
		{
			// Can't convert null to a value type so we just return the type's default value
			return GetDefault(destinationType);
		}
		else if (destinationType == typeof(int[]) && valueType == stringType)
		{
			return ConvertNice.ToIntArray((string)value);
		}
		else if (destinationType == stringType && value is int[])
		{
			return ConvertNice.ToString((int[])value);
		}
		else if (destinationType == stringType && value is string[])
		{
			return string.Join(",", (string[])value);
		}
		else if (destinationType == typeof(string[]) && valueType == stringType)
		{
			string temp = (string)value;
			if (temp.Length > 0)
			{
				return temp.Split(',');
			}
			else
			{
				//return empty array, instead of array with one empty string
				return new string[0];
			}
		}
		else if (destinationType == typeof(bool))
		{
			return ConvertNice.ToBoolean(value);
		}
		else if (destinationType == typeof(TimeSpan) && value is int)
		{
			// TimeSpans are stored in the DB as seconds, so convert them nicely
			return new TimeSpan(0, 0, (int)value);
		}
		else if (destinationType == typeof(TimeSpan) && valueType == stringType)
		{
			TimeSpan span;
			if (TimeSpan.TryParse((string)value, out span))
			{
				return span;
			}
			else
			{
				error = "Could not convert value '" + value + "' to TimeSpan";
				return TimeSpan.Zero;
			}
		}
		else if (destinationType == typeof(DateTime))
		{
			DateTime defaultDate = DateTime.MinValue.AddMilliseconds(1);
			DateTime dt = ConvertNice.ToDateTime(value, defaultDate);
			if (dt == defaultDate)
			{
				error = "Could not convert value '" + value.ToString() + "' to DateTime";
				return DateTime.MinValue;
			}
			return dt;
		}
		else if (value is Guid && destinationType == stringType)
		{
			return value.ToString();
		}
		else if (destinationType == typeof(Guid) && valueType == stringType)
		{
			return new Guid((string)value);
		}
		else if (destinationType == stringType && value is DateTime)
		{
			return ((DateTime)value).ToString(DateHelper.LongDateFormat);
		}
		else if (destinationType == typeof(byte[]))
		{
			if (value == null) return new byte[0];

			try
			{
				return Convert.FromBase64String(value.ToString());
			}
			catch (Exception ex)
			{
				error = "Could not map value '" + value.ToString().Truncate(100, true) + "' to a valid value of type " + destinationType.Name + ": " + ex.Message;
				return new byte[0];
			}
		}
		else if (destinationType == typeof(string) && !(valueType == stringType) && value != null && !valueType.IsValueType)
		{
			// If it's a not-null reference type being converted to a string lets see if there's a type converter
			TypeConverter tc = TypeDescriptor.GetConverter(valueType);
			// I don't think this can actually return null
			if (tc != null)
			{
				try
				{
					return tc.ConvertFromString(value.ToString());
				}
				catch (Exception ex)
				{
					//ErrorLog.AddDebugLogItem("ConvertNice", "Error converting value '" + value + "' to type " + destinationType.Name + ": " + ErrorLog.ExceptionDescription(ex));
					error = "Error converting value '" + value + "' to type " + destinationType.Name + ": " + ex.Message;
					return value.ToString();
				}
			}
		}
		else
		{
			try
			{
				return Convert.ChangeType(value, destinationType);
			}
			catch (Exception ex)
			{
				//ErrorLog.AddDebugLogItem("ConvertNice", "Error converting value '" + value + "' to type " + destinationType.Name + ": " + ErrorLog.ExceptionDescription(ex));
				error = "Error converting value '" + value + "' to type " + destinationType.Name + ": " + ex.Message;
			}
		}

		return GetDefault(destinationType);
	}

	private static object GetDefault(Type type)
	{
		if (type.IsValueType)
		{
			return Activator.CreateInstance(type);
		}
		return null;
	}

	#endregion ChangeType Methods
}
