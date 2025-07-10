using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GovernmentProcedureWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCurrentCode()
		{
			IGovernmentProcedure governmentProcedure = new GovernmentProcedureWrapper("AA");
			NUnit.Framework.Assert.That(governmentProcedure.CurrentCode, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison), "GovernmentProcedure.CurrentCode should be");
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			IGovernmentProcedure governmentProcedure = new GovernmentProcedureWrapper("AA");
			NUnit.Framework.Assert.That(governmentProcedure.TransportTypeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportTypeCode()
		{
			IGovernmentProcedure governmentProcedure = new GovernmentProcedureWrapper(null, "AA");
			NUnit.Framework.Assert.That(governmentProcedure.TransportTypeCode, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(governmentProcedure.CurrentCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			IGovernmentProcedure governmentProcedure = new GovernmentProcedureWrapper(null, "AA");
			NUnit.Framework.Assert.That(governmentProcedure.Description.ToString(), NUnit.Framework.Is.Null.Or.Empty, "AA - should be [null] or [empty]");
		}
	}
}
