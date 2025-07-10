using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class RefDocTypeValidationTest : BusinessObjectValidationTestCase
	{
		#region Validation

		public void TestCheckRT_LogMacro_SyntaxError()
		{
			DocType.RT_LogMacro = "Bad <Macro";
			AssertHasErrorContaining(DocType.RT_LogMacroInfo, "There were errors when compiling your macro");

			DocType.RT_LogMacro = "Good Macro";
			AssertNoErrorContaining(DocType.RT_LogMacroInfo, "There were errors when compiling your macro");
		}

		public void TestCheckRT_Desc()
		{
			DocType.RT_Desc = ZString.Empty;
			Assert("Expecting RT_Desc to be empty and have errors.", DocType.RT_DescInfo.HasErrors());
			DocType.RT_Desc = "asd";
			Assert("Expecting RT_Desc to have too few characters and have errors.", DocType.RT_DescInfo.HasErrors());
			DocType.RT_Desc = "Australia, Dollars";
			Assert("RT_Desc should be correct, not expecting errors.", !DocType.RT_DescInfo.HasNotifications());
		}

		public void TestCheckRT_DocType()
		{
			DocType.RT_DocType = ZString.Empty;
			Assert("Expecting RT_DocType to be empty and have errors.", DocType.RT_DocTypeInfo.HasErrors());
			DocType.RT_IsSystem = false;
			DocType.RT_DocType = "ET";
			Assert("Expecting RT_DocType to have too few characters and have errors.", DocType.RT_DocTypeInfo.HasErrors());
			DocType.RT_IsSystem = true;
			DocType.RT_DocType = "E2";
			Assert("The validation don't need to check the data created by system", !DocType.RT_DocTypeInfo.HasErrors());
			DocType.RT_IsSystem = false;
			DocType.RT_DocType = "MSC";
			Assert("Expecting DocType to have errors because it is the same as an ALL category code.", DocType.RT_DocTypeInfo.HasErrors());
			DocType.RT_DocType = "ETO";
			Assert("RT_DocType should be correct, not expecting errors.", !DocType.RT_DocTypeInfo.HasNotifications());
			DocType.RT_DocType = "A:B";
			Assert("Expecting DocType to have errors because it contains a :.", DocType.RT_DocTypeInfo.HasErrors());
		}

		public void TestCheckRT_ReferenceType()
		{
			DocType.RT_ReferenceType = ZString.Empty;
			Assert("Expecting RT_ReferenceType to be empty and have errors.", DocType.RT_ReferenceTypeInfo.HasErrors());
			DocType.RT_ReferenceType = "ET";
			Assert("Expecting RT_ReferenceType to have too few characters and have errors.", DocType.RT_ReferenceTypeInfo.HasErrors());
			DocType.RT_ReferenceType = "ETO";
			Assert("RT_ReferenceType not from list", DocType.RT_ReferenceTypeInfo.HasNotifications());
			DocType.RT_ReferenceType = "SCL";
			Assert("RT_ReferenceType from list and valid, no errors", !DocType.RT_ReferenceTypeInfo.HasNotifications());
			DocType.RT_ReferenceType = "CD:";
			Assert("Expecting RT_ReferenceType to have errors because it contains a :.", DocType.RT_ReferenceTypeInfo.HasErrors());
		}

		public void TestIsUniqueAndNotInAllCategory()
		{
			DocType.RT_DocType = "MSC";
			Assert(!DocType.Validation.IsUniqueAndNotInALLCategory());

			DocType.RT_DocType = "ZZZ";
			Assert(DocType.Validation.IsUniqueAndNotInALLCategory());

			RefDocType newDocType = Factory.New(typeof(RefDocType)) as RefDocType;
			newDocType.RT_DocType = "AAA";
			newDocType.RT_ReferenceType = "SHP";
			Assert(newDocType.Validation.IsUniqueAndNotInALLCategory());

			RefDocType anotherNewDocType = Factory.New(typeof(RefDocType)) as RefDocType;
			anotherNewDocType.RT_DocType = "AAA";
			anotherNewDocType.RT_ReferenceType = "SHP";
			Assert(!anotherNewDocType.Validation.IsUniqueAndNotInALLCategory());

			RefDocType unrelatedDocType = Factory.New(typeof(RefDocType)) as RefDocType;
			unrelatedDocType.RT_DocType = "AAA";
			unrelatedDocType.RT_ReferenceType = "DEC";
			Assert(unrelatedDocType.Validation.IsUniqueAndNotInALLCategory());
		}

		public void TestCheckRT_SE_NKDocumentReceivedEvent()
		{
			DocType.RT_SE_NKDocumentReceivedEvent = Events.CustomsCleared.Code;
			AssertNoErrors("Valid event code", DocType.RT_SE_NKDocumentReceivedEventInfo);

			DocType.RT_SE_NKDocumentReceivedEvent = "~ZZ";
			AssertHasErrors("Invalid event code", DocType.RT_SE_NKDocumentReceivedEventInfo);
		}

		#endregion

		#region Implementation

		RefDocType DocType;

		protected bool OnDocType_ConfirmUpdate(RefDocType sender, ConfirmEventArgs e)
		{
			return true;
		}

		protected override void SetUp()
		{
			base.SetUp();
			DocType = Factory.New<RefDocType>();
			DocType.RT_DocType = "BBB";
			DocType.RT_ReferenceType = "ALL";
		}

		public void TestGetExistingInAnyCategoryOtherThanAll()
		{
			RefDocType doctype1 = Factory.New<RefDocType>();
			doctype1.RT_Desc = "desc1";
			doctype1.RT_DocType = "YYY";
			doctype1.RT_ReferenceType = "SCL";
			Factory.Save();

			RefDocType doctype2 = Factory.New<RefDocType>();
			doctype2.RT_Desc = "desc2";
			doctype2.RT_DocType = "789";
			doctype2.RT_ReferenceType = "BPW";
			doctype2.Validation.ValidateRT_DocType();

			Assert(!doctype2.HasErrors);

			RefDocType doctype3 = Factory.New<RefDocType>();
			doctype3.RT_Desc = "desc3";
			doctype3.RT_DocType = "YYY";
			doctype3.RT_ReferenceType = "ALL";
			RefDocType[] types = doctype3.GetDuplicatedDocTypes();

			AssertEquals("Should be one that can cause duplication", 1, types.Length);
			AssertEquals("Should be the proper one", doctype1.PK, types[0].PK);
		}

		public void TestCheckIfViolatingUniqueIndex()
		{
			StmMenuItem mu1 = Factory.NewWithValidTestData<StmMenuItem>();
			StmMenuItem mu2 = Factory.NewWithValidTestData<StmMenuItem>();
			RefDocType docType1 = Factory.NewWithValidTestData<RefDocType>();
			docType1.RT_DocType = "AA1";
			docType1.RT_ReferenceType = "ALL";

			RefDocType docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "AA2";
			docType2.RT_ReferenceType = "ALL";

			RefDocType docType3 = Factory.NewWithValidTestData<RefDocType>();

			StmMenuEDocs edoc1 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc1.SX_SU = mu1.PK;
			edoc1.SX_RT_DocType = docType1.PK;

			StmMenuEDocs edoc2 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc2.SX_SU = mu1.PK;
			edoc2.SX_RT_DocType = docType2.PK;

			StmMenuEDocs edoc3 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc3.SX_SU = mu2.PK;
			edoc3.SX_RT_DocType = docType3.PK;

			Factory.Save();

			Assert(docType2.Validation.CheckIfViolatingUniqueIndexStmMenuEDocs(mu1.PK, docType1.PK));
			Assert(!docType3.Validation.CheckIfViolatingUniqueIndexStmMenuEDocs(mu2.PK, docType1.PK));
		}

		#endregion
	}
}
