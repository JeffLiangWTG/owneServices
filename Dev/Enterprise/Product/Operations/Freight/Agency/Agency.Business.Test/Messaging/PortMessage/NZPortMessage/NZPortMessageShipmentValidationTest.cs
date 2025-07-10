using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	sealed class NZPortMessageShipmentValidationTest : BaseAgencyTest
	{
		#region JS_OA_BookedShippingLineAddressInfo

		public void TestJS_OA_BookedShippingLineAddress_ValueIsEmpty_AddMessageError()
		{
			var shipment = Factory.New<BillOfLading>();

			var validation = new NZPortMessageShipmentValidation(shipment);
			validation.ValidateJS_OA_BookedShippingLineAddress();
			AssertHasMessageErrors("JS_OA_BookedShippingLineAddress", shipment.JS_OA_BookedShippingLineAddressInfo);
		}

		public void TestJS_OA_BookedShippingLineAddress_CodeIsNotSpecified_AddMessageError()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Ukraine);
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OA_BookedShippingLineAddress = organisation.MainAddress.PK;

			var validation = new NZPortMessageShipmentValidation(shipment);
			validation.ValidateJS_OA_BookedShippingLineAddress();
			AssertHasMessageErrors("JS_OA_BookedShippingLineAddress", shipment.JS_OA_BookedShippingLineAddressInfo);
		}

		public void TestJS_OA_BookedShippingLineAddress_CodeIsSpecified_DoNotAddErrorsAndWarnings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.NewZealand);
				var shipment = Factory.New<BillOfLading>();
				shipment.JS_OA_BookedShippingLineAddress = organisation.MainAddress.PK;

				var validation = new NZPortMessageShipmentValidation(shipment);
				validation.ValidateJS_OA_BookedShippingLineAddress();
				AssertNoMessageErrors("JS_OA_BookedShippingLineAddress", shipment.JS_OA_BookedShippingLineAddressInfo);
			}
		}

		#endregion

		#region JS_OH_DeliveryAgent

		public void TestJS_OH_DeliveryAgent_ValueIsEmpty_AddMessageError()
		{
			var shipment = Factory.New<BillOfLading>();

			var validation = new NZPortMessageShipmentValidation(shipment);
			validation.ValidateJS_OH_DeliveryAgent();
			AssertHasMessageErrors("JS_OH_DeliveryAgent", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestJS_OH_DeliveryAgent_CodeIsNotSpecified_AddMessageError()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Ukraine);
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = organisation.PK;

			var validation = new NZPortMessageShipmentValidation(shipment);
			validation.ValidateJS_OH_DeliveryAgent();
			AssertHasMessageErrors("JS_OH_DeliveryAgent", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestJS_OH_DeliveryAgent_CodeIsSpecified_ShouldNotAddErrorsAndWarnings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.NewZealand);
				var shipment = Factory.New<BillOfLading>();
				shipment.JS_OH_DeliveryAgent = organisation.PK;

				var validation = new NZPortMessageShipmentValidation(shipment);
				validation.ValidateJS_OH_DeliveryAgent();
				AssertNoMessageErrors("JS_OH_DeliveryAgent", shipment.JS_OH_DeliveryAgentInfo);
			}
		}

		public void TestJS_OH_DeliveryAgent_DeliveryAgentDoesNotExist()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = ZGuid.NewZGuid();

			var validation = new NZPortMessageShipmentValidation(shipment);
			AssertNoExceptionThrown(() => validation.ValidateJS_OH_DeliveryAgent());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			NZPortMessageValidationStrategy.RegisterForFactory(Factory);
		}

		#endregion

	}
}
