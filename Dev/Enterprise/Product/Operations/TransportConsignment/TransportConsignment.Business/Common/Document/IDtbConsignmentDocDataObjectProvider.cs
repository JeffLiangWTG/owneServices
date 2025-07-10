namespace Enterprise.TransportConsignment.Business
{
	public interface IDtbConsignmentDocDataObjectProvider
	{
		object GetDocDataObject(object parent, string dataContext);
	}
}
