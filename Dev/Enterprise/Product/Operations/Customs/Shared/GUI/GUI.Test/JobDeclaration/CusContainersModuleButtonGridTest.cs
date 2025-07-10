using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CusContainersModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestEditCusContainerLicenceCheckpoint()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var container = declaration.CusContainers.AddNew();
			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.JobShipment.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(shipment))
			{
				AssertEditLicenceCheckpoint(form, declaration, Env.Licence.Forwarder);
			}
		}

		void AssertEditLicenceCheckpoint(ZForm parentForm, BaseJobDeclaration declaration, LicenceCheckpoint expected)
		{
			using (var userControl = new CusContainersModuleButtonGridControl())
			{
				userControl.SetDataBinding(declaration, "");
				parentForm.Controls.Add(userControl);

				var grid = userControl.CusContainersModuleButtonGrid;
				grid.SelectFirstRowIfOnlyRowInGrid();
				grid.EditButton.PerformClick();

				using (var shownForm = (ZForm)grid.LastShownZForm)
				{
					AssertEquals("ContainsCheckpoint(" + expected.Name + ")", true, shownForm.LicensedComponentManager.ContainsCheckpoint(expected));
				}
			}
		}

		sealed class CusContainersModuleButtonGridForTest : ZModuleButtonGrid
		{
			public ZToolStripButton EditButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).OfType<ZToolStripButton>().First();
				}
			}
		}

		sealed class CusContainersModuleButtonGridControl : ZUserControl
		{
			public CusContainersModuleButtonGridControl()
			{
				InitializeComponent();
			}

			void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.ColumnName = "zTextBoxColumnStyleInfo1";
				CusContainersModuleButtonGrid = new CusContainersModuleButtonGridForTest();

				CusContainersModuleButtonGrid.ModuleID = ModuleIDs.Containers;
				CusContainersModuleButtonGrid.BindToFindBoxList = "CusContainers";
				CusContainersModuleButtonGrid.BindToGridList = "CusContainers";
				CusContainersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				Controls.Add(CusContainersModuleButtonGrid);
			}

			internal CusContainersModuleButtonGridForTest CusContainersModuleButtonGrid;
		}
	}

	[TestedType(typeof(ZModuleButtonGrid))]
	sealed class CusContainersModuleButtonGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
	{
	}
}
