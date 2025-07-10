using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	enum ChargeCode
	{
		DGCIN,
		WBFIN,
		YACCF,
		DGCOUT,
		INFOUT,
		STORAGE,
	}

	public class CYDYardTestHelper : WhsTestHelperFunctionsEnv
	{
		public CYDYardTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ClientRate CreateClientRate(OrgHeader client)
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;
			return clientRate;
		}

		public RateEntry CreateRateEntry(ClientRate clientRate, RateEntryParameters rateEntryParams)
		{
			var entry = clientRate.AddRateEntry(rateEntryParams.RateCategory, rateEntryParams.Mode);
			UpdateRateEntry(entry, rateEntryParams);

			return entry;
		}

		public void UpdateRateEntry(RateEntry entry, RateEntryParameters rateEntryParams)
		{
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_RateStartDate = rateEntryParams.StartDate.IsEmpty ? ZDateTime.Now.AddDays(-5).Date : rateEntryParams.StartDate;
			entry.TI_RateEndDate = rateEntryParams.EndDate;
			entry.TI_WW_Warehouse = rateEntryParams.YardPk;
			entry.TI_Mode = rateEntryParams.Mode;
			entry.TI_YardUnitType = rateEntryParams.UnitType;
			entry.TI_YardUnitLoad = rateEntryParams.UnitLoad;
			entry.TI_RC = string.IsNullOrEmpty(rateEntryParams.ContainerType) ? ZGuid.Empty : Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, rateEntryParams.ContainerType).PK;
			entry.TI_MatchContainerRateClass = rateEntryParams.ContainerClassMatch;

			if (rateEntryParams is CYDRateEntryParameters cYDRateEntryParameters)
			{
				CreateRateLineForUnit(entry, "LIFTIN", "Container liftin charge", ChargeCodeGroupList.Codes.YardGateIn, cYDRateEntryParameters.LiftInCharge, UnitCalculator.Code);
				CreateRateLineForUnit(entry, "LIFTIN+", "Container liftin surcharge", ChargeCodeGroupList.Codes.YardGateIn, cYDRateEntryParameters.LiftInChargePlus, UnitCalculator.Code);
				CreateRateLineForUnit(entry, "LIFTOUT", "Container liftout charge", ChargeCodeGroupList.Codes.YardGateOut, cYDRateEntryParameters.LiftOutCharge, UnitCalculator.Code);
				CreateRateLineForUnit(entry, "LIFTOUT+", "Container liftout surcharge", ChargeCodeGroupList.Codes.YardGateOut, cYDRateEntryParameters.LiftOutChargePlus, UnitCalculator.Code);
				CreateRateLineForUnit(entry, "STORAGE", "Storage Charge", ChargeCodeGroupList.Codes.YardStorage, cYDRateEntryParameters.StorageCharge, TimeCalculator.Code);
			}
			else if (rateEntryParams is CYURateEntryParameters cYURateEntryParameters)
			{
				entry.TI_OH_ControllingCustomer = cYURateEntryParameters.Client;
				CreateRateLineForUnit(entry, "DGCIN", "Depot Gate In Charge", ChargeCodeGroupList.Codes.YardTransportationUnitGateIn, cYURateEntryParameters.DepotGateInCharge, UnitCalculator.Code);
				CreateRateLineForUnit(entry, "WBFIN", "Weigh Bridge Fee", ChargeCodeGroupList.Codes.YardTransportationUnitGateIn, cYURateEntryParameters.WeighBridgeFee, UnitCalculator.Code);
				CreateRateLineForUnit(entry, "DGCOUT", "Depot Gate Out Charge", ChargeCodeGroupList.Codes.YardTransportationUnitGateOut, cYURateEntryParameters.DepotGateOutCharge, UnitCalculator.Code);
				CreateRateLineForUnit(entry, "INFOUT", "Infrastructure Levy", ChargeCodeGroupList.Codes.YardTransportationUnitGateOut, cYURateEntryParameters.InfrastructureLevy, UnitCalculator.Code);
				if (cYURateEntryParameters.VehicleAccessFee > 0)
				{
					CreateRateLineForUnit(entry, "YACCF", "Vehicle Access Fee", ChargeCodeGroupList.Codes.YardTransportationUnitGateIn, cYURateEntryParameters.VehicleAccessFee, FlatCalculator.Code);
				}
			}
			else if (rateEntryParams is CYMRateEntryParameters cYMRateEntryParameters)
			{
				CreateRateLineForUnit(entry, cYMRateEntryParameters.MaterialChargeCode, "MNR Charges", ChargeCodeGroupList.Codes.MNRWorkOrderHeader, 0, CombinedBreaksWithIncrementCalculator.Code, quantityUnit: cYMRateEntryParameters.MeasurementUnit, cbiCalculatorParametersForMaterial: cYMRateEntryParameters.CBICalculatorParametersForMaterial);
				if (!string.IsNullOrEmpty(cYMRateEntryParameters.LabourChargeCode))
				{
					CreateRateLineForUnit(entry, cYMRateEntryParameters.LabourChargeCode, "LBR Charges", ChargeCodeGroupList.Codes.LabourHourRate, 0, CombinedBreaksWithIncrementCalculator.Code, quantityUnit: cYMRateEntryParameters.MeasurementUnit, cbiCalculatorParametersForLabour: cYMRateEntryParameters.CBICalculatorParametersForLabour);
				}
			}
		}

		RateLine CreateRateLineForUnit(RateEntry rateEntry, ZString code, ZString description, ZString chargeGroup, int amount, string calculatorCode, string chargeSubGroup = "", string chargeType = ChargeType.Revenue, string quantityUnit = "UNT", List<CBICalculatorParametersForMaterial> cbiCalculatorParametersForMaterial = null, List<CBICalculatorParametersForLabour> cbiCalculatorParametersForLabour = null)
		{
			var chargeCode = GetOrCreateChargeCode(code, rateEntry.TI_RateCategory, description, chargeGroup, chargeSubGroup, chargeType);
			var rateLine = rateEntry.AddRateLine(chargeCode, calculatorCode, quantityUnit);
			if (calculatorCode == UnitCalculator.Code)
			{
				rateLine.GetCalculator<UnitCalculator>().PerUnit = amount;
			}
			else if (calculatorCode == FlatCalculator.Code)
			{
				rateLine.GetCalculator<FlatCalculator>().BaseRate = amount;
			}
			else if (calculatorCode == CombinedBreaksWithIncrementCalculator.Code)
			{
				var calculator = rateLine.GetCalculator<CombinedBreaksWithIncrementCalculator>();
				if (cbiCalculatorParametersForMaterial != null)
				{
					foreach (var parameter in cbiCalculatorParametersForMaterial)
					{
						AddRateLineItem(calculator, parameter.ItemType, parameter.BreakAmount, parameter.Value);
					}
				}

				if (cbiCalculatorParametersForLabour != null)
				{
					foreach (var parameter in cbiCalculatorParametersForLabour)
					{
						AddRateLineItem(calculator, parameter.ItemType, parameter.BreakAmount, breakHour: parameter.BreakHour, breakHourRate: parameter.BreakHourRate);
					}
				}
			}
			else if (calculatorCode == TimeCalculator.Code)
			{
				_ = rateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, amount, QuantityUnit.DY);
				rateLine.GetCalculator<TimeCalculator>().ExcludeHolidays = string.Empty;
			}
			return rateLine;
		}

		RateLineItem AddRateLineItem(Calculator calculator, string itemType, decimal breakAmount, decimal relevantValue = 0, decimal breakHour = 0, decimal breakHourRate = 0)
		{
			var rateLineItem = calculator.RateLineItems.AddNew();
			rateLineItem.TM_Type = itemType;
			rateLineItem.TM_Break = breakAmount;
			rateLineItem.TM_Value = relevantValue;
			rateLineItem.TM_BreakHour = breakHour;
			rateLineItem.TM_BreakHourRate = breakHourRate;

			return rateLineItem;
		}

		public AccChargeCode GetOrCreateChargeCode(ZString code, ZString rateCategory, ZString description, ZString chargeGroup, string chargeSubGroup = "", string chargeType = ChargeType.Revenue)
		{
			var result = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, code));

			if (result == null)
			{
				result = Factory.NewWithValidTestData<AccChargeCode>();

				result.AC_Code = code;
				result.AC_Desc = $"{rateCategory} - {chargeGroup} {description}";
				result.AC_ChargeGroup = chargeGroup;
				result.AC_ChargeSubGroup = chargeSubGroup;
				result.AC_ChargeType = chargeType;
				result.AC_IsActive = true;
				result.AC_DepartmentFilterList = "ALL";
				result.SetGLAccountDataForTesting();

				var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
				rate.SetRateNumerator_ForTestOnly(0);
				rate.Factory.Save();

				result.AC_AT_GSTRate = rate.PK;
			}

			return result;
		}

		public CYDReceiveAdvice CreateReceiveAdvice(OrgHeader client, WhsWarehouse yard)
		{
			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.Client.E2_OA_Address = client.MainAddress.PK;
			receiveAdvice.YRA_WW_Yard = yard.PK;

			return receiveAdvice;
		}

		public CYDReceiveAdvice CreateReceiveAdvice(OrgHeader client, WhsWarehouse yard, string acceptanceNumber)
		{
			var receiveAdvice = CreateReceiveAdvice(client, yard);
			receiveAdvice.YRA_AcceptanceNumber = acceptanceNumber;

			return receiveAdvice;
		}

		public MNRWorkOrderHeader CreateMNRWorkOrderHeader(WhsWarehouse yard, ZGuid parentId)
		{
			var mnrWorkOrderHeader = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			mnrWorkOrderHeader.MWO_ParentID = parentId;
			mnrWorkOrderHeader.MWO_WW_Facility = yard.PK;

			return mnrWorkOrderHeader;
		}

		public MNRSurvey CreateMNRSurvey(MNRWorkOrderHeader mnrWorkOrderHeader, ZGuid parentId)
		{
			var survey = Factory.NewWithValidTestData<MNRSurvey>();
			survey.MRS_ParentID = parentId;
			survey.MRS_MWO_MNRWorkOrderHeader = mnrWorkOrderHeader.PK;

			return survey;
		}

		public MNRWorkOrderLine CreateMNRWorkOrderLine(MNRWorkOrderHeader mnrWorkOrderHeader, RefUnitSection refUnitSection, RefMRComponentCode refComponent, RefMaterial refMaterial, RefRepairCode refRepair, decimal length, decimal width, short quantity = 1)
		{
			var mnrWorkOrderLine = Factory.NewWithValidTestData<MNRWorkOrderLine>();
			mnrWorkOrderLine.MWL_MWO_MNRWorkOrderHeader = mnrWorkOrderHeader.PK;
			mnrWorkOrderLine.MWL_RUS_UnitSection = refUnitSection.PK;
			mnrWorkOrderLine.MWL_RCC_ComponentCode = refComponent.PK;
			mnrWorkOrderLine.MWL_RMC_Material = refMaterial.PK;
			mnrWorkOrderLine.MWL_RRC_RepairCode = refRepair.PK;
			mnrWorkOrderLine.MWL_Length = length;
			mnrWorkOrderLine.MWL_Width = width;
			mnrWorkOrderLine.MWL_UnitOfDimension = "M";
			mnrWorkOrderLine.MWL_MaterialQuantity = quantity;

			return mnrWorkOrderLine;
		}

		public RefMaterial CreateRefMaterial(string code, string description, string group = "CEDEX")
		{
			var refMaterial = Factory.New<RefMaterial>();
			refMaterial.RMC_Group = group;
			refMaterial.RMC_Code = code;
			refMaterial.RMC_Description = description;

			return refMaterial;
		}

		public RefUnitSection CreateRefUnitSection(string code, string description, string group = "CEDEX")
		{
			var refUnitSection = Factory.New<RefUnitSection>();
			refUnitSection.RUS_Code = code;
			refUnitSection.RUS_Group = group;
			refUnitSection.RUS_Description = description;

			return refUnitSection;
		}

		public RefMRComponentCode CreateRefMRComponentCode(string code, string description, string group = "CEDEX")
		{
			var refComponent = Factory.New<RefMRComponentCode>();
			refComponent.RCC_Group = group;
			refComponent.RCC_Code = code;
			refComponent.RCC_Description = description;

			return refComponent;
		}

		public RefRepairCode CreateRefRepairCode(string code, string description, string group = "CEDEX", string serviceType = "RPR")
		{
			var refRepair = Factory.New<RefRepairCode>();
			refRepair.RRC_Group = group;
			refRepair.RRC_Code = code;
			refRepair.RRC_Description = description;
			refRepair.RRC_ServiceType = serviceType;

			return refRepair;
		}

		public CYDYardUnitState AddReceiveAdviceLine(CYDReceiveAdvice receiveAdvice, string unitID, string containerTypeCode = "20GP", bool isContainerEmpty = false)
		{
			var unitLineItem = Factory.NewWithValidTestData<CYDUnitLineItem>();
			unitLineItem.YLI_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode)).PK;
			unitLineItem.YLI_IsEmpty = isContainerEmpty;

			var receiveAdviceLine = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;
			receiveAdviceLine.YRL_YLI_UnitLineItem = unitLineItem.PK;

			var yardUnit = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnit.YUS_WW_CurrentYard = receiveAdvice.YRA_WW_Yard;
			yardUnit.YUS_YRL_ReceiveLine = receiveAdviceLine.PK;
			yardUnit.YUS_YLI_UnitLineItem = unitLineItem.PK;
			yardUnit.YUS_UnitID = unitID;

			return yardUnit;
		}

		public void UnloadYardUnit(CYDYardUnitState yardUnit, ZDateTimeOffset unloadTime, WhsLocation location, string containerTypeCode = "20GP", bool isContainerEmpty = false)
		{
			var unitLineItem = Factory.NewWithValidTestData<CYDUnitLineItem>();
			unitLineItem.YLI_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode)).PK;
			unitLineItem.YLI_IsEmpty = isContainerEmpty;

			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			delivery.YDL_YLI_UnitLineItem = unitLineItem.PK;

			var receiveTransportationUnit = CreateTransportationUnit("ADCEDS");
			delivery.YDL_YTU_DeliveryTransportationUnit = receiveTransportationUnit.PK;

			var yardUnitLineItem = Factory.NewWithValidTestData<CYDUnitLineItem>();
			yardUnitLineItem.YLI_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode)).PK;
			yardUnitLineItem.YLI_IsEmpty = isContainerEmpty;

			yardUnit.YUS_YDL_Delivery = delivery.PK;
			yardUnit.YUS_UnloadTime = unloadTime;
			yardUnit.YUS_GS_NKUnloadUser = "~BP";
			yardUnit.YUS_WL_CurrentYardLocation = location.PK;
			yardUnit.YUS_YLI_UnitLineItem = yardUnitLineItem.PK;
			yardUnit.YUS_YTU_ReceiveTransportationUnit = receiveTransportationUnit.PK;
		}

		public CYDReleaseAdvice CreateReleaseAdvice(OrgHeader client, WhsWarehouse yard)
		{
			var releaseAdvice = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			releaseAdvice.Client.E2_OA_Address = client.MainAddress.PK;
			releaseAdvice.YRE_WW_Yard = yard.PK;

			return releaseAdvice;
		}

		public CYDReleaseAdvice CreateReleaseAdvice(OrgHeader client, WhsWarehouse yard, string releaseNumber)
		{
			var releaseAdvice = CreateReleaseAdvice(client, yard);
			releaseAdvice.YRE_ReleaseNumber = releaseNumber;

			return releaseAdvice;
		}

		public void AddReleaseAdviceLine(CYDReleaseAdvice releaseAdvice, CYDYardUnitState yardUnit)
		{
			var releaseAdviceLine = Factory.NewWithValidTestData<CYDReleaseAdviceLine>();
			releaseAdviceLine.YEL_YRE_ReleaseAdvice = releaseAdvice.PK;
			yardUnit.YUS_YEL_ReleaseLine = releaseAdviceLine.PK;
			releaseAdviceLine.YEL_YLI_UnitLineItem = yardUnit.ReceiveAdviceLine.YRL_YLI_UnitLineItem;
		}

		public void LoadYardUnit(CYDYardUnitState yardUnit, ZDateTimeOffset loadTime, string containerTypeCode = "20GP", bool isContainerEmpty = false)
		{
			if (yardUnit.YUS_YEL_ReleaseLine == ZGuid.Empty)
			{
				throw new InvalidOperationException("Cannot load a yard unit that has no release advice");
			}

			var unitLineItem = Factory.NewWithValidTestData<CYDUnitLineItem>();
			unitLineItem.YLI_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode)).PK;
			unitLineItem.YLI_IsEmpty = isContainerEmpty;

			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			pickup.YPL_YLI_UnitLineItem = unitLineItem.PK;

			var dispatchTransportationUnit = CreateTransportationUnit("ADCEDS");
			pickup.YPL_YTU_PickupTransportationUnit = dispatchTransportationUnit.PK;

			yardUnit.YUS_LoadTime = loadTime;
			yardUnit.YUS_GS_NKLoadUser = "~BP";
			yardUnit.YUS_WL_CurrentYardLocation = ZGuid.Empty;
			yardUnit.YUS_YPL_Pickup = pickup.PK;
			yardUnit.YUS_YTU_DispatchTransportationUnit = pickup.YPL_YTU_PickupTransportationUnit;
		}

		public void SetContainerHandlingRateClass(string rateClass, string containerType)
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
			container.RC_HandlingRateClass = rateClass;
		}

		public CYDTransportationUnit CreateTransportationUnit(string transportationReference, string transportationID = null)
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			transportationUnit.YTU_TransportationReference = transportationReference;
			if (transportationID != null)
			{
				transportationUnit.YTU_TransportationUnitID = transportationID;
			}

			return transportationUnit;
		}

		public CYDTransportationUnit CreateTransportationUnit(string transportationReference, OrgHeader transportor, WhsWarehouse yard)
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			transportationUnit.YTU_TransportationReference = transportationReference;
			transportationUnit.TransportCompanyDocAddress.E2_OA_Address = transportor.MainAddress.PK;
			transportationUnit.YTU_WW_Yard = yard.PK;

			return transportationUnit;
		}

		public CYDDelivery AddDelivery(CYDTransportationUnit transportationUnit, CYDYardUnitState yardUnit)
		{
			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			delivery.YDL_YTU_DeliveryTransportationUnit = transportationUnit.PK;
			delivery.YDL_YRL_ReceiveAdviceLine = yardUnit.YUS_YRL_ReceiveLine;
			delivery.YDL_YLI_UnitLineItem = yardUnit.ReceiveAdviceLine.YRL_YLI_UnitLineItem;
			yardUnit.YUS_YDL_Delivery = delivery.PK;
			yardUnit.YUS_YTU_ReceiveTransportationUnit = transportationUnit.PK;

			return delivery;
		}

		public CYDPickup AddPickup(CYDTransportationUnit transportationUnit, CYDYardUnitState yardUnit)
		{
			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			pickup.YPL_YTU_PickupTransportationUnit = transportationUnit.PK;
			pickup.YPL_YEL_ReleaseAdviceLine = yardUnit.YUS_YEL_ReleaseLine;
			pickup.YPL_YLI_UnitLineItem = yardUnit.ReleaseAdviceLine.YEL_YLI_UnitLineItem;
			yardUnit.YUS_YPL_Pickup = pickup.PK;
			yardUnit.YUS_YTU_DispatchTransportationUnit = transportationUnit.PK;

			return pickup;
		}

		public void GateInTransportationUnit(CYDTransportationUnit transportationUnit, ZDateTimeOffset gateInTime, WhsLocation waitingBay)
		{
			transportationUnit.YTU_WL_WaitingBayLocation = waitingBay.PK;
			transportationUnit.YTU_GateInTime = gateInTime;
		}

		public void GateOutTransportationUnit(CYDTransportationUnit transportationUnit, ZDateTimeOffset gateOutTime)
		{
			transportationUnit.YTU_GateOutTime = gateOutTime;
		}

		public CYDYardStorageFreeDays CreateYardStorageFreeDays(OrgHeader client, WhsWarehouse yard, ZString unitType, ZDecimal unitLength, ZString containerClass, ZString unitLoad, ZString transportMode, int freeDays)
		{
			var storageFreeDays = Factory.NewWithValidTestData<CYDYardStorageFreeDays>();
			storageFreeDays.YFD_OB_Client = client.CompanyData.PK;
			if (yard != null)
			{
				storageFreeDays.YFD_WW_Yard = yard.PK;
			}
			storageFreeDays.YFD_UnitType = unitType;
			storageFreeDays.YFD_YardUnitLength = unitLength;
			storageFreeDays.YFD_ContainerClass = containerClass;
			storageFreeDays.YFD_UnitLoad = unitLoad;
			storageFreeDays.YFD_TransportMode = transportMode;
			storageFreeDays.YFD_FreeDays = (ZByte)freeDays;
			return storageFreeDays;
		}	

		public PeriodicInvoicing CreatePeriodicInvoicing(OrgHeader client, WhsWarehouse yard, ZDateTime fromDate, ZDateTime toDate)
		{
			var periodicInvoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
			periodicInvoice.ET_StorageType = PeriodicInvoicingStorageTypes.Codes.ContainerYard;
			periodicInvoice.ET_OH_Client = client.PK;
			periodicInvoice.ET_WW = yard.PK;
			periodicInvoice.ET_StorageFromDate = fromDate;
			periodicInvoice.ET_StorageToDate = toDate;
			return periodicInvoice;
		}

		public CYDAdHocServiceOrder CreateAdHocServiceOrder(WhsWarehouse yard)
		{
			var adHocServiceOrder = Factory.NewWithValidTestData<CYDAdHocServiceOrder>();
			adHocServiceOrder.YAO_WW_Facility = yard.PK;
			return adHocServiceOrder;
		}

		public CYDAdHocService CreateAdHocServiceWithJobService(CYDAdHocServiceOrder adHocServiceOrder, CYDYardUnitState yardUnitState, ZDateTimeOffset bookedDate, ZDateTimeOffset completedDate)
		{
			var jobService = Factory.NewWithValidTestData<JobService>();
			jobService.ES_ParentTableCode = "YAO";
			jobService.ES_ParentID = adHocServiceOrder.PK;
			jobService.ES_BookedDateTimeOffset = bookedDate;
			jobService.ES_CompletedDateTimeOffset = completedDate;
			Factory.Save();

			var adHocService = Factory.NewWithValidTestData<CYDAdHocService>();
			adHocService.YAS_YAO_ServiceOrder = adHocServiceOrder.PK;
			adHocService.YAS_YUS_YardUnitState = yardUnitState.PK;
			adHocService.YAS_ES_JobService = jobService.PK;
			return adHocService;
		}

		public CYDYardStorageFreeDays CreateYardStorageFreeDays(OrgHeader client, WhsWarehouse yard, ZByte freeDays)
		{
			var storageFreeDays = Factory.NewWithValidTestData<CYDYardStorageFreeDays>();
			storageFreeDays.YFD_OB_Client = client.CompanyData.PK;
			storageFreeDays.YFD_WW_Yard = yard.PK;
			storageFreeDays.YFD_FreeDays = freeDays;
			return storageFreeDays;
		}

		public void SetExcludeHolidays(RateLine rateLine)
		{
			rateLine.GetCalculator<TimeCalculator>().ExcludeHolidays = Calculator.Items.ExcludeWeekendsAndPublicHolidays;
		}

		public void SetClientCalculationMethod(OrgHeader client, string calculationMethod)
		{
			client.CompanyData.OB_ARYardStorageCalcMethod = calculationMethod;
		}

		public OrgHeader CreateDebtor(string code)
		{
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.OH_Code = code;
			return debtor;
		}

		#region Warehouse

		public override WhsWarehouse CreateWarehouse(ZString name, bool shouldPreGenerateDDL = false)
		{
			var whs = base.CreateWarehouse(name, shouldPreGenerateDDL);
			whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			return whs;
		}

		public WhsWarehouse CreateContainerYardInCurrentBranch(string code = "WH1", OrgAddress address = null)
		{
			if (address == null)
			{
				address = Factory.NewWithValidTestData<OrgAddress>();
			}
			var whs = CreateWarehouse(code, address, GlbBranch.CurrentBranch);
			whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			return whs;
		}

		public WhsWarehouse GetOrCreateContainerYardInCurrentBranch()
		{
			return ContainerYardHelper.GetContainerYardInCurrentBranch(Factory) ?? CreateContainerYardInCurrentBranch();
		}

		#endregion
	}
}
