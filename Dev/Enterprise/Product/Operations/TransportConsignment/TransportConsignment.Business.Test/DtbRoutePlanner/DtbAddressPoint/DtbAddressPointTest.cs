using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbAddressPoint))]
	sealed class DtbAddressPointTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		// calculated

		#region TestAddressCompanyName

		public void TestAddressCompanyName()
		{
			var org = Helper.CreateOrganisation("Honda");
			var addressPoint = new DtbAddressPoint(org.MainAddress);
			AssertEquals("Precondition", "", addressPoint.AddressCompanyName);
			org.OH_FullName = "Honda Motorcycles";
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "Honda Motorcycles", addressPoint.AddressCompanyName);
		}

		#endregion

		#region TestAddressLine1

		public void TestAddressLine1()
		{
			var org = Helper.CreateOrganisation("Honda");
			var addressPoint = new DtbAddressPoint(org.MainAddress);
			AssertEquals("Precondition", "Main st", addressPoint.AddressLine1);
			org.MainAddress.OA_Address1 = "Unit 3a 72 O'Riordan Street Alexandria";
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "Unit 3a 72 O'Riordan Street Alexandria", addressPoint.AddressLine1);
		}

		#endregion

		#region TestAddressCity

		public void TestAddressCity()
		{
			var org = Helper.CreateOrganisation("Honda");
			var addressPoint = new DtbAddressPoint(org.MainAddress);
			AssertEquals("Precondition", "", addressPoint.AddressCity);
			org.MainAddress.OA_City = "Sydney";
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "Sydney", addressPoint.AddressCity);
		}

		#endregion

		#region TestAddressState

		public void TestAddressState()
		{
			var org = Helper.CreateOrganisation("Honda");
			var addressPoint = new DtbAddressPoint(org.MainAddress);
			AssertEquals("Precondition", "", addressPoint.AddressState);
			org.MainAddress.OA_State = "NSW";
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "NSW", addressPoint.AddressState);
		}

		#endregion

		#region TestAddressPostCode

		public void TestAddressPostCode()
		{
			var org = Helper.CreateOrganisation("Honda");
			var addressPoint = new DtbAddressPoint(org.MainAddress);
			AssertEquals("Precondition", "", addressPoint.AddressPostCode);
			org.MainAddress.OA_PostCode = "2015";
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "2015", addressPoint.AddressPostCode);
		}

		#endregion

		#region TestDirectDeliveryAddressCompanyName

		public void TestDirectDeliveryAddressCompanyName()
		{
			AssertEquals("Should not blowup.", "", new DtbAddressPoint(Helper.CreateOrganisation("OtherOrg").MainAddress).DirectDeliveryAddressCompanyName);

			var org = Helper.CreateOrganisation("Honda");
			org.OH_FullName = "Honda Motorcycles";
			var addressPoint = new DtbAddressPoint(new PickupAndDeliveryPair(Helper.CreateOrganisation("OtherOrg").MainAddress, org.MainAddress));
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "Honda Motorcycles", addressPoint.DirectDeliveryAddressCompanyName);
		}

		#endregion

		#region TestDirectDeliveryAddressLine1

		public void TestDirectDeliveryAddressLine1()
		{
			AssertEquals("Should not blowup.", "", new DtbAddressPoint(Helper.CreateOrganisation("OtherOrg").MainAddress).DirectDeliveryAddressLine1);

			var org = Helper.CreateOrganisation("Honda");
			org.MainAddress.OA_Address1 = "Unit 3a 72 O'Riordan Street Alexandria";
			var addressPoint = new DtbAddressPoint(new PickupAndDeliveryPair(Helper.CreateOrganisation("OtherOrg").MainAddress, org.MainAddress));
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "Unit 3a 72 O'Riordan Street Alexandria", addressPoint.DirectDeliveryAddressLine1);
		}

		#endregion

		#region TestDirectDeliveryAddressCity

		public void TestDirectDeliveryAddressCity()
		{
			AssertEquals("Should not blowup.", "", new DtbAddressPoint(Helper.CreateOrganisation("OtherOrg").MainAddress).DirectDeliveryAddressCity);

			var org = Helper.CreateOrganisation("Honda");
			org.MainAddress.OA_City = "Sydney";
			var addressPoint = new DtbAddressPoint(new PickupAndDeliveryPair(Helper.CreateOrganisation("OtherOrg").MainAddress, org.MainAddress));
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "Sydney", addressPoint.DirectDeliveryAddressCity);
		}

		#endregion

		#region TestDirectDeliveryAddressState

		public void TestDirectDeliveryAddressState()
		{
			AssertEquals("Should not blowup.", "", new DtbAddressPoint(Helper.CreateOrganisation("OtherOrg").MainAddress).DirectDeliveryAddressState);

			var org = Helper.CreateOrganisation("Honda");
			org.MainAddress.OA_State = "NSW";
			var addressPoint = new DtbAddressPoint(new PickupAndDeliveryPair(Helper.CreateOrganisation("OtherOrg").MainAddress, org.MainAddress));
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "NSW", addressPoint.DirectDeliveryAddressState);
		}

		#endregion

		#region TestDirectDeliveryAddressPostCode

		public void TestDirectDeliveryAddressPostCode()
		{
			AssertEquals("Should not blowup.", "", new DtbAddressPoint(Helper.CreateOrganisation("OtherOrg").MainAddress).DirectDeliveryAddressState);

			var org = Helper.CreateOrganisation("Honda");
			org.MainAddress.OA_PostCode = "2015";
			var addressPoint = new DtbAddressPoint(new PickupAndDeliveryPair(Helper.CreateOrganisation("OtherOrg").MainAddress, org.MainAddress));
			AssertEquals("Properties proxied from address so grid colours can be configured. DONT DELETE!", "2015", addressPoint.DirectDeliveryAddressPostCode);
		}

		#endregion

		#region TestAddressPK

		public void TestAddressPK()
		{
			var org = Helper.CreateOrganisation("Honda");
			var consignment = Helper.CreateBookingConsignment();
			var instruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.PickUp, org.MainAddress);
			AssertEquals("AddressPK should return OA PK.", org.MainAddress.PK, new DtbAddressPoint(org.MainAddress).AddressPK);
			AssertEquals("AddressPK should return JDA PK.", instruction.Address.PK, new DtbAddressPoint(instruction.Address).AddressPK);
		}

		#endregion

		#region TestDirectDeliveryAddressPoint

		public void TestDirectDeliveryAddressPoint_WithJobDocAddressConstructor()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();

			var pickupAddressPoint = new DtbAddressPoint(new PickupAndDeliveryPair(consignment.PickupInstruction.Address, consignment.DeliveryInstruction.Address));
			AssertEquals("Honda Motorcycles", pickupAddressPoint.Address.Organisation.OH_FullName);
			AssertEquals("Geoff's House", pickupAddressPoint.DirectDeliveryAddress.Organisation.OH_FullName);
		}

		public void TestDirectDeliveryAddressPoint_WithOrgAddressConstructor()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();

			var pickupAddressPoint = new DtbAddressPoint(new PickupAndDeliveryPair(consignment.PickupInstruction.Address.Address, consignment.DeliveryInstruction.Address));
			AssertEquals("Honda Motorcycles", pickupAddressPoint.Address.Organisation.OH_FullName);
			AssertEquals("Geoff's House", pickupAddressPoint.DirectDeliveryAddress.Organisation.OH_FullName);
		}

		#endregion

		// calculated -- totals

		#region TestTotalWeightUnit

		public void TestTotalWeightUnit()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var addressPoint = Helper.CreateAddressPointForPickupConfirmation(consignment);

			var originalUnit = PackingRegistry.Instance.WeightUnit.Value;
			try
			{
				PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);
				AssertEquals(addressPoint.TotalWeightUnit, Constants.Weight.Kilograms);

				PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Grams);
				AssertEquals(addressPoint.TotalWeightUnit, Constants.Weight.Grams);
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
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var addressPoint = Helper.CreateAddressPointForPickupConfirmation(consignment);

			var originalUnit = PackingRegistry.Instance.VolumeUnit.Value;
			try
			{
				PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicMetres);
				AssertEquals(addressPoint.TotalVolumeUnit, Constants.Volume.CubicMetres);

				PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicInches);
				AssertEquals(addressPoint.TotalVolumeUnit, Constants.Volume.CubicInches);
			}
			finally
			{
				PackingRegistry.Instance.SetVolumeUnitForTest(originalUnit);
			}
		}

		#endregion

		#endregion

		#region TestSchema

		public void TestSchema()
		{
			AssertEquals(DtbBookingConfirmationSchema.Constants.TableName, DtbAddressPoint.Schema.TableName);
		}

		#endregion

		#region TestLookups

		public void TestLookups()
		{
			AssertEquals(typeof(DtbAddressPointLookups), GetNewAddressPoint().Lookups.GetType());
		}

		#endregion

		#region Implementation

		DtbAddressPoint GetNewAddressPoint()
		{
			return (DtbAddressPoint)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var consignment = Helper.CreateBookingConsignment();
			var instruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.PickUp);
			return new DtbAddressPoint(instruction.Address);
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
