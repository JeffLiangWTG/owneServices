using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public void TestRequiresMergeCore()
		{
			var mergeManager = new MergeManager((JobDeclaration)GetJobDeclaration());
			AssertEquals(false, mergeManager.RequiresMergeBeforeSave);
		}
		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
