using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageAdditionalInformation))]
	sealed class LicensingMessageAdditionalInformationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProcessNumber()
		{
			NUnit.Framework.Assert.That(additionalInformation.ProcessNumber, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDelProcessNumber()
		{
			NUnit.Framework.Assert.That(additionalInformation.DelProcessNumber, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestContent()
		{
			NUnit.Framework.Assert.That(additionalInformation.Content, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			additionalInformation = new LicensingMessageAdditionalInformation("1", "2", "3");
		}

		IAdditionalInformation additionalInformation;
	}
}
