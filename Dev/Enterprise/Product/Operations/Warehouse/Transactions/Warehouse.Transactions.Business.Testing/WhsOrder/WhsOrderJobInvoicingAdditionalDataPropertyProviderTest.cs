using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderJobInvoicingAdditionalDataPropertyProviderTest : WhsJobInvoicingAdditionalDataPropertyProviderTest
	{
		#region Properties

		#region TestAllConsigneePropertiesAreEmptyForNonOrder

		public void TestAllConsigneePropertiesAreEmptyForNonOrder()
		{
			var customProperties = GetAdditionalDataProvider.GetAdditionalProperties().CustomProperties.Where(c => c.Identifier.StartsWith("Consignee")).ToArray();
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_CustomerReference = "Customer Reference";
			Helper.CreateRatingJob(receive);
			data.Part1.OP_Weight = 2m;
			data.Part1.OP_WeightUQ = "KG";
			data.Part1.OP_Cubic = 3m;
			data.Part1.OP_CubicUQ = "M3";
			Helper.CreateProductUnit(data.Part1, "PLT", 40);
			Factory.Save();

			var charge = GetChargeWithData(receive.WD_ExternalReference, data.Part1.OP_PartNum, 50m);
			AssertEquals("Consignee only exists on Pickable Dockets, so a Receive job should have Empty Values for each Consignee Property.", true,
				customProperties.Length > 0 && customProperties.All(c => ((IZType)c.GetValue(charge)).IsEmpty));
		}

		#endregion

		#region TestGetConsigneeCode

		public void TestGetConsigneeCode()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCode];
			AssertNotNull("ConsigneeCode should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			var consignee = Helper.CreateClient("CONSIGNEE");

			var order = (WhsOrder)Docket;
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			order.ConsigneeDocAddress.E2_CompanyName = "OVERRIDEN";
			AssertEquals("CONSIGNEE", customProperty.GetValue(charge));

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "OVERRIDEN";
			AssertEquals("OVERRIDEN", customProperty.GetValue(charge));
		}

		#endregion

		#region TestGetConsigneeAddress1

		public void TestGetConsigneeAddress1()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress1];
			AssertNotNull("ConsigneeAddress1 should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			SetupMainOrgAddress(consignee, "Addr1", "Addr2", "City", "12345", "State", "UAIEV");
			AssertEquals("#1", customProperty.GetValue(charge));

			((WhsOrder)Docket).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("Addr1", customProperty.GetValue(charge));
		}

		#endregion

		#region TestGetConsigneeAddress2

		public void TestGetConsigneeAddress2()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress2];
			AssertNotNull("ConsigneeAddress2 should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			SetupMainOrgAddress(consignee, "Addr1", "Addr2", "City", "12345", "State", "UAIEV");
			AssertEquals("", customProperty.GetValue(charge));

			((WhsOrder)Docket).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("Addr2", customProperty.GetValue(charge));
		}

		#endregion

		#region TestGetConsigneeCity

		public void TestGetConsigneeCity()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCity];
			AssertNotNull("ConsigneeCity should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			SetupMainOrgAddress(consignee, "Addr1", "Addr2", "City", "12345", "State", "UAIEV");
			AssertEquals("", customProperty.GetValue(charge));

			((WhsOrder)Docket).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("City", customProperty.GetValue(charge));
		}

		#endregion

		#region TestGetConsigneePostCode

		public void TestGetConsigneePostCode()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneePostCode];
			AssertNotNull("ConsigneePostCode should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			SetupMainOrgAddress(consignee, "Addr1", "Addr2", "City", "12345", "State", "UAIEV");
			AssertEquals("", customProperty.GetValue(charge));

			((WhsOrder)Docket).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("12345", customProperty.GetValue(charge));
		}

		#endregion

		#region TestGetConsigneeState

		public void TestGetConsigneeState()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeState];
			AssertNotNull("ConsigneeState should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			SetupMainOrgAddress(consignee, "Addr1", "Addr2", "City", "12345", "State", "UAIEV");
			AssertEquals("", customProperty.GetValue(charge));

			((WhsOrder)Docket).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("State", customProperty.GetValue(charge));
		}

		#endregion

		#region TestGetConsigneeUNLOCO

		public void TestGetConsigneeUNLOCO()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeUNLOCO];
			AssertNotNull("ConsigneeUNLOCO should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			SetupMainOrgAddress(consignee, "Addr1", "Addr2", "City", "12345", "State", "UAIEV");
			AssertEquals("", customProperty.GetValue(charge));

			((WhsOrder)Docket).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("UAIEV", customProperty.GetValue(charge));

			((WhsOrder)Docket).ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals("", customProperty.GetValue(charge));

			((WhsOrder)Docket).ConsigneeDocAddress.E2_AddressOverride = false;
			AssertEquals("UAIEV", customProperty.GetValue(charge));
		}

		#endregion

		#region Implementation

		void SetupMainOrgAddress(OrgHeader org, ZString address1, ZString address2, ZString city, ZString postCode, ZString state, ZString relatedPortCode)
		{
			OrgAddress address = org.MainAddress;
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_State = state;
			address.OA_RL_NKRelatedPortCode = relatedPortCode;
		}

		protected override WhsDocket GetNewDocket(OrgHeader client, WhsWarehouse whs)
		{
			return Helper.CreateWhsOrder(client, whs);
		}

		protected override WhsJobInvoicingAdditionalDataPropertyProvider GetAdditionalDataProvider
		{
			get { return new WhsOrderJobInvoicingAdditionalDataPropertyProvider(); }
		}

		#endregion

		#endregion
	}
}
