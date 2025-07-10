using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal sealed class NZReleaseOrderShipmentValidationTest : ReleaseImportOrderShipmentValidationTest
	{
		#region CheckJS_OA_BookedShippingLineAddress

		public void TestJS_OA_BookedShippingLineAddress_ValueIsEmpty_AddMessageError()
		{
			var shipment = Factory.New<BillOfLading>();

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_OA_BookedShippingLineAddress();
			AssertHasMessageErrors("JS_OA_BookedShippingLineAddress", shipment.JS_OA_BookedShippingLineAddressInfo);
		}

		public void TestJS_OA_BookedShippingLineAddress_DischargePortIsEmpty_AddMessageError()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Ukraine);
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OA_BookedShippingLineAddress = organisation.MainAddress.PK;
			shipment.JS_NKDischargePort = string.Empty;

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_OA_BookedShippingLineAddress();
			AssertHasMessageErrors("JS_OA_BookedShippingLineAddress", shipment.JS_OA_BookedShippingLineAddressInfo);
		}

		public void TestJS_OA_BookedShippingLineAddress_CodeIsNotMatched_AddMessageError()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Ukraine);
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OA_BookedShippingLineAddress = organisation.MainAddress.PK;
			shipment.JS_NKDischargePort = "AUSYD";

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_OA_BookedShippingLineAddress();
			AssertHasMessageErrors("JS_OA_BookedShippingLineAddress", shipment.JS_OA_BookedShippingLineAddressInfo);
		}

		public void TestJS_OA_BookedShippingLineAddress_CodeIsMatched_DoNotAddErrorsAndWarnings()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.NewZealand);
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OA_BookedShippingLineAddress = organisation.MainAddress.PK;
			shipment.JS_NKDischargePort = "NZABY";

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_OA_BookedShippingLineAddress();
			AssertNoMessageErrors("JS_OA_BookedShippingLineAddress", shipment.JS_OA_BookedShippingLineAddressInfo);
		}

		#endregion

		#region JS_OH_DeliveryAgent

		public void TestJS_OH_DeliveryAgent_ValueIsEmpty_AddMessageError()
		{
			var shipment = Factory.New<BillOfLading>();

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_OH_DeliveryAgent();
			AssertHasMessageErrors("JS_OH_DeliveryAgent", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestJS_OH_DeliveryAgent_DischargePortIsEmpty_AddMessageError()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Ukraine);
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = organisation.PK;
			shipment.JS_NKDischargePort = string.Empty;

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_OH_DeliveryAgent();
			AssertHasMessageErrors("JS_OH_DeliveryAgent", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestJS_OH_DeliveryAgent_CodeIsNotMatched_AddMessageError()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Ukraine);
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = organisation.PK;
			shipment.JS_NKDischargePort = "AUSYD";

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_OH_DeliveryAgent();
			AssertHasMessageErrors("JS_OH_DeliveryAgent", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestJS_OH_DeliveryAgent_CodeIsMatched_ShouldNotAddErrorsAndWarnings()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.NewZealand);
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = organisation.PK;
			shipment.JS_NKDischargePort = "NZABY";

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_OH_DeliveryAgent();
			AssertNoMessageErrors("JS_OH_DeliveryAgent", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestJS_OH_DeliveryAgent_DeliveryAgentDoesNotExist()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = ZGuid.NewZGuid();

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			AssertNoExceptionThrown(() => validation.ValidateJS_OH_DeliveryAgent());
		}

		#endregion

		#region JS_NKDischargePort

		public void TestJS_NKDischargePort_PortIsNotEnabled_DoNotValidate()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new PortMessagingPortCollection()))
				{
					var voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
					voyage.GenerateSailings();

					var shipment = Factory.New<BillOfLading>();
					shipment.JS_JX = voyage.Sailings[0].PK;

					var validation = new NZReleaseOrderShipmentValidation(shipment);
					validation.ValidateJS_NKDischargePort();

					AssertNoMessageErrors("JS_NKDischargePort", shipment.JS_NKDischargePortInfo);
				}
			}
		}

		public void TestJS_NKDischargePort_PortIsEnabled_AddMessageErrorIfNoCTO()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();

				var portMessagingPort = importReleaseOrderPorts.AddNew();
				portMessagingPort.Port = "NZAKL";
				portMessagingPort.SenderID = "SenderID";
				portMessagingPort.Enabled = true;

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				{
					var voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
					voyage.GenerateSailings();

					var shipment = Factory.New<BillOfLading>();
					shipment.JS_JX = voyage.Sailings[0].PK;

					var validation = new NZReleaseOrderShipmentValidation(shipment);

					voyage.Destinations[0].JB_Calc_ArrivalCTOAddressOrg = ZGuid.Empty;
					validation.ValidateJS_NKDischargePort();
					AssertHasMessageErrors("JS_NKDischargePort", shipment.JS_NKDischargePortInfo);

					voyage.Destinations[0].JB_Calc_ArrivalCTOAddressOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
					validation.ValidateJS_NKDischargePort();
					AssertNoMessageErrors("JS_NKDischargePort", shipment.JS_NKDischargePortInfo);
				}
			}
		}

		#endregion

		#region Implementation

		GlbBranch NewZelandBranch
		{
			get
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_RL_NKHomePort = "NZAKL";
				branch.Company.GC_RN_NKCountryCode = "NZ";

				Factory.Save();

				return branch;
			}
		}

		#endregion
	}
}
