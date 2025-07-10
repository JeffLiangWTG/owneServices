using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport
{
	public class ImportCollectionInfoImplForOrgFlattened : ImportCollectionInfoImpl
	{
		public ImportCollectionInfoImplForOrgFlattened(IBusinessObjectCollection collection)
			: base(collection)
		{
			var orgCasing = Env.Registry.OrgAllowMixedCase ? ZCharacterCasing.Normal : ZCharacterCasing.Upper;

			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_Code) { HeaderText = Res.GetString("6ccd23cd-524c-4ca9-9016-795c99be7c4c", "Code"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_FullName) { HeaderText = Res.GetString("8d6e15b4-77da-444b-a210-5852795d15f7", "Name"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_Address1, true) { HeaderText = Res.GetString("d96e037e-0c0f-479b-97b1-78adf05b194e", "Address 1"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_Address2) { HeaderText = Res.GetString("d92ef1d6-1ff5-4c6e-80a0-235c43fa0dc2", "Address 2"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_AdditionalAddressInformation) { HeaderText = Res.GetString("f4bf7d4c-30c9-4422-9857-a80dd77b2fba", "Additional Address Info"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_City) { HeaderText = Res.GetString("b06f2863-b745-44ff-bff7-66589228fabb", "City"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_State) { HeaderText = Res.GetString("524a8b93-0736-497b-9925-53f7b967afff", "State"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_PostCode) { HeaderText = Res.GetString("b9dbaa13-487d-40ff-9068-24c59bfed820", "Postcode"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_RL_NKClosestPort) { HeaderText = "UNLOCO", CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Country, true) { HeaderText = Res.GetString("3412304b-7096-4c46-9571-52f97f526ff0", "Country/Region"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.City) { HeaderText = Res.GetString("3e8f1714-58e8-40d6-96d3-77e676f3ca21", "Port City"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_Phone) { HeaderText = Res.GetString("46c0dc95-e9df-43a5-8148-a8a485365cad", "Phone"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_Fax) { HeaderText = Res.GetString("a70fcb6c-4c98-44d3-be8d-25ebc9e08e14", "Fax"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_Email) { HeaderText = Res.GetString("77c003b2-43c7-4b49-bc44-dbc8f27c0fca", "Email") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.PU_URL) { HeaderText = Res.GetString("4ef1fe9c-0151-4c36-91a7-71d015d78d14", "Web") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.BusRegNo) { HeaderText = Res.GetString("55066f81-37a8-4334-a2e9-c36ea2b9c916", "Business Registration Number"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.BusRegACN) { HeaderText = Res.GetString("0a95bd00-bfff-41a2-b8e3-0d6fda59147c", "Government Corporation Code"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OB_IsDebtor) { HeaderText = Res.GetString("49500535-d060-4c20-80c4-04d42c347bee", "Debtor") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OB_IsCreditor) { HeaderText = Res.GetString("8543e01e-ba8a-4788-8bcf-d7adc1f9b288", "Creditor") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsConsignee) { HeaderText = Res.GetString("b7d73ba8-e087-48cd-9551-d652e2b4ed16", "Consignee") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsConsignor) { HeaderText = Res.GetString("2be81ea0-e435-4369-9f00-48949aefbf61", "Consignor") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsForwarder) { HeaderText = Res.GetString("c77c42a4-1580-4c5d-9a15-ee11b914e7ef", "Forwarder") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsBroker) { HeaderText = Res.GetString("768541e9-19ec-41af-a60a-18074298c4f1", "Broker") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsShippingProvider) { HeaderText = Res.GetString("b953baaf-60b2-46f8-a4b6-2cf3de4e0fe5", "Carrier") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsShippingLine) { HeaderText = Res.GetString("bb65687c-a55c-4aa9-b04c-8495bed1297b", "Ship Line") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsAirLine) { HeaderText = Res.GetString("b0cbb692-8aa9-4513-8ee0-47f95c3fe84a", "Airline") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsLocalTransport) { HeaderText = Res.GetString("fb014f15-c779-4511-87fd-c42f5f99eed8", "Port Transport") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsSalesLead) { HeaderText = Res.GetString("27c122e9-c670-4279-b9a6-596c7dc33213", "Sales Lead") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsMiscFreightServices) { HeaderText = Res.GetString("8021bf77-67d8-4199-9550-7aa91d1cd355", "Services") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsCompetitor) { HeaderText = Res.GetString("89a326b4-e7d5-4d93-9382-ff0c7bdc20cc", "Competitor") });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_ContactName) { HeaderText = Res.GetString("20169e89-333a-4a35-8982-ad8597a06430", "Contact Name") });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_Title) { HeaderText = Res.GetString("9be88da5-e830-4fd8-bdb7-56e9b960c610", "Contact Job Title") });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_Email) { HeaderText = Res.GetString("d7e33d71-51e0-40a2-8e9f-10b991d2290c", "Contact Email") });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_Phone) { HeaderText = Res.GetString("71979b6b-bd09-4d8e-ae77-2299aa80e572", "Contact Phone"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_Mobile) { HeaderText = Res.GetString("98ca7d55-800b-4335-96d3-13c65c4f4666", "Contact Mobile"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_Fax) { HeaderText = Res.GetString("7f111af1-e102-4968-b1ab-093e20393902", "Contact Fax"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Debtor) { HeaderText = Res.GetString("991fa81c-8ebb-41fd-97a3-fb161d3e8c99", "Debtor Code"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.DebtorGroup) { HeaderText = Res.GetString("bfe3d019-5a55-4838-bb9d-059d61f8cc75", "Debtor Group"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.DebtorSettlementGroup) { HeaderText = Res.GetString("282b6c47-87b9-4e8a-a514-ebd16705b888", "Debtor Settlement Group"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.CurrencyCode) { HeaderText = Res.GetString("8f530b76-feca-4932-9f2e-7220f2fcd00e", "Currency"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.CreditLimit) { HeaderText = Res.GetString("bf002091-df3d-4490-97ed-bd1b7d4ea904", "Credit Limit") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OM_ARCreditRating) { HeaderText = Res.GetString("aa8c8e87-d881-4492-97f5-6bf1821083fa", "Credit Rating"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.GSTApplicable) { HeaderText = "GST" });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.PY_InvoiceTerm) { HeaderText = Res.GetString("51fa5675-662a-4b03-bd94-9142c792f0f8", "Inv. Terms Standard") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.PY_InvoiceDays) { HeaderText = Res.GetString("fb14b343-df7c-45b5-b711-46b17848e8ee", "Inv. Days Standard") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.InvoiceTermDisbursement) { HeaderText = Res.GetString("5caf040f-8d63-417d-b823-7d2a016bd2ad", "Inv. Terms Disbursement") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.InvoiceDaysDisbursement) { HeaderText = Res.GetString("600a38b8-47cd-43a9-b7db-47dacbd37e8a", "Inv. Days Disbursement") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.CreditorGroup) { HeaderText = Res.GetString("466ac256-4caf-4129-8958-832d6f8bc22b", "Creditor Group"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.CustomsAgent) { HeaderText = Res.GetString("00837853-ff5d-482b-b7f6-520ff2280e51", "Customs Agent"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.PostalAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Postal_OA_Address1) { HeaderText = Res.GetString("75b2b812-2728-425f-8616-c505ebb4a45c", "Postal Address 1"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.PostalAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Postal_OA_Address2) { HeaderText = Res.GetString("1c154c7e-c826-44f6-ab67-8c2f84fb4a03", "Postal Address 2"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.PostalAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Postal_OA_City) { HeaderText = Res.GetString("2e74cbd4-bff1-4dea-8dab-1654196255f9", "Postal City"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.PostalAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Postal_OA_State) { HeaderText = Res.GetString("85efe5d5-b190-4a97-abfd-923c984208d3", "Postal State"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.PostalAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Postal_OA_PostCode) { HeaderText = Res.GetString("a922fe07-95fb-4da1-b2f4-8073e36e1e69", "Postal Postcode"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.DeliveryAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Delivery_OA_Address1) { HeaderText = Res.GetString("d1735766-aaa5-4f91-ad91-638e57713b79", "Delivery Address 1"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.DeliveryAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Delivery_OA_Address2) { HeaderText = Res.GetString("2a054486-bd3f-4f5a-8429-f9131166337d", "Delivery Address 2"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.DeliveryAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Delivery_OA_City) { HeaderText = Res.GetString("95baec73-7210-4479-9def-262228709039", "Delivery City"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.DeliveryAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Delivery_OA_State) { HeaderText = Res.GetString("3cda76b8-321d-405a-bca7-be43d3bb6033", "Delivery State"), CharacterCasing = orgCasing });
			AddProperty(ChildProperty.DeliveryAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Delivery_OA_PostCode) { HeaderText = Res.GetString("3ee7493f-2c8a-4955-9e36-8076bc080408", "Delivery Postcode"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.A1_BankName) { HeaderText = Res.GetString("02a6180c-0369-427f-92b8-60118802b554", "Bank") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.A1_AccountName) { HeaderText = Res.GetString("cb8f56ad-4ce4-4091-8da1-e02d1c24fef5", "Account Name") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.A1_BankAccount) { HeaderText = Res.GetString("aaaa8071-83c3-4b9b-81a2-46b83c7c4824", "Account Number") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.A1_BankBsb) { HeaderText = "BSB" });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.CustomsCode) { HeaderText = "CCD" });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.SupplierCode) { HeaderText = "CSC" });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.CMRSupplierCode) { HeaderText = "SCC" });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.PremiseID) { HeaderText = "CPP" });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.CarrierCode) { HeaderText = "CCC" });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.ManifestProviderCode) { HeaderText = "CMP" });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.WorkNotes) { HeaderText = Res.GetString("45858ca3-991a-4ecd-ae90-36ed3c162adb", "Work Notes") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.ForwardingNotes) { HeaderText = Res.GetString("18226c5b-1525-4a6d-8853-3eb90f2b6ba7", "Handling Notes") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.DeliveryNotes) { HeaderText = Res.GetString("d21ac09d-368c-49ba-bffd-1c6b9f8bd71f", "Delivery Notes") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.ARNotes) { HeaderText = Res.GetString("7e52c857-6451-48f2-96c4-6bbb4847498c", "AR Notes") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.InvoiceNotes) { HeaderText = Res.GetString("0998c84d-e6a1-4b17-b65a-dc978c94e8d4", "AR Credit Notes") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.APNotes) { HeaderText = Res.GetString("dfa5b71e-4f41-4f35-99a5-90cdfa7a44e3", "AP Notes") });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_ContactSource) { HeaderText = Res.GetString("24c6ef9d-c596-45d8-8d80-b38838fe1db2", "Contact Source Type") });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_DetailsVerified) { HeaderText = Res.GetString("06477f32-fab5-46ff-94cc-99a346d8ba15", "Contact Date Details Verified") });
			AddProperty(ChildProperty.Contact, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OC_Salutation) { HeaderText = Res.GetString("e209624b-c1d1-4261-9185-f69f337016a8", "Contact Salutation") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_Language) { HeaderText = Res.GetString("783a41aa-0a2a-40a2-ae86-d8ca12dba167", "Language"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.MainAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OA_Language) { HeaderText = Res.GetString("7bfb1cd1-58e9-4bfb-9175-b2f9a6107d29", "Main Address Language"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.PostalAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Postal_OA_Language) { HeaderText = Res.GetString("b3f939d6-5972-42a9-8fc3-e1a65cb05fb0", "Postal Address Language"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(ChildProperty.DeliveryAddress, new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.Delivery_OA_Language) { HeaderText = Res.GetString("aa79b4cc-7c8d-4225-a4bd-3fd944b22cdf", "Delivery Address Language"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.A1_RX_NKAccountCurrency) { HeaderText = Res.GetString("01e7b117-4f9c-4d00-8d4b-7b1e89a3144b", "Bank Currency"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsWarehouseClient) { HeaderText = Res.GetString("98a29b91-b78e-4bc6-9b9e-4924d160ddaa", "Warehouse") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsControllingAgent) { HeaderText = Res.GetString("26624387-1505-41bf-bdc7-28604f3c4d6b", "Controlling Agent") });
			AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(AutoOrgFlattened.Schema.OH_IsControllingCustomer) { HeaderText = Res.GetString("fdd36155-730d-4133-8d40-9c02ca43d1e7", "Controlling Customer") });

			#region Reg Properties
			for (int i = 1; i <= 99; ++i)
			{
				string propertyName = "RegDetail" + i;
				string headerText = Res.GetString("4f13b8c0-86d6-41e6-9b5c-9431902a49ed", "Registration Number {0}", i);
				AddProperty(new ImportPropertyInfoImpl<OrgFlattened>(propertyName) { HeaderText = headerText });
			}
			#endregion
		}

		readonly IList<IImportPropertyInfo> headerProperties = new List<IImportPropertyInfo>();
		public IEnumerable<IImportPropertyInfo> HeaderProperties
		{
			get { return headerProperties; }
		}

		void AddProperty(IImportPropertyInfo property)
		{
			headerProperties.Add(property);

			Add(property);
		}

		readonly Dictionary<ChildProperty, IList<IImportPropertyInfo>> childImportPropertyInfosLookup = new Dictionary<ChildProperty, IList<IImportPropertyInfo>>();
		public IList<IImportPropertyInfo> GetChildImportPropertyInfos(ChildProperty child)
		{
			return childImportPropertyInfosLookup[child];
		}

		void AddProperty(ChildProperty child, IImportPropertyInfo property)
		{
			IList<IImportPropertyInfo> childProperties;
			if (!childImportPropertyInfosLookup.TryGetValue(child, out childProperties))
			{
				childProperties = new List<IImportPropertyInfo>();
				childImportPropertyInfosLookup[child] = childProperties;
			}
			childProperties.Add(property);

			Add(property);
		}

		public enum ChildProperty
		{
			MainAddress,
			PostalAddress,
			DeliveryAddress,
			Contact
		}
	}
}
