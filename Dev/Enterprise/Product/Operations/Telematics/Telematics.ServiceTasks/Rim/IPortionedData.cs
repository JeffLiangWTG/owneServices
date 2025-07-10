namespace Enterprise.Telematics.ServiceTasks.Rim
{
	public interface IPortionedData
	{
		string BatchId { get; }
		string Message { get; }
	}
}
