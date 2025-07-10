using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderJobInvoicingAdditionalDataPropertyProvider : WhsJobInvoicingAdditionalDataPropertyProvider
	{
		#region Properties

		protected override CustomPropertyContainer<JobCharge> AddModuleSpecificAdditionalProperties(CustomPropertyContainer<JobCharge> properties)
		{
			base.AddModuleSpecificAdditionalProperties(properties);

			properties.AddCustomProperty(Schema.ConsigneeAddress1, Res.GetString("6f044b10-d669-4349-8479-c11520e54672", "Consignee Address 1"), typeof(ZString), (c) => GetConsigneeAddress1(c), visible: false);
			properties.AddCustomProperty(Schema.ConsigneeAddress2, Res.GetString("23de81a3-59bf-4a54-8fb4-0f1bf4a01542", "Consignee Address 2"), typeof(ZString), (c) => GetConsigneeAddress2(c), visible: false);
			properties.AddCustomProperty(Schema.ConsigneeCity, Res.GetString("ea3f4ba2-5d6b-41f5-abc7-76f5054115a9", "Consignee City"), typeof(ZString), (c) => GetConsigneeCity(c), visible: false);
			properties.AddCustomProperty(Schema.ConsigneePostCode, Res.GetString("116d617b-32d6-454b-8594-fb7d0288cb9e", "Consignee Post Code"), typeof(ZString), (c) => GetConsigneePostCode(c), visible: false);
			properties.AddCustomProperty(Schema.ConsigneeState, Res.GetString("730800fd-ea3d-4a68-a88d-ac97ff647281", "Consignee State"), typeof(ZString), (c) => GetConsigneeState(c), visible: false);
			properties.AddCustomProperty(Schema.ConsigneeUNLOCO, Res.GetString("03693780-0a76-4b10-b0ae-f822b8f3a46a", "Consignee UNLOCO"), typeof(ZString), (c) => GetConsigneeUNLOCO(c), visible: false);
			properties.AddCustomProperty(Schema.ConsigneeCode, Res.GetString("0d515435-3942-455f-86aa-a31b9c8beaff", "Consignee Code"), typeof(ZString), (c) => GetConsigneeCode(c), visible: false);

			return properties;
		}

		#region ConsigneeCode

		static ZString GetConsigneeCode(JobCharge charge)
		{
			var consigneeCode = ZString.Empty;

			var order = GetOrder(charge);
			if (order != null)
			{
				var consigneeDocAddress = order.ConsigneeDocAddress;
				if (consigneeDocAddress.E2_AddressOverride)
				{
					consigneeCode = consigneeDocAddress.E2_CompanyNameTruncated;
				}
				else
				{
					consigneeCode = consigneeDocAddress.Organisation?.OH_Code ?? ZString.Empty;
				}
			}

			return consigneeCode;
		}

		#endregion

		#region ConsigneeAddress1

		static ZString GetConsigneeAddress1(JobCharge charge)
		{
			return GetOrder(charge)?.ConsigneeDocAddress?.E2_Address1 ?? ZString.Empty;
		}

		#endregion

		#region ConsigneeAddress2

		static ZString GetConsigneeAddress2(JobCharge charge)
		{
			return GetOrder(charge)?.ConsigneeDocAddress?.E2_Address2 ?? ZString.Empty;
		}

		#endregion

		#region ConsigneeCity

		static ZString GetConsigneeCity(JobCharge charge)
		{
			return GetOrder(charge)?.ConsigneeDocAddress?.E2_City ?? ZString.Empty;
		}

		#endregion

		#region ConsigneePostCode

		static ZString GetConsigneePostCode(JobCharge charge)
		{
			return GetOrder(charge)?.ConsigneeDocAddress?.E2_Postcode ?? ZString.Empty;
		}

		#endregion

		#region ConsigneeState

		static ZString GetConsigneeState(JobCharge charge)
		{
			return GetOrder(charge)?.ConsigneeDocAddress?.E2_State ?? ZString.Empty;
		}

		#endregion

		#region ConsigneeUNLOCO

		static ZString GetConsigneeUNLOCO(JobCharge charge)
		{
			var consigneeDocAddress = GetOrder(charge)?.ConsigneeDocAddress;
			return consigneeDocAddress != null && !consigneeDocAddress.E2_AddressOverride
				? consigneeDocAddress.Address?.OA_RL_NKRelatedPortCode ?? ZString.Empty
				: ZString.Empty;
		}

		#endregion

		#endregion

		static WhsOrder GetOrder(JobCharge charge) => WhsChargeHelper.GetDocket(charge) as WhsOrder;
	}
}
