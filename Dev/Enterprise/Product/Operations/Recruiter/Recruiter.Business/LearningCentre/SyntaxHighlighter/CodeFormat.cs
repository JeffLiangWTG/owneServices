#region Modified on 24/06/2009 Samuel Wang [samuel.wang@cargowise.com]
/*
 * Modified the highlighter format for FlexCel report
 */
#endregion
#region Copyright © 2008 Rickard Nilsson [rickard@rickardnilsson.net]
/*
 * This software is an altered version of the original and is provied 'as-is'.
 */
#endregion
#region Copyright © 2001-2003 Jean-Claude Manoli [jc@manoli.net]
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author(s) be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *   1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 * 
 *   2. Altered source versions must be plainly marked as such, and must not
 *      be misrepresented as being the original software.
 * 
 *   3. This notice may not be removed or altered from any source distribution.
 */
#endregion

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace CodeFormatter
{
	/// <summary>
	/// Provides a base class for formatting most programming languages.
	/// </summary>
	public abstract class CodeFormat : SourceFormat
	{
		protected CodeFormat()
		{
			Parser
				.DefineAs(
					Comments,
					PreprocessorDirectives,
					StringLiterals,
					Keywords_);
		}

		/// <summary>
		/// Must be overridden to provide a list of keywords defined in 
		/// each language.
		/// </summary>
		/// <remarks>
		/// Keywords must be separated with spaces.
		/// </remarks>
		protected abstract string Keywords
		{
			get;
		}

		/// <summary>
		/// Can be overridden to provide a list of preprocessors defined in 
		/// each language.
		/// </summary>
		/// <remarks>
		/// Preprocessors must be separated with spaces.
		/// </remarks>
		protected virtual string Preprocessors
		{
			get { return ""; }
		}

		/// <summary>
		/// Must be overridden to provide a regular expression string
		/// to match strings literals. 
		/// </summary>
		protected abstract string StringRegEx
		{
			get;
		}

		/// <summary>
		/// Must be overridden to provide a regular expression string
		/// to match comments. 
		/// </summary>
		protected abstract string CommentRegEx
		{
			get;
		}

		/// <summary>
		/// Determines if the language is case sensitive.
		/// </summary>
		/// <value><b>true</b> if the language is case sensitive, <b>false</b> 
		/// otherwise. The default is true.</value>
		/// <remarks>
		/// A case-insensitive language formatter must override this 
		/// property to return false.
		/// </remarks>
		public virtual bool CaseSensitive
		{
			get { return true; }
		}

		protected override string MatchEval(Match match)
		{
			return string.Empty;
		}

		#region Parser elements
		protected virtual ParsingStrategy Comments
		{
			get
			{
				return new ParsingStrategy(
					(NoResString)"comment", CommentRegEx, // Constant string used in code.
					delegate(Match match)
					{
						using (StringReader reader = new StringReader(match.ToString()))
						{
							string line;
							StringBuilder sb = new StringBuilder();
							while ((line = reader.ReadLine()) != null)
							{
								if (sb.Length > 0)
								{
									sb.Append("\n");
								}
								sb.Append((NoResString)"<font color=\"green\">"); // Html syntax for code block highlighting.
								sb.Append(line);
								sb.Append((NoResString)"</font>"); // Html syntax for code block highlighting.
							}
							return sb.ToString();
						}
					});
			}
		}

		protected virtual ParsingStrategy StringLiterals
		{
			get
			{
				return new ParsingStrategy(
					(NoResString)"stringliteral", StringRegEx, // Constant string used in code.
					delegate(Match match)
					{
						return (NoResString)"<font color=\"#FF00FF\">" + match + (NoResString)"</font>"; // Html syntax for code block highlighting.
					});
			}
		}

		protected virtual ParsingStrategy PreprocessorDirectives
		{
			get
			{
				return new ParsingStrategy(
					(NoResString)"preprocessordirective", // Constant string used in code.
					Preprocessors,
					delegate(Match match)
					{
						return match.Value;
					});
			}
		}

		protected virtual ParsingStrategy Keywords_
		{
			get
			{
				Regex r;
				r = new Regex(@"\w+|-\w+|#\w+|@@\w+|#(?:\\(?:s|w)(?:\*|\+)?\w+)+|@\\w\*+");
				string regKeyword = r.Replace(Keywords, @"(?<=^|\W)$0(?=\W)");
				r = new Regex(@" +");
				regKeyword = r.Replace(regKeyword, @"|");

				return new ParsingStrategy(
					(NoResString)"keyword", regKeyword, // Constant string used in code.
					delegate(Match match)
					{
						return (NoResString)"<font color=\"#0000FF\">" + match + (NoResString)"</font>"; // Html syntax for code block highlighting.
					});
			}
		}
		#endregion
	}
}

