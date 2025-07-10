using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.PlugIn.Testing
{
	sealed class CustomsPlugInTest : TestCaseWithFactory
	{
		public void TestFormPreSaved()
		{
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			shipment.JS_HouseBill = "TEST001";
			using (var form = new ZForm(shipment))
			{
				using (var testPlugIn = new CustomsPlugInForTest(shipment, form))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					Assert("Save successfully", CustomsPlugIn.FormPreSaved(shipment, testPlugIn.GetForm()));
					Assert("shipment is saved.", shipment.IsInDatabase);
				}

				var shipment2 = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
				shipment2.JS_HouseBill = "TEST001";
				using (var testPlugIn2 = new CustomsPlugInForTest(shipment2, form))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					Assert("Save cancelled", !CustomsPlugIn.FormPreSaved(shipment2, testPlugIn2.GetForm()));
					Assert("shipment2 is NOT saved.", !shipment2.IsInDatabase);
				}

				var shipment3 = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
				Assert("Precondition: HasChanges is false", !shipment3.HasChanges);
				using (var testPlugIn3 = new CustomsPlugInForTest(shipment3, form))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					Assert("Save successfully", CustomsPlugIn.FormPreSaved(shipment3, testPlugIn3.GetForm()));
					Assert("A new shipment has no changes should be saved.", shipment3.IsInDatabase);
				}
			}
		}

		public void TestChangeTheVisibilityDoNotCalledRecursively()
		{
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			shipment.JS_HouseBill = "TEST001";
			using (var form = new ZForm(shipment))
			{
				using (var testPlugIn = new CustomsPlugInForTest(shipment, form))
				{
					testPlugIn.ChangeCount = 0;
					testPlugIn.OnChangeTheVisibilityRequiredForTest();
					AssertEquals("ChangeTheVisibilityCore should be called 1 times", 1, testPlugIn.ChangeCount);
				}
			}
		}
	}

	sealed class CustomsPlugInForTest : CustomsPlugIn
	{
		public CustomsPlugInForTest(BusinessObject hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		public CustomsPlugInForTest(BusinessObject hostBusinessEntity, ZForm form)
			: this(hostBusinessEntity)
		{
			fForm = form;
		}

		public ZForm GetForm() => base.Form;

		protected override void ChangeTheVisibilityCore()
		{
			ChangeCount++;
			ChangeTheVisibility();
		}

		public int ChangeCount { get; set; }

		public void OnChangeTheVisibilityRequiredForTest()
		{
			OnChangeTheVisibilityRequired(null, null);
		}

		public override string Name => "CustomsPlugIn Test BO";
		protected override ZBool HasUserControl => false;
		protected override LicenceCheckpoint LicenceCheckPoint => (LicenceCheckpoint)EnvProxy.Instance.Licence.AlwaysAllow;
	}
}
