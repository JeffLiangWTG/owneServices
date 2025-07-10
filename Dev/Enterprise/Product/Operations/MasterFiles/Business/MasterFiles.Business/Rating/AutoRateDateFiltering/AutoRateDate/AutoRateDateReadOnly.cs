namespace Enterprise.MasterFiles.Business
{
	public class AutoRateDateReadOnly : JobConfigurationSelectorReadOnly
	{
		public AutoRateDateReadOnly(IJobConfigurationSelector parent) : base(parent)
		{
		}

		public bool Location_ReadOnly => AutoRateDateReadOnlyHelper.Location_ReadOnly(Parent.DirectionCodeInfo);

		public bool ContainerMode_ReadOnly => AutoRateDateReadOnlyHelper.ContainerMode_ReadOnly(Parent.JobType, Parent.Mode);
	}
}
