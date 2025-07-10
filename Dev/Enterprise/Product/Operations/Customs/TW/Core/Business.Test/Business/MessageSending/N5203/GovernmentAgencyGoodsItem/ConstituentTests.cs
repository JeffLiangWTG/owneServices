using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ConstituentTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestElementDescription()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Compositions = "TTT22";
				NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo("TTT22").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_Compositions = "NO.008 ";
				NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestLevelID()
		{
			NUnit.Framework.Assert.That(constituent.LevelID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestThickness()
		{
			NUnit.Framework.Assert.That(constituent.Thickness, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
			constituent = new Constituent(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
		IConstituent constituent;
	}
}
