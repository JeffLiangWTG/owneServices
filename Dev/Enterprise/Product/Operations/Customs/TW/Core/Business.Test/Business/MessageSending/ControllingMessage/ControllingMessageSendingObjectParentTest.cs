using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageSendingObjectParent))]
	sealed class ControllingMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ControllingMessageSendingObjectParent(Declaration, controllingMessageType, "menu Caption");
		}

		[ExpectNoExceptions]
		public void TestMenuCaption()
		{
			var testWrapper1 = GetNewBusinessObject() as ControllingMessageSendingObjectParent;
			NUnit.Framework.Assert.That(testWrapper1.MenuCaption, NUnit.Framework.Is.EqualTo("menu Caption").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSendingObjectsCollection()
		{
			var testWrapper1 = GetNewBusinessObject() as ControllingMessageSendingObjectParent;
			NUnit.Framework.Assert.That(testWrapper1.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestSecurityRightToSendWithMessageErrors()
		{
			var testWrapper = GetNewBusinessObject() as ControllingMessageSendingObjectParent;
			NUnit.Framework.Assert.That(testWrapper.SecurityCheckpointToSendWithMessageError, NUnit.Framework.Is.EqualTo(Env.Security.CustomsDeclarationSendWithMessageErrors));
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
		[ExpectNoExceptions]
		public void TestIJobDeclarationMessageSendingObjectParent()
		{
			var jobDeclarationMessageSendingObjectParent = GetNewBusinessObject() as IJobDeclarationMessageSendingObjectParent;
			NUnit.Framework.Assert.That(jobDeclarationMessageSendingObjectParent, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Business.IJobDeclarationMessageSendingObjectParent)));
			NUnit.Framework.Assert.That(jobDeclarationMessageSendingObjectParent.BizObjValidationMessageErrors, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(jobDeclarationMessageSendingObjectParent.AdditionalWarnings, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(jobDeclarationMessageSendingObjectParent.ParentDeclaration, NUnit.Framework.Is.EqualTo(Declaration).Using(CustomComparers.TypeComparison));
		}
	}
}
