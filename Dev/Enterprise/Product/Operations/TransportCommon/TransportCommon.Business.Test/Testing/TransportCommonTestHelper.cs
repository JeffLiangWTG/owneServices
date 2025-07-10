using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business.Testing
{
	public class TransportCommonTestHelper
	{
		public TransportCommonTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public readonly BusinessObjectFactory Factory;

		#region Confirmation

		public DtbTransportConfirmation CreateConfirmation(DtbTransportInstruction instruction, ZString confirmationTypeCode)
		{
			var result = (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			result.KK_ConfirmationType = confirmationTypeCode;
			return result;
		}

		public DtbTransportConfirmation CreateConfirmation(DtbTransportInstructionPkgDivot packageDivot, ZString confirmationTypeCode)
		{
			return CreateConfirmation(packageDivot, confirmationTypeCode, packageDivot.KD_Quantity);
		}

		public DtbTransportConfirmation CreateConfirmation(DtbTransportInstructionPkgDivot packageDivot, ZString confirmationTypeCode, ZInt quantity)
		{
			var result = (DtbTransportConfirmation)packageDivot.Confirmations.AddNew();
			result.KK_ConfirmationType = confirmationTypeCode;
			result.KK_Quantity = quantity;
			return result;
		}

		#endregion

		#region CreateServiceLevel

		public RefServiceLevel CreateServiceLevel(ZString code)
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = code;
			return serviceLevel;
		}

		#endregion

		#region CreateTransitTime

		public RefTransitTime CreateTransitTime(ZString transitTimeserviceLevelCode, ZInt transitHours, ZGuid originDomesticZonePK, ZGuid destinationDomesticZonePK)
		{
			var transitTime = Factory.New<RefTransitTime>();
			transitTime.RTT_RS_NKServiceLevel = transitTimeserviceLevelCode;
			transitTime.RTT_TransitHours = transitHours;
			transitTime.RTT_TZ_OriginDomesticZone = originDomesticZonePK;
			transitTime.RTT_TZ_DestinationDomesticZone = destinationDomesticZonePK;
			return transitTime;
		}

		#endregion

		#region PortHub

		public IPortHubSelection CreatePortHub(OrgAddress depotAddress)
		{
			var portHub = Factory.New<IPortHubSelection>();
			portHub.TY_OA_DepotAddress = depotAddress.PK;

			return portHub;
		}

		public IPortHubZonePivot CreatePortHubZonePivot(IPortHubSelection portHub, RateTransportZone zone)
		{
			var pivot = Factory.New<IPortHubZonePivot>();
			pivot.TX_TY_Hub = ((BusinessObject)portHub).PK;
			pivot.TX_TZ_Zone = zone.PK;

			return pivot;
		}

		public void SetPortHubDirection(IPortHubSelection portHub, ZString direction)
		{
			portHub.TY_Direction = direction;
		}

		#endregion

		#region Driver

		public GlbStaff CreateDriver(ZString driverName, ZString loginName)
		{
			var driver = Factory.New<GlbStaff>();
			driver.GS_FullName = driverName;
			driver.GS_LoginName = loginName;
			driver.GS_Code = driverName.SubstringSafe(0, 3);

			return driver;
		}

		public GlbStaff CreateDriver(ZString driverName, ZString loginName, string certificateType, string licenseCode)
		{
			var driver = CreateDriver(driverName, loginName);

			var certification = driver.Certificates.AddNew();
			certification.XZ_Type = certificateType;
			certification.XZ_RefNumber = licenseCode;

			return driver;
		}

		public GlbGroup CreateDriverGroup(ZString groupCode, params GlbStaff[] drivers)
		{
			var driversGroup = Factory.New<GlbGroup>();
			driversGroup.GG_Code = groupCode;
			driversGroup.Staff.AddRange(drivers);

			return driversGroup;
		}

		#endregion

		#region Instruction

		public DtbTransportInstruction CreateInstruction(DtbTransport transport, ZString instructionType, ZString orgType, OrgAddress address)
		{
			var result = CreateInstruction(transport, instructionType);

			result.OrganisationType = orgType;
			if (address != null)
			{
				result.Address.E2_AddressOverride = false;
				result.Address.E2_OA_Address = address.PK;
			}

			return result;
		}

		public DtbTransportInstruction CreateInstruction(DtbTransport transport, string instructionType = InstructionTypes.Codes.PickUp)
		{
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			instruction.KN_InstructionType = instructionType;
			return instruction;
		}

		#endregion

		#region Organisation

		public OrgHeader CreateOrganisation(ZString code, bool isDebtor = false, string address1 = "Main st")
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = code;
			organisation.MainAddress.OA_Address1 = address1;
			if (isDebtor)
			{
				organisation.OH_IsDebtor = isDebtor;
			}

			return organisation;
		}

		public OrgAddress AddAddressToOrganisation(OrgHeader organisation, ZString dropMode)
		{
			return AddAddressToOrganisation(organisation, "", OrgAddressType.Office, dropMode);
		}

		public OrgAddress AddAddressToOrganisation(OrgHeader organisation, ZString address1, OrgAddressType addressType)
		{
			return AddAddressToOrganisation(organisation, address1, addressType, "");
		}

		public OrgAddress AddAddressToOrganisation(OrgHeader organisation, ZString address1, OrgAddressType addressType, ZString dropMode)
		{
			var address = organisation.Addresses.AddNew();

			address.AddressCapability.SetCapabilityEnabled(addressType);
			address.AddressCapability.SetIsMainAddress(addressType);
			address.OA_Address1 = address1;

			return address;
		}

		#endregion

		#region Package

		public PkgPackage CreatePackage(ZString packageID, int quantity)
		{
			return CreatePackage(packageID, quantity, "PKG");
		}

		public PkgPackage CreatePackage(ZString packageID, int quantity, string packageType)
		{
			var result = CreatePackage(packageID, null, quantity);
			result.KP_F3_NKPackType = packageType;

			return result;
		}

		public PkgPackage CreatePackage(ZString packageID, DtbTransportInstructionPkgDivot packageDivot = null, int quantity = 1, decimal weight = 0, decimal volume = 0)
		{
			var result = Factory.New<PkgPackage>();

			result.KP_F3_NKPackType = "PKG";
			result.KP_PackageID = packageID;
			result.KP_PackageQty = quantity;
			result.KP_Weight = weight;
			result.KP_Volume = volume;

			if (packageDivot != null)
			{
				packageDivot.KD_KP_Package = result.PK;
			}

			return result;
		}

		#region CreatePackage

		public PkgPackage CreatePackageContainer(ZString packageID, DtbTransportInstructionPkgDivot packageDivot = null, int quantity = 1, decimal weight = 0, decimal volume = 0, string containerType = "20GP")
		{
			var result = CreatePackage(packageID, packageDivot, quantity, weight, volume);
			result.KP_F3_NKPackType = Constants.PkgUnit.Container;
			result.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;

			// assigning specific container type is overriding Weight/Volume
			result.KP_Weight = weight;
			result.KP_Volume = volume;

			return result;
		}

		#endregion

		#endregion

		#region Package Divot

		public DtbTransportInstructionPkgDivot CreatePackageDivot(DtbTransportInstruction instruction, int qty = 1)
		{
			return CreatePackageDivot(instruction, null, qty);
		}

		public DtbTransportInstructionPkgDivot CreatePackageDivot(DtbTransportInstruction instruction, PkgPackage package, int qty)
		{
			var result = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			if (package != null)
			{
				result.KD_KP_Package = package.PK;
			}
			result.KD_Quantity = qty;
			return result;
		}

		#endregion

		#region Template Instruction

		public DtbTransportInstructionTmpl AddInstructionToTemplate(DtbTransportTmpl template, ZString orgType, ZString instructionType, bool isContainerRateable = false, bool isLooseRateable = false, string packageCategory = "")
		{
			var instruction = template.Instructions.AddNew();
			instruction.K2_OrgType = orgType;
			instruction.K2_InstructionType = instructionType;
			instruction.K2_IsContainerRateable = isContainerRateable;
			instruction.K2_IsLooseRateable = isLooseRateable;
			instruction.K2_PackageType = packageCategory;
			instruction.K2_Sequence = template.Instructions.Count;

			return instruction;
		}

		#endregion

		#region Vehicle

		public RefEquipment CreateVehicle(ZString vehicleCode, string registrationCode = "")
		{
			var vehicle = CreateVehicle(null, vehicleCode);
			if (!string.IsNullOrEmpty(registrationCode))
			{
				vehicle.RQ_Registration = registrationCode;
			}
			return vehicle;
		}

		public RefEquipment CreateVehicle(GlbStaff driver, ZString vehicleCode)
		{
			var truck = Factory.New<RefEquipment>();
			truck.RQ_GS_NKPreferredDriver = (driver != null) ? driver.GS_Code : ZString.Empty;
			truck.RQ_ShortCode = vehicleCode;
			truck.RQ_IsVehicle = true;

			return truck;
		}

		public RefEquipment CreateVehicleWithEquipmentType(ZString vehicleCode, string registrationCode)
		{
			var vehicle = CreateVehicle(vehicleCode, registrationCode);
			var equipmentType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "RTRK").PK;
			vehicle.RQ_RC_RoadContainerType = equipmentType;

			return vehicle;
		}

		#endregion

		#region Zone

		public RateTransportZone CreateZone(string name)
		{
			return CreateZone(name, CreateZoneRateProvider());
		}

		public RateTransportZone CreateZone(string name, RateTransportProvider provider)
		{
			var zone = provider.Zones.AddNew();
			zone.TZ_ZoneName = name;

			return zone;
		}

		public RateTransportZone CreateZone(string name, ZString zoneType)
		{
			var provider = CreateZoneRateProvider(zoneType);
			var zone = CreateZone(name, provider);
			zone.TZ_ZoneName = name;

			return zone;
		}

		public RateTransportZone CreateZoneWithPostCodes(string zoneName, RateTransportProvider provider, string fromPostCode, string toPostCode)
		{
			var zone = provider.Zones.AddNew();
			zone.TZ_ZoneName = zoneName;

			var refPostCodeFrom = Factory.New<RefPostCode>();
			refPostCodeFrom.RK_CityTownPostCode = fromPostCode;
			var refPostCodeTo = Factory.New<RefPostCode>();
			refPostCodeTo.RK_CityTownPostCode = toPostCode;

			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_FromPostCode = refPostCodeFrom.RK_CityTownPostCode;
			zoneItem.TQ_ToPostCode = refPostCodeTo.RK_CityTownPostCode;

			return zone;
		}

		public RateTransportZone CreateZoneWithCity(string zoneName, RateTransportProvider provider, string city, string postCode = "")
		{
			var zone = provider.Zones.AddNew();
			zone.TZ_ZoneName = zoneName;

			AddCityToZone(zone, city, postCode);

			return zone;
		}

		public RateTransportZoneItem AddCityToZone(RateTransportZone zone, string city, string postcode = "")
		{
			return AddCityToZone(zone, CreateCityTown(city, "", postcode));
		}

		public RateTransportZoneItem AddCityToZone(RateTransportZone zone, RefCityTown cityTown)
		{
			var item = zone.Items.AddNew();
			item.TQ_R9_CityTown = cityTown.PK;

			return item;
		}

		public RateTransportZoneItem AddPostCodeToZone(RateTransportZone zone, string postCode)
		{
			var item = zone.Items.AddNew();
			var refPostCode = Factory.New<RefPostCode>();
			refPostCode.RK_CityTownPostCode = postCode;
			refPostCode.RK_RN_NKCountry = "AU";
			item.TQ_FromPostCode = refPostCode.RK_CityTownPostCode;

			return item;
		}

		public RateTransportZoneItem AddPostCodesToZone(RateTransportZone zone, string fromPostCode, string toPostCode)
		{
			var item = zone.Items.AddNew();

			var refPostCodeFrom = Factory.New<RefPostCode>();
			refPostCodeFrom.RK_CityTownPostCode = fromPostCode;
			refPostCodeFrom.RK_RN_NKCountry = "AU";

			var refPostCodeTo = Factory.New<RefPostCode>();
			refPostCodeTo.RK_CityTownPostCode = toPostCode;
			refPostCodeTo.RK_RN_NKCountry = "AU";

			item.TQ_FromPostCode = refPostCodeFrom.RK_CityTownPostCode;
			item.TQ_ToPostCode = refPostCodeTo.RK_CityTownPostCode;

			return item;
		}

		RateTransportProvider CreateZoneRateProvider()
		{
			return CreateZoneRateProvider("AU");
		}

		RateTransportProvider CreateZoneRateProvider(ZString zoneType)
		{
			return CreateZoneRateProvider(zoneType, "AU");
		}

		public RateTransportProvider CreateZoneRateProvider(string countryCode, OrgHeader transportCo = null, RefCityTown zoneHubLocation = null)
		{
			return CreateZoneRateProvider(RatingConstants.RatingZoneTypes.All, countryCode, transportCo, zoneHubLocation);
		}

		public RateTransportProvider CreateZoneRateProvider(string zoneType, string countryCode, OrgHeader transportCo = null, RefCityTown zoneHubLocation = null)
		{
			var provider = Factory.New<RateTransportProvider>();
			provider.TP_ZoneType = zoneType;
			provider.TP_RN_NKCountry = countryCode;

			if (transportCo != null)
			{
				provider.TP_OH_RelatedParty = transportCo.PK;
			}

			if (zoneHubLocation != null)
			{
				provider.TP_R9_ZoneHubLocation = zoneHubLocation.PK;
			}

			return provider;
		}

		#endregion

		#region CityTown

		public RefCityTown CreateCityTown(string internationalName, string countryCode = "", string postcode = "")
		{
			var cityTown = Factory.NewWithValidTestData<RefCityTown>();
			cityTown.R9_InternationalName = internationalName;
			cityTown.R9_RN_NKCountry = countryCode;
			if (!string.IsNullOrEmpty(postcode))
			{
				cityTown.PostCodes.AddNew().RK_CityTownPostCode = postcode;
			}

			return cityTown;
		}

		#endregion

		#region GetChargeLineOnUniversal

		public ChargeLine GetChargeLineOnUniversal(ZString? chargeCode, ZDecimal? costOSAmount, ZString? creditor, ZString? debtor, ZDecimal? sellOSAmount)
		{
			var chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);
			var now = ZDateTime.Today;
			if (chargeCode.HasValue)
			{
				chargeLine.ChargeCode = new ChargeCode();
				chargeLine.ChargeCode.Code = chargeCode;
				chargeLine.ChargeCode.Description = "International Freight";
			}

			chargeLine.CostAPInvoiceNumber = "001";
			chargeLine.CostOSAmount = costOSAmount;
			chargeLine.Branch = new Branch { Code = "SYD", Name = "Branch" };
			chargeLine.Department = new Department { Code = "FIS", Name = "Test Department" };
			chargeLine.CostDueDate = now;
			chargeLine.CostInvoiceDate = now.AddDays(1);
			chargeLine.CostOSCurrency = new Currency { Code = "AUD" };
			chargeLine.Description = "Charge Description 1";
			chargeLine.DisplaySequence = 1;
			chargeLine.SellInvoiceType = "FIN";
			chargeLine.SellOSCurrency = new Currency { Code = "AUD" };

			if (creditor.HasValue)
			{
				chargeLine.Creditor = new OrganizationReference();
				chargeLine.Creditor.Key = creditor;
				chargeLine.Creditor.Type = nameof(DataContextType.Organization);
			}

			if (debtor.HasValue)
			{
				chargeLine.Debtor = new OrganizationReference();
				chargeLine.Debtor.Key = debtor;
				chargeLine.Debtor.Type = nameof(DataContextType.Organization);
			}

			chargeLine.SellOSAmount = sellOSAmount;

			return chargeLine;
		}

		#endregion

		#region Workflow

		public ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			return result;
		}

		public GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		#endregion

		#region CreateCompany

		public GlbCompany CreateCompany(string code, string name, OrgHeader org)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = code;
			company.GC_Name = name;
			company.GC_OH_OrgProxy = org.PK;
			return company;
		}

		#endregion

		#region CreateBranch

		public GlbBranch CreateBranch(string code, string name, GlbCompany company, OrgHeader org)
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = code;
			branch.GB_BranchName = name;
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = org.PK;
			return branch;
		}

		#endregion
	}
}
