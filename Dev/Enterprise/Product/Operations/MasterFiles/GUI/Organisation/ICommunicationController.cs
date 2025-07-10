namespace Enterprise.MasterFiles.GUI
{
	public interface ICommunicationController
	{
		bool CreateNewWithParentFormBizObjDefaults { get; set; }

#if DEBUG
		System.Windows.Forms.Form ActiveFormOverrideForTesting { get; set; }
#endif
	}
}
