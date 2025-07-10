using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(CusEntryHeaderProcessTask<CusEntryHeader>))]
sealed class CusEntryHeaderProcessTaskTest : ProcessTaskTest
{
	public void TestParentType()
	{
		var task = Factory.New<CusEntryHeaderProcessTaskForTest<CusEntryHeader>>();
		AssertEquals(typeof(CusEntryHeader), task.ParentType_Exposed);
	}

	protected override void SetUp()
	{
		base.SetUp();
		processTask = CreateEntryHeaderProcessTask(Factory);
	}
	CusEntryHeaderProcessTask<CusEntryHeader> processTask;

	protected override BusinessObject GetNewBusinessObject() => processTask;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateEntryHeaderProcessTask(factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => processTask;

	static CusEntryHeaderProcessTask<CusEntryHeader> CreateEntryHeaderProcessTask(BusinessObjectFactory factory)
	{
		var declaration = factory.New<BaseJobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var processTask = factory.New<CusEntryHeaderProcessTask<CusEntryHeader>>();
		processTask.P9_ParentID = entryHeader.PK;
		processTask.P9_ParentTableCode = entryHeader.TablePrefix;
		return processTask;
	}

	class CusEntryHeaderProcessTaskForTest<TType>(BusinessObjectFactory factory, DataRow row) : CusEntryHeaderProcessTask<TType>(factory, row) where TType : CusEntryHeader
	{
		public Type ParentType_Exposed => ParentType;
	}
}
