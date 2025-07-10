#nullable enable
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentJobInvoicingAdditionalDataPropertyProvider
	{
		public const string DocketReference = "DocketReference";
		public const string DocketID = "DocketID";
		public const string CustomerReference = "CustomerReference";
		public const string ConsigneeAddress1 = "ConsigneeAddress1";
		public const string ConsigneeAddress2 = "ConsigneeAddress2";
		public const string ConsigneeCity = "ConsigneeCity";
		public const string ConsigneePostCode = "ConsigneePostCode";
		public const string ConsigneeState = "ConsigneeState";
		public const string ConsigneeUNLOCO = "ConsigneeUNLOCO";
		public const string ConsigneeCode = "ConsigneeCode";

		public CustomPropertyContainer<JobCharge> GetAdditionalProperties()
		{
			var properties = new CustomPropertyContainer<JobCharge>();

			properties.AddCustomProperty(DocketReference, Res.GetString("7d432036-fabd-4765-9a3a-63fbea5c8b30", "Job Reference"), typeof(ZString), valueGetter: (c) => GetJobReference(c), visible: false);
			properties.AddCustomProperty(DocketID, Res.GetString("577596e1-e47d-48d0-b5e3-f9790e798e6d", "Job ID"), typeof(ZString), (c) => GetJobID(c), visible: false);
			properties.AddCustomProperty(CustomerReference, Res.GetString("1249b2b6-3f9e-4280-abbc-c56111c4ec25", "Customer Reference"), typeof(ZString), (c) => GetCustomerReference(c), visible: false);
			properties.AddCustomProperty(ConsigneeAddress1, Res.GetString("9fda1fa1-778f-475c-89f4-19d5a6d34ff2", "Consignee Address 1"), typeof(ZString), (c) => GetConsigneeAddress1(c), visible: false);
			properties.AddCustomProperty(ConsigneeAddress2, Res.GetString("7854f051-b439-4a3a-a757-5ef1690c414f", "Consignee Address 2"), typeof(ZString), (c) => GetConsigneeAddress2(c), visible: false);
			properties.AddCustomProperty(ConsigneeCity, Res.GetString("107dee58-34e8-46b9-ac10-c24481460973", "Consignee City"), typeof(ZString), (c) => GetConsigneeCity(c), visible: false);
			properties.AddCustomProperty(ConsigneePostCode, Res.GetString("48eef572-dff0-4abc-a047-e5bfe58d20a0", "Consignee Post Code"), typeof(ZString), (c) => GetConsigneePostCode(c), visible: false);
			properties.AddCustomProperty(ConsigneeState, Res.GetString("4598ed09-2f7f-4d57-8e69-1c9067ebff63", "Consignee State"), typeof(ZString), (c) => GetConsigneeState(c), visible: false);
			properties.AddCustomProperty(ConsigneeUNLOCO, Res.GetString("c6fa0766-2047-4879-b1fe-66972a1a1cb5", "Consignee UNLOCO"), typeof(ZString), (c) => GetConsigneeUNLOCO(c), visible: false);
			properties.AddCustomProperty(ConsigneeCode, Res.GetString("0035d94e-8de7-4552-870e-587b164ddb0e", "Consignee Code"), typeof(ZString), (c) => GetConsigneeCode(c), visible: false);

			return properties;
		}

		ZString GetJobID(JobCharge charge)
		{
			var jobID = ZString.Empty;
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);

			if (consignment == null)
			{
				return ZString.Empty;
			}

			var transportBooking = consignment.Factory.Load<DtbBooking>(consignment.LTC_KM_Booking);

			jobID = transportBooking?.ParentJob?.JobNumber ?? ZString.Empty;
			if (jobID.IsEmpty)
			{
				jobID = transportBooking?.KM_JobID ?? ZString.Empty;
			}
			if (jobID.IsEmpty)
			{
				jobID = consignment.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference).FirstOrDefault();
			}

			return jobID;
		}

		ZString GetJobReference(JobCharge charge)
		{
			var jobReference = ZString.Empty;
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);

			if (consignment != null)
			{
				jobReference = consignment.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).FirstOrDefault();
			}
			return jobReference;
		}

		ZString GetCustomerReference(JobCharge charge)
		{
			var customerReference = ZString.Empty;
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);

			if (consignment != null)
			{
				customerReference = consignment.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber).FirstOrDefault();
			}
			return customerReference;
		}

		ZString GetConsigneeAddress1(JobCharge charge)
		{
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);
			if (consignment == null)
			{
				return ZString.Empty;
			}

			return GetConsigneeAddress(consignment)?.E2_Address1 ?? ZString.Empty;
		}

		ZString GetConsigneeAddress2(JobCharge charge)
		{
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);
			if (consignment == null)
			{
				return ZString.Empty;
			}

			return GetConsigneeAddress(consignment)?.E2_Address2 ?? ZString.Empty;
		}

		ZString GetConsigneeCity(JobCharge charge)
		{
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);
			if (consignment == null)
			{
				return ZString.Empty;
			}

			return GetConsigneeAddress(consignment)?.E2_City ?? ZString.Empty;
		}

		ZString GetConsigneePostCode(JobCharge charge)
		{
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);
			if (consignment == null)
			{
				return ZString.Empty;
			}

			return GetConsigneeAddress(consignment)?.E2_Postcode ?? ZString.Empty;
		}

		ZString GetConsigneeState(JobCharge charge)
		{
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);
			if (consignment == null)
			{
				return ZString.Empty;
			}

			return GetConsigneeAddress(consignment)?.E2_State ?? ZString.Empty;
		}

		ZString GetConsigneeUNLOCO(JobCharge charge)
		{
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);
			if (consignment == null)
			{
				return ZString.Empty;
			}

			var consigneeJobDocAddress = GetConsigneeAddress(consignment);
			return consigneeJobDocAddress != null && !consigneeJobDocAddress.E2_AddressOverride
				? consigneeJobDocAddress.Address?.OA_RL_NKRelatedPortCode ?? ZString.Empty
				: ZString.Empty;
		}

		ZString GetConsigneeCode(JobCharge charge)
		{
			var consignment = DtbConsignmentChargeHelper.GetConsignment(charge);
			if (consignment == null)
			{
				return ZString.Empty;
			}

			return GetConsigneeAddress(consignment)?.Organisation?.OH_Code ?? ZString.Empty;
		}

		JobDocAddress? GetConsigneeAddress(DtbConsignment consignment)
		{
			var deliveryAddress = consignment.DeliveryAddress;
			if (deliveryAddress != null)
			{
				return deliveryAddress.Address;
			}
			return null;
		}
	}
}
