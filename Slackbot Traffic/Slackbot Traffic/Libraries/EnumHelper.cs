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
	}
}