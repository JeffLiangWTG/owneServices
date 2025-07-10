using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(DeclarationFromShipmentPullerDialog))]
	sealed class DeclarationFromShipmentPullerDialogTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new DeclarationFromShipmentPullerDialog(new Business.DeclarationFromShipmentPuller(Factory));
		}

		public void TestCreateButton_ClickSetsFlag()
		{
			using (DeclarationFromShipmentPullerDialog dialog = new DeclarationFromShipmentPullerDialog(new Business.DeclarationFromShipmentPuller(Factory)))
			{
				dialog.Show();
				AssertEquals("CreateClicked", false, dialog.CreateClicked);
				dialog.BusinessEntity.ValidateShipmentPK();
				dialog.CreateButton.PerformClick();
				AssertEquals("CreateClicked", false, dialog.CreateClicked);
				dialog.BusinessEntity.ShipmentPK = Factory.New<ForwardingShipment>().PK;
				dialog.CreateButton.PerformClick();
				AssertEquals("CreateClicked", true, dialog.CreateClicked);
			}
		}
	}
}
