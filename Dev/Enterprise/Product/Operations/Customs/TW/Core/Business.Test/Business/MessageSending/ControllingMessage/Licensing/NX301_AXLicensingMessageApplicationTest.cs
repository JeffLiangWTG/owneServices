using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_AXLicensingMessageApplication))]
	sealed class NX301_AXLicensingMessageApplicationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProvedPaper()
		{
			header.TW1_ProofOfPaper = true;
			var additionalInfo = application.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInfo.ProvedPaper, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "ProvedPaper");

			header.TW1_ProofOfPaper = false;
			additionalInfo = application.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInfo.ProvedPaper, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison), "ProvedPaper");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			application = new NX301_AXLicensingMessageApplication(header, null);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		IApplication application;
	}
}
