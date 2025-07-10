using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(EIDOMessagingIdentityCollection))]
	internal class EIDOMessagingIdentityCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EIDOMessagingIdentityCollection>
	{
		public void TestParent()
		{
			AssertEquals(Messaging, Messaging.Identities.Parent);
			AssertEquals(Messaging, Messaging.Identities.AddNew().Parent);
		}

		public void TestReadOnly()
		{
			Messaging.ReadOnly = true;
			AssertEquals("AllowNew", false, Messaging.Identities.AllowNew);
			AssertEquals("AllowRemove", false, Messaging.Identities.AllowRemove);
			Messaging.ReadOnly = false;
			AssertEquals("AllowNew", true, Messaging.Identities.AllowNew);
			AssertEquals("AllowRemove", true, Messaging.Identities.AllowRemove);
		}

		#region Implementation
		EIDOMessagingHeader Messaging
		{
			get
			{
				return messaging ?? (messaging = new EIDOMessagingHeader());
			}
		}

		EIDOMessagingHeader messaging;
		protected override EIDOMessagingIdentityCollection GetCollectionToTest()
		{
			return new EIDOMessagingIdentityCollection(new EIDOMessagingHeader());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EIDOMessagingIdentity(new EIDOMessagingHeader());
		}
		#endregion
	}
}
