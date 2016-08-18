using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Slackbot_Traffic.Libraries
{
	/// <summary>
	/// Contains helper routines for working with strings
	/// </summary>
	public static class StringHelper
	{
		#region Misc Utilities

		/// <summary>
		/// Returns where a string contains a word that should be considered "true"
		/// </summary>
		public static bool IsTrueString(string value)
		{
			if (value == null) return false;

			return (value == "-1" || value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("y", StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Returns where a string contains a word that should be considered "false"
		/// </summary>
		public static bool IsFalseString(string value)
		{
			if (value == null) return false;

			return (value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase) || value.Equals("no", StringComparison.OrdinalIgnoreCase) || value.Equals("n", StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Converts a number to words, so 4 becomes "four"
		/// </summary>
		/// <param name="numberVal">The number val.</param>
		/// <returns></returns>
		public static string NumberToWords(float numberVal)
		{
			return NumberToWords(Convert.ToDecimal(numberVal));
		}

		/// <summary>
		/// Converts a number to words, so 4 becomes "four"
		/// </summary>
		/// <param name="numberVal">The number val.</param>
		/// <returns></returns>
		public static string NumberToWords(double numberVal)
		{
			return NumberToWords(Convert.ToDecimal(numberVal));
		}

		/// <summary>
		/// Converts a number to words, so 4 becomes "four"
		/// </summary>
		/// <param name="numberVal">The number val.</param>
		/// <returns></returns>
		public static string NumberToWords(decimal numberVal)
		{
			return NumberToWords(numberVal, true);
		}

		/// <summary>
		/// Converts a number to words, so 4 becomes "four"
		/// </summary>
		/// <param name="numberVal">The number val.</param>
		/// <param name="trimTrailingZerosAfterDecimalPoint">if set to <c>true</c> [trim trailing zeros after decimal point].</param>
		/// <returns></returns>
		public static string NumberToWords(decimal numberVal, bool trimTrailingZerosAfterDecimalPoint)
		{
			string result = NumberToWords(Convert.ToInt32(Math.Floor(numberVal)));
			string num = numberVal.ToString();
			if (num.Contains('.'))
			{
				num = num.Substring(num.IndexOf('.') + 1);
				if (trimTrailingZerosAfterDecimalPoint)
				{
					num = num.TrimEnd('0');
				}
				if (num.Length > 0)
				{
					string[] ones = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
					result += " point";
					foreach (char digit in num)
					{
						result += " " + ones[(digit - '0')];
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Converts a number to words, so 4 becomes "four"
		/// </summary>
		/// <param name="numberVal">The number val.</param>
		/// <returns></returns>
		public static string NumberToWords(int numberVal)
		{
			string[] powers = new string[] { "thousand", "million", "billion" };
			string[] ones = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
			string[] tens = new string[] { "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
			StringBuilder wordValue = new StringBuilder();

			if (numberVal == 0)
			{
				return "Zero";
			}
			if (numberVal < 0)
			{
				wordValue.Append("negative ");
				numberVal = -numberVal;
			}

			long[] partStack = new long[] { 0, 0, 0, 0 };
			int partNdx = 0;

			while (numberVal > 0)
			{
				partStack[partNdx++] = numberVal % 1000;
				numberVal /= 1000;
			}

			for (int i = 3; i >= 0; i--)
			{
				long part = partStack[i];

				if (part >= 100)
				{
					wordValue.Append(ones[part / 100 - 1] + " hundred ");
					part %= 100;
				}

				if (part >= 20)
				{
					if ((part % 10) != 0)
					{
						wordValue.Append(tens[part / 10 - 2] + "-" + ones[part % 10 - 1] + " ");
					}
					else
					{
						wordValue.Append(tens[part / 10 - 2] + " ");
					}
				}
				else if (part > 0)
				{
					wordValue.Append(ones[part - 1] + " ");
				}

				if (part != 0 && i > 0)
				{
					wordValue.Append(powers[i - 1] + " ");
				}
			}

			wordValue[0] = char.ToUpperInvariant(wordValue[0]);
			return wordValue.ToString().Trim();
		}

		/// <summary>
		/// Converts a variable name "camelCase" => "Camel Case"
		/// </summary>
		/// <param name="variableName"></param>
		/// <returns></returns>
		public static string CamelCaseToProperCaseWithSpace(string variableName)
		{
			StringBuilder result = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(variableName) && variableName.Length > 0)
			{
				char[] charArray = variableName.Replace("_", " ").Trim().ToCharArray();
				result.Append(charArray[0].ToString().ToUpper());
				for (int i = 1; i < charArray.Length; i++)
				{
					// Don't bother adding a space if we already have a space
					if (!char.IsWhiteSpace(charArray[i - 1]))
					{
						// Add a space if:
						// * The character is uppercase, and either preceeded or followed by a lowercase character and isn't a 'D' preceeded by an 'I'
						// * The character is uppercase R, and preceded bt r and followed by e
						// * The character is a number, not preceeded by another number
						// Looks more complex than it is
						// Probably should be refactored to a state machine or something thats a bit easier to follow
						if (
								(char.IsUpper(charArray[i])
									&& (char.IsLower(charArray[i - 1]) || (((i + 1) != charArray.Length) && (char.IsLower(charArray[i + 1]))))
									&& !(charArray[i] == 'D' && charArray[i - 1] == 'I')

									&& !(charArray[i] == 'R'
										&& (i - 4 >= 0)
										&& (i + 2 <= charArray.Length)
										&& (charArray[i + 1] == 'e' && charArray[i + 2] == 'z')
										&& (charArray[i - 1] == 'r' && charArray[i - 2] == 'a' && charArray[i - 3] == 't' && (charArray[i - 4] == 'S' || charArray[i - 4] == 's')))

								)
								|| (char.IsNumber(charArray[i]) && !char.IsNumber(charArray[i - 1]))
							)
						{
							result.Append(" ");
						}
					}

					result.Append(charArray[i]);
				}
			}

			return result.ToString();
		}

		
		

		/// <summary>
		/// Determines whether [is enum column] [the specified column name]
		/// </summary>
		/// <param name="columnName">name of the column</param>
		/// <returns></returns>
		public static bool IsEnumColumn(string columnName)
		{
			return columnName.EndsWith("Enum", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Changes a singular string to plural e.g. "Monkey" => "Monkeys".
		/// </summary>
		/// <param name="text">The text to PLURALIZE!</param>
		/// <returns>A plural string.</returns>
		public static string Pluralize(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			string result = text;

			if (result.Equals("process", StringComparison.OrdinalIgnoreCase))
			{
				return result + "es";
			}
			else if (result.Equals("sheep", StringComparison.OrdinalIgnoreCase))
			{
				return "sheep";
			}
			else if (result.Equals("day", StringComparison.OrdinalIgnoreCase))
			{
				return "days";
			}
			else if (result.Equals("leaf", StringComparison.OrdinalIgnoreCase))
			{
				return "leaves";
			}
			else if (result.Equals("thief", StringComparison.OrdinalIgnoreCase))
			{
				return "thieves";
			}
			else if (result.Equals("potato", StringComparison.OrdinalIgnoreCase))
			{
				return "potatoes";
			}
			else if (result.EndsWith("alumni", StringComparison.OrdinalIgnoreCase))
			{
				return result;
			}
			else if (result.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
				!result.EndsWith("ey", StringComparison.OrdinalIgnoreCase))
			{
				// Don't pluralize "by"
				if (result.Length > 2 && !result.EndsWith(" by", StringComparison.OrdinalIgnoreCase))
				{
					result = result.Truncate(result.Length - 1) + "ies";
				}
			}
			else if (result.EndsWith("us", StringComparison.OrdinalIgnoreCase))
			{
				// http://en.wikipedia.org/wiki/Plural_form_of_words_ending_in_-us
				result += "es";
			}
			else if (result.EndsWith("x", StringComparison.OrdinalIgnoreCase))
			{
				result += "es";
			}
			else if (!result.EndsWith("s", StringComparison.OrdinalIgnoreCase))
			{
				result += "s";
			}

			return result;
		}

		/// <summary>
		/// Changes a singular string to plural if number is not 1.
		/// </summary>
		/// <param name="text">The text to PLURALIZE!</param>
		/// <param name="number">The number.</param>
		/// <returns>A plural string.</returns>
		public static string Pluralize(string text, int number)
		{
			if (number != 1)
			{
				return Pluralize(text);
			}
			else
			{
				return text;
			}
		}

		/// <summary>
		/// Adds the indefinite article to a string.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>The indefinite article, then the text.</returns>
		public static string AddIndefiniteArticle(string text)
		{
			string result = "";

			if (text.StartsWith("a", StringComparison.OrdinalIgnoreCase) ||
				text.StartsWith("e", StringComparison.OrdinalIgnoreCase) ||
				text.StartsWith("i", StringComparison.OrdinalIgnoreCase) ||
				text.StartsWith("o", StringComparison.OrdinalIgnoreCase) ||
				text.StartsWith("u", StringComparison.OrdinalIgnoreCase))
			{
				result = "an ";
			}
			else
			{
				result = "a ";
			}

			result += text;

			return result;
		}

		/// <summary>
		/// Converts a number to its ordinal notation (2 == 2nd, 5 == 5th, 11 == 11th)
		/// </summary>
		public static string OrdinalizeNumber(int number)
		{
			string num = number.ToString();
			string appendage;
			if (num.EndsWith("1", StringComparison.OrdinalIgnoreCase) && !num.EndsWith("11", StringComparison.OrdinalIgnoreCase))
			{
				appendage = "st";
			}
			else if (num.EndsWith("2", StringComparison.OrdinalIgnoreCase) && !num.EndsWith("12", StringComparison.OrdinalIgnoreCase))
			{
				appendage = "nd";
			}
			else if (num.EndsWith("3", StringComparison.OrdinalIgnoreCase) && !num.EndsWith("13", StringComparison.OrdinalIgnoreCase))
			{
				appendage = "rd";
			}
			else
			{
				appendage = "th";
			}

			return num + appendage;
		}

		#endregion Misc Utilities

		#region ToString Methods

		/// <summary>
		/// Converts a timestamp array, as returned by Sql Server, into a string that is comparable to the values
		/// shown in Query Analyzer
		/// </summary>
		/// <param name="timestamp">The timestamp to convert</param>
		/// <returns>A string of the form 0x00000000005FD</returns>
		public static string TimestampToString(byte[] timestamp)
		{
			if (timestamp == null)
			{
				return "null";
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				foreach (byte s in timestamp)
				{
					sb.Append(s.ToString("X"));
				}
				return "0x" + sb.ToString().PadLeft(16, '0');
			}
		}

		/// <summary>
		/// Rounds a decimal to 4 decimal places, then strips any zeroes off the end until the number reaches 2 decimal places. The decimal is then returned as a string.
		/// </summary>
		/// <param name="value">The decimal to round.</param>
		/// <returns>The rounded value, as a string.</returns>
		public static string LooselyRoundDecimalToString(decimal value)
		{
			// Defaults are based on currency fields, which "should" be 2 decimal places, but can be up to 4
			const int DefaultMinPlaces = 2;
			const int DefaultMaxPlaces = 4;
			return LooselyRoundDecimalToString(value, DefaultMinPlaces, DefaultMaxPlaces);
		}

		/// <summary>
		/// Rounds a decimal to <paramref name="maxPlaces"/> number of decimal places, then strips any zeroes off the end until the number reaches <paramref name="minPlaces"/> number of decimal places. The decimal is then returned as a string.
		/// </summary>
		/// <param name="value">The decimal to round.</param>
		/// <param name="minPlaces">The minimum number of decimal places to use.</param>
		/// <param name="maxPlaces">The maximum number of decimal places to use.</param>
		/// <returns>The rounded value, as a string.</returns>
		public static string LooselyRoundDecimalToString(decimal value, int minPlaces, int maxPlaces)
		{
			if (maxPlaces < minPlaces) throw new ArgumentException("Maximum decimal places cannot be less than the minimum.", "maxPlaces");

			if (minPlaces < 0)
			{
				minPlaces = 0; // Can't have less than zero decimal places
			}

			if (maxPlaces < 0)
			{
				maxPlaces = 0; // Can't have less than zero decimal places
			}

			string formatString = "0";
			if (maxPlaces > 0)
			{
				formatString += ".";
			}

			for (int i = 0; i < maxPlaces; i++)
			{
				formatString += (i < minPlaces) ? "0" : "#"; // 0: Force digit to show, #: Only show significant digits
			}

			return Math.Round(value, maxPlaces).ToString(formatString); // Perform proper mathmatical rounding before formatting the number
		}

		/// <summary>
		/// Converts a cosby number to a string describing that cosby
		/// </summary>
		/// <param name="cosbyNumber">The cosby to convert</param>
		/// <returns>The name of the cosby, if it exists.</returns>
		public static string CosbyNumberToString(int cosbyNumber)
		{
			switch (cosbyNumber)
			{
				case 1:
				{
					return "Entertainment Cosby";
				}
				case 2:
				{
					return "House Keeping Cosby";
				}
				case 3:
				{
					return "Moral Support Cosby";
				}
				case 4:
				{
					return "Tattle Tale Cosby";
				}
				case 5:
				{
					return "Dancing Cosby";
				}
				case 6:
				{
					return "Bath Tub Cosby";
				}
				case 7:
				{
					return "Useless Cosby";
				}
				case 8:
				{
					// His curiosity often gets the Cosbys into trouble, such as when he finds and reveals
					// the cloner plans to Evil Cosby, who then murders him as seen at the end of episode 3
					return "Curiosity Cosby";
				}
				case 9:
				{
					return "Buck Naked Cosby";
				}
				case 10:
				{
					// Super-intelligence and data analysis
					return "Data Analysis Cosby";
				}
				case 20:
				{
					// The ability to create amazing feats of carpentry, as he built the additional barracks
					// and probably most of the buildings in the training facillity
					return "Carpentry Cosby";
				}
				case 30:
				{
					// His super power would appear to be the production and use of weapons, particularly guns
					return "Weapons Cosby";
				}
				case 40:
				{
					// His super power is being a good country music player
					return "Country Cosby";
				}
				case 47:
				{
					// Taking out the Garbage. This Cosby is believed to be deceased. He attempts suicide
					// in episode 2 because he feels useless. He ties a noose around his neck while standing
					// on a chair. After being talked out of it by Preacher Cosby, he accidentally falls
					// off the chair and hangs himself. He is never seen again
					return "Mood Swing Cosby";
				}
				case 50:
				{
					// A parody of R2-D2, he is a small, apparently cyborg Cosby who follows Data Analysis
					// Cosby around and communicates by a series of "blips" and "bloo"s
					return "BC-D2";
				}
				case 60:
				{
					// 	Her most obvious super power is the fact that she is female (with a well proportioned
					// body) but she has also exhibited psychic powers (sensing trouble in the compound)
					return "Cosbyette";
				}
				case 61:
				{
					// Provides spiritual comfort, for example talking one out of suicide
					return "Preacher Cosby";
				}
				case 70:
				{
					// One of the members of Cosby Team Triosby, he has the power to turn into toothpaste
					// and seep through small gaps, or make things trip
					return "Toothpaste Cosby";
				}
				case 80:
				{
					// One of the members of Cosby Team Triosby, he can absorb TV and radio waves and turn
					// them into harmful electric blasts
					return "Frequency Cosby";
				}
				case 85:
				{
					return "April Fools Cosby";
				}
				case 90:
				{
					// One of the members of Cosby Team Triosby, he can fly and hover like a humming bird,
					// and also create small explosives in his stomach which he can then defecate on
					// land-based targets
					return "Hummingbird Cosby";
				}
				case 100:
				{
					// His super powers are unknown, although being "evil" would appear to be one of them.
					// He plots to use the cloning machine himself to make his own army of Cosbys
					return "Evil Cosby";
				}
				default:
				{
					return "Cosby #" + cosbyNumber.ToString();
				}
			}
		}

		#endregion ToString Methods

		#region Replace Field Values Methods

		/// <summary>
		/// Determines whether the specified text is RTF.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		public static bool IsRtf(string text)
		{
			if (text == null) return false;

			return text.StartsWith(@"{\rtf", StringComparison.OrdinalIgnoreCase);
		}
		
		#endregion Replace Field Values Methods

		#region CSV Helpers

		/// <summary>
		/// Fixes the CSV string.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns></returns>
		public static string FixCSVString(string input)
		{
			return FixCSVString(input, false);
		}

		/// <summary>
		/// Fixes the CSV string.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <param name="alwaysQualify">if set to <c>true</c> [always qualify].</param>
		/// <returns></returns>
		public static string FixCSVString(string input, bool alwaysQualify)
		{
			return FixCSVString(input, null, alwaysQualify);
		}

		/// <summary>
		/// Fixes the CSV string.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <param name="textQualifier">The text qualifier.</param>
		/// <param name="alwaysQualify">if set to <c>true</c> [always qualify].</param>
		/// <returns></returns>
		public static string FixCSVString(string input, string textQualifier, bool alwaysQualify)
		{
			if (string.IsNullOrEmpty(textQualifier))
			{
				textQualifier = "\"";
			}

			string endQualifier = "\"";
			if (textQualifier == "[")
			{
				endQualifier = "]";
			}

			string value = input;

			if (value.Contains(textQualifier) || value.Contains("\"") || value.Contains("\n") || value.Contains("\r") || value.Contains(","))
			{
				if (textQualifier == "[" && value.StartsWith("[", StringComparison.OrdinalIgnoreCase) && value.EndsWith("]", StringComparison.OrdinalIgnoreCase))
				{
					return value;
				}

				value = value.Replace(textQualifier, textQualifier + textQualifier);
				value = value.Replace("\n", "\\n");
				value = value.Replace("\r", "\\r");

				if (textQualifier == "[")
				{
					value = textQualifier + value + endQualifier;
				}
				else
				{
					value = textQualifier + value + textQualifier;
				}
			}
			else if (alwaysQualify && !(value.StartsWith(textQualifier, StringComparison.OrdinalIgnoreCase) && value.EndsWith(textQualifier, StringComparison.OrdinalIgnoreCase)))
			{
				if (textQualifier == "[")
				{
					value = textQualifier + value + endQualifier;
				}
				else
				{
					value = textQualifier + value + textQualifier;
				}
			}
			return value;
		}

		#endregion CSV Helpers

		#region Googlizing Search Helpers

		/// <summary>
		/// Gets the search words.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="removeCommonWords">if set to <c>true</c> [remove common words].</param>
		/// <returns></returns>
		public static string[] GetSearchWords(string input, bool removeCommonWords)
		{
			string searchString = input;

			// special case - if there is only one word, just return it
			if (!searchString.Contains(' '))
			{
				return new string[] { searchString };
			}

			// remove common words
			if (removeCommonWords)
			{
				Regex commonWords = new Regex(@"\b(?:a(?:[nst]|re|nd)?|b[ey]|f(?:or|rom)|i[nst]?|o[fnr]|t(?:o|hat|he|his)|w(?:as|h(?:at|en|ere|ich|o)|i(?:th|ll)))\b");
				searchString = commonWords.Replace(searchString, "");
			}

			// remove punctuation
			searchString = searchString.Replace(",", "").Replace(";", "").Replace(".", "");

			// split up in to words
			Regex splitWords = new Regex(@"(?<=(?:^|\s|,)"")[^""]*?(?="")|(?<=\s|^)(?!"")[\w\W]+?(?=\s|$)");
			var words = from match in splitWords.Matches(searchString).Cast<Match>()
						where match.Value.IsNotWhitespace()
						select match.Value.Trim();

			return words.ToArray();
		}

		#endregion Googlizing Search Helpers

		#region Xml Helper Methods

		/// <summary>
		/// Whether a given character is allowed by XML 1.0.
		/// </summary>
		// I know this is in StringHelper, and its an int, but really it would be a char passed in
		public static bool IsLegalXmlChar(int character)
		{
			// from: http://seattlesoftware.wordpress.com/2008/09/11/hexadecimal-value-0-is-an-invalid-character/
			return
			(
				   character == 0x9                                  /* == '\t' == 9   */
				|| character == 0xA                                  /* == '\n' == 10  */
				|| character == 0xD                                  /* == '\r' == 13  */
				|| (character >= 0x20 && character <= 0xD7FF)
				|| (character >= 0xE000 && character <= 0xFFFD)
				|| (character >= 0x10000 && character <= 0x10FFFF)
			);
		}

		/// <summary>
		/// Toes the legal XML.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string ToLegalXml(string value)
		{
			Contract.Requires(value != null);
			Contract.Ensures(Contract.Result<string>() != null);

			StringBuilder sanitized = null;
			int i = 0;
			foreach (char character in value)
			{
				if (StringHelper.IsLegalXmlChar(character))
				{
					if (sanitized != null)
					{
						sanitized.Append(character);
					}
				}
				else
				{
					if (sanitized == null)
					{
						sanitized = new StringBuilder(value.Substring(0, i));
					}
					sanitized.Append('&');
					sanitized.Append('#');
					sanitized.Append((int)character);
					sanitized.Append(';');
				}
				i++;
			}
			if (sanitized == null)
			{
				return value;
			}
			else
			{
				return sanitized.ToString();
			}
		}

		/// <summary>
		/// Unescapes illegal XML characters that have been encoded using <see cref="ToLegalXml"/>.
		/// </summary>
		/// <param name="value">The string with encodings to be unescaped.</param>
		/// <returns>A string with illegal XML entities unescaped.</returns>
		public static string UnescapeIllegalXmlEntities(string value)
		{
			Contract.Requires(value != null);
			Contract.Ensures(Contract.Result<string>() != null);

			value = Regex.Replace(value, @"&#(\d+);", m =>
			{
				char character = (char)int.Parse(m.Groups[1].Value);
				return character.ToString();
			});

			return value;
		}

		/// <summary>
		/// Fixes the xml namespace
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToLegalXmlName(string value)
		{
			var name = value.Replace(':', '_');
			return System.Xml.XmlConvert.EncodeName(name);
		}

		#endregion Xml Helper Methods

		#region HTML Helpers

		/// <summary>
		/// Determines whether the specified input is HTML.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>
		///   <c>true</c> if the specified input is HTML; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsHtml(string input)
		{
			if (input == null) return false;

			return (input.IndexOf("<HTML>", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<B>", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<STRONG>", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<P>", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<IMG", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<P ", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<P />", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<BR>", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<BR />", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("<A HREF", StringComparison.OrdinalIgnoreCase) != -1 ||
					input.IndexOf("</A>", StringComparison.OrdinalIgnoreCase) != -1);
		}

		#endregion HTML Helpers

		#region Base 64 Helper(s?)

		// ****************************************************
		// * From: http://stackoverflow.com/a/23955827
		// ****************************************************

		// Characters that are used in base64 strings.
		private static readonly char[] Base64Chars = new[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/' };

		/// <summary>
		/// Extension method to test whether the value is a base64 string
		/// </summary>
		/// <param name="value">Value to test</param>
		/// <returns>Boolean value, true if the string is base64, otherwise false</returns>
		public static Boolean IsBase64String(string value)
		{
			// The quickest test. If the value is null or is equal to 0 it is not base64
			// Base64 string's length is always divisible by four, i.e. 8, 16, 20 etc.
			// If it is not you can return false. Quite effective
			if (value == null || value.Length == 0 || value.Length % 4 != 0)
			{
				return false;
			}

			// Further, if it meets the above criterias, then test for spaces.
			// If it contains spaces, it is not base64
			if (value.Contains(' ') || value.Contains('\t') || value.Contains('\r') || value.Contains('\n'))
			{
				return false;
			}

			// 98% of all non base64 values are invalidated by this time.
			var index = value.Length - 1;

			// if there is padding step back
			if (value[index] == '=')
			{
				index--;
			}

			// if there are two padding chars step back a second time
			if (value[index] == '=')
			{
				index--;
			}

			// Now traverse over characters
			// You should note that I'm not creating any copy of the existing strings,
			// assuming that they may be quite large
			for (var i = 0; i <= index; i++)
			{
				// If any of the character is not from the allowed list
				if (!Base64Chars.Contains(value[i]))
				{
					// return false
					return false;
				}
			}

			// If we got here, then the value is a valid base64 string
			return true;
		}

		#endregion Base 64 Helper(s?)

		/// <summary>
		/// All invalid path chars
		/// </summary>
		public static readonly char[] AllInvalidPathChars = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray();

		/// <summary>
		/// Gets the valid filename.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns></returns>
		public static string GetValidFilename(string fileName)
		{
			foreach (char character in AllInvalidPathChars)
			{
				fileName = fileName.Replace(character, '_');
			}
			return fileName;
		}

		/// <summary>
		/// Truncate a string instance if it is greater than a given length.
		/// </summary>
		/// <param name="input">String to Evaluate.</param>
		/// <param name="maxLength">Maximum Length of string to Truncate to.</param>
		/// <returns></returns>
		[Pure]
		[System.ComponentModel.Description("Truncate a string instance if it is greater than a given length.")]
		public static string Truncate(this string input, int maxLength)
		{
			return input.Truncate(maxLength, false);
		}

		/// <summary>
		/// Truncate a string instance if it is greater than a given length.
		/// </summary>
		/// <param name="input">String to Evaluate.</param>
		/// <param name="maxLength">Maximum Length of string to Truncate to.</param>
		/// <param name="addEllipses">Whether to add ellipses to the string if its truncating. This ends up truncating to maxLength - 3</param>
		/// <returns></returns>
		[System.ComponentModel.Description("Truncate a string instance if it is greater than a given length.")]
		public static string Truncate(this string input, int maxLength, bool addEllipses)
		{
			if (input == null || maxLength == 0)
			{
				return "";
			}
			else if (maxLength > 0 && input.Length > maxLength)
			{
				if (addEllipses && maxLength > 3)
				{
					return input.Substring(0, maxLength - 3) + "...";
				}
				else
				{
					return input.Substring(0, maxLength);
				}
			}
			else
			{
				return input;
			}
		}

		/// <summary>
		/// Determines whether a string is not whitespace, or null. Ie, if it has something in it other than spaces. NOTE: This method calls Trim()
		/// </summary>
		/// <param name="input">The input string.</param>
		public static bool IsNotWhitespace(this string input)
		{
			return !string.IsNullOrWhiteSpace(input);
		}
	}
}