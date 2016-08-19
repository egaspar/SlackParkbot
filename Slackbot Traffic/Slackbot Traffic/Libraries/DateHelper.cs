using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using Slackbot_Traffic.Libraries;
using static Slackbot_Traffic.Libraries.EnumHelper;

/// <summary>
///  Contains routines for working with dates
/// </summary>
public static class DateHelper
{
	#region Constants

	private const string SqlDateFormat = "yyyy-MMM-dd";

	/// <summary>
	/// Format to use for dates that is unambiguous, so SQL server can safely convert from a string to a date
	/// </summary>
	public const string SqlDateTimeFormat = "yyyy-MMM-dd HH:mm:ss";

	/// <summary>
	/// The minimum value for a small datetime
	/// </summary>
	public static readonly DateTime SmallDateTimeMin = new DateTime(1900, 1, 1);

	/// <summary>
	/// The maximum value for a small datetime
	/// </summary>
	public static readonly DateTime SmallDateTimeMax = new DateTime(2079, 6, 6, 23, 59, 0);

	#endregion Constants

	#region Properties

	/// <summary>
	/// Gets or sets the format to use for short dates e.g. '31/01/2000'. Used for displaying dates
	/// in grids and reports.
	/// </summary>
	public static string ShortDateFormat
	{
		get { return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern; }
	}

	/// <summary>
	/// Gets or sets the javascript format to use for short dates e.g. '31/01/2000'. Used for displaying dates
	/// in grids and reports.
	/// </summary>
	public static string JavaScriptShortDateFormat
	{
		get { return ToJavaScriptDateFormat(ShortDateFormat); }
	}

	/// <summary>
	/// Gets the default format to use for short dates.
	/// </summary>
	public static string ShortDateFormatDefault
	{
		get
		{
			return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		}
	}

	/// <summary>
	/// Gets or sets the format to use for medium dates e.g. 'Wed 31/01/2000'. Used for date data entry.
	/// </summary>
	public static string MediumDateFormat
	{
		get
		{
			return "ddd " + System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		}
	}

	/// <summary>
	/// Gets the default format to use for medium dates.
	/// </summary>
	public static string MediumDateFormatDefault
	{
		get
		{
			return "ddd " + System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		}
	}

	/// <summary>
	/// Gets or sets the format to use for long dates e.g. 'Wed, 31 Jan 2000'. Used for displaying
	/// dates to the user.
	/// </summary>
	public static string LongDateFormat
	{
		get { return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern.Replace("dddd", "ddd").Replace("MMMM", "MMM"); }
	}

	/// <summary>
	/// Gets or sets the format to use for long dates in javascript e.g. 'Wed, 31 Jan 2000'. Used for displaying
	/// dates to the user.
	/// </summary>
	public static string JavaScriptLongDateFormat
	{
		get { return ToJavaScriptDateFormat(LongDateFormat); }
	}

	/// <summary>
	/// Gets the default format to use for long dates.
	/// </summary>
	public static string LongDateFormatDefault
	{
		get
		{
			return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern.Replace("dddd", "ddd").Replace("MMMM", "MMM");
		}
	}

	/// <summary>
	/// Gets or sets the format to use for times.
	/// </summary>
	public static bool Use24HourTime
	{
		get { return true; }
	}

	/// <summary>
	/// Gets the default format to use for times.
	/// </summary>
	public static bool Use24HourTimeDefault
	{
		get { return true; }
	}

	/// <summary>
	/// Gets the date that is used as a minimum or empty value in the database.
	/// </summary>
	public static DateTime MinDatabaseValue
	{
		get { return new DateTime(1900, 1, 1); }
	}

	/// <summary>
	/// Gets the date that is used as a maximum or empty value in the database.
	/// </summary>
	public static DateTime MaxDatabaseValue
	{
		get { return new DateTime(2100, 1, 1); }
	}

	#endregion Properties

	#region Formatting

	/// <summary>
	/// Returns a date format string using the default format for data entry (Medium).
	/// </summary>
	[System.ComponentModel.Description("Returns a date format string using the default format for data entry (Medium).")]
	public static string FormatString()
	{
		return FormatString(DateFormat.Medium, false);
	}

