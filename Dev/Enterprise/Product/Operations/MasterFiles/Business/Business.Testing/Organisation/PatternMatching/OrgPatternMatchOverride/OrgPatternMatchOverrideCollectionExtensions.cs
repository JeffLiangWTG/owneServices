namespace Enterprise.MasterFiles.Business.Testing
{
	public static class OrgPatternMatchOverrideCollectionExtensions
	{
		public static OrgPatternMatchOverride CreatePatternMatchOverrideForTest(this OrgHeader orgHeader)
		{
			return orgHeader.PatternMatchOverrides_ForBinding.AddNew();
		}
	}
}
