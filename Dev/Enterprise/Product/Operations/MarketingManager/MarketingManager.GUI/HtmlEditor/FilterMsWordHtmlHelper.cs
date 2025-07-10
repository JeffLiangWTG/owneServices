using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Enterprise.MarketingManager.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "WTG2006:Do not pass the Compiled option into static methods on Regex.", Justification = "<Pending>")]
	public static class FilterMsWordHtmlHelper
	{
		const string ParagraphMsoNormalClassSelector = "p.MsoNormal"; //normal paragraph style

		public static string GetFilteredOrRawHtmlContentFromUpload(string rawHtml)
		{
			return IsMsWordContentInRawHtml(rawHtml) ? GetFilteredHtmlContent(rawHtml) : rawHtml;
		}

		/// <summary>
		/// Takes the raw HTML text and consider it to be the MS word 
		/// copied junky html content and filters it accordingly.
		/// </summary>
		/// <param name="rawHtml">
		/// Raw HTML text that may be MS HTML
		/// </param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html tags")]
		static string GetFilteredHtmlContent(string rawHtml)
		{
			// Ordered reqular expressions execution list
			var orderedRegexList = new List<Regex>();

			// Replacement map
			var junkMsoPatternsMap = new Dictionary<Regex, string>();

			// style='mso-bidi-font-weight:normal'
			var msoRuleRegex = new Regex(@"\s*style\s*=\s*[\w+\s+]*('|"")[\w+\s+]*mso[\w+\s\-\:]*('|"")",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// mso-ascii-theme-font:minor-latin and others
			var msoStyleRuleRegex = new Regex(@"\s*mso-[\s\w\-]+:[^;']*;*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoStyleRuleRegex);
			junkMsoPatternsMap.Add(msoStyleRuleRegex, string.Empty);

			// lang=EN-CA and others
			msoRuleRegex = new Regex(@"\s*lang\s*=(""|')*\s*[-\w]*(""|')*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// <a name="OLE_LINK7">
			msoRuleRegex = new Regex(@"\s*name\s*=\s*""\s*OLE_LINK[\w+\s+]*""",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// class=MsoNormal            
			msoRuleRegex = new Regex(@"\s*class\s*=\s*Mso\w*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// Word-specific tag <o:p>            
			msoRuleRegex = new Regex(@"<\s*O\s*:\s*P\s*>|<\s*/\s*O\s*:\s*P\s*>",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// Word-specific tags such as <![if !supportLineBreakNewLine]> or <![endif]> or <!--[if gte vml 1]> etc.            
			msoRuleRegex = new Regex(@"\s*<![\[*\w*\s*!\]=-]*>\s*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// Word-specific tags such as <v:shapetype ...> or <v:f ... > etc.            
			msoRuleRegex = new Regex(@"\s*</*\s*(v|o)\s*:\s*[\w\s="",:@/\\~.#';]*/*>\s*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// Word-specific attributes such as v:ext=..., v:shapes=... etc.            
			msoRuleRegex = new Regex(@"\s+(v|o)\s*:\s*[\w="",:@\\~.#';]*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// Empty tags like <SPAN></SPAN>
			msoRuleRegex = new Regex(@"\s*(<\s*A\s*>)+\s*(<\s*/\s*A\s*>)+\s*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);
			msoRuleRegex = new Regex(@"\s*(<\s*P\s*>)+\s*(<\s*/\s*P\s*>)+\s*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);
			msoRuleRegex = new Regex(@"\s*(<\s*SPAN\s*>)+\s*(<\s*/\s*SPAN\s*>)+\s*",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			// Empty style and class attributes
			msoRuleRegex = new Regex(@"\s*(style|class)='\s*'",
									 RegexOptions.Compiled | RegexOptions.IgnoreCase);
			orderedRegexList.Add(msoRuleRegex);
			junkMsoPatternsMap.Add(msoRuleRegex, string.Empty);

			var resultHtml = rawHtml;
			var resultContext = rawHtml;

			// Reconstruct images that don't have corresponding <img> tags
			var imageGroupRegex =
				new Regex(
					@"<v:shape\s*id=(?:""|')(?<id>[^""']+)(?:""|')[\s\S]+?style=(?:""|')(?<style>[^""']+)(?:""|')[\s\S]+?(?=</v:shape>)</v:shape>",
					RegexOptions.Compiled | RegexOptions.IgnoreCase);
			Match match;
			var startMatchIndex = 0;
			while ((match = imageGroupRegex.Match(resultHtml, startMatchIndex)).Success)
			{
				startMatchIndex = match.Index + match.Length;
				if (match.Groups.Count > 2)
				{
					// Extract picture id and style from the captured groups
					var pictureId = match.Groups[1].Value;
					var pictureStyle = match.Groups[2].Value;

					// We should check that Word has generated <img> tag for embedded image
					if (!Regex.IsMatch(resultHtml, string.Format(@"<img[\s\S]+?v:shapes=(?:""|'){0}(?:""|')", pictureId),
									  RegexOptions.Compiled | RegexOptions.IgnoreCase))
					{
						// If we didn't find <img> tag generated by Word - we should generate it
						var imgSrcMatch = Regex.Match(match.Value,
														@"<v:imagedata\s+src=(?:""|')(?<src>[^""']+)?(?:""|')",
														RegexOptions.Compiled | RegexOptions.IgnoreCase);
						if (imgSrcMatch.Success && imgSrcMatch.Groups.Count > 1)
						{
							var pictureSrc = imgSrcMatch.Groups[1].Value;
							var replacementSpacesValue = string.Format(@"<img style='{0}' src=""{1}"">", pictureStyle,
																		  pictureSrc);

							resultHtml = resultHtml.Substring(0, match.Index) + replacementSpacesValue +
										 resultHtml.Substring(match.Index + match.Length);

							startMatchIndex = match.Index + replacementSpacesValue.Length;
						}
					}
				}
			}

			// Inject styles from integrated style sheet directly to elements style
			var inlineStyleSheet = new Dictionary<string, WordInlineStyle>();
			var styleNameRegEx = new Regex(@"[^\@](\w+.\s*Mso\w+)|([\w+.:-]*)\s*\{", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var document = HtmlDOMLib.ParseHtml(resultContext);
			foreach (var node in document.DocumentNode.SelectNodes("//style"))
			{
				foreach (Match styleMatch in styleNameRegEx.Matches(node.InnerHtml))
				{
					var styleFullName = styleMatch.Groups[1].Value;
					if (string.IsNullOrEmpty(styleFullName) && styleMatch.Groups.Count > 2)
					{
						styleFullName = styleMatch.Groups[2].Value;
					}

					if (string.IsNullOrEmpty(styleFullName))
					{
						continue;
					}

					var contentStartIndex = node.InnerHtml.IndexOf('{', styleMatch.Index);
					var contentEndIndex = node.InnerHtml.IndexOf('}', styleMatch.Index);
					var lengthToExtract = contentEndIndex - contentStartIndex - 1;

					if (lengthToExtract <= 0)
					{
						continue;
					}

					var styleContent = node.InnerHtml.Substring(contentStartIndex + 1, lengthToExtract);

					styleContent = msoStyleRuleRegex.Replace(styleContent, string.Empty);
					styleContent = Regex.Replace(styleContent, @"\r\n\t", " ", RegexOptions.Compiled);
					styleContent = styleContent.Replace('\"', '\'');

					var inlineStyle = new WordInlineStyle(styleFullName, styleContent);
					if (inlineStyle.Valid)
					{
						if (inlineStyleSheet.ContainsKey(styleFullName))
						{
							inlineStyleSheet[styleFullName] = inlineStyle;
						}
						else
						{
							inlineStyleSheet.Add(styleFullName, inlineStyle);
						}
					}
				}
			}

			// Apply collected styles to elements
			document = HtmlDOMLib.ParseHtml(resultHtml);
			InjectInlineStylesToDocument(document.DocumentNode, inlineStyleSheet);
			resultHtml = document.DocumentNode.OuterHtml;

			// <span style='mso-spacerun:yes'>В </span> => replace with &nbsp;
			var spaceRunRegex = new Regex(@"<\s*span\s*style[^']*'mso-spacerun:[^:']+'\s*>(?<content>[^<>]*)<\s*/span\s*>",
										   RegexOptions.Compiled | RegexOptions.IgnoreCase);
			while ((match = spaceRunRegex.Match(resultHtml)).Success)
			{
				if (match.Groups.Count > 1)
				{
					// Get 'B B B B ...' inside span tag                    
					var spacesValue = match.Groups[1].Value;

					// Build replacement string where each B and space is replaced with &nbsp;
					var replacementSpacesValue = string.Empty;
					for (var i = 0; i < spacesValue.Length; i++)
					{
						replacementSpacesValue += "&nbsp;";
					}

					resultHtml = resultHtml.Substring(0, match.Index) + replacementSpacesValue +
								 resultHtml.Substring(match.Index + match.Length);
				}
			}

			// <span style='mso-tab-count:N'>  </span> => replace with &nbsp; repeated N*5 (tab size for MS Word)
			var spaceTabCountRegex = new Regex(@"<\s*span\s*style[^']*'mso-tab-count:(?<count>[^:']+)'\s*>[^<>]*<\s*/span\s*>",
										   RegexOptions.Compiled | RegexOptions.IgnoreCase);

			while ((match = spaceTabCountRegex.Match(resultHtml)).Success)
			{
				if (match.Groups.Count > 1)
				{
					// Get tab count value                    
					int tabCount;

					if (!int.TryParse(match.Groups[1].Value, out tabCount))
					{
						tabCount = 1;
					}

					var replacementSpacesValue = string.Empty;

					for (var i = 0; i < tabCount; i++)
					{
						replacementSpacesValue += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
					}

					resultHtml = resultHtml.Substring(0, match.Index) + replacementSpacesValue + resultHtml.Substring(match.Index + match.Length);
				}
			}

			// Remove all junk MS Word specific tags using regular expression patterns
			foreach (var regex in orderedRegexList)
			{
				var replacement = junkMsoPatternsMap[regex];
				resultHtml = regex.Replace(resultHtml, replacement);
			}

			resultHtml = FilterJunkSpanTags(resultHtml);

			return resultHtml;
		}

		/// <summary>
		/// Injects the inline styles to document.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="inlineStyleSheet">The inline style sheet.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html attribute")]
		static void InjectInlineStylesToDocument(HtmlNode node, Dictionary<string, WordInlineStyle> inlineStyleSheet)
		{
			var className = node.GetAttributeValue("class", string.Empty);
			var key = string.Join(".", new[] { node.Name, className }).TrimEnd('.');

			WordInlineStyle inlineStyle;
			if (inlineStyleSheet.TryGetValue(key, out inlineStyle))
			{
				node.Attributes.Remove("class");

				var clearMarginInMsoNormal = false;

				//check if this node is a paragraph and has MsoNormal style and has margin=auto
				if (string.Equals(key, ParagraphMsoNormalClassSelector, StringComparison.OrdinalIgnoreCase))
				{
					var paragraphMarginAutoRegex = new Regex(@"(^|\s|;)mso-margin-(bottom|top)-alt\s*:\s*auto\s*(;|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
					foreach (HtmlAttribute htmlAttribute in node.Attributes)
					{
						if (string.Equals(htmlAttribute.Name, "style", StringComparison.OrdinalIgnoreCase)
							&& paragraphMarginAutoRegex.IsMatch(htmlAttribute.Value))
						{
							clearMarginInMsoNormal = true; //we should not add MsoNormal margin, because the current node has margin=auto
							break;
						}
					}
				}

				var styleContent = clearMarginInMsoNormal ? RemoveMarginsInStyle(inlineStyle.Content) : inlineStyle.Content;

				node.Attributes.Add("style", styleContent);
			}

			foreach (var childNode in node.ChildNodes)
			{
				InjectInlineStylesToDocument(childNode, inlineStyleSheet);
			}
		}

		/// <summary>
		/// Removes margin in <paramref name="styleValue"/>.
		/// </summary>
		/// <param name="styleValue">The styles.</param>
		/// <returns></returns>
		static string RemoveMarginsInStyle(string styleValue)
		{
			Regex marginRegex = new Regex(@"(?<=(^|;|\s))margin((-top)|(-bottom))?\s*:\s*[^;""]+;?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			styleValue = marginRegex.Replace(styleValue, "");

			return styleValue;
		}

		/// <summary>
		/// Filters the junk span tags.
		/// </summary>
		/// <param name="html">The HTML.</param>
		/// <returns></returns>
		static string FilterJunkSpanTags(string html)
		{
			var spanRemover = new VoidSpanRemover();
			return spanRemover.ParseAndFix(html);
		}

		/// <summary>
		/// Determines whether [is ms word content in clipboard] [the specified raw HTML].
		/// </summary>
		/// <param name="rawHtml">The raw HTML.</param>
		/// <returns></returns>
		static bool IsMsWordContentInRawHtml(string rawHtml)
		{
			return Regex.IsMatch(rawHtml, @"<meta[\s*\w*\=]*Word[\s*\w*\.]*>",
								 RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
		}
	}
}
