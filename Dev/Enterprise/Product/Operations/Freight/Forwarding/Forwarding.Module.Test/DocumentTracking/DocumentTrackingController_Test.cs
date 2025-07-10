using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(DocumentTrackingController))]
	public class DocumentTrackingController_Test : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocumentTracking;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobRequiredDocument doc = shipment.DocsAndCartage.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.MasterBill);
			Factory.Save();
			return doc;
		}

		public void TestModuleID()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.DocumentTracking);
			AssertEquals(ModuleIDs.DocumentTracking, controller.ModuleID);
		}

		public void TestDocumentTrackingShouldHaveNewButtonDisabled()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			JobRequiredDocument doc = shipment.DocsAndCartage.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.MasterBill);

			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.DocumentTracking);
			using (IZForm form = controller.ShowEditForm(doc))
			{
				AssertEquals("Form should have NEW button disabled", ODisplayMode.NewSaved, ((ZForm)form).DisplayMode);
			}
		}
	}
}
