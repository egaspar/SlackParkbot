using System;
using System.ComponentModel;

namespace Slackbot_Traffic.Libraries
{
	/// <summary>
	/// Provides static methods for dealing with enums.
	/// </summary>
	public static class EnumHelper
	{
		/// <summary>
		/// Gets the description of an enum value from its Description attribute or name.
		/// </summary>
		/// <param name="enumValue">The enum value.</param>
		/// <returns>The corresponding enum description.</returns>
		public static string EnumDescription(Enum enumValue)
		{
			if (enumValue == null) throw new ArgumentNullException("enumValue");

			string desc = "";

			DescriptionAttribute descAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(enumValue.GetType().GetField(enumValue.ToString()), typeof(DescriptionAttribute));
			if (descAttribute != null)
			{
				desc = descAttribute.Description;
			}
			else
			{
				// Just return the formatted enum name
				desc = StringHelper.CamelCaseToProperCaseWithSpace(enumValue.ToString());
			}

			return desc;
		}

		/// <summary>
		/// Standard date formatting options.
		/// </summary>
		public enum DateFormat
		{
			/// <summary>
			/// The format to use for short dates e.g. '31/01/2000'. Used for displaying dates in grids and reports.
			/// </summary>
			Short,

			/// <summary>
			/// The format to use for medium dates e.g. 'Wed 31/01/2000'. Used for date data entry.
			/// </summary>
			Medium,

			/// <summary>
			/// The format to use for long dates e.g. 'Wed, 31 Jan 2000'. Used for displaying dates to the user.
			/// </summary>
			Long
		}
	}

}