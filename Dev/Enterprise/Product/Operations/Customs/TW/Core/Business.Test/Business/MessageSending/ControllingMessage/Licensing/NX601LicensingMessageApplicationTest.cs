using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX601LicensingMessageApplication))]
	sealed class NX601LicensingMessageApplicationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestReturnSample()
		{
			header.TW1_ApplyForSampleReturn = true;
			var additionalInfo = application.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInfo.ReturnSample, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "ReturnSample");

			header.TW1_ApplyForSampleReturn = false;
			additionalInfo = application.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInfo.ReturnSample, NUnit.Framework.Is.EqualTo(ZString.Empty), "ReturnSample");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			application = new NX601LicensingMessageApplication(header, null);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		IApplication application;
	}
}
