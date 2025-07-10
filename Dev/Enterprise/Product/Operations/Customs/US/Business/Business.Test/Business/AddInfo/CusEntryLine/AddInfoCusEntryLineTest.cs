using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoCusEntryLine))]
	sealed class AddInfoCusEntryLineTest : AddInfoAbstractTest
	{
		public override void TestIsExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AddInfoCusEntryLine addInfo = new AddInfoCusEntryLine(entryLine.CL_AddInfoInfo);
			AssertNull("PreCondition: Declaration must be null", entryLine.Declaration);
			AssertEquals("IsExport", false, addInfo.IsExport);
			entryHeader.CH_JE = declaration.PK;
			AssertNotNull("PreCondition: Declaration must be null", entryLine.Declaration);
			AssertEquals("IsExport", true, addInfo.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExport", false, addInfo.IsExport);
		}

		public override void TestIsDrawback()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AddInfoCusEntryLine addInfo = new AddInfoCusEntryLine(entryLine.CL_AddInfoInfo);
			AssertNull("PreCondition: Declaration must be null", entryLine.Declaration);
			Assert("IsDrawback", !addInfo.IsDrawback);
			entryHeader.CH_JE = declaration.PK;
			AssertNotNull("PreCondition: Declaration must be null", entryLine.Declaration);
			Assert("IsDrawback", addInfo.IsDrawback);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsDrawback", !addInfo.IsDrawback);
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoCusEntryLineLookups);

		protected override Type GetExpectedValidationType() => typeof(AddInfoCusEntryLineValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = Factory.New<CusEntryLine>();
			return new AddInfoCusEntryLine(entryLine.CL_AddInfoInfo);
		}
	}
}
