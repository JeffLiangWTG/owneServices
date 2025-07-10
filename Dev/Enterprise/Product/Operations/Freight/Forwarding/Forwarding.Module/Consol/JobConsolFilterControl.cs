using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class JobConsolFilterControl : ZFilterStripControl, IFilterControl, IGridControl
	{
		public JobConsolFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				AddInstalmentNumberColumn();
				InitializeComponent();
				AddScreeningStatusColumnToGrid();
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, JobInvoicingConsumerTypes.Consol.Code);
				ObjectFactory.Get<Enterprise.Integration.Customs.CA.IConsolModuleColumnsAndFiltersProvider>().AddColumns(this);
				ObjectFactory.Get<Enterprise.Integration.Customs.IForwardingConsolModuleCustomColumnsAndFiltersProvider>().AddColumns(this);
			}
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new JobConsolModuleStrip();
		}

		void AddInstalmentNumberColumn()
		{
			if (GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.UnitedArabEmirates)
			{
				ZArchitecture.ZTextBoxColumnStyleInfo instalmentNumberColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
				instalmentNumberColumn.ColumnName = "UAEInstalmentNumber";
				ControlDpiScalingHelper.SetWidth(ref instalmentNumberColumn, 80, true);
				instalmentNumberColumn.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobConsolFilterControl|4777a440-0b89-4796-8af5-1924e77c2532", "Installment #");
				instalmentNumberColumn.IsVisible = false;
				this.FilteredGrid.ColumnStyles.Add(instalmentNumberColumn);
			}
		}

		void AddScreeningStatusColumnToGrid()
		{
			var columnForScreeningStatus = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var screeningStatusLabel = (!ComplianceRiskHelper.CheckIfComplianceRiskEnabled(typeof(ForwardingConsol), allowViewType: true)) ? Res.GetData("JobConsolFilterControl|8d22b02a-a968-4af8-b70b-9ccd9f937cd4", "Screening Status") : Res.GetData("JobConsolFilterControl|4f96dee8-eb05-42b4-8a62-f4a269db122c", "Legacy Screening Status");

			columnForScreeningStatus.CaptionResourceString = screeningStatusLabel;
			columnForScreeningStatus.ColumnName = "JK_ScreeningStatus";
			columnForScreeningStatus.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(32);

			FilteredGrid.ColumnStyles.Add(columnForScreeningStatus);
		}
	}
}
