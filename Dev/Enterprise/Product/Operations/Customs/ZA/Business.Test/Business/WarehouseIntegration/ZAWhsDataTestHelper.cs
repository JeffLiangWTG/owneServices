using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public class ZAWhsDataTestHelper : Customs.Business.Testing.WhsDataTestHelper<JobDeclaration, OrgSupplierPart, CusClassification, CusClassPartPivot>
	{
		public ZAWhsDataTestHelper()
		{
		}

		public ZAWhsDataTestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void SetupCustomsData()
		{
			UniversalTariffHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			UniversalTariffHelper.CreateCustomsStatusCusCodeEntry("1");
			UniversalTariffHelper.CreateCustomsStatusCusCodeEntry("4");
			UniversalTariffHelper.CreateCustomsStatusCusCodeEntry("6");
			UniversalTariffHelper.CreateCustomsStatusCusCodeEntry("7");
			UniversalTariffHelper.CreateCustomsStatusCusCodeEntry("27");
			UniversalTariffHelper.CreateCustomsStatusCusCodeEntry("28");
			Factory.Save();
		}

		internal new ZAUniversalReferenceTestDataHelper UniversalTariffHelper
		{
			get
			{
				return (ZAUniversalReferenceTestDataHelper)base.UniversalTariffHelper;
			}
		}

		protected override UniversalReferenceTestDataHelper GetNewUniversalReferenceTestDataHelper()
		{
			return new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupCustomsData();
		}

		public CusEntryHeader GetNewEntryHeader(ZString messageType, ZString declarationReference, ZString procedureCode, ZString entryNumber, ZString previousProcedureCode, ZDecimal quantity, bool isVirtualWarehouse = true)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_CustomsOffice = "BFN";
			declaration.JE_AGTCode = "QWERTYUI";
			declaration.JE_GS_NKCusAgent = CurrentStaff.GS_Code;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = procedureCode;
			entryInstruction.CEI_Description = procedureCode + " DESC";
			entryInstruction.CEI_OA_Warehouse = WhsWarehouse.WW_OA_WarehouseAddress;
			entryInstruction.CEI_OA_Warehouse2 = WhsWarehouse.WW_OA_WarehouseAddress;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			AddInvoiceLine(invoice, Part, quantity, entryInstruction.PK, previousProcedureCode, entryNumber, 1);
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders[0];
			if (entry.IsIntoWarehouseWarehousing)
			{
				entry.EntryNumber = entryNumber;
			}

			return entry;
		}

		public JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, OrgSupplierPart part, ZDecimal quantity, ZGuid entryInstructionPK, ZString previousProcedureCode, ZString previousEntryNumber, ZShort previousEntryLineNumber)
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstructionPK;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + previousProcedureCode;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_BondedWhsQuantity = quantity;
			invoiceLine.JI_BondedWhsUnitQty = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			if (invoiceLine.CusProcedure?.IsOutOfWarehouse() ?? false)
			{
				invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
				invoiceLine.JI_PreviousEntryLineNumber = previousEntryLineNumber;
			}

			return invoiceLine;
		}

		public CONTRLEDIMessage CreateCONTRLEDIMessage(ZString messageNo, ZString responseCode)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchangeForTest>();
			interchange.EI_ReceiveTransmit = EDIInterchangeForTest.Direction.Receive;
			var contrlMessage = Factory.New<CONTRLEDIMessage>();
			contrlMessage.EM_MessageText = ZString.Format(controlMessageResponse.Replace("\r\n", ""), messageNo, responseCode);
			contrlMessage.EM_MessageNum = "1";
			contrlMessage.EM_EI = interchange.PK;
			contrlMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			return contrlMessage;
		}

		readonly string controlMessageResponse = @"UNH+1+CONTRL:D:3:UN:CONTRL'
UCI+181+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'
UCM+{0}+CUSDEC:D:96B:UN:ZZZ01+{1}'
UNT+4+1'";
		public CUSRESEDIMessage CreateCUSRESEDIMessage(ZString messageNo, ZString responseCode, ZString entryNumber, ZString lrn)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchangeForTest>();
			if (responseCode == "1") // release
			{
				var today = ZDateTime.Now.ToString("yyyyMMdd");
				interchange.EI_HeaderText = "UNB+UNOB:4++::X:X+" + today + ":1552+16074++CUSRES++1+X+1'"; // Preparation time is today, so we can use it for entry release date
			}

			interchange.EI_ReceiveTransmit = EDIInterchangeForTest.Direction.Receive;
			var cusressMessage = Factory.New<CUSRESEDIMessage>();
			cusressMessage.EM_MessageText = ZString.Format(cusresMessageResponse.Replace("\r\n", ""), messageNo, responseCode, entryNumber, lrn);
			cusressMessage.EM_MessageNum = "1";
			cusressMessage.EM_EI = interchange.PK;
			cusressMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			return cusressMessage;
		}

		public const string cusresMessageResponse = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+{3}:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+{1}:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:{2}'
DTM+137:20160401:102'
RFF+ACD:{0}'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
		public const string EntryNumber = "JSA201603315000938";
		public const string EntryNumber2 = "JSA201702205000703";
		public GlbGroup PostMasterGroup
		{
			get
			{
				if (postMasterGroup == null)
				{
					postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
					var groupNotification = new AutoBillingGroupNotification();
					groupNotification.SendGroupPK = postMasterGroup.PK;
					CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
				}

				return postMasterGroup;
			}
		}

		GlbGroup postMasterGroup;
		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.SouthAfrica;
			}
		}

		protected override ZString TariffType
		{
			get
			{
				return UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
			}
		}

		protected override ZString CusTariffUOMCustomsUQ
		{
			get
			{
				return UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType;
			}
		}

		protected override ZString CusTariffUOMCustomsSecondUQ
		{
			get
			{
				return UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType;
			}
		}

		protected override ZString CusTariffUOMCustomsThirdUQ
		{
			get
			{
				return UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType;
			}
		}
	}
}
