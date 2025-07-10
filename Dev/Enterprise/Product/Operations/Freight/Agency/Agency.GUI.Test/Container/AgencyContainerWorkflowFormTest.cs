using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(AgencyContainerWorkflowForm))]
	internal class AgencyContainerWorkflowFormTest : ZFormBasherTest
	{
		#region TestControllerID
		public void TestControllerID()
		{
			using (var form = new AgencyContainerWorkflowFormForTest(Factory.New<AgencyShipmentContainer>()))
			{
				AssertEquals(ControllerIDs.AgencyBooking, form.ControllerID);
			}
		}

		#endregion
		#region TestFormCaption
		public void TestFormCaption()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerNum = "FAKE1234560";
			using (var form = new AgencyContainerWorkflowFormForTest(container))
			{
				AssertEquals("FAKE1234560 Workflow / eDocs", form.FormCaption);
			}
		}

		#endregion
		#region TestTabPages
		public void TestTabPages()
		{
			using (AgencyContainerWorkflowFormForTest form = new AgencyContainerWorkflowFormForTest(Factory.New<AgencyShipmentContainer>()))
			{
				AssertEquals(false, form.ShowNotesTabForTest);
				AssertEquals(false, form.MainTabControlForTest.TabPages.Contains(form.MainTabPageForTest));
			}
		}

		#endregion
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new AgencyContainerWorkflowForm(Factory.New<AgencyShipmentContainer>());
		}
		#endregion
	}
}
