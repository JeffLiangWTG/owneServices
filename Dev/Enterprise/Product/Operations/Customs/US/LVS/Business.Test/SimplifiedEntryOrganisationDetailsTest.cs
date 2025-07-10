using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	class SimplifiedEntryOrganisationDetailsTest : TestCaseWithFactory
	{
		public void TestEntities()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			consignment.ULB_SellerName = "Me";
			consignment.ULB_SellerAddress1 = "street";
			consignment.ULB_SellerAddress2 = "avenue";
			consignment.ULB_SellerCity = "Dieppe";
			consignment.ULB_SellerState = "Normandy";
			consignment.ULB_SellerPostCode = "76320";
			consignment.ULB_RN_NKSellerCountry = "FR";

			consignment.ULB_ConsigneeQualifier = "A";
			consignment.ULB_ConsigneeIdentifier = "B";
			consignment.ULB_ConsigneeName = "Me2";
			consignment.ULB_ConsigneeAddress1 = "street2";
			consignment.ULB_ConsigneeAddress2 = "avenue2";
			consignment.ULB_ConsigneeCity = "Rouen";
			consignment.ULB_ConsigneeState = "Normandy";
			consignment.ULB_ConsigneePostCode = "76000";
			consignment.ULB_RN_NKConsigneeCountry = "FR";

			var entities = ((IACECargoReleaseHeader)consignment).Entities;

			AssertEquals("4 Entities", 4, entities.Count());

			var seller = entities.OfType<SimplifiedEntryOrganisationDetails>().FirstOrDefault(x => ((ISimplifiedEntryOrganisationDetails)x).EntityCode == "SE");
			AssertNotNull("Entity Code", seller);
			AssertEquals("ULB_SellerName", "Me", ((ISimplifiedEntryOrganisationDetails)seller).CompanyName);
			AssertEquals("ULB_SellerAddress1", "street", ((ISimplifiedEntryOrganisationDetails)seller).AddressLine1);
			AssertEquals("ULB_SellerAddress2", "avenue", ((ISimplifiedEntryOrganisationDetails)seller).AddressLine2);
			AssertEquals("ULB_SellerCity", "Dieppe", ((ISimplifiedEntryOrganisationDetails)seller).City);
			AssertEquals("ULB_SellerState", "Normandy", ((ISimplifiedEntryOrganisationDetails)seller).State);
			AssertEquals("ULB_SellerPostCode", "76320", ((ISimplifiedEntryOrganisationDetails)seller).PostCode);
			AssertEquals("ULB_RN_NKSellerCountry", "FR", ((ISimplifiedEntryOrganisationDetails)seller).Country);

			var consignee = entities.OfType<SimplifiedEntryOrganisationDetails>().FirstOrDefault(x => ((ISimplifiedEntryOrganisationDetails)x).EntityCode == "CN");
			AssertNotNull("Entity Code", consignee);
			AssertEquals("ULB_ConsigneeQualifier", "A", ((ISimplifiedEntryOrganisationDetails)consignee).EntityIdentifierQualifier);
			AssertEquals("ULB_ConsigneeIdentifier", "B", ((ISimplifiedEntryOrganisationDetails)consignee).EntityIdentifier);
			AssertEquals("ULB_ConsigneeName", "Me2", ((ISimplifiedEntryOrganisationDetails)consignee).CompanyName);
			AssertEquals("ULB_ConsigneeAddress1", "street2", ((ISimplifiedEntryOrganisationDetails)consignee).AddressLine1);
			AssertEquals("ULB_ConsigneeAddress2", "avenue2", ((ISimplifiedEntryOrganisationDetails)consignee).AddressLine2);
			AssertEquals("ULB_ConsigneeCity", "Rouen", ((ISimplifiedEntryOrganisationDetails)consignee).City);
			AssertEquals("ULB_ConsigneeState", "Normandy", ((ISimplifiedEntryOrganisationDetails)consignee).State);
			AssertEquals("ULB_ConsigneePostCode", "76000", ((ISimplifiedEntryOrganisationDetails)consignee).PostCode);
			AssertEquals("ULB_RN_NKConsigneeCountry", "FR", ((ISimplifiedEntryOrganisationDetails)consignee).Country);

			var manufacturer = entities.OfType<SimplifiedEntryOrganisationDetails>().FirstOrDefault(x => ((ISimplifiedEntryOrganisationDetails)x).EntityCode == "MF");
			AssertNotNull("Entity Code", seller);
			AssertEquals("ULB_SellerName", "Me", ((ISimplifiedEntryOrganisationDetails)seller).CompanyName);
			AssertEquals("ULB_SellerAddress1", "street", ((ISimplifiedEntryOrganisationDetails)seller).AddressLine1);
			AssertEquals("ULB_SellerAddress2", "avenue", ((ISimplifiedEntryOrganisationDetails)seller).AddressLine2);
			AssertEquals("ULB_SellerCity", "Dieppe", ((ISimplifiedEntryOrganisationDetails)seller).City);
			AssertEquals("ULB_SellerState", "Normandy", ((ISimplifiedEntryOrganisationDetails)seller).State);
			AssertEquals("ULB_SellerPostCode", "76320", ((ISimplifiedEntryOrganisationDetails)seller).PostCode);
			AssertEquals("ULB_RN_NKSellerCountry", "FR", ((ISimplifiedEntryOrganisationDetails)seller).Country);

			var buyer = entities.OfType<SimplifiedEntryOrganisationDetails>().FirstOrDefault(x => ((ISimplifiedEntryOrganisationDetails)x).EntityCode == "BY");
			AssertNotNull("Entity Code", consignee);
			AssertEquals("ULB_ConsigneeQualifier", "A", ((ISimplifiedEntryOrganisationDetails)consignee).EntityIdentifierQualifier);
			AssertEquals("ULB_ConsigneeIdentifier", "B", ((ISimplifiedEntryOrganisationDetails)consignee).EntityIdentifier);
			AssertEquals("ULB_ConsigneeName", "Me2", ((ISimplifiedEntryOrganisationDetails)consignee).CompanyName);
			AssertEquals("ULB_ConsigneeAddress1", "street2", ((ISimplifiedEntryOrganisationDetails)consignee).AddressLine1);
			AssertEquals("ULB_ConsigneeAddress2", "avenue2", ((ISimplifiedEntryOrganisationDetails)consignee).AddressLine2);
			AssertEquals("ULB_ConsigneeCity", "Rouen", ((ISimplifiedEntryOrganisationDetails)consignee).City);
			AssertEquals("ULB_ConsigneeState", "Normandy", ((ISimplifiedEntryOrganisationDetails)consignee).State);
			AssertEquals("ULB_ConsigneePostCode", "76000", ((ISimplifiedEntryOrganisationDetails)consignee).PostCode);
			AssertEquals("ULB_RN_NKConsigneeCountry", "FR", ((ISimplifiedEntryOrganisationDetails)consignee).Country);
		}
	}
}