	/// <summary>
	/// Returns a date format string using one of the standard formats.
	/// </summary>
	[System.ComponentModel.Description("Returns a date format string using one of the standard formats.")]
	public static string FormatString(DateFormat format, bool includeTime)
	{
		return FormatString(format, includeTime, false);
	}

	/// <summary>
	/// Returns a date format string using one of the standard formats.
	/// </summary>
	[System.ComponentModel.Description("Returns a date format string using one of the standard formats.")]
	public static string FormatString(DateFormat format, bool includeTime, bool showSeconds)
	{
		string result = "";

		switch (format)
		{
			case DateFormat.Short:
			{
				result = DateHelper.ShortDateFormat;
				break;
			}
			case DateFormat.Medium:
			{
				result = DateHelper.MediumDateFormat;
				break;
			}
			case DateFormat.Long:
			{
				result = DateHelper.LongDateFormat;
				break;
			}
		}

		if (includeTime)
		{
			result += " " + TimeFormatString(showSeconds);
		}

		return result;
	}

	/// <summary>
	/// Timeformat string for javascript
	/// </summary>
	/// <param name="showSeconds">if set to <c>true</c> [show seconds].</param>
	/// <returns></returns>
	public static string JavaScriptTimeFormatString(bool showSeconds)
	{
		return TimeFormatString(showSeconds).Replace("tt", "A");
	}

	private static string ToJavaScriptDateFormat(string format)
	{
		return format.Replace("yyyy", "YYYY")
					.Replace("yy", "YY")
					.Replace("d", "D")      //We want to replace d -> D and dd -> DD
					.Replace("DDD", "ddd")  //but not dddd or ddd
					.Replace("DDDD", "dddd");
	}

	/// <summary>
	/// Gets the Time format string.
	/// </summary>
	public static string TimeFormatString()
	{
		return TimeFormatString(false);
	}

	/// <summary>
	/// Gets the Time format string.
	/// </summary>
	public static string TimeFormatString(bool showSeconds)
	{
		string result = "";
		if (Use24HourTime)
		{
			result += "HH";
		}
		else
		{
			result += "h";
		}
		result += ":mm";
		if (showSeconds)
		{
			result += ":ss";
		}
		if (!Use24HourTime)
		{
			result += " tt";
		}
		return result;
	}

	/// <summary>
	/// Gets a user readable description of the format
	/// </summary>
	public static string FormatDescription(string format)
	{
		return format.Replace("tt", "AM/PM");
	}

	/// <summary>
	/// Converts a UNIX timestamp to a standard DateTime object
	/// </summary>
	public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
	{
		// Unix timestamp is seconds past epoch
		DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		return dtDateTime.AddSeconds(unixTimeStamp);
	}

	#endregion Formatting

	#region ToString Methods

	/// <summary>
	/// Formats a date using the default format for data entry (Medium).
	/// </summary>
	[System.ComponentModel.Description("Formats a date using the default format for data entry (Medium).")]
	public static string ToString(DateTime date)
	{
		return date.ToString(FormatString());
	}

	/// <summary>
	/// Formats a date using one of the standard formats.
	/// </summary>
	[System.ComponentModel.Description("Formats date using one of the standard formats.")]
	public static string ToString(DateTime date, DateFormat format, bool includeTime)
	{
		return date.ToString(FormatString(format, includeTime));
	}

	/// <summary>
	/// Formats a date using one of the standard formats.
	/// </summary>
	[System.ComponentModel.Description("Formats date using one of the standard formats.")]
	public static string ToString(DateTime date, DateFormat format, bool includeTime, bool showSeconds)
	{
		return date.ToString(FormatString(format, includeTime, showSeconds));
	}

	/// <summary>
	/// Formats a date using the default format for data entry (Medium).
	/// </summary>
	/// <returns>A formatted date string if the object is not null and a date; otherwise a string containing two spaces.</returns>
	[System.ComponentModel.Description("Formats a date using one of the standard formats.")]
	public static string ToString(object date)
	{
		return ToString(date, DateFormat.Medium, false, false);
	}

