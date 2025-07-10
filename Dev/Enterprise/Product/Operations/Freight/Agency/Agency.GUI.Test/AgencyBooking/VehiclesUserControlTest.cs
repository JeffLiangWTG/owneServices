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
	internal class VehiclesUserControlTest : BaseAgencyTest
	{
		public void TestHarmonisedCodeColumn()
		{
			var shipment = Factory.New<AgencyBooking>();
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

		public void TestOnAfterFirstBinding()
		{
			var shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.RollOnRollOff;
			shipment.ShippingContainers.AddNew();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.VehiclesGrid.Columns.Contains("UNDGs+UNDGSubstanceManagerGuid+Value"));
			}
		}

		public void TestShowWorkflowForm()
		{
			var shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.RollOnRollOff;
			shipment.ShippingContainers.AddNew();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				new AgencyContainerWorkflowFormHelperTest().AssertMenuItems(form.VehiclesGrid, shipment.ShippingContainers[0]);
			}
		}

		#region Types
		class FormForTesting : ZForm
		{
			public FormForTesting(AgencyBooking shipment) : base(shipment)
			{
				ControllerID = ControllerIDs.AgencyBooking;
				mainControl = new VehiclesUserControl();
				mainControl.Dock = DockStyle.Fill;
				Controls.Add(mainControl);
				mainControl.SetDataBinding(shipment, "");
			}

			public ZGrid VehiclesGrid
			{
				get
				{
					return (ZGrid)mainControl.Controls.Find("VehiclesGrid", true)[0];
				}
			}

			readonly VehiclesUserControl mainControl;
		}
		#endregion
	}
}
