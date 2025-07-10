using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ContainerUserControlTest : BaseFreightTest
	{
		[GuiTest]
		public void TestImportProcessTabLabels()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();

			consol.JK_TransportMode = Constants.TransportModes.Air;
			using (ContainerUserControlTestForm form = new ContainerUserControlTestForm(consol.Containers))
			{
				form.Show();
				form.control.CurrentContainer = container;
				form.control.SelectImportTab();

				ZTabPage importTabPage = (ZTabPage)form.control.DetailTabControl.Controls.Find("ImportTabPage", true)[0];
				ZGroupBox fCLDatesGroupBox = (ZGroupBox)importTabPage.Controls.Find("FCLDatesGroupBox", true)[0];
				ZGroupBox lCLDatesGroupBox = (ZGroupBox)importTabPage.Controls.Find("LCLDatesGroupBox", true)[0];
				ZDateEdit fCLUnloadFromVesselDateEdit = (ZDateEdit)importTabPage.Controls.Find("FCLUnloadFromVesselDateEdit", true)[0];
				ZDateEdit fCLWharfGateOutDateEdit = (ZDateEdit)importTabPage.Controls.Find("FCLWharfGateOutDateEdit", true)[0];
				ZCheckBox fCLHeldForTransitStagingCheckBox = (ZCheckBox)importTabPage.Controls.Find("FCLHeldForTransitStagingCheckBox", true)[0];

				AssertEquals("ULD Dates", fCLDatesGroupBox.Text);
				AssertEquals("LSE Dates", lCLDatesGroupBox.Text);
				AssertEquals("ULD Unload", fCLUnloadFromVesselDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("CTO Gate Out", fCLWharfGateOutDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("Held for ULD Transit Staging", fCLHeldForTransitStagingCheckBox.Extensions.Get<ILabelCaptionRenderer>().Caption);
			}

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			using (ContainerUserControlTestForm form = new ContainerUserControlTestForm(consol.Containers))
			{
				form.Show();
				form.control.CurrentContainer = container;
				form.control.SelectImportTab();

				ZTabPage importTabPage = (ZTabPage)form.control.DetailTabControl.Controls.Find("ImportTabPage", true)[0];
				ZGroupBox fCLDatesGroupBox = (ZGroupBox)importTabPage.Controls.Find("FCLDatesGroupBox", true)[0];
				ZGroupBox lCLDatesGroupBox = (ZGroupBox)importTabPage.Controls.Find("LCLDatesGroupBox", true)[0];
				ZDateEdit fCLUnloadFromVesselDateEdit = (ZDateEdit)importTabPage.Controls.Find("FCLUnloadFromVesselDateEdit", true)[0];
				ZDateEdit fCLWharfGateOutDateEdit = (ZDateEdit)importTabPage.Controls.Find("FCLWharfGateOutDateEdit", true)[0];
				ZCheckBox fCLHeldForTransitStagingCheckBox = (ZCheckBox)importTabPage.Controls.Find("FCLHeldForTransitStagingCheckBox", true)[0];

				AssertEquals("CTO Dates", fCLDatesGroupBox.Text);
				AssertEquals("CFS Dates", lCLDatesGroupBox.Text);
				AssertEquals("FCL Unload", fCLUnloadFromVesselDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("Wharf Gate Out", fCLWharfGateOutDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("Held for FCL Transit Staging", fCLHeldForTransitStagingCheckBox.Extensions.Get<ILabelCaptionRenderer>().Caption);
			}
		}

		#region ContainerPenalty Grid

		[GuiTest]
		public void TestContainerPenaltyGrid()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			using (var form = new ContainerUserControlTestForm(consol.Containers))
			{
				form.Show();
				form.control.CurrentContainer = container;

				form.control.SelectImportTab();
				var importTabPage = (ZTabPage)form.control.DetailTabControl.Controls.Find("ImportTabPage", true)[0];
				var penalityGrid = importTabPage.Controls.Find("PenaltiesGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(penalityGrid);

				foreach (var columnName in new[]
				{
					"CPY_PenaltyType",
					"CPY_CreditorType",
					"CPY_TimeUnit",
					"CPY_FreeTime",
					"CPY_Duration",
					"CPY_PerUnitCost",
					"CPY_TotalCost",
					"CPY_RX_NKCurrency",
					"CPY_RL_NKLocation",
					"CPY_OH_Creditor"
				})
				{
					var columnStyle = penalityGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull(columnStyle);
				}

				var penaltyTypeDescriptionColumnStyle = penalityGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "PenaltyTypeDescription");
				AssertNotNull(penaltyTypeDescriptionColumnStyle);
				AssertEquals("Penalty Description", penaltyTypeDescriptionColumnStyle.CaptionResourceString.Caption);
			}
		}

		[GuiTest]
		public void TestPenaltyTypeDescriptionColumnInExportProcessContainerPenaltyGrid()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			using (var form = new ContainerUserControlTestForm(consol.Containers))
			{
				form.Show();
				form.control.CurrentContainer = container;

				form.control.SelectExportTab();
				var exportTabPage = (ZTabPage)form.control.DetailTabControl.Controls.Find("ExportTabPage", true)[0];
				var penalityGrid = exportTabPage.Controls.Find("ExportPenaltiesGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(penalityGrid);

				var penaltyTypeDescriptionColumnStyle = penalityGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "PenaltyTypeDescription");
				AssertNotNull(penaltyTypeDescriptionColumnStyle);
				AssertEquals("Penalty Description", penaltyTypeDescriptionColumnStyle.CaptionResourceString.Caption);
			}
		}

		#endregion

		public void TestDefaultTabLabelsWhenContainerIsDeleted()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();

			using (var form = new ContainerUserControlTestForm(consol.Containers))
			{
				form.Show();
				form.control.CurrentContainer = container;
				container.Delete();
				form.control.SelectImportTab();

				var importTabPage = (ZTabPage)form.control.DetailTabControl.Controls.Find("ImportTabPage", true)[0];
				var datesGroupBox_FCL = (ZGroupBox)importTabPage.Controls.Find("FCLDatesGroupBox", true)[0];
				var datesGroupBox_LCL = (ZGroupBox)importTabPage.Controls.Find("LCLDatesGroupBox", true)[0];
				var unloadFromVesselDateEdit = (ZDateEdit)importTabPage.Controls.Find("FCLUnloadFromVesselDateEdit", true)[0];
				var wharfGateOutDateEdit = (ZDateEdit)importTabPage.Controls.Find("FCLWharfGateOutDateEdit", true)[0];
				var heldForTransitStagingCheckBox = (ZCheckBox)importTabPage.Controls.Find("FCLHeldForTransitStagingCheckBox", true)[0];

				AssertEquals("CTO Dates", datesGroupBox_FCL.Text);
				AssertEquals("CFS Dates", datesGroupBox_LCL.Text);
				AssertEquals("FCL Unload", unloadFromVesselDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("Wharf Gate Out", wharfGateOutDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("Held for FCL Transit Staging", heldForTransitStagingCheckBox.Extensions.Get<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestVGMStatusControlInVGMTab()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();

			using (var form = new ContainerUserControlTestForm(consol.Containers))
			{
				form.Show();
				form.control.CurrentContainer = container;
				container.Delete();

				var vgmTabPage = (ZTabPage)form.control.DetailTabControl.Controls.Find("VGMTabPage", true)[0];
				AssertNotNull(vgmTabPage);

				form.control.DetailTabControl.SelectedTab = vgmTabPage;

				var vgmStatusDropEdit = vgmTabPage.Controls.Find("VGMStatusDropEdit", true).FirstOrDefault() as ZDropEdit;
				AssertNotNull(vgmStatusDropEdit);
				Assert(vgmStatusDropEdit.ReadOnly);
			}
		}

		[GuiTest]
		public void TestExportTabPage_WhenNotForwardingConsolOrForwardingContainer()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			using (var form = new ContainerUserControlTestForm(consol.Containers))
			{
				form.Show();
				form.control.CurrentContainer = container;

				form.control.SelectExportTab();
				var exportTabPage = (ZTabPage)form.control.DetailTabControl.Controls.Find("ExportTabPage", true)[0];
				var relatedContainerLoadListFindBox = exportTabPage.Controls.Find("RelatedContainerLoadListFindBox", true).FirstOrDefault() as ZGuidFindBox;
				var relatedContainerLoadPlanFindBox = exportTabPage.Controls.Find("RelatedContainerLoadPlanFindBox", true).FirstOrDefault() as ZGuidFindBox;
				var supplierBookingCodeFindBox = exportTabPage.Controls.Find("SupplierBookingCodeFindBox", true).FirstOrDefault() as ZGuidFindBox;
				AssertNotNull(relatedContainerLoadListFindBox);
				AssertNotNull(relatedContainerLoadPlanFindBox);
				AssertNotNull(supplierBookingCodeFindBox);
				AssertEquals(false, relatedContainerLoadListFindBox.Visible);
				AssertEquals(false, relatedContainerLoadPlanFindBox.Visible);
				AssertEquals(false, supplierBookingCodeFindBox.Visible);
			}
		}

		[GuiTest]
		public void TestExportTabPage_WhenContainerIsEmpty_RelatedContainerLoadListAndSupplierBookingCodeFindBoxNotOverlap()
		{
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				using (var form = new ContainerUserControlTestForm(consol, consol.Containers))
				{
					form.Show();
					form.control.SelectExportTab();
					var exportTabPage = (ZTabPage)form.control.DetailTabControl.Controls.Find("ExportTabPage", true)[0];
					var relatedContainerLoadListFindBox = exportTabPage.Controls.Find("RelatedContainerLoadListFindBox", true).FirstOrDefault() as ZGuidFindBox;
					var relatedContainerLoadPlanFindBox = exportTabPage.Controls.Find("RelatedContainerLoadPlanFindBox", true).FirstOrDefault() as ZGuidFindBox;
					var supplierBookingCodeFindBox = exportTabPage.Controls.Find("SupplierBookingCodeFindBox", true).FirstOrDefault() as ZGuidFindBox;
					form.control.Manager.SuspendBinding();
					AssertNotNull(relatedContainerLoadListFindBox);
					AssertNotNull(relatedContainerLoadPlanFindBox);
					AssertNotNull(supplierBookingCodeFindBox);
					AssertEquals(false, relatedContainerLoadListFindBox.Visible);
					AssertEquals(true, relatedContainerLoadPlanFindBox.Visible);
					AssertEquals(true, supplierBookingCodeFindBox.Visible);
				}
			});
		}

		public void TestRefContainerLoadListOrJobSupplierBooking()
		{
			TestRefContainerLoadListOrJobSupplierBooking(true);
			TestRefContainerLoadListOrJobSupplierBooking(false);
		}

		void TestRefContainerLoadListOrJobSupplierBooking(bool enableAdvOrmFeature)
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var containerLoadList = Factory.NewWithValidTestData(CargoWise.Application.ObjectFactory.GetType(typeof(ICommonContainerLoadList)));
			containerLoadList["CLH_LoadListId"] = "CLL001";
			var container = Factory.New<DummyContainer>();
			container.SetContainerLoadListOrSupplierBookingRef(containerLoadList.PK, true, false);
			consol.Containers.Add(container);

			var availableDataSource = new List<object> { new Mock<IForwardingConsol>().Object, new Mock<IForwardingContainer>().Object };

			foreach (var refContainerLoadListIdVisible in new ZBool[] { ZBool.True, ZBool.False })
			{
				foreach (var refSupplierBookingVisible in new ZBool[] { ZBool.True, ZBool.False })
				{
					foreach (var dataSource in availableDataSource)
					{
						container.SetContainerLoadListOrSupplierBookingRef(containerLoadList.PK, refContainerLoadListIdVisible, refSupplierBookingVisible);

						AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
						{
							using (var form = new ContainerUserControlTestForm(dataSource, consol.Containers))
							{
								form.Show();
								form.control.SelectExportTab();
								var relatedContainerLoadListFindBox = (ZGuidFindBox)form.control.DetailTabControl.Controls.Find("RelatedContainerLoadListFindBox", true)[0];
								var supplierBookingCodeFindBox = (ZGuidFindBox)form.control.DetailTabControl.Controls.Find("SupplierBookingCodeFindBox", true)[0];

								AssertEquals(refSupplierBookingVisible && enableAdvOrmFeature, supplierBookingCodeFindBox.Visible);
								AssertEquals(relatedContainerLoadListFindBox.Visible, relatedContainerLoadListFindBox.ReadOnly);
								AssertEquals(refContainerLoadListIdVisible && enableAdvOrmFeature, relatedContainerLoadListFindBox.Visible);
								AssertEquals("CY Load ID", relatedContainerLoadListFindBox.GetExtension<LabelCaptionRenderer>().Caption);
							}
						});
					}
				}
			}
		}

		public void TestContainerLoadPlan()
		{
			TestContainerLoadPlan(true);
			TestContainerLoadPlan(false);
		}

		void TestContainerLoadPlan(bool enableAdvOrmFeature)
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var containerLoadPlan = Factory.NewWithValidTestData(CargoWise.Application.ObjectFactory.GetType(typeof(ICommonContainerLoadList)));
			var supplierBooking = Factory.NewWithValidTestData(CargoWise.Application.ObjectFactory.GetType(typeof(IJobSupplierBooking)));

			var container = Factory.New<DummyContainer>();
			consol.Containers.Add(container);

			var availableDataSource = new List<object> { new Mock<IForwardingConsol>().Object, new Mock<IForwardingContainer>().Object };

			foreach (var refSupplierBookingReadOnly in new[] { ZBool.True, ZBool.False })
			{
				foreach (var refContainerLoadPlanReadOnly in new[] { ZBool.True, ZBool.False })
				{
					foreach (var dataSource in availableDataSource)
					{
						container.SetContainerLoadPlanAndSupplierBooking(refContainerLoadPlanReadOnly, refSupplierBookingReadOnly);
						AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
						{
							using (var form = new ContainerUserControlTestForm(dataSource, consol.Containers))
							{
								form.Show();
								form.control.SelectExportTab();
								var relatedContainerLoadPlanFindBox = (ZGuidFindBox)form.control.DetailTabControl.Controls.Find("RelatedContainerLoadPlanFindBox", true)[0];
								var supplierBookingCodeFindBox = (ZGuidFindBox)form.control.DetailTabControl.Controls.Find("SupplierBookingCodeFindBox", true)[0];

								AssertEquals(refSupplierBookingReadOnly, supplierBookingCodeFindBox.ReadOnly);
								AssertEquals(refContainerLoadPlanReadOnly, relatedContainerLoadPlanFindBox.ReadOnly);
								AssertEquals(enableAdvOrmFeature, relatedContainerLoadPlanFindBox.Visible);
							}
						});
					}
				}
			}
		}

		#region Implementation

		public class ContainerUserControlTestForm : ZForm
		{
			public ContainerUserControlTestForm(CommonContainerCollection containers)
				: base(containers)
			{
				this.Controls.Add(control);
			}

			public ContainerUserControlTestForm(Object dataSource, CommonContainerCollection containers)
				: base(dataSource)
			{
				control.SetDataBinding(containers, "");
				this.Controls.Add(control);
			}

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				InitializeComponent();
			}

			new void InitializeComponent()
			{
				base.InitializeComponent();
				control = new ContainersUserTestControl();
				control.Dock = DockStyle.Fill;
				this.ClientSize = control.Size;
			}

			public ContainersUserTestControl control;
		}

		public class ContainersUserTestControl : ContainersUserControl
		{
			public CurrencyManager Manager => (CurrencyManager)GetBindingManager("JobContainer");
		}

		#endregion

		class DummyContainer : CommonContainer
		{
			public DummyContainer(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			ZGuid relatedContainerLoadListPK;
			public override ZGuid RelatedContainerLoadListPK => AdvOrmFeatureHelper.IsEnabled ? relatedContainerLoadListPK : ZGuid.Empty;

			ZBool relatedContainerLoadListVisible;
			public override ZBool RelatedContainerLoadListVisible => relatedContainerLoadListVisible && AdvOrmFeatureHelper.IsEnabled;

			ZBool relatedSupplierBookingVisible;
			public override ZBool RelatedSupplierBookingVisible => relatedSupplierBookingVisible && AdvOrmFeatureHelper.IsEnabled;

			ZBool relatedContainerLoadPlanReadOnly;
			public override bool JC_CLH_LoadListPlan_ReadOnly => relatedContainerLoadPlanReadOnly;

			ZBool relatedSupplierBookingReadOnly;
			public override bool JC_JSB_SupplierBooking_ReadOnly => relatedSupplierBookingReadOnly;

			public override ZBool RelatedContainerLoadPlanVisible => AdvOrmFeatureHelper.IsEnabled;

			public void SetContainerLoadListOrSupplierBookingRef(ZGuid pk, ZBool refContainerLoadListIdVisible, ZBool refSupplierBookingVisible)
			{
				(relatedContainerLoadListPK, this.relatedContainerLoadListVisible, this.relatedSupplierBookingVisible) = (pk, refContainerLoadListIdVisible, refSupplierBookingVisible);
			}

			public void SetContainerLoadPlanAndSupplierBooking(ZBool refContainerLoadPlanReadOnly, ZBool refSupplierBookingReadOnly)
			{
				(relatedContainerLoadPlanReadOnly, relatedSupplierBookingReadOnly) = (refContainerLoadPlanReadOnly, refSupplierBookingReadOnly);
			}
		}
	}
}