	/// <summary>
	/// Formats a date using one of the standard formats.
	/// </summary>
	/// <returns>A formatted date string if the object is not null and a date; otherwise a string containing two spaces.</returns>
	[System.ComponentModel.Description("Formats a date using the StarNet standard formats.")]
	public static string ToString(object date, DateFormat format, bool includeTime)
	{
		return ToString(date, format, includeTime, false);
	}

	/// <summary>
	/// Formats a date using one of the standard formats.
	/// </summary>
	/// <returns>A formatted date string if the object is not null and a date; otherwise a string containing two spaces.</returns>
	[System.ComponentModel.Description("Formats a date using the StarNet standard formats.")]
	public static string ToString(object date, DateFormat format, bool includeTime, bool showSeconds)
	{
		string result = "  ";
		if (date is DateTime || (date != null && IsDate(date)))
		{
			result = Convert.ToDateTime(date).ToString(FormatString(format, includeTime, showSeconds));
		}
		return result;
	}

	/// <summary>
	/// Formats a timespan in plain english
	/// </summary>
	/// <param name="timeSpan"></param>
	/// <returns></returns>
	[System.ComponentModel.Description("Formats a timespan in plain english")]
	public static string ToString(TimeSpan? timeSpan)
	{
		if (timeSpan == null)
		{
			return string.Empty;
		}

		TimeSpan span = timeSpan.Value;

		string formatted = string.Format("{0}{1}{2}{3}",
			span.Duration().Days > 0 ? string.Format("{0:0} days, ", span.Days) : string.Empty,
			span.Duration().Hours > 0 ? string.Format("{0:0} hours, ", span.Hours) : string.Empty,
			span.Duration().Minutes > 0 ? string.Format("{0:0} minutes, ", span.Minutes) : string.Empty,
			span.Duration().Seconds > 0 ? string.Format("{0:0} seconds", span.Seconds) : string.Empty);

		if (formatted.EndsWith(", ", StringComparison.OrdinalIgnoreCase)) formatted = formatted.Substring(0, formatted.Length - 2);

		if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

		return formatted;
	}

