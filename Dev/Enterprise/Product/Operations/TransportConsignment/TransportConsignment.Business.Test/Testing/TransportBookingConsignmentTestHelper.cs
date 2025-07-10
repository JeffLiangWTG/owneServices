using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class TransportBookingConsignmentTestHelper : TransportCommonTestHelper, IDtbConsignmentTestHelper
	{
		public TransportBookingConsignmentTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region AddressPoint

		public DtbAddressPoint CreateAddressPointForDeliveryConfirmation(DtbBookingConsignment consignment)
		{
			Argument.GreaterThanOrEqual(consignment.DeliveryInstruction.Confirmations.Count, 1, "consignment.PickupInstruction.Confirmations");

			var deliveryDocAddress = consignment.DeliveryInstruction.Address;
			var deliveryOrgAddress = deliveryDocAddress != null && deliveryDocAddress.Address != null ? deliveryDocAddress.Address : CreateOrganisation("Honda").MainAddress;
			return CreateAddressPoint(consignment.DeliveryInstruction.Address.Address, consignment.DeliveryInstruction.Confirmations.ToArray());
		}

		public DtbAddressPoint CreateAddressPointForPickupConfirmation(DtbBookingConsignment consignment)
		{
			Argument.GreaterThanOrEqual(consignment.PickupInstruction.Confirmations.Count, 1, "consignment.PickupInstruction.Confirmations");

			var pickupDocAddress = consignment.PickupInstruction.Address;
			var pickupOrgAddress = pickupDocAddress != null && pickupDocAddress.Address != null ? pickupDocAddress.Address : CreateOrganisation("Honda").MainAddress;
			return CreateAddressPoint(pickupOrgAddress, consignment.PickupInstruction.Confirmations.ToArray());
		}

		public DtbAddressPoint CreateAddressPoint(DtbConsignmentInstruction instruction)
		{
			return CreateAddressPoint(instruction.Address.Address, instruction.Confirmations.ToArray());
		}

		public DtbAddressPoint CreateAddressPoint(OrgAddress address, params DtbConsignmentConfirmation[] confirmations)
		{
			var result = new DtbAddressPoint(address);
			result.Confirmations.AddRange(confirmations);

			return result;
		}

		#endregion

		#region Confirmation

		// For Instruction

		public DtbConsignmentConfirmation CreateConfirmation(DtbConsignmentInstruction instruction, ZString confirmationType)
		{
			return CreateConfirmation(instruction, confirmationType, "");
		}

		public DtbConsignmentConfirmation CreateConfirmation(DtbConsignmentInstruction instruction, ZString confirmationType, ZString referenceNum)
		{
			return CreateConfirmation(instruction, confirmationType, ZDateTime.Empty, ZDateTime.Empty, referenceNum);
		}

		public DtbConsignmentConfirmation CreateConfirmation(DtbConsignmentInstruction instruction, ZString confirmationType, ZDateTime fromDate, ZDateTime toDate)
		{
			return CreateConfirmation(instruction, confirmationType, fromDate, toDate, "");
		}

		public DtbConsignmentConfirmation CreateConfirmation(DtbConsignmentInstruction instruction, ZString confirmationType, ZDateTime fromDate, ZDateTime toDate, ZString referenceNum)
		{
			var result = instruction.Confirmations.AddNew(confirmationType);
			result.KK_ReferenceNum = referenceNum;
			result.KK_RequiredFrom = fromDate;
			result.KK_RequiredTo = toDate;

			return result;
		}

		// For Pkg Divot

		public DtbConsignmentConfirmation CreateConfirmation(DtbConsignmentInstructionPkgDivot pkgDivot, ZString confirmationType)
		{
			return pkgDivot.ConfirmationsDivotOnly.AddNew(confirmationType);
		}

		#endregion

		#region BookingConsignment

		// CreateBookingConsignment

		public DtbBookingConsignment CreateBookingConsignment()
		{
			return CreateBookingConsignment(CreateConsolidation());
		}

		public DtbBookingConsignment CreateBookingConsignment(DtbConsignmentConsolidation consolidation)
		{
			return consolidation.Bookings.AddNew();
		}

		// CreateBookingConsignmentWithTemplate

		public DtbBookingConsignment CreateBookingConsignmentWithTemplate()
		{
			return CreateBookingConsignmentWithTemplate(CreateConsolidation());
		}

		public DtbBookingConsignment CreateBookingConsignmentWithTemplate(DtbConsignmentConsolidation consolidation)
		{
			var result = CreateBookingConsignment(consolidation);
			result.KM_KT_NKBookingTemplate = DtbBookingConsignment.TemplateCode;

			return result;
		}

		public DtbBookingConsignment CreateBookingConsignmentWithTemplate(OrgHeader from, OrgHeader to)
		{
			var result = CreateBookingConsignmentWithTemplate();
			result.ConsolidationSingleJob.BookedByAddress.E2_OA_Address = from.MainAddress.PK;
			result.PickupInstruction.Address.OrganisationPK = from.PK;
			result.DeliveryInstruction.Address.OrganisationPK = to.PK;

			return result;
		}

		// CreateBookingConsignmentWithTemplateFromBooking

		public DtbBookingConsignment CreateBookingConsignmentWithTemplateFromBooking(ZString bookingID)
		{
			var consignment = CreateBookingConsignmentWithTemplate();
			CreateBooking(bookingID, consignment);

			return consignment;
		}

		// CreateBookingConsignmentWithTemplateAndAddresses

		public DtbBookingConsignment CreateBookingConsignmentWithTemplateAndAddresses()
		{
			var from = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "FROM");
			if (from == null)
			{
				from = CreateOrganisation("FROM");
				from.OH_FullName = "Honda Motorcycles";
				from.MainAddress.OA_Address1 = "1/2";
				from.MainAddress.OA_Address2 = "Some Other Street";
				from.MainAddress.OA_City = "Melbourne";
				from.MainAddress.OA_PostCode = "3039";
				from.MainAddress.OA_State = "VIC";
				from.MainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
				from.OH_Code = "FROM";
			}

			var to = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "TO");
			if (to == null)
			{
				to = CreateOrganisation("TO");
				to.OH_FullName = "Geoff's House";
				to.MainAddress.OA_Address1 = "123/124";
				to.MainAddress.OA_Address2 = "Fake Street";
				to.MainAddress.OA_City = "Sydney";
				to.MainAddress.OA_PostCode = "2067";
				to.MainAddress.OA_State = "NSW";
				to.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				to.OH_Code = "TO";
			}

			return CreateBookingConsignmentWithTemplate(from, to);
		}

		public DtbBookingConsignment CreateBookingConsignmentWithTemplateAndAddressesFromBooking(ZString bookingID)
		{
			var consignment = CreateBookingConsignmentWithTemplateAndAddresses();
			CreateBooking(bookingID, consignment);

			return consignment;
		}

		#endregion

		#region Consolidation

		public DtbConsignmentConsolidation CreateConsolidation(IDtbBooking parentBooking)
		{
			var consolidation = CreateConsolidation();
			consolidation.KB_ParentID = parentBooking.PK;
			consolidation.KB_ParentTableCode = ((BusinessObject)parentBooking).TablePrefix;
			return consolidation;
		}

		public DtbConsignmentConsolidation CreateConsolidation()
		{
			return CreateConsolidation("");
		}

		public DtbConsignmentConsolidation CreateConsolidation(ZString jobID)
		{
			var consolidation = Factory.New<DtbConsignmentConsolidation>();
			consolidation.KB_JobID = jobID;

			return consolidation;
		}

		#endregion

		#region Instruction

		public DtbConsignmentInstruction CreateInstruction(DtbBookingConsignment consignment, ZString instructionType)
		{
			return CreateInstruction(consignment, instructionType, "AVL");
		}

		public DtbConsignmentInstruction CreateInstruction(DtbBookingConsignment consignment, ZString instructionType, OrgAddress address)
		{
			return CreateInstruction(consignment, instructionType, "AVL", address);
		}

		public DtbConsignmentInstruction CreateInstruction(DtbBookingConsignment consignment, ZString instructionType, OrgAddress address, ZString dropMode)
		{
			return CreateInstruction(consignment, instructionType, "AVL", address, dropMode);
		}

		public DtbConsignmentInstruction CreateInstruction(DtbBookingConsignment consignment, ZString instructionType, ZString instructionStatus)
		{
			return CreateInstruction(consignment, instructionType, instructionStatus, null);
		}

		public DtbConsignmentInstruction CreateInstruction(DtbBookingConsignment consignment, ZString instructionType, ZString instructionStatus, OrgAddress address)
		{
			return CreateInstruction(consignment, instructionType, instructionStatus, address, "");
		}

		public DtbConsignmentInstruction CreateInstruction(DtbBookingConsignment consignment, ZString instructionType, ZString instructionStatus, OrgAddress address, ZString dropMode)
		{
			var instruction = consignment.Instructions.AddNew();
			instruction.KN_InstructionType = instructionType;
			instruction.KN_Status = instructionStatus;
			instruction.KN_DropMode = dropMode;

			if (address != null)
			{
				instruction.Address.E2_OA_Address = address.PK;
			}

			return instruction;
		}

		#endregion

		#region DtbBookingInstruction
		// For LTConsignmentConsolidationDataContextManagerTest.TestShipmentDataObjectReader

		public DtbBookingInstruction CreateInstruction(DtbBooking booking, ZString instructionType, ZString orgType, OrgAddress address)
		{
			var result = CreateInstruction(booking, instructionType);

			result.OrganisationType = orgType;
			if (address != null)
			{
				result.Address.E2_AddressOverride = false;
				result.Address.E2_OA_Address = address.PK;
			}

			return result;
		}

		public DtbBookingInstruction CreateInstruction(DtbBooking booking, string instructionType = InstructionTypes.Codes.PickUp)
		{
			var instruction = booking.Instructions.AddNew();
			instruction.KN_InstructionType = instructionType;
			return instruction;
		}

		#endregion

		#region InstructionPkgDivot

		public DtbConsignmentInstructionPkgDivot CreateInstructionPkgDivot(DtbConsignmentInstruction instruction, PkgPackage package, ZInt quantity)
		{
			var pkgDivot = instruction.PackageDivots.AddNew();
			pkgDivot.KD_KP_Package = package.PK;
			pkgDivot.KD_Quantity = quantity;

			return pkgDivot;
		}

		#endregion

		#region Package

		/// <summary>
		/// Packages are always added as Outers.
		/// </summary>
		public PkgPackage CreatePackage(DtbBookingConsignment consignment, ZDecimal weight, ZDecimal volume, int qty = 1)
		{
			return CreatePackage(consignment, PackingRegistry.Instance.OuterPackageUnit.Value, weight, volume, qty);
		}

		/// <summary>
		/// Packages are always added as Outers.
		/// </summary>
		public PkgPackage CreatePackage(DtbBookingConsignment consignment, ZString packType, ZDecimal weight, ZDecimal volume, int qty = 1)
		{
			var result = consignment.PackageJob.Packages.AddNew();
			result.KP_PackageQty = qty;
			result.KP_F3_NKPackType = packType;
			result.KP_Weight = weight;
			result.KP_Volume = volume;

			return result;
		}

		#endregion

		#region PackageDivot
		// For LTConsignmentConsolidationDataContextManagerTest.TestShipmentDataObjectReader

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

		#endregion

		#region ParentBooking

		public IDtbBooking CreateBooking(ZString bookingID)
		{
			var consolidation = (DtbBookingConsolidation)Factory.New<IDtbBookingConsolidation>();
			var booking = (IDtbBooking)consolidation.Bookings.AddNew();
			booking.KM_JobID = bookingID;

			return booking;
		}

		public IDtbBooking CreateBooking(ZString bookingID, DtbBookingConsignment consignment)
		{
			var booking = CreateBooking(bookingID);
			consignment.ConsolidationSingleJob.KB_ParentID = booking.PK;
			consignment.ConsolidationSingleJob.KB_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			return booking;
		}

		#endregion

		#region RunSheet

		public DtbConsignmentRunSheet CreateRunSheet()
		{
			return CreateRunSheet(ZDateTimeOffset.Now);
		}

		public DtbConsignmentRunSheet CreateRunSheet(string runSheetNumber)
		{
			return CreateRunSheet(runSheetNumber, null, null, null, null, null);
		}

		public DtbConsignmentRunSheet CreateRunSheet(OrgHeader transportCompany)
		{
			return CreateRunSheet("", null, null, null, null, transportCompany);
		}

		public DtbConsignmentRunSheet CreateRunSheet(GlbStaff driver)
		{
			return CreateRunSheet("", driver, null, null, null, null);
		}

		public DtbConsignmentRunSheet CreateRunSheet(RefEquipment vehicle)
		{
			return CreateRunSheet("", null, vehicle, null, null, null);
		}

		public DtbConsignmentRunSheet CreateRunSheet(ZDateTimeOffset startTime)
		{
			return CreateRunSheet(null, null, startTime, new ZDateTimeOffset(new ZDateTime(startTime.Year, startTime.Month, startTime.Day).EndOfDay(), DateTimeKind.Local));
		}

		public DtbConsignmentRunSheet CreateRunSheet(GlbStaff driver, RefEquipment truck, ZDateTimeOffset? startTime, ZDateTimeOffset? endTime)
		{
			return CreateRunSheet("", driver, truck, startTime, endTime, null);
		}

		public DtbConsignmentRunSheet CreateRunSheet(OrgHeader transportCompany, GlbStaff driver, RefEquipment truck, ZDateTimeOffset? startTime, ZDateTimeOffset? endTime)
		{
			return CreateRunSheet("", driver, truck, startTime, endTime, transportCompany);
		}

		public DtbConsignmentRunSheet CreateRunSheet(OrgHeader transportCompany, ZDateTimeOffset? startTime, ZDateTimeOffset? endTime)
		{
			return CreateRunSheet("", null, null, startTime, endTime, transportCompany);
		}

		public DtbConsignmentRunSheet CreateRunSheet(string runSheetNumber, GlbStaff driver, RefEquipment vehicle, ZDateTimeOffset? startTime, ZDateTimeOffset? endTime, OrgHeader transportCompany, string driverName = "")
		{
			var result = Factory.New<DtbConsignmentRunSheet>();
			if (!string.IsNullOrEmpty(runSheetNumber))
			{
				result.KG_RunSheetNumber = runSheetNumber;
			}

			if (startTime.HasValue)
			{
				result.KG_StartTime = startTime.Value;
			}

			if (endTime.HasValue)
			{
				result.KG_EndTime = endTime.Value;
			}

			if (driver != null)
			{
				result.KG_GS_NKTruckDriver = driver.GS_Code;
			}

			if (vehicle != null)
			{
				result.KG_RQ_Truck = vehicle.PK;
			}

			if (transportCompany != null)
			{
				result.KG_OH_TransportCo = transportCompany.PK;
			}

			if (!string.IsNullOrEmpty(driverName))
			{
				result.KG_AdHocDriversName = driverName;
			}
			result.KG_GB_Branch = GlbBranch.CurrentBranch.PK;

			return result;
		}

		#endregion

		#region RunSheetInstruction

		public DtbConsignmentRunSheetInstruction CreateRunSheetInstruction(DtbAddressPoint addressPoint)
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			instruction.Confirmations.AddRange(addressPoint.Confirmations);

			return instruction;
		}

		public DtbConsignmentRunSheetInstruction CreateRunSheetInstruction(DtbConsignmentConfirmation confirmation)
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			confirmation.KK_K1_RunSheetInstruction = instruction.PK;

			return instruction;
		}

		public DtbConsignmentRunSheetInstruction CreateRunSheetInstruction(DtbConsignmentRunSheet runSheet, params DtbConsignmentConfirmation[] confirmations)
		{
			return runSheet.AddNewRunSheetInstructions(confirmations).Single(); // Do not use this Helper method if multiple RSI should be created i.e. multiple confirmation types using DirectMode.
		}

		#endregion

		#region Signature

		public ZBlob GetSignature()
		{
			var stream = new MemoryStream();
			var writer = new BinaryWriter(stream);

			writer.Write(200); // width
			writer.Write(100); // height
			writer.Write(2); // number of lines
			writer.Write(6); // number of points for 1st line

			// D
			writer.Write(20); // line 1, point 1, x
			writer.Write(20); // line 1, point 1, y

			writer.Write(20); // line 1, point 2, x
			writer.Write(80); // line 1, point 2, y

			writer.Write(30); // ...
			writer.Write(80);

			writer.Write(40);
			writer.Write(60);

			writer.Write(30);
			writer.Write(20);

			writer.Write(20);
			writer.Write(20);

			// B
			writer.Write(9); // number of points for 2nd line

			writer.Write(50);
			writer.Write(20);

			writer.Write(50);
			writer.Write(80);

			writer.Write(60);
			writer.Write(80);

			writer.Write(70);
			writer.Write(70);

			writer.Write(60);
			writer.Write(60);

			writer.Write(50);
			writer.Write(50);

			writer.Write(60);
			writer.Write(40);

			writer.Write(70);
			writer.Write(30);

			writer.Write(50);
			writer.Write(20);

			return new ZBlob(stream.ToArray());
		}

		#endregion

		#region IDtbConsignmentTestHelper Members

		BusinessObject IDtbConsignmentTestHelper.CreateConfirmation(BusinessObject instruction, ZString confirmationType)
		{
			return CreateConfirmation((DtbConsignmentInstruction)instruction, confirmationType);
		}

		BusinessObject IDtbConsignmentTestHelper.CreateConsignment()
		{
			return CreateBookingConsignment();
		}

		BusinessObject IDtbConsignmentTestHelper.CreateInstruction(BusinessObject consignment, ZString instructionType)
		{
			return CreateInstruction((DtbBookingConsignment)consignment, instructionType);
		}

		BusinessObject IDtbConsignmentTestHelper.CreateRunSheet(string runSheetNumber)
		{
			return CreateRunSheet(runSheetNumber);
		}

		BusinessObject IDtbConsignmentTestHelper.CreateRunSheetInstruction(BusinessObject confirmation)
		{
			return CreateRunSheetInstruction((DtbConsignmentConfirmation)confirmation);
		}

		#endregion
	}
}
