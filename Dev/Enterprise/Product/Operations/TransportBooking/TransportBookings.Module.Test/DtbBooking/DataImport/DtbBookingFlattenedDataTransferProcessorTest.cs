using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class DtbBookingFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "LOLA";
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "DELIVERY ADDRESS";

			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";

			flatRecord.Consolidation_KB_GoodsDescription = "DESC111";

			flatRecord.Pickup_KN_DropMode = "ABC";
			flatRecord.PickupConfirmation_KK_RequiredFrom = new ZDateTime(2008, 12, 3);
			flatRecord.PickupAddress_E2_Address1 = "PICKUP ADDRESS";

			flatRecord.Delivery_KN_DropMode = "XYZ";
			flatRecord.DeliveryConfirmation_KK_RequiredTo = new ZDateTime(2010, 12, 16);
			flatRecord.DeliveryAddress_OH_Code = "LOLA";
			flatRecord.DeliveryAddress_E2_Address1 = "DELIVERY ADDRESS";

			flatRecord.Delivery_ConNoteNo = "DlvConZub";

			flatRecord.LocalClientAddress_OH_Code = "LOLA";

			flatRecord.Pack_KP_GoodsDescription = "PACK1";
			flatRecord.Pack_KP_PackageQty = 2;

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\n", processor.Log);

			var booking = Factory.LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals(true, booking.IsInDatabase);

			AssertEquals("DESC111", booking.ConsolidationSingleJob.KB_GoodsDescription);

			AssertEquals(2, booking.Instructions.Count);
			AssertEquals(1, booking.FirstPickup.Confirmations.Count);
			AssertEquals("One for delivery, one for connote as it is provided", 2, booking.LastDelivery.Confirmations.Count);

			AssertEquals("ABC", booking.FirstPickup.KN_DropMode);
			AssertEquals(new ZDateTime(2008, 12, 3), booking.FirstPickup.FirstPickupConfirmation.KK_RequiredFrom);
			AssertEquals(true, booking.FirstPickup.Address.E2_AddressOverride);
			AssertEquals("PICKUP ADDRESS", booking.FirstPickup.Address.E2_Address1);

			AssertEquals("XYZ", booking.LastDelivery.KN_DropMode);
			AssertEquals(new ZDateTime(2010, 12, 16), booking.LastDelivery.LastDeliveryConfirmation.KK_RequiredTo);
			AssertEquals(false, booking.LastDelivery.Address.E2_AddressOverride);
			AssertEquals(address.PK, booking.LastDelivery.Address.E2_OA_Address);
			AssertEquals("DlvConZub", booking.LastDelivery.ConNoteNo);

			AssertEquals(1, booking.FirstPickup.DivotsWithPackages.Count);
			AssertEquals(2, booking.FirstPickup.DivotsWithPackages.Packages.Single().KP_PackageQty);
			AssertEquals("PACK1", booking.FirstPickup.DivotsWithPackages.Packages.Single().KP_GoodsDescription);
			AssertEquals(1, booking.FirstPickup.PackageDivots.Count);
			AssertEquals(2, booking.FirstPickup.PackageDivots[0].KD_Quantity);

			AssertEquals(1, booking.LastDelivery.DivotsWithPackages.Count);
			AssertEquals(booking.FirstPickup.DivotsWithPackages.Packages.Single(), booking.LastDelivery.DivotsWithPackages.Packages.Single());
			AssertEquals(2, booking.LastDelivery.DivotsWithPackages.Packages.Single().KP_PackageQty);
			AssertEquals("PACK1", booking.LastDelivery.DivotsWithPackages.Packages.Single().KP_GoodsDescription);
			AssertEquals(1, booking.LastDelivery.PackageDivots.Count);
			AssertEquals(2, booking.LastDelivery.PackageDivots[0].KD_Quantity);

			var requestedBillingParty = booking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertEquals(org.MainAddress.PK, requestedBillingParty.E2_OA_Address);
			AssertEquals(false, requestedBillingParty.E2_AddressOverride);
		}

		public void TestImport_MultipleLinesSameHeader()
		{
			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";
			flatRecord.Pack_KP_GoodsDescription = "PACK1";
			flatRecord.Pack_KP_PackageQty = 1;

			var flatRecord2 = collection.AddNew();
			flatRecord2.KM_TransportReference = "ALEE";
			flatRecord2.Pack_KP_GoodsDescription = "PACK2";
			flatRecord2.Pack_KP_PackageQty = 1;

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\nUpdating Job [Ref: ALEE]\r\n", processor.Log);

			var bookings = Factory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals(1, bookings.Length);
			var booking = bookings.Single();
			AssertEquals(2, booking.Instructions.Count);
			AssertEquals(1, booking.FirstPickup.Confirmations.Count);
			AssertEquals(1, booking.LastDelivery.Confirmations.Count);

			AssertEquals(2, booking.FirstPickup.DivotsWithPackages.Count);
			AssertEquals(1, booking.FirstPickup.DivotsWithPackages.Packages.ElementAt(0).KP_PackageQty);
			AssertEquals("PACK1", booking.FirstPickup.DivotsWithPackages.Packages.ElementAt(0).KP_GoodsDescription);
			AssertEquals(1, booking.FirstPickup.DivotsWithPackages.Packages.ElementAt(1).KP_PackageQty);
			AssertEquals("PACK2", booking.FirstPickup.DivotsWithPackages.Packages.ElementAt(1).KP_GoodsDescription);

			AssertEquals(2, booking.FirstPickup.PackageDivots.Count);
			AssertEquals(1, booking.FirstPickup.PackageDivots[0].KD_Quantity);
			AssertEquals(1, booking.FirstPickup.PackageDivots[1].KD_Quantity);
		}

		public void TestImport_MultipleLinesSameHeaderAndPackage()
		{
			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";
			flatRecord.Pack_KP_PackageID = "PACK1";
			flatRecord.Pack_KP_F3_NKPackType = Constants.PkgUnit.Box;
			flatRecord.Pack_KP_PackageQty = 1;

			var flatRecord2 = collection.AddNew();
			flatRecord2.KM_TransportReference = "ALEE";
			flatRecord2.Pack_KP_PackageID = "PACK1";
			flatRecord.Pack_KP_F3_NKPackType = Constants.PkgUnit.Pallet;

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\nUpdating Job [Ref: ALEE]\r\n", processor.Log);

			var bookings = Factory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals(1, bookings.Length);
			var booking = bookings.Single();

			AssertEquals(1, booking.FirstPickup.DivotsWithPackages.Count);
			AssertEquals("PackType has been updated to Pallet.", Constants.PkgUnit.Pallet, booking.FirstPickup.DivotsWithPackages.Packages.Single().KP_F3_NKPackType);
			AssertEquals("PACK1", booking.FirstPickup.DivotsWithPackages.Packages.ElementAt(0).KP_PackageID);
			AssertEquals(1, booking.FirstPickup.PackageDivots.Count);
			AssertEquals(1, booking.FirstPickup.PackageDivots[0].KD_Quantity);
		}

		public void TestImport_NoPackage()
		{
			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\n", processor.Log);

			var bookings = Factory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals(1, bookings.Length);
			var booking = bookings.Single();
			AssertEquals(0, booking.FirstPickup.DivotsWithPackages.Count);
		}

		public void TestImport_PackageHasError()
		{
			var packType = Factory.NewWithValidTestData<RefPackType>();
			packType.F3_Code = "TTT";
			packType.F3_Height = 48m;
			packType.F3_Length = 48m;
			packType.F3_Width = 48m;
			packType.F3_UnitOfDimension = Constants.Length.Centimetres;
			packType.F3_Weight = 5m;
			packType.F3_UnitOfWeight = Constants.Weight.Kilograms;

			Factory.Save();

			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";
			flatRecord.Pack_KP_DimensionUQ = Constants.Length.Metres;
			flatRecord.Pack_KP_F3_NKPackType = "TTT";
			flatRecord.Pack_KP_PackageQty = 33;

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals(@"Package has errors. Nothing was saved. Please refer to the following messages: 
Error - KP_Volume: The number 3,649,536 is too large, the maximum value allowed for Volume is 999,999.999.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, processor.IsCanceled);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var bookings = newFactory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals("Should not create TransportBooking", 0, bookings.Length);
		}

		public void TestImport_SamePickupInstructionUpdated()
		{
			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";
			flatRecord.Pack_KP_GoodsDescription = "PACK1";
			flatRecord.Pack_KP_PackageQty = 1;

			var flatRecord2 = collection.AddNew();
			flatRecord2.KM_TransportReference = "ALEE";
			flatRecord2.Pack_KP_GoodsDescription = "PACK2";
			flatRecord2.Pack_KP_PackageQty = 1;

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\nUpdating Job [Ref: ALEE]\r\n", processor.Log);

			var bookings = Factory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals(1, bookings.Length);
			var booking = bookings.Single();
			AssertEquals(2, booking.Instructions.Count);
			var pickupInstructions = booking.Instructions.Where(x => x.KN_InstructionType == InstructionTypes.Codes.PickUp).ToArray();
			AssertEquals(1, pickupInstructions.Length);
			AssertEquals(1, booking.FirstPickup.Confirmations.Count);

			AssertEquals(2, booking.FirstPickup.DivotsWithPackages.Count);
		}

		public void TestImport_DifferentPickupInstructionUpdated()
		{
			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";
			flatRecord.Pack_KP_GoodsDescription = "PACK1";
			flatRecord.Pack_KP_PackageQty = 1;
			flatRecord.Pickup_Sequence = 1;

			var flatRecord2 = collection.AddNew();
			flatRecord2.KM_TransportReference = "ALEE";
			flatRecord2.Pack_KP_GoodsDescription = "PACK2";
			flatRecord2.Pack_KP_PackageQty = 1;
			flatRecord2.Pickup_Sequence = 2;

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\nUpdating Job [Ref: ALEE]\r\n", processor.Log);

			var bookings = Factory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals(1, bookings.Length);
			var booking = bookings.Single();
			AssertEquals(3, booking.Instructions.Count);
			var pickupInstructions = booking.Instructions.Where(x => x.KN_InstructionType == InstructionTypes.Codes.PickUp).ToArray();
			AssertEquals(2, pickupInstructions.Length);
			AssertEquals(1, pickupInstructions[0].Confirmations.Count);
			AssertEquals(1, pickupInstructions[1].Confirmations.Count);

			AssertEquals(1, pickupInstructions[0].DivotsWithPackages.Count);
			AssertEquals(1, pickupInstructions[1].DivotsWithPackages.Count);
		}

		public void TestImport_SameDeliveryInstructionUpdated()
		{
			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";
			flatRecord.Pack_KP_GoodsDescription = "PACK1";
			flatRecord.Pack_KP_PackageQty = 1;

			var flatRecord2 = collection.AddNew();
			flatRecord2.KM_TransportReference = "ALEE";
			flatRecord2.Pack_KP_GoodsDescription = "PACK2";
			flatRecord2.Pack_KP_PackageQty = 1;

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\nUpdating Job [Ref: ALEE]\r\n", processor.Log);

			var bookings = Factory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals(1, bookings.Length);
			var booking = bookings.Single();
			AssertEquals(2, booking.Instructions.Count);
			var pickupInstructions = booking.Instructions.Where(x => x.KN_InstructionType == InstructionTypes.Codes.PickUp).ToArray();
			AssertEquals(1, pickupInstructions.Length);
			AssertEquals(1, pickupInstructions[0].Confirmations.Count);

			var deliveryInstructions = booking.Instructions.Where(x => x.KN_InstructionType == InstructionTypes.Codes.Delivery).ToArray();
			AssertEquals(1, deliveryInstructions.Length);
			AssertEquals(2, deliveryInstructions[0].DivotsWithPackages.Count);
		}

		public void TestImport_DifferentDeliveryInstructionUpdated()
		{
			var collection = new DtbBookingFlattenedCollection(Factory);
			var flatRecord = collection.AddNew();
			flatRecord.KM_TransportReference = "ALEE";
			flatRecord.Pack_KP_GoodsDescription = "PACK1";
			flatRecord.Pack_KP_PackageQty = 1;
			flatRecord.Delivery_Sequence = 2;

			var flatRecord2 = collection.AddNew();
			flatRecord2.KM_TransportReference = "ALEE";
			flatRecord2.Pack_KP_GoodsDescription = "PACK2";
			flatRecord2.Pack_KP_PackageQty = 1;
			flatRecord2.Delivery_Sequence = 3;

			var info = new ImportCollectionInfoImplForDtbBookingFlattened(collection);
			var processor = new DtbBookingFlattenedDataTransferProcessor(info);

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\nUpdating Job [Ref: ALEE]\r\n", processor.Log);

			var bookings = Factory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.KM_TransportReference, "ALEE"));
			AssertEquals(1, bookings.Length);
			var booking = bookings.Single();
			AssertEquals(3, booking.Instructions.Count);
			var pickupInstructions = booking.Instructions.Where(x => x.KN_InstructionType == InstructionTypes.Codes.PickUp).ToArray();
			AssertEquals(1, pickupInstructions.Length);
			AssertEquals(1, pickupInstructions[0].Confirmations.Count);

			var deliveryInstructions = booking.Instructions.Where(x => x.KN_InstructionType == InstructionTypes.Codes.Delivery).ToArray();
			AssertEquals(2, deliveryInstructions.Length);
			AssertEquals(1, deliveryInstructions[0].DivotsWithPackages.Count);
			AssertEquals(1, deliveryInstructions[1].DivotsWithPackages.Count);
		}
	}
}
