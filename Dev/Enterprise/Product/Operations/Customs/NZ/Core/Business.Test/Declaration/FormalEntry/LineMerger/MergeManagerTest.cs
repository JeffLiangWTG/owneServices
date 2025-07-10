namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	using System;

	class MergeManagerTest : Customs.Business.Testing.MergeManagerWhichSupportsAutoMergeTest
	{
		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
