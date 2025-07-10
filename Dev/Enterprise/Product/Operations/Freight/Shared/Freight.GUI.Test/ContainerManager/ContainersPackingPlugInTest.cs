using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	public abstract class ContainersPackingPlugInTest : BaseFreightTest
	{
		#region Enterprise Incident Tests

		[RequiresSTA]
		public void TestPackClickI00015340()
		{
			var consol = GetConsolToPack();
			using (var form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.Select(0);
				AssertEquals("Failed To Selected Row on Containers Grid", 1, form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.SelectedRowCount);
				AssertEquals("UnAllocatedPackLinesGrid Selected Count", 0, form.ContainersPackingPlugIn.PackingDetailsUserControl.UnAllocatedPackLinesGrid.SelectedRowCount);
				AssertEquals("Un allocation Pack lines Consol Count", 1, consol.UnAllocatedPackLines.Count);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.UnAllocatedPackLinesGrid.Select(0);
				form.ExposePack();
				AssertEquals("Depots Like to be able to pack containers I00015340", 1, consol.Containers[0].PackLines.Count);
			}
		}

		public void TestPackClick_WithOverrideWarning()
		{
			var consol = GetConsolToPackWithOverrideContainerWeight();
			using (var form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.Select(0);

				AssertEquals("Failed To Selected Row on Containers Grid", 1, form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.SelectedRowCount);
				AssertEquals("UnAllocatedPackLinesGrid Selected Count", 0, form.ContainersPackingPlugIn.PackingDetailsUserControl.UnAllocatedPackLinesGrid.SelectedRowCount);
				AssertEquals("Un allocation Pack lines Consol Count", 1, consol.UnAllocatedPackLines.Count);

				form.ContainersPackingPlugIn.PackingDetailsUserControl.UnAllocatedPackLinesGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.ExposePack();

				AssertEquals("Attaching Shipment s1. Gross Weight of Container JFDU8392839 is overridden. Would you like to retain the Overridden Gross Weight?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Depots Like to be able to pack containers I00015340", 1, consol.Containers[0].PackLines.Count);
			}
		}

		#endregion

		#region Selection From Grid

		public void TestSelectedContainersFormContainerGrid1Container()
		{
			var consol = GetConsolToUnpack();
			using (var form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();
				AssertEquals("If only 1 container, it should always be the selected container be default", consol.Containers[0], form.ContainersPackingPlugIn.GetSelectedContainers()[0]);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.Select(0);
				AssertEquals("If only 1 container, it should always be the selected container if selected", consol.Containers[0], form.ContainersPackingPlugIn.GetSelectedContainers()[0]);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.UnSelect(0);
				AssertEquals("If only 1 container, it should always be the selected container even if unselected", consol.Containers[0], form.ContainersPackingPlugIn.GetSelectedContainers()[0]);
			}
		}

		public void TestSelectedContainersFromContainerGridMultipleContainer()
		{
			var consol = GetConsolToUnpack();
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "FFDU3928392";

			using (var form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.Select(0);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.Select(1);
				AssertEquals("2 containers should be returned", 2, form.ContainersPackingPlugIn.GetSelectedContainers().Length);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.UnSelect(0);
				AssertEquals("1 container should be returned", 1, form.ContainersPackingPlugIn.GetSelectedContainers().Length);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.UnSelect(1);
				AssertEquals("0 containers should be returned", 0, form.ContainersPackingPlugIn.GetSelectedContainers().Length);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.Select(0);
				AssertEquals("First Container Should Be Selected", consol.Containers[0], form.ContainersPackingPlugIn.GetSelectedContainers()[0]);
				Assert("HouseBill column should exist on UnAllocated Grid", form.ContainersPackingPlugIn.PackingDetailsUserControl.UnAllocatedPackLinesGrid.Columns.Contains(PackLine.Schema.JL_JS_HouseBill));
				Assert("HouseBill column should exist on Packed Containers Grid", form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainerDetailsGrid.Columns.Contains(PackLine.Schema.JL_JS_HouseBill));
			}
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestUnpackAllWithNoContainer_I3469()
		{
			var consol = GetConsolToUnpack();
			using (var form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();
				form.ExposeUnpack();
			}
		}

		#region TestUnpackFromContainerWarning

		public void TestUnpackFromContainerWarning()
		{
			var consol = GetConsolToUnpack();
			AssertEquals("Precondition - consol should have 1 container", 1, consol.Containers.Count);
			JobService fumigation = consol.Containers[0].Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;

			using (var form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();

				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainerDetailsGrid.Select(0);
				AssertEquals("UnAllocatedPackLinesGrid Selected Count", 1, form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainerDetailsGrid.SelectedRowCount);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainerDetailsGrid.ContextMenu.MenuItems[0].PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Warning The services on the selected shipments will be reset.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		#endregion

		#region TestElementNotInCollectionIssueOnUnPackClick

		[ExpectNoExceptions]
		public void TestElementNotInCollectionIssueOnUnPackClick()
		{
			var consol1 = CreateConsol();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.AutomaticallyUpdatePackLineContainers = true;

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "AKHL8392839";
			container1.JC_RC = RC_40GP_PK;

			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "HJKP0392839";
			container2.JC_RC = RC_40GP_PK;

			var container3 = consol1.Containers.AddNew();
			container3.JC_ContainerNum = "KLJH09872839";
			container3.JC_RC = RC_40GP_PK;

			CommonShipment shipment = consol1.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 25;

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 12;
			packLine1.JL_ActualVolume = 1m;
			packLine1.JL_ActualWeight = 1m;
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 13;
			packLine2.JL_ActualVolume = 2m;
			packLine2.JL_ActualWeight = 2m;

			packLine1.SetContainer(container1.PK);
			packLine2.SetContainer(container2.PK);

			Factory.Save();
			using (var form = CreateForm(consol1))
			{
				form.Show();
				form.SelectContainersTab();
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.Select(1);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.Select(2);
				form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainerDetailsGrid.Select(0);
				form.ExposeUnpack();
			}
		}

		#endregion

		#region ExportReferenceNumber HeaderText

		[RequiresSTA]
		public void TestExportReferenceNumberHeaderText()
		{
			foreach (var countryCode in new[]
			{
				Core.Constants.CountryCodes.China,
				Core.Constants.CountryCodes.Taiwan,
				Core.Constants.CountryCodes.HongKong
			})
			{
				AssertExportReferenceNumberHeaderText(countryCode, Constants.TransportModes.Sea, "Shipping Order/Shi Lian Dan");
				AssertExportReferenceNumberHeaderText(countryCode, Constants.TransportModes.Air, "Export Reference Number");
			}

			foreach (var countryCode in new[]
			{
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.UnitedStates
			})
			{
				AssertExportReferenceNumberHeaderText(countryCode, Constants.TransportModes.Sea, "Export Reference Number");
				AssertExportReferenceNumberHeaderText(countryCode, Constants.TransportModes.Air, "Export Reference Number");
			}
		}

		protected void AssertExportReferenceNumberHeaderText(string countryCode, string transportMode, string expectHeaderText)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var consol = GetConsolToUnpack();
				consol.JK_TransportMode = transportMode;
				consol.JK_RL_NKLoadPort = "CNAAT";

				using (var form = CreateForm(consol))
				{
					form.Show();
					form.SelectContainersTab();

					var exportRefNumberCol = form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainerDetailsGrid.Columns[PackLine.Schema.JL_ExportRefNumber];
					AssertNotNull(exportRefNumberCol);
					AssertEquals(expectHeaderText, exportRefNumberCol.ColumnStyle.HeaderText);

					consol.JK_RL_NKLoadPort = "JPTYO";
					AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

					consol.JK_RL_NKLoadPort = "TWACH";
					AssertEquals(expectHeaderText, exportRefNumberCol.ColumnStyle.HeaderText);

					consol.JK_RL_NKLoadPort = "AUBNE";
					AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

					consol.JK_RL_NKLoadPort = "HKHKG";
					AssertEquals(expectHeaderText, exportRefNumberCol.ColumnStyle.HeaderText);
				}
			}
		}

		[RequiresSTA]
		public void TestExportReferenceNumberHeaderText_WhenTransportModeChanged()
		{
			foreach (var countryCode in new[]
			{
				Core.Constants.CountryCodes.China,
				Core.Constants.CountryCodes.Taiwan,
				Core.Constants.CountryCodes.HongKong
			})
			{
				AssertExportReferenceNumberHeaderText(countryCode,
					Constants.TransportModes.Sea, "Shipping Order/Shi Lian Dan",
					Constants.TransportModes.Air, "Export Reference Number");
			}

			foreach (var countryCode in new[]
			{
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.UnitedStates
			})
			{
				AssertExportReferenceNumberHeaderText(countryCode,
					Constants.TransportModes.Sea, "Export Reference Number",
					Constants.TransportModes.Air, "Export Reference Number");
			}
		}

		protected void AssertExportReferenceNumberHeaderText(string countryCode,
			string transportMode1, string expectHeaderText1,
			string transportMode2, string expectHeaderText2)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var consol = GetConsolToUnpack();
				consol.JK_TransportMode = transportMode1;
				consol.JK_RL_NKLoadPort = "CNAAT";

				using (var form = CreateForm(consol))
				{
					form.Show();
					form.SelectContainersTab();

					var exportRefNumberCol = form.ContainersPackingPlugIn.PackingDetailsUserControl.ContainerDetailsGrid.Columns[PackLine.Schema.JL_ExportRefNumber];
					AssertNotNull(exportRefNumberCol);
					AssertEquals(expectHeaderText1, exportRefNumberCol.ColumnStyle.HeaderText);

					consol.JK_TransportMode = transportMode2;
					AssertEquals(expectHeaderText2, exportRefNumberCol.ColumnStyle.HeaderText);

					consol.JK_TransportMode = transportMode1;
					AssertEquals(expectHeaderText1, exportRefNumberCol.ColumnStyle.HeaderText);
				}
			}
		}

		#endregion

		protected CommonConsol GetConsolToPack()
		{
			var result = CreateConsol();
			result.JK_TransportMode = Constants.TransportModes.Sea;
			result.Transports[0].JW_JX = ExportSailing1.PK;
			result.AutomaticallyUpdatePackLineContainers = false;

			var container = result.Containers.AddNew();
			container.JC_ContainerNum = "JFDU8392839";
			container.JC_RC = RC_40GP_PK;

			CommonShipment shipment = result.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 10;
			AssertEquals("Consol 1 Shipment Unpacked", 1, shipment.OuterPackLines.Count);
			AssertEquals("Consol 1 Container Unpacked", 0, container.PackLines.Count);
			AssertEquals("Consol should have one unallocated pack line", 1, result.UnAllocatedPackLines.Count);
			return result;
		}

		protected CommonConsol GetConsolToPackWithOverrideContainerWeight()
		{
			var result = CreateConsol();
			result.JK_TransportMode = Constants.TransportModes.Sea;
			result.Transports[0].JW_JX = ExportSailing1.PK;
			result.AutomaticallyUpdatePackLineContainers = false;

			var container = result.Containers.AddNew();
			container.JC_ContainerNum = "JFDU8392839";
			container.JC_IsGrossWeightOverridden = true;
			container.JC_RC = RC_40GP_PK;

			CommonShipment shipment = result.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "s1";
			shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 10;
			AssertEquals("Consol 1 Shipment Unpacked", 1, shipment.OuterPackLines.Count);
			AssertEquals("Consol 1 Container Unpacked", 0, container.PackLines.Count);
			AssertEquals("Consol should have one unallocated pack line", 1, result.UnAllocatedPackLines.Count);
			return result;
		}

		protected CommonConsol GetConsolToUnpack()
		{
			var result = CreateConsol();
			result.JK_TransportMode = Constants.TransportModes.Sea;
			result.Transports[0].JW_JX = ImportSailing1.PK;
			result.AutomaticallyUpdatePackLineContainers = true;

			var container = result.Containers.AddNew();
			container.JC_ContainerNum = "JFDU8392839";
			container.JC_RC = RC_40GP_PK;

			CommonShipment shipment = result.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 10;
			AssertEquals("Consol 1 Shipment packed", 1, shipment.OuterPackLines.Count);
			AssertEquals("Consol 1 Container packed", 1, container.PackLines.Count);
			AssertEquals("Consol should have one unallocated pack line", 0, result.UnAllocatedPackLines.Count);
			return result;
		}

		protected abstract HelperForm CreateForm(CommonConsol consol);
		protected abstract CommonConsol CreateConsol();

		public class HelperForm : ZTemplateForm
		{
			public HelperForm(CommonConsol loadList, ControllerID controllerID)
				: base(loadList)
			{
				InitializeComponent();
				this.controllerID = controllerID;
				var tabPage = new ZTabPage();
				MainTabControl.Controls.Add(tabPage);
				PlugIns.Add(controllerID);
			}

			readonly ControllerID controllerID;

			public ContainersPackingPlugIn ContainersPackingPlugIn
			{
				get
				{
					return (ContainersPackingPlugIn)PlugIns.GetPlugIn(this.controllerID);
				}
			}

			public ZModuleButtonGrid ContainersModuleButtonGrid
			{
				get
				{
					return ContainersPackingPlugIn.PackingDetailsUserControl.ContainersModuleButtonGrid;
				}
			}

			public ZGrid UnAllocatedPackLinesGrid
			{
				get
				{
					return ContainersPackingPlugIn.PackingDetailsUserControl.UnAllocatedPackLinesGrid;
				}
			}

			public void SelectContainersTab()
			{
				ContainersPackingPlugIn.SelectTabPage();
			}

			public void ExposePack()
			{
				ContainersPackingPlugIn.packMenuItem.PerformClick();
			}

			public void ExposeUnpack()
			{
				ContainersPackingPlugIn.unpackMenuItem.PerformClick();
			}

			System.ComponentModel.IContainer components;

			new void InitializeComponent()
			{
				this.components = new System.ComponentModel.Container();
			}

			protected override void Dispose(bool isNotFinalizing)
			{
				if (isNotFinalizing && components != null)
				{
					components.Dispose();
				}
				base.Dispose(isNotFinalizing);
			}
		}
	}
}
