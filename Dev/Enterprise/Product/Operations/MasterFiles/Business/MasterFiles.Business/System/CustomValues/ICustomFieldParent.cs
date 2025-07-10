namespace Enterprise.MasterFiles.Business
{
	public delegate void OnResetCustomBusinessObjectDelegate();

	public interface ICustomFieldParent : ZArchitecture.Business.ICustomFieldProvider
	{
		void ResetCustomBusinessObject();
		OnResetCustomBusinessObjectDelegate OnResetCustomBusinessObject { get; set; }
	}
}
