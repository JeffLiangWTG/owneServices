using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceReportTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceReportTypeCollection()
		{
			CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		public ComplianceReportTypeCollection(ZString countryCode)
			: this()
		{
			AddDefaultReportTypes(countryCode);
		}

		public new ComplianceReportType this[int i]
		{
			get { return (ComplianceReportType)Elements[i]; }
		}

		public ComplianceReportType this[ZString reportType]
		{
			get
			{
				return this.Cast<ComplianceReportType>().FirstOrDefault(element => element.ReportType == reportType && element.CountryCode == CountryCode);
			}
		}

		public new ComplianceReportType AddNew()
		{
			return (ComplianceReportType)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceReportTypeCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceReportType();
		}

#if DEBUG
		public
#endif
 ZString CountryCode
		{ get; set; }

		public ZString CodesAsString
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				foreach (ComplianceReportType element in this)
				{
					foreach (ComplianceReportsSetupCategory el in element.ReportTypeCategories)
					{
						builder.Append(element.ReportType + "." + el.Category);
					}
				}
				return builder.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public ZString GetDescriptionFromCode(string reportType)
		{
			string trimmedCode = reportType.Trim();
			foreach (ComplianceReportType element in
				this.Cast<ComplianceReportType>().Where(element => String.Equals(element.ReportType.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase)))
			{
				return element.ReportTypeDescription;
			}
			return ZString.Empty;
		}

		public ComplianceReportsSetupCategoryCollection GetReportTypeCategoriesFromCode(string reportType)
		{
			string trimmedCode = reportType.Trim();
			foreach (ComplianceReportType element in
				this.Cast<ComplianceReportType>().Where(element => String.Equals(element.ReportType.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase)))
			{
				return element.ReportTypeCategories;
			}
			return new ComplianceReportsSetupCategoryCollection();
		}

		public void AddDefaultReportTypes(ZString countryCode)
		{
			if (!countryCode.IsEmpty)
			{
				CountryCode = countryCode;
			}

			switch (CountryCode)
			{
#if DEBUG
				case "Test0":
					ComplianceReportType testComplianceReportType = AddNew();
					testComplianceReportType.AddDefaultReportTypes(
							AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet,
							"Balance Sheet",
							new ComplianceReportsSetupCategoryCollection(AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet, "Test0"),
							"Test0");
					break;
#endif

				default:

					foreach (var reportType in DefaultReportTypes)
					{
						ComplianceReportType complianceReportType = AddNew();
						complianceReportType.AddDefaultReportTypes(
							reportType.Key,
							reportType.Value,
							new ComplianceReportsSetupCategoryCollection(reportType.Key, CountryCode),
							CountryCode);
					}
					break;
			}
		}

		Dictionary<string, string> defaultReportTypes;
		internal Dictionary<string, string> DefaultReportTypes
		{
			get
			{
				if (defaultReportTypes == null)
				{
					defaultReportTypes = new Dictionary<string, string>();
					switch (CountryCode)
					{
						case Constants.CountryCodes.China:
						case Constants.Languages.ChineseSimplified:

							defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.BalanceSheet);
							defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.ProfitAndLoss);
							defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.ProfitAndLossMonthly);
							defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.VATDetailed);
							defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.AssetProvision);
							defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.P_LAppropriation);
							defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.EquityMovement);

#if DEBUG
							defaultReportTypes.Add("TT0", "Test Type 0");
#endif
							break;
					}
				}
				return defaultReportTypes;
			}
		}

		internal string DefaultReportTypesAsString
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				foreach (var key in DefaultReportTypes.Keys)
				{
					builder.Append(key);
				}

				return builder.ToStringWithDelimiterBetweenAppends(", ");
			}
		}
	}
}
