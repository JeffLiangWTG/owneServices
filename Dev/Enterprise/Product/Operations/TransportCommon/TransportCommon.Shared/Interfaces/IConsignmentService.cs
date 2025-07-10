using CargoWise.EntityFramework;

namespace Enterprise.TransportCommon.Shared
{
	public interface IConsignmentService
	{
		void LogServicesCommenced();
		BusinessObjectFactory Factory { get; }
	}
}
