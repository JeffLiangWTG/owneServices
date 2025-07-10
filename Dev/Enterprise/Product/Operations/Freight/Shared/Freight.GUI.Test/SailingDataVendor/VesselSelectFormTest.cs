using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.GUI.Testing
{
	[TestedType(typeof(VesselSelectForm))]
	sealed class VesselSelectFormTest : ZFormBasherTest
	{
		public void TestOKButtonClick()
		{
			Form.Show();
			Application.DoEvents();
			Form.VesselsGrid.Select(0);
			AssertEquals("There should be 1 item selected for the test", 1, Form.VesselsGrid.SelectedElements.Length);

			Form.OKButton.PerformClick();
			AssertEquals("The selected vessel should be set", BusinessEntity.AvailableVessels[0], BusinessEntity.SelectedVessel);
			AssertEquals("The form should be closed after selecting a vessel", true, Form.IsDisposed);
		}

		public void TestOKButtonClick_WhenNoVesselSelected()
		{
			Form.Show();
			Application.DoEvents();
			AssertEquals("There should be no selected elements for the test", 0, Form.VesselsGrid.SelectedElements.Length);

			Form.OKButton.PerformClick();
			AssertEquals("Select a vessel from the list", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("After a warning is displayed, the form should remain open", false, Form.IsDisposed);
		}

		#region Test Classes

		class TestVesselSelectForm : VesselSelectForm
		{
			public TestVesselSelectForm(QueryUserSelectVesselFromLloydsNumber businessEntity)
				: base(businessEntity)
			{
			}

			public new ZGrid VesselsGrid
			{
				get { return base.VesselsGrid; }
			}

			public new ZButton OKButton
			{
				get { return base.OKButton; }
			}
		}

		#endregion

		#region Implementation

		RefVessel NewVessel(ZString vesselName, ZString lloydsNumber)
		{
			RefVessel result = Factory.New<RefVessel>();
			result.RV_Name = vesselName;
			result.RV_LloydsNumber = lloydsNumber;
			return result;
		}

		QueryUserSelectVesselFromLloydsNumber BusinessEntity
		{
			get
			{
				if (fBusinessEntity == null)
				{
					RefVessel ambiguousVessel1 = NewVessel("Different Name 1", "Lloyds");
					RefVessel ambiguousVessel2 = NewVessel("Different Name 2", "Lloyds");
					fBusinessEntity = new QueryUserSelectVesselFromLloydsNumber(Factory, "VesselName", "Lloyds");
				}
				return fBusinessEntity;
			}
		}
		QueryUserSelectVesselFromLloydsNumber fBusinessEntity;

		TestVesselSelectForm Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new TestVesselSelectForm(BusinessEntity);
				}
				return fForm;
			}
		}
		TestVesselSelectForm fForm;

		protected override Form GetFormToBashCore()
		{
			return new TestVesselSelectForm(BusinessEntity);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (fForm != null)
			{
				fForm.Dispose();
			}
		}

		#endregion
	}
}
