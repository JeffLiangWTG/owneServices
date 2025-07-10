namespace Enterprise.MasterFiles.Business.Testing
{
	class WebSecurityRightsProviderForTest : WebSecurityRightsProvider
	{
		public new void Add(WebSecurityRight right)
		{
			base.Add(right);
		}
	}
}
