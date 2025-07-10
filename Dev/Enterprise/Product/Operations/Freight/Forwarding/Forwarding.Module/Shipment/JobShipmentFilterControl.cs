using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;
using IForwardingShipmentModuleCustomColumnsAndFiltersProvider = Enterprise.Integration.Customs.IForwardingShipmentModuleCustomColumnsAndFiltersProvider;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class JobShipmentFilterControl : ZFilterStripControl, IFilterControl, IGridControl
	{
		public JobShipmentFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				InitializeComponent();
				AddScreeningStatusColumnToGrid();
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, JobInvoicingConsumerTypes.Shipment.Code);
				ObjectFactory.Get<Enterprise.Integration.Customs.CA.IShipmentModuleColumnsAndFiltersProvider>().AddColumns(this);
				ObjectFactory.Get<IForwardingShipmentModuleCustomColumnsAndFiltersProvider>().AddColumns(this);

				SetConsignorTerminology();
			}
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new JobShipmentModuleStrip();
		}

		#region Consignor Terminology

		void SetConsignorTerminology()
		{
			string consignorShipperTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			FilteredGrid.SetColumnCaption("ConsignorNameOrPK", consignorShipperTerminology);
			FilteredGrid.SetColumnCaption("ConsignorDocumentaryAddress+E2_CompanyName", Res.GetString("d56996f5-85e8-4b3d-94cc-6c664ca46bc6", "{0} Full Name", consignorShipperTerminology));
			FilteredGrid.SetColumnCaption("ConsignorDocumentaryAddress+AddressAsASingleLine", Res.GetString("bd0812ea-676c-4270-98e4-c6120e94a08d", "{0} Address", consignorShipperTerminology));
		}

		#endregion

		#region Custom Fields

		void AddScreeningStatusColumnToGrid()
		{
			var columnForScreeningStatus = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var screeningStatusLabel = (!ComplianceRiskHelper.CheckIfComplianceRiskEnabled(typeof(ForwardingShipment), allowViewType: true)) ? Res.GetData("JobShipmentFilterControl|8d22b02a-a968-4af8-b70b-9ccd9f937cd4", "Screening Status") : Res.GetData("JobShipmentFilterControl|4f96dee8-eb05-42b4-8a62-f4a269db122c", "Legacy Screening Status");

			columnForScreeningStatus.CaptionResourceString = screeningStatusLabel;
			columnForScreeningStatus.ColumnName = "JS_ScreeningStatus";
			columnForScreeningStatus.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(32);

			FilteredGrid.ColumnStyles.Add(columnForScreeningStatus);
		}

		void JobShipmentFilterControl_Load(object sender, System.EventArgs e)
		{
			AddShipmentCustomFieldsToFilterGrid();
			HandleUSCustomsColumns();
			AddViewMeasurementsToFilterGrid();
			AddViewShipmentTrackingToFilterGrid();
		}

		void AddViewMeasurementsToFilterGrid()
		{
			var viewMeasurementsMenuItem = new ZMenuItem(ResString.GetMultilingualString("0bcf2292-61d4-52b2-48c2-c9b37ee50894", "View Measurements"));
			viewMeasurementsMenuItem.Click += (s, e) => ViewMeasurements();
			FilteredGrid.ContextMenu.MenuItems.Add(10, new ZMenuItem("-"));
			FilteredGrid.ContextMenu.MenuItems.Add(10, viewMeasurementsMenuItem);
		}

		void AddShipmentCustomFieldsToFilterGrid()
		{
			if (!DesignMode)
			{
				new CustomFieldColumnCreator().Set(FilteredGrid, new JobDocsAndCartageCustomFieldsDescriptor(), "DocsAndCartage", true, true);
			}
		} 

		void AddViewShipmentTrackingToFilterGrid()
		{
			if (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
			{
				var viewShipmentTrackingMenuItem = new ZMenuItem(ResString.GetMultilingualString("0ce7ba3f-8359-45e2-8508-2c519d548151", "Shipment Visibility"));
				viewShipmentTrackingMenuItem.Click += (s, e) => ViewShipmentTracking();
				FilteredGrid.ContextMenu.MenuItems.Add(11, viewShipmentTrackingMenuItem);
			}
		}

		#endregion

		#region US Customs Fields

		const string ImportColumnName = "JS_Calc_ImportManifestStatus";
		void HandleUSCustomsColumns()
		{
			if (!DesignMode && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) != Core.Constants.CountryCodes.Australia)
			{
				FilteredGrid.RemoveFromAvailableColumns(new string[] { ImportColumnName });
			}
			else
			{
				FilteredGrid.AddToAvailableColumns(new string[] { ImportColumnName });
			}

			if (!DesignMode && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				ZGridColumnInfo[] toRemove = new ZGridColumnInfo[2];
				foreach (ZGridColumnInfo column in FilteredGrid.ColumnStyles)
				{
					if (column.ColumnName == "EntryNumberStatus")
					{
						toRemove.SetValue(column, 1);
					}
				}

				foreach (ZGridColumnInfo remove in toRemove)
				{
					if (remove != null)
					{
						FilteredGrid.ColumnStyles.Remove(remove);
					}
				}
			}
		}

		#endregion

		#region View Measurements

		void ViewMeasurements()
		{
			var selectedShipments = FilteredGrid.GetSelectedElements<ForwardingShipment>();
			if (selectedShipments.Length == 0)
			{
				Globals.Message.Show(Res.GetString("d35af18c-0846-209d-4541-3c56cd3aa1dd", "Please select a shipment."),
					Res.GetString("5ae22c77-ffde-b09e-4fb3-9a8eabc2bdde", "Select Shipment"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				var title = Res.GetString("06d8b448-7dcf-c6ab-49d9-0070029657f1", "View Measurements");
				var shipmentMeasurementDescription = ShipmentMeasurementsHelper.GetMeasurements(selectedShipments);
				ShipmentViewMeasurementsForm.ShowDialog(title, shipmentMeasurementDescription);
			}
		}

		#endregion

		#region View Shipment Tracking

		void ViewShipmentTracking()
		{
			var selectedShipments = FilteredGrid.GetSelectedElements<ForwardingShipment>();
			if (selectedShipments.Length == 0)
			{
				Globals.Message.Show(Res.GetString("d35af18c-0846-209d-4541-3c56cd3aa1dd", "Please select a shipment."),
					Res.GetString("5ae22c77-ffde-b09e-4fb3-9a8eabc2bdde", "Select Shipment"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				ViewShipmentTrackingHelper.LaunchURL(selectedShipments);
			}
		}

		#endregion
	}
}
