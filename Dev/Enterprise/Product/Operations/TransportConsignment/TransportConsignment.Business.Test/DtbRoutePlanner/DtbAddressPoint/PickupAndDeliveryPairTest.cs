namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class PickupAndDeliveryPairTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var pair = new PickupAndDeliveryPair(consignment.PickupInstruction.Address, consignment.DeliveryInstruction.Address);
			AssertEquals("Honda Motorcycles", pair.PickupAddress.Organisation.OH_FullName);
			AssertEquals("Geoff's House", pair.DeliveryAddress.Organisation.OH_FullName);
		}

		#endregion

		#region TestEquals_JDA_Different

		public void TestEquals_JDA_Different()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment1.PickupInstruction.Address.E2_AddressOverride = true;
			consignment1.DeliveryInstruction.Address.E2_AddressOverride = true;

			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses(); // Use different JDAs
			consignment2.PickupInstruction.Address.E2_AddressOverride = true;
			consignment2.DeliveryInstruction.Address.E2_AddressOverride = true;

			var pair1 = new PickupAndDeliveryPair(consignment1.PickupInstruction.Address, consignment1.DeliveryInstruction.Address);
			var pair2 = new PickupAndDeliveryPair(consignment2.DeliveryInstruction.Address, consignment2.PickupInstruction.Address);
			Assert("Precondition", pair1.PickupAddress.E2_AddressOverride && pair1.DeliveryAddress.E2_AddressOverride);
			Assert("Precondition", pair2.PickupAddress.E2_AddressOverride && pair2.DeliveryAddress.E2_AddressOverride);
			Assert("Pairs should not be equal.", !pair1.Equals(pair2));
			Assert("Pairs should not be equal.", !pair2.Equals(pair1));
			Assert("Pairs should have different hash codes - order matters.", !(pair1.GetHashCode().Equals(pair2.GetHashCode())));

			AssertEquals("Identical addresses should return the same key.", pair1.PickupKey, pair2.DeliveryKey);
			AssertEquals("Pair hash code should be composed of both pickup and delivery key for consolidation purposes.",
				pair1.GetHashCode(), (pair1.PickupKey + pair1.DeliveryKey).GetHashCode());
		}

		#endregion

		#region TestEquals_JDA_Different_SameOrganisation

		public void TestEquals_JDA_Different_SameOrganisation()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment1.PickupInstruction.Address.E2_AddressOverride = true;
			consignment1.DeliveryInstruction.Address.E2_AddressOverride = true;

			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses(); // Use different JDAs
			consignment2.PickupInstruction.Address.E2_AddressOverride = true;
			consignment2.DeliveryInstruction.Address.E2_AddressOverride = true;
			consignment2.DeliveryInstruction.Address.OrganisationPK = consignment2.PickupInstruction.Address.OrganisationPK;

			var pair1 = new PickupAndDeliveryPair(consignment1.PickupInstruction.Address, consignment1.DeliveryInstruction.Address);
			var pair2 = new PickupAndDeliveryPair(consignment2.PickupInstruction.Address, consignment2.DeliveryInstruction.Address);
			Assert("Precondition", pair1.PickupAddress.E2_AddressOverride && pair1.DeliveryAddress.E2_AddressOverride);
			Assert("Precondition", pair2.PickupAddress.E2_AddressOverride && pair2.DeliveryAddress.E2_AddressOverride);
			Assert("Pairs should not be equal.", pair1.Equals(pair2));
			Assert("Pairs should not be equal.", pair2.Equals(pair1));
			Assert("Pairs should not be equal.", pair1.GetHashCode().Equals(pair2.GetHashCode()));
			AssertNotEquals("Different addresses should *not* return the same key.", pair1.PickupKey, pair1.DeliveryKey);
			AssertNotEquals("Different addresses should *not* return the same key.", pair2.PickupKey, pair2.DeliveryKey);
			AssertEquals("Identical addresses should return the same key.", pair1.PickupKey, pair2.PickupKey);
			AssertEquals("Identical addresses should return the same key.", pair2.DeliveryKey, pair2.DeliveryKey);

			AssertEquals("Pair hash code should be composed of both pickup and delivery key for consolidation purposes.",
				pair1.GetHashCode(), (pair1.PickupKey + pair1.DeliveryKey).GetHashCode());
			AssertEquals("Pair hash code should be composed of both pickup and delivery key for consolidation purposes.",
				pair2.GetHashCode(), (pair2.PickupKey + pair2.DeliveryKey).GetHashCode());
		}

		#endregion

		#region TestEquals_JDA_Same

		public void TestEquals_JDA_Same()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment1.PickupInstruction.Address.E2_AddressOverride = true;
			consignment1.DeliveryInstruction.Address.E2_AddressOverride = true;

			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses(); // Use different JDAs
			consignment2.PickupInstruction.Address.E2_AddressOverride = true;
			consignment2.DeliveryInstruction.Address.E2_AddressOverride = true;

			var pair1 = new PickupAndDeliveryPair(consignment1.PickupInstruction.Address, consignment1.DeliveryInstruction.Address);
			var pair2 = new PickupAndDeliveryPair(consignment2.PickupInstruction.Address, consignment2.DeliveryInstruction.Address);
			Assert("Precondition", pair1.PickupAddress.E2_AddressOverride && pair1.DeliveryAddress.E2_AddressOverride);
			Assert("Precondition", pair2.PickupAddress.E2_AddressOverride && pair2.DeliveryAddress.E2_AddressOverride);
			Assert(pair1.Equals(pair2));
			Assert(pair2.Equals(pair1));
			Assert(pair1.GetHashCode().Equals(pair2.GetHashCode()));
		}

		#endregion

		#region TestEquals_OrgAddresses_Different

		public void TestEquals_OrgAddresses_Different()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var pair1 = new PickupAndDeliveryPair(consignment.PickupInstruction.Address, consignment.DeliveryInstruction.Address);
			var pair2 = new PickupAndDeliveryPair(consignment.DeliveryInstruction.Address, consignment.PickupInstruction.Address);
			Assert("Precondition", !pair1.PickupAddress.E2_AddressOverride && !pair1.DeliveryAddress.E2_AddressOverride);
			Assert("Precondition", !pair2.PickupAddress.E2_AddressOverride && !pair2.DeliveryAddress.E2_AddressOverride);
			Assert(!pair1.Equals(pair2));
			Assert(!pair2.Equals(pair1));
			Assert(!(pair1.GetHashCode().Equals(pair2.GetHashCode())));
		}

		#endregion

		#region TestEquals_OrgAddresses_Same

		public void TestEquals_OrgAddresses_Same()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var pair1 = new PickupAndDeliveryPair(consignment.PickupInstruction.Address, consignment.PickupInstruction.Address);
			var pair2 = new PickupAndDeliveryPair(consignment.PickupInstruction.Address, consignment.PickupInstruction.Address);
			Assert("Precondition", !pair1.PickupAddress.E2_AddressOverride && !pair1.DeliveryAddress.E2_AddressOverride);
			Assert("Precondition", !pair2.PickupAddress.E2_AddressOverride && !pair2.DeliveryAddress.E2_AddressOverride);
			Assert(pair1.Equals(pair2));
			Assert(pair2.Equals(pair1));
			Assert(pair1.GetHashCode().Equals(pair2.GetHashCode()));
		}

		#endregion

		#region TestEquals_OrgAddressNeverEqualsJDA

		public void TestEquals_OrgAddressNeverEqualsJDA()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment1.PickupInstruction.Address.E2_AddressOverride = true;
			consignment1.DeliveryInstruction.Address.E2_AddressOverride = true;

			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();

			var pair1 = new PickupAndDeliveryPair(consignment1.PickupInstruction.Address, consignment1.DeliveryInstruction.Address);
			var pair2 = new PickupAndDeliveryPair(consignment2.PickupInstruction.Address, consignment2.DeliveryInstruction.Address);
			Assert("Precondition", pair1.PickupAddress.E2_AddressOverride && pair1.DeliveryAddress.E2_AddressOverride);
			Assert("Precondition", !pair2.PickupAddress.E2_AddressOverride && !pair2.DeliveryAddress.E2_AddressOverride);
			Assert(!pair1.Equals(pair2));
			Assert(!pair2.Equals(pair1));
			Assert(!pair1.GetHashCode().Equals(pair2.GetHashCode()));
		}

		#endregion
	}
}
