using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageSendingObject))]
	sealed class ControllingMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ControllingMessageSendingObject(Factory.NewWithValidTestData<CusTWControllingMessageHeader>());
		}

		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			header.TW1_ControllingMessageType = "X101";
			header.TW1_ControllingAgency = "FT";
			header.TW1_FunctionalReferenceId = "C";
			header.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			var sendingObject = new ControllingMessageSendingObject(header);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sendingObject.MessageType, NUnit.Framework.Is.EqualTo("X101").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sendingObject.Description, NUnit.Framework.Is.EqualTo("產地證明申辦訊息").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sendingObject.MessageNumber, NUnit.Framework.Is.EqualTo("C").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sendingObject.ShouldSend, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sendingObject.CertificateType, NUnit.Framework.Is.EqualTo(CertificateTypeList.Codes.Code13).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sendingObject.CertificateTypeDescription, NUnit.Framework.Is.EqualTo(header.CertificateTypeDescription));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
		}
	}
}
