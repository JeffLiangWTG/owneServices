using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccAlternateGLAccountDissectionLookups;

namespace Enterprise.MasterFiles.Business
{
	public static class AccountingMasterFilesConstants
	{
		public static class ReportCodeOfLocalReport
		{
			public const string BalanceSheet = "BSH";
			public const string ProfitAndLoss = "P&L";
			public const string ProfitAndLossMonthly = "PLM";
			public const string P_LAppropriation = "PLA";
			public const string AssetProvision = "SPA";
			public const string EquityMovement = "SSE";
			public const string CashFlowStatement = "CFS";
			public const string ChartOfAccount = "COA";
			public const string VATDetailed = "VAT";
		}

		public static class ReportDescriptionsOfLocalReport
		{
			public static MultilingualString BalanceSheet = ResString.GetMultilingualString("8e6e53dd-5bd2-40a4-b8c3-dc24144ebff9", "Balance Sheet");
			public static MultilingualString ProfitAndLoss = ResString.GetMultilingualString("17937D28-DB3E-4AD1-9A1C-CD3A2B6E68C2", "Profit And Loss for Year");
			public static MultilingualString ProfitAndLossMonthly = ResString.GetMultilingualString("E4B138CB-9FFC-4949-A8E3-507FA4A6D8F0", "Profit And Loss for Monthly");
			public static MultilingualString P_LAppropriation = ResString.GetMultilingualString("27071E55-559F-4B10-82DB-31050058DA7A", "Profit and Loss Appropriation");
			public static MultilingualString AssetProvision = ResString.GetMultilingualString("e55395f5-7e54-4d24-a2d7-a91df67470fe", "Statement of Provision for Impairments of Asset");
			public static MultilingualString EquityMovement = ResString.GetMultilingualString("52A22971-F528-4F3B-A8B5-8BE98F2E2572", "Statement of Shareholders' Equity");
			public static MultilingualString CashFlowStatement = ResString.GetMultilingualString("506753b0-f6de-4a32-a42d-26f26d9c75d5", "Cash Flow Statement");
			public static MultilingualString ChartOfAccount = ResString.GetMultilingualString("456753b0-f6de-4a32-a42d-26f26d9c7685", "Chart of Account");
			public static MultilingualString VATDetailed = ResString.GetMultilingualString("92E5A88A-3108-40FD-958D-54DB73CB6D3A", "VAT Detailed Report");
		}

		public static class DefaultReportCategory
		{
			public const string Undefined = "XXX";
			public static string UndefinedDescription
			{
				get { return Res.GetString("5042a738-0241-4324-9c83-a396c3211ff0", "Undefined Category"); }
			}
		}

