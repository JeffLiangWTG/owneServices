#if DEBUG
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class TransportConsignmentTestHelper : TransportCommonTestHelper
	{
		public TransportConsignmentTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Consignment

		public DtbConsignment CreateConsignment()
		{
			return CreateConsignment("LTC001");
		}

		public DtbConsignment CreateConsignment(ZString jobID)
		{
			return CreateConsignment(jobID, ConsignmentStatuses.Codes.Booked);
		}

		public DtbConsignment CreateConsignment(ZString jobID, ZString status)
		{
			return CreateConsignment(Constants.CartageDirection.Local, status, 1, Constants.Length.Kilometres, jobID);
		}

		public DtbConsignment CreateConsignment(ZString direction, ZString status, ZDecimal distance, ZString distanceUnit, ZString jobID)
		{
			return CreateConsignment(direction, status, distance, distanceUnit, jobID, ZString.Empty);
		}

		public DtbConsignment CreateConsignment(ZString direction, ZString status, ZDecimal distance, ZString distanceUnit, ZString jobID, ZString connoteNumber)
		{
			var consignment = Factory.New<DtbConsignment>();
			consignment.LTC_Direction = direction;
			consignment.LTC_Distance = distance;
			consignment.LTC_DistanceUnit = distanceUnit;
			consignment.LTC_Status = status;
			consignment.LTC_JobID = jobID;
			consignment.LTC_ConnoteNumber = connoteNumber;

			return consignment;
		}

		public DtbConsignment CreateConsignment(ZString serviceLevel, ZString direction, ZString connoteNumber, ZDecimal goodsValue, ZString goodsValueCurrency, ZDecimal insuranceValue, ZString insuranceValueCurrency, ZString jobType, ZString incoterm, ZString incotermAdditionalTerms)
		{
			var consignment = Factory.New<DtbConsignment>();
			consignment.LTC_RS_NKServiceLevel = serviceLevel;
			consignment.LTC_Direction = direction;
			consignment.LTC_ConnoteNumber = connoteNumber;
			consignment.LTC_GoodsValue = goodsValue;
			consignment.LTC_RX_NKGoodsValueCurrency = goodsValueCurrency;
			consignment.LTC_InsuranceValue = insuranceValue;
			consignment.LTC_RX_NKInsuranceValueCurrency = insuranceValueCurrency;
			consignment.LTC_JobType = jobType;
			consignment.LTC_Incoterm = incoterm;
			consignment.LTC_AdditionalTerms = incotermAdditionalTerms;

			return consignment;
		}

		#endregion

		#region AdditionalReference
		public void CreateAdditionalReference(DtbConsignment consignment, ZDateTime issueDate, ZString entryType, ZString entryNum, ZString lineReference)
		{
			var reference = consignment.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = entryType;
			reference.CE_EntryNum = entryNum;
			reference.CE_EntryLineReference = lineReference;
			reference.CE_IssueDate = issueDate;
		}
		#endregion

		#region ConsignmentAddress

		public DtbConsignmentAddress CreateConsignmentAddress()
		{
			var consignment = CreateConsignment();

			return CreateConsignmentAddress(consignment);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(ZString addressType)
		{
			var consignment = CreateConsignment();

			return CreateConsignmentAddress(consignment, addressType);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbConsignment consignment)
		{
			return CreateConsignmentAddress(consignment, ZString.Empty);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbConsignment consignment, ZString addressType)
		{
			return CreateConsignmentAddress(consignment, addressType, null);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbConsignment consignment, ZString addressType, OrgAddress address)
		{
			return CreateConsignmentAddressWithAutoSequence(consignment, addressType, address, "PIC");
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbConsignment consignment, ZString addressType, ZInt sequence, ZString orgType)
		{
			var consignmentAddress = CreateConsignmentAddress(consignment, addressType, null, ConsignmentAddressStatus.Codes.Allocated, sequence);

			return consignmentAddress;
		}

		public DtbConsignmentAddress CreateConsignmentAddressWithAutoSequence(DtbConsignment consignment, ZString addressType, OrgAddress address, ZString addressStatus, DocAddressType docAddressType = DocAddressType.LocalCartageCFS)
		{
			var sequence = (consignment == null ? 1 : consignment.Addresses.Select(ca => ca.LTS_Sequence).OrderByDescending(s => s).FirstOrDefault() + 1);
			return CreateConsignmentAddress(consignment, addressType, address, addressStatus, sequence, docAddressType);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbConsignment consignment, ZString addressType, OrgAddress address, ZString addressStatus, ZInt sequence, DocAddressType docAddressType = DocAddressType.LocalCartageCFS)
		{
			var consignmentAddress = Factory.New<DtbConsignmentAddress>();
			consignmentAddress.LTS_LTC_Consignment = consignment != null ? consignment.PK : ZGuid.Empty;
			consignmentAddress.LTS_InstructionType = addressType;

			consignmentAddress.LTS_Sequence = sequence;
			consignmentAddress.LTS_Status = addressStatus;

			if (address != null)
			{
				consignmentAddress.DocAddresses.FindOrCreateWithDocAddressType(docAddressType).E2_OA_Address = address.PK;
			}

			return consignmentAddress;
		}

		public DtbConsignmentAddress CreateConsignmentAddressWithAction(DtbConsignment consignment, ZString addressType, ZString actionType, DocAddressType docAddressType = DocAddressType.LocalCartageCFS, OrgAddress address = null)
		{
			var consignmentAddress = CreateConsignmentAddressWithAutoSequence(consignment, addressType, address, ConsignmentAddressStatus.Codes.Allocated, docAddressType);
			var action = CreateConsignmentAction(consignmentAddress, actionType);
			return consignmentAddress;
		}

		#endregion

		#region RunSheetInstruciton

		public DtbConsignmentRunSheetInstruction CreateRunSheetInstruction(DtbConsignmentAction action, ZGuid runSheetPK)
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			instruction.K1_KG_RunSheet = runSheetPK;
			action.LTA_K1_RunSheetInstruction = instruction.PK;
			return instruction;
		}

		public DtbConsignmentRunSheetInstruction CreateRunSheetInstruction(DtbConsignmentAction action, ZGuid runSheetPK, int seqNo = 0)
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			instruction.K1_KG_RunSheet = runSheetPK;
			instruction.K1_Sequence = seqNo;
			action.LTA_K1_RunSheetInstruction = instruction.PK;
			return instruction;
		}

		#endregion

		#region CreateConsignmentAction

		public DtbConsignmentAction CreateConsignmentAction()
		{
			var address = CreateConsignmentAddress();

			return CreateConsignmentAction(address);
		}

		public DtbConsignmentAction CreateConsignmentAction(DtbConsignmentAddress address)
		{
			return CreateConsignmentAction(address, ZString.Empty);
		}

		public DtbConsignmentAction CreateConsignmentAction(DtbConsignmentAddress address, ZString actionType)
		{
			return CreateConsignmentAction(address, actionType, ZDateTimeOffset.Empty);
		}

		public DtbConsignmentAction CreateConsignmentAction(DtbConsignmentAddress address, ZString actionType, ZDateTimeOffset estimated)
		{
			return CreateConsignmentAction(address, actionType, estimated, false);
		}

		public DtbConsignmentAction CreateConsignmentAction(DtbConsignmentAddress address, ZString actionType, ZDateTimeOffset estimated, ZBool createRunSheetInstruction)
		{
			var action = Factory.New<DtbConsignmentAction>();
			action.LTA_LTS_ConsignmentAddress = address != null ? address.PK : ZGuid.Empty;
			action.LTA_ActionType = actionType;
			action.LTA_EstimatedTime = estimated;

			if (createRunSheetInstruction)
			{
				var runsheet = Factory.New<DtbConsignmentRunSheet>();
				var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
				action.LTA_K1_RunSheetInstruction = instruction.PK;
				instruction.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-10);
				instruction.K1_KG_RunSheet = runsheet.PK;
			}

			return action;
		}

		#endregion

		public DtbConsignmentLeg CreateConsignmentLeg(DtbConsignment consignment, DtbConsignmentAction pickupConsignmentAction, DtbConsignmentAction deliveryConsignmentAction)
		{
			var consignmentLeg = Factory.New<DtbConsignmentLeg>();
			consignmentLeg.LTG_LTC_Consignment = consignment?.PK ?? ZGuid.Empty;
			consignmentLeg.LTG_LTA_Pickup = pickupConsignmentAction?.PK ?? ZGuid.Empty;
			consignmentLeg.LTG_LTA_Delivery = deliveryConsignmentAction?.PK ?? ZGuid.Empty;

			return consignmentLeg;
		}

		#region CreateRunSheet

		public DtbConsignmentRunSheet CreateRunSheet(OrgHeader transportCompany = null,
			string runSheetNumber = "", ZGuid? truckPK = null, ZDateTimeOffset? startTime = null, ZDateTimeOffset? endTime = null,
			ZDateTimeOffset? actualStartTime = null, ZDateTimeOffset? actualEndTime = null)
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();

			if (transportCompany != null)
			{
				runSheet.KG_OH_TransportCo = transportCompany.PK;
			}

			if (!string.IsNullOrEmpty(runSheetNumber))
			{
				runSheet.KG_RunSheetNumber = runSheetNumber;
			}

			if (truckPK.HasValue)
			{
				runSheet.KG_RQ_Truck = truckPK.Value;
			}

			if (startTime.HasValue)
			{
				runSheet.KG_StartTime = startTime.Value;
			}

			if (endTime.HasValue)
			{
				runSheet.KG_EndTime = endTime.Value;
			}

			if (actualStartTime.HasValue)
			{
				runSheet.KG_ActualStartTime = actualStartTime.Value;
			}

			if (actualEndTime.HasValue)
			{
				runSheet.KG_ActualEndTime = actualEndTime.Value;
			}

			return runSheet;
		}

		#endregion

		#region CreateRunSheetInstruction

		public DtbConsignmentRunSheetInstruction CreateRunSheetInstruction(DtbConsignmentRunSheet runSheet, params DtbConsignmentAction[] actions)
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			instruction.K1_KG_RunSheet = runSheet.PK;
			actions.ForEach(a => a.LTA_K1_RunSheetInstruction = instruction.PK);
			return instruction;
		}

		#endregion

		#region CreateOrganisation

		public OrgHeader CreateOrganisation(string orgCode, string closestPortUNLOCO)
		{
			var header = CreateOrganisation(orgCode);
			header.OH_RL_NKClosestPort = closestPortUNLOCO;
			return header;
		}

		#endregion

		#region CreateOrgAddress

		public OrgAddress CreateOrgAddress(OrgHeader header, string portCode, string addressCode = "", decimal? latitude = null, decimal? longitude = null)
		{
			var address = header.Addresses.AddNew();
			address.Address1 = "Test";
			address.AddressCode = addressCode;
			address.OA_RL_NKRelatedPortCode = portCode;
			if (latitude != null)
			{
				address.OA_Latitude = new ZDecimal(latitude);
			}
			if (longitude != null)
			{
				address.OA_Longitude = new ZDecimal(longitude);
			}
			return address;
		}

		#endregion

		#region Service

		public JobService CreateService(DtbConsignment consignment, ZString serviceCode, OrgHeader serviceOrg, ZDateTime bookedDate, ZString reference, ZString serviceNot)
		{
			var service = consignment.Services.AddIfNotExists(serviceCode).First();
			service.ES_Booked = bookedDate;
			service.ES_OA_Location = serviceOrg.MainAddress.PK;
			service.ES_OH_Contractor = serviceOrg.PK;
			service.ES_References = reference;
			service.ES_ServiceNote = serviceNot;

			return service;
		}

		#endregion

		#region Package

		public PkgPackage CreatePackage(DtbConsignment consignment, ZDecimal weight, ZDecimal volume, int qty = 1)
		{
			return CreatePackage(consignment, PackingRegistry.Instance.OuterPackageUnit.Value, weight, volume, qty);
		}

		public PkgPackage CreatePackage(DtbConsignment consignment, ZString packageID, ZString packType)
		{
			return CreatePackage(consignment, packType, 1m, 1m, 1, packageID);
		}

		public PkgPackage CreateChildPackage(PkgPackage package, ZString packageID, ZString packType)
		{
			var childPackage = package.Packages.AddNew();
			childPackage.KP_PackageQty = 1;
			childPackage.KP_F3_NKPackType = packType;
			childPackage.KP_PackageID = packageID;
			return childPackage;
		}

		public PkgPackage CreatePackage(DtbConsignment consignment, ZString packType, ZDecimal weight, ZDecimal volume, int qty = 1, string packageID = "")
		{
			var result = consignment.PackageJob.Packages.AddNew();
			result.KP_PackageQty = qty;
			result.KP_F3_NKPackType = packType;
			result.KP_Weight = weight;
			result.KP_Volume = volume;
			result.KP_PackageID = packageID;

			return result;
		}

		public PkgPackage CreatePackage(PkgPackageJob packageJob, ZString packType, string packageID)
		{
			var package = packageJob.Packages.AddNew(packType, packageID);

			package.KP_Volume = ZDecimal.Zero;

			return package;
		}

		#endregion

		public DtbConsignmentActionPackageDivot CreatePackageDivot(DtbConsignmentAction action, PkgPackage package)
		{
			var divot = action.PackageDivots.AddNew();

			divot.LTP_KP_Package = package.PK;
			divot.LTP_PackageQuantity = 1;

			return divot;
		}

		public JobHeader CreateJobHeader(DtbConsignment consignment)
		{
			var jobHeader = new JobHeader.Loader(consignment).TryCreate();

			return jobHeader;
		}

		public DtbBooking CreateBookingWithConsolidation()
		{
			var consolidation = CreateConsolidation();
			var result = CreateBooking(consolidation);

			return result;
		}

		public DtbBooking CreateBooking(DtbBookingConsolidation consolidation)
		{
			return consolidation.Bookings.AddNew();
		}

		public DtbBookingConsolidation CreateConsolidation(IForwardingShipment shipment)
		{
			var result = CreateConsolidation();
			result.KB_ParentID = shipment.PK;
			result.KB_ParentTableCode = "JS";
			result.KB_JobDirection = "PIC";

			return result;
		}

		public DtbBookingConsolidation CreateConsolidation()
		{
			return Factory.New<DtbBookingConsolidation>();
		}

		public IForwardingShipment CreateForwardingShipment()
		{
			var shipment = Factory.New<IForwardingShipment>();

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			return shipment;
		}

		public DtbConsignmentVariation CreateVariation()
		{
			var consignment = CreateConsignment("CN001");
			var consignmentAddress = CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
			var package = CreatePackage(consignment, 1, 2, 3);
			var divot = CreatePackageDivot(action, package);

			var runSheet = CreateRunSheet();
			CreateRunSheetInstruction(action, runSheet.PK);

			return CreateVariation(consignment, divot);
		}

		public DtbConsignmentVariation CreateVariation(BusinessObject parentJob, DtbConsignmentActionPackageDivot parent)
		{
			var variation = Factory.New<DtbConsignmentVariation>();
			variation.LTV_ParentId = parent.PK;
			variation.LTV_ParentTableCode = DtbConsignmentActionPackageDivotSchema.Constants.Prefix;
			variation.LTV_JobId = parentJob.PK;
			variation.LTV_JobTableCode = parentJob.TablePrefix;
			variation.LTV_Status = DtbConsignmentVariationStatuses.Codes.Open;
			variation.LTV_Type = DtbConsignmentVariationTypes.Codes.QuantityWeightVolume;
			variation.LTV_PlannedQty = 1;
			variation.LTV_ActualQty = 2;

			return variation;
		}

		public JobHeader CreateJobHeaderForConsignment(DtbConsignment consignment)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			job.JH_ParentTableCode = DtbConsignmentSchema.Constants.Prefix;

			return job;
		}

		public JobCharge CreateJobCharge(JobHeader job)
		{
			var result = Factory.New<JobCharge>();
			result.JR_JH = job.PK;
			return result;
		}

		#region Consignment Lodgement

		public DtbConsignmentLodgement CreateConsignmentLodgement()
		{
			return CreateConsignmentLodgement("LCL_001");
		}

		public DtbConsignmentLodgement CreateConsignmentLodgement(string lodgementID)
		{
			var consignmentLodgement = Factory.New<DtbConsignmentLodgement>();
			consignmentLodgement.LCL_LodgementID = lodgementID;
			consignmentLodgement.LCL_Type = DtbConsignmentLodgementTypes.Codes.Booking;
			consignmentLodgement.LCL_MovementType = DtbConsignmentLodgementMovementTypes.Codes.Outbound;
			consignmentLodgement.LCL_Type = DtbConsignmentLodgementTypes.Codes.Booking;
			return consignmentLodgement;
		}

		public DtbConsignmentLodgementPivot CreateConsignmentLodgementPivot()
		{
			var consignment = CreateConsignment();
			var conLodgement = CreateConsignmentLodgement();
			return CreateConsignmentLodgementPivot(consignment,conLodgement);
		}

		public DtbConsignmentLodgementPivot CreateConsignmentLodgementPivot(DtbConsignment consignment, DtbConsignmentLodgement conLodgement)
		{
			var consignmentLodgementPivot = Factory.New<DtbConsignmentLodgementPivot>();
			consignmentLodgementPivot.LCP_LCL_Lodgement = conLodgement.PK;
			consignmentLodgementPivot.LCP_LTC_Consignment = consignment.PK;
			return consignmentLodgementPivot;
		}

		#endregion
	}
}
#endif
