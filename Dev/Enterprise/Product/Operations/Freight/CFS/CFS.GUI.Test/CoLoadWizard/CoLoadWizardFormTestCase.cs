using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.GUI
{
	public class CoLoadWizardFormTestCase : TestCaseWithFactory
	{
		public void TestButtonEnables()
		{
			using (CoLoadWizardForm testForm = GetTestForm())
			{
				testForm.Show();
				AssertEquals("Back button should not be enabled at first step", false, testForm.BackButtonInternal.Enabled);
				AssertEquals("Next button should be enabled", true, testForm.NextButtonInternal.Enabled);
				AssertEquals("Visible", true, testForm.Visible);
			}
		}
		#region Implementation
		CoLoadWizardForm GetTestForm()
		{
			return new CoLoadWizardForm(CoLoadWizardShipmentBizO());
		}
		CoLoadWizardShipment CoLoadWizardShipmentBizO()
		{
			PackUnpackShipment testCoLoadShipment = Factory.New<PackUnpackShipment>();
			testCoLoadShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			testCoLoadShipment.ConsigneePK = GetCoLoadForwarder();
			CoLoadWizardShipment result = new CoLoadWizardShipment(testCoLoadShipment);
			return result;
		}
		ZGuid GetCoLoadForwarder()
		{
			ZQuery coLoadFilter = new ZQuery(OrgHeaderSchema.OH_IsForwarder, ZBool.True);
			var result = Factory.LoadTop1<OrgHeader>(coLoadFilter);
			return result.PK;
		}

		#endregion

	}
}

namespace Enterprise.Freight.CFS.GUI
{
	using Enterprise.ZArchitecture.GUI.Testing;
	using NUnit.Framework;

	[TestedType(typeof(CoLoadWizardForm))]
	public class CoLoadWizardFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			CoLoadWizardForm result = new CoLoadWizardForm(CoLoadWizardShipmentBizO());
			return result;
		}

		protected CoLoadWizardShipment CoLoadWizardShipmentBizO()
		{
			PackUnpackShipment testCoLoadShipment = Factory.New<PackUnpackShipment>();
			testCoLoadShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			testCoLoadShipment.ConsigneePK = GetCoLoadForwarder();
			CoLoadWizardShipment result = new CoLoadWizardShipment(testCoLoadShipment);
			return result;
		}

		protected ZGuid GetCoLoadForwarder()
		{
			ZQuery coLoadFilter = new ZQuery(OrgHeaderSchema.OH_IsForwarder, ZBool.True);
			var result = Factory.LoadTop1<OrgHeader>(coLoadFilter);
			return result.PK;
		}

		#endregion

	}
}
