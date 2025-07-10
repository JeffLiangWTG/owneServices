using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ToOrderCollection))]
	public class ToOrderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ToOrderCollection(Factory);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var collection = GetCollectionToTest();

			var orgHeader = Factory.New<OrgHeader>();
			var notification = collection.GetAllNotificationsWhenAdditionalFilterNotMet(orgHeader);

			AssertEquals("An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.", notification);
		}

		public void TestOrganizationsRequiringTRI()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TEST001";
			org1.OH_FullName = "Test Organization1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TEST002";
			org2.OH_FullName = "Test Organization2";
			Factory.Save();

			var collection = (ToOrderCollection)GetCollectionToTest();
			collection.Load();

			AssertEquals(collection.Count, 0);

			var orgCusCode = org2.CustomsCodes.AddNew();
			orgCusCode.OK_CustomsRegNo = "123456";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			Factory.Save();

			collection.Load();
			AssertEquals(collection.Count, 1);
			AssertEquals(collection[0].OH_Code, "TEST002");
		}
	}
}
