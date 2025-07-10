namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IForwardingPackLine { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IForwardingPackLineManyToManyCollection { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface ICIMEDIMessage { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface ICFSContainerLoadList { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface ICIMEDIInterchange { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IForwardingDocsAndCartage { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOrderProcessTasks { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOrderLineProcessTask { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOrderContainer { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOrderLine : Enterprise.Integration.Forwarding.IOrderLine { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOrderLineDelivery { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOrderLineDeliverContainer { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IJobShipmentPreplanning { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IJobShipmentPreplanningProcessTask { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IForwardingShipmentForm { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IJobSupplierBooking : Enterprise.Integration.Forwarding.IJobSupplierBooking { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IJobSupplierBookingCollection : Enterprise.Integration.Forwarding.IJobSupplierBookingCollection { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface ICYContainerLoadList { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IContainerLoadListHeaderCollection : Enterprise.Integration.Forwarding.IContainerLoadListHeaderCollection { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IBulkConsolCreationForm { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface ISchedulesConsolsForm { }
	}
}
