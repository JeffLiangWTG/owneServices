using CargoWise.Application;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module
{
	public partial class DtbBookingFilterControl : BookingTransportFilterControl
	{
		// for the designer
		public DtbBookingFilterControl()
			: this(null, null)
		{
		}

		public DtbBookingFilterControl(IDtbBookingCollection gridCollection, DtbBookingFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, WorkflowDescriptors.DtbBookingWorkflowDescriptorCode);
			
			if (!TransportRegistry.Instance.MasterBookingsEnabled.Value)
			{
				grid.ColumnStyles.Remove(grid.GetColumnStyle(nameof(DtbBooking.MasterBooking) + "+" + nameof(DtbBooking.KM_JobID)));
			}

			if (!ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				grid.ColumnStyles.Remove(grid.GetColumnStyle(nameof(DtbBooking.CO2eStatus)));
				grid.ColumnStyles.Remove(grid.GetColumnStyle(nameof(DtbBooking.TotalCO2eForSorting)));
			}
		}
	}

	// Inheriting directly from a generic class crashes designer, therefore a seperate class is created to extend the generic class
	public partial class BookingTransportFilterControl : ZFilterStripControl<DtbBookingWorkflowFilterStrip>
	{
		// for the designer
		public BookingTransportFilterControl()
		{
		}

		public BookingTransportFilterControl(IDtbBookingCollection gridCollection, DtbBookingFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
