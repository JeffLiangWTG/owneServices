namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Testing
{
	class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public override void TestRequiresMerge()
		{
			var dec = GetJobDeclaration();
			AssertEquals("RequiresMerge", false, dec.MergeManager.RequiresMerge);
		}

		public override void TestSupportsAutoMerge()
		{
			var dec = GetJobDeclaration();
			dec.ActiveEntryHeaders.AddNew();
			AssertEquals("SupportsAutoMerge", false, dec.MergeManager.SupportsAutoMerge);
		}

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			return declaration;
		}
	}
}
