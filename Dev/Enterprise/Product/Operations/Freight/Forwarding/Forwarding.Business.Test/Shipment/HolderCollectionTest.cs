using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(HolderCollection))]
	public class HolderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new HolderCollection(shipment);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var collection = new HolderCollection(shipment);

			var orgHeader = Factory.New<OrgHeader>();
			var notification = collection.GetAllNotificationsWhenAdditionalFilterNotMet(orgHeader);

			AssertEquals("An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.", notification);
		}

		public void TestLoadFromShipmentAndNeedTRI()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TEST001";
			org1.OH_FullName = "Test Organization1";
			org1.OH_IsConsignor = true;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TEST002";
			org2.OH_FullName = "Test Organization2";
			org2.OH_IsConsignee = true;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "TEST003";
			org3.OH_FullName = "Test Organization3";

			Factory.Save();
			var collection = new HolderCollection(shipment);
			collection.Load();

			AssertEquals(0, collection.Count);

			var orgCusCode2 = org2.CustomsCodes.AddNew();
			orgCusCode2.OK_CustomsRegNo = "123456";
			orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

			var orgCusCode3 = org3.CustomsCodes.AddNew();
			orgCusCode3.OK_CustomsRegNo = "654321";
			orgCusCode3.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

			Factory.Save();
			collection.Load();

			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(["TEST002", "TEST003"], collection.Select(c => c.OH_Code));
		}
	}
}
