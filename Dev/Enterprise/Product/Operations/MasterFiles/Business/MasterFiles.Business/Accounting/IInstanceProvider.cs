namespace Enterprise.MasterFiles.Business
{
	public interface IInstanceProvider<InstanceType>
	{
		InstanceType Get();
	}
}
