namespace Enterprise.Freight.Integration
{
	public static class SailingDataVendor
	{
		// If you add members to these types, create a new file for them.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IDakosySailingScheduleDataVendor { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOneStopSailingScheduleDataVendor { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOneStopContainerEventDataVendor { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IOnlineSailingSchedulesDataVendor
		{
		}
	}
}
