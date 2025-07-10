using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GovernmentProcedureTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTransportTypeCode()
		{
			NUnit.Framework.Assert.That(governmentProcedure.TransportTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCurrentCode()
		{
			NUnit.Framework.Assert.That(governmentProcedure.CurrentCode, NUnit.Framework.Is.EqualTo("44").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			NUnit.Framework.Assert.That(governmentProcedure.Description.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var line = Factory.New<JobComInvoiceLine>();
			line.JI_Procedure = "44";
			governmentProcedure = new GovernmentProcedure(line);
		}

		IGovernmentProcedure governmentProcedure;
	}
}
