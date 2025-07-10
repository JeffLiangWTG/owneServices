using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DeclarationDuplicateLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var declarationDuplicate = declaration.CustomsEntryInstructions.AddNew().DeclarationDuplicates.AddNew();
			NUnit.Framework.Assert.That(declarationDuplicate.Lookups.Parent, NUnit.Framework.Is.EqualTo(declarationDuplicate));
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var declarationDuplicate = declaration.CustomsEntryInstructions.AddNew().DeclarationDuplicates.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(object.ReferenceEquals(declarationDuplicate.Lookups.CY_CodeList, Factory.GetCachedValue<IMPDuplicateTypeList>()), NUnit.Framework.Is.EqualTo(true));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(object.ReferenceEquals(declarationDuplicate.Lookups.CY_CodeList, Factory.GetCachedValue<EXPDuplicateTypeList>()), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestDuplicateTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var declarationDuplicate = declaration.CustomsEntryInstructions.AddNew().DeclarationDuplicates.AddNew();
			var declarationDuplicateLookups = declarationDuplicate.Lookups;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.Count, NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(IMPDuplicateTypeList.Codes._2), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(IMPDuplicateTypeList.Codes._3), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(IMPDuplicateTypeList.Codes._4), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(IMPDuplicateTypeList.Codes._5), NUnit.Framework.Is.True);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.Count, NUnit.Framework.Is.EqualTo(5));
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(EXPDuplicateTypeList.Codes._3), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(EXPDuplicateTypeList.Codes._4), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(EXPDuplicateTypeList.Codes._5), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(EXPDuplicateTypeList.Codes._6), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declarationDuplicateLookups.CY_CodeList.ContainsCode(EXPDuplicateTypeList.Codes._7), NUnit.Framework.Is.True);
		}
	}
}
