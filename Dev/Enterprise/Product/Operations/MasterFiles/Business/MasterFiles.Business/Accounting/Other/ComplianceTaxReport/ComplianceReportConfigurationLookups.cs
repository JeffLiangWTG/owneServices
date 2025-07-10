using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	public class ComplianceReportConfigurationLookups : ZLookups
	{
		public ComplianceReportConfigurationLookups(ComplianceReportConfiguration parent)
			: base(parent)
		{
		}

		protected new ComplianceReportConfiguration Parent
		{
			get { return (ComplianceReportConfiguration)base.Parent; }
		}

		protected override BusinessObjectFactory Factory
		{
			get
			{
				return base.Factory ?? factory ?? (factory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory factory;

		#region Country List

		public RefCountryCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = new RefCountryCollection(Factory);
				}
				return countryList;
			}
		}
		RefCountryCollection countryList;

		#endregion

		#region TaxRegistrationTypeList

		public CodeDescriptionPairList TaxRegistrationTypeList
		{
			get { return GetTaxRegistrationTypeList(); }
		}

		protected CodeDescriptionPairList GetTaxRegistrationTypeList()
		{
			return new OrgCodeLists().CustomsCodes_List(RefCountry.LoadFromCountryCode(Factory, Parent.Country));
		}

		public CodeDescriptionPairList RepCountryRegistrationCodeList
		{
			get { return GetRepCountryRegistrationCodeList(); }
		}

		protected CodeDescriptionPairList GetRepCountryRegistrationCodeList()
		{
			return new OrgCodeLists().CustomsCodes_List(RefCountry.LoadFromCountryCode(Factory, Parent.Country));
		}

		#endregion

		#region PeriodicityList

		public CodeDescriptionPairList ReportPeriodicityList { get => GetReportPeriodicityList(Parent.Country); }

		public static CodeDescriptionPairList GetReportPeriodicityList(string countryCode)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ReportPeriodicityCodes.AccountingPeriod, Res.GetString("ac889940-a539-4c6c-a208-44fd9f0f2d20", "Accounting Period"));
			result.AddPair(ReportPeriodicityCodes.CalendarMonth, Res.GetString("f04d7add-b016-4997-85e9-52d8cbfe27d4", "Calendar Month"));
			result.AddPair(ReportPeriodicityCodes.DateRange, Res.GetString("1f478db6-1378-4cc6-8e05-fe9efef76814", "Date Range"));
			result.AddPair(ReportPeriodicityCodes.FinancialYear, Res.GetString("92bcdb2c-9f87-409a-960c-1dd0f16b20de", "Financial Year"));
			if (SupportsComplianceFinancialYear(countryCode))
			{
				result.AddPair(ReportPeriodicityCodes.ComplianceFinancialYear, Res.GetString("e9fc43db-3fe2-431d-af68-4202faf430bb", "Compliance Financial Year"));
			}
			result.AddPair(ReportPeriodicityCodes.RangeAccountingPeriod, Res.GetString("7A42D56F-6B67-4D97-A4D4-DEEB05AF3910", "Range of Accounting Periods"));
			result.AddPair(ReportPeriodicityCodes.MonthlyQuarterlyYearly, Res.GetString("47F0AF53-8F70-475D-B763-473187EFD00B", "Calendar Month/Quarter/Year"));
			return result;
		}

		static bool SupportsComplianceFinancialYear(string countryCode)
		{
			var cfy = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode) as IInstanceProvider<IComplianceFinancialYear>)?.Get();
			return cfy != null;
		}

		public static class ReportPeriodicityCodes
		{
			public const string AccountingPeriod = "PER";
			public const string CalendarMonth = "MNT";
			public const string DateRange = "RNG";
			public const string FinancialYear = "FYR";
			public const string ComplianceFinancialYear = "CFY";
			public const string RangeAccountingPeriod = "PRS";
			public const string MonthlyQuarterlyYearly = "MQY";
		}

		#endregion

		#region Report Base Table Prefix List

		public CodeDescriptionPairList ReportBaseTablePrefixList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ReportBaseTablePrefixListCodes.TransactionHeader, Res.GetString("38f44fe6-0317-49ac-bfbf-a9d6c02b002a", "Transaction Header"));
				result.AddPair(ReportBaseTablePrefixListCodes.TransactionLine, Res.GetString("94fd582e-db9a-4ba8-ad1b-47dec2bb34c2", "Transaction Line"));
				result.AddPair(ReportBaseTablePrefixListCodes.ComplianceDocumentHeader, Res.GetString("5cfcf58c-d526-4723-8cac-6743e74da81d", "Compliance Document Header"));
				result.AddPair(ReportBaseTablePrefixListCodes.AllTransactions, Res.GetString("2bdda03d-d2b5-4ae3-a0cf-9b37b9f6431d", "All Transaction"));
				result.AddPair(ReportBaseTablePrefixListCodes.GeneralLedgerData, Res.GetString("8550859e-17cb-4fe2-b49d-7a794700c66c", "General Ledger Data"));
				return result;
			}
		}

		public static class ReportBaseTablePrefixListCodes
		{
			public const string TransactionHeader = "AH";
			public const string TransactionLine = "AL";
			public const string AllTransactions = "**";
			public const string ComplianceDocumentHeader = "ADH";
			public const string GeneralLedgerData = "GLD";
		}

		#endregion

		#region Report Line Grouping List

		public CodeDescriptionPairList ReportLineGroupingList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ReportLineGroupingListCodes.NoGrouping, Res.GetString("856062be-356d-44db-9230-6b9f016f8365", "No Grouping"));

				if (Parent.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.AllTransactions || Parent.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData)
				{
					result.AddPair(ReportLineGroupingListCodes.DayBook, Res.GetString("1819cb94-e330-4e8a-a8ed-9f38ff2392e0", "Day Book Grouping"));
					result.AddPair(ReportLineGroupingListCodes.DayBookWithoutGrouping, Res.GetString("ae5e6f33-32a1-4a00-b793-f4fa601fb5d2", "Day Book without Grouping"));
					result.AddPair(ReportLineGroupingListCodes.DayBookWithPresentation, Res.GetString("e9366a34-cc78-433d-9b6b-634b068538e5", "Day Book with Presentation Journals"));
				}
				else if (Parent.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.TransactionHeader || Parent.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.TransactionLine)
				{
					result.AddPair(ReportLineGroupingListCodes.TaxReporting, Res.GetString("dcf18633-5322-4734-bfe3-fd56616fb7fd", "By Tax Reporting Details"));
					result.AddPair(ReportLineGroupingListCodes.TransactionHeader, Res.GetString("95d93f9e-8f62-4191-bdfd-914312b7c65e", "By Transaction Header"));
					result.AddPair(ReportLineGroupingListCodes.TransactionHeaderWithLines, Res.GetString("19bcfbf7-c76c-494a-ae69-0b74de9e515e", "By Transaction Header with Lines"));
					result.AddPair(ReportLineGroupingListCodes.Organisation, Res.GetString("7e1ff0ad-cfdd-4d26-a438-15c65f525939", "By Organization"));
					result.AddPair(ReportLineGroupingListCodes.OrganisationBLCode, Res.GetString("2f2cf30c-05be-44d6-9a71-9f695a16c706", "By Organization and Italy BL Code"));
					result.AddPair(ReportLineGroupingListCodes.OrganisationSubCode, Res.GetString("909a8c31-0db0-4a28-994c-a8166a0cb8ca", "By Organization and Report Sub Code"));
					result.AddPair(ReportLineGroupingListCodes.DayBook, Res.GetString("1819cb94-e330-4e8a-a8ed-9f38ff2392e0", "Day Book Grouping"));
					result.AddPair(ReportLineGroupingListCodes.DayBookWithoutGrouping, Res.GetString("ae5e6f33-32a1-4a00-b793-f4fa601fb5d2", "Day Book without Grouping"));
					result.AddPair(ReportLineGroupingListCodes.DayBookWithPresentation, Res.GetString("e9366a34-cc78-433d-9b6b-634b068538e5", "Day Book with Presentation Journals"));
					result.AddPair(ReportLineGroupingListCodes.TransactionPayments, Res.GetString("75966a10-0bd9-44dc-a45e-c7245d80b617", "Transaction Payments"));
					result.AddPair(ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable, Res.GetString("faa95821-a958-4165-abbd-77f928612e35", "Payment Times - Small Business Reportable"));
					result.AddPair(ReportLineGroupingListCodes.PaymentTimesAll, Res.GetString("fedc498b-37bc-493a-949e-9ccc01a3f8de", "Payment Times - All"));
					result.AddPair(ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode, Res.GetString("29189F86-0CB4-4C03-B42F-766842CEFE2A", "Transaction Header and Report Sub Code"));
					result.AddPair(ReportLineGroupingListCodes.ReportSubCodeAndTransactionHeader, Res.GetString("3EBBDD79-61B1-4D33-BEDD-E161DF8A535C", "Report Sub Code and Transaction Header"));
				}

				return result;
			}
		}

		public static class ReportLineGroupingListCodes
		{
			public const string NoGrouping = "";
			public const string TaxReporting = "TXR";
			public const string TransactionHeader = "HDR";
			public const string TransactionHeaderWithLines = "HDL";
			public const string Organisation = "ORG";
			public const string OrganisationBLCode = "OBL";
			public const string OrganisationSubCode = "ORS";
			public const string DayBook = "DAB";
			public const string DayBookWithoutGrouping = "DBW";
			public const string DayBookWithPresentation = "DBP";
			public const string TransactionPayments = "TPA";
			public const string PaymentTimesSmallBusinessReportable = "PTR";
			public const string PaymentTimesAll = "PTA";
			public const string TransactionHeaderAndReportSubCode = "HRS";
			public const string ReportSubCodeAndTransactionHeader = "RSH";
		}

		#endregion

		#region ReportLineOrderingList

		public CodeDescriptionPairList ReportLineOrderingList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (Parent.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.ComplianceDocumentHeader)
				{
					if (Parent.Country == CountryCodes.Taiwan)
					{
						result.AddPair(ReportLineOrderingListCodes.FormatCodeAndDocumentNumber, Res.GetString("ee77b236-51d0-4ff8-8593-c35b74041678", "By Format Code and Document Number"));
					}
					result.AddPair(ReportLineOrderingListCodes.ComplianceDocumentNumber, Res.GetString("5059b61f-6711-44e7-836a-d6340af5e644", "By Compliance Document Number"));
				}
				else
				{
					result.AddPair(ReportLineOrderingListCodes.LedgerAscendinging, Res.GetString("3e3bf4ae-f05b-4919-be80-34b69dce9f62", "By Ledger Ascending"));
					result.AddPair(ReportLineOrderingListCodes.LedgerDescendinging, Res.GetString("2638986e-4648-4cab-a14d-fd0a10d18c08", "By Ledger Descending"));
					result.AddPair(ReportLineOrderingListCodes.ComplianceSubType, Res.GetString("af728e77-7f7f-412e-a9a6-626e65525e8c", "By Compliance Sub Type"));
					result.AddPair(ReportLineOrderingListCodes.Organisation, Res.GetString("a8764d5a-18ee-4490-95e1-ca68c629a696", "By Organization"));
				}
				return result;
			}
		}

		public static class ReportLineOrderingListCodes
		{
			public const string LedgerAscendinging = "LAS";
			public const string LedgerDescendinging = "LDS";
			public const string ComplianceSubType = "CST";
			public const string FormatCodeAndDocumentNumber = "FDN";
			public const string ComplianceDocumentNumber = "CDN";
			public const string Organisation = "ORG";
		}

		#endregion

		#region ReportAmountsRoundingTypeList

		public CodeDescriptionPairList ReportAmountsRoundingTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ReportAmountsRoundingTypeListCodes.NoRoundingOrTruncating, Res.GetString("574f497c-db01-4171-9a3a-ed6c737b764e", "No Rounding or Truncating"));
				result.AddPair(ReportAmountsRoundingTypeListCodes.Rounding, Res.GetString("8ffc6206-03ff-497c-996b-4cd1c3c79c8a", "Rounding"));
				result.AddPair(ReportAmountsRoundingTypeListCodes.Truncating, Res.GetString("e0b1c9c2-3c38-4ce0-838c-fea9f85c91d5", "Truncating"));
				return result;
			}
		}

		public static class ReportAmountsRoundingTypeListCodes
		{
			public const string NoRoundingOrTruncating = "";
			public const string Rounding = "RND";
			public const string Truncating = "TRN";
		}

		#endregion

		#region Goods Service Type List

		public CodeDescriptionPairList GoodsServiceTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(GoodsServiceTypeCodes.GoodsAndService, Res.GetString("b812cc56-6d8f-45fb-84f5-bd7413caf091", "Goods and Service"));
				result.AddPair(GoodsServiceTypeCodes.GoodsOnly, Res.GetString("1ddbf7bd-126e-452b-8d23-ac13ce19020a", "Goods Only"));
				result.AddPair(GoodsServiceTypeCodes.ServiceOnly, Res.GetString("9d007587-8a86-4d34-9a2c-4d4d427dff21", "Service Only"));
				return result;
			}
		}

		public static class GoodsServiceTypeCodes
		{
			public const string GoodsAndService = "";
			public const string GoodsOnly = "GDS";
			public const string ServiceOnly = "SRV";
		}

		#endregion

		#region RecipientOrgs

		public OrgHeaderCollection RecipientOrgs
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Amount Threshold Level

		public CodeDescriptionPairList AmountThresholdLevelList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(AmountThresholdLevelListCodes.NoThreshold, Res.GetString("4b549190-c5de-410c-b3f8-2111fa6b11ed", "No Threshold"));
				result.AddPair(AmountThresholdLevelListCodes.TransactionHeader, Res.GetString("2b2c47e1-2880-4933-9643-00b3f2f1065c", "By Transaction Header"));
				result.AddPair(AmountThresholdLevelListCodes.Organisation, Res.GetString("f518febf-ff11-43dc-ab00-73bce30bcc6e", "By Organization"));
				return result;
			}
		}

		public static class AmountThresholdLevelListCodes
		{
			public const string NoThreshold = "";
			public const string TransactionHeader = "HDR";
			public const string Organisation = "ORG";
		}

		#endregion
	}
}
