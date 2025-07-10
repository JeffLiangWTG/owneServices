using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceReportsSetupCategoryCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceReportsSetupCategoryCollection() { }

		public ComplianceReportsSetupCategoryCollection(string reportType, string countryCode)
		{
			AddDefaultsReportCategories(reportType, countryCode);
		}

		public string ReportType { get; set; }

		public void AddDefaultsReportCategories(string reportType, string countryCode)
		{
			ZDecimal sequence = 1m;
			ZBool allowDuplicate = false;
			RemoveAll();
			ReportType = reportType;

			switch (countryCode)
			{
#if DEBUG
				case "Test0":
					Add(new ComplianceReportsSetupCategory("A01", "Balance Sheet1", 0, allowDuplicate));
					Add(new ComplianceReportsSetupCategory("A02", "Balance Sheet2", 0, allowDuplicate));
					Add(new ComplianceReportsSetupCategory("A03", "Balance Sheet3", 0, allowDuplicate));

					break;
				case "Test1":
					Add(new ComplianceReportsSetupCategory("A01", "Balance Sheet1", 0, allowDuplicate));
					Add(new ComplianceReportsSetupCategory("B02", "Balance Sheet2", 0, allowDuplicate));
					Add(new ComplianceReportsSetupCategory("A03", "Balance Sheet3", 0, allowDuplicate));
					break;
#endif
				case Constants.CountryCodes.China:
				case Constants.Languages.ChineseSimplified:
					switch (ReportType)
					{
#if DEBUG
						case "TT0":
							Add(new ComplianceReportsSetupCategory("A01", "Balance Sheet1", 0, true));
							Add(new ComplianceReportsSetupCategory("A02", "Balance Sheet2", 1, false));
							Add(new ComplianceReportsSetupCategory("A03", "Balance Sheet3", 2, true));
							Add(new ComplianceReportsSetupCategory("A04", "Balance Sheet4", 3, false));
							break;
						case "TT1":
							Add(new ComplianceReportsSetupCategory("A01", "Balance Sheet1", 0, true));
							Add(new ComplianceReportsSetupCategory("B02", "Balance Sheet2", 1, false));
							Add(new ComplianceReportsSetupCategory("A03", "Balance Sheet3", 2, true));
							Add(new ComplianceReportsSetupCategory("A04", "Balance Sheet4", 3, false));
							break;
#endif
						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet:

							BalanceSheet_China balanceSheetChina = new BalanceSheet_China();

							foreach (ICodeDescription cd in balanceSheetChina)
							{
								sequence++;
								allowDuplicate = false;
								switch (cd.Code)
								{
									case BalanceSheet_China.Codes.D10:
									case BalanceSheet_China.Codes.D11:
									case BalanceSheet_China.Codes.D12:
									case BalanceSheet_China.Codes.D37:
									case BalanceSheet_China.Codes.D38:
									case BalanceSheet_China.Codes.D45:
									case BalanceSheet_China.Codes.D46:
									case BalanceSheet_China.Codes.H07:
									case BalanceSheet_China.Codes.H08:
									case BalanceSheet_China.Codes.H09:
									case BalanceSheet_China.Codes.H10:
									case BalanceSheet_China.Codes.H11:
									case BalanceSheet_China.Codes.H37:
									case BalanceSheet_China.Codes.H38:
									case BalanceSheet_China.Codes.H43:
									case BalanceSheet_China.Codes.H44:
									case BalanceSheet_China.Codes.H45:
									case BalanceSheet_China.Codes.H53:
									case BalanceSheet_China.Codes.H54:
										allowDuplicate = true;
										break;
									case BalanceSheet_China.Codes.D20:
										sequence = 18m;
										break;
									case BalanceSheet_China.Codes.D30:
										sequence = 28m;
										break;
									case BalanceSheet_China.Codes.D32:
										sequence = 30m;
										break;
									case BalanceSheet_China.Codes.H01:
										sequence = 55m;
										break;
									case BalanceSheet_China.Codes.H30:
										sequence = 73m;
										break;
									case BalanceSheet_China.Codes.H41:
										sequence = 86m;
										break;
									case BalanceSheet_China.Codes.H57:
										sequence = 102m;
										break;
									case BalanceSheet_China.Codes.H59:
										sequence = 104m;
										break;
								}
								Add(new ComplianceReportsSetupCategory(cd.Code, cd.Description, sequence, allowDuplicate));
							}

							break;

						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision:
							AssetProvision_China assetProvisionChina = new AssetProvision_China();
							foreach (ICodeDescription cd in assetProvisionChina)
							{
								sequence = Convert.ToInt32(cd.Code.Substring(1, 2));
								Add(new ComplianceReportsSetupCategory(cd.Code, cd.Description, sequence, allowDuplicate));
							}

							break;

						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.CashFlowStatement:

							break;
						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement:
							StatementOfShareholdersEquity sSEquity = new StatementOfShareholdersEquity();
							foreach (ICodeDescription cd in sSEquity)
							{
								sequence = Convert.ToInt32(cd.Code.Substring(1, 2));
								allowDuplicate = sequence >= 49 && sequence <= 66;
								Add(new ComplianceReportsSetupCategory(cd.Code, cd.Description, sequence, allowDuplicate));
							}
							break;
						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss:

							ProfitAndLoss_China profitAndLossChina = new ProfitAndLoss_China();
							foreach (ICodeDescription cd in profitAndLossChina)
							{
								allowDuplicate = false;
								switch (cd.Code)
								{
									case ProfitAndLoss_China.Codes.D09:
									case ProfitAndLoss_China.Codes.D10:
									case ProfitAndLoss_China.Codes.D12:
									case ProfitAndLoss_China.Codes.D13:
									case ProfitAndLoss_China.Codes.D14:
									case ProfitAndLoss_China.Codes.D15:

									case ProfitAndLoss_China.Codes.H22:
									case ProfitAndLoss_China.Codes.H23:
									case ProfitAndLoss_China.Codes.H24:
									case ProfitAndLoss_China.Codes.H25:
									case ProfitAndLoss_China.Codes.H26:
									case ProfitAndLoss_China.Codes.H27:
									case ProfitAndLoss_China.Codes.H28:
									case ProfitAndLoss_China.Codes.H29:
									case ProfitAndLoss_China.Codes.H30:
										allowDuplicate = true;
										break;
								}
								Add(new ComplianceReportsSetupCategory(cd.Code, cd.Description, sequence, allowDuplicate));
								sequence++;
							}
							break;

						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly:

							ProfitAndLossMonthly_China profitAndLossMonthlyChina = new ProfitAndLossMonthly_China();
							foreach (ICodeDescription cd in profitAndLossMonthlyChina)
							{
								sequence = Convert.ToInt32(cd.Code.Substring(1, 2));
								Add(new ComplianceReportsSetupCategory(cd.Code, cd.Description, sequence, allowDuplicate));
							}
							break;
						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed:
							VATDetailedReport_China vATDetailedReportChina = new VATDetailedReport_China();
							foreach (ICodeDescription cd in vATDetailedReportChina)
							{
								sequence = Convert.ToInt32(cd.Code.Substring(1, 2));
								Add(new ComplianceReportsSetupCategory(cd.Code, cd.Description, sequence, allowDuplicate));
							}
							break;

						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation:
							ProfitAndLossAppropriation profitAndLossAppropriation = new ProfitAndLossAppropriation();
							foreach (ICodeDescription cd in profitAndLossAppropriation)
							{
								sequence = Convert.ToInt32(cd.Code.Substring(1, 2));
								Add(new ComplianceReportsSetupCategory(cd.Code, cd.Description, sequence, allowDuplicate));
							}
							break;

						case AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount:
							Add(new ComplianceReportsSetupCategory(AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet, Res.GetString("8e6e53dd-5bd2-40a4-b8c3-dc24144ebff9", "Balance Sheet"), 0, false));
							Add(new ComplianceReportsSetupCategory(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss, Res.GetString("37e7c016-6de8-4a68-b422-2bd779773f7c", "Profit And Loss"), 0, false));
							Add(new ComplianceReportsSetupCategory(AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation, Res.GetString("f841c250-204d-4dd4-acdb-ce6493ad6278", "P&L Appropriation"), 0, false));
							Add(new ComplianceReportsSetupCategory(AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision, Res.GetString("755395f5-7e54-4d24-a2d7-a91df67470ff", "Asset Provision"), 0, false));
							Add(new ComplianceReportsSetupCategory(AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement, Res.GetString("b3bc8e5a-7747-4ef8-9e1c-bc98e07162f7", "Equity Movement"), 0, false));
							Add(new ComplianceReportsSetupCategory(AccountingMasterFilesConstants.ReportCodeOfLocalReport.CashFlowStatement, Res.GetString("506753b0-f6de-4a32-a42d-26f26d9c75d5", "Cash Flow Statement"), 0, false));
							break;
					}
					break;
			}

			Add(new ComplianceReportsSetupCategory(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, AccountingMasterFilesConstants.DefaultReportCategory.UndefinedDescription, 0, false));
		}

		public ZString CodesAsString
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				foreach (ComplianceReportsSetupCategory element in this)
				{
					builder.Append(element.Category);
				}
				return builder.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public bool ContainsCategory(string category)
		{
			string trimmedCategory = category.Trim();
			return this.Cast<ComplianceReportsSetupCategory>().Any(element => String.Equals(element.Category.Trim(), trimmedCategory, StringComparison.OrdinalIgnoreCase));
		}

		public ZString GetDescriptionFromCode(ZString category)
		{
			string trimmedCode = category.Trim();
			foreach (ComplianceReportsSetupCategory element in this)
			{
				if (String.Equals(element.Category.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return element.CategoryDescription;
				}
			}
			return ZString.Empty;
		}

		public new ComplianceReportsSetupCategory this[int i]
		{
			get { return (ComplianceReportsSetupCategory)Elements[i]; }
		}

		public ComplianceReportsSetupCategory this[string category]
		{
			get
			{
				string trimmedCode = category.Trim();
				return this.Cast<ComplianceReportsSetupCategory>().FirstOrDefault(element => String.Equals(element.Category.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase));
			}
		}

		public new ComplianceReportsSetupCategory AddNew()
		{
			return (ComplianceReportsSetupCategory)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceReportsSetupCategoryCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceReportsSetupCategory();
		}

		protected override bool AllowSort => false;
	}
}
