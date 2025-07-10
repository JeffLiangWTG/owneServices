using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NCATKMessageSendingObjectParent))]
	sealed class NCATKMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NCATKMessageSendingObjectParent(Declaration, controllingMessageType);
		}

		[ExpectNoExceptions]
		public void TestSendingObjectType()
		{
			var testWrapper1 = GetNewBusinessObject() as ControllingMessageSendingObjectParent;
			NUnit.Framework.Assert.That(testWrapper1.SendingObjectsCollection.First(), NUnit.Framework.Is.TypeOf<NCATKMessageSendingObject>());
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
					var messageHeader = fDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
					messageHeader.TW1_ControllingMessageType = controllingMessageType;
				}

				return fDeclaration;
			}
		}

		JobDeclaration fDeclaration;
		readonly ZString controllingMessageType = ControllingMessageTypeList.Codes.X101;
	}
}
