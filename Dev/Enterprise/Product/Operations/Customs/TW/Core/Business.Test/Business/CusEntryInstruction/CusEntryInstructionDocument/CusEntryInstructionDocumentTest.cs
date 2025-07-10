using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionDocument))]
	sealed class CusEntryInstructionDocumentTest : Customs.Business.Testing.CusCodeDataTest<CusEntryInstructionDocument>
	{
		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(Factory.New<CusEntryInstructionDocument>().Validation, NUnit.Framework.Is.TypeOf<CusEntryInstructionDocumentValidation>());
		}

		[ExpectNoExceptions]
		public void TestDocumentNumber()
		{
			var cusEntryInstructionDocument = Factory.New<CusEntryInstructionDocument>();
			CombineAssertions(() =>
			{
				cusEntryInstructionDocument.CY_Data = "XX";
				NUnit.Framework.Assert.That(cusEntryInstructionDocument.CY_Data, NUnit.Framework.Is.EqualTo("XX").Using(CustomComparers.TypeComparison));
				cusEntryInstructionDocument.CY_Data = "YY";
				NUnit.Framework.Assert.That(cusEntryInstructionDocument.CY_Data, NUnit.Framework.Is.EqualTo("YY").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(cusEntryInstructionDocument.CY_DataInfo.MaxLength, NUnit.Framework.Is.EqualTo(35));
				NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(cusEntryInstructionDocument.CY_DataInfo).Caption, NUnit.Framework.Is.EqualTo("Attached Document No."), "Caption");
			});
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var cusEntryInstructionDocument = Factory.New<CusEntryInstructionDocument>();
			NUnit.Framework.Assert.That(cusEntryInstructionDocument.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.AttachedDocumentNumber).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCY_DataAllowWesternEuropeanCharactersOnly()
		{
			var cusEntryInstructionDocument = Factory.New<CusEntryInstructionDocument>();
			NUnit.Framework.Assert.That(cusEntryInstructionDocument.CY_DataAllowWesternEuropeanCharactersOnly, NUnit.Framework.Is.EqualTo(false));
		}

		protected override IEnumerable<CusEntryInstructionDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var cusEntryInstruction = declaration.CusEntryInstruction;
			yield return cusEntryInstruction.DocumentNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return cusEntryInstruction.DocumentNumbers.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, CusEntryInstructionDocument bizObj)
		{
			factory.Load<CusEntryInstruction>(bizObj.CY_ParentID);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			cusEntryInstruction = jobDeclaration.CusEntryInstruction;
		}

		JobDeclaration jobDeclaration;
		CusEntryInstruction cusEntryInstruction;
	}
}
