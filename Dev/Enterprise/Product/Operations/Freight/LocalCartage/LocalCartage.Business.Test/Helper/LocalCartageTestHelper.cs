using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using AccChargeCodeDO = CargoWise.Database.TestFramework.ObjectModel.AccChargeCode;
using AccTransactionLinesDO = CargoWise.Database.TestFramework.ObjectModel.AccTransactionLines;
using JobBookedCtgMoveDO = CargoWise.Database.TestFramework.ObjectModel.JobBookedCtgMove;
using JobCartageDO = CargoWise.Database.TestFramework.ObjectModel.JobCartage;
using JobChargeAttribDO = CargoWise.Database.TestFramework.ObjectModel.JobChargeAttrib;
using JobChargeDO = CargoWise.Database.TestFramework.ObjectModel.JobCharge;
using JobContainerLegsDO = CargoWise.Database.TestFramework.ObjectModel.JobContainerLegs;
using JobHeaderDO = CargoWise.Database.TestFramework.ObjectModel.JobHeader;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class LocalCartageTestHelper : CartageTestHelper
	{
		public LocalCartageTestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static ZString HomePort
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					return GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}

				return "AUSYD";
			}
		}

		public static ZString USPort
		{
			get
			{
				return "USLAX";
			}
		}

		public static ZString USPortAlt
		{
			get
			{
				return "USSFO";
			}
		}

		public static ZString AlternateHomePort
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					ZString uNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					ZString countryCode = uNLOCO.Substring(0, 2);
					return GetPortInCountryExcluding(countryCode, uNLOCO);
				}

				return "AUPER";
			}
		}

		public static ZString AlternateHomePort2
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					ZString uNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					ZString countryCode = uNLOCO.Substring(0, 2);
					return GetPortInCountryExcluding(countryCode, uNLOCO, AlternateHomePort);
				}

				return "AUPER";
			}
		}

		public static ZString OverseasPort
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "SGSIN")
				{
					return "SGSIN";
				}
				else
				{
					return "USLAX";
				}
			}
		}

		public static ZString OverseasPort2
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "HKHKG")
				{
					return "HKHKG";
				}
				else
				{
					return "THBKK";
				}
			}
		}

		public static ZString OverseasPort3
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "DEHAM")
				{
					return "DEHAM";
				}
				else
				{
					return "CNSHA";
				}
			}
		}

		public static ZString OverseasPort4
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "TTTEM")
				{
					return "TTTEM";
				}
				else
				{
					return "JPTYO";
				}
			}
		}

		public static ZString GetPortInCountryExcluding(string countryCode, params string[] uNLOCOs)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode);
			foreach (string uNLOCO in uNLOCOs)
			{
				filter.AddToFilter(JoinCondition.And, RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, uNLOCO);
			}

			var resultPort = factory.LoadTop1<RefUNLOCO>(filter);
			return resultPort.RL_Code;
		}

		const string TestVessel1Name = "APL EMERALD";
		public RefVessel TestVessel1
		{
			get
			{
				if (fTestVessel1 == null)
				{
					fTestVessel1 = LoadOrCreateVessel(TestVessel1Name, "8610033");
				}

				return fTestVessel1;
			}
		}

		RefVessel fTestVessel1;

		const string TestVessel2Name = "The Black Pearl";
		public RefVessel TestVessel2
		{
			get
			{
				if (fTestVessel2 == null)
				{
					fTestVessel2 = LoadOrCreateVessel(TestVessel2Name, "8610033");
				}

				return fTestVessel2;
			}
		}

		RefVessel fTestVessel2;

		RefVessel LoadOrCreateVessel(string name, string lloydsNumber)
		{
			var vessel = RefVessel.LookupVesselByName(name, Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_LloydsNumber = lloydsNumber;
				vessel.RV_Name = name;
			}

			return vessel;
		}

		public JobSailing CreateSailing(RefVessel vessel, ZString voyage, ZString load, ZString discharge, ZDateTime eTD, string transportMode = Core.Constants.TransportModes.Sea)
		{
			JobVoyage resultVoyage = Factory.New<JobVoyage>();
			resultVoyage.JV_AirSeaRoad = transportMode;
			resultVoyage.JV_RV_NKVessel = vessel.RV_FK;
			resultVoyage.JV_VoyageFlight = voyage;
			VoyageOrigin origin = resultVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = eTD;
			VoyageDestination destination = resultVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			resultVoyage.GenerateSailings();
			return resultVoyage.Sailings[0];
		}

		public JobSailing CreateSailingWithEstimatedArrival(RefVessel vessel, ZString voyage, ZString load, ZString discharge, ZDateTime eTD, ZDateTime eTA, string transportMode = Core.Constants.TransportModes.Sea)
		{
			JobVoyage resultVoyage = Factory.New<JobVoyage>();
			resultVoyage.JV_AirSeaRoad = transportMode;
			resultVoyage.JV_RV_NKVessel = vessel.RV_FK;
			resultVoyage.JV_VoyageFlight = voyage;
			VoyageOrigin origin = resultVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = eTD;
			VoyageDestination destination = resultVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = eTA;
			resultVoyage.GenerateSailings();
			return resultVoyage.Sailings[0];
		}

		public JobDocAddress CreateOrUpdateJobDocAddressOnCartage(CommonCartage cartage, DocAddressType addressType, ZString addressTypeCode, ZString orgName, ZString address1, ZString postCode, ZString city, ZString homePort, bool overrideAddress)
		{
			var docAddress = (JobDocAddress)cartage.DocAddresses.FirstOrDefault(a => (ZString)a[JobDocAddressSchema.E2_AddressType] == addressTypeCode);
			if (docAddress == null)
			{
				docAddress = CreateJobDocAddress(cartage, addressType, orgName, address1, postCode, city, homePort, overrideAddress);
			}
			else
			{
				UpdateDocAddress(ref docAddress, orgName, address1, postCode, city, homePort, overrideAddress);
			}

			return docAddress;
		}

		public JobDocAddress CreateJobDocAddress(IDocAddresses parent, DocAddressType addressType, ZString orgName, ZString address1, ZString postCode, ZString city, ZString homePort, bool overrideAddress)
		{
			JobDocAddress docAddress = parent.DocAddresses.CreateWithAddressType(addressType);
			UpdateDocAddress(ref docAddress, orgName, address1, postCode, city, homePort, overrideAddress);

			return docAddress;
		}

		void UpdateDocAddress(ref JobDocAddress docAddress, ZString orgName, ZString address1, ZString postCode, ZString city, ZString homePort, bool overrideAddress)
		{
			if (overrideAddress)
			{
				docAddress.E2_AddressOverride = true;
				docAddress.E2_CompanyName = orgName;
				docAddress.E2_Address1 = address1;
				docAddress.E2_Postcode = postCode;
				docAddress.E2_City = city;
			}
			else
			{
				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_FullName = orgName;
				org.OH_RL_NKClosestPort = homePort;
				org.MainAddress.OA_Address1 = address1;
				org.MainAddress.OA_PostCode = postCode;
				org.MainAddress.OA_City = city;
				docAddress.E2_OA_Address = org.MainAddress.PK;
			}
		}

		public OrgHeader CreateOrgHeader(ZString orgCode, ZString mainAddressCode)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_Code = orgCode;
			var mainAddress = result.MainAddress;
			mainAddress.OA_Address1 = mainAddressCode;
			mainAddress.OA_City = "Sydney";
			return result;
		}

		public OrgAddress AddOrgAddress(OrgHeader organisation, ZString addressCode)
		{
			var result = organisation.Addresses.AddNew();
			result.OA_Code = addressCode;
			return result;
		}

		public CommonCartage CreateCartage(string jobType, int bookedMoves)
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = jobType;
			if (cartage.IsContainerised)
			{
				cartage.ContainerBookedMoves.DeleteAll();
				for (int i = 0; i < bookedMoves; i++)
				{
					var move = cartage.ContainerBookedMoves.AddNew();
					var container = move.Container;
					if (move.CartageLegs.Count == 0)
					{
						move.CartageLegs.AddNew();
					}
				}
			}

			if (cartage.IsLoose)
			{
				cartage.LooseBookedMoves.DeleteAll();
				for (int i = 0; i < bookedMoves; i++)
				{
					CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
					if (move.CartageLegs.Count == 0)
					{
						move.CartageLegs.AddNew();
					}
				}
			}

			return cartage;
		}

		public CommonCartage CreateInternalCartageWithNoJobHeader(CommonShipment shipment)
		{
			var cartage = shipment.Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = shipment.TablePrefix;
			cartage.JJ_ConsignmentID = shipment.JS_UniqueConsignRef + "/I";
			return cartage;
		}

		public CommonCartage CreateInternalCartage(CommonShipment shipment)
		{
			var cartage = CreateInternalCartageWithNoJobHeader(shipment);
			new JobHeader.Loader(cartage).TryLoadOrCreate().JH_GE = GlbDepartment.CurrentDepartment.PK;
			return cartage;
		}

		public void CreateAndAssignAddresses(CommonCartageLeg leg1, CommonCartageLeg leg2, DocAddressType addressType1, DocAddressType addressType2, DocAddressType addressType3)
		{
			var docAddress1 = CreateJobDocAddress(leg1.Cartage, addressType1, "org4", "add4", "2000", "SYDNEY", "AUSYD", true);
			var docAddress2 = CreateJobDocAddress(leg1.Cartage, addressType2, "org5", "add5", "2000", "SYDNEY", "AUSYD", true);
			var docAddress3 = CreateJobDocAddress(leg1.Cartage, addressType3, "org6", "add6", "2000", "SYDNEY", "AUSYD", true);
			leg1.JU_E2PickupAddressID = docAddress1.PK;
			leg1.JU_E2DeliveryAddressID = docAddress2.PK;
			leg2.JU_E2PickupAddressID = docAddress2.PK;
			leg2.JU_E2DeliveryAddressID = docAddress3.PK;
		}

		public void AddCartageLegsToBookedMovement(CommonBookedCtgMove move, params DocAddressType[] route)
		{
			var cartage = move.Cartage;
			CommonCartageLeg previousLeg = null;
			for (int j = 1; j < route.Length; j++)
			{
				var leg = move.CartageLegs.AddNew();
				if (previousLeg == null)
				{
					var pickUpAddress = cartage.DocAddresses.AddNew(Factory.NewWithValidTestData<OrgAddress>(), route[j - 1]);
					pickUpAddress.E2_AddressOverride = true;
					pickUpAddress.E2_City = "Sydney";
					leg.JU_E2PickupAddressID = pickUpAddress.PK;
				}
				else
				{
					leg.JU_E2PickupAddressID = previousLeg.JU_E2DeliveryAddressID;
				}

				var deliveryAddress = cartage.DocAddresses.AddNew(Factory.NewWithValidTestData<OrgAddress>(), route[j]);
				deliveryAddress.E2_AddressOverride = true;
				deliveryAddress.E2_City = "Melbourne";
				leg.JU_E2DeliveryAddressID = deliveryAddress.PK;
				previousLeg = leg;
			}
		}

		public void AddCartageLegWithWaitPoint(CommonBookedCtgMove move, DocAddressType pickup, DocAddressType waitpoint, DocAddressType deliver)
		{
			AddCartageLegsToBookedMovement(move, pickup, deliver);
			var cartage = move.Cartage;
			var waitPointAddress = cartage.DocAddresses.AddNew(Factory.NewWithValidTestData<OrgAddress>(), waitpoint);
			waitPointAddress.E2_AddressOverride = true;
			waitPointAddress.E2_City = "Canberra";
			move.LastCartageLeg.JU_E2WaitPointAddressID = waitPointAddress.PK;
		}

		public CommonCartage CreateCartageWithoutTemplate(int numContainers, int numLooseMoves, params DocAddressType[] route)
		{
			if (route.Length < 2)
			{
				throw new ArgumentException("Please provide at least 2 DocAddressTypes to build cartage legs");
			}

			var cartage = Factory.New<CommonCartage>();
			for (int i = 0; i < numContainers; i++)
			{
				var containerMove = cartage.ContainerBookedMoves.AddNew();
				containerMove.Container.JC_ContainerNum = "C000" + i;
				containerMove.CartageLegs.DeleteAll();
				AddCartageLegsToBookedMovement(containerMove, route);
			}

			for (int i = 0; i < numLooseMoves; i++)
			{
				var looseMove = cartage.LooseBookedMoves.AddNew();
				AddCartageLegsToBookedMovement(looseMove, route);
			}

			return cartage;
		}

		public CusEntryNumber AddAdditionalNumberRefenceToCartage(CommonCartage cartage, string entryType, ZString entryNumber)
		{
			var refNumber = cartage.AdditionalReferenceNumbers.AddNew();
			refNumber.CE_EntryType = entryType;
			refNumber.CE_EntryNum = entryNumber;
			return refNumber;
		}

		public static (CommonCartage testCartage, CartageType testCartageType, ICartageParent testCartageParent, OrgHeader testOrgProxy) CreateTestCartageWithParentForwardingShipment(BusinessObjectFactory factory, string roadName = "Address Road", string shipmentUniqueConsignRef = "ABC0123", bool createCartage = true, string overrideLocalTransportProviderOrgName = null)
		{
			var otherFactory = new BusinessObjectFactory();
			var orgProxy = otherFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbCompany.CurrentCompany.GC_OH_OrgProxy));
			orgProxy.OH_IsLocalTransport = true;
			var orgAddressProxy = otherFactory.New<OrgAddress>();
			orgAddressProxy.OA_OH = orgProxy.PK;
			orgAddressProxy.OA_Address1 = "1 " + roadName;
			orgAddressProxy.OA_AuthorityToLeave = "DEF";
			orgAddressProxy.OA_ValidationStatus = "UNV";
			var orgAddressConsignorPickup = otherFactory.New<OrgAddress>();
			orgAddressConsignorPickup.OA_OH = orgProxy.PK;
			orgAddressConsignorPickup.OA_Address1 = "2 " + roadName;
			orgAddressConsignorPickup.OA_AuthorityToLeave = "DEF";
			orgAddressConsignorPickup.OA_ValidationStatus = "UNV";
			var orgAddressConsignorPickupCapability = otherFactory.New<OrgAddressCapability>();
			orgAddressConsignorPickupCapability.PZ_OA = orgAddressConsignorPickup.PK;
			orgAddressConsignorPickupCapability.PZ_AddressType = "PIC";
			var orgAddressPickupDepotAddress = otherFactory.New<OrgAddress>();
			orgAddressPickupDepotAddress.OA_OH = orgProxy.PK;
			orgAddressPickupDepotAddress.OA_Address1 = "3 " + roadName;
			orgAddressPickupDepotAddress.OA_AuthorityToLeave = "DEF";
			orgAddressPickupDepotAddress.OA_ValidationStatus = "UNV";
			var forwardingShipmentPreload = otherFactory.New<CommonShipment>();
			forwardingShipmentPreload.JS_UniqueConsignRef = shipmentUniqueConsignRef;
			forwardingShipmentPreload.JS_IsForwardRegistered = true;
			forwardingShipmentPreload.JS_OA_ExportReceivingDepot = orgAddressPickupDepotAddress.PK;
			var jobDocsAndCartage = JobDocsAndCartage.New(forwardingShipmentPreload);
			jobDocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddressProxy.PK;
			otherFactory.Save();
			var forwardingShipment = factory.Load<CommonShipment>(forwardingShipmentPreload.PK);
			var forwardingShipmentCartageParent = (ICartageParent)forwardingShipment;
			var cartageType = forwardingShipmentCartageParent.CartageTypes.First();
			if (overrideLocalTransportProviderOrgName != null)
			{
				cartageType.LocalTransportProviderAddress.Header.OH_FullName = overrideLocalTransportProviderOrgName;
			}

			// CNR address JobDocAddress record to forwardingShipment (ConsignorPickupDocAddress - DocAddressType.ConsignorPickupDeliveryAddress == "CRG", AddressType (OA capability PZ_AddressType) == "PIC", ContactType = LocalTransport = "TRN") - required for Cartage of type LCL EXPORT
			var consignorPickupDocAddressLink = factory.New<JobDocAddress>();
			consignorPickupDocAddressLink.E2_ParentID = forwardingShipment.PK;
			consignorPickupDocAddressLink.E2_ParentTableCode = "JS";
			consignorPickupDocAddressLink.E2_AddressType = "CRG";
			consignorPickupDocAddressLink.E2_OA_Address = orgAddressConsignorPickup.PK;
			// CFS address JobDocAddress record to forwardingShipment (DepartureCFSDocAddress - parent forwardingShipment, DocAddressType.DepartureCFSAddress == "DCF", linked to PickupDepotAddress == JS_OA_ExportReceivingDepot) - required for Cartage of type LCL EXPORT
			var cfsAddressLink = factory.New<JobDocAddress>();
			cfsAddressLink.E2_ParentID = forwardingShipment.PK;
			cfsAddressLink.E2_ParentTableCode = "JS";
			cfsAddressLink.E2_AddressType = "DCF";
			cfsAddressLink.E2_OA_Address = orgAddressPickupDepotAddress.PK;
			CommonCartage cartage = null;
			if (createCartage)
			{
				cartage = factory.New<CommonCartage>();
				cartage.JJ_ParentID = forwardingShipment.PK;
				cartage.JJ_ParentTableCode = "JS";
				cartage.JJ_Direction = cartageType.GetMatchingDirectionCodes().First();
			}

			factory.Save();
			return (testCartage: cartage, testCartageType: cartageType, testCartageParent: forwardingShipmentCartageParent, testOrgProxy: orgProxy);
		}

		public CommonBookedCtgMove SetupBookedMove(CommonBookedCtgMove move, ZInt packs, ZString packType, ZDecimal weight, ZString weightUnit, ZDecimal volume, ZString volumeUnit)
		{
			move.EW_BookedPackCount = packs;
			move.EW_F3_NKPackType = packType;
			move.EW_BookedWeight = weight;
			move.EW_WeightUQ = weightUnit;
			move.EW_BookedVolume = volume;
			move.EW_VolumeUQ = volumeUnit;
			return move;
		}

		public CommonBookedCtgMove SetupBookedMove(CommonBookedCtgMove move, JobDocAddress firstAddress, JobDocAddress secondAddress, ZDateTime setReqTimes)
		{
			move.EW_E2PickupAddressID = firstAddress.PK;
			move.EW_E2WaitPointAddressID = secondAddress.PK;
			move.EW_RequestedPickupTimeStart = setReqTimes.AddMinutes(1);
			move.EW_RequestedPickupTimeEnd = setReqTimes.AddMinutes(2);
			move.EW_RequestedDeliveryTimeStart = setReqTimes.AddMinutes(3);
			move.EW_RequestedDeliveryTimeEnd = setReqTimes.AddMinutes(4);
			return move;
		}

		public CommonCartageLeg SetupLeg(CommonCartageLeg leg, JobDocAddress pickup, JobDocAddress waitpoint, JobDocAddress delivery, ZDateTime setInOutTimes)
		{
			leg.JU_E2PickupAddressID = pickup != null ? pickup.PK : ZGuid.Empty;
			leg.JU_E2WaitPointAddressID = waitpoint != null ? waitpoint.PK : ZGuid.Empty;
			leg.JU_E2DeliveryAddressID = delivery != null ? delivery.PK : ZGuid.Empty;
			leg.JU_PlannedPickupTime = setInOutTimes.AddMinutes(-10);
			leg.JU_PlannedPickupTimeEnd = setInOutTimes.AddMinutes(-9);
			leg.JU_EstimatedDeliveryTime = setInOutTimes.AddMinutes(-8);
			leg.JU_EstimatedDeliveryTimeEnd = setInOutTimes.AddMinutes(-7);
			leg.JU_PickupTimeIn = setInOutTimes.AddMinutes(1);
			leg.JU_PickupTimeOut = setInOutTimes.AddMinutes(2);
			leg.JU_WaitPointTimeIn = leg.JU_E2WaitPointAddressID.IsValid ? setInOutTimes.AddMinutes(3) : ZDateTime.Empty;
			leg.JU_WaitPointTimeOut = leg.JU_E2WaitPointAddressID.IsValid ? setInOutTimes.AddMinutes(4) : ZDateTime.Empty;
			leg.JU_DeliverTimeIn = setInOutTimes.AddMinutes(5);
			leg.JU_DeliverTimeOut = setInOutTimes.AddMinutes(6);
			return leg;
		}

		public ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			result.P0_IsActive = true;
			return result;
		}

		public GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		public ProcessTask CreateProcessTask(ProcessTaskTemplate template, ZString descriprion, ZString processTaskType, ZInt sequence, ZString milestoneEvent)
		{
			var processTask = template.WorkflowItems.AddNew();
			processTask.P9_Description = descriprion;
			processTask.P9_Type = processTaskType;
			processTask.P9_Sequence = sequence;
			processTask.TriggerConditions.TriggerEventCode = milestoneEvent;
			processTask.P9_SE_NKExceptionEvent = "EXC";
			return processTask;
		}

		public RefEquipment CreateVehicleWithEquipmentType(ZString vehicleCode, string registrationCode)
		{
			var vehicle = Factory.New<RefEquipment>();
			vehicle.RQ_ShortCode = vehicleCode;
			vehicle.RQ_Registration = registrationCode;
			vehicle.RQ_IsVehicle = true;
			var equipmentType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "RTRK").PK;
			vehicle.RQ_RC_RoadContainerType = equipmentType;
			return vehicle;
		}

		public GlbStaff CreateDriver(ZString driverName, ZString loginName)
		{
			var driver = Factory.New<GlbStaff>();
			driver.GS_FullName = driverName;
			driver.GS_LoginName = loginName;
			driver.GS_Code = driverName.SubstringSafe(0, 3);
			return driver;
		}

		public DbCommand CreateCommand_LocalCartageLegsByVehicleOrTransportCompany(DbCommand command, Guid globalCompanyPK)
		{
			command.CommandType = CommandType.StoredProcedure;
			command.AddParameter("@PickupTimeOutFrom", SqlDbType.DateTime, new DateTime(1900, 1, 1));
			command.AddParameter("@PickupTimeOutTo", SqlDbType.DateTime, new DateTime(1900, 1, 1));
			command.AddParameter("@DeliverTimeOutFrom", SqlDbType.DateTime, new DateTime(1900, 1, 1));
			command.AddParameter("@DeliverTimeOutTo", SqlDbType.DateTime, new DateTime(1900, 1, 1));
			command.AddParameter("@LegTypeMode", SqlDbType.VarChar, "");
			command.AddParameter("@TransportCompany", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@Vehicle", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@JobLocalClient", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@Chargeable", SqlDbType.VarChar, "");
			command.AddParameter("@LocalCartageBranch", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@JobHeaderFK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@CurrentCompanyPK", SqlDbType.UniqueIdentifier, globalCompanyPK);

			return command;
		}

		public JobHeaderDO InsertIntoDbAndReturnJobHeader_LocalCartageLegsByVehicleOrTransportCompany(Guid globalCompanyPK, Guid globalDepartmentPK, Guid globalBranchPK, Guid glHeaderPK, DbConnection connection)
		{
			var jobCartage = new JobCartageDO() { JJ_GB = globalBranchPK }.InsertAndReturnObject(connection);
			var jobBookedCtgMove = new JobBookedCtgMoveDO() { EW_JJ = jobCartage.PK }.InsertAndReturnObject(connection);
			var jobContainerLeg = new JobContainerLegsDO() { JU_EW = jobBookedCtgMove.PK }.InsertAndReturnObject(connection);

			var accChargeCode = new AccChargeCodeDO("FRT") { AC_GC = globalCompanyPK }.InsertAndReturnObject(connection);
			var jobHeader = new JobHeaderDO("WRK", globalBranchPK, globalDepartmentPK, globalCompanyPK, jobCartage.PK).InsertAndReturnObject(connection);

			var unpostedCostTransactionLine = new AccTransactionLinesDO("ACR", -5, globalDepartmentPK, globalBranchPK, globalCompanyPK) { AL_JH = jobHeader.PK, AL_AG = glHeaderPK }.InsertAndReturnObject(connection);
			var unpostedRevenueTransactionLine = new AccTransactionLinesDO("WIP", -15, globalDepartmentPK, globalBranchPK, globalCompanyPK) { AL_JH = jobHeader.PK, AL_AG = glHeaderPK }.InsertAndReturnObject(connection);
			var unpostedRevenuePerLegTransactionLine = new AccTransactionLinesDO("WIP", -25, globalDepartmentPK, globalBranchPK, globalCompanyPK) { AL_JH = jobHeader.PK, AL_AG = glHeaderPK }.InsertAndReturnObject(connection);

			new JobChargeDO(jobHeader.PK, accChargeCode.PK, globalBranchPK, globalCompanyPK, globalDepartmentPK) { JR_AL_APLine = unpostedCostTransactionLine.PK }.Insert(connection);
			new JobChargeDO(jobHeader.PK, accChargeCode.PK, globalBranchPK, globalCompanyPK, globalDepartmentPK) { JR_AL_ARLine = unpostedRevenueTransactionLine.PK }.Insert(connection);
			var unpostedRevenuePerLegJobCharge = new JobChargeDO(jobHeader.PK, accChargeCode.PK, globalBranchPK, globalCompanyPK, globalDepartmentPK) { JR_AL_ARLine = unpostedRevenuePerLegTransactionLine.PK }.InsertAndReturnObject(connection);
			new JobChargeAttribDO("CLG", jobContainerLeg.PK.ToString(), unpostedRevenuePerLegJobCharge.PK).Insert(connection);

			// old transaction line that shouldn't be used
			new AccTransactionLinesDO("WIP", -500, globalDepartmentPK, globalBranchPK, globalCompanyPK) { AL_JH = jobHeader.PK, AL_AG = glHeaderPK, AL_ReverseDate = new DateTime(2010, 6, 23, 10, 27, 0) }.Insert(connection);

			var postedCostTransactionLine = new AccTransactionLinesDO("CST", 50, globalDepartmentPK, globalBranchPK, globalCompanyPK) { AL_JH = jobHeader.PK, AL_ReverseDate = new DateTime(2010, 6, 23, 10, 27, 0) }.InsertAndReturnObject(connection);
			var postedRevenueTransactionLine = new AccTransactionLinesDO("REV", 60, globalDepartmentPK, globalBranchPK, globalCompanyPK) { AL_JH = jobHeader.PK, AL_ReverseDate = new DateTime(2010, 6, 23, 10, 27, 0) }.InsertAndReturnObject(connection);
			var postedRevenuePerLegTransactionLine = new AccTransactionLinesDO("REV", 70, globalDepartmentPK, globalBranchPK, globalCompanyPK) { AL_JH = jobHeader.PK, AL_ReverseDate = new DateTime(2010, 6, 23, 10, 27, 0) }.InsertAndReturnObject(connection);

			new JobChargeDO(jobHeader.PK, accChargeCode.PK, globalBranchPK, globalCompanyPK, globalDepartmentPK) { JR_AL_APLine = postedCostTransactionLine.PK }.Insert(connection);
			new JobChargeDO(jobHeader.PK, accChargeCode.PK, globalBranchPK, globalCompanyPK, globalDepartmentPK) { JR_AL_ARLine = postedRevenueTransactionLine.PK }.Insert(connection);
			var postedRevenuePerLegJobCharge = new JobChargeDO(jobHeader.PK, accChargeCode.PK, globalBranchPK, globalCompanyPK, globalDepartmentPK) { JR_AL_ARLine = postedRevenuePerLegTransactionLine.PK }.InsertAndReturnObject(connection);
			new JobChargeAttribDO("CLG", jobContainerLeg.PK.ToString(), postedRevenuePerLegJobCharge.PK).Insert(connection);

			return jobHeader;
		}
	}
}
