using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	public class WhsDataTestHelper : WhsDataTestHelper<BaseJobDeclaration, OrgSupplierPart, BaseCusClassification, BaseCusClassPartPivot>
	{
		public WhsDataTestHelper()
			: base()
		{
		}

		public WhsDataTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static IWhsDataTestHelper New(string countryCode, BusinessObjectFactory factory)
		{
			var types = ObjectFactory.Get<Hashtable>("WhsDataTestHelpers");
			var objectHandle = (ObjectHandle)types[countryCode];
			return objectHandle != null ? (IWhsDataTestHelper)objectHandle.GetObject(factory) : new WhsDataTestHelper(factory);
		}

		public ChangOfRegimeEntryData CreateChangeOfRegimeEntryData() => CreateChangeOfRegimeEntryData(JobMessageTypeList.Codes.Import, "BER001234", "ENT43523", 100m);

		public ChangOfRegimeEntryData CreateChangeOfRegimeEntryData(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			var procedure = ChangeOfOwnershipCusProcedure;
			var importer = Importer;
			var importerCompanyData = importer.CompanyData;
			importerCompanyData.OB_IMUsedBondedWhs = true;
			importerCompanyData.OB_CusInventoryForInwardProcessing = true;
			var warehouseAddress = (OrgAddress)WhsWarehouse.WarehouseAddress;
			var warehouseCompanyData = warehouseAddress.Header.CompanyData;
			warehouseCompanyData.OB_IMUsedBondedWhs = true;
			warehouseCompanyData.OB_CusInventoryForInwardProcessing = false;
			var warehouse2Address = (OrgAddress)WhsWarehouse2.WarehouseAddress;
			var warehouse2CompanyData = warehouse2Address.Header.CompanyData;
			warehouse2CompanyData.OB_IMUsedBondedWhs = false;
			warehouse2CompanyData.OB_CusInventoryForInwardProcessing = true;

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.SupportMultipleWarehouseEntryCoreForTesting = true;
			declaration.JE_DeclarationReference = declarationReference;
			var instructionMock = Factory.NewMoq<CusEntryInstruction>();
			var instructionMockProtected = instructionMock.Protected();
			instructionMockProtected.Setup<bool>("IsChangeOfRegimeWarehousingEnabledCore").Returns(true);
			var instruction = instructionMock.Object;
			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Add(instruction);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = messageType;
			instruction.CEI_Style = procedure.ZZ6_ProcedureCode;
			instruction.CEI_Description = procedure.ZZ6_ProcedureCode + " DESC";
			instruction.CEI_OA_Warehouse = warehouseAddress.PK;
			instruction.CEI_OA_Warehouse2 = warehouse2Address.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.EntryNumber = entryNumber;
			entry.CH_BGMReference = new ZString(declaration.JE_DeclarationReference + entryNumber).Trim().Left(entry.CH_BGMReferenceInfo.MaxLength);
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
			invoiceLine.JI_CL = entryLine.PK;
			return new ChangOfRegimeEntryData
			{
				Declaration = declaration,
				InstructionMock = instructionMock,
				Instruction = instruction,
				Invoice = invoice,
				InvoiceLine = invoiceLine,
				Entry = entry,
				EntryLine = entryLine,
			};
		}

		public class ChangOfRegimeEntryData
		{
			public BaseJobDeclarationWithEntryInstructions Declaration { get; set; }
			public Mock<CusEntryInstruction> InstructionMock { get; set; }
			public CusEntryInstruction Instruction { get; set; }
			public BaseJobComInvoiceHeader Invoice { get; set; }
			public BaseJobComInvoiceLine InvoiceLine { get; set; }
			public CusEntryHeader Entry { get; set; }
			public CusEntryLine EntryLine { get; set; }
		}
	}

	public interface IWhsDataTestHelper
	{
		BusinessObjectFactory Factory { get; }
		RefCusProcedure ChangeOfOwnershipCusProcedure { get; }
		BaseCusClassification Classification { get; }
		BaseCusClassification Classification2 { get; }
		OrgHeader Importer { get; }
		RefCusProcedure InwardCusProcedure { get; }
		RefCusProcedure OutwardCusProcedure { get; }
		OrgHeader Owner { get; }
		OrgSupplierPart OwnerPart { get; }
		OrgSupplierPart OwnerPart2 { get; }
		BaseCusClassPartPivot OwnerPart2Pivot { get; }
		BaseCusClassPartPivot OwnerPartPivot { get; }
		OrgSupplierPart Part { get; }
		OrgSupplierPart Part2 { get; }
		BaseCusClassPartPivot Pivot { get; }
		BaseCusClassPartPivot Pivot2 { get; }
		OrgHeader Supplier { get; }
		UniversalReferenceTestDataHelper UniversalTariffHelper { get; }
		OrgHeader Warehouse { get; }
		OrgHeader Warehouse2 { get; }
		IWhsTransactionTestHelper WhsHelper { get; }
		IWhsWarehouse WhsWarehouse { get; }
		IWhsWarehouse WhsWarehouse2 { get; }

		OrgSupplierPart CreateProduct(ZGuid ownerPK, ZString code);
		BaseJobDeclaration GetNewDeclaration(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity);
		IWhsBondedWarehouseAttribute GetNewWhsBondedWarehouseAttribute(ZGuid receiveLinePK, ZDecimal valueForDuty, ZDecimal customsQty, ZString customsUnitOfQty, ZString countryOfOrigin, ZDecimal bondedWhsQty, ZString bondedWhsUnitOfQty, ZString addInfo, ZString entryKey, ZShort entryLineNo, decimal tilv = 0, string tilvCurrency = "");
		IWhsBondedWarehouseAttribute GetNewWhsBondedWarehouseAttribute(ZGuid receiveLinePK, ZDecimal valueForDuty, ZDecimal customsQty, ZString customsUnitOfQty, ZDecimal customsSecondQty, ZString customsSecondUnitQty, ZString countryOfOrigin, ZDecimal bondedWhsQty, ZString bondedWhsUnitOfQty, ZString addInfo, ZString entryKey, ZShort entryLineNo, decimal tilv = 0, string tilvCurrency = "");
		IWhsBondedWarehouseAttribute GetNewWhsBondedWarehouseAttribute(ZGuid receiveLinePK, ZDecimal valueForDuty, ZDecimal customsQty, ZString customsUnitOfQty, ZDecimal customsSecondQty, ZString customsSecondUnitQty, ZDecimal customsThirdQty, ZString customsThirdUnitQty, ZString countryOfOrigin, ZDecimal bondedWhsQty, ZString bondedWhsUnitOfQty, ZString addInfo, ZString entryKey, ZShort entryLineNo, decimal tilv = 0, string tilvCurrency = "");
		IWhsReceive GetNewWhsReceive(ZGuid whsWarehousePK, ZGuid clientPK);
		IWhsReceiveLine GetNewWhsReceiveLine(ZGuid receivePK, ZGuid partPK, ZString packageGroupId, decimal perPackageQty, decimal quantity = 0, decimal currentQty = 0, string packType = "", string partAttribute1 = "", string partAttribute2 = "", string partAttribute3 = "", string serialNumber = "", string bondedEntryKey = "", ZDateTime? arrivalDate = null);
		IWhsWarehouse GetNewWhsWarehouse(ZGuid warehouseAddressPK, ZBool isVirtualWarehouse, ZString warehouseCode, ZString? warehouseType = null);
		void SetTariffAndSave(OrgSupplierPart part, ZString? customsUQ = null, ZString? customsSecondUQ = null, ZString? customsThirdUQ = null);
		void SetTariffAndSave(BaseCusClassPartPivot pivot, ZString? customsUQ = null, ZString? customsSecondUQ = null, ZString? customsThirdUQ = null, bool shouldSave = true);
		void SetupPartAttributesForOrganisation(OrgHeader org, ZString? attrib1Name = null, ZString? attrib1Type = null, ZString? attrib2Name = null, ZString? attrib2Type = null, ZString? attrib3Name = null, ZString? attrib3Type = null);
	}

	public class WhsDataTestHelper<TDeclaratin, TPart, TClassification, TPivot> : TestCaseWithFactory, IWhsDataTestHelper
		where TDeclaratin : BaseJobDeclaration
		where TPart : OrgSupplierPart
		where TClassification : BaseCusClassification
		where TPivot : BaseCusClassPartPivot
	{
		public WhsDataTestHelper()
		{
		}

		public WhsDataTestHelper(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		public static void AssertInventoryAvailabilityInActualDatabase(ZString entryNumber, ZShort entryLineNo, ZDecimal availability, OrgHeader client = null)
		{
			AssertInventoryAvailabilityInActualDatabase(entryNumber + "-" + entryLineNo, availability, client);
		}

		public static void AssertInventoryAvailabilityInActualDatabase(ZString customsReference, ZDecimal availability, OrgHeader client = null)
		{
			var factory = new BusinessObjectFactory(); // requires new factory as existing factories might not be properly updated due to databus refresh being disable in universal xml
			var whsQuery = new ZQuery(WhsInventoryViewSchema.WI_BondedEntryKey, customsReference);
			if (client != null)
			{
				whsQuery.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, client.PK);
			}
			var total = ZDecimal.Zero;
			foreach (var inventory in factory.Load<IWhsInventoryView>(whsQuery))
			{
				total += (ZDecimal)inventory["WI_AvailableToPickQuantity"];
			}
			TestCaseWithFactory.AssertEquals((client == null ? "" : "Client: " + client.OH_Code + " ") + customsReference, availability, total);
		}

		public static IWhsInventoryView[] GetWhsInventoryFromDatabase(ZString customsReference)
		{
			var whsQuery = new ZQuery(WhsInventoryViewSchema.WI_BondedEntryKey, customsReference);
			var factory = new BusinessObjectFactory(); // requires new factory as existing factories might not be properly updated due to databus refresh being disable in universal xml
			return factory.Load<IWhsInventoryView>(whsQuery);
		}

		public void TearDownCountrySetter()
		{
			countrySetter.Dispose();
			countrySetter = null;
		}

		public void SetupCountrySetterAndInvoicing()
		{
			var data = (ZArchitecture.Environment.RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode);
		}

		public CusEntryHeader GetNewEntryHeader(ZString messageType, ZString declarationReference, ZString procedureCode, ZString entryNumber, ZString previousProcedureCode, ZDecimal quantity, ZString instructionStyle, ZString currencyCode)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_CustomsOffice = "BFN";
			declaration.JE_GS_NKCusAgent = CurrentStaff.GS_Code;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Style = instructionStyle;
			entryInstruction.CEI_Description = instructionStyle + " DESC";
			entryInstruction.CEI_OA_Warehouse = WhsWarehouse.WW_OA_WarehouseAddress;
			entryInstruction.CEI_OA_Warehouse2 = WhsWarehouse.WW_OA_WarehouseAddress;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = currencyCode;
			AddInvoiceLine(invoice, Part, quantity, entryInstruction.PK, procedureCode, previousProcedureCode, entryNumber, 1);
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders[0];
			if (entry.IsIntoWarehouseWarehousing)
			{
				entry.EntryNumber = entryNumber;
			}
			return entry;
		}

		public GlbStaff CurrentStaff
		{
			get
			{
				if (currentStaff == null)
				{
					currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
					currentStaff.GS_EmailAddress = "staff@cargowise.com";
				}
				return currentStaff;
			}
		}
		GlbStaff currentStaff;

		public BaseJobComInvoiceLine AddInvoiceLine(BaseJobComInvoiceHeader invoice, OrgSupplierPart part, ZDecimal quantity, ZGuid entryInstructionPK, ZString procedureCode, ZString previousProcedureCode, ZString previousEntryNumber, ZShort previousEntryLineNumber)
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstructionPK;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_Procedure = procedureCode + previousProcedureCode;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_BondedWhsQuantity = quantity;
			invoiceLine.JI_BondedWhsUnitQty = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			if (invoiceLine.CusProcedure is RefCusProcedure procedure && invoiceLine.IsOutOfRegime(procedure))
			{
				invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
				invoiceLine.JI_PreviousEntryLineNumber = previousEntryLineNumber;
			}
			return invoiceLine;
		}

		public void AssertInvoiceLine(BaseJobComInvoiceLine invoiceLine, ZGuid entryInstructionPK, ZDecimal invoiceQty, ZString invoiceUQ, ZDecimal countableQty, ZString countableUQ, ZDecimal customsQty, ZString customsUQ,
			ZDecimal customsThirdQty, ZString customsThirdUQ, ZDecimal linePrice, ZString ppc)
		{
			AssertEquals("invoiceLine.JI_CEI", entryInstructionPK, invoiceLine.JI_CEI);
			AssertEquals("invoiceLine.JI_InvoiceQuantity", invoiceQty, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("invoiceLine.JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
			AssertEquals("invoiceLine.JI_BondedWhsQuantity", countableQty, invoiceLine.JI_BondedWhsQuantity);
			AssertEquals("invoiceLine.JI_BondedWhsUnitQty", countableUQ, invoiceLine.JI_BondedWhsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsUnitQty", customsUQ, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsQuantity", customsQty, invoiceLine.JI_CustomsQuantity);
			AssertEquals("invoiceLine.JI_CustomsThirdQuantity", customsThirdQty, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals("invoiceLine.JI_CustomsThirdUnitQty", customsThirdUQ, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("invoiceLine.JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
			AssertEquals("invoiceLine.JI_Calc_PreviousProcedure", ppc, invoiceLine.JI_Calc_PreviousProcedure);
		}

		public IWhsWarehouse GetNewWhsWarehouse(ZGuid warehouseAddressPK, ZBool isVirtualWarehouse, ZString warehouseCode, ZString? warehouseType = null)
		{
			var whsWarehouse = (IWhsWarehouse)WhsHelper.CreateWarehouse(warehouseCode + " NAME");
			whsWarehouse.WW_WarehouseCode = warehouseCode;
			if (warehouseType.HasValue)
			{
				whsWarehouse.WW_WarehouseType = warehouseType.Value;
			}

			whsWarehouse.WW_OA_WarehouseAddress = warehouseAddressPK;
			whsWarehouse.WW_IsVirtualWarehouse = isVirtualWarehouse;
			whsWarehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			WhsHelper.EnableWarehouseForBond(whsWarehouse, true);
			var prevDefault = whsWarehouse.Areas.Cast<IWhsArea>().First(a => a.WA_Name == "DEFAULT");
			var bondArea = whsWarehouse.Areas.Cast<IWhsArea>().First(a => a.WA_Name == "BOND");
			prevDefault.WA_IsDefaultPickArea = false;
			prevDefault.WA_IsDefaultPutawayArea = false;
			bondArea.WA_IsDefaultPickArea = true;
			bondArea.WA_IsDefaultPutawayArea = true;

			return whsWarehouse;
		}

		public IWhsReceive GetNewWhsReceive(ZGuid whsWarehousePK, ZGuid clientPK)
		{
			return GetNewWhsReceive(whsWarehousePK, clientPK, "RCV1", null);
		}

		public IWhsReceive GetNewWhsReceive(ZGuid whsWarehousePK, ZGuid clientPK, ZString refTxt, ZDateTimeOffset? arrivalDate = null)
		{
			var rcvPk = WhsHelper.CreateWhsReceive(clientPK, whsWarehousePK, refTxt, null);
			var whsReceive = Factory.Load<IWhsReceive>(rcvPk);
			whsReceive.WD_DocketSubType = "CUS";
			whsReceive.WD_ArrivalDate = arrivalDate ?? ZDateTimeOffset.Today.AddDays(-3);
			return whsReceive;
		}

		public IWhsReceiveLine GetNewWhsReceiveLine(ZGuid receivePK, ZGuid partPK, ZString packageGroupId, decimal perPackageQty, decimal quantity = 0m, decimal currentQty = 0, string packType = "UNT", string partAttribute1 = "", string partAttribute2 = "", string partAttribute3 = "", string serialNumber = "", string bondedEntryKey = "", ZDateTime? arrivalDate = null)
		{
			var whsReceive = Factory.Load<IWhsReceive>(receivePK);
			var part = Factory.Load<OrgSupplierPart>(partPK);

			var inventory = GetNewReceiveInventory(whsReceive, part, packageGroupId, perPackageQty, quantity, currentQty, packType, partAttribute1, partAttribute2, partAttribute3, serialNumber, bondedEntryKey, arrivalDate?.ToOffset());

			var whsLine = Factory.Load<IWhsReceiveLine>(inventory.WI_WE_InDocketLine);
			return whsLine;
		}

		public IWhsInventoryView GetNewReceiveInventory(IWhsReceive whsReceive, OrgSupplierPart part, ZString packageGroupId, decimal perPackageQty, decimal quantity = 0m, decimal currentQty = 0, string packType = "UNT",
			string partAttribute1 = "", string partAttribute2 = "", string partAttribute3 = "", string serialNumber = "", string bondedEntryKey = "", ZDateTimeOffset? arrivalDate = null, string location = "A-1")
		{
			var adjustmentArrivalDate = arrivalDate ?? whsReceive.WD_ArrivalDate;

			var whsInventory = WhsHelper.CreateWhsReceiveInventoryLine(whsReceive.PK, part.PK, perPackageQty, quantity, currentQty, packageGroupId, packType, adjustmentArrivalDate, bondedEntryKey, location);
			var whsLine = Factory.Load<IWhsReceiveLine>(whsInventory.WI_WE_InDocketLine);
			SetUpPartAttribute(whsLine, part, 1, partAttribute1);
			SetUpPartAttribute(whsLine, part, 2, partAttribute2);
			SetUpPartAttribute(whsLine, part, 3, partAttribute3);
			SetUpSerialNumber(whsLine, part, serialNumber);

			return whsInventory;
		}

		public IWhsOrder GetNewWhsOrder(ZGuid clientPK, IWhsWarehouse whsWarehouse, ZString refTxt)
		{
			var orderPK = WhsHelper.CreateWhsOrder(clientPK, whsWarehouse.PK, refTxt, null);
			var order = Factory.Load<IWhsOrder>(orderPK);
			return order;
		}

		public IWhsDocketLine GetNewWhsOrderLine(IWhsOrder order, OrgSupplierPart part, ZDecimal quantity)
		{
			var orderLinePK = WhsHelper.CreateWhsOrderLine(order.PK, part.PK, quantity);
			var orderLine = Factory.Load<IWhsDocketLine>(orderLinePK);
			return orderLine;
		}

		public IWhsPick GetNewWhsPick(IWhsPickableDocket[] orders)
		{
			var pickPK = WhsHelper.CreateWhsPick(orders.Select(order => order.PK).ToArray());
			var pick = Factory.Load<IWhsPick>(pickPK);
			return pick;
		}

		public IWhsPickLine GetNewWhsPickLine(IWhsDocketLine orderLine, IWhsInventoryView inventory, ZDecimal qty)
		{
			var pickLine = (IWhsPickLine)WhsHelper.CreateWhsPickLine(orderLine, inventory, qty);
			return pickLine;
		}

		void SetUpPartAttribute(IWhsReceiveLine whsLine, OrgSupplierPart part, int attributeNumber, ZString value)
		{
			if (!value.IsEmpty)
			{
				var relation = part.RelatedOrganisations[0];
				relation[$"OU_UsePartAttrib{attributeNumber}"] = true;
				relation.Organisation.MiscServ[$"OM_IMPartAttrib{attributeNumber}Name"] = $"Vin{attributeNumber}";
				relation.Organisation.MiscServ[$"OM_IMPartAttrib{attributeNumber}Type"] = PartAttributeTypeList.Codes.NonMandatory;
				whsLine[$"WE_PartAttrib{attributeNumber}"] = value;
			}
		}

		void SetUpSerialNumber(IWhsReceiveLine whsLine, OrgSupplierPart part, ZString value)
		{
			if (!value.IsEmpty)
			{
				var relation = part.RelatedOrganisations[0];
				relation.OU_UseSerialNumber = true;
				relation.Organisation.MiscServ.OM_IMUseSerialNumber = true;
				whsLine.WE_SerialNumber = value;
			}
		}

		public IWhsBondedWarehouseAttribute GetNewWhsBondedWarehouseAttribute(ZGuid receiveLinePK, ZDecimal valueForDuty, ZDecimal customsQty, ZString customsUnitOfQty, ZDecimal customsSecondQty, ZString customsSecondUnitQty, ZDecimal customsThirdQty, ZString customsThirdUnitQty, ZString countryOfOrigin, ZDecimal bondedWhsQty, ZString bondedWhsUnitOfQty, ZString addInfo, ZString entryKey, ZShort entryLineNo, decimal tilv = 0m, string tilvCurrency = "")
		{
			var whsAttribute = GetNewWhsBondedWarehouseAttribute(receiveLinePK, valueForDuty, customsQty, customsUnitOfQty, customsSecondQty, customsSecondUnitQty, countryOfOrigin, bondedWhsQty, bondedWhsUnitOfQty, addInfo, entryKey, entryLineNo, tilv, tilvCurrency);
			whsAttribute.WB_CustomsThirdQuantity = customsThirdQty;
			whsAttribute.WB_CustomsThirdUnitQty = customsThirdUnitQty;
			return whsAttribute;
		}

		public IWhsBondedWarehouseAttribute GetNewWhsBondedWarehouseAttribute(ZGuid receiveLinePK, ZDecimal valueForDuty, ZDecimal customsQty, ZString customsUnitOfQty, ZDecimal customsSecondQty, ZString customsSecondUnitQty, ZString countryOfOrigin, ZDecimal bondedWhsQty, ZString bondedWhsUnitOfQty, ZString addInfo, ZString entryKey, ZShort entryLineNo, decimal tilv = 0m, string tilvCurrency = "")
		{
			var whsAttribute = GetNewWhsBondedWarehouseAttribute(receiveLinePK, valueForDuty, customsQty, customsUnitOfQty, countryOfOrigin, bondedWhsQty, bondedWhsUnitOfQty, addInfo, entryKey, entryLineNo, tilv, tilvCurrency);
			whsAttribute.WB_CustomsSecondQuantity = customsSecondQty;
			whsAttribute.WB_CustomsSecondUnitQty = customsSecondUnitQty;
			return whsAttribute;
		}

		public IWhsBondedWarehouseAttribute GetNewWhsBondedWarehouseAttribute(ZGuid receiveLinePK, ZDecimal valueForDuty, ZDecimal customsQty, ZString customsUnitOfQty, ZString countryOfOrigin, ZDecimal bondedWhsQty, ZString bondedWhsUnitOfQty, ZString addInfo, ZString entryKey, ZShort entryLineNo, decimal tilv = 0m, string tilvCurrency = "")
		{
			var whsLine = Factory.Load<IWhsDocketLine>(receiveLinePK) as IWhsReceiveLine;
			var whsAttribute = whsLine != null ? whsLine.CustomsData : Factory.New<IWhsBondedWarehouseAttribute>();
			whsAttribute.WB_ParentID = receiveLinePK;
			whsAttribute.WB_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			whsAttribute.WB_ValueForDuty = valueForDuty;
			whsAttribute.WB_CustomsQty = customsQty;
			whsAttribute.WB_CustomsUnitOfQty = customsUnitOfQty;
			whsAttribute.WB_RN_NKCountryOfOrigin = countryOfOrigin;
			whsAttribute.WB_BondedWhsQty = bondedWhsQty;
			whsAttribute.WB_BondedWhsUnitOfQty = bondedWhsUnitOfQty;
			whsAttribute.WB_AddInfo = addInfo;
			whsAttribute.WB_EntryKey = entryKey;
			whsAttribute.WB_EntryLineNo = entryLineNo;
			whsAttribute.WB_TILV = tilv;
			whsAttribute.WB_RX_NKTILVCurrency = tilvCurrency;

			return whsAttribute;
		}

		public IWhsReceiveLine GetNewWhsReceiveLineWithBondedAttribute(ZGuid receivePK, ZGuid partPK, ZDecimal vfd, ZDecimal qty, ZGuid locationPK, ZShort lineNo, string entryKey = "EN00123")
		{
			var receiveLine = GetNewWhsReceiveLine(receivePK, partPK, ZString.Empty, perPackageQty: 1m, quantity: qty, currentQty: qty);
			GetNewWhsBondedWarehouseAttribute(receiveLine.PK, vfd, qty, "UNT", "AU", qty, "UNT", ZString.Empty, entryKey, lineNo);
			receiveLine.WE_WL = locationPK;

			return receiveLine;
		}

		public void CreateAndFinaliseWorkOrderWithLine(ZGuid clientPK, ZGuid whsPK, ZGuid productPK, ZDecimal qty)
		{
			var workOrder = WhsHelper.CreateWhsWorkOrderWithLine(clientPK, whsPK, productPK, qty);
			workOrder.WD_IsInwardsProcessingJob = true;
			WhsHelper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder.PK);
			Factory.Save();
			Assert("Precondition: Work Order is finalised", !workOrder.WD_FinalisedDate.IsEmpty);
		}

		public IWhsWarehouse WhsWarehouse
		{
			get
			{
				if (whsWarehouse == null)
				{
					whsWarehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WH1"));
					if (whsWarehouse == null)
					{
						var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
						var warehouseMainAddress = Warehouse.MainAddress;
						whsWarehouse = (IWhsWarehouse)helper.CreateWarehouse(warehouseMainAddress.OA_Address1, "WH1", "BOND1");
						whsWarehouse.WW_OA_WarehouseAddress = warehouseMainAddress.PK;
						whsWarehouse.WW_IsBondedWarehouse = true;
						whsWarehouse.WW_IsVirtualWarehouse = true;
						((IWhsArea)whsWarehouse.Areas[0]).WA_AreaType = "BON";
						whsWarehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
						whsWarehouse.WW_AutoPrintPackingSlip = false;
					}
				}
				return whsWarehouse;
			}
		}
		IWhsWarehouse whsWarehouse;

		public IWhsWarehouse WhsWarehouse2
		{
			get
			{
				if (whsWarehouse2 == null)
				{
					whsWarehouse2 = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WH2"));
					if (whsWarehouse2 == null)
					{
						var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
						var warehouse2MainAddress = Warehouse2.MainAddress;
						whsWarehouse2 = (IWhsWarehouse)helper.CreateWarehouse(warehouse2MainAddress.OA_Address1, "WH2", "BOND2");
						whsWarehouse2.WW_OA_WarehouseAddress = warehouse2MainAddress.PK;
						whsWarehouse2.WW_IsBondedWarehouse = true;
						whsWarehouse2.WW_IsVirtualWarehouse = true;
						((IWhsArea)whsWarehouse2.Areas[0]).WA_AreaType = "BON";
						whsWarehouse2.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
						whsWarehouse2.WW_AutoPrintPackingSlip = false;
					}
				}
				return whsWarehouse2;
			}
		}
		IWhsWarehouse whsWarehouse2;

		public IWhsArea SetupInwardProcessingAreaAndSave(IWhsWarehouse whs)
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var inwardProcessingArea = (IWhsArea)whs.Areas.AddNew();
			inwardProcessingArea.WA_WW_Whs = whs.PK;
			var code = whs.WW_WarehouseName;
			inwardProcessingArea.WA_Name = new ZString(code + " AREA").Left(WhsAreaSchema.WA_Name.MaxLength);
			inwardProcessingArea.WA_AreaType = "IPR";
			inwardProcessingArea.WA_IsPickingArea = true;
			inwardProcessingArea.WA_IsPutawayArea = true;
			var inwardProcessingRow = (IWhsRow)whs.Rows.AddNew();
			inwardProcessingRow.WR_WW_Whs = whs.PK;
			inwardProcessingRow.WR_Name = new ZString(code + " ROW").Left(WhsRowSchema.WR_Name.MaxLength);
			helper.GenerateLocations(whs.PK);
			Factory.Save();

			var inwardProcessingLocation = helper.FindLocation(whs.PK, inwardProcessingRow.WR_Name);
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
			Factory.Save();
			return inwardProcessingArea;
		}

		public UniversalReferenceTestDataHelper UniversalTariffHelper => universalTariffHelper ?? (universalTariffHelper = GetNewUniversalReferenceTestDataHelper());
		UniversalReferenceTestDataHelper universalTariffHelper;

		protected virtual UniversalReferenceTestDataHelper GetNewUniversalReferenceTestDataHelper()
		{
			return new UniversalReferenceTestDataHelper(Factory);
		}

		public RefCusProcedure ChangeOfOwnershipCusProcedure
		{
			get
			{
				if (changeOfOwnershipCusProcedure == null)
				{
					changeOfOwnershipCusProcedure = UniversalTariffHelper.CreateRefCusProcedure(CountryCode, ZString.Empty, "CP", "00", ZString.Empty, "CP DESC", "EXW", intoWarehouse: true, outOfWarehouse: true, group: "CP");
				}
				return changeOfOwnershipCusProcedure;
			}
		}
		RefCusProcedure changeOfOwnershipCusProcedure;

		public RefCusProcedure InwardCusProcedure
		{
			get
			{
				if (inwardCusProcedure == null)
				{
					inwardCusProcedure = UniversalTariffHelper.CreateRefCusProcedure(CountryCode, ZString.Empty, "IP", "00", ZString.Empty, "IP DESC", "IMP", intoWarehouse: true, group: "IP");
				}
				return inwardCusProcedure;
			}
		}
		RefCusProcedure inwardCusProcedure;

		public RefCusProcedure OutwardCusProcedure
		{
			get
			{
				if (outwardCusProcedure == null)
				{
					outwardCusProcedure = UniversalTariffHelper.CreateRefCusProcedure(CountryCode, ZString.Empty, "OP", "IP", ZString.Empty, "OP DESC", "EXW", outOfWarehouse: true, group: "OP");
				}
				return outwardCusProcedure;
			}
		}
		RefCusProcedure outwardCusProcedure;

		public void SetTariffAndSave(OrgSupplierPart part, ZString? customsUQ = null, ZString? customsSecondUQ = null, ZString? customsThirdUQ = null)
		{
			foreach (var pivot in part.GetPivots<BaseCusClassPartPivot>(part.CurrentCompanyCustomsCountryCode))
			{
				SetTariffAndSave(pivot, customsUQ, customsSecondUQ, customsThirdUQ, false);
			}
			if (part.HasChanges)
			{
				Factory.Save();
			}
		}

		public virtual void SetTariffAndSave(BaseCusClassPartPivot pivot, ZString? customsUQ = null, ZString? customsSecondUQ = null, ZString? customsThirdUQ = null, bool shouldSave = true)
		{
			var tariffType = UniversalTariffHelper.CreateNewOrGetExistingTariffType(CountryCode, TariffType);
			Factory.Save();
			var tariffNo = pivot.TariffNumber;
			var tariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(CountryCode, TariffType, tariffNo, ZDateTime.Today)
				?? UniversalTariffHelper.CreateTariff(CountryCode, tariffType.PK, tariffNo, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), tariffNo + " DESC");
			SetTariffUQ(tariff, CusTariffUOMCustomsUQ, customsUQ);
			SetTariffUQ(tariff, CusTariffUOMCustomsSecondUQ, customsSecondUQ);
			SetTariffUQ(tariff, CusTariffUOMCustomsThirdUQ, customsThirdUQ);
			if (shouldSave)
			{
				Factory.Save();
			}
		}

		protected void SetTariffUQ(TariffView tariff, ZString uomType, ZString? uom)
		{
			if (uom.HasValue && !uomType.IsEmpty)
			{
				var unitOfMeasure = tariff.UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == uomType);
				if (unitOfMeasure == null)
				{
					unitOfMeasure = UniversalTariffHelper.CreateTariffUOM(tariff, uomType, uom.Value);
				}
				else
				{
					unitOfMeasure.ZZ8_UOM = uom.Value;
				}
			}
		}

		protected virtual ZString TariffType
		{
			get { return "HSN"; }
		}

		protected virtual ZString CusTariffUOMCustomsUQ
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString CusTariffUOMCustomsSecondUQ
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString CusTariffUOMCustomsThirdUQ
		{
			get { return ZString.Empty; }
		}

		public TDeclaratin GetNewDeclaration(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			return GetNewDeclarationCore(messageType, declarationReference, entryNumber, quantity);
		}

		protected virtual TDeclaratin GetNewDeclarationCore(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			var declaration = Factory.New<TDeclaratin>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.WarehouseDocAddress.E2_OA_Address = WhsWarehouse.WW_OA_WarehouseAddress;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			declaration.SetSupportsBondedWarehousingForTesting(true);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = entryNumber;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = invoice.LocalCurrencyCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			invoiceLine.JI_CL = entryLine.PK;
			return declaration;
		}

		public OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMP");
					if (importer == null)
					{
						importer = Factory.New<OrgHeader>();
						importer.OH_Code = "IMP";
						importer.OH_FullName = "TestImp";
						importer.MainAddress.OA_Address1 = "IMP ADDRESS 1";
						importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
						importer.MiscServ.OM_IMPartAttrib1Type = "NON";
						importer.CompanyData.OB_IMUsedBondedWhs = true;
						importer.OH_IsConsignee = true;
						importer.OH_IsWarehouseClient = true;
					}
				}
				return importer;
			}
		}
		OrgHeader importer;

		public OrgHeader Importer2
		{
			get
			{
				if (importer2 == null)
				{
					importer2 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IM2");
					if (importer2 == null)
					{
						importer2 = Factory.New<OrgHeader>();
						importer2.OH_Code = "IM2";
						importer2.OH_FullName = "TestImp2";
						importer2.MainAddress.OA_Address1 = "IMP ADDRESS 2";
						importer2.MiscServ.OM_IMPartAttrib1Name = "VIN2";
						importer2.MiscServ.OM_IMPartAttrib1Type = "NON";
						importer2.CompanyData.OB_IMUsedBondedWhs = true;
						importer2.OH_IsConsignee = true;
						importer2.OH_IsWarehouseClient = true;
					}
				}
				return importer2;
			}
		}
		OrgHeader importer2;

		public string WarehouseDefaultCountry = Core.Constants.CountryCodes.UnitedStates;

		public OrgHeader Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "W1");
					if (warehouse == null)
					{
						warehouse = Factory.New<OrgHeader>();
						warehouse.OH_Code = "W1";
						warehouse.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
						warehouse.MainAddress.OA_Address1 = "W1 ADDRESS 1";
						warehouse.MainAddress.LocalControlledPremisesID = "23423";
						warehouse.MainAddress.OA_RN_NKCountryCode = WarehouseDefaultCountry;
					}
				}
				return warehouse;
			}
		}
		OrgHeader warehouse;

		public OrgHeader Warehouse2
		{
			get
			{
				if (warehouse2 == null)
				{
					warehouse2 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "W2");
					if (warehouse2 == null)
					{
						warehouse2 = Factory.New<OrgHeader>();
						warehouse2.OH_Code = "W2";
						warehouse2.MainAddress.OA_Address1 = "W2 ADDRESS 1";
						warehouse2.MainAddress.LocalControlledPremisesID = "43423";
						warehouse2.MainAddress.OA_RN_NKCountryCode = WarehouseDefaultCountry;
					}
				}
				return warehouse2;
			}
		}
		OrgHeader warehouse2;

		public TPart Part
		{
			get
			{
				if (part == null)
				{
					part = (TPart)new OrgSupplierPart.Loader(Factory, typeof(TPart)).Load("~~1", Importer, null);
					if (part == null)
					{
						part = CreateProduct(Importer.PK, "~~1");
						part.OP_Desc = "~~1 DESC";
					}
					var pivot = Pivot; // Force the creation of the pivot
				}
				return part;
			}
		}
		TPart part;

		public TClassification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.LoadTop1<TClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "~~1L"));
					if (classification == null)
					{
						classification = Factory.New<TClassification>();
						classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
						classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
						classification.CC_LookupCode = "~~1L";
						classification.CC_TariffNum = "0000000000";
					}
				}
				return classification;
			}
		}
		TClassification classification;

		public TPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					var query = new ZQuery(CusClassPartPivotSchema.CI_CC, Classification.PK);
					query.AddToFilter(CusClassPartPivotSchema.CI_OP, Part.PK);
					pivot = Factory.LoadTop1<TPivot>(query);
					if (pivot == null)
					{
						pivot = Factory.New<TPivot>();
						pivot.CI_CC = Classification.PK;
						pivot.CI_OP = Part.PK;
						pivot.CI_ChildType = PivotChildType;
					}
				}
				return pivot;
			}
		}
		TPivot pivot;

		public TPart Part2
		{
			get
			{
				if (part2 == null)
				{
					part2 = (TPart)new OrgSupplierPart.Loader(Factory, typeof(TPart)).Load("~~2", Importer, null);
					if (part2 == null)
					{
						part2 = CreateProduct(Importer.PK, "~~2");
						part2.OP_Desc = "~~2 DESC";
					}
					var pivot = Pivot2; // Force the creation of the pivot
				}
				return part2;
			}
		}
		TPart part2;

		public TClassification Classification2
		{
			get
			{
				if (classification2 == null)
				{
					classification2 = Factory.LoadTop1<TClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "~~2L"));
					if (classification2 == null)
					{
						classification2 = Factory.New<TClassification>();
						classification2.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
						classification2.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
						classification2.CC_LookupCode = "~~2L";
						classification2.CC_TariffNum = "0000000001";
					}
				}
				return classification2;
			}
		}
		TClassification classification2;

		public TPivot Pivot2
		{
			get
			{
				if (pivot2 == null)
				{
					var query = new ZQuery(CusClassPartPivotSchema.CI_CC, Classification2.PK);
					query.AddToFilter(CusClassPartPivotSchema.CI_OP, Part2.PK);
					pivot2 = Factory.LoadTop1<TPivot>(query);
					if (pivot2 == null)
					{
						pivot2 = Factory.New<TPivot>();
						pivot2.CI_CC = Classification2.PK;
						pivot2.CI_OP = Part2.PK;
						pivot2.CI_ChildType = PivotChildType;
					}
				}
				return pivot2;
			}
		}
		TPivot pivot2;

		public void SetupPartAttributesForOrganisation(OrgHeader org, ZString? attrib1Name = null, ZString? attrib1Type = null, ZString? attrib2Name = null, ZString? attrib2Type = null, ZString? attrib3Name = null, ZString? attrib3Type = null)
		{
			if (attrib1Name.HasValue && attrib1Type.HasValue)
			{
				org.MiscServ.OM_IMPartAttrib1Name = attrib1Name.Value;
				org.MiscServ.OM_IMPartAttrib1Type = attrib1Type.Value;
			}
			if (attrib2Name.HasValue && attrib2Type.HasValue)
			{
				org.MiscServ.OM_IMPartAttrib2Name = attrib2Name.Value;
				org.MiscServ.OM_IMPartAttrib2Type = attrib2Type.Value;
			}
			if (attrib3Name.HasValue && attrib3Type.HasValue)
			{
				org.MiscServ.OM_IMPartAttrib3Name = attrib3Name.Value;
				org.MiscServ.OM_IMPartAttrib3Type = attrib3Type.Value;
			}
		}

		public OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SUP324");
					if (supplier == null)
					{
						supplier = Factory.New<OrgHeader>();
						supplier.OH_Code = "SUP324";
						supplier.MainAddress.OA_Address1 = "SUP ADDRESS 1";
						supplier.OH_IsConsignor = true;
					}
				}
				return supplier;
			}
		}
		OrgHeader supplier;

		public OrgHeader Owner
		{
			get
			{
				if (owner == null)
				{
					owner = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "OWN324");
					if (owner == null)
					{
						owner = Factory.New<OrgHeader>();
						owner.OH_Code = "OWN324";
						owner.MainAddress.OA_Address1 = "OWN ADDRESS 1";
						owner.MiscServ.OM_IMPartAttrib2Name = "VIN1";
						owner.MiscServ.OM_IMPartAttrib2Type = "NON";
						owner.CompanyData.OB_IMUsedBondedWhs = true;
						owner.OH_IsConsignee = true;
						owner.OH_IsWarehouseClient = true;
					}
				}
				return owner;
			}
		}
		OrgHeader owner;

		public TPart OwnerPart
		{
			get
			{
				if (ownerPart == null)
				{
					ownerPart = (TPart)new OrgSupplierPart.Loader(Factory, typeof(TPart)).Load(Part.OP_PartNum, Owner, null);
					if (ownerPart == null)
					{
						ownerPart = CreateProduct(Owner.PK, Part.OP_PartNum);
						ownerPart.OP_Desc = "~~1 OWNER DESC";
					}
					var pivot = OwnerPartPivot; // Force the creation of the pivot
				}
				return ownerPart;
			}
		}
		TPart ownerPart;

		public TPivot OwnerPartPivot
		{
			get
			{
				if (ownerPartPivot == null)
				{
					var query = new ZQuery(CusClassPartPivotSchema.CI_CC, Classification.PK);
					query.AddToFilter(CusClassPartPivotSchema.CI_OP, OwnerPart.PK);
					ownerPartPivot = Factory.LoadTop1<TPivot>(query);
					if (ownerPartPivot == null)
					{
						ownerPartPivot = Factory.New<TPivot>();
						ownerPartPivot.CI_CC = Classification.PK;
						ownerPartPivot.CI_OP = OwnerPart.PK;
						ownerPartPivot.CI_ChildType = PivotChildType;
					}
				}
				return ownerPartPivot;
			}
		}
		TPivot ownerPartPivot;

		public TPart OwnerPart2
		{
			get
			{
				if (ownerPart2 == null)
				{
					ownerPart2 = (TPart)new OrgSupplierPart.Loader(Factory, typeof(TPart)).Load(Part2.OP_PartNum, Owner, null);
					if (ownerPart2 == null)
					{
						ownerPart2 = CreateProduct(Owner.PK, Part2.OP_PartNum);
						ownerPart2.OP_Desc = "~~2 OWNER DESC";
					}
					var pivot = OwnerPart2Pivot; // Force the creation of the pivot
				}
				return ownerPart2;
			}
		}
		TPart ownerPart2;

		public TPivot OwnerPart2Pivot
		{
			get
			{
				if (ownerPartPivot == null)
				{
					var query = new ZQuery(CusClassPartPivotSchema.CI_CC, Classification.PK);
					query.AddToFilter(CusClassPartPivotSchema.CI_OP, OwnerPart2.PK);
					ownerPart2Pivot = Factory.LoadTop1<TPivot>(query);
					if (ownerPartPivot == null)
					{
						ownerPart2Pivot = Factory.New<TPivot>();
						ownerPart2Pivot.CI_CC = Classification.PK;
						ownerPart2Pivot.CI_OP = OwnerPart2.PK;
						ownerPart2Pivot.CI_ChildType = PivotChildType;
					}
				}
				return ownerPart2Pivot;
			}
		}
		TPivot ownerPart2Pivot;

		public IWhsTransactionTestHelper WhsHelper
		{
			get { return whsHelper ?? (whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory)); }
		}
		IWhsTransactionTestHelper whsHelper;

		public TPart CreateProduct(ZGuid ownerPK, ZString code)
		{
			var part = Factory.New<TPart>();
			part.OP_PartNum = code;
			part.OP_Desc = code;
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Unit;
			part.OP_CountDecimalPlaces = 0;
			part.OP_Weight = 2.0m;
			part.OP_Cubic = 0.02m;
			part.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			part.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			part.RelatedOrganisations.AddOrganisationIfNotExist(ownerPK, OrgPartRelation.RelationshipTypes.Owner);
			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = part.OP_StockKeepingUnit;
			partUnit.OF_ParentPackType = Core.Constants.PkgUnit.Carton;
			partUnit.OF_QuantityInParent = 12;

			return part;
		}

		#region Implementation

		protected virtual string CountryCode
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		protected virtual ZString PivotChildType
		{
			get { return ClassificationTypeProvider.GetProviderFor(CountryCode).HTBCode; }
		}

		public const string InventoryAvailableStatusCode = "AVL";
		public const string InventoryHeldStatusCode = "HEL";

		IDisposable countrySetter;
		protected override void SetUp()
		{
			SetupCountrySetterAndInvoicing();
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TearDownCountrySetter();
		}

		readonly BusinessObjectFactory factory;

		protected override BusinessObjectFactory NewFactory()
		{
			return factory ?? base.NewFactory();
		}

		#region IWhsDataTestHelper Members

		BusinessObjectFactory IWhsDataTestHelper.Factory => Factory;
		BaseCusClassification IWhsDataTestHelper.Classification => Classification;
		BaseCusClassification IWhsDataTestHelper.Classification2 => Classification2;
		OrgSupplierPart IWhsDataTestHelper.OwnerPart => OwnerPart;
		OrgSupplierPart IWhsDataTestHelper.OwnerPart2 => OwnerPart2;
		BaseCusClassPartPivot IWhsDataTestHelper.OwnerPart2Pivot => OwnerPart2Pivot;
		BaseCusClassPartPivot IWhsDataTestHelper.OwnerPartPivot => OwnerPartPivot;
		OrgSupplierPart IWhsDataTestHelper.Part => Part;
		OrgSupplierPart IWhsDataTestHelper.Part2 => Part2;
		BaseCusClassPartPivot IWhsDataTestHelper.Pivot => Pivot;
		BaseCusClassPartPivot IWhsDataTestHelper.Pivot2 => Pivot2;

		OrgSupplierPart IWhsDataTestHelper.CreateProduct(ZGuid ownerPK, ZString code)
		{
			return CreateProduct(ownerPK, code);
		}

		BaseJobDeclaration IWhsDataTestHelper.GetNewDeclaration(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			return GetNewDeclaration(messageType, declarationReference, entryNumber, quantity);
		}

		#endregion

		#endregion
	}
}
