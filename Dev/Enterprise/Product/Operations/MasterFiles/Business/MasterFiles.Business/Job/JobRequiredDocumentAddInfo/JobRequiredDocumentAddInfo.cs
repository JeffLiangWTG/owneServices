using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.MasterFiles.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(Schema.EX_ReferenceNumber), DescriptionProperty(Schema.EX_ReferenceNumber)]
	public class JobRequiredDocumentAddInfo : AutoJobRequiredDocumentAddInfo, IDocManagerSupportProvider
	{
		public JobRequiredDocumentAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EX_GC_Company = GlbCompany.CurrentCompany.PK;
		}

		IEnumerable<IDocManagerSupport> IDocManagerSupportProvider.DocManagerSupports
		{
			get
			{
				var requiredDocument = RequiredDocument;

				return requiredDocument != null ? ((IDocManagerSupportProvider)requiredDocument).DocManagerSupports : Enumerable.Empty<IDocManagerSupport>();
			}
		}

		ZBool isReferenceNumberGenerated;

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsDeleted && EX_ReferenceNumber.IsEmpty)
			{
				var numberFountainStrategy = DisHost?.DISReferenceNumberFountainStrategy;
				if (numberFountainStrategy != null)
				{
					EX_ReferenceNumber = numberFountainStrategy.GetDISReferenceNumber();
					isReferenceNumberGenerated = true;
				}
			}
		}

		public IDISHost DisHost
		{
			get
			{
				var disHostProvider = RequiredDocument?.DocumentParent as IDISHostProvider;
				return disHostProvider?.DISHost;
			}
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (uniqueIndexFailureHandlers == null || !uniqueIndexFailureHandlers.Any())
				{
					uniqueIndexFailureHandlers = new List<IUniqueIndexFailureHandler>();
					var numberFountainStrategy = DisHost?.DISReferenceNumberFountainStrategy;
					if (numberFountainStrategy != null)
					{
						uniqueIndexFailureHandlers.Add(numberFountainStrategy.GetUniqueIndexFailureHandler(this));
					}
				}
				return uniqueIndexFailureHandlers;
			}
		}
		List<IUniqueIndexFailureHandler> uniqueIndexFailureHandlers;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && isReferenceNumberGenerated)
			{
				EX_ReferenceNumber = ZString.Empty;
				isReferenceNumberGenerated = false;
			}
		}

		#region properties

		[ResourceStringData("JobRequiredDocumentAddInfo|DocumentType", Caption = "Doc Type")]
		public ZString DocType
		{
			get { return RequiredDocument?.EQ_DocType ?? ZString.Empty; }
		}

		public ZPropertyInfo DocTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DocType)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|Category", Caption = "Category")]
		public ZString Category
		{
			get { return RequiredDocument?.EQ_DocCategory ?? ZString.Empty; }
		}

		public ZPropertyInfo CategoryInfo
		{
			get { return GetZPropertyInfo(nameof(Category)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|Description", Caption = "Doc Description")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		public ZString Description
		{
			get { return RequiredDocument?.EQ_DocDescription ?? ZString.Empty; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|Period", Caption = "Period")]
		public ZString Period
		{
			get { return RequiredDocument?.EQ_DocPeriod ?? ZString.Empty; }
		}

		public ZPropertyInfo PeriodInfo
		{
			get { return GetZPropertyInfo(nameof(Period)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|Received", Caption = "Date Received")]
		public ZDateTimeOffset DateReceived
		{
			get { return RequiredDocument?.EQ_DateReceived ?? ZDateTimeOffset.Empty; }
		}

		public ZPropertyInfo DateReceivedInfo
		{
			get { return GetZPropertyInfo(nameof(DateReceived)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|ValidToDate", Caption = "Valid To Date")]
		public ZDateTime ValidToDate
		{
			get { return RequiredDocument?.EQ_ValidToDate ?? ZDateTime.Empty; }
		}

		public ZPropertyInfo ValidToDateInfo
		{
			get { return GetZPropertyInfo(nameof(ValidToDate)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|DocNumber", Caption = "Doc Num.")]
		public ZString DocNumber
		{
			get { return RequiredDocument?.EQ_DocNumber ?? ZString.Empty; }
		}

		public ZPropertyInfo DocNumberInfo
		{
			get { return GetZPropertyInfo(nameof(DocNumber)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|DocUsage", Caption = "Usage")]
		public ZString DocUsage
		{
			get { return RequiredDocument?.EQ_DocUsage ?? ZString.Empty; }
		}

		public ZPropertyInfo DocUsageInfo
		{
			get { return GetZPropertyInfo(nameof(DocUsage)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|Country", Caption = "Country/Region")]
		public ZString Country
		{
			get { return RequiredDocument?.EQ_RN_NKRelatedCountry ?? ZString.Empty; }
		}

		public ZPropertyInfo CountryInfo
		{
			get { return GetZPropertyInfo(nameof(Country)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|OriginalDocRequired", Caption = "Original Required")]
		public ZBool OriginalDocRequired
		{
			get { return RequiredDocument?.EQ_OriginalDocRequired ?? ZBool.False; }
		}

		public ZPropertyInfo OriginalDocRequiredInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalDocRequired)); }
		}

		[ResourceStringData("JobRequiredDocumentAddInfo|CreditControlDoc", Caption = "Credit Control")]
		public ZBool CreditControlDoc
		{
			get { return RequiredDocument?.EQ_CreditControlDoc ?? ZBool.False; }
		}

		public ZPropertyInfo CreditControlDocInfo
		{
			get { return GetZPropertyInfo(nameof(CreditControlDoc)); }
		}
		#endregion
	}
}
