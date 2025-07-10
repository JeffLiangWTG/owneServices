using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	class DtbInstructionTileTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestZoneLabelHasCorrectPositionAndText

		public void TestZoneLabelHasCorrectPosition()
		{
			var zone1 = Helper.CreateZone("Zone 1");
			var zone2 = Helper.CreateZone("A Really Long Zone Name Must Be Cut");
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			consignment.PickupInstruction.KN_TZ_DomesticZone = zone1.PK;
			consignment.DeliveryInstruction.KN_TZ_DomesticZone = zone2.PK;

			using (var form = new ZForm(consignment))
			{
				var instructionTile1 = new TestDtbInstructionTile();
				var instructionTile2 = new TestDtbInstructionTile();
				form.Controls.Add(instructionTile1);
				form.Controls.Add(instructionTile2);
				instructionTile1.CaptionResourceString = Res.GetData("TestOnly", "Pickup");
				instructionTile2.CaptionResourceString = Res.GetData("TestOnly", "Delivery");
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(instructionTile1, "PickUpInstruction");
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(instructionTile2, "DeliveryInstruction");
				instructionTile1.BindToOrganisations = "PickupInstruction.Lookups.ConsignorOrganisations";
				instructionTile2.BindToOrganisations = "DeliveryInstruction.Lookups.ConsigneeOrganisations";
				form.Show();

				var widthT1 = instructionTile1.ZoneLabel.Width;
				var locationT1 = instructionTile1.ZoneLabel.Location.X;
				var widthT2 = instructionTile2.ZoneLabel.Width;
				var locationT2 = instructionTile2.ZoneLabel.Location.X;

				AssertEquals("Zone Label should not overlap the Groupbox Caption", true, locationT1 >= ControlDpiScalingHelper.ScaleToCurrentDpiX(70));
				AssertEquals("Zone Label should not go outside the border of the Groupbox", true, (widthT1 + locationT1) < instructionTile1.DocAddressControl.ControlWidth);
				AssertEquals("Zone Label should not overlap the Groupbox Caption", true, locationT2 >= ControlDpiScalingHelper.ScaleToCurrentDpiX(90));
				AssertEquals("Zone Label should not go outside the border of the Groupbox", true, (widthT2 + locationT2) < instructionTile2.DocAddressControl.ControlWidth);
			}
		}

		public void TestZoneLabelHasCorrectText()
		{
			var zone1 = Helper.CreateZone("Zone 1");
			var zone2 = Helper.CreateZone("A Really Long Zone Name Must Be Cut");
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			consignment.PickupInstruction.KN_TZ_DomesticZone = zone1.PK;
			consignment.DeliveryInstruction.KN_TZ_DomesticZone = zone2.PK;

			using (var form = new ZForm(consignment))
			{
				var instructionTile1 = new TestDtbInstructionTile();
				var instructionTile2 = new TestDtbInstructionTile();
				form.Controls.Add(instructionTile1);
				form.Controls.Add(instructionTile2);
				instructionTile1.CaptionResourceString = Res.GetData("TestOnly", "Pickup");
				instructionTile2.CaptionResourceString = Res.GetData("TestOnly", "Delivery");
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(instructionTile1, "PickUpInstruction");
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(instructionTile2, "DeliveryInstruction");
				instructionTile1.BindToOrganisations = "PickupInstruction.Lookups.ConsignorOrganisations";
				instructionTile2.BindToOrganisations = "DeliveryInstruction.Lookups.ConsigneeOrganisations";
				form.Show();

				var zone2notTruncated = consignment.DeliveryInstruction.ZoneDescription == instructionTile2.ZoneLabel.Text;
				var trimmedZone2Label = instructionTile2.ZoneLabel.Text.TrimEnd(')', '.');
				var zone2TruncatedCorrectly = consignment.DeliveryInstruction.ZoneDescription.StartsWith(trimmedZone2Label);

				AssertEquals("The text 'Zone' should have been added as a prefix to the zone's name", consignment.PickupInstruction.ZoneDescription, instructionTile1.ZoneLabel.Text);
				AssertEquals("Zone Label should have been truncated if it cannot fit within the Groupbox", true, zone2notTruncated || zone2TruncatedCorrectly);
			}
		}

		#endregion

		#region TestBindToOrganisations

		public void TestBindToOrganisations()
		{
			using (var control = new DtbInstructionTile())
			{
				AssertEquals("Precondition", "", control.BindToOrganisations);
				AssertEquals("Precondition", "", control.DocAddressControl.BindToOrganisations);

				control.BindToOrganisations = "Orgs";
				AssertEquals("Orgs", control.BindToOrganisations);
				AssertEquals("Orgs", control.DocAddressControl.BindToOrganisations);
			}
		}

		#endregion

		#region TestCaptionResourceString

		public void TestCaptionResourceString()
		{
			using (var control = new DtbInstructionTile())
			{
				AssertEquals("Precondition", ResourceStringData.Empty, control.CaptionResourceString);
				AssertEquals("Precondition", ResourceStringData.Empty, control.DocAddressControl.CaptionResourceString);

				var data = Res.GetData("blah", "blah");
				control.CaptionResourceString = data;
				AssertEquals(data, control.CaptionResourceString);
				AssertEquals(data, control.DocAddressControl.CaptionResourceString);
			}
		}

		#endregion

		#region Implementation

		class TestDtbInstructionTile : DtbInstructionTile
		{
			public new ZLabel ZoneLabel
			{
				get { return base.ZoneLabel; }
			}
		}

		#endregion
	}
}
