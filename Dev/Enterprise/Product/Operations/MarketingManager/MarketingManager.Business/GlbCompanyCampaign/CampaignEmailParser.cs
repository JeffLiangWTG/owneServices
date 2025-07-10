using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Business;
using HtmlAgilityPack;

namespace Enterprise.MarketingManager.Business
{
	public class ContactDocumentParser : DocumentParser<OrgContact>
	{
		public ContactDocumentParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordEmail>(); }
		}
	}

	public class CampaignDocumentParser : DocumentParser<GlbCompanyCampaignItem>
	{
		public CampaignDocumentParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool ThrowErrorOnParsing
		{
			get { return throwErrorOnParsing; }
			set { throwErrorOnParsing = value; }
		}

		protected override Type TypeOfWrapper
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocCompanyCampaignItem>(); }
		}

		internal Type InternalTypeofWrapperForTest() => TypeOfWrapper;

		protected override ZString ParseCore(GlbCompanyCampaignItem objectToWrap, ZString documentText)
		{
			DocBuilderParsingRoots = new BusinessObject[] { objectToWrap, GetDocWrapper(objectToWrap, Factory) };
			return base.ParseCore(objectToWrap, documentText);
		}

		#region preview / email

		public enum ParseType
		{
			PlainText,
			HtmlPreview,
			HtmlEmail,
		}

		[Obsolete("please use [public ZString Parse(ParseType parseType, ... )] instead", true)] // true will cause a compile-time error
		public new ZString Parse(GlbCompanyCampaignItem campaignItem, ZString htmlText)
		{
			if (campaignItem != null && htmlText.Length > 0) //avoid CA1801
			{
			}

			throw new NotImplementedException();
		}

		public ZString Parse(ParseType parseType, GlbCompanyCampaignItem campaignItem, ZString htmlText, Dictionary<string, byte[]> embedInHtmlImages = null)
		{
			var result = base.Parse(campaignItem, htmlText);

			if (parseType == ParseType.PlainText)
			{
				return result;
			}

			if (parseType == ParseType.HtmlEmail && embedInHtmlImages == null)
			{
				throw new ArgumentNullException(nameof(embedInHtmlImages));
			}

			embedInHtmlImages?.Clear();
			var doc = new HtmlDocument();
			doc.LoadHtml(result);
			var nodesDeleting = new List<HtmlNode>();

			foreach (var node in doc.DocumentNode.Descendants())
			{
				var src = node.Attributes[AttributeSource];
				var macro = node.Attributes[AttributeMacro];

				if (node.Name == TagNameImage && src != null && macro != null) //macro images
				{
					if (string.IsNullOrWhiteSpace(macro.Value)) //empty image
					{
						nodesDeleting.Add(node);
					}
					else
					{
						if (parseType == ParseType.HtmlPreview)
						{
							src.Value = Base64ImagePrefix + macro.Value;
						}
						else if (parseType == ParseType.HtmlEmail)
						{
							var bytes = LoadImageFromBase64String(macro.Value);
							var fileId = Guid.NewGuid();
							src.Value = $"{fileId}.png";
							embedInHtmlImages.GetOrAdd(src.Value, () => bytes);
						}

						//the image size should based on width
						RemoveImageHeightAttribute(node);
					}
					macro.Remove();
				}
				else if (node.Name == TagNameImage && src != null && Regex.IsMatch(src.Value, @"data:image\/(jpg|jpeg|gif|png|bmp|svg)+;base64,[^\"",:]*", RegexOptions.IgnoreCase) && parseType == ParseType.HtmlEmail) //Image embedded in Html
				{
					if (TryExtractExtensionAndBytesFromEmbeddedImage(src.Value, out var extension, out var bytes))
					{
						var fileId = Guid.NewGuid();
						src.Value = $"{fileId}.{extension}";
						embedInHtmlImages.GetOrAdd(src.Value, () => bytes);
					}
				}
			}

			nodesDeleting.ForEach((x) => x.Remove());
			return doc.DocumentNode.WriteTo();
		}

		void RemoveImageHeightAttribute(HtmlNode node)
		{
			if (node != null && node.Name == TagNameImage)
			{
				node.Attributes[AttributeHeight]?.Remove();
				var style = node.Attributes[AttributeStyle]?.Value;

				if (!string.IsNullOrWhiteSpace(style) && style.IndexOf(AttributeHeight, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					var regex = new Regex(RegexPatternHeightStyle, RegexOptions.IgnoreCase);
					var newStyle = regex.Replace(style, String.Empty);
					node.Attributes[AttributeStyle].Value = newStyle;
				}
			}
		}

		byte[] LoadImageFromBase64String(string base64String)
		{
			var bytes = Convert.FromBase64String(base64String);

			if (bytes.Length > MaxBitmapImageSize && bytes[0] == 'B' && bytes[1] == 'M')
			{
				using (var memoryStreamReadOnly = new MemoryStream(bytes))
				{
					using (var image = Image.FromStream(memoryStreamReadOnly))
					{
						using (var ms = new MemoryStream())
						{
							image.Save(ms, ImageFormat.Png);
							bytes = ms.ToArray();
						}
					}
				}
			}

			return bytes;
		}

		bool TryExtractExtensionAndBytesFromEmbeddedImage(string sourceValue, out string extension, out byte[] bytes)
		{
			extension = string.Empty;
			bytes = Array.Empty<byte>();
			var commaSplits = sourceValue.Split(',');
			if (commaSplits.Length == 2)
			{
				extension = commaSplits[0].Replace("data:image/", "").Replace(";base64", "");
				if (string.IsNullOrEmpty(commaSplits[1]))
				{
					return false;
				}

				try
				{
					bytes = LoadImageFromBase64String(commaSplits[1]);
					return true;
				}
				catch (FormatException)
				{
				}
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not GUI text, internal use only.")]
		const string TagNameImage = "img";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not GUI text, internal use only.")]
		const string AttributeSource = "src";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not GUI text, internal use only.")]
		const string AttributeMacro = "macro";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not GUI text, internal use only.")]
		const string AttributeHeight = "height";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not GUI text, internal use only.")]
		const string AttributeStyle = "style";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not GUI text, internal use only.")]
		const string RegexPatternHeightStyle = @"height(.*?)(;)";
		const int MaxBitmapImageSize = 100000;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not GUI text, internal use only.")]
		const string Base64ImagePrefix = "data:image/png;base64,";

		#endregion
	}
}
