#region History and Copyright

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

#endregion

using System.Text;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace CodeFormatter
{
	/// <summary>
	/// Generates color-coded HTML 4.01 from C# source code.
	/// </summary>
	public class CSharpFormat : CLikeFormat
	{
		public CSharpFormat()
		{
			Parser
				.DefineAs(
					Comments,
					PreprocessorDirectives,
					StringLiterals,
					Keywords_,
					TypeDeclarations,
					Declarations,
					ConstructorInvocations,
					Attributes,
					GenericTypes,
					StaticInvocations);
		}

		/// <summary>
		/// The list of C# keywords.
		/// </summary>
		protected override string Keywords
		{
			get
			{
				return (NoResString)"abstract as base bool break byte case catch char "
				+ (NoResString)"checked class const continue decimal default delegate do double else "
				+ (NoResString)"enum event explicit extern false finally fixed float for foreach get goto "      // These are C# key words in a string
				+ (NoResString)"if implicit in int interface internal is lock long namespace new null "
				+ (NoResString)"object operator out override partial params private protected public readonly "
				+ (NoResString)"ref return sbyte sealed set short sizeof stackalloc static string struct "
				+ (NoResString)"switch this throw true try typeof uint ulong unchecked unsafe ushort "
				+ (NoResString)"using value virtual void volatile where while yield "
				+ (NoResString)"var from where select orderby descending into"; // C# 3.0
			}
		}

		/// <summary>
		/// The list of C# preprocessors.
		/// </summary>
		protected override string Preprocessors
		{
			get
			{
				return (NoResString)"#if #else #elif #endif #define #undef #warning "
					+ (NoResString)"#error #line #region #endregion #pragma"; // These are C# key words in a string
			}
		}

		#region Parsing strategies
		/// <summary>
		/// Formatting the declaration of types
		/// </summary>
		protected virtual ParsingStrategy TypeDeclarations
		{
			get
			{
				return new ParsingStrategy(
					(NoResString)"typeexpression",  // Constant string used in code.
					(NoResString)@"(?<=class +|interface +|enum +|struct +)(?<type>[A-Z]\w*)( *: *(?<namespace>([A-Z]\w*\.)*)(?<type>[A-Z]\w*)( *, *(?<namespace>([A-Z]\w*\.)*)(?<type>[A-Z]\w*))*)?", // It is a regular expression.
					delegate(Match match)
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendFormat((NoResString)"<font color=\"green\">{0}</font>", match.Groups["type"].Captures[0].Value); // Html syntax for code block highlighting.

						if (match.Groups["type"].Captures.Count > 1)
						{
							sb.Append(" : ");
							for (int i = 1; i < match.Groups["type"].Captures.Count; i++)
							{
								if (i > 1)
								{
									sb.Append(" , ");
								}
								Capture capture = match.Groups["type"].Captures[i];
								sb.AppendFormat((NoResString)"{0}<font color=\"green\">{1}</font>",  // Html syntax for code block highlighting.
									match.Groups["namespace"].Captures[i - 1].Value,
									capture.Value);
							}
						}
						return sb.ToString();
					});
			}
		}

		/// <summary>
		/// Formatting the declaration of type variables
		/// </summary>
		protected virtual ParsingStrategy Declarations
		{
			get
			{
				return new ParsingStrategy(
					(NoResString)"declaration", // Constant string used in code.
					(NoResString)@"(?<=public +|private +|protected +|internal +|static +|^|[^\.\w])(?<namespace>([A-Z]\w*\.)*)(?<type>[A-Z]\w*)(?= +\w|&lt;)", // It is a regular expression.
					delegate(Match match)
					{
						return string.Format((NoResString)"{0}<font color=\"green\">{1}</font>",  // Html syntax for code block highlighting.
							match.Groups["namespace"].Value,
							match.Groups["type"].Value);
					});
			}
		}

		/// <summary>
		/// Formatting of object initiation
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is a regular expression., Constant string used in code., Html syntax for code block highlighting.")]
		protected virtual ParsingStrategy ConstructorInvocations
		{
			get
			{
				const string prefix = @"(?<=new( |&nbsp;)+)";
				const string nameSpace = @"(?<namespace>([A-Z]\w*\.)*)";
				const string type = @"(?<type>[A-Z]\w*)";
				const string postfix = @"(?=((<|&lt;).*(>|&gt;))?(\(| *{))";

				const string constrinv = prefix + nameSpace + type + postfix;
				return new ParsingStrategy((NoResString)"constrinv", constrinv,
					delegate(Match match)
					{
						return string.Format((NoResString)"{0}<font color=\"green\">{1}</font>",
							match.Groups["namespace"].Value,
							match.Groups["type"].Value);
					});
			}
		}

		/// <summary>
		/// Formatting invocation of static methods and properties
		/// </summary>
		protected virtual ParsingStrategy StaticInvocations
		{
			get
			{
				return new ParsingStrategy(
					(NoResString)"staticinv", // Constant string used in code.
					(NoResString)@"(?<=(?<nskwrd>namespace +)|^|[^\.\w])(?<namespace>([A-Z]\w*\.)*)(?<type>[A-Z]\w*)(?=\.)", // It is a regular expression.
					delegate(Match match)
					{
						if (match.Groups["nskwrd"].Success)
						{
							return match.Value;
						}

						return string.Format((NoResString)"{0}<font color=\"green\">{1}</font>",  // Html syntax for code block highlighting.
							match.Groups["namespace"].Value,
							match.Groups["type"].Value);
					});
			}
		}

		/// <summary>
		/// Formatting of type attributes
		/// </summary>
		protected virtual ParsingStrategy Attributes
		{
			get
			{
				return new ParsingStrategy(
					(NoResString)"attribute", // Constant string used in code.
					(NoResString)@"(?<=\[[^A-Z\]]*)(?<type>[A-Z]\w*)(?<params>(\([^\)]+\))?)((?<infix>[^,\]]*,[^A-Z\]]*)(?<type>[A-Z]\w*)(?<params>(\([^\)]+\))?))*(?=[^\]]*\])", // It is a regular expression.
					delegate(Match match)
					{
						StringBuilder sb = new StringBuilder();
						for (int i = 0; i < match.Groups["type"].Captures.Count; i++)
						{
							Capture capture = match.Groups["type"].Captures[i];
							if (i > 0)
							{
								sb.Append(match.Groups["infix"].Value);
							}

							sb.AppendFormat((NoResString)"<font color=\"green\">{0}</font>", capture.Value); // Html syntax for code block highlighting.
							string param = match.Groups["params"].Captures[i].Value;
							Regex regex = new Regex(StringRegEx, RegexOptions.Compiled | RegexOptions.Singleline);
							param = regex.Replace(param, StringLiterals.Evaluate);
							sb.Append(param);
						}

						return sb.ToString();
					});
			}
		}

		/// <summary>
		/// Formatting type parameters of generics
		/// </summary>
		protected virtual ParsingStrategy GenericTypes
		{
			get
			{
				return new ParsingStrategy(
					(NoResString)"generictype",  // Constant string used in code.
					(NoResString)@"(?<=&lt;(</?(br /|p)>| |&nbsp;|\\r|\\n|\\t|\w+,)*)(?<namespace>([A-Z]\w*\.)*)(?<type>[A-Z]\w*)(?=.*&gt;)", // It is a regular expression.
					delegate(Match match)
					{
						return string.Format((NoResString)"{0}<font color=\"green\">{1}</font>",  // Html syntax for code block highlighting.
							match.Groups["namespace"].Value,
							match.Groups["type"].Value);
					});
			}
		}
		#endregion
	}
}
