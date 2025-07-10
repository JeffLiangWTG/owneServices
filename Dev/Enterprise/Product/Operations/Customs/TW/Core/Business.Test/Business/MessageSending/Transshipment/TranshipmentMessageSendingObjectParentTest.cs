using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TranshipmentMessageSendingObjectParent))]
	sealed class TranshipmentMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TranshipmentMessageSendingObjectParent(Factory.NewWithValidTestData<CusInBondHeader>(), MessageTypeList.Codes.TRA);
		}

		[ExpectNoExceptions]
		public void TestSendingObjectsCollection()
		{
			var testWrapper1 = new TranshipmentMessageSendingObjectParent(Factory.NewWithValidTestData<CusInBondHeader>(), MessageTypeList.Codes.TRA);
			NUnit.Framework.Assert.That(testWrapper1.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestSecurityRightToSendWithMessageErrors()
		{
			var testWrapper = new TranshipmentMessageSendingObjectParent(Factory.NewWithValidTestData<CusInBondHeader>(), MessageTypeList.Codes.TRA);
			NUnit.Framework.Assert.That(testWrapper.SecurityCheckpointToSendWithMessageError, NUnit.Framework.Is.EqualTo(Env.Security.CustomsDeclarationSendWithMessageErrors));
		}
	}
}
