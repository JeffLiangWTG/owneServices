using System.Windows.Forms;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BillOfLadingVehiclesPageTest : BaseAgencyTest
	{
		public void TestHarmonisedCodeColumn()
		{
			var shipment = Factory.New<BillOfLading>();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				var harmonisedCodeColumnInfo = (TariffColumnStyleInfo)form.VehiclesGrid.GetColumnStyle(AutoJobContainer.Schema.JC_HarmonisedCode);
				AssertNotNull(harmonisedCodeColumnInfo);
				AssertNull(harmonisedCodeColumnInfo.GetCountryCode?.Invoke());
				AssertEquals("WCO", harmonisedCodeColumnInfo.GetDataGrouping());
				AssertEquals("HSN", harmonisedCodeColumnInfo.TariffType);
			}
		}

		public void TestAdditionalColumns_UNDG()
		{
			var shipment = Factory.New<BillOfLading>();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.VehiclesGrid.Columns.Contains("UNDGs+UNDGSubstanceManagerGuid+Value"));
			}
		}

		public void TestShowWorkflowForm()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.RollOnRollOff;
			shipment.Vehicles.AddNew();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				new AgencyContainerWorkflowFormHelperTest().AssertMenuItems(form.VehiclesGrid, shipment.Vehicles[0]);
			}
		}

		#region Implementation
		class FormForTesting : ZForm
		{
			public FormForTesting(BillOfLading billOfLading) : base(billOfLading)
			{
				ControllerID = ControllerIDs.AgencyBillOfLading;
				mainControl = new BillOfLadingVehiclesPage();
				mainControl.Dock = DockStyle.Fill;
				Controls.Add(mainControl);
				mainControl.SetDataBinding(billOfLading, "");
			}

			public ZGrid VehiclesGrid
			{
				get
				{
					return (ZGrid)mainControl.Controls.Find("VehiclesGrid", true)[0];
				}
			}

			readonly BillOfLadingVehiclesPage mainControl;
		}
		#endregion
	}
}
