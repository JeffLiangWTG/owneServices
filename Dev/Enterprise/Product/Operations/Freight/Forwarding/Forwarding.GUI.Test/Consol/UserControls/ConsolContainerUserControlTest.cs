using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ConsolContainerUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestMergeContextMenuItem()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			using (var form = new MockConsolForm(consol))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var grid = form.ConsolContainerUserControl.JobContainerBoundGrid.InnerGrid;
				grid.SelectAllElements();

				grid.ContextMenu.MenuItems[1].PerformClick();
				AssertEquals(1, consol.Containers.Count);
				AssertCollectionContains(container1, consol.Containers);
				AssertCollectionNotContains(container2, consol.Containers);
			}
		}

		public void TestSplitContextMenuItem()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 200m;
			shipment.JS_ActualVolume = 100m;
			shipment.JS_OuterPacks = 3;

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;

			using (MockConsolForm form = new MockConsolForm(consol))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ConsolContainerUserControl.JobContainerBoundGrid.InnerGrid.ContextMenu.MenuItems[0].PerformClick();
				AssertEquals(2, consol.Containers.Count);
				AssertCollectionNotContains(multiContainer, consol.Containers);
			}
		}

		public void TestJCSealNumBoxStyle()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			using (var form = new MockConsolForm(consol))
			{
				form.Show();
				var grid = form.ConsolContainerUserControl.JobContainerBoundGrid.InnerGrid;
				AssertEquals("JC_SealNum should be zTextBoxColumnStyleInfo Type", typeof(ZArchitecture.ZTextBoxColumnStyle), grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(columnStyle => columnStyle.ColumnName == "JC_SealNum").ColumnStyleType);
			}
		}

		public void TestContainersColumnsBoundToGrid()
		{
			using (var containerUserControl = new ConsolContainerUserControl())
			{
				var grid = containerUserControl.JobContainerBoundGrid;
				Assert("Grid should include JC_SealNum column", ColumnExists(grid, "JC_SealNum"));
				Assert("Grid should include GoodsWeightForBinding column", ColumnExists(grid, "JC_TareWeight"));
				Assert("Grid should include GoodsWeightForBinding column", ColumnExists(grid, "GoodsWeightForBinding"));
				Assert("Grid should include JC_DunnageWeight column", ColumnExists(grid, "JC_DunnageWeight"));
				Assert("Grid should include WeightUnitForBinding column", ColumnExists(grid, "WeightUnitForBinding"));
				Assert("Grid should include JC_EmptyRequired column", ColumnExists(grid, "JC_EmptyRequired"));
				Assert("Grid should include JC_ReleaseNum column", ColumnExists(grid, "JC_ReleaseNum"));
				Assert("Grid should include JC_ContainerYardEmptyPickupGateOut column", ColumnExists(grid, "JC_ContainerYardEmptyPickupGateOut"));
				Assert("Grid should include JC_DepartureCartageAdvised column", ColumnExists(grid, "JC_DepartureCartageAdvised"));
				Assert("Grid should include JC_DepartureCartageRef column", ColumnExists(grid, "JC_DepartureCartageRef"));
				Assert("Grid should include JC_DepartureCartageComplete column", ColumnExists(grid, "JC_DepartureCartageComplete"));
				Assert("Grid should include JC_DepartureSlotDateTime column", ColumnExists(grid, "JC_DepartureSlotDateTime"));
				Assert("Grid should include JC_DepartureSlotReference column", ColumnExists(grid, "JC_DepartureSlotReference"));
				Assert("Grid should include JC_FCLWharfGateIn column", ColumnExists(grid, "JC_FCLWharfGateIn"));
				Assert("Grid should include JC_FCLUnloadFromVessel column", ColumnExists(grid, "JC_FCLUnloadFromVessel"));
				Assert("Grid should include JC_ContainerImportDORelease column", ColumnExists(grid, "JC_ContainerImportDORelease"));
				Assert("Grid should include JC_ArrivalCartageAdvised column", ColumnExists(grid, "JC_ArrivalCartageAdvised"));
				Assert("Grid should include JC_ArrivalCartageRef column", ColumnExists(grid, "JC_ArrivalCartageRef"));
				Assert("Grid should include JC_ArrivalSlotDateTime column", ColumnExists(grid, "JC_ArrivalSlotDateTime"));
				Assert("Grid should include JC_FCLWharfGateOut column", ColumnExists(grid, "JC_FCLWharfGateOut"));
				Assert("Grid should include JC_ArrivalEstimatedDelivery column", ColumnExists(grid, "JC_ArrivalEstimatedDelivery"));
				Assert("Grid should include JC_ArrivalCartageComplete column", ColumnExists(grid, "JC_ArrivalCartageComplete"));
				Assert("Grid should include JC_EmptyReturnedBy column", ColumnExists(grid, "JC_EmptyReturnedBy"));
				Assert("Grid should include JC_EmptyReadyForReturn column", ColumnExists(grid, "JC_EmptyReadyForReturn"));
				Assert("Grid should include JC_ContainerYardEmptyReturnGateIn column", ColumnExists(grid, "JC_ContainerYardEmptyReturnGateIn"));
				Assert("Grid should include JC_GrossWeightVerificationType column", ColumnExists(grid, "JC_GrossWeightVerificationType"));
				Assert("Grid should include JC_GrossWeightVerificationDateTime column", ColumnExists(grid, "JC_GrossWeightVerificationDateTime"));
				Assert("Grid should include GrossWeightVerifiedByNameOrPK column", ColumnExists(grid, "GrossWeightVerifiedByNameOrPK"));
				Assert("Grid should include JC_ArrivalCTOStorageStartDate column", ColumnExists(grid, "JC_ArrivalCTOStorageStartDate"));
				Assert("Grid should include JC_GrossWeightVerificationStatus column", ColumnExists(grid, "JC_GrossWeightVerificationStatus"));
				Assert("Grid should include JC_EmptyReturnReference column", ColumnExists(grid, "JC_EmptyReturnReference"));
			}
		}

		/// <summary>
		/// If you need to reduce the grid size, first confirm with the product team to ensure it won't negatively impact the user experience.  
		/// A smaller grid may require users to scroll when displaying more than three rows, which can feel inconvenient.
		/// </summary>
		public void TestContainerGrid_Has_TheMinimmumSize()
		{
			using (var containerUserControl = new ConsolContainerUserControl())
			using (var innerGrid = containerUserControl.JobContainerBoundGrid.InnerGrid)
			{
				Assert("Grid height is not less that a threshold", containerUserControl.JobContainerBoundGrid.Size.Height >= 140);
				Assert("Grid height is not less that a threshold", innerGrid.Size.Height >= 102);
			}
		}

		public void TestContainerChange_Debugging()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mock = new Mock<ConsolContainerUserControl>();

			using (var form = new MockConsolForm(consol))
			{
				form.Show();

				bool forceNull = false;
				mock.CallBase = true;
				mock.Protected()
					.Setup<CurrencyManager>("ContainerListManager")
					.Returns(() =>
						forceNull ? null : form.ConsolContainerUserControl.ContainerListManagerForDebugging);

				mock.Object.SetDataBinding(consol, "");

				consol.Containers.AddNew();
				forceNull = true;
				AssertNoExceptionThrown("no exception thrown", () => consol.Containers.AddNew());
			}
			mock.Object.Dispose();
		}

		public void TestAllocationIDColumnConditionalVisibility()
		{
			var testCases = new[]
			{
				(enableContracts: false, enableAllocations: false, expectVisible: false),
				(enableContracts: true, enableAllocations: false, expectVisible: false),
				(enableContracts: false, enableAllocations: true, expectVisible: false),
				(enableContracts: true, enableAllocations: true, expectVisible: true),
			};

			foreach (var (enableContracts, enableAllocations, expectVisible) in testCases)
			{
				using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableContracts))
				using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableAllocations))
				using (var containerUserControl = new ConsolContainerUserControl())
				{
					var grid = containerUserControl.JobContainerBoundGrid;
					var message = expectVisible
						? "Allocation ID column should show when both contracts registry items are true."
						: "Allocation ID column should not show unless both contracts registry items are true.";
					AssertEquals(message, expectVisible, ColumnExists(grid, "JC_RCA_AllocationLine"));
				}
			}
		}

		#region Implmentation

		bool ColumnExists(ZModuleButtonGrid grid, string columnName)
		{
			return grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == columnName);
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			if (fUserControl != null)
			{
				fUserControl.Dispose();
			}

			base.TearDown();
		}

		readonly ConsolContainerUserControl fUserControl;

		class MockConsolForm : ZForm
		{
			public MockConsolForm(ForwardingConsol consol)
				: base(consol)
			{
			}

			public ConsolContainerUserControl ConsolContainerUserControl { get; private set; }

			protected override void InitializeComponent()
			{
				ConsolContainerUserControl = new ConsolContainerUserControl();
				Controls.Add(ConsolContainerUserControl);
			}
		}

		#endregion
	}
}
