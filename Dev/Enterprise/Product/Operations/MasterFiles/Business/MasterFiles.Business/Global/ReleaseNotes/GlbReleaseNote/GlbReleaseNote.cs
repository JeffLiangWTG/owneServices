using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.CustomerService;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.GF_Category)]
	[DescriptionProperty(Schema.GF_Summary)]
	public class GlbReleaseNote : AutoGlbReleaseNote
	{
		public GlbReleaseNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.GF_ReleaseNoteDate = ZDateTime.Today;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			GlbReleaseNoteRead[] readRecords = Factory.Load<GlbReleaseNoteRead>(new ZQuery(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, PK));
			foreach (GlbReleaseNoteRead note in readRecords)
			{
				note.Delete();
			}

			base.Delete();
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GlbReleaseNoteFetchStrategy(this);
		}

		class GlbReleaseNoteFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public GlbReleaseNoteFetchStrategy(GlbReleaseNote businessObject)
				: base(businessObject)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(typeof(RefCountry), RefCountrySchema.RN_Code, BusinessObject.GF_RN_NKCountryForReleaseNote);
				Factory.AddFetchHint(new ReleaseNoteReadFetchHint(BusinessObject));
			}

			protected new GlbReleaseNote BusinessObject
			{
				get { return (GlbReleaseNote)base.BusinessObject; }
			}
		}

		internal class ReleaseNoteReadFetchHint : IFetchHint
		{
			public ReleaseNoteReadFetchHint(GlbReleaseNote note)
			{
				this.notePK = note.PK;
			}

			readonly ZGuid notePK;

			IEnumerable<SchemaColumn> IFetchHint.LoadWithBlobs
			{
				get { return Array.Empty<SchemaColumn>(); }
			}

			string IFetchHint.BuilderKey
			{
				get { return "ReleaseNoteReadFetchHint"; }
			}

			ZQuery IFetchHint.GetQuery()
			{
				if (GlbStaff.CurrentUser != null)
				{
					ZQuery result = new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
					result.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, notePK);
					return result;
				}
				return new ZQuery();
			}

			void IFetchHint.GenerateQuery(QueryBuilder builder)
			{
				if (builder.IsEmpty)
				{
					var mainQuery = (GlbStaff.CurrentUser != null)
						? new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK)
						: new ZQuery();

					builder.Init(mainQuery, GlbReleaseNoteReadSchema.GR_ReleaseNoteID);
				}

				builder.AddValue(notePK);
			}

			IQueryHashKey IFetchHint.GetHashKeyObject()
			{
				return new FetchHint.EnumerableHashObject { GlbStaff.CurrentUser.PK, notePK };
			}

			bool IFetchHint.IsDataHintLoaded
			{
				get { return fIsDataHintLoaded; }
				set { fIsDataHintLoaded = value; }
			}

			bool fIsDataHintLoaded;

			bool IFetchHint.IsNeeded(QueryHistoryProvider historyProvider)
			{
				if (GlbStaff.CurrentUser != null)
				{
					ZQuery query = new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
					query.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, notePK);

					return !historyProvider.IsQueryCached(TableName, query);
				}
				return false;
			}

			public string TableName
			{
				get { return GlbReleaseNoteReadSchema.Constants.TableName; }
			}
		}

		#endregion

		#region Properties

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

		[List("Lookups.SectionList")]
		public override ZString GF_Section
		{
			get { return base.GF_Section; }
			set { base.GF_Section = value; }
		}

		#region Category Display Name

		public ZString CategoryDisplayName
		{
			get
			{
				ZString result;

				try
				{
					result = (GF_Category.IsEmpty) ? "" : Env.Licence.GetCheckpointFromCode(GF_Category).DisplayName;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					result = "";
				}

				return result;
			}
		}

		public ZPropertyInfo CategoryDisplayNameInfo
		{
			get { return GetZPropertyInfo(nameof(CategoryDisplayName)); }
		}

		#endregion

		#region Section Name

		[ResourceStringData("GlbReleaseNote|SectionName", Caption = "News Section Name")]
		public ZString SectionName
		{
			get { return Lookups.SectionList.GetDescriptionFromCode(GF_Section); }
		}

		#endregion

		#region Title

		public ZString Title
		{
			get
			{
				var summary = GF_SummaryMultilingual.ToString();
				int index;
				var shouldSplit = (IsWiseTechGlobalItem && (index = summary.IndexOf(':')) >= 0) ||
												(index = summary.IndexOf('\n')) >= 0;
				return shouldSplit ? summary.Substring(0, index) : summary;
			}
		}

		public ZPropertyInfo TitleInfo
		{
			get { return GetZPropertyInfo(nameof(Title)); }
		}

		#endregion

		#region Summary

		[BusinessObjectTestExclude]
		[GlbReleaseNoteTranslatableDataField(Schema.TableName, Schema.GF_Summary, MaxLength = Schema.GF_SummaryMaxLength, Asmid = ResString.AssemblyId)]
		public override ZString GF_Summary { get => base.GF_Summary; set => base.GF_Summary = value; }

		public MultilingualString GF_SummaryMultilingual => GetMultilingual(GF_SummaryInfo);

		public ZPropertyInfo GF_SummaryMultilingualInfo => GetZPropertyInfo(nameof(GF_SummaryMultilingual));

		public ZString SummaryForWeb => GF_SummaryMultilingual.Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

		public ZPropertyInfo SummaryForWebInfo
		{
			get { return GetZPropertyInfo(nameof(SummaryForWeb)); }
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is part of a URL.")]
		public virtual ZString GetDownloadURL()
		{
			ZString result = base.GF_URL;

			if (IsWiseTechGlobalItem)
			{
				const string QueryStringKeyFile = "file";
				const string QueryStringKeyLanguage = "language";

				if (IsWiseTechGlobalItemViaTrustedMessaging)
				{
					if (EnvProxy.Instance.CurrentUser != null)
					{
						var queryString = HttpUtility.ParseQueryString(string.Empty);
						queryString.Add(QueryStringKeyFile, WebUtility.UrlEncode(base.GF_URL));
						queryString.Add(QueryStringKeyLanguage, EnvProxy.Instance.CurrentUser.Language);
						var returnUrl = FormattableString.Invariant($"{WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/')}/Download.aspx?{queryString}");
						var autoLoginUrl = ObjectFactory.Get<IUserPortalClient>().GetMyAccountAutoLoginUrl(new Uri(returnUrl))?.ToString();
						return !string.IsNullOrEmpty(autoLoginUrl) ? autoLoginUrl : returnUrl;
					}
				}
				else
				{
					IStaffContactConverter converter = ObjectFactory.Get<IStaffContactConverter>();
					if (converter != null)
					{
						if (EnvProxy.Instance.CurrentUser != null)
						{
							SecureQueryString queryString = converter.CurrentStaffAndRegistrationToSecuredQueryString();
							queryString[QueryStringKeyFile] = base.GF_URL;
							queryString[QueryStringKeyLanguage] = EnvProxy.Instance.CurrentUser.Language;
							result = Registry.Business.WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/') + (NoResString)"/Download.aspx?qdata=" + WebUtility.UrlEncode(queryString.ToString());
						}
					}
				}
			}

			return result;
		}

		public bool IsWiseTechGlobalItemViaTrustedMessaging => IsWiseTechGlobalItem && WebDataRegistry.Instance.EnableTrustedMessaging.Value;

		public bool IsWiseTechGlobalItem
		{
			get { return GF_Section == NewsSectionTypeList.Codes.ProductUpdates || GF_Section == NewsSectionTypeList.Codes.WiseLearningUpdates || GF_Section == NewsSectionTypeList.Codes.WiseNews || GF_Section == NewsSectionTypeList.Codes.TechnicalAdvisoryNotes; }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Getting/Setting Is Read

		// Bound Property
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

		public ZPropertyInfo IsCurrentlyReadInfo
		{
			get { return GetZPropertyInfo(nameof(IsCurrentlyRead)); }
		}

		bool noteReadIsLoaded;
		GlbReleaseNoteRead noteRead;

		void EnsureNoteReadIsLoaded()
		{
			if (!noteReadIsLoaded && GlbStaff.CurrentUser != null)
			{
				ZQuery filter = new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
				filter.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, PK);
				noteRead = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
				noteReadIsLoaded = true;
			}
		}
		#endregion

		#region Image

		public Image ThumbnailImage
		{
			get { return GetImage(GF_Thumbnail); }
			set { SetImage(value, GlbReleaseNoteSchema.GF_Thumbnail); }
		}

		Image GetImage(ZBlob blob)
		{
			if (!blob.IsEmpty)
			{
				return Image.FromStream(new MemoryStream(blob));
			}

			return null;
		}

		void SetImage(Image value, SchemaBinaryColumn blobColumn)
		{
			if (value != null)
			{
				using var stream = new MemoryStream();
				value.Save(stream, value.RawFormat.Equals(ImageFormat.MemoryBmp) ? ImageFormat.Bmp : value.RawFormat);
				this[blobColumn] = stream.ToArray();
			}
			else
			{
				this[blobColumn] = ZBlob.Empty;
			}
		}

		#endregion

		#region Test
#if DEBUG

		internal void ClearNoteReadIsLoaded()
		{
			noteReadIsLoaded = false;
		}

#endif
		#endregion
	}
}
