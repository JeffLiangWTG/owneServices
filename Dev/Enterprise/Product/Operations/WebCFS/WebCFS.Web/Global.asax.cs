using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.WebCFS.Web
{
	/// <summary>
	///  Global settings for the Application
	/// </summary>
	public partial class Global : ZGlobal
	{
		#region Pages and constants

		public override string DefaultPage
		{
			get { return ApplicationRoot + "Default.aspx"; }
		}

		#region Custom pages

		public string RelativeContainerPage
		{
			get { return "Containers/Containers.aspx"; }
		}

		public string RelativeFumigationPage
		{
			get { return "Fumigation/Fumigation.aspx"; }
		}

		public string RelativeSailingPage
		{
			get { return "Sailing/Sailing.aspx"; }
		}

		#endregion

		#endregion

		public override string BaseStyleSheet
		{
			get
			{
				var theme = WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, UrlForRegistryItems);
				if (theme == null || theme == ThemeCodeDescriptionPairList.Codes.CUS)
				{
					return base.BaseStyleSheet;
				}
				else
				{
					return ApplicationRoot + String.Format(CultureInfo.CurrentCulture, (NoResString)"App_Themes/{0}/BaseStyle.css", GetThemeName(theme));
				}
			}
		}

		public override string LogoImage
		{
			get
			{
				var theme = WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, UrlForRegistryItems);
				if (theme == null || theme == ThemeCodeDescriptionPairList.Codes.CUS)
				{
					return base.LogoImage;
				}
				else
				{
					return ApplicationRoot + String.Format(CultureInfo.CurrentCulture, (NoResString)"App_Themes/{0}/Images/Logo.gif", GetThemeName(theme));
				}
			}
		}

		string GetThemeName(string themeCode)
		{
			ThemeCodeDescriptionPairList list = new ThemeCodeDescriptionPairList();
			return list.GetMultilingualDescriptionFromCode(themeCode).GetUnresolvedString();
		}

		protected override ZGlobalConfig GetNewGlobalConfig()
		{
			return new CFSGlobalConfig();
		}

		[SuppressMessage("Microsoft.Design", "CA1056", Justification = "Need too many refactoring")]
		public virtual string UrlForRegistryItems
		{
			get { return HttpContext.Current.Request.Url.Host; }
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637607 - This component is needed for designer as suggested in the comment above.")]
		System.ComponentModel.IContainer components = null;

		public Global()
		{
			InitializeComponent();
		}

#if DEBUG
		public
#else
			protected
#endif
 override void PopulateWebRegistry(string url)
		{
			base.PopulateWebRegistry(url);

			var urls = WebDataRegistry.Instance.WebCFSUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
			if (!WebDataRegistry.ContainsWebUrl(urls, url))
			{
				// Populate WebCFSUrls
				urls.Add(url);
				WebDataRegistry.Instance.WebCFSUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, urls.ToArray());

				// Populate WebCFSTheme
				var theme = ThemeCodeDescriptionPairList.Codes.CUS;
				var themeObjects = WebDataRegistry.Instance.WebCFSTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				if (!themeObjects.Any(x => x.Url == url && x.Code != theme))
				{
					themeObjects.Add(new WebTrackerTheme(url, theme));
					WebDataRegistry.Instance.WebCFSTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, themeObjects.ToArray());
				}

				if (theme == ThemeCodeDescriptionPairList.Codes.CUS)
				{
					// Populate WebCFSCustomImages for images that are not present for All URLs
					var imageObjects = WebDataRegistry.Instance.WebCFSCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
					if (!imageObjects.Any(x => x.Url == url))
					{
						var overridenImages = imageObjects.Where(x => string.IsNullOrEmpty(x.Url)).Select(x => x.Name).ToArray();
						var imagesFolder = Path.Combine(ApplicationRoot, (NoResString)"Images");
						var imagePaths = new DirectoryInfo(MapPath(imagesFolder)).GetFiles()
							.Where(x => WebTrackerCustomImage.IsSupportedFileType(x.Extension)).Select(x => x.FullName);

						foreach (var imagePath in imagePaths)
						{
							var imageName = Path.GetFileName(imagePath);
							if (!overridenImages.Contains(imageName))
							{
								try
								{
									var imageData = File.ReadAllBytes(imagePath);
									imageObjects.Add(new WebTrackerCustomImage(imageName, url, imageData));
								}
								catch (UnauthorizedAccessException)
								{
									//supress error throw
								}
							}
						}
						WebDataRegistry.Instance.WebCFSCustomImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, imageObjects.ToArray());
					}

					// Populate WebTrackerCustomCss
					var cssObjects = WebDataRegistry.Instance.WebCFSCustomCss.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
					if (!cssObjects.Any(x => x.Url == url))
					{
						cssObjects.Add(new WebTrackerCustomCss(url, File.ReadAllText(MapPath(BaseStyleSheet))));
						WebDataRegistry.Instance.WebCFSCustomCss.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cssObjects.ToArray());
					}
				}
			}
		}
	}
}
