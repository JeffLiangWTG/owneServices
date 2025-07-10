namespace Enterprise.MasterFiles.Integration
{
	public interface IShowEDocUrlHandler
	{
		string Create(IeDoc storageDoc);
		string CreateRtf(IeDoc storageDoc, string caption);
	}
}
