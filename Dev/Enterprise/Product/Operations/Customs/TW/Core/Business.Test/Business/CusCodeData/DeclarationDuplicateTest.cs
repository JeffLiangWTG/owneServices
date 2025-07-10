using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DeclarationDuplicate))]
	sealed class DeclarationDuplicateTest : Customs.Business.Testing.CusCodeDataTest<DeclarationDuplicate>
	{
		[ExpectNoExceptions]
		public void TestPropertyCaptions()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(declarationDuplicate.CopyInfo, "Copy", "The number of the declaration copies requested.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(declarationDuplicate.CY_CodeInfo, "Type", "The type of the declaration document copy requested.");
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(declarationDuplicate.Validation.GetType(), NUnit.Framework.Is.EqualTo(typeof(DeclarationDuplicateValidation)), "Validation");
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			NUnit.Framework.Assert.That(declarationDuplicate.Lookups.GetType(), NUnit.Framework.Is.EqualTo(typeof(DeclarationDuplicateLookups)), "Lookups");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			NUnit.Framework.Assert.That(declarationDuplicate.CY_Type, NUnit.Framework.Is.EqualTo("CDD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declarationDuplicate.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.DeclarationDuplicate).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declarationDuplicate.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(CusEntryInstructionSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declarationDuplicate.Copy, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			NUnit.Framework.Assert.That(declarationDuplicate.Parent, NUnit.Framework.Is.EqualTo(customsEntryInstruction));
		}

		#region Implementation
		protected override IEnumerable<DeclarationDuplicate> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().DeclarationDuplicates.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return customsEntryInstruction.DeclarationDuplicates.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = declaration ?? (declaration = Factory.New<JobDeclaration>());
			customsEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declarationDuplicate = customsEntryInstruction.DeclarationDuplicates.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction customsEntryInstruction;
		DeclarationDuplicate declarationDuplicate;
		#endregion
	}
}
