using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportCommon;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;
using EnterpriseBusinessObject = Enterprise.ZArchitecture.EnterpriseBusinessObject;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class TransportBookingTestHelper : ITransportBookingTestHelper
	{
		public TransportBookingTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public readonly BusinessObjectFactory Factory;

		public IDtbConsignmentConsolidation CreateConsignmentConsol()
		{
			return Factory.New<IDtbConsignmentConsolidation>();
		}

		public IDtbConsignmentConsolidation CreateConsignmentConsol(DtbBooking booking)
		{
			var consignmentConsol = CreateConsignmentConsol();
			AddConsignmentConsolToBooking(booking, consignmentConsol);
			return consignmentConsol;
		}

		public void AddConsignmentConsolToBooking(DtbBooking booking, IDtbConsignmentConsolidation consignmentConsol)
		{
			consignmentConsol.KB_ParentID = booking.PK;
			consignmentConsol.KB_ParentTableCode = booking.TablePrefix;
		}

		IDtbBookingConsolidation ITransportBookingTestHelper.CreateConsolidation()
		{
			return CreateConsolidation();
		}

		public DtbBookingConsolidation CreateConsolidation()
		{
			return Factory.New<DtbBookingConsolidation>();
		}

		public DtbBookingConsolidation CreateConsolidation(IDtbBookingParent bookingParent)
		{
			var result = CreateConsolidation();
			AttachConsolidationToParent(result, bookingParent);
			return result;
		}

		public void AttachConsolidationToParent(DtbBookingConsolidation consolidation, IDtbBookingParent bookingParent)
		{
			consolidation.KB_ParentID = bookingParent.PK;
			consolidation.KB_ParentTableCode = bookingParent.TablePrefix;
			consolidation.KB_JobDirection = bookingParent.GetSupportedDirections()[0].ToString();
		}

		public DtbBookingConsolidation CreateConsolidationMultiJob(OrgHeader transportCo = null)
		{
			var result = Factory.New<DtbBookingConsolidation>();

			using (result.SuspendSettingHasChanges())
			{
				result.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

				if (transportCo != null)
				{
					result.Address.OrganisationPK = transportCo.PK;
				}
			}

			return result;
		}

		public GlbDepartment CreateDepartment(string code)
		{
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = code;

			return department;
		}

		IDtbBooking ITransportBookingTestHelper.CreateBooking() => CreateBooking();
		IDtbBooking ITransportBookingTestHelper.CreateBooking(OrgHeader transportCo) => CreateBooking(transportCo);

		public DtbBooking CreateBooking(OrgHeader transportCo = null, bool isMaster = false)
		{
			var consolidation = CreateConsolidation();
			var result = CreateBooking(consolidation);

			result.KM_IsMaster = isMaster;

			if (transportCo != null)
			{
				result.Address.OrganisationPK = transportCo.PK;
			}

			return result;
		}

		IDtbBooking ITransportBookingTestHelper.CreateBooking(IDtbBookingConsolidation consolidation)
		{
			return CreateBooking((DtbBookingConsolidation)consolidation);
		}

		public DtbBooking CreateBooking(DtbBookingConsolidation consolidation)
		{
			return consolidation.Bookings.AddNew();
		}

		public DtbBooking CreateBooking(DtbBookingConsolidation consolidation, ZString bookingTemplate, ZString description, ZString direction, ZString transportReference)
		{
			var booking = CreateBooking(consolidation);
			booking.KM_KT_NKBookingTemplate = bookingTemplate;
			booking.KM_Description = description;
			booking.KM_Direction = direction;
			booking.KM_TransportReference = transportReference;

			return booking;
		}

		public DtbBooking CreateBookingThatWillPassValidationForSendingXUSToCTO(IDtbBookingParent parent = null)
		{
			if (parent == null)
			{
				var shipment = Factory.New<IForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				parent = (IDtbBookingParent)shipment;
			}
			var consolidation = CreateConsolidation(parent);
			var booking = CreateBooking(consolidation);
			booking.ConsolidationSingleJob.KB_ParentID = parent.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = parent.TablePrefix;
			booking.ConsolidationSingleJob.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var additionalReferenceNumber = booking.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber.CE_EntryType = CargoWise.Definitions.TransportAdditionalReferenceTypes.Codes.CarrierBookingReference;
			additionalReferenceNumber.CE_EntryNum = "Carrier booking reference 123";

			CreateInstructionForBookingThatWillPassValidationForSendingXUSToCTO(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, "CONT1111112");
			CreateInstructionForBookingThatWillPassValidationForSendingXUSToCTO(booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, "CONT2222229");
			CreateInstructionForBookingThatWillPassValidationForSendingXUSToCTO(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD, "CONT3333335");

			return booking;
		}

		public DtbBookingInstruction CreateInstructionForBookingThatWillPassValidationForSendingXUSToCTO(DtbBooking booking, string instructionType, string organisationType, string packageID)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var instruction = CreateInstruction(booking, instructionType, organisationType, org.MainAddress);
			instruction.OrganisationType = organisationType;
			instruction.KN_DropMode = Constants.FCLEquipmentNeeded.WaitForUnpack;
			instruction.Address.ValidationStatus = AddressValidationStatus.Verified;
			instruction.Address.E2_City = "City1";
			instruction.Address.Postcode = "1234";
			instruction.Address.E2_CompanyName = "Company1";
			instruction.Address.E2_State = "NSW";

			foreach (var confirmation in instruction.Confirmations)
			{
				confirmation.KK_RequiredFrom = new ZDateTime(2025, 2, 11);
				confirmation.KK_RequiredTo = new ZDateTime(2025, 2, 13);
				confirmation.KK_ReferenceNum = "Ref123";
			}

			var packageDivot = instruction.PackageDivots.AddNew();
			var package = CreatePackage(packageID, 1);
			package.KP_KJ_ParentPackageJob = booking.ConsolidationSingleJob.PackageJob.PK;
			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var twentyGPO = LoadRefContainer("20GP");
			package.Container.K0_RC_ContainerType = twentyGPO.PK;
			packageDivot.KD_KP_Package = package.PK;
			packageDivot.KD_Quantity = package.KP_PackageQty;

			return instruction;
		}

		public Shipment CreateShipmentWithContainerCollection()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());

			return shipment;
		}

		public DtbBookingInstruction CreateContainerYardInstructionForBooking(DtbBooking booking, string instructionType, OrgAddress address)
		{
			var instruction = booking.Instructions.AddNew();
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.KN_InstructionType = instructionType;
			instruction.KN_IsContainerRateable = true;
			instruction.KN_DropMode = "MDE";
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			instruction.Address.E2_OA_Address = address.PK;

			return instruction;
		}

		public PkgPackage CreateContainerPkgPackageOnInstruction(DtbBookingInstruction instruction, string packageID)
		{
			var package = Factory.NewWithValidTestData<PkgPackage>();
			package.KP_PackageID = packageID;
			package.PackageJob.KJ_ParentID = ZGuid.NewZGuid();
			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			package.Container.K0_RC_ContainerType = containerType20GP.PK;
			var package1Divot = instruction.PackageDivots.AddNew();
			package1Divot.KD_KP_Package = package.PK;

			return package;
		}

		public OrgHeader CreateOrgHeader(BusinessObjectFactory factory, string orgHeaderCode)
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = orgHeaderCode;

			return orgHeader;
		}

		public OrgAddress CreateOrgAddress(BusinessObjectFactory factory, OrgHeader orgHeader, string addressCode)
		{
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgAddress.AddressCode = addressCode;
			orgAddress.OA_OH = orgHeader.PK;

			return orgAddress;
		}

		public OrganizationAddress CreateContainerYardOrganizationAddress(string addressType, OrgHeader orgHeader, string addressCode)
		{
			var organizationAddress = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD("CYD");
			organizationAddress.AddressShortCode = addressCode;
			organizationAddress.AddressType = addressType;
			organizationAddress.OrganizationCode = orgHeader.OH_Code;

			return organizationAddress;
		}

		public Container CreateUniversalContainerOnShipment(string containerNumber, List<OrganizationAddress> addressCollection, Shipment shipment, ZInt? link)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			container.ContainerNumber = containerNumber;
			container.Link = link;

			container.SetOrganizationAddressCollection(() => addressCollection);
			shipment.ContainerCollection.Add(container);

			return container;
		}

		public DtbBookingTmpl CreateTransportBookingTemplate(ZString code, ZString description, ZString direction, string ratingFreightMode = "BTH")
		{
			var template = Factory.New<DtbBookingTmpl>();
			template.KT_Code = code;
			template.KT_Description = description;
			template.KT_Direction = direction;
			template.KT_RatingFreightMode = ratingFreightMode;

			return template;
		}

		public StmNote CreateNote(EnterpriseBusinessObject parent, string description)
		{
			var note = parent.Notes.AddNew();
			note.ST_Description = description;
			return note;
		}

		public Customs.ICusEntryNumber CreateAddtionalReference(ITransportAdditionalReferenceNumbers transportWithAddtionalReferences, string type, string number, DateTime issueDate, string entryLineReference = "", string countryCode = "")
		{
			var reference = transportWithAddtionalReferences.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = type;
			reference.CE_EntryNum = number;
			reference.CE_EntryLineReference = entryLineReference;
			reference.CE_IssueDate = issueDate;
			reference.CE_RN_NKCountryCode = countryCode;
			return reference;
		}

		public Freight.LocalCartage.Integration.ICommonCartage CreatePortTransport(DtbBooking booking)
		{
			var result = Factory.New<Freight.LocalCartage.Integration.ICommonCartage>();
			result.JJ_ParentID = booking.PK;
			result.JJ_ParentTableCode = booking.TablePrefix;

			return result;
		}

		public DtbBookingInstruction CreateInstruction(string instructionType = "")
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			instruction.KN_InstructionType = instructionType;

			return instruction;
		}

		// Easily confused with another method that receives a DtbBooking and an ordinary string
		public DtbBookingInstruction CreateInstruction(DtbBooking booking, ZString instructionType)
		{
			return CreateInstruction(booking, instructionType, "", null);
		}

		IDtbBookingInstruction ITransportBookingTestHelper.CreateInstruction(IDtbBooking booking, ZString instructionType) => CreateInstruction((DtbBooking)booking, instructionType);

		IDtbBookingInstruction ITransportBookingTestHelper.CreateInstruction(IDtbBooking booking, ZString instructionType, ZString orgType, OrgAddress address)
		{
			return CreateInstruction((DtbBooking)booking, instructionType, orgType, address);
		}

		IDtbBookingInstruction ITransportBookingTestHelper.CreateInstruction(IDtbBooking booking, ZString instructionType, ZString orgType, OrgAddress address, IPkgPackage package, ZDateTime estimated)
		{
			var instruction = CreateInstruction((DtbBooking)booking, instructionType, orgType, address);

			var packageDivot = instruction.PackageDivots.AddNew();
			var pkgPackage = (PkgPackage)package;
			packageDivot.KD_KP_Package = pkgPackage.PK;
			packageDivot.KD_Quantity = pkgPackage.KP_PackageQty;

			var comfirmation = instruction.Confirmations.AddNew();
			comfirmation.KK_ConfirmationType = instructionType;
			comfirmation.KK_Estimated = estimated;

			return instruction;
		}

		public DtbBookingInstructionPkgDivot CreatePackageDivot(DtbBookingInstruction instruction, PkgPackage package)
		{
			return CreatePackageDivot(instruction, package, package.KP_PackageQty);
		}

		public IEnumerable<DtbBookingInstructionPkgDivot> CreateAndAssignPackageDivots(DtbBooking booking, PkgPackage package, int qty = 1)
		{
			var result = new List<DtbBookingInstructionPkgDivot>();

			foreach (var instruction in booking.Instructions)
			{
				CreatePackageDivot(instruction, package);
			}

			return result;
		}

		public DtbBookingConfirmation CreateConfirmation(ZString confirmationTypeCode, ZDateTime actualDate)
		{
			var result = Factory.New<DtbBookingConfirmation>();
			result.KK_ConfirmationType = confirmationTypeCode;
			result.KK_Actual = actualDate;

			return result;
		}

		public DtbBookingConfirmation GetOrCreateConfirmation(DtbBookingInstruction instruction, ZString confirmationTypeCode)
		{
			var confirmation = confirmationTypeCode == ConfirmationTypes.Codes.PickUp ? instruction.FirstPickupConfirmation : instruction.LastDeliveryConfirmation;
			return confirmation ?? (CreateConfirmation(instruction, confirmationTypeCode));
		}

		public IForwardingShipment CreateForwardingShipment(ZString jobID, ZString houseBill, ZString transportMode, ZString packingMode)
		{
			var shipmentBO = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = jobID;
			shipmentBO[JobShipmentSchema.JS_HouseBill] = houseBill;
			shipmentBO[JobShipmentSchema.JS_TransportMode] = transportMode;
			shipmentBO[JobShipmentSchema.JS_PackingMode] = packingMode;
			shipmentBO[JobShipmentSchema.JS_IsForwardRegistered] = true;
			shipmentBO[JobShipmentSchema.JS_IsBooking] = false;
			shipmentBO[JobShipmentSchema.JS_IsCFSRegistered] = false;
			shipmentBO[JobShipmentSchema.JS_IsShipping] = false;

			return (IForwardingShipment)shipmentBO;
		}

		public void SetupForwardingShipmentAddresses(IForwardingShipment shipment, OrgHeader consignor, OrgHeader consignee, OrgHeader exportCFS, OrgHeader importCFS)
		{
			var shipmentBO = (BusinessObject)shipment;
			SetDocAddress(shipmentBO, "ConsignorDocumentaryAddress", consignor);
			SetDocAddress(shipmentBO, "ConsigneeDocumentaryAddress", consignee);
			SetAddress(shipmentBO, JobShipmentSchema.Constants.JS_OA_ExportReceivingDepot, exportCFS);
			SetAddress(shipmentBO, JobShipmentSchema.Constants.JS_OA_ImportReleaseDepot, importCFS);
		}

		public IForwardingConsol CreateForwardingConsol(IForwardingShipment shipment, ZString consolID, ZString transportMode, ZString masterBill)
		{
			var consolBO = Factory.New(ObjectFactory.GetType<IForwardingConsol>());
			consolBO[JobConsolSchema.JK_UniqueConsignRef] = consolID;
			consolBO[JobConsolSchema.JK_AgentType] = "AGT";
			consolBO[JobConsolSchema.JK_TransportMode] = transportMode;
			consolBO[JobConsolSchema.JK_MasterBillNum] = masterBill;

			if (shipment != null)
			{
				((IForwardingConsol)consolBO).AddShipment(shipment);
			}

			return (IForwardingConsol)consolBO;
		}

		public void SetupForwardingConsolAddresses(IForwardingConsol consol, OrgHeader exportCTO, OrgHeader exportCFS, OrgHeader exportCYD, OrgHeader importCTO, OrgHeader importCFS, OrgHeader importCYD)
		{
			var consolBO = (BusinessObject)consol;
			SetAddress(consolBO, JobConsolSchema.Constants.JK_OA_DepartureCTOAddress, exportCTO);
			SetAddress(consolBO, JobConsolSchema.Constants.JK_OA_PackDepotAddress, exportCFS);
			SetAddress(consolBO, JobConsolSchema.Constants.JK_OA_ContainerYardEmptyPickupAddress, exportCYD);
			SetAddress(consolBO, JobConsolSchema.Constants.JK_OA_ArrivalCTOAddress, importCTO);
			SetAddress(consolBO, JobConsolSchema.Constants.JK_OA_UnpackDepotAddress, importCFS);
			SetAddress(consolBO, JobConsolSchema.Constants.JK_OA_ContainerYardEmptyReturnAddress, importCYD);
		}

		public IForwardingContainer CreateForwardingContainer(IForwardingConsol consol, ZString containerNo, ZString containerType, string seal = "", string releaseNum = "")
		{
			var containerBO = Factory.New(ObjectFactory.GetType<IForwardingContainer>());
			containerBO[JobContainerSchema.JC_ContainerNum] = containerNo;
			containerBO[JobContainerSchema.JC_RC] = LoadRefContainer(containerType).PK;
			containerBO[JobContainerSchema.JC_JK] = consol.PK;
			containerBO[JobContainerSchema.JC_SealNum] = seal;
			containerBO[JobContainerSchema.JC_ReleaseNum] = releaseNum;

			return (IForwardingContainer)containerBO;
		}

		public Freight.Integration.Forwarding.IForwardingPackLine CreateForwardingPackline(IForwardingShipment shipment, IForwardingContainer container, ZInt packCount)
		{
			var packlineBO = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IForwardingPackLine>());
			packlineBO[JobPackLinesSchema.JL_FreightMode] = "OUT";
			packlineBO[JobPackLinesSchema.JL_PackageCount] = packCount;
			packlineBO[JobPackLinesSchema.JL_JS] = shipment.PK;

			var packlineContainerPivotBO = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobContainerPackPivot>());
			packlineContainerPivotBO[JobContainerPackPivotSchema.J6_JC] = ((BusinessObject)container).PK;
			packlineContainerPivotBO[JobContainerPackPivotSchema.J6_JL] = packlineBO.PK;

			return (Freight.Integration.Forwarding.IForwardingPackLine)packlineBO;
		}

		public Freight.Integration.Forwarding.IForwardingPackLine CreateForwardingPackline(IForwardingShipment shipment, string packType, ZInt packCount)
		{
			var packlineBO = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IForwardingPackLine>());
			packlineBO[JobPackLinesSchema.JL_FreightMode] = "OUT";
			packlineBO[JobPackLinesSchema.JL_PackageCount] = packCount;
			packlineBO[JobPackLinesSchema.JL_JS] = shipment.PK;
			packlineBO[JobPackLinesSchema.JL_F3_NKPackType] = packType;

			return (Freight.Integration.Forwarding.IForwardingPackLine)packlineBO;
		}

		public BusinessObject CreateFCLBillOfLadingContainer(BusinessObject parentShipment, string containerNumber, string sealNumber)
		{
			var container = (BusinessObject)Factory.New<IBillOfLadingContainer>();
			PopulateFCLContainer(container, parentShipment, containerNumber, sealNumber);
			return container;
		}

		public BusinessObject CreateFCLAgencyShipmentContainer(BusinessObject parentShipment, string containerNumber, string sealNumber)
		{
			var container = (BusinessObject)Factory.New<IAgencyShipmentContainer>();
			PopulateFCLContainer(container, parentShipment, containerNumber, sealNumber);
			return container;
		}

		void PopulateFCLContainer(BusinessObject container, BusinessObject parentShipment, string containerNumber, string sealNumber)
		{
			container[JobContainerSchema.JC_ContainerNum] = containerNumber;
			container[JobContainerSchema.JC_RC] = LoadRefContainer("20GP").PK;
			container[JobContainerSchema.JC_SealNum] = sealNumber;
			container[JobContainerSchema.JC_ContainerMode] = "FCL";
			container[JobContainerSchema.JC_GrossWeight] = 2280;
			container[JobContainerSchema.JC_GrossWeightUQ] = "KG";
			container[JobContainerSchema.JC_IsSealOk] = true;
			container[JobContainerSchema.JC_TotalHeight] = 8.5;
			container[JobContainerSchema.JC_TotalLength] = 20.0;
			container[JobContainerSchema.JC_TotalWidth] = 8.0;
			container[JobContainerSchema.JC_TotalUnitOfMeasure] = "FT";
			((BusinessObjectCollection)parentShipment["FCLContainers"]).Add(container);
		}

		void SetAddress(BusinessObject bo, ZString oaSchemaColumn, OrgHeader organisation)
		{
			if (organisation != null)
			{
				bo[oaSchemaColumn] = organisation.MainAddress.PK;
			}
		}

		void SetDocAddress(BusinessObject bo, ZString docAddressPropertyName, OrgHeader organisation)
		{
			if (organisation != null)
			{
				((JobDocAddress)bo[docAddressPropertyName]).E2_OA_Address = organisation.MainAddress.PK;
			}
		}

		public OrgHeader CreateOrLoadOrganisation(ZString code)
		{
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code)) ?? CreateOrganisation(code);

			return organisation;
		}

		public PkgPackageJob CreatePackageJob(DtbBookingConsolidation transportBooking)
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = transportBooking.PK;
			packageJob.KJ_ParentTableCode = transportBooking.TablePrefix;

			return packageJob;
		}

		public RefContainer LoadRefContainer(ZString containerType)
		{
			return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
		}

		public JobCharge CreateCharge(JobHeader jobHeader, AccChargeCode accChargeCode, ZGuid consolCostPK, ZDecimal osCostAmt, ZDecimal localCostAmt, GlbBranch currentBranch = null)
		{
			var charge = Factory.New<JobCharge>();
			charge.JR_GC = jobHeader.JH_GC;
			charge.JR_AC = accChargeCode.PK;
			charge.JR_JH = jobHeader.PK;
			charge.JR_E6 = consolCostPK;
			charge.JR_OSCostAmt = osCostAmt;
			charge.JR_LocalCostAmt = localCostAmt;
			charge.JR_GB = currentBranch != null ? currentBranch.PK : GlbBranch.CurrentBranch.PK;
			charge.JR_GC = currentBranch != null ? currentBranch.Company.PK : GlbCompany.CurrentCompany.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			return charge;
		}

		public IJobConsolCost CreateConsolCost(DtbBookingConsolidation multiJobConsolidation, AccChargeCode accChargeCode, ZDecimal osCostAmt, ZDecimal localCostAmt, GlbCompany currentCompany = null, string costRef = "")
		{
			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = multiJobConsolidation.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = DtbBookingConsolidationSchema.Constants.Prefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = accChargeCode.PK;
			consolCost[JobConsolCostSchema.E6_GC] = currentCompany != null ? currentCompany.PK : GlbCompany.CurrentCompany.PK;
			consolCost[JobConsolCostSchema.E6_OSCostAmount] = osCostAmt;
			consolCost[JobConsolCostSchema.E6_LocalCostAmount] = localCostAmt;

			if (!string.IsNullOrEmpty(costRef))
			{
				consolCost[JobConsolCostSchema.E6_CostReference] = costRef;
			}

			return (IJobConsolCost)consolCost;
		}

		public IJobConsolCost CreateConsolCost(DtbBooking masterBooking, AccChargeCode accChargeCode, ZDecimal osCostAmt, ZDecimal localCostAmt, GlbCompany currentCompany = null, string costRef = "")
		{
			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = masterBooking.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = DtbBookingSchema.Constants.Prefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = accChargeCode.PK;
			consolCost[JobConsolCostSchema.E6_GC] = currentCompany != null ? currentCompany.PK : GlbCompany.CurrentCompany.PK;
			consolCost[JobConsolCostSchema.E6_OSCostAmount] = osCostAmt;
			consolCost[JobConsolCostSchema.E6_LocalCostAmount] = localCostAmt;

			if (!string.IsNullOrEmpty(costRef))
			{
				consolCost[JobConsolCostSchema.E6_CostReference] = costRef;
			}

			return (IJobConsolCost)consolCost;
		}

		public IDisposable SetRegistryItem_PreventMilestoneFutureActualStart(bool value)
		{
			return WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public OrgHeader MapOrganisation(string mappedCode)
		{
			var branchOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbCompany.CurrentCompany.OrgProxy.PK));
			var patternOverride = branchOrganisation.CreatePatternMatchOverrideForTest();
			patternOverride.OO_ForeignCode = "EDIDATEDI";
			patternOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			var mappedOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, mappedCode));
			patternOverride.OO_LocalGuid = mappedOrganisation.PK;

			return mappedOrganisation;
		}

		IDtbBookingConfirmation ITransportBookingTestHelper.CreateConfirmation(IDtbBookingInstruction instruction, ZString confirmationTypeCode) => CreateConfirmation((DtbBookingInstruction)instruction, confirmationTypeCode);

		public DtbBookingConfirmation CreateConfirmation(DtbBookingInstruction instruction, ZString confirmationTypeCode)
		{
			var result = instruction.Confirmations.AddNew();
			result.KK_ConfirmationType = confirmationTypeCode;
			return result;
		}

		public DtbBookingConfirmation CreateConfirmation(DtbBookingInstructionPkgDivot packageDivot, ZString confirmationTypeCode)
		{
			return CreateConfirmation(packageDivot, confirmationTypeCode, packageDivot.KD_Quantity);
		}

		public DtbBookingConfirmation CreateConfirmation(DtbBookingInstructionPkgDivot packageDivot, ZString confirmationTypeCode, ZInt quantity)
		{
			var result = packageDivot.Confirmations.AddNew();
			result.KK_ConfirmationType = confirmationTypeCode;
			result.KK_Quantity = quantity;
			return result;
		}

		public RefServiceLevel CreateServiceLevel(ZString code)
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = code;
			return serviceLevel;
		}

		public RefTransitTime CreateTransitTime(ZString transitTimeserviceLevelCode, ZInt transitHours, ZGuid originDomesticZonePK, ZGuid destinationDomesticZonePK)
		{
			var transitTime = Factory.New<RefTransitTime>();
			transitTime.RTT_RS_NKServiceLevel = transitTimeserviceLevelCode;
			transitTime.RTT_TransitHours = transitHours;
			transitTime.RTT_TZ_OriginDomesticZone = originDomesticZonePK;
			transitTime.RTT_TZ_DestinationDomesticZone = destinationDomesticZonePK;
			return transitTime;
		}

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

		public DtbBookingInstruction CreateInstruction(DtbBooking booking, ZString instructionType, ZString orgType, OrgAddress address)
		{
			var result = CreateInstructionCore(booking, instructionType);

			result.OrganisationType = orgType;
			if (address != null)
			{
				result.Address.E2_AddressOverride = false;
				result.Address.E2_OA_Address = address.PK;
			}

			return result;
		}

		// Easily confused with another method that receives a DtbBooking and a ZString
		public DtbBookingInstruction CreateInstruction(DtbBooking booking, string instructionType = InstructionTypes.Codes.PickUp)
		{
			return CreateInstructionCore(booking, instructionType);
		}

		// To prevent infinite loops
		DtbBookingInstruction CreateInstructionCore(DtbBooking booking, string instructionType)
		{
			var instruction = booking.Instructions.AddNew();
			instruction.KN_InstructionType = instructionType;
			return instruction;
		}

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

		OrgHeader ITransportBookingTestHelper.CreateOrganisation(ZString code)
		{
			return CreateOrganisation(code);
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

		public PkgPackage CreatePackage(ZString packageID, DtbBookingInstructionPkgDivot packageDivot = null, int quantity = 1, decimal weight = 0, decimal volume = 0)
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

		public PkgPackage CreatePackageContainer(ZString packageID, DtbBookingInstructionPkgDivot packageDivot = null, int quantity = 1, decimal weight = 0, decimal volume = 0, string containerType = "20GP")
		{
			var result = CreatePackage(packageID, packageDivot, quantity, weight, volume);
			result.KP_F3_NKPackType = Constants.PkgUnit.Container;
			result.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;

			// assigning specific container type is overriding Weight/Volume
			result.KP_Weight = weight;
			result.KP_Volume = volume;

			return result;
		}

		public DtbBookingInstructionPkgDivot CreatePackageDivot(DtbBookingInstruction instruction, int qty = 1)
		{
			return CreatePackageDivot(instruction, null, qty);
		}

		public DtbBookingInstructionPkgDivot CreatePackageDivot(DtbBookingInstruction instruction, PkgPackage package, int qty)
		{
			var result = instruction.PackageDivots.AddNew();
			if (package != null)
			{
				result.KD_KP_Package = package.PK;
			}
			result.KD_Quantity = qty;
			return result;
		}

		public DtbBookingInstructionTmpl AddInstructionToTemplate(DtbBookingTmpl template, ZString orgType, ZString instructionType, bool isContainerRateable = false, bool isLooseRateable = false, string packageCategory = "")
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

		public RefCityTown CreateCityTown(string internationalName, string countryCode = "", string postcode = "", string stateCode = "")
		{
			var cityTown = Factory.NewWithValidTestData<RefCityTown>();
			cityTown.R9_InternationalName = internationalName;
			cityTown.R9_RN_NKCountry = countryCode;
			if (!string.IsNullOrEmpty(postcode))
			{
				cityTown.PostCodes.AddNew().RK_CityTownPostCode = postcode;
			}

			if (!string.IsNullOrWhiteSpace(stateCode))
			{
				cityTown.R9_RW_NKState = stateCode;
			}

			return cityTown;
		}

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

		public GlbCompany CreateCompany(string code, string name, OrgHeader org)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = code;
			company.GC_Name = name;
			company.GC_OH_OrgProxy = org.PK;
			return company;
		}

		public GlbBranch CreateBranch(string code, string name, GlbCompany company, OrgHeader org)
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = code;
			branch.GB_BranchName = name;
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = org.PK;
			return branch;
		}

		public JobHeader LoadOrCreateJobHeader(IJobHeaderParent parent)
		{
			var jobHeader = new JobHeader.Loader(parent).TryLoadOrCreate();

			return jobHeader;
		}

		public IForwardingShipment CreateForwardingShipment()
		{
			var shipment = Factory.New<IForwardingShipment>();

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";

			return shipment;
		}

		public IDtbConsignment CreateConsignment(ZGuid bookingPk)
		{
			var consignment = Factory.New<IDtbConsignment>();

			consignment.LTC_KM_Booking = bookingPk;
			consignment.LTC_Direction = "LOC";
			consignment.LTC_Status = "BKD";

			return consignment;
		}

		public void AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(bool isPreConditionAssert)
		{
			var preConditionAssertPreamble = isPreConditionAssert ? "Pre-condition: " : string.Empty;
			var templateQuery = new ZQuery(DtbBookingTmplSchema.KT_Code, "EFPL");
			var template = Factory.LoadTop1<DtbBookingTmpl>(templateQuery);

			AssertionWithHtml.CombineAssertions(FormattableString.Invariant($"{preConditionAssertPreamble}EFPL Template has correct number of instructions and the instructions have correct values in K2_PackageType"), () =>
			{
				Assertion.AssertEquals(FormattableString.Invariant($"{preConditionAssertPreamble}Template has four instructions"), 4, template.Instructions.Count);
				foreach (var instructionTemplate in template.Instructions)
				{
					var expectedInstructionType = string.Empty;
					var expectedPackageCategory = string.Empty;

					switch (instructionTemplate.K2_Sequence)
					{
						case 1:
							expectedInstructionType = InstructionTypes.Codes.PickUp;
							expectedPackageCategory = PackageCategories.Codes.Loose;
							break;

						case 2:
							expectedInstructionType = InstructionTypes.Codes.PickUp;
							expectedPackageCategory = PackageCategories.Codes.Containers;
							break;

						case 3:
							expectedInstructionType = InstructionTypes.Codes.Multi;
							expectedPackageCategory = PackageCategories.Codes.Both;
							break;

						case 4:
							expectedInstructionType = InstructionTypes.Codes.Delivery;
							expectedPackageCategory = PackageCategories.Codes.Containers;
							break;

						default:
							Assertion.Assert(FormattableString.Invariant($"{preConditionAssertPreamble}Instruction template on template EFPL with unexpected sequence {instructionTemplate.K2_Sequence}, instruction type '{instructionTemplate.K2_InstructionType}', package type '{instructionTemplate.K2_PackageType}'"), false);
							continue;
					}

					Assertion.AssertEquals(FormattableString.Invariant($"{preConditionAssertPreamble}Instruction template on template EFPL with sequence {instructionTemplate.K2_Sequence} should have K2_InstructionType '{expectedInstructionType}'"), expectedInstructionType, instructionTemplate.K2_InstructionType);
					Assertion.AssertEquals(FormattableString.Invariant($"{preConditionAssertPreamble}Instruction template on template EFPL with sequence {instructionTemplate.K2_Sequence} should have K2_PackageType '{expectedPackageCategory}'"), expectedPackageCategory, instructionTemplate.K2_PackageType);
				}
			});
		}

		public void AssignBookingToAnAuthorisedCarrierBookingAgent(DtbBooking booking)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var hubMode = org.EDICommunicationsModes.AddNew();
			hubMode.EK_Module = RelatableActivityTypeList.Codes.TransportBooking;
			hubMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			hubMode.EK_Destination = "BLUME_EAD";
			var address = org.MainAddress;
			booking.CarrierBookingAgentDocAddress.E2_OA_Address = address.PK;
		}

		public void AddBookingConfirmedEventToBooking(DtbBooking booking, ZString eI_From)
		{
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_From = eI_From;
			var message = Factory.New<XmlEDIMessage>();
			interchange.AddMessage(message);

			var eventBKC = booking.Logs.AddNew(Events.BookingConfirmed);

			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;
			genPivot.XX_Relation1ID = eventBKC.PK;
			genPivot.XX_Relation1TableCode = eventBKC.TablePrefix;
			genPivot.XX_Relation2ID = message.PK;
			genPivot.XX_Relation2TableCode = message.TablePrefix;
		}
	}
}