		public static CodeDescriptionPairList SupplyTypeClassificationList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(SupplyTypeClassificationCodes.LOC, ResString.GetMultilingualString("22d0edab-2b6a-49df-a893-05812d7bc877", "Local Service"));
				lookUpList.AddPair(SupplyTypeClassificationCodes.LOX, ResString.GetMultilingualString("25d1807a-b79d-46f9-bbc6-6d9c64d89c04", "Local Service provided by 3rd Party"));
				lookUpList.AddPair(SupplyTypeClassificationCodes.LOA, ResString.GetMultilingualString("2e7d0309-f65c-4d0a-9198-966a937c51fa", "Local Agent Service"));
				lookUpList.AddPair(SupplyTypeClassificationCodes.INT, ResString.GetMultilingualString("704c212d-62bb-4b87-a6a4-a2c341132e6e", "International Service"));
				lookUpList.AddPair(SupplyTypeClassificationCodes.INX, ResString.GetMultilingualString("29a74492-2c9c-47c8-892e-d851e6ef6a74", "International Service provided by 3rd Party"));
				lookUpList.AddPair(SupplyTypeClassificationCodes.INA, ResString.GetMultilingualString("a2af00bf-6e6e-48a0-b118-d1c78b60ae58", "International Agent Service"));
				lookUpList.AddPair(SupplyTypeClassificationCodes.DSB, ResString.GetMultilingualString("13befe3e-7d20-4565-8708-764c6d71ac53", "DSB - Disbursement/Reimbursement"));
				return lookUpList;
			}
		}

		public static class SupplyTypeClassificationCodes
		{
			public const string LOC = "LOC";
			public const string LOX = "LOX";
			public const string LOA = "LOA";
			public const string INT = "INT";
			public const string INX = "INX";
			public const string INA = "INA";
			public const string DSB = "DSB";
		}

		public class OrganisationTaxConfiguartionTypes : CodeDescriptionPairList
		{
			public OrganisationTaxConfiguartionTypes()
				: base()
			{
				Add(NotApplicable);
				Add(Default);
				Add(AccrualBasis);
				Add(CashBasis);
			}

			public static CodeDescriptionPair NotApplicable { get { return new CodeDescriptionPair(Constants.OrganisationTaxConfiguartionTypes.NotApplicable, ResString.GetMultilingualString("49819CEB-CEAA-4DA5-AEF3-C56ADECDA774", "Not applicable")); } }
			public static CodeDescriptionPair Default { get { return new CodeDescriptionPair(Constants.OrganisationTaxConfiguartionTypes.Default, ResString.GetMultilingualString("6525EA3B-E07A-4C05-BBA7-8A38BFD353E7", "Tax Is Applicable – Default Tax Recognition Rules Apply")); } }
			public static CodeDescriptionPair AccrualBasis { get { return new CodeDescriptionPair(Constants.OrganisationTaxConfiguartionTypes.AccrualBasis, ResString.GetMultilingualString("3507A5B5-8124-4706-9C6A-B6FD6BD50D0D", "Accrual Basis - All Tax for this Organization reported on Accrual Basis")); } }
			public static CodeDescriptionPair CashBasis { get { return new CodeDescriptionPair(Constants.OrganisationTaxConfiguartionTypes.CashBasis, ResString.GetMultilingualString("11B812E3-97F3-4221-BCF4-CCA5C8AB3457", "Cash Basis – All Tax for this Organization reported on Cash Basis")); } }
		}

		public class OrganisationCreateComplianceDocumentOnPostingTypes : CodeDescriptionPairList
		{
			public OrganisationCreateComplianceDocumentOnPostingTypes()
				: base()
			{
				Add(NotApplicable);
				Add(RollupByCharge);
				Add(PerComplianceDocumentNumber);
			}

			public static CodeDescriptionPair NotApplicable { get { return new CodeDescriptionPair(Constants.OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable, ResString.GetMultilingualString("B968B5B7-F85B-433D-A57B-E0130C02ABFB", "Not applicable")); } }
			public static CodeDescriptionPair RollupByCharge { get { return new CodeDescriptionPair(Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge, ResString.GetMultilingualString("78DD5044-B596-41D7-96D5-6C20B8578EB9", "Roll-up by Charge Code")); } }
			public static CodeDescriptionPair PerComplianceDocumentNumber { get { return new CodeDescriptionPair(Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber, ResString.GetMultilingualString("015E555C-1744-4CAA-A920-6A9837F80010", "Per Compliance Document Number")); } }
		}

		public class TransactionLineTaxBasisTypes : CodeDescriptionPairList
		{
			public TransactionLineTaxBasisTypes()
				: base()
			{
				Add(Accrual);
				Add(Cash);
			}

			public static CodeDescriptionPair Accrual { get { return new CodeDescriptionPair(Constants.TransactionLineTaxBasisTypes.Accrual, ResString.GetMultilingualString("4259E1C9-227E-4BCF-A06E-7E8AF6C03A5B", "Accrual")); } }
			public static CodeDescriptionPair Cash { get { return new CodeDescriptionPair(Constants.TransactionLineTaxBasisTypes.Cash, ResString.GetMultilingualString("34137B6C-E172-4D25-BBBB-9D86C77DF636", "Cash")); } }
		}

		public static class SignificantDateCodes
		{
			public const string ActualArrivalDate = "ARV";
			public const string ActualDepartureDate = "DEP";
			public const string EstimatedArrivalDate = "EAD";
			public const string EstimatedDepartureDate = "EDD";
			public const string AWBIssueDate = "AWB";
			public const string CustomsClearanceDate = "CUS";
			public const string DeliveryDate = "DEL";
			public const string PickupDate = "PIC";
			public const string VesselArrivalDate = "VAD";
			public const string VesselDepartureDate = "VDD";
			public const string HouseBillIssueDate = "HBD";
		}

		public static class DocReceivedDateDefaultLogics
		{
			public static class Code
			{
				public const string Blank = "BLK";
				public const string CreateDate = "CRE";
				public const string InvoiceDate = "INV";
			}

			public static class Descriptions
			{
				public static MultilingualString Blank { get { return ResString.GetMultilingualString("ADD49303-6A41-4D70-9DEB-4B445EB90302", "Blank"); } }
				public static MultilingualString CreateDate { get { return ResString.GetMultilingualString("4E3B4FA0-BCAD-44CE-B679-2C52C79906C0", "Create Date"); } }
				public static MultilingualString InvoiceDate { get { return ResString.GetMultilingualString("B552B19D-DEB8-47C0-BAA7-44C29F1DCA43", "Invoice Date"); } }
			}
		}

		public class SubAccountTypeList : CodeDescriptionPairList
		{
			public SubAccountTypeList()
				: base()
			{
				Add(Organization);
				Add(SalesGroup);
				Add(StaffAndResources);
				Add(StaffGroup);
			}

			public static CodeDescriptionPair Organization { get { return new CodeDescriptionPair(Constants.SubAccountType.Organization, Constants.SubAccountTypeDescriptions.Organization); } }
			public static CodeDescriptionPair SalesGroup { get { return new CodeDescriptionPair(Constants.SubAccountType.SalesGroup, Constants.SubAccountTypeDescriptions.SalesGroup); } }
			public static CodeDescriptionPair StaffAndResources { get { return new CodeDescriptionPair(Constants.SubAccountType.StaffAndResources, Constants.SubAccountTypeDescriptions.StaffAndResources); } }
			public static CodeDescriptionPair StaffGroup { get { return new CodeDescriptionPair(Constants.SubAccountType.StaffGroup, Constants.SubAccountTypeDescriptions.StaffGroup); } }
		}

		public const int MaxPossibleAuthorizationLevel = 3;

		public static class OrganisationTypeCodes
		{
			public const string All = "ALL";
			public const string LocalClient = "LOC";
			public const string OverseasAgent = "OGT";
			public const string Consignee = "CNE";
			public const string Consignor = "CNR";
			public const string SendingAgent = "SGT";
			public const string ReceivingAgent = "RGT";
			public const string Importer = "IMP";
			public const string Supplier = "SUP";
			public const string AllDebtors = "ADB";
			public const string ControllingCustomer = "CTP";
			public const string ControllingAgent = "CAG";
		}

		public static class OrganisationTypeDescriptions
		{
			public static MultilingualString All
			{
				get { return ResString.GetMultilingualString("49be996c-1883-4e21-ae01-8b462009d17b", "Any Organization Type"); }
			}
			public static string LocalClient
			{
				get { return Res.GetString("fb262c95-bf1e-4aef-98e9-1b8dc55a8b9c", "Local Client"); }
			}
			public static string OverseasAgent
			{
				get { return Res.GetString("bf233c94-1f40-48b0-8a1f-092057daa935", "Overseas Agent"); }
			}
			public static string Consignee
			{
				get { return Res.GetString("b73d97e4-6dc4-4c21-9d22-e8d673c9a7ce", "Consignee"); }
			}
			public static string Consignor
			{
				get { return Res.GetString("7997e683-d66a-4304-9358-306992b6e27e", "Consignor"); }
			}
			public static string SendingAgent
			{
				get { return Res.GetString("6ddaa4e8-5130-47d0-89d2-b36b0bb53ec9", "Sending Agent"); }
			}
			public static string ReceivingAgent
			{
				get { return Res.GetString("a7dc3750-3491-4475-9118-6c0de040d379", "Receiving Agent"); }
			}
			public static string Importer
			{
				get { return Res.GetString("b8f1c522-3008-455f-b0b3-e21bd4e6238c", "Importer"); }
			}
			public static string Supplier
			{
				get { return Res.GetString("24692e0d-6d12-4bed-9024-fc8fef8aa853", "Supplier"); }
			}
			public static string AllDebtors
			{
				get { return Res.GetString("04ba8815-231a-4813-9be1-7cc6c0706fa9", "All Debtors"); }
			}
			public static string ControllingCustomer
			{
				get { return Res.GetString("84D12433-51A9-4232-A1A2-2FEAF3122FE0", "Controlling Customer"); }
			}
			public static string ControllingAgent
			{
				get { return Res.GetString("B0BA3F1E-E4BD-44BD-A26B-B4E5EA5478CD", "Controlling Agent"); }
			}
		}

		public static class INCOTermCodes
		{
			public const string All = "ALL";
		}

		public static class INCOTermDescriptions
		{
			public static MultilingualString All
			{
				get { return ResString.GetMultilingualString("10A7D10B-E0C7-453B-8790-9D8922E499FD", "All Incoterms"); }
			}
		}

		public static class FreightPaymentTermCodes
		{
			public const string All = "ALL";
		}

		public static class FreightPaymentTermDescriptions
		{
			public static MultilingualString All
			{
				get { return ResString.GetMultilingualString("3A3817A7-91E3-454F-9A70-37B41BB77145", "All Payment Terms"); }
			}
		}

		public static CodeDescriptionPairList GetExchangeRateTypesList_SystemLevel() => new CodeDescriptionPairList(GetDefaultExchangeRateTypesList())
		{
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C01Rate, Constants.ExchangeRateTypes.Description.C01Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C02Rate, Constants.ExchangeRateTypes.Description.C02Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C03Rate, Constants.ExchangeRateTypes.Description.C03Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C04Rate, Constants.ExchangeRateTypes.Description.C04Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C05Rate, Constants.ExchangeRateTypes.Description.C05Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C06Rate, Constants.ExchangeRateTypes.Description.C06Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C07Rate, Constants.ExchangeRateTypes.Description.C07Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C08Rate, Constants.ExchangeRateTypes.Description.C08Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C09Rate, Constants.ExchangeRateTypes.Description.C09Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C10Rate, Constants.ExchangeRateTypes.Description.C10Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C11Rate, Constants.ExchangeRateTypes.Description.C11Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C12Rate, Constants.ExchangeRateTypes.Description.C12Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C13Rate, Constants.ExchangeRateTypes.Description.C13Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C14Rate, Constants.ExchangeRateTypes.Description.C14Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C15Rate, Constants.ExchangeRateTypes.Description.C15Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C16Rate, Constants.ExchangeRateTypes.Description.C16Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C17Rate, Constants.ExchangeRateTypes.Description.C17Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C18Rate, Constants.ExchangeRateTypes.Description.C18Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C19Rate, Constants.ExchangeRateTypes.Description.C19Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C20Rate, Constants.ExchangeRateTypes.Description.C20Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C21Rate, Constants.ExchangeRateTypes.Description.C21Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C22Rate, Constants.ExchangeRateTypes.Description.C22Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C23Rate, Constants.ExchangeRateTypes.Description.C23Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C24Rate, Constants.ExchangeRateTypes.Description.C24Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C25Rate, Constants.ExchangeRateTypes.Description.C25Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C26Rate, Constants.ExchangeRateTypes.Description.C26Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C27Rate, Constants.ExchangeRateTypes.Description.C27Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C28Rate, Constants.ExchangeRateTypes.Description.C28Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C29Rate, Constants.ExchangeRateTypes.Description.C29Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C30Rate, Constants.ExchangeRateTypes.Description.C30Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C31Rate, Constants.ExchangeRateTypes.Description.C31Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C32Rate, Constants.ExchangeRateTypes.Description.C32Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C33Rate, Constants.ExchangeRateTypes.Description.C33Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C34Rate, Constants.ExchangeRateTypes.Description.C34Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C35Rate, Constants.ExchangeRateTypes.Description.C35Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C36Rate, Constants.ExchangeRateTypes.Description.C36Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C37Rate, Constants.ExchangeRateTypes.Description.C37Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C38Rate, Constants.ExchangeRateTypes.Description.C38Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C39Rate, Constants.ExchangeRateTypes.Description.C39Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C40Rate, Constants.ExchangeRateTypes.Description.C40Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C41Rate, Constants.ExchangeRateTypes.Description.C41Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C42Rate, Constants.ExchangeRateTypes.Description.C42Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C43Rate, Constants.ExchangeRateTypes.Description.C43Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C44Rate, Constants.ExchangeRateTypes.Description.C44Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C45Rate, Constants.ExchangeRateTypes.Description.C45Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C46Rate, Constants.ExchangeRateTypes.Description.C46Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C47Rate, Constants.ExchangeRateTypes.Description.C47Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C48Rate, Constants.ExchangeRateTypes.Description.C48Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C49Rate, Constants.ExchangeRateTypes.Description.C49Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C50Rate, Constants.ExchangeRateTypes.Description.C50Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C51Rate, Constants.ExchangeRateTypes.Description.C51Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C52Rate, Constants.ExchangeRateTypes.Description.C52Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C53Rate, Constants.ExchangeRateTypes.Description.C53Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C54Rate, Constants.ExchangeRateTypes.Description.C54Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C55Rate, Constants.ExchangeRateTypes.Description.C55Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C56Rate, Constants.ExchangeRateTypes.Description.C56Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C57Rate, Constants.ExchangeRateTypes.Description.C57Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C58Rate, Constants.ExchangeRateTypes.Description.C58Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C59Rate, Constants.ExchangeRateTypes.Description.C59Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C60Rate, Constants.ExchangeRateTypes.Description.C60Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C61Rate, Constants.ExchangeRateTypes.Description.C61Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C62Rate, Constants.ExchangeRateTypes.Description.C62Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C63Rate, Constants.ExchangeRateTypes.Description.C63Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C64Rate, Constants.ExchangeRateTypes.Description.C64Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C65Rate, Constants.ExchangeRateTypes.Description.C65Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C66Rate, Constants.ExchangeRateTypes.Description.C66Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C67Rate, Constants.ExchangeRateTypes.Description.C67Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C68Rate, Constants.ExchangeRateTypes.Description.C68Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C69Rate, Constants.ExchangeRateTypes.Description.C69Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C70Rate, Constants.ExchangeRateTypes.Description.C70Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C71Rate, Constants.ExchangeRateTypes.Description.C71Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C72Rate, Constants.ExchangeRateTypes.Description.C72Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C73Rate, Constants.ExchangeRateTypes.Description.C73Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C74Rate, Constants.ExchangeRateTypes.Description.C74Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C75Rate, Constants.ExchangeRateTypes.Description.C75Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C76Rate, Constants.ExchangeRateTypes.Description.C76Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C77Rate, Constants.ExchangeRateTypes.Description.C77Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C78Rate, Constants.ExchangeRateTypes.Description.C78Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C79Rate, Constants.ExchangeRateTypes.Description.C79Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C80Rate, Constants.ExchangeRateTypes.Description.C80Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C81Rate, Constants.ExchangeRateTypes.Description.C81Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C82Rate, Constants.ExchangeRateTypes.Description.C82Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C83Rate, Constants.ExchangeRateTypes.Description.C83Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C84Rate, Constants.ExchangeRateTypes.Description.C84Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C85Rate, Constants.ExchangeRateTypes.Description.C85Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C86Rate, Constants.ExchangeRateTypes.Description.C86Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C87Rate, Constants.ExchangeRateTypes.Description.C87Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C88Rate, Constants.ExchangeRateTypes.Description.C88Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C89Rate, Constants.ExchangeRateTypes.Description.C89Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C90Rate, Constants.ExchangeRateTypes.Description.C90Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C91Rate, Constants.ExchangeRateTypes.Description.C91Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C92Rate, Constants.ExchangeRateTypes.Description.C92Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C93Rate, Constants.ExchangeRateTypes.Description.C93Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C94Rate, Constants.ExchangeRateTypes.Description.C94Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C95Rate, Constants.ExchangeRateTypes.Description.C95Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C96Rate, Constants.ExchangeRateTypes.Description.C96Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C97Rate, Constants.ExchangeRateTypes.Description.C97Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C98Rate, Constants.ExchangeRateTypes.Description.C98Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.C99Rate, Constants.ExchangeRateTypes.Description.C99Rate),
		};

		public static CodeDescriptionPairList GetDefaultExchangeRateTypesList() => new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.BuyRate, Constants.ExchangeRateTypes.Description.BuyRate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.SellRate, Constants.ExchangeRateTypes.Description.SellRate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate, Constants.ExchangeRateTypes.Description.CustomsMeasureEURExRate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.CustomsRate, Constants.ExchangeRateTypes.Description.CustomsRate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.CustomsRateSecondary, Constants.ExchangeRateTypes.Description.CustomsRateSecondary),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.GlobalCreditControl, Constants.ExchangeRateTypes.Description.GlobalCreditControl),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.PeriodEndRate, Constants.ExchangeRateTypes.Description.PeriodEndRate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.IATARate, Constants.ExchangeRateTypes.Description.IATARate)
		};

		public static CodeDescriptionPairList GetExchangeRateTypesList_CompanyLevel() => new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L01Rate, Constants.ExchangeRateTypes.Description.L01Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L02Rate, Constants.ExchangeRateTypes.Description.L02Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L03Rate, Constants.ExchangeRateTypes.Description.L03Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L04Rate, Constants.ExchangeRateTypes.Description.L04Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L05Rate, Constants.ExchangeRateTypes.Description.L05Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L06Rate, Constants.ExchangeRateTypes.Description.L06Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L07Rate, Constants.ExchangeRateTypes.Description.L07Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L08Rate, Constants.ExchangeRateTypes.Description.L08Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L09Rate, Constants.ExchangeRateTypes.Description.L09Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L10Rate, Constants.ExchangeRateTypes.Description.L10Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L11Rate, Constants.ExchangeRateTypes.Description.L11Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L12Rate, Constants.ExchangeRateTypes.Description.L12Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L13Rate, Constants.ExchangeRateTypes.Description.L13Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L14Rate, Constants.ExchangeRateTypes.Description.L14Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L15Rate, Constants.ExchangeRateTypes.Description.L15Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L16Rate, Constants.ExchangeRateTypes.Description.L16Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L17Rate, Constants.ExchangeRateTypes.Description.L17Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L18Rate, Constants.ExchangeRateTypes.Description.L18Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L19Rate, Constants.ExchangeRateTypes.Description.L19Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L20Rate, Constants.ExchangeRateTypes.Description.L20Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L21Rate, Constants.ExchangeRateTypes.Description.L21Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L22Rate, Constants.ExchangeRateTypes.Description.L22Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L23Rate, Constants.ExchangeRateTypes.Description.L23Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L24Rate, Constants.ExchangeRateTypes.Description.L24Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L25Rate, Constants.ExchangeRateTypes.Description.L25Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L26Rate, Constants.ExchangeRateTypes.Description.L26Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L27Rate, Constants.ExchangeRateTypes.Description.L27Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L28Rate, Constants.ExchangeRateTypes.Description.L28Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L29Rate, Constants.ExchangeRateTypes.Description.L29Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L30Rate, Constants.ExchangeRateTypes.Description.L30Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L31Rate, Constants.ExchangeRateTypes.Description.L31Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L32Rate, Constants.ExchangeRateTypes.Description.L32Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L33Rate, Constants.ExchangeRateTypes.Description.L33Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L34Rate, Constants.ExchangeRateTypes.Description.L34Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L35Rate, Constants.ExchangeRateTypes.Description.L35Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L36Rate, Constants.ExchangeRateTypes.Description.L36Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L37Rate, Constants.ExchangeRateTypes.Description.L37Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L38Rate, Constants.ExchangeRateTypes.Description.L38Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L39Rate, Constants.ExchangeRateTypes.Description.L39Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L40Rate, Constants.ExchangeRateTypes.Description.L40Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L41Rate, Constants.ExchangeRateTypes.Description.L41Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L42Rate, Constants.ExchangeRateTypes.Description.L42Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L43Rate, Constants.ExchangeRateTypes.Description.L43Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L44Rate, Constants.ExchangeRateTypes.Description.L44Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L45Rate, Constants.ExchangeRateTypes.Description.L45Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L46Rate, Constants.ExchangeRateTypes.Description.L46Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L47Rate, Constants.ExchangeRateTypes.Description.L47Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L48Rate, Constants.ExchangeRateTypes.Description.L48Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L49Rate, Constants.ExchangeRateTypes.Description.L49Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L50Rate, Constants.ExchangeRateTypes.Description.L50Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L51Rate, Constants.ExchangeRateTypes.Description.L51Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L52Rate, Constants.ExchangeRateTypes.Description.L52Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L53Rate, Constants.ExchangeRateTypes.Description.L53Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L54Rate, Constants.ExchangeRateTypes.Description.L54Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L55Rate, Constants.ExchangeRateTypes.Description.L55Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L56Rate, Constants.ExchangeRateTypes.Description.L56Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L57Rate, Constants.ExchangeRateTypes.Description.L57Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L58Rate, Constants.ExchangeRateTypes.Description.L58Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L59Rate, Constants.ExchangeRateTypes.Description.L59Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L60Rate, Constants.ExchangeRateTypes.Description.L60Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L61Rate, Constants.ExchangeRateTypes.Description.L61Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L62Rate, Constants.ExchangeRateTypes.Description.L62Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L63Rate, Constants.ExchangeRateTypes.Description.L63Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L64Rate, Constants.ExchangeRateTypes.Description.L64Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L65Rate, Constants.ExchangeRateTypes.Description.L65Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L66Rate, Constants.ExchangeRateTypes.Description.L66Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L67Rate, Constants.ExchangeRateTypes.Description.L67Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L68Rate, Constants.ExchangeRateTypes.Description.L68Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L69Rate, Constants.ExchangeRateTypes.Description.L69Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L70Rate, Constants.ExchangeRateTypes.Description.L70Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L71Rate, Constants.ExchangeRateTypes.Description.L71Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L72Rate, Constants.ExchangeRateTypes.Description.L72Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L73Rate, Constants.ExchangeRateTypes.Description.L73Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L74Rate, Constants.ExchangeRateTypes.Description.L74Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L75Rate, Constants.ExchangeRateTypes.Description.L75Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L76Rate, Constants.ExchangeRateTypes.Description.L76Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L77Rate, Constants.ExchangeRateTypes.Description.L77Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L78Rate, Constants.ExchangeRateTypes.Description.L78Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L79Rate, Constants.ExchangeRateTypes.Description.L79Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L80Rate, Constants.ExchangeRateTypes.Description.L80Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L81Rate, Constants.ExchangeRateTypes.Description.L81Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L82Rate, Constants.ExchangeRateTypes.Description.L82Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L83Rate, Constants.ExchangeRateTypes.Description.L83Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L84Rate, Constants.ExchangeRateTypes.Description.L84Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L85Rate, Constants.ExchangeRateTypes.Description.L85Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L86Rate, Constants.ExchangeRateTypes.Description.L86Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L87Rate, Constants.ExchangeRateTypes.Description.L87Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L88Rate, Constants.ExchangeRateTypes.Description.L88Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L89Rate, Constants.ExchangeRateTypes.Description.L89Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L90Rate, Constants.ExchangeRateTypes.Description.L90Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L91Rate, Constants.ExchangeRateTypes.Description.L91Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L92Rate, Constants.ExchangeRateTypes.Description.L92Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L93Rate, Constants.ExchangeRateTypes.Description.L93Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L94Rate, Constants.ExchangeRateTypes.Description.L94Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L95Rate, Constants.ExchangeRateTypes.Description.L95Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L96Rate, Constants.ExchangeRateTypes.Description.L96Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L97Rate, Constants.ExchangeRateTypes.Description.L97Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L98Rate, Constants.ExchangeRateTypes.Description.L98Rate),
			new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.L99Rate, Constants.ExchangeRateTypes.Description.L99Rate),
		};

		public static class CreditorGroupConstants
		{
			public static string DefaultHoldOption => HoldOptionType.Codes.DNM;

			public static CodeDescriptionPairList HoldOptions
			{
				get
				{
					// Should NOT reuse HoldOptionType. Auto generated HoldOptionType has no PK for pair-elements comparison.
					var holdOptions = new CodeDescriptionPairList();
					holdOptions.Add(new CodeElement(HoldOptionType.Codes.DNM, HoldOptionType.Codes.DNM, HoldOptionType.Descriptions.DNM));
					holdOptions.Add(new CodeElement(HoldOptionType.Codes.ALM, HoldOptionType.Codes.ALM, HoldOptionType.Descriptions.ALM));

					return holdOptions;
				}
			}
		}

		public static class JobTypes
		{
			public const string NonJobRelated = "NJR";
		}

		public sealed class ComplianceNumberAllocationDateOptions : CodeDescriptionPairList
		{
			public ComplianceNumberAllocationDateOptions()
			{
				Add(PostDate);
				Add(InvoiceDate);
				Add(NoControl);
			}

			public static CodeDescriptionPair PostDate { get { return new CodeDescriptionPair("PST", ResString.GetMultilingualString("54E80B1B-309A-4F05-8D84-917E94EA60DF", "Post Date Order")); } }
			public static CodeDescriptionPair InvoiceDate { get { return new CodeDescriptionPair("INV", ResString.GetMultilingualString("853B10E5-E073-4FAC-9338-FAABFF608AF3", "Invoice Date Order")); } }
			public static CodeDescriptionPair NoControl { get { return new CodeDescriptionPair("NOT", ResString.GetMultilingualString("32599AA3-F09C-457D-B4D2-A1973160344C", "No Date Order Enforced")); } }
		}

		public static string ErrorMessageForInvalidDataIfChargeTypeIsComment
		{
			get { return Res.GetString("e1498330-f202-4b95-a80c-832e6515ce94", "This data is invalid. There is no sense in it if the charge type is 'CMT'"); }
		}

		public class CreditControlApprovalModes : CodeDescriptionPairList
		{
			public CreditControlApprovalModes()
			{
				Add(ApproveInExternalSystem);
				Add(ApproveLocallyInCW1);
			}

			public static CodeDescriptionPair ApproveInExternalSystem { get { return new CodeDescriptionPair("EXT", ResString.GetMultilingualString("EE8ECD25-5E80-437B-B5E4-7102293C83F7", "Submit Requests to External System")); } }
			public static CodeDescriptionPair ApproveLocallyInCW1 { get { return new CodeDescriptionPair("CW1", ResString.GetMultilingualString("77AD1D4E-2234-4196-B9CD-D2CE0997D66F", "Approve requests in CW1")); } }
		}

		public class TargetJobDefaultingOptions : CodeDescriptionPairList
		{
			public TargetJobDefaultingOptions()
			{
				Add(SameConsol);
				Add(RelatedShipmentNumber);
				Add(PreviousConsol);
			}

			public static CodeDescriptionPair SameConsol => new CodeDescriptionPair("SCL", ResString.GetMultilingualString("C6A1043E-F781-4FA4-8370-455B857CAC73", "Same Consol"));
			public static CodeDescriptionPair RelatedShipmentNumber => new CodeDescriptionPair("REL", ResString.GetMultilingualString("4CE72A9D-FDAA-49F1-9D9C-3D4EEDDFCD66", "Related Shipment Number"));
			public static CodeDescriptionPair PreviousConsol => new CodeDescriptionPair("PCL", ResString.GetMultilingualString("3F8547C3-441C-42CC-BE03-889DCF92E92C", "Previous Consol"));
		}

		public class JobDeactivationConfigurations : CodeDescriptionPairList
		{
			public JobDeactivationConfigurations()
			{
				Add(Default);
				Add(NoActiveWIPACR);
			}

			public static CodeDescriptionPair Default => new("DEF", ResString.GetMultilingualString("b9911ea6-ed77-43b6-93f7-426394ebcb27", "No accounting transaction has been posted"));

			public static CodeDescriptionPair NoActiveWIPACR => new("NPL", ResString.GetMultilingualString("928b8dc0-a4b3-4679-ad6f-fde218e3a9d5", "No active WIPs and ACRs"));
		}

		public static class ComplianceReportCodes
		{
			public const string ReportableSmallBusines = "RSB";
		}

		public sealed class TransactionLineNegativeAmountAllowedOnAccountReceivableModes : CodeDescriptionPairList
		{
			public TransactionLineNegativeAmountAllowedOnAccountReceivableModes()
			{
				Add(Allowed);
				Add(Disallowed);
				Add(NegativeChargesAllowed);
			}

			public static CodeDescriptionPair Allowed { get { return new CodeDescriptionPair("ALL", ResString.GetMultilingualString("c36c5ca8-40b4-43b4-8d32-7dcfe246adfd", "Negative Charges Allowed - Allows negative line amounts on all the AR transactions.")); } }
			public static CodeDescriptionPair Disallowed { get { return new CodeDescriptionPair("NAL", ResString.GetMultilingualString("7fc251c1-93f1-46f1-a3d2-0b5a85189544", "Negative Charges Not Allowed - Stops users from posting negative line amounts on AR Invoice, Credit Note or Adjustment Note transactions.")); } }
			public static CodeDescriptionPair NegativeChargesAllowed { get { return new CodeDescriptionPair("NOT", ResString.GetMultilingualString("B9E2220B-4D2F-4183-B6B8-5FB3850CD39A", "Negative Charges Allowed if the Debtor is Not Applicable to Tax")); } }
		}

		public static readonly string DefaultInvoiceTerm = "DEF";

		public static class InvoiceDateDefaultValueCodes
		{
			public const string CurrentDate = "ADD";
			public const string Blank = "BLK";
		}

		public static CodeDescriptionPairList InvoiceDateDefaultValueTypes
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(InvoiceDateDefaultValueCodes.CurrentDate, ResString.GetMultilingualString("B0762A78-51AD-49A3-B6F6-117AF5ED3876", "Invoice Add Date"));
				lookUpList.AddPair(InvoiceDateDefaultValueCodes.Blank, ResString.GetMultilingualString("6337B237-99A3-4C2D-81B4-AA1556917147", "Blank value"));
				return lookUpList;
			}
		}

		public static class TransactionNumberCodes
		{
			public const string ComplianceNr = "COM";
			public const string InvoiceNr = "INV";
			public const string ComplianceOrInvoiceNr = "CIN";
		}

		public class EReportingTransactionNumberOptions : CodeDescriptionPairList
		{
			public EReportingTransactionNumberOptions()
			{
				Add(ComplianceNr);
				Add(InvoiceNr);
				Add(ComplianceOrInvoiceNr);
			}

			public static CodeDescriptionPair ComplianceNr => new CodeDescriptionPair(TransactionNumberCodes.ComplianceNr, ResString.GetMultilingualString("45FDAE66-F627-4D3F-811E-DAD434E3AAE0", "Compliance Number"));
			public static CodeDescriptionPair InvoiceNr => new CodeDescriptionPair(TransactionNumberCodes.InvoiceNr, ResString.GetMultilingualString("B2C15D5F-08E3-4382-A7EA-7CEC53028915", "Invoice Number"));
			public static CodeDescriptionPair ComplianceOrInvoiceNr => new CodeDescriptionPair(TransactionNumberCodes.ComplianceOrInvoiceNr, ResString.GetMultilingualString("FDE2D41F-A93F-4888-A334-9633C53A77CF", "Compliance Number, fall back to Invoice Number"));
		}

		public static class EReportingGEIMessageSystemTypeCodes
		{
			public const string Default = "DEF";
			public const string AlwaysProductionSystem = "PRD";
			public const string AlwaysTestSystem = "TST";
		}

		public class EReportingGEIMessageSystemTypeOptions : CodeDescriptionPairList
		{
			public EReportingGEIMessageSystemTypeOptions()
			{
				Add(Default);
				Add(AlwaysProductionSystem);
				Add(AlwaysTestSystem);
			}

			public static CodeDescriptionPair Default => new CodeDescriptionPair(EReportingGEIMessageSystemTypeCodes.Default, ResString.GetMultilingualString("CD5F3D11-F5BC-4510-A128-3943A0DC654F", "Default - the system which GEI message sent to will be based on license code"));
			public static CodeDescriptionPair AlwaysProductionSystem => new CodeDescriptionPair(EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem, ResString.GetMultilingualString("8C48588B-4B34-4799-B855-29BA6A3586A1", "Always Production System - GEI message will be send to production system"));
			public static CodeDescriptionPair AlwaysTestSystem => new CodeDescriptionPair(EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, ResString.GetMultilingualString("D368F378-6F58-48E6-AAD0-6BB073CD2426", "Always Test System - GEI message will be send to test system"));
		}

		public class EReportingSubmitPivotDefaultStatusForPayablesOnlyItalyOptions : CodeDescriptionPairList
		{
			public EReportingSubmitPivotDefaultStatusForPayablesOnlyItalyOptions()
			{
				Add(Pending);
				Add(Queued);
			}

			public static CodeDescriptionPair Pending => new CodeDescriptionPair(Constants.EInvoicingPivotState.Pending, ResString.GetMultilingualString("0AB596D0-C61B-497B-852B-73F90DE5B84D", "Pending - Pending user action"));

			public static CodeDescriptionPair Queued => new CodeDescriptionPair(Constants.EInvoicingPivotState.Queued, ResString.GetMultilingualString("1DBACDE6-ECC5-47B0-9BEF-DCFE0D00235A", "Queued - Queued"));
		}

		public static class ApportionmentMethod
		{
			public const string AllCode = "ALL";

			public static class ModuleDescription
			{
				public static MultilingualString AllModulesDescription { get { return ResString.GetMultilingualString("495825fb-6603-4f0f-b6d2-c7577d8abce3", "Any Module"); } }
				public static MultilingualString ForwardingDescription { get { return ResString.GetMultilingualString("42e8e2a6-cc5f-4cd1-a5ad-f074f23c333e", "Forwarding"); } }
				public static MultilingualString TransportDescription { get { return ResString.GetMultilingualString("E6E78C93-7487-4853-A259-41ADA31C5B6F", "Transport"); } }
				public static MultilingualString TransitWarehouseDescription { get { return ResString.GetMultilingualString("1720f3ea-8304-4606-931a-b46ee6b57550", "Transit Warehouse"); } }
			}

			public static MultilingualString AllConsolTypeDescription => ResString.GetMultilingualString("6642c927-857a-48e5-b17e-79cc7189247b", "Any Consol Type");
			public static MultilingualString AllContainerModeDescription => ResString.GetMultilingualString("c0f95eef-b763-4f29-9bac-e81b638e7190", "Any Container Mode");
			public static MultilingualString AllTransportModeDescription => ResString.GetMultilingualString("12c9ed75-0ad4-4618-82f9-c1585b3c642e", "Any Transport Mode");
		}

		#region Transaction Header Reference Types

		public static class AccTransactionHeaderReferenceTypes 
		{
			public const string ITR = "ITR";
			public const string IRR = "IRR";
			public const string IRD = "IRD";
			public const string PIR = "PIR";
			public const string OTI = "OTI";
			public const string ATH = "ATH";
			public const string KRE = "KRE";
			public const string KRI = "KRI";
			public const string RED = "RED";
			public const string CDS = "CDS";
			public const string MXR = "MXR";
			public const string IVA = OrgCusCode.CodeTypes.IVA;
			public const string EINV_REVERSAL_CODE = "ERC";
			public const string ReceivableDisbursementInvoice = "RDI";
		}

		public class AccTransactionHeaderReferenceTypesList : CodeDescriptionPairList
		{
			public AccTransactionHeaderReferenceTypesList()
			{
				Add(ITR);
				Add(IRR);
				Add(IRD);
				Add(PIR);
				Add(OTI);
				Add(ATH);
				Add(KRE);
				Add(KRI);
				Add(RED);
				Add(CDS);
				Add(MXR);
				Add(IVA);
				Add(EINV_REVERSAL_CODE);
				Add(ReceivableDisbursementInvoice);
			}

			public static CodeDescriptionPair ITR => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.ITR, null);
			public static CodeDescriptionPair IRR => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.IRR, null);
			public static CodeDescriptionPair IRD => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.IRD, null);
			public static CodeDescriptionPair PIR => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.PIR, null);
			public static CodeDescriptionPair OTI => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.OTI, null);
			public static CodeDescriptionPair ATH => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.ATH, null);
			public static CodeDescriptionPair KRE => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.KRE, null);
			public static CodeDescriptionPair KRI => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.KRI, null);
			public static CodeDescriptionPair RED => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.RED, null);
			public static CodeDescriptionPair CDS => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.CDS, null);
			public static CodeDescriptionPair MXR => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.MXR, null);
			public static CodeDescriptionPair IVA => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.IVA, null);

			public static CodeDescriptionPair EINV_REVERSAL_CODE => new CodeDescriptionPair(
				AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE,
				ResString.GetMultilingualString("4E1D135D-712C-448E-9D30-17CE16AEECD2", "E-Invoicing Reversal Code"));

			public static CodeDescriptionPair ReceivableDisbursementInvoice => new CodeDescriptionPair(AccTransactionHeaderReferenceTypes.ReceivableDisbursementInvoice, null);
		}

		#endregion

		public static CodeDescriptionPair ReasonFreeTextCode =>
			new CodeDescriptionPair("TXT", ResString.GetMultilingualString("b7a6153c-e4ab-405f-aab0-316756799399", "Free Text"));

		public class ComplianceDocumentNumberAllocationRuleTypes : CodeDescriptionPairList
		{
			public static CodeDescriptionPair LBD
			{
				get { return new CodeDescriptionPair("LBD", ResString.GetMultilingualString("7750175A-3773-433F-B85A-88676E7B4522", "Compliance number allocation based on Current Login Branch and Department")); }
			}

			public static CodeDescriptionPair HBD
			{
				get { return new CodeDescriptionPair("HBD", ResString.GetMultilingualString("54D8A292-D9DF-4ADC-A10B-A6990713939F", "Compliance number allocation based on Transaction Header Branch and Department")); }
			}

			public ComplianceDocumentNumberAllocationRuleTypes()
			{
				Add(LBD);
				Add(HBD);
			}
		}

		public class AccOrgTaxConfigurationTemplateTypes : CodeDescriptionPairList
		{
			public AccOrgTaxConfigurationTemplateTypes()
				: base()
			{
				Add(ReceivablesOrganizationsTemplate);
				Add(PayablesOrganizationsTemplate);
			}

			public static CodeDescriptionPair ReceivablesOrganizationsTemplate { get { return new CodeDescriptionPair("A/R", ResString.GetMultilingualString("E16A21FB-BDBD-42A1-8D47-A20D3FFC75DD", "Receivables Organizations Template")); } }
			public static CodeDescriptionPair PayablesOrganizationsTemplate { get { return new CodeDescriptionPair("A/P", ResString.GetMultilingualString("E269BEC1-D8C6-41C4-9528-D65F091822F8", "Payables Organizations Template")); } }
		}

		public static class ReserveSurchargeCodes
		{
			public const string All = "ALL";
			public const string Non = "NON";
		}

		public static class OFXOAuthWebsiteURL
		{
			public const string ProductionURL = "https://live.api.ofx.com/v1/oauth/authorize";
			public const string TestingURL = "https://sandbox.api.ofx.com/v1/oauth/authorize";
		}

		public static class SAEInvoicingAPIEndPoints
		{
			public const string Production_CSID_URL = "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/compliance";
			public const string Testing_CSID_URL = "https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance";
			public const string Developer_CSID_URL = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/compliance";
			public const string Production_Onboarding_URL = "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/production/csids";
			public const string Testing_Onboarding_URL = "https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/production/csids";
		}

		public static class KoreaSouthRegistryNumber
		{
			public const string Testing = "12345678";

			public const string Production = "41000193";
		}

		public static string ChargeLocalCostAmountCannotBeZeroErrorMessage => Res.GetString("85f71299-f9ec-4277-bd6e-dc3dd4a91ebe", "Local amount cannot be zero when Overseas Cost Amount is non zero.");

		public static class ValidationErrorMessages
		{
			public static string EPaymentReferenceTypeCannotBeEmptyErrorMessage => Res.GetString("9EB32AB9-CAB4-4C58-B9FB-D0B962AF2277", "Payment Reference Type is Mandatory for E-Payments. Please select a Payment Reference Type.");
			public static string DuplicateCurrencyCodeForJobBillingExRateConfig => Res.GetString("70633018-F220-4A46-B94C-4708CFD57CF0", @"The same Currency Code already exists in the Job Billing Exchange Rate configuration.
Please check values for each parameter, make sure the currency lists don't overlap when you save multiple Job Exchange Rate Configurations with the same attributes (job type, transport mode, ledger, direction, and currency type).");
		}

		public static class TaxIDAndTaxMessageValidationOption
		{
			public const string TaxID = "IDS";
			public const string TaxMessage = "MSG";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Null code")]
		public static CodeDescriptionPair NullTaxRateType => new CodeDescriptionPair("<Null>", ResString.GetMultilingualString("365B7206-5B5F-4DEC-A482-E01749BFF3EB", "Non Tax Invoices"));

		public static CodeDescriptionPairList TaxRateTypes
		{
			get
			{
				var types = new CodeDescriptionPairList();
				types.AddPair(AccTaxRate.Types.Rated, Res.GetString("01c9ba95-ab8c-40be-a82b-64b289343d67", "Rated"));
				types.AddPair(AccTaxRate.Types.ReverseRated, Res.GetString("37c4c5a4-4405-41db-a776-cbc5b9076912", "Reverse Rated"));
				types.AddPair(AccTaxRate.Types.Exempt, Res.GetString("41387ff8-0687-4898-82af-416a57ad1a1d", "Exempt"));
				types.AddPair(AccTaxRate.Types.CapitalRated, Res.GetString("fd9ee717-ed35-4400-97a4-de3901fbb3ef", "Capital Rated"));
				types.AddPair(AccTaxRate.Types.NotReportable, Res.GetString("94494e73-6e00-4cd2-981b-b38acb24396e", "Not Reportable"));
				types.AddPair(AccTaxRate.Types.Suspended, Res.GetString("b32d9786-cbbe-4d84-bf6f-61214b2f3c51", "Suspended"));
				types.AddPair(AccTaxRate.Types.RatedInAnotherCountry, Res.GetString("6c6bf962-ac09-478d-b594-b35a34837b46", "Rated in Another Country/Region"));
				types.AddPair(AccTaxRate.Types.ReportableUnderBusinessTax, Res.GetString("d60c3c73-b28c-42ce-8136-617ac3cdf14c", "Not Reportable Business Tax"));
				types.AddPair(AccTaxRate.Types.ExcludedFromTheTaxBase, Res.GetString("4bd87645-ded9-4655-aaaa-7b81465d2fbd", "Excluded from the Tax Base"));
				types.AddPair(AccTaxRate.Types.IntegratedGST, Res.GetString("e504d57b-9537-4f65-948f-1ebc1b36cf7b", "Integrated GST"));
				types.AddPair(AccTaxRate.Types.ServiceTax, Res.GetString("3ab6aff1-cdcd-4850-bdfb-b4e023aa47e2", "Service Tax"));
				return types;
			}
		}

		public static class KoreaEInvoicingTypeCodeCategory
		{
			public const string TaxInvoice = "01XX/02XX";
			public const string Invoice = "03XX/04XX";
			public const string NotApplicable = "N/A";
		}

		public static class AutoJobRevenueJournalCodes
		{
			public const string Yes = "YES";
			public const string Tax = "TAX";
			public const string Non = "NON";
		}

		public class AutoJobRevenueJournalOptions : CodeDescriptionPairList
		{
			public AutoJobRevenueJournalOptions()
			{
				Add(Yes);
				Add(Tax);
				Add(Non);
			}

			public static CodeDescriptionPair Yes => new CodeDescriptionPair(AutoJobRevenueJournalCodes.Yes, ResString.GetMultilingualString("341F3E41-95FF-4CEB-96C0-2A0B7E391208", "Enable"));
			public static CodeDescriptionPair Tax => new CodeDescriptionPair(AutoJobRevenueJournalCodes.Tax, ResString.GetMultilingualString("76E37CE0-6A10-4823-81F4-43CC181E34C3", "Enable for same GST/VAT registration"));
			public static CodeDescriptionPair Non => new CodeDescriptionPair(AutoJobRevenueJournalCodes.Non, ResString.GetMultilingualString("95AC5786-3507-472B-9080-7EA2ACB2126E", "Disable"));
		}

		public static class AlternateGLAccountAttributeCode
		{
			public const string ORG = "ORG";
			public const string OCG = "OCG";
			public const string TIC = "TIC";
			public const string LFO = "LFO";
			public const string LFE = "LFE";
			public const string SPR = "SPR";
		}

		public static CodeDescriptionPairList GetAttributeList()
		{
			var attributeList = new CodeDescriptionPairList();
			var nonGlobalAttributeList = GetNonGlobalAttributeList();
			attributeList.AddRange(nonGlobalAttributeList);
			attributeList.AddPair(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, ResString.GetMultilingualString("C0704675-E8E2-40C0-9CD0-CE50649A70FA", "The GL Balance will be dissected based on the consolidation category classification group code (TPY/INT) of organization recorded against accounting transactions"));
			attributeList.AddPair(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, ResString.GetMultilingualString("C0BD0711-EB2F-4DBD-A7C9-48A8A49483E1", "The GL Balance will be dissected based on tax id recorded against accounting transactions differentiating tax ids without extra tax and tax ids with extra tax reported separately"));
			attributeList.AddPair(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, ResString.GetMultilingualString("351C7EC5-7FF7-4DAB-8F3B-198B5485D59C", "The GL Balance will be dissected based on location of organization recorded against accounting transactions is Local or Foreign entity"));
			attributeList.AddPair(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, ResString.GetMultilingualString("379E157C-8033-4F5A-9D90-2C8358F3659A", "The GL Balance will be dissected based on location of organization recorded against accounting transactions is Local, Within EU or Outside EU"));
			attributeList.AddPair(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, ResString.GetMultilingualString("117E4EEB-DA5D-4413-A524-5A602DA685DE", "The GL Balance will be dissected based on whether the accounting transactions relates to sales/purchases or sales/purchases returns"));

			return attributeList;
		}

		public static CodeDescriptionPairList GetNonGlobalAttributeList()
		{
			var attributeList = new CodeDescriptionPairList();
			attributeList.AddPair(NonGlobalAttributeCode.ORG, ResString.GetMultilingualString("6F7F91F3-3619-4DF5-B76C-A1D9DC7F4D96", "The GL Balance will be dissected based on organization code recorded against accounting transactions"));
			return attributeList;
		}

		public static class AlternateGLAccountFormatType
		{
			public const char AlphabatFormat = 'X';
			public const char NumberFormat = '9';
		}

		public static CodeDescriptionPairList OCGList
		{
			get
			{
				var oCGList = new ConsolidatedAccountingCategoryClassList();
				oCGList.RemoveCode(ConsolidatedAccountingCategoryClassList.Codes.All);
				oCGList.Add(NAV.NAVPair);
				return oCGList;
			}
		}

		public static class TICCodes
		{
			public const string ETI = "ETI";
			public const string STI = "STI";
		}

		public static CodeDescriptionPairList TICList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TICCodes.ETI, ResString.GetMultilingualString("C1D7A528-3D35-42FA-9BFC-6C69985910FC", "Tax ID with Extra Tax"));
				result.AddPair(TICCodes.STI, ResString.GetMultilingualString("077044A7-EF5F-42A8-918E-AF90AA303364", "Standard Tax IDs"));
				result.Add(NAV.NAVPair);
				return result;
			}
		}

		public static class LFOCodes
		{
			public const string LOC = "LOC";
			public const string FOR = "FOR";
		}

		public static CodeDescriptionPairList LFOList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(LFOCodes.LOC, ResString.GetMultilingualString("1A33E98E-482B-4E57-A7A7-2AD1671F3636", "Local"));
				result.AddPair(LFOCodes.FOR, ResString.GetMultilingualString("0AACB5C3-620D-417E-AAF1-B181CA417E19", "Foreign"));
				result.Add(NAV.NAVPair);
				return result;
			}
		}

		public static class LFECodes
		{
			public const string LOC = "LOC";
			public const string WEU = "WEU";
			public const string OEU = "OEU";
		}

		public static CodeDescriptionPairList LFEList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(LFECodes.LOC, ResString.GetMultilingualString("6E835CA4-F3DF-478E-B39B-73C489E65480", "Local"));
				result.AddPair(LFECodes.WEU, ResString.GetMultilingualString("B34A74F4-0FEA-4F7D-9C18-1B745F8AC61E", "Within EU"));
				result.AddPair(LFECodes.OEU, ResString.GetMultilingualString("78605755-9967-4047-9C79-36E7C045BBD5", "Outside EU"));
				result.Add(NAV.NAVPair);
				return result;
			}
		}

		public static class SPRCodes
		{
			public const string SPS = "SPS";
			public const string SPR = "SPR";
		}

		public static CodeDescriptionPairList SPRList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(SPRCodes.SPS, ResString.GetMultilingualString("FCAC5909-C070-468C-9FF2-2BFA31CE39D4", "Sales/Purchases"));
				result.AddPair(SPRCodes.SPR, ResString.GetMultilingualString("E1F71C98-3D3F-47C6-9D7C-4E380E681FAF", "Sales/Purchases Return"));
				result.Add(NAV.NAVPair);
				return result;
			}
		}

		public static class NAV
		{
			public const string Code = "NAV";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description string")]
			public const string Description = "No Attribute Value";

			public static CodeDescriptionPair NAVPair
			{
				get
				{
					return new CodeDescriptionPair(Code, ResString.GetMultilingualString("B1870CF9-E477-47F1-9E90-58C06C24BFF9", Description));
				}
			}
		}

		public static class ExportMultipleDebtorOrganizationContactEmailCodes
		{
			public const string DEF = "DEF";
			public const string MAR = "MAR";
		}

		public static CodeDescriptionPairList ExportMultipleDebtorOrganizationContactEmailOptions
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ExportMultipleDebtorOrganizationContactEmailCodes.DEF, ResString.GetMultilingualString("6DBC7D12-7076-4028-B271-C9BEE7B4EDEF", "Default - The system will export the email address of single Debtor Organization Contact using existing fallback rules"));
				result.AddPair(ExportMultipleDebtorOrganizationContactEmailCodes.MAR, ResString.GetMultilingualString("741D95AF-4036-4B3C-8996-AF402CF79B31", "Multiple AR Contacts - The system will export multiple Debtor Organization Contact email addresses. Refer to respective country's e-Learning Materials."));
				return result;
			}
		}

		public static class AllowDuplicateInvoiceNumberRule
		{
			public const string STD = "STD";
			public const string CAL = "CAL";
		}

		public class AllowDuplicateInvoiceNumberRules : CodeDescriptionPairList
		{
			public AllowDuplicateInvoiceNumberRules()
			{
				Add(STD);
				Add(CAL);
			}

			public static CodeDescriptionPair STD => new CodeDescriptionPair(AllowDuplicateInvoiceNumberRule.STD, ResString.GetMultilingualString("DF7D971F-5793-4829-B8E6-BE5B60E6D904", "Standard"));
			public static CodeDescriptionPair CAL => new CodeDescriptionPair(AllowDuplicateInvoiceNumberRule.CAL, ResString.GetMultilingualString("39942A71-93E8-4D14-9DE7-C0C10F1AD05A", "Calendar"));
		}

		public static CodeDescriptionPairList WithoutParentAccountTypeList
		{
			get
			{
				var fWithoutParentAccountTypeList = new CodeDescriptionPairList();
				fWithoutParentAccountTypeList.AddPair(Core.Constants.AccountType.Total, Res.GetString("9DEADC93-B043-40F1-AD88-FE2205777EBA", "Total Account"));
				fWithoutParentAccountTypeList.AddPair(Core.Constants.AccountType.Header, Res.GetString("28B39FCF-B893-46D8-81CB-DD0AEA522D23", "Header"));
				fWithoutParentAccountTypeList.AddPair(Core.Constants.AccountType.Consolidation, Res.GetString("F29E5FFE-81B2-48A2-B843-6645FAA4D040", "Consolidated Account"));
				fWithoutParentAccountTypeList.AddPair(Core.Constants.AccountType.Alternate, Res.GetString("5D2DF39C-261F-4F92-B70A-8EE52F2BDE76", "Alternate Account"));
				fWithoutParentAccountTypeList.AddPair(Core.Constants.AccountType.ChartOnly, Res.GetString("AE3A3FB7-5A9F-4F2E-8549-FCDFA7F7868C", "Chart Only"));
				fWithoutParentAccountTypeList.AddPair(Core.Constants.AccountType.Rollup, Res.GetString("82B1F439-F6E7-4C5D-8160-5C89AAA2E0BE", "Roll up"));
				fWithoutParentAccountTypeList.AddPair(Core.Constants.AccountType.Group, Res.GetString("2716F5D4-610E-4364-9923-F1A441AD8DA8", "Group"));

				return fWithoutParentAccountTypeList;
			}
		}

		public static class CurrencyTranslationLevelCodes
		{
			public const string Journal = "JNL";
			public const string ClosingBalance = "BAL";
		}

		public static CodeDescriptionPairList CurrencyTranslationLevelList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(CurrencyTranslationLevelCodes.Journal, ResString.GetMultilingualString("8899060E-41E7-475B-835A-6888C592AF74", "Journal"));
				result.AddPair(CurrencyTranslationLevelCodes.ClosingBalance, ResString.GetMultilingualString("D47A1BC0-076F-4800-8402-DA15FAB388F8", "Closing Balance"));
				return result;
			}
		}
	}
}
