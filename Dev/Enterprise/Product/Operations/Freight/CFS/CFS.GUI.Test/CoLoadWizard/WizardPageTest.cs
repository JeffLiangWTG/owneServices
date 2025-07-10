using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	public abstract class WizardPageTest : TestCaseWithFactory
	{
		public void TestWizardPages()
		{
			using (CoLoadWizardForm testForm = new CoLoadWizardForm(CoLoadWizardShipmentBizO()))
			{
				testForm.Show();
				bool thisWizardFound = false;
				for (int i = 0; i < testForm.LastWizardPage + 1; i++)
				{
					testForm.CurrentWizardPageStep = GetExpectedWizardStep();
					if (testForm.CurrentWizardPage.GetType() == GetExpectedWizardType())
					{
						thisWizardFound = true;
					}
				}
				AssertEquals("Wizard Page not found in wizard", true, thisWizardFound);
			}
		}

		protected abstract Type GetExpectedWizardType();
		protected abstract CoLoadWizardSteps GetExpectedWizardStep();

		#region Implementation

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
