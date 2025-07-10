using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.eTail.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVConsignmentUserControl : ZUserControl
	{
		public HVLVConsignmentUserControl()
		{
			InitializeComponent();
			SetUpContextMenu();

			ConsignmentsGrid.GetDeleteMenuVisibleMethod = () => !CurrentConsignment?.IsInDatabase ?? false;
			if (HVLVOriginLoadList.IsFunctionalTesting)
			{
				ConsignmentsGrid.MouseMove += ConsignmentsGrid_MouseMove;
			}

			showLineDetailsCheckBox.AllowOverlap(itemsUserControl);
			consignmentSplitContainer.AllowOverlap(showConsignmentDetailsCheckBox);
		}

		void ConsignmentsGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && ConsignmentsGrid.SelectedRowCount > 0)
			{
				ConsignmentsGrid.DoDragDrop(ConsignmentsGrid.SelectedElements, DragDropEffects.Link);
			}
		}

		HVLVConsignment CurrentConsignment => ConsignmentsGrid.GetCurrent() as HVLVConsignment;

		ZGrid ConsignmentsGrid => consignmentsModuleButtonGrid.InnerGrid;

		internal ZTextBox VolumeWeightTextBox => consignmentDetailsUserControl.VolumeWeightTextBox;
		internal ZGrid HVLVItemsGrid => itemsUserControl.HVLVItemsGrid;
		IHVLVConsignmentCollectionParent Header => DataSource as IHVLVConsignmentCollectionParent;

		public bool ShowAttachDetachButton
		{
			set
			{
				consignmentsModuleButtonGrid.ShowAttachButton = value;
				consignmentsModuleButtonGrid.ShowDetachButton = value;
				consignmentsModuleButtonGrid.SetToolStripVisibility(true);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var header = dataSource as IHVLVConsignmentCollectionParent;
			using (header?.SetIsDataBinding())
			{
				if (dataSource != null)
				{
					consignmentsCoverPanel.DataBindings.RemoveBinding("IsVisibleForBinding");
				}

				base.SetDataBinding(dataSource, dataMember);

				if (header is HVLVConsignmentHeader consignmentHeader && consignmentsModuleButtonGrid.ShowAttachButton)
				{
					consignmentsModuleButtonGrid.Attaching += BindDetachedConsignmentList;
				}

				if (dataSource != null)
				{
					consignmentsCoverPanel.DataBindings.Add(new KBinding("IsVisibleForBinding", dataSource, "ConsignmentsNotLoaded", ZBool.False, DataSourceUpdateMode.Never));
				}
			}
		}

		void BindDetachedConsignmentList(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			consignmentsModuleButtonGrid.FindBoxList ??= (Header as HVLVConsignmentHeader).Lookups.DetachedConsignment_List;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (!consignmentsCoverPanel.Visible)
			{
				filterStripControl.FirePerformSearch();
			}
		}

		public void RemoveColumnsByName(IEnumerable<string> columnNames)
		{
			foreach (var columnName in columnNames)
			{
				ConsignmentsGrid.SetAvailability(false, columnName);
			}
		}

		public static FilterStripBusinessObject GetConsignmentFilterBusinessObject()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var filterBizo = ObjectFactory.Get<FilterStripBusinessObject>("HVLVConsignmentFilterBusinessObject");
				ObjectFactory.Get<IFilterModuleStrategy>("DeniedPartyScreeningFilterModuleStrategy").RunOnModuleFiltersCreated(filterBizo.ModuleFilters, typeof(HVLVConsignment), null);

				return filterBizo;
			}

			return null;
		}

		void FilterStripControl_PerformSearch(object sender, EventArgs e)
		{
			if (Header != null)
			{
				ApplyFilter();
			}
		}

		void SetUpContextMenu()
		{
			var dividerMenuItem = new ZMenuItem("-");
			ConsignmentsGrid.ContextMenu.MenuItems.Add(dividerMenuItem);

			var calculateChargeableMenuItem = HVLVMenuItemHelper.CalculateChargeableMenuItem(CalculateChargeable);
			ConsignmentsGrid.ContextMenu.MenuItems.Add(calculateChargeableMenuItem);

			calculateLMCDepotDetailsMenuItem = HVLVMenuItemHelper.CalculateLMCDepotDetailsMenuItem(CalculateLMCDepotDetails);
			ConsignmentsGrid.ContextMenu.MenuItems.Add(calculateLMCDepotDetailsMenuItem);

			convertToStandAloneDeclarationMenuItem = HVLVMenuItemHelper.ConvertToStandAloneDeclarationMenuItem(ConvertToStandAloneDeclarationAction);
			ConsignmentsGrid.ContextMenu.MenuItems.Add(convertToStandAloneDeclarationMenuItem);

			toggleConsignmentActiveStatusMenuItem = HVLVMenuItemHelper.ToggleActiveStatus(ToggleSelectedConsignmentsActiveStatus);
			ConsignmentsGrid.ContextMenu.MenuItems.Add(ConsignmentsGrid.DeleteMenuItem.Index + 1, toggleConsignmentActiveStatusMenuItem);

			lastMileCarrierBookingMenuItem = new LastMileCarrierBookingMenuItem(() => CurrentConsignment);
			ConsignmentsGrid.ContextMenu.MenuItems.Add(lastMileCarrierBookingMenuItem);

			cancelLastMileCarrierBookingMenuItem = new CancelLastMileCarrierBookingMenuItem(() => CurrentConsignment);
			ConsignmentsGrid.ContextMenu.MenuItems.Add(cancelLastMileCarrierBookingMenuItem);

			var menuItem = HVLVMenuItemHelper.TransportBooking(() => CurrentConsignment);
			ConsignmentsGrid.ContextMenu.MenuItems.Add(menuItem);

			ConsignmentsGrid.ContextMenu.Popup += ConsignmentContextMenu_Popup;
		}

		ZMenuItem calculateLMCDepotDetailsMenuItem;
		ZMenuItem convertToStandAloneDeclarationMenuItem;
		ZMenuItem toggleConsignmentActiveStatusMenuItem;
		LastMileCarrierBookingMenuItem lastMileCarrierBookingMenuItem;
		CancelLastMileCarrierBookingMenuItem cancelLastMileCarrierBookingMenuItem;

		void ToggleSelectedConsignmentsActiveStatus(object sender, EventArgs e)
		{
			var selectedConsignments = ConsignmentsGrid.SelectedElements.OfType<HVLVConsignment>();
			var validConsignments = selectedConsignments.Where(consignment => consignment.IsInDatabase);

			foreach (var consignment in validConsignments)
			{
				if (consignment.HasCustomsStatus && consignment.HVC_IsActive)
				{
					var message = consignment.CannotDeactivateConsignmentsHavingCustomsStatusMessage;
					Globals.Message.Show(message);
					return;
				}
				else
				{
					consignment.HVC_IsActive = !consignment.HVC_IsActive;
				}
			}
		}

		void CalculateLMCDepotDetails(object sender, EventArgs e)
		{
			var consignment = CurrentConsignment;

			if (consignment != null)
			{
				LMCDepotDetailsCalculator.UpdateConsignmentsDestinationDetails(new[] { consignment.PK });
				consignment.Reload();
				consignment.RefreshBindingForLMCProperties();
			}
		}

		HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator LMCDepotDetailsCalculator => lmcDepotDetailsCalculator ?? (lmcDepotDetailsCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator());
		HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator lmcDepotDetailsCalculator;

		void CalculateChargeable(object sender, EventArgs e)
		{
			var consignments = Header.Consignments;

			foreach (HVLVConsignment consignment in consignments)
			{
				consignment.CalculateChargeable();
			}
		}

		void ShowConsignmentDetailsCheckBox_CheckedChanged(object sender, EventArgs args)
		{
			consignmentDetailsUserControl.Visible = showConsignmentDetailsCheckBox.Checked;
		}

		void ConsignmentContextMenu_Popup(object sender, EventArgs args)
		{
			UpdateLMCDepotDetailsMenuItemVisible();
			UpdateConvertToStandAloneDeclarationMenuItemVisible();
			lastMileCarrierBookingMenuItem.UpdateVisibilityAndCaption();
			cancelLastMileCarrierBookingMenuItem.UpdateVisibilityAndCaption();
			HVLVMenuItemHelper.UpdateToggleActiveStatusMenuItemUsabilityAndCaption(ConsignmentsGrid.SelectedElements, toggleConsignmentActiveStatusMenuItem);

			var selectedConsignment = CurrentConsignment;
			if (selectedConsignment != null)
			{
				ConsignmentsGrid.DeleteMenuItem.Enabled = selectedConsignment.CanDelete;
			}
		}

		void UpdateLMCDepotDetailsMenuItemVisible()
		{
			calculateLMCDepotDetailsMenuItem.Visible = CurrentConsignment != null;
		}

		void ShowLineDetailsCheckBox_CheckedChanged(object sender, EventArgs args)
		{
			HVLVItemSplitContainer.Panel2Collapsed = !showLineDetailsCheckBox.Checked;
		}

		void ConvertToStandAloneDeclarationAction(object sender, EventArgs args)
		{
			ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclaration(CurrentConsignment);
		}

		void UpdateConvertToStandAloneDeclarationMenuItemVisible()
		{
			convertToStandAloneDeclarationMenuItem.Visible = ConvertToStandAloneDeclarationHelper.ShouldShowConvertToStandAloneDeclarationMenuItem(CurrentConsignment);
		}

		public void ApplyFilter(HVLVConsignmentInMemoryFilter inMemoryFilter = null)
		{
			using (PerformanceStatisticsCollector.StartMonitoring(GetType().Name, nameof(ApplyFilter)))
			{
				var consignmentsView = Header?.ConsignmentsFilteredView;
				if (consignmentsView != null)
				{
					consignmentsView.Filter = filterStripControl?.FilterBusinessObject?.Filter;
					consignmentsView.InMemoryFilter = inMemoryFilter;

					if (!consignmentsView.CollectionToFilter.IsLoaded)
					{
						consignmentsView.CollectionToFilter.Load();
					}

					consignmentsView.Rebuild();
				}
			}
		}

		void consignmentsCoverPanelLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			filterStripControl.FirePerformSearch();
		}

		protected override void Dispose(bool disposing)
		{
			consignmentsModuleButtonGrid.Attaching -= BindDetachedConsignmentList;
			base.Dispose(disposing);
		}
	}
}
