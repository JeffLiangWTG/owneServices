using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ModeAndPartyControlJobTest : ShipmentBasicRegistrationControlJobTestBase
	{
		protected override void TestAutoCreateRegistryIsOn(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment)
		{
			var modeAndPartyControl = form.ShipmentBasicRegistrationControl.GetModeAndPartyControl();
			Assert(modeAndPartyControl.LocalClientOrgControl.Visible);
			Assert(!modeAndPartyControl.JobHeaderClientCoveringLabel.Visible);
		}

		protected override void TestJobDeleted(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment)
		{
			var modeAndPartyControl = form.ShipmentBasicRegistrationControl.GetModeAndPartyControl();
			AssertEquals(false, modeAndPartyControl.LocalClientOrgControl.Visible);
			AssertEquals(true, modeAndPartyControl.JobHeaderClientCoveringLabel.Visible);
			AssertEquals("Billing job is deleted.", modeAndPartyControl.JobHeaderClientCoveringLabel.Text);
		}

		protected override void TestJobRecreated(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment)
		{
			var modeAndPartyControl = form.ShipmentBasicRegistrationControl.GetModeAndPartyControl();
			AssertEquals(true, modeAndPartyControl.LocalClientOrgControl.Visible);
			AssertEquals(false, modeAndPartyControl.JobHeaderClientCoveringLabel.Visible);
		}

		protected override void TestAutoCreateRegistryIsOff(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment)
		{
			var modeAndPartyControl = form.ShipmentBasicRegistrationControl.GetModeAndPartyControl();
			AssertNull(shipment.ShipmentJobHeader);
			Assert(!modeAndPartyControl.LocalClientOrgControl.Visible);
			Assert(modeAndPartyControl.JobHeaderClientCoveringLabel.Visible);
			AssertEquals(form.ShipmentBasicRegistrationControl.JobHandler.InitializationMessage, modeAndPartyControl.JobHeaderClientCoveringLabel.Text);
		}

		protected override void TestJobExistsAndAutoCreateRegistryIsOff(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment)
		{
			var modeAndPartyControl = form.ShipmentBasicRegistrationControl.GetModeAndPartyControl();
			AssertEquals(true, modeAndPartyControl.LocalClientOrgControl.Visible);
			AssertEquals(false, modeAndPartyControl.JobHeaderClientCoveringLabel.Visible);
		}

		[RequiresSTA]
		public void TestUpdateLocalClient()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			AssertNull(shipment.ShipmentJobHeader);

			shipment.JS_RL_NKOrigin = "SAABQ";

			var localClient = factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LocalClient1";

			GlbCompany saCompany = Factory.New<GlbCompany>();
			saCompany.GC_Code = "DSA";
			GlbBranch saBranch = saCompany.Branches.AddNew();

			var loader = new JobHeader.Loader(shipment);
			using (var job = loader.TryCreate(saBranch))
			{
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
				using (var form = new ShipmentFormWithBasicRegistration(shipment))
				{
					form.Show();

					shipment.ShipmentJobHeader.Delete();

					loader.Load(true, saCompany);

					Assert(!shipment.IsShipmentJobHeaderForCurrentCompany);

					var modeAndPartyControl = form.ShipmentBasicRegistrationControl.GetModeAndPartyControl();
					var bindingDataItem = (ZAddressWithContact)modeAndPartyControl.LocalClientOrgControl.CurrentDataItem;

					AssertNull(bindingDataItem.OrgHeader);
				}
			}
		}
	}
}
