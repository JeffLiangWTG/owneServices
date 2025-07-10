using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301LicensingMessageSendingObjectParent))]
	sealed class NX301LicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX301MessageSendingObject>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			return new NX301LicensingMessageSendingObjectParent(decl);
		}
	}
}
