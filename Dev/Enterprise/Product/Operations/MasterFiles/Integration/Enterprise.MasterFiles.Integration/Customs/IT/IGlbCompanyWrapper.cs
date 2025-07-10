namespace Enterprise.MasterFiles.Integration
{
	public static partial class CustomsIntegration
	{
		public static partial class IT
		{
			public interface IGlbCompanyWrapper : Integration.IGlbCompanyWrapper
			{
				IGlbMauExternalPasswordCollection PasswordCollection { get; }
			}
		}
	}
}