	/// <summary>
	/// This extension method formats the date to a string that can be parsed in JavaScript to a date object.
	/// NOTE: Use Date.parse() in JS to convert to a date object, as this is cross browser compatible.
	/// </summary>
	/// <param name="dateTime"></param>
	/// <returns>String representation of the date time that can be converted in JS to a Date object with Date.parse()</returns>
	public static string ToJavaScriptParsableString(this DateTime dateTime)
	{
		return dateTime.ToString("s", CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Returns the time of <paramref name="dateTime"/> if the date is today, or the date part otherwise.
	/// </summary>
	/// <param name="dateTime"></param>
	/// <returns></returns>
	public static string ToDateOrTodayTime(this DateTime dateTime)
	{
		if (dateTime == null)
		{
			return "";
		}
		else if (dateTime.Date.Equals(DateHelper.Today))
		{
			return dateTime.ToString(DateHelper.TimeFormatString());
		}
		else
		{
			return DateHelper.ToString(dateTime, DateFormat.Short, false);
		}
	}

	#endregion ToString Methods

	#region ToRangeString Methods

	/// <summary>
	/// Formats two dates into a date range string using one of the standard formats, return a string of spaces if the object is not a date or is null.
	/// </summary>
	[System.ComponentModel.Description("Formats two dates into a date range string using one of the standard formats.")]
	public static string ToRangeString(object date1, object date2)
	{
		return ToRangeString(date1, date2, DateFormat.Short, false);
	}

	/// <summary>
	/// Formats two dates into a date range string using one of the standard formats, return a string of spaces if the object is not a date or is null.
	/// </summary>
	[System.ComponentModel.Description("Formats two dates into a date range string using one of the standard formats.")]
	public static string ToRangeString(object date1, object date2, bool includeNightsString)
	{
		string nightsString = "";
		if (includeNightsString && IsDate(date1) && IsDate(date2))
		{
			nightsString = " (" + GetNightsString(ConvertNice.ToDateTime(date1), ConvertNice.ToDateTime(date2), false) + ")";
		}

		return ToRangeString(date1, date2, DateFormat.Short, false) + nightsString;
	}

	/// <summary>
	/// Formats two dates into a date range string using the short date format.
	/// </summary>
	[System.ComponentModel.Description("Formats two dates into a date range string using the short date format.")]
	public static string ToRangeString(DateTime date1, DateTime date2)
	{
		return ToRangeString(date1, date2, DateFormat.Short, false);
	}

	/// <summary>
	/// Formats two dates into a date range string using the short date format.
	/// </summary>
	[System.ComponentModel.Description("Formats two dates into a date range string using the short date format.")]
	public static string ToRangeString(DateTime date1, DateTime date2, bool includeNightsString)
	{
		string nightsString = "";
		if (includeNightsString)
		{
			nightsString = " (" + GetNightsString(date1, date2, false) + ")";
		}

		return ToRangeString(date1, date2, DateFormat.Short, false) + nightsString;
	}

	/// <summary>
	/// Formats two dates into a date range string using one of the standard formats.
	/// </summary>
	/// <returns>A formatted date string if the object is not null and a date; otherwise a string containing a dash.</returns>
	[System.ComponentModel.Description("Formats two dates into a date range string using one of the standard formats")]
	public static string ToRangeString(object date1, object date2, DateFormat format, bool includeTime)
	{
		string result;
		if (IsDate(date1) && IsDate(date2))
		{
			result = ToRangeString(Convert.ToDateTime(date1), Convert.ToDateTime(date2), format, includeTime);
		}
		else if (IsDate(date1) && !IsDate(date2))
		{
			result = ToString(Convert.ToDateTime(date1), format, includeTime) + " - ";
		}
		else if (!IsDate(date1) && IsDate(date2))
		{
			result = " - " + ToString(Convert.ToDateTime(date2), format, includeTime);
		}
		else
		{
			result = " - ";
		}
		return result;
	}

	/// <summary>
	/// Formats two dates into a date range string using one of the standard formats.
	/// </summary>
	[System.ComponentModel.Description("Formats two dates into a date range string using one of the standard formats.")]
	public static string ToRangeString(DateTime date1, DateTime date2, DateFormat format, bool includeTime)
	{
		string dateFormat = FormatString(format, includeTime);

		if (includeTime && date1.Date.Equals(date2.Date))
		{
			// Only show the time range if both dates are the same
			return string.Format("{0} {1} - {2}", date1.ToString(FormatString(format, false)), date1.ToString(TimeFormatString()), date2.ToString(TimeFormatString()));
		}
		else if (!includeTime && format == DateFormat.Short && date1.Year == date2.Year)
		{
			// Get rid of the year part of the first date if it's the same as the second and we're not showing times
			return string.Format("{0} - {1}", date1.ToString(dateFormat.Replace("/yyyy", "")), date2.ToString(dateFormat));
		}
		else
		{
			return string.Format("{0} - {1}", date1.ToString(dateFormat), date2.ToString(dateFormat));
		}
	}

	#endregion ToRangeString Methods

	#region Other Methods

	/// <summary>
	/// Returns criteria to check if <paramref name="checkDate"/> is between (or equal to) <paramref name="fromDate"/> and <paramref name="toDate"/>, or if
	/// those dates are null.
	/// </summary>
	/// <param name="checkDate">The date to check, this must be a DateTime object, a DateTime DBColumn object, or a special field.</param>
	/// <param name="fromDate">The column to check as being null or before the <paramref name="checkDate"/>.</param>
	/// <param name="toDate">The column to check as being null or after the <paramref name="checkDate"/>.</param>
	/// <returns>Criteria containing the logic to check if the <paramref name="checkDate"/> is between the <paramref name="fromDate"/> and
	/// <paramref name="toDate"/>.</returns>
	//public static SqlCriteriaList GetBetweenNullableDatesCriteria(object checkDate, DBColumn<DateTime?> fromDate, DBColumn<DateTime?> toDate)
	//{
	//	if (!(checkDate is DateTime || checkDate is DBColumn<DateTime> || checkDate is DBColumn<DateTime?> || checkDate is SpecialField))
	//	{
	//		throw new ArgumentException("Value must be a DateTime or datetime DBColumn object.", "checkDate");
	//	}

	//	SqlCriteriaList crit = new SqlCriteriaList();
	//	crit.Add(new SqlCriteriaList(BooleanOperator.Or,
	//		new SqlCriteria(fromDate, SimpleOperator.Equals, null),
	//		new SqlCriteria(fromDate, SimpleOperator.LessThanOrEqualTo, checkDate)
	//		));

	//	crit.Add(new SqlCriteriaList(BooleanOperator.Or,
	//		new SqlCriteria(toDate, SimpleOperator.Equals, null),
	//		new SqlCriteria(toDate, SimpleOperator.GreaterThanOrEqualTo, checkDate)
	//		));

	//	return crit;
	//}

	/// <summary>
	/// Checks if dates are backwards, and swaps them if necessary.
	/// </summary>
	/// <param name="start">The intended earlier date.</param>
	/// <param name="end">The intended later date.</param>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
	public static void CheckDateOrder(ref DateTime start, ref DateTime end)
	{
		if (end < start)
		{
			DateTime temp = start;
			start = end;
			end = temp;
		}
	}

	/// <summary>
	/// Get the number of new days there are between 2 dates (i.e. how many times you pass midnight).
	/// </summary>
	/// <remarks>
	/// This is different than counting the number of days there are between dates, as it handles time as well:
	///		01/01/2015 08:00 - 01/01/2015 17:00 = 0 (0 whole days)
	///		01/01/2015 08:00 - 02/01/2015 08:00 = 1 (1 whole day)
	///		01/01/2015 17:00 - 02/01/2015 08:00 = 1 (0 whole days - this is the case we solve)
	///		01/01/2015 08:00 - 02/01/2015 17:00 = 1 (1 whole day)
	/// </remarks>
	/// <param name="start">The start date</param>
	/// <param name="end">The end date</param>
	/// <returns>The number of nights between the dates.</returns>
	public static int GetNightDifference(DateTime start, DateTime end)
	{
		return (end.Date - start.Date).Days;
	}

	/// <summary>
	/// Get a string describing the number of nights between the two supplied dates
	/// </summary>
	/// <param name="start">The start date</param>
	/// <param name="end">The end date</param>
	/// <param name="includeTime">Whether to include time as part of the description</param>
	/// <returns></returns>
	public static string GetNightsString(DateTime start, DateTime end, bool includeTime)
	{
		string nightsString;

		// if the date picked has added 1 millisond to indicate time has been entered, we want to remove it so the calc is correct
		if (includeTime && start.TimeOfDay.TotalMilliseconds == 1.0)
		{
			start = start.AddMilliseconds(-1);
		}

		if (includeTime)
		{
			TimeSpan span = end - start;

			string nights = "";
			string comma = "";
			if (span.Days != 0)
			{
				nights += comma + span.Days + " days";
				comma = ", ";
			}
			if (span.Hours != 0)
			{
				nights += comma + span.Hours + " hrs";
				comma = ", ";
			}
			if (span.Minutes != 0)
			{
				nights += comma + span.Minutes + " mins";
				comma = ", ";
			}
			nightsString = nights;
		}
		else
		{
			int nights = GetNightDifference(start, end);
			nightsString = string.Format("{0} {1}", nights, StringHelper.Pluralize("night", nights));
		}

		return nightsString;
	}

	/// <summary>
	/// Checks whether the supplied string is a valid date.
	/// </summary>
	/// <param name="dt">The string to check.</param>
	/// <returns>True if the supplied string is numeric, false otherwise.</returns>
	[System.ComponentModel.Description("Checks whether the supplied string is a valid date.")]
	public static bool IsDate(object dt)
	{
		if (dt == null)
		{
			return false;
		}

		DateTime res;
		return System.DateTime.TryParse(dt.ToString(), out res);
	}

	/// <summary>
	/// Formats a date using the Sql Server date format, including the time component e.g. '2000-JAN-31 13:00:00'.
	/// </summary>
	/// <param name="date">The date to format.</param>
	/// <returns>string in the format: yyyy-MMM-dd HH:mm:ss</returns>
	public static string ToSqlDate(DateTime date)
	{
		return ToSqlDate(date, true);
	}

	/// <summary>
	/// Formats a date using the Sql Server date format, optionally including the time component
	/// e.g. '2000-JAN-31' or '2000-JAN-31 13:00:00'.
	/// </summary>
	/// <returns>String formatted in SQL Server format</returns>
	public static string ToSqlDate(DateTime date, bool includeTime)
	{
		string result = "";

		if (includeTime)
		{
			result = "'" + date.ToString(DateHelper.SqlDateTimeFormat) + "'";
		}
		else
		{
			result = "'" + date.ToString(DateHelper.SqlDateFormat) + "'";
		}

		return result;
	}

	/// <summary>
	/// Formats a date so that it can be sorted e.g. '2000/01/31 13:00:00'. Note that the time portion
	/// will always be in 24 hour format.
	/// </summary>
	/// <param name="date">The date to format.</param>
	/// <returns>A formatted date string.</returns>
	public static string ToSortingDate(DateTime date)
	{
		return date.ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.SortableDateTimePattern);
	}

	/// <summary>
	/// Converts an object containing an integer of minutes since midnight (Minute Of Day) into a DateTime
	/// object with the time component
	/// </summary>
	/// <returns>DateTime object with todays date and time specified by minutes parameter</returns>
	public static DateTime ConvertMinuteOfDayToTime(object minutes)
	{
		return ConvertMinuteOfDayToTime(Convert.ToInt32(minutes));
	}

	/// <summary>
	/// Converts an integer represents minutes since midnight (Minute Of Day) into a DateTime
	/// object with the time component
	/// </summary>
	/// <returns>DateTime object with todays date and time specified by minutes parameter</returns>
	public static DateTime ConvertMinuteOfDayToTime(int minutes)
	{
		return DateHelper.Today.AddMinutes(minutes);
	}

	/// <summary>
	/// Converts an integer representing a number of seconds into a string.
	/// </summary>
	/// <param name="seconds">The duration in seconds.</param>
	/// <returns></returns>
	public static string ToDurationString(int seconds)
	{
		Contract.Requires(seconds != int.MinValue);

		TimeSpan span = TimeSpan.FromSeconds(Math.Abs(seconds));
		string durationString;

		// this is a bit untidy... but C# formatting can't specify min number of digits and precision in one command
		if (span.TotalHours < 24)
		{
			durationString = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)span.TotalHours, span.Minutes, span.Seconds);
		}
		else
		{
			durationString = string.Format("{0:F0}:{1:D2}:{2:D2}", span.TotalHours, span.Minutes, span.Seconds);
		}

