using System;
using System.Data;
using System.Net;
using System.Web;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.CustomerService;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business;

[CodeProperty(Schema.GF_Category), DescriptionProperty(Schema.GF_Summary)]
public class GlbReleaseNoteCombined : AutoGlbReleaseNoteCombined
{
	public GlbReleaseNoteCombined(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GF_ReleaseNoteDate = ZDateTime.UtcToday;
	}

	#region Delete

	public override void Delete()
	{
		var readRecords = Factory.Load<GlbReleaseNoteRead>(new ZQuery(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, PK));

		foreach (var readRecord in readRecords)
		{
			readRecord.Delete();
		}

		base.Delete();
	}

	#endregion

	#region Properties

	// TODO: This needs to be updated to read data from GlbReleaseNote table
	public ZBlob GF_Thumbnail { get; set; }

	[List("Lookups.SectionList")]
	public override ZString GF_Section
	{
		get { return base.GF_Section; }
		set { base.GF_Section = value; }
	}

	[List("Lookups.Countries")]
	public override ZString GF_RN_NKCountryForReleaseNote
	{
		get { return base.GF_RN_NKCountryForReleaseNote; }
		set { base.GF_RN_NKCountryForReleaseNote = value; }
	}

	[List("Lookups.Categories")]
	public override ZString GF_Category
	{
		get { return base.GF_Category; }
		set { base.GF_Category = value; }
	}

	#endregion

	#region Category Display Name

	public ZString CategoryDisplayName
	{
		get
		{
			if (GF_Category.IsEmpty)
			{
				return ZString.Empty;
			}

			var checkPoint = Env.Licence.GetCheckpointFromCode(GF_Category);

			return checkPoint?.DisplayName ?? ZString.Empty;
		}
	}
	public ZPropertyInfo CategoryDisplayNameInfo => GetZPropertyInfo(nameof(CategoryDisplayName));

	#endregion

	#region IsCurrentlyRead

	bool isNoteReadLoaded;

	GlbReleaseNoteRead noteRead;

	public ZBool IsCurrentlyRead
	{
		get
		{
			EnsureNoteReadIsLoaded();

			return noteRead != null;
		}
		set
		{
			if (IsCurrentlyRead != value && GlbStaff.CurrentUser != null)
			{
				if (noteRead == null)
				{
					noteRead = Factory.New<GlbReleaseNoteRead>();
					noteRead.GR_ReleaseNoteID = PK;
					noteRead.GR_GS_Staff = GlbStaff.CurrentUser.PK;
				}
				else
				{
					noteRead.Delete();
					noteRead = null;
				}

				IsCurrentlyReadInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo IsCurrentlyReadInfo => GetZPropertyInfo(nameof(IsCurrentlyRead));

	void EnsureNoteReadIsLoaded()
	{
		if (!isNoteReadLoaded && GlbStaff.CurrentUser != null)
		{
			var filter = new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
			filter.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, PK);
			noteRead = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
			isNoteReadLoaded = true;
		}
	}

	internal void ClearIsNoteReadLoaded()
	{
		isNoteReadLoaded = false;
	}

	#endregion

	#region Title

	public ZString Title
	{
		get
		{
			int index;
			var summary = GF_SummaryMultilingual.ToString();
			var shouldSplit = (IsWiseTechGlobalItem && (index = summary.IndexOf(':')) >= 0) || (index = summary.IndexOf('\n')) >= 0;

			return shouldSplit ? summary.Substring(0, index) : summary;
		}
	}

	public ZPropertyInfo TitleInfo => GetZPropertyInfo(nameof(Title));

	#endregion

	#region Summary

	[BusinessObjectTestExclude]
	[GlbReleaseNoteCombinedTranslatableDataField(Schema.TableName, Schema.GF_Summary, MaxLength = Schema.GF_SummaryMaxLength, Asmid = ResString.AssemblyId)]
	public override ZString GF_Summary
	{
		get => base.GF_Summary;
		set => base.GF_Summary = value;
	}

	public MultilingualString GF_SummaryMultilingual => GetMultilingual(GF_SummaryInfo);

	public ZPropertyInfo GF_SummaryMultilingualInfo => GetZPropertyInfo(nameof(GF_SummaryMultilingual));

	public ZString SummaryForWeb => GF_SummaryMultilingual.Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

	public ZPropertyInfo SummaryForWebInfo => GetZPropertyInfo(nameof(SummaryForWeb));

	#endregion

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is part of a URL.")]
	public virtual ZString GetDownloadURL()
	{
		var result = base.GF_URL;

		if (IsWiseTechGlobalItem && EnvProxy.Instance.CurrentUser != null)
		{
			const string queryStringKeyFile = "file";
			const string queryStringKeyLanguage = "language";

			if (IsWiseTechGlobalItemViaTrustedMessaging)
			{
				var queryString = HttpUtility.ParseQueryString(string.Empty);
				queryString.Add(queryStringKeyFile, WebUtility.UrlEncode(result));
				queryString.Add(queryStringKeyLanguage, EnvProxy.Instance.CurrentUser.Language);
				var returnUrl = FormattableString.Invariant($"{WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/')}/Download.aspx?{queryString}");
				var autoLoginUrl = ObjectFactory.Get<IUserPortalClient>().GetMyAccountAutoLoginUrl(new Uri(returnUrl))?.ToString();

				return string.IsNullOrEmpty(autoLoginUrl) ? returnUrl : autoLoginUrl;
			}

			if (ObjectFactory.Get<IStaffContactConverter>() is { } converter)
			{
				var queryString = converter.CurrentStaffAndRegistrationToSecuredQueryString();
				queryString[queryStringKeyFile] = result;
				queryString[queryStringKeyLanguage] = EnvProxy.Instance.CurrentUser.Language;
				result = WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/') + (NoResString)"/Download.aspx?qdata=" + WebUtility.UrlEncode(queryString.ToString());
			}
		}

		return result;
	}

	public bool IsWiseTechGlobalItemViaTrustedMessaging => IsWiseTechGlobalItem && WebDataRegistry.Instance.EnableTrustedMessaging.Value;

	public bool IsWiseTechGlobalItem => GF_Section == NewsSectionTypeList.Codes.ProductUpdates || GF_Section == NewsSectionTypeList.Codes.WiseLearningUpdates || GF_Section == NewsSectionTypeList.Codes.WiseNews;
}
