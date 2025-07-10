using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentAction))]
	sealed class DtbConsignmentActionTest : EnterpriseBusinessObjectTestCase
	{
		#region TestIsPickUp

		public void TestIsPickUp()
		{
			var action = Helper.CreateConsignmentAction();
			AssertEquals(false, action.IsPickUp);

			action.LTA_ActionType = InstructionTypes.Codes.PickUp;
			AssertEquals(true, action.IsPickUp);

			action.LTA_ActionType = InstructionTypes.Codes.Delivery;
			AssertEquals(false, action.IsPickUp);
		}

		#endregion

		#region TestIsDelivery

		public void TestIsDelivery()
		{
			var action = Helper.CreateConsignmentAction();
			AssertEquals(false, action.IsDelivery);

			action.LTA_ActionType = InstructionTypes.Codes.Delivery;
			AssertEquals(true, action.IsDelivery);

			action.LTA_ActionType = InstructionTypes.Codes.PickUp;
			AssertEquals(false, action.IsDelivery);
		}

		#endregion

		#region TestRunSheetInstruction

		public void TestRunSheetInstruction()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = address.Actions.AddNew();
			AssertNull(action.RunSheetInstruction);

			var runsheetInstruction = TransportBookingHelper.CreateRunSheet().RunSheetInstructions.AddNew();
			action.LTA_K1_RunSheetInstruction = runsheetInstruction.PK;
			AssertEquals(runsheetInstruction, action.RunSheetInstruction);
		}

		#endregion

		#region TestConsignmentAddress

		public void TestConsignmentAddress()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = address.Actions.AddNew();
			AssertEquals(address, action.ConsignmentAddress);
		}

		#endregion

		#region Properties

		#region TestLTA_LTS_ConsignmentAddress

		public void TestLTA_LTS_ConsignmentAddress()
		{
			var consignment = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var address2 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);

			var action = address1.Actions.AddNew();
			AssertEquals(address1.PK, action.LTA_LTS_ConsignmentAddress);

			action.LTA_LTS_ConsignmentAddress = address2.PK;
			AssertEquals(address2.PK, action.LTA_LTS_ConsignmentAddress);
		}

		#endregion

		#endregion

		#region Calculated Properties

		#region TestBookingID

		public void TestBookingID()
		{
			var booking = TransportBookingHelper.CreateBooking("B123");
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var action = address.Actions[0];
			AssertEquals("", action.BookingID);

			SetBookingIdToConsignment(booking, consignment);
			AssertEquals("B123", action.BookingID);
		}

		#endregion

		#region TestConsignmentID

		public void TestConsignmentID()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var action = address.Actions[0];

			AssertEquals("LTC001", action.ConsignmentID);
		}

		#endregion

		#region TestBillToPartyCode

		public void TestBillToPartyCode()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var action = address.Actions[0];
			AssertEquals("No Billing exists, should be empty.", "", action.BillToPartyCode);

			new JobHeader.Loader(consignment).TryLoadOrCreate();
			AssertNotNull("Precondition", consignment.Job);
			AssertEquals("Bill exists without a Local Client, should be empty.", "", action.BillToPartyCode);

			var billToParty = Helper.CreateOrganisation("BtP");
			billToParty.OH_FullName = "Different to Code";
			consignment.Job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;
			AssertEquals("BtP", action.BillToPartyCode);
		}

		#endregion

		#region TestConsignorAddress

		public void TestConsignorAddress()
		{
			var consignor = CreateOrg("consignor", "Honda Motorcycles", "1/2", "Some Other Street", "Melbourne", "3039", "VIC");
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, consignor.MainAddress);
			var action = address.Actions[0];

			AssertEquals("Honda Motorcycles", action.ConsignorName);
			AssertEquals("1/2 SOME OTHER STREET MELBOURNE VIC 3039", action.ConsignorAddressAsSingleLine);
			AssertEquals("Melbourne", action.ConsignorCity);
			AssertEquals("Honda Motorcycles", action.ConsignorName);
			AssertEquals("3039", action.ConsignorPostcode);
			AssertEquals("VIC", action.ConsignorState);
		}

		public void TestConsignorAddress_EmptyAddress()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var action = address.Actions[0];

			AssertEquals(ZString.Empty, action.ConsignorName);
			AssertEquals(ZString.Empty, action.ConsignorAddressAsSingleLine);
			AssertEquals(ZString.Empty, action.ConsignorCity);
			AssertEquals(ZString.Empty, action.ConsignorPostcode);
			AssertEquals(ZString.Empty, action.ConsignorState);
		}

		#endregion

		#region TestConsignorReference

		public void TestConsignorReference()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var pickupAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var deliveryAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);

			var pickupAction = pickupAddress.Actions[0];
			var deliveryAction = deliveryAddress.Actions[0];

			pickupAction.LTA_ReferenceNumber = "Consignor Ref.";
			deliveryAction.LTA_ReferenceNumber = "Consignee Ref.";
			AssertEquals("Consignor Ref.", pickupAction.ConsignorReference);
			AssertEquals("Consignor Ref.", deliveryAction.ConsignorReference);
		}

		#endregion

		#region TestConsigneeAddress

		public void TestConsigneeAddress()
		{
			var consignee = CreateOrg("consignee", "Wisetech", "72", "O'Riordan Street", "Alexandria", "2015", "NSW");
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, consignee.MainAddress);
			var action = address.Actions[0];

			AssertEquals("Wisetech", action.ConsigneeName);
			AssertEquals("72 O'RIORDAN STREET ALEXANDRIA NSW 2015", action.ConsigneeAddressAsSingleLine);
			AssertEquals("Alexandria", action.ConsigneeCity);
			AssertEquals("2015", action.ConsigneePostcode);
			AssertEquals("NSW", action.ConsigneeState);
		}

		public void TestConsigneeAddress_EmptyAddress()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);
			var action = address.Actions[0];

			AssertEquals(ZString.Empty, action.ConsigneeName);
			AssertEquals(ZString.Empty, action.ConsigneeAddressAsSingleLine);
			AssertEquals(ZString.Empty, action.ConsigneeCity);
			AssertEquals(ZString.Empty, action.ConsigneePostcode);
			AssertEquals(ZString.Empty, action.ConsigneeState);
		}

		#endregion

		#region TestConsigneeReference

		public void TestConsigneeReference()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var pickupAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var deliveryAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);

			var pickupAction = pickupAddress.Actions[0];
			var deliveryAction = deliveryAddress.Actions[0];

			pickupAction.LTA_ReferenceNumber = "Consignor Ref.";
			deliveryAction.LTA_ReferenceNumber = "Consignee Ref.";

			AssertEquals("Consignee Ref.", pickupAction.ConsigneeReference);
			AssertEquals("Consignee Ref.", deliveryAction.ConsigneeReference);
		}

		#endregion

		#region TestReceiveBySignature

		public void TestReceiveBySignature()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var pickupAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var pickupAction = pickupAddress.Actions[0];
			AssertEquals(false, pickupAction.HasSignature);

			pickupAction.LTA_SignedBy = "BLAH";
			pickupAction.LTA_SignedBySignature = ZBlob.FromAscii("signatureBytes");

			AssertEquals("BLAH", pickupAction.LTA_SignedBy);
			AssertEquals(ZBlob.FromAscii("signatureBytes"), pickupAction.LTA_SignedBySignature);
			AssertEquals(true, pickupAction.HasSignature);
		}

		#endregion

		#region TestLoosePackageIds

		public void TestLoosePackageIds()
		{
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var package1 = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "P1");
			var package2 = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package, "P2");

			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, consignment.LoosePackages);
			AssertContainsExactElementsInAnyOrder(new[] { "P1", "P2" }, pickupAction.LoosePackageIds);
			AssertContainsExactElementsInAnyOrder(new[] { "P1", "P2" }, deliveryAction.LoosePackageIds);
		}

		#endregion

		#region CreateOrg

		OrgHeader CreateOrg(ZString code, ZString fullName, ZString address1, ZString address2, ZString city, ZString postCode, ZString state)
		{
			var org = Helper.CreateOrganisation(code);
			org.OH_FullName = fullName;

			var address = org.MainAddress;
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_State = state;

			return org;
		}

		#endregion

		#region Test Packages

		public void TestPackages_WhenNoPackageDivot_ShouldReturnAllPackagesInPackageJob()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var pickupAction = Helper.CreateConsignmentAction(address, InstructionTypes.Codes.PickUp);
			var package1 = Helper.CreatePackage(consignment, "P1", Constants.PkgUnit.Box);
			var package2 = Helper.CreatePackage(consignment, "P2", Constants.PkgUnit.Case);
			helper.CreatePackageDivot(pickupAction, package1);
			helper.CreatePackageDivot(pickupAction, package2);

			AssertEquals(2, pickupAction.Packages.Count());
			AssertEquals(1, pickupAction.Packages.Count(p => p.KP_PackageID == "P1"));
			AssertEquals(1, pickupAction.Packages.Count(p => p.KP_PackageID == "P2"));
		}

		public void TestPackages_WhenPackageDivot_ShouldReturnPackagesInPackageDivot()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var pickupAction = Helper.CreateConsignmentAction(address, InstructionTypes.Codes.PickUp);
			var package1 = Helper.CreatePackage(consignment, "P1", Constants.PkgUnit.Box);
			Helper.CreatePackage(consignment, "P2", Constants.PkgUnit.Case);
			helper.CreatePackageDivot(pickupAction, package1);

			AssertEquals(1, pickupAction.Packages.Count());
			AssertEquals(1, pickupAction.Packages.Count(p => p.KP_PackageID == "P1"));
			AssertEquals(0, pickupAction.Packages.Count(p => p.KP_PackageID == "P2"));
		}

		#endregion

		#endregion

		#region IConsignmentAction

		#region TestKeyForCache

		public void TestKeyForCache()
		{
			var action = Factory.New<DtbConsignmentAction>();
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not be setting KeyForCache to null.", () => action.KeyForCache = null);

			action.KeyForCache = "Key";
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not be setting KeyForCache twice.", () => action.KeyForCache = "Another Key");
		}

		#endregion

		#region TestBookedPickupPackageList

		public void TestBookedPickupPackageList()
		{
			AssertPackageTotalProperty<ZString>("4x CTN,5x PLT", "", a => string.Join(",", a.BookedPickupPackageList.PackTypeCounts), valueIsBlankForDelivery: true);
		}

		#endregion

		#region TestTotalWeightUnit

		public void TestTotalWeightUnit()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = address.Actions.AddNew();

			var originalUnit = PackingRegistry.Instance.WeightUnit.Value;
			try
			{
				PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);
				AssertEquals(action.TotalWeightUnit, Constants.Weight.Kilograms);

				PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Grams);
				AssertEquals(action.TotalWeightUnit, Constants.Weight.Grams);
			}
			finally
			{
				PackingRegistry.Instance.SetWeightUnitForTest(originalUnit);
			}
		}

		#endregion

		#region TestTotalVolumeUnit

		public void TestTotalVolumeUnit()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = address.Actions.AddNew();

			var originalUnit = PackingRegistry.Instance.VolumeUnit.Value;
			try
			{
				PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicMetres);
				AssertEquals(action.TotalVolumeUnit, Constants.Volume.CubicMetres);

				PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicInches);
				AssertEquals(action.TotalVolumeUnit, Constants.Volume.CubicInches);
			}
			finally
			{
				PackingRegistry.Instance.SetVolumeUnitForTest(originalUnit);
			}
		}

		#endregion

		#region TestTotalPackages

		public void TestTotalPackages()
		{
			AssertPackageTotalProperty<ZInt>(6, 10, a => a.TotalPackages);
		}

		#endregion

		#region TestTotalWeight

		public void TestTotalWeight()
		{
			AssertPackageTotalProperty<ZDecimal>(25m, 43m, a => a.TotalWeight);
		}

		#endregion

		#region TestTotalVolume

		public void TestTotalVolume()
		{
			AssertPackageTotalProperty<ZDecimal>(9.2m, 9m, a => a.TotalVolume);
		}

		#endregion

		#region AssertPackageTotalProperty

		void AssertPackageTotalProperty<T>(T expectedValueForConsignment1, T expectedValueForConsignment2, Func<DtbConsignmentAction, T> getValue, bool valueIsBlankForDelivery = false)
			where T : IZType
		{
			var booking = (DtbBooking)TransportBookingHelper.CreateBooking("B123");
			var package1 = booking.PackageJob.Packages.AddNew("PLT", 3);
			var package2 = booking.PackageJob.Packages.AddNew("CTN", 3);
			var package3 = booking.PackageJob.Packages.AddNew("CTN", 1);
			var package4 = booking.PackageJob.Packages.AddNew("PLT", 2);
			var instruction = TransportBookingHelper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			instruction.DivotsWithPackages.AddPackage(package1);
			instruction.DivotsWithPackages.AddPackage(package2);
			instruction.DivotsWithPackages.AddPackage(package3);
			instruction.DivotsWithPackages.AddPackage(package4);

			var pickupAddress = Helper.CreateOrganisation("PICKUP").MainAddress;
			pickupAddress.OA_Address1 = "1 Honda St";

			var deliverAddress = Helper.CreateOrganisation("DELIVERY").MainAddress;
			deliverAddress.OA_Address1 = "1 Shop St";

			// consignment1
			var consignment1 = Helper.CreateConsignment("LTC001");
			consignment1.LTC_JobType = "LTL";
			SetBookingIdToConsignment(booking, consignment1);
			var address1 = Helper.CreateConsignmentAddressWithAction(consignment1, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, pickupAddress);
			var pickupAction1 = Helper.CreateConsignmentAction(address1, ConsignmentAddressTypes.Codes.PickUp);
			var address2 = Helper.CreateConsignmentAddressWithAction(consignment1, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, deliverAddress);

			// consignment2
			var consignment2 = Helper.CreateConsignment("LTC002");
			consignment2.LTC_JobType = "LTL";
			var address3 = Helper.CreateConsignmentAddressWithAction(consignment2, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, pickupAddress);
			var address4 = Helper.CreateConsignmentAddressWithAction(consignment2, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.PickUp);
			address4.Address.E2_AddressOverride = true;
			address4.Address.E2_Address1 = "2 Shop St";

			// consignment3
			var consignment3 = Helper.CreateConsignment("LTC003");
			consignment3.LTC_JobType = "LTL";
			var address5 = Helper.CreateConsignmentAddressWithAction(consignment3, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			var poke = address5.Address;

			// consignment 1 Packages
			var outer1 = consignment1.PackageJob.Packages.AddNew("PLT", 2);
			var inner = outer1.Packages.AddNew("CTN", 10);
			var outer2 = consignment1.PackageJob.Packages.AddNew("PLT", 4);
			inner.KP_Weight = 10m;
			inner.KP_Volume = 3.4m;
			outer1.KP_Weight = 20m;
			outer1.KP_Volume = 2.2m;
			outer2.KP_Weight = 5m;
			outer2.KP_Volume = 7m;

			// consignment 2 Packages
			var outer3 = consignment2.PackageJob.Packages.AddNew("CTN", 5);
			outer3.KP_Weight = 0m;
			outer3.KP_Volume = 0m;
			Factory.Save();

			var outer4 = consignment2.PackageJob.Packages.AddNew("BOX", 5);
			outer4.KP_Weight = 43m;
			outer4.KP_Volume = 9m;

			// consignment 3 Packages
			var outer5 = consignment3.PackageJob.Packages.AddNew("KEG", 5);
			outer5.KP_Weight = 100m;
			outer5.KP_Volume = 100m;
			Factory.Save();

			Factory.InitialiseActionsTotalsCache("123", new DtbConsignmentAction[] { address1.Actions[0], address1.Actions[1], address2.Actions[0], address3.Actions[0], address4.Actions[0] });
			var action1 = address1.Actions[0];
			var action2 = address1.Actions[1];
			var action3 = address2.Actions[0];
			var action4 = address3.Actions[0];
			var action5 = address4.Actions[0];

			AssertEquals(expectedValueForConsignment1, getValue(action1));
			AssertEquals(expectedValueForConsignment1, getValue(action2));
			AssertEquals(valueIsBlankForDelivery ? default(T) : expectedValueForConsignment1, getValue(action3));
			AssertEquals(expectedValueForConsignment2, getValue(action4));
			AssertEquals(valueIsBlankForDelivery ? default(T) : expectedValueForConsignment2, getValue(action5));

			outer1.KP_Volume = 100;
			outer1.KP_Weight = 100;
			Factory.Save();
			AssertEquals("Value is cached", expectedValueForConsignment1, getValue(action1));
		}

		static void SetBookingIdToConsignment(Enterprise.Integration.TransportBooking.IDtbBooking booking, DtbConsignment consignment)
		{
			var reference = consignment.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.BookingJobId;
			reference.CE_EntryNum = booking.KM_JobID.ToString();
		}

		#endregion

		#region TestIConsignmentAction_Properties

		public void TestIConsignmentAction_Properties()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var action = address.Actions[0];

			var outer = consignment.PackageJob.Packages.AddNew("PLT", 2);
			var inner = outer.Packages.AddNew("CTN", 10);

			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			action.LTA_EstimatedTime = nowDTO.AddMinutes(1);
			action.LTA_RequiredFrom = nowDTO.AddMinutes(2);
			action.LTA_RequiredTo = nowDTO.AddMinutes(3);
			action.LTA_ReferenceNumber = "LTA1";

			AssertEquals(2, action.ActionQuantity);
			AssertEquals(address, action.ConsignmentAddress);
			AssertEquals(now.AddMinutes(1), action.Estimated);
			AssertEquals(now.AddMinutes(2), action.RequiredFrom);
			AssertEquals(now.AddMinutes(3), action.RequiredTo);
			AssertEquals(ActionTypes.Codes.PickUp, action.ActionType);
			AssertEquals("LTA1", action.ReferenceNumber);
			AssertEquals(false, action.IsEmptyContainer);
		}

		#endregion

		#region ConsignorOrConsigneeAddress

		public void TestConsignorOrConsigneeAddress_Consignor()
		{
			var consignor = CreateOrg("consignor", "Honda Motorcycles", "1/2", "Some Other Street", "Melbourne", "3039", "VIC");
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, consignor.MainAddress);
			var action = address.Actions[0];

			AssertEquals("Honda Motorcycles - Melbourne Victoria", ((IConsignmentAction)action).ConsignorOrConsigneeAddress);
		}

		public void TestConsignorOrConsigneeAddress_Consignee()
		{
			var consignee = CreateOrg("consignee", "Wisetech", "72", "O'Riordan Street", "Alexandria", "2015", "NSW");
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, consignee.MainAddress);
			var action = address.Actions[0];

			AssertEquals("Wisetech - Alexandria New South Wales", ((IConsignmentAction)action).ConsignorOrConsigneeAddress);
		}

		#endregion

		#endregion

		#region Action ID

		public void TestSave_ShouldSetActionID()
		{
			var consignmentAddress = Helper.CreateConsignmentAddress(ConsignmentAddressTypes.Codes.PickUp);
			var action1 = Helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
			var action2 = Helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);

			AssertEquals(string.Empty, action1.LTA_ActionID);
			AssertEquals(string.Empty, action2.LTA_ActionID);

			Factory.Save();

			AssertEquals("LTA00000001", action1.LTA_ActionID);
			AssertEquals("LTA00000002", action2.LTA_ActionID);
		}

		public void TestCodeProperty()
		{
			var action = (DtbConsignmentAction)GetNewBusinessObjectForDeleteTest(Factory);
			var codeProperty = action.GetAttribute<CodePropertyAttribute>();

			AssertNotNull(codeProperty);
			AssertEquals(nameof(action.LTA_ActionID), codeProperty.PropertyName);
			AssertEquals("LTA_ActionID", codeProperty.PropertyName);
		}

		#endregion

		#region IDocManagerSupport

		public void TestIDocManagerSupport_DocManagerInfo()
		{
			var action = Factory.New<DtbConsignmentAction>();
			var info = ((IDocManagerSupport)action).DocManagerInfo;

			AssertEquals("LTA", info.DocManagerCode);
			AssertEquals(action, info.BusinessEntity);
		}

		#endregion

		#region Implementations

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consignmentAddress = Helper.CreateConsignmentAddress(ConsignmentAddressTypes.Codes.PickUp);
			return Helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
		}

		#endregion

		#region Helpers

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		TransportBookingConsignmentTestHelper TransportBookingHelper
		{
			get { return transportBookingHelper ?? (transportBookingHelper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper transportBookingHelper;
		#endregion
	}
}
