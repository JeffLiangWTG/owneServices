namespace Enterprise.MasterFiles.Integration
{
	public static partial class CustomsIntegration
	{
		public static partial class IT
		{
			public interface IGlbStaffWrapper : Integration.IGlbStaffWrapper
			{
				IGlbBrokerExternalPasswordCollection PasswordCollection { get; }
			}
		}
	}
}
