using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoCusEntryHeader))]
	sealed class AddInfoCusEntryHeaderTest : AddInfoAbstractTest
	{
		public override void TestIsExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AddInfoCusEntryHeader addInfo = new AddInfoCusEntryHeader(entryHeader.CH_AddInfoInfo);
			AssertNull("PreCondition: Declaration must be null", entryHeader.Declaration);
			AssertEquals("IsExport", false, addInfo.IsExport);
			entryHeader.CH_JE = declaration.PK;
			AssertNotNull("PreCondition: Declaration must not be null", entryHeader.Declaration);
			AssertEquals("IsExport", true, addInfo.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExport", false, addInfo.IsExport);
		}

		public void TestUS_R_IsHMFApplicable_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			var addinfo = originalEntry.GetWrappedEntry().GetAddInfo();
			AssertEquals(true, addinfo.US_R_IsHMFApplicableInfo.ReadOnly);
			originalEntry.US_R_NoLineDetails = false;
			AssertEquals(false, addinfo.US_R_IsHMFApplicableInfo.ReadOnly);
		}

		public override void TestIsDrawback()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AddInfoCusEntryHeader addInfo = new AddInfoCusEntryHeader(entryHeader.CH_AddInfoInfo);
			AssertNull("PreCondition: Declaration must be null", entryHeader.Declaration);
			Assert("IsDrawback", !addInfo.IsDrawback);
			entryHeader.CH_JE = declaration.PK;
			AssertNotNull("PreCondition: Declaration must not be null", entryHeader.Declaration);
			Assert("IsDrawback", addInfo.IsDrawback);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsDrawback", !addInfo.IsDrawback);
		}

		public void TestGetReconValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			ReconOriginalEntryHeader reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			CusEntryHeader entry = reconOriginalEntry.GetWrappedEntry();
			AssertEquals(typeof(ReconAddInfoOriginalCusEntryHeaderValidation), entry.AddInfoValidation.GetType());
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoCusEntryHeaderLookups);

		protected override Type GetExpectedValidationType() => typeof(AddInfoCusEntryHeaderValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			return new AddInfoCusEntryHeader(entryHeader.CH_AddInfoInfo);
		}
	}
}