		if (seconds < 0)
		{
			durationString = "-" + durationString;
		}

		return durationString;
	}

	/// <summary>
	/// Converts a TimeSpan to a nice duration string. eg. '1hr 30mins 1sec 5ms' or '2hrs 4min'
	/// </summary>
	public static string ToNiceDurationString(TimeSpan timeSpan)
	{
		System.Collections.Generic.List<string> elements = new System.Collections.Generic.List<string>();

		if (timeSpan.Hours > 0)
		{
			elements.Add(string.Concat(timeSpan.Hours, (timeSpan.Hours > 1 ? "hrs" : "hr")));
		}
		if (timeSpan.Minutes > 0)
		{
			elements.Add(string.Concat(timeSpan.Minutes, (timeSpan.Minutes > 1 ? "mins" : "min")));
		}
		if (timeSpan.Seconds > 0)
		{
			elements.Add(string.Concat(timeSpan.Seconds, (timeSpan.Seconds > 1 ? "secs" : "sec")));
		}
		if (timeSpan.Milliseconds > 0)
		{
			elements.Add(string.Concat(timeSpan.Milliseconds, "ms"));
		}

		return string.Join(" ", elements);
	}

	/// <summary>
	/// Determines whether the current instant in time is within a date range.
	/// </summary>
	/// <param name="start">The start date.</param>
	/// <param name="end">The end date.</param>
	/// <param name="compareTimes">If set to <c>true</c> times should be compared too.</param>
	/// <returns><c>true</c> if the current instant is within the specified range; otherwise <c>false</c>.</returns>
	public static bool IsActive(DateTime? start, DateTime? end, bool compareTimes)
	{
		DateTime instant = compareTimes ? DateHelper.Now : DateHelper.Today;

		if ((start == null || start <= instant) && (end == null || end >= instant))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Calculate if 2 date ranges overlap.
	/// </summary>
	/// <remarks>
	/// "The dates are exclusive, as anywhere where the actual start and end date matters a time is also included, and the likelyhood of overlapping to the second is almost non-existant." - Chris Osgood famous last words 20/08/2014
	/// </remarks>
	/// <param name="start1">The start date of the first date range.</param>
	/// <param name="end1">The end date of the first date range.</param>
	/// <param name="start2">The start date of the second date range.</param>
	/// <param name="end2">The end date of the second date range.</param>
	/// <returns>True if the dates overlap at any point, exclusive of start and end dates.</returns>
	public static bool DateRangesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
	{
		if (end1 < start1) throw new ArgumentException("End date must occur before start date", "end1");
		if (end2 < start2) throw new ArgumentException("End date must occur before start date", "end2");

		// Following logic is written to work EXACTLY like the DateRangeOperator.InsideRange in SqlCrtieria
		return !(end1 <= start2 || end2 <= start1);
	}

	/// <summary>
	/// Gets the next occurrence of a day given a date. This is inclusive of the current day.
	/// </summary>
	/// <param name="date">The date to base the next occurrence on.</param>
	/// <param name="day">The day of the week to match.</param>
	/// <param name="daysInAWeek">The number of days in 1 week.</param>
	/// <returns>The date of the next DayOfWeek occurrence.</returns>
	public static DateTime GetNextOccurrenceOfDay(DateTime date, DayOfWeek day, int daysInAWeek)
	{
		int difference = ((int)day - (int)date.DayOfWeek + daysInAWeek) % daysInAWeek;
		return date.AddDays(difference);
	}

	/// <summary>
	/// Gets the previous occurrence of a day given a date. This is inclusive of the current day.
	/// </summary>
	/// <param name="date">The date to base the previous occurence on.</param>
	/// <param name="day">The day of the week to match.</param>
	/// <param name="daysInAWeek">The number of days in 1 week.</param>
	/// <returns>The date of previous DayOfWeek occurrence.</returns>
	public static DateTime GetPreviousOccurrenceOfDay(DateTime date, DayOfWeek day, int daysInAWeek)
	{
		int difference = ((int)day - (int)date.DayOfWeek - daysInAWeek) % daysInAWeek;
		return date.AddDays(difference);
	}

	#endregion Other Methods

	/// <summary>
	/// Gets the current date and time as it should be represented in the users timezone
	/// </summary>
	/// <value>The now.</value>
	public static DateTime Now
	{
		get
		{
			//try
			//{
			//	if (StarNet.Library.DB.DataAccessHelper.Current.StoreDatesAsOffsets)
			//	{
			//		return ProcessNull.GetDateTime(DateTimeOffset.Now);
			//	}
			//	else
			//	{
			//		return DateTime.Now;
			//	}
			//}
			//catch
			//{
				return DateTime.Now;
			//}
		}
	}

	/// <summary>
	/// Gets the current date at midnight as it should be represented in the users timezone
	/// </summary>
	/// <value>The today.</value>
	public static DateTime Today
	{
		get
		{
			return DateHelper.Now.Date;
		}
	}

	/// <summary>
	/// Maxes the specified date1.
	/// </summary>
	/// <param name="date1">The date1.</param>
	/// <param name="date2">The date2.</param>
	/// <returns></returns>
	public static DateTime Max(DateTime date1, DateTime date2)
	{
		if (date1 > date2)
		{
			return date1;
		}
		else
		{
			return date2;
		}
	}

	/// <summary>
	/// Mins the specified date1.
	/// </summary>
	/// <param name="date1">The date1.</param>
	/// <param name="date2">The date2.</param>
	/// <returns></returns>
	public static DateTime Min(DateTime date1, DateTime date2)
	{
		if (date1 < date2)
		{
			return date1;
		}
		else
		{
			return date2;
		}
	}
}