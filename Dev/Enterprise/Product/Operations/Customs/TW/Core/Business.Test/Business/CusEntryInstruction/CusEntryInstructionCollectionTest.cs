using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection))]
	sealed class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new CusEntryInstructionCollection(testDeclaration);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusEntryInstructionCollection);
		}

		[ExpectNoExceptions]
		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_CustomsOffice = "AA";
			declaration.JE_LocationOfGoods = "LOCAT";
			var instruction = declaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(instruction.CEI_GoodsLocation, NUnit.Framework.Is.EqualTo(declaration.JE_LocationOfGoods));
			NUnit.Framework.Assert.That(instruction.CEI_Style, NUnit.Framework.Is.EqualTo(Constants.DeclarationTypes.Export.G5).Using(CustomComparers.TypeComparison));
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			instruction = declaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(instruction.CEI_Style, NUnit.Framework.Is.EqualTo(Constants.DeclarationTypes.Import.G1).Using(CustomComparers.TypeComparison));
		}
	}
}
