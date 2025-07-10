using Enterprise.Customs.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryInstructionDeepCloneStrategyTest : Customs.Business.Testing.CusEntryInstructionDeepCloneStrategyTest
	{
		[ExpectNoExceptions]
		public void TestClone()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.TW_TradersRemarks = "AABBCC";
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.CusEntryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("AABBCC").Using(CustomComparers.TypeComparison));
			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.CusEntryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}
	}
}
