//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSADHFormDataValidation
//
//    This class should be used for overriding validation in AutoSADHFormDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.SADH
{
	public class SADHFormDataValidation : AutoSADHFormDataValidation
	{
		public SADHFormDataValidation(AutoSADHFormData parent)
			: base(parent)
		{
			formData = (SADHFormData)parent;
		}
		readonly SADHFormData formData;

		#region ExchangeRate Validation
		public void ValidateExchangeRate()
		{
			ValidateCalculatedProperty(formData.ExchangeRateInfo);
		}

		protected void CheckExchangeRate()
		{
			if (formData.D1_MessageType == JobMessageTypeList.Codes.Export)
			{
				if (formData.ExchangeRate == ZDecimal.Zero)
				{
					formData.ExchangeRateInfo.AddMessageError(ExchangeRateZero);
				}
			}
		}
		public static string ExchangeRateZero
		{
			get { return Res.GetString("e614467e-2a7d-4f1f-bb16-6eca0b091af6", "There is no valid exchange rate for this Currency. Please check with your system administrator to ensure that your system is downloading customs exchange rate correctly."); }
		}
		#endregion

		protected override void CheckD1_MessageType()
		{
			base.CheckD1_MessageType();

			if (formData.D1_MessageTypeInfo.Value.IsEmpty)
			{
				formData.D1_MessageTypeInfo.AddMessageError(D1_MessageTypeEmpty);
			}

			ListValidation.MessageErrorIfInvalidCode(formData.D1_MessageTypeInfo, formData.Lookups.MessageTypeList);
			ValidateD1_RL_NKCountryOfDestination();
			ValidateD1_RL_NKCountryOfOrigin();
		}
		public static string D1_MessageTypeEmpty
		{
			get { return Res.GetString("0df79b2d-d1b0-4cf1-871c-2fbdb042f59a", "Please enter a Message Type."); }
		}

		protected override void CheckD1_ModeOfTransportAtTheBorder()
		{
			base.CheckD1_ModeOfTransportAtTheBorder();

			if (formData.D1_ModeOfTransportAtTheBorder.IsEmpty)
			{
				formData.D1_ModeOfTransportAtTheBorderInfo.AddMessageError(D1_ModeTransportAtBorderEmpty);
			}

			ListValidation.MessageErrorIfInvalidCode(formData.D1_ModeOfTransportAtTheBorderInfo, formData.Lookups.ModeOfTransportAtBorderList);
			ValidateD1_FlightDate();
			ValidateD1_FlightNo();
			ValidateD1_VesselCode();
		}
		public static string D1_ModeTransportAtBorderEmpty
		{
			get { return Res.GetString("c7c324d1-4618-46ab-8346-ecab071bb97a", "Please enter a Mode of Transport."); }
		}

		protected override void CheckD1_CommodityCode()
		{
			base.CheckD1_CommodityCode();

			if (formData.D1_CommodityCode.IsEmpty)
			{
				formData.D1_CommodityCodeInfo.AddMessageError(D1_CommodityCodeEmpty);
			}
		}
		public static string D1_CommodityCodeEmpty
		{
			get { return Res.GetString("cd17a3e6-ab30-42d8-afd2-c7644cb97d04", "Please enter a Commodity Code."); }
		}

		protected override void CheckD1_DescriptionOfGoods()
		{
			base.CheckD1_DescriptionOfGoods();

			if (formData.D1_DescriptionOfGoods.IsEmpty)
			{
				formData.D1_DescriptionOfGoodsInfo.AddWarning(D1_PackagesDescriptionGoodsEmpty);
			}
		}
		public static string D1_PackagesDescriptionGoodsEmpty
		{
			get { return Res.GetString("5a369938-8182-418d-a82f-418f8a171d57", "Please enter a Description of Goods."); }
		}

		protected override void CheckD1_RL_NKPlaceOfUnloading()
		{
			base.CheckD1_RL_NKPlaceOfUnloading();

			if (formData.D1_RL_NKPlaceOfUnloading.IsEmpty)
			{
				formData.D1_RL_NKPlaceOfUnloadingInfo.AddMessageError(D1_RL_NKPlaceUnloadingEmpty);
			}

			ListValidation.MessageErrorIfInvalidCode(formData.D1_RL_NKPlaceOfUnloadingInfo, formData.Lookups.DischargeList, D1_RL_NKPlaceUnloadingIsInvalid);
		}
		public static string D1_RL_NKPlaceUnloadingEmpty
		{
			get { return Res.GetString("4c10ad7e-8028-44d1-bd52-fb3641f452ab", "Please enter a Place of Unloading."); }
		}
		public static IMultilingualString D1_RL_NKPlaceUnloadingIsInvalid
		{
			get { return ResString.GetMultilingualString("3511f18a-8a34-41ff-98c0-9f9999934d47", "Please enter a valid Place of Unloading."); }
		}

		protected override void CheckD1_OH_ConsigneeIsNotEmpty()
		{
			if (formData.D1_OH_Consignee.IsEmpty)
			{
				formData.D1_OH_ConsigneeInfo.AddMessageError(D1_OH_ConsigneeEmpty);
			}
		}
		public static string D1_OH_ConsigneeEmpty
		{
			get { return Res.GetString("405c6708-87c6-4dce-902f-59989104e893", "Please enter a Consignee."); }
		}

		protected override void CheckD1_OH_ConsignorIsNotEmpty()
		{
			if (formData.D1_OH_Consignor.IsEmpty)
			{
				formData.D1_OH_ConsignorInfo.AddMessageError(D1_OH_ConsignorEmpty);
			}
		}
		public static string D1_OH_ConsignorEmpty
		{
			get { return Res.GetString("6e3b6cd3-2826-4a67-a2ac-0038237f910b", "Please enter a Consignor."); }
		}

		protected override void CheckD1_OH_ConsigneeIsValidZGuid()
		{
			ListValidation.ErrorIfInvalidPK(formData.D1_OH_ConsigneeInfo, formData.Lookups.ImportersList, D1_OH_ConsigneeInvalid);
		}
		public static MultilingualString D1_OH_ConsigneeInvalid
		{
			get { return ResString.GetMultilingualString("98250e5a-b5e9-4c55-ac49-f9c5bf3af3db", "Please enter a valid Consignee."); }
		}

		protected override void CheckD1_OH_ConsignorIsValidZGuid()
		{
			ListValidation.ErrorIfInvalidPK(formData.D1_OH_ConsignorInfo, formData.Lookups.SuppliersList, D1_OH_ConsignorInvalid);
		}
		public static MultilingualString D1_OH_ConsignorInvalid
		{
			get { return ResString.GetMultilingualString("dbb3411d-130e-425d-8b61-17480d813594", "Please enter a valid Consignor."); }
		}

		protected override void CheckD1_RX_InvoiceCurrency()
		{
			base.CheckD1_RX_InvoiceCurrency();
			ValidateExchangeRate();
		}
		protected override void CheckD1_RX_InvoiceCurrencyIsNotEmpty()
		{
			if (formData.D1_RX_InvoiceCurrency.IsEmpty)
			{
				formData.D1_RX_InvoiceCurrencyInfo.AddError(D1_RX_InvoiceCurrencyEmpty);
			}
		}
		public static string D1_RX_InvoiceCurrencyEmpty
		{
			get { return Res.GetString("23509096-84d0-4c9f-b589-d83ce90cf631", "Please enter a Currency for this Invoice."); }
		}

		protected override void CheckD1_RX_InvoiceCurrencyIsValidZGuid()
		{
			ListValidation.ErrorIfInvalidPK(formData.D1_RX_InvoiceCurrencyInfo, formData.Lookups.CurrencyList, D1_RX_InvoiceCurrencyInvalid);
		}
		public static MultilingualString D1_RX_InvoiceCurrencyInvalid
		{
			get { return ResString.GetMultilingualString("e7c2f26d-1b85-42dd-99dd-7896b8fee68f", "Please enter a valid Invoice Currency."); }
		}

		protected override void CheckD1_RL_NKCountryOfDispatchOrExport()
		{
			base.CheckD1_RL_NKCountryOfDispatchOrExport();

			if (formData.D1_RL_NKCountryOfDispatchOrExport.IsEmpty)
			{
				formData.D1_RL_NKCountryOfDispatchOrExportInfo.AddMessageError(D1_RL_NKCDispExpCodeEmpty);
			}

			ListValidation.MessageErrorIfInvalidCode(formData.D1_RL_NKCountryOfDispatchOrExportInfo, formData.Lookups.OriginList, D1_RL_NKCDispExpCodeIsInvalid);
		}
		public static string D1_RL_NKCDispExpCodeEmpty
		{
			get { return Res.GetString("2140efe4-fc5e-4807-993f-11e152a6f31e", "Please enter a Country of Dispatch/Export."); }
		}
		public static IMultilingualString D1_RL_NKCDispExpCodeIsInvalid
		{
			get { return ResString.GetMultilingualString("8d93c126-3f71-49f5-bbf1-9e5a60f7201d", "Please enter a valid Country of Dispatch/Export."); }
		}

		protected override void CheckD1_RL_NKCountryOfOrigin()
		{
			base.CheckD1_RL_NKCountryOfOrigin();

			if (formData.D1_MessageType == JobMessageTypeList.Codes.Import)
			{
				if (formData.D1_RL_NKCountryOfOrigin.IsEmpty)
				{
					formData.D1_RL_NKCountryOfOriginInfo.AddMessageError(D1_RL_NKCountryOriginCodeEmpty);
				}
			}

			ListValidation.MessageErrorIfInvalidCode(formData.D1_RL_NKCountryOfOriginInfo, formData.Lookups.OriginList, D1_RL_NKCountryOriginCodeIsInvalid);
		}
		public static string D1_RL_NKCountryOriginCodeEmpty
		{
			get { return Res.GetString("307dbf26-3a23-4eb9-88cb-dcee6af41620", "Please enter a Country of Origin."); }
		}
		public static IMultilingualString D1_RL_NKCountryOriginCodeIsInvalid
		{
			get { return ResString.GetMultilingualString("64f3976e-9980-4cac-9b34-795e409c4966", "Please enter a valid Country of Origin."); }
		}

		protected override void CheckD1_RL_NKCountryOfDestination()
		{
			base.CheckD1_RL_NKCountryOfDestination();

			if (formData.D1_MessageType == JobMessageTypeList.Codes.Import)
			{
				if (formData.D1_RL_NKCountryOfDestination.IsEmpty)
				{
					formData.D1_RL_NKCountryOfDestinationInfo.AddMessageError(D1_RL_NKCountryDestinationCodeEmpty);
				}
			}

			ListValidation.MessageErrorIfInvalidCode(formData.D1_RL_NKCountryOfDestinationInfo, formData.Lookups.FinalDestinationList, D1_RL_NKCountryDestinationCodeIsInvalid);
		}
		public static string D1_RL_NKCountryDestinationCodeEmpty
		{
			get { return Res.GetString("e2cb3a9c-e26d-4803-ad2d-8ca8d395c4ff", "Please enter a Country of Destination."); }
		}
		public static IMultilingualString D1_RL_NKCountryDestinationCodeIsInvalid
		{
			get { return ResString.GetMultilingualString("123458ff-db3c-4ccc-bc40-a7342a6d0270", "Please enter a valid Country of Destination."); }
		}

		protected override void CheckD1_RN_NKItemCountryOfOrigin()
		{
			base.CheckD1_RN_NKItemCountryOfOrigin();

			if (formData.D1_RN_NKItemCountryOfOrigin.IsEmpty)
			{
				formData.D1_RN_NKItemCountryOfOriginInfo.AddMessageError(D1_RN_NKGoodsCountryOriginCodeEmpty);
			}

			ListValidation.MessageErrorIfInvalidCode(formData.D1_RN_NKItemCountryOfOriginInfo, formData.Lookups.CountryList, D1_RN_NKGoodsCountryOriginCodeIsInvalid);
		}
		public static string D1_RN_NKGoodsCountryOriginCodeEmpty
		{
			get { return Res.GetString("baa33e1e-f8ae-44a8-b98e-76ef3ffe1d73", "Please enter a Country of Origin for Goods."); }
		}
		public static IMultilingualString D1_RN_NKGoodsCountryOriginCodeIsInvalid
		{
			get { return ResString.GetMultilingualString("93d28392-2b91-4185-a68d-9cf9ab73178f", "Please enter a valid Country of Origin for Goods."); }
		}

		protected override void CheckD1_TotalPackagesPackType()
		{
			base.CheckD1_TotalPackagesPackType();

			if (formData.D1_TotalPackages > 0 && formData.D1_TotalPackagesPackType.IsEmpty)
			{
				formData.D1_TotalPackagesPackTypeInfo.AddMessageError(D1_TotalPackagesPackTypeEmpty);
			}

			ListValidation.MessageErrorIfInvalidCode(formData.D1_TotalPackagesPackTypeInfo, formData.Lookups.PackagesTypeList, D1_TotalPackagesPackTypeIsInvalid);
		}
		public static string D1_TotalPackagesPackTypeEmpty
		{
			get { return Res.GetString("b6f23bed-4658-4234-bbbc-9cdfaae58c04", "Please enter a Package Type. This is required when Total Packages is greater than 0."); }
		}
		public static IMultilingualString D1_TotalPackagesPackTypeIsInvalid
		{
			get { return ResString.GetMultilingualString("5e99cd8b-8888-47f2-908f-7d7079910d65", "Please enter a valid Package Type."); }
		}

		protected override void CheckD1_MarksAndNumbers()
		{
			base.CheckD1_MarksAndNumbers();

			if (formData.D1_MarksAndNumbers.IsEmpty)
			{
				formData.D1_MarksAndNumbersInfo.AddMessageError(D1_PackagesDescriptionGoodsPackageMarksNumberKindEmpty);
			}
		}
		public static string D1_PackagesDescriptionGoodsPackageMarksNumberKindEmpty
		{
			get { return Res.GetString("fe7e6fdb-7add-4bfd-a0a7-a6909a27997a", "Please enter any Marks and Numbers that appear on the packaging of the goods."); }
		}

		protected override void CheckD1_FlightDateIsValidZDateTime()
		{
			if (formData.D1_ModeOfTransportAtTheBorder == TransportTypeList.Codes.Air)
			{
				if (!formData.D1_FlightDate.IsValid)
				{
					formData.D1_FlightDateInfo.AddError(D1_IdentityNationalityActiveTransportBorderFlightDateIsInvalid);
				}
			}
		}
		public static string D1_IdentityNationalityActiveTransportBorderFlightDateIsInvalid
		{
			get { return Res.GetString("6ec3e781-1928-469c-b44c-3e9b597bfc16", "Please enter a valid Flight Date."); }
		}

		protected override void CheckD1_DeliveryTerms()
		{
			ListValidation.ErrorIfInvalidCode(formData.D1_DeliveryTermsInfo, formData.Lookups.DeliveryTermsList, D1_DeliveryTermsIsInvalid);
		}
		public static IMultilingualString D1_DeliveryTermsIsInvalid
		{
			get { return ResString.GetMultilingualString("8c40b983-f3a6-48ba-a016-2f3bd7c37da4", "Delivery Term."); }
		}
	}
}
