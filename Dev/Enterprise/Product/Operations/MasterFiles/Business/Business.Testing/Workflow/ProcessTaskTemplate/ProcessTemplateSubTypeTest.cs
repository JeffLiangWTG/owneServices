using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTemplateSubTypeTest : TestCase
	{
		public void TestList()
		{
			ProcessTemplateSubType subType = new ProcessTemplateSubType("Description", CodeDescPairList);
			AssertEquals(CodeDescPairList, subType.List);
			AssertEquals(false, subType.IsListRequired);

			subType = new ProcessTemplateSubType("Description", CodeDescPairList, true);
			AssertEquals(CodeDescPairList, subType.List);
			AssertEquals(true, subType.IsListRequired);
		}

		public void TestList_WithPullListDelegate()
		{
			ProcessTemplateSubType subType = new ProcessTemplateSubType("Description", delegate
			{ return CodeDescPairList; });
			AssertEquals(CodeDescPairList, subType.List);
			AssertEquals(false, subType.IsListRequired);

			subType = new ProcessTemplateSubType("Description", delegate
			{ return CodeDescPairList; }, true);
			AssertEquals(CodeDescPairList, subType.List);
			AssertEquals(true, subType.IsListRequired);
		}

		public void TestCollection()
		{
			ProcessTemplateSubType subType = new ProcessTemplateSubType("Description", StaffCollection);
			AssertEquals(StaffCollection, subType.Collection);
			AssertEquals(false, subType.IsListRequired);
			AssertEquals(true, subType.UseCollection);

			subType = new ProcessTemplateSubType("Description", StaffCollection, true);
			AssertEquals(StaffCollection, subType.Collection);
			AssertEquals(true, subType.IsListRequired);
			AssertEquals(true, subType.UseCollection);
		}

		readonly CodeDescriptionPairList CodeDescPairList = new CodeDescriptionPairList();
		readonly IActiveBusinessObjectCollection StaffCollection = new GlbStaffCollection(new BusinessObjectFactory());
	}
}
