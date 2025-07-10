namespace Enterprise.ProcessManagement.Business
{
	public interface IExternalEntityLinkable
	{
		string ParentTableCode { get; }
		string ID { get; }
	}
}
