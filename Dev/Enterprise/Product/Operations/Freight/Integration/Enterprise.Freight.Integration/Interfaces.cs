using Enterprise.Integration;

namespace Enterprise.Freight.Integration
{
	// If you add members to these types, create a new file for them.
	public interface IJobContainerPackPivot { }
	public interface IContainerProcessTask : IExceptionDurationProcessTask { }
	public interface IOrderItem { }
	public interface IJobMAWB { }
	public interface IJobVoyageCollection { }
	public interface ISailingScheduleDataVendor { }
	public interface IJobTradeLaneCollection { }
}
