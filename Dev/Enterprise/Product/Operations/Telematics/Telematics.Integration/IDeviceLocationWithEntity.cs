namespace Enterprise.Telematics.Integration
{
	public interface IDeviceLocationWithEntity : IDeviceLocation, IWithEntity
	{
		ITelEdge GetOperator();
	}
}
