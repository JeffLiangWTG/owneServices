using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EmailToContactBusinessObjectCollection))]
	sealed class EmailToContactBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EmailToContactBusinessObjectCollection>
	{
		#region Allow New

		public void TestAllowNew()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			EmailToContactBusinessObjectCollection collection = new EmailToContactBusinessObjectCollection(bizO);

			Assert("Should not allow new", !collection.AllowNew);
		}

		#endregion

		#region Implementation

		protected override EmailToContactBusinessObjectCollection GetCollectionToTest()
		{
			BizO = Factory.New<DummyBusinessObject>();
			return new EmailToContactBusinessObjectCollection(BizO);
		}

		DummyBusinessObject BizO;

		protected override void SetUp()
		{
			base.SetUp();
			BizO = Factory.New<DummyBusinessObject>();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EmailToContactBusinessObject(BizO);
		}

		#endregion
	}
}
