using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.TW.MessageDefinitions.N5108;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5108MessageHelper : TWMessageHelper
	{
		public N5108MessageHelper(TWMessage message) : base(message)
		{
			response = (Response)Message.IncomingMessageKeyInfomation.Result;
			declaration = response?.Declaration;
			borderTransportMeans = declaration?.BorderTransportMeans;
			consignment = declaration?.Consignment;
			consignmentItem = consignment?.ConsignmentItem;
		}

		public const string N5101HTypeCode = "5101H";
		readonly Response response;
		readonly ResponseDeclaration declaration;
		readonly ResponseDeclarationBorderTransportMeans borderTransportMeans;
		readonly ResponseDeclarationConsignment consignment;
		readonly ResponseDeclarationConsignmentConsignmentItem consignmentItem;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				WriteErrors(table, response.Error);
				WriteRow(table, Captions.StatusResponseCode, GetN5108ResponseCodeList(response.Status?.NameCode?.Value));
				WriteRow(table, Captions.ArrivalDateTime, borderTransportMeans?.ArrivalDateTime);
				WriteRow(table, Captions.TransportID, borderTransportMeans?.Id?.Value);
				WriteRow(table, Captions.JourneyID, borderTransportMeans?.JourneyId?.Value);
				WriteRow(table, Captions.Registration, borderTransportMeans?.TwRegistration?.Value);
				WriteRow(table, Captions.CategoriesOfTransport, borderTransportMeans?.TypeCode?.Value);
				WriteRow(table, Captions.BoardedQuantity, consignment?.BoardedQuantity?.Value);
				WriteRow(table, Captions.ManifestSerialNumber, consignment?.TwManifestSerialNumber?.Value);
				WriteRow(table, Captions.ShippingOrderNumber, consignment?.TwShippingOrderNumber?.Value);
				WriteRow(table, Captions.OriginalMessageIdentifier, consignmentItem?.PreviousDocument?.TwFunctionalReferenceId?.Value);
				WriteRow(table, Captions.OriginalMessageType, consignmentItem?.PreviousDocument?.TypeCode?.Value);
				WriteRow(table, Captions.OriginalSenderCode, consignmentItem?.PreviousDocument?.Submitter?.Id?.Value);
				WriteTransportContractDocuments(table, consignment?.TransportContractDocument);
				WriteTransportEquipments(table, consignment?.TransportEquipment);
				WriteRow(table, Captions.TransportTypeCode, declaration?.GovernmentProcedure?.TwTransportTypeCode?.Value);
				WriteRow(table, Captions.TotalNumberOfEmptyContainers, declaration?.TransportEquipment?.TwEmptyContainerQuantity?.Value);
				WriteRow(table, Captions.FullContainerNumber, declaration?.TransportEquipment?.TwFullContainerQuantity?.Value);
			}
		}

		void WriteErrors(HtmlTableCreator table, Collection<ResponseError> errors)
		{
			if (errors != null)
			{
				foreach (var error in errors)
				{
					WriteRow(table, Captions.ErrorValidationCode, GetN5108_ErrorCodeList(error.ValidationCode?.Value));
				}
			}
		}

		void WriteTransportContractDocuments(HtmlTableCreator table, Collection<ResponseDeclarationConsignmentTransportContractDocument> transportContractDocuments)
		{
			if (transportContractDocuments != null)
			{
				foreach (var document in transportContractDocuments)
				{
					var typeCode = document.TypeCode.Value;
					var transportModeCaption = GetTransportModeCaption(typeCode);
					WriteRow(table, transportModeCaption, document.Id.Value);
				}
			}
		}

		void WriteTransportEquipments(HtmlTableCreator table, Collection<ResponseDeclarationConsignmentTransportEquipment> transportEquipments)
		{
			if (transportEquipments != null)
			{
				foreach (var transportEquipment in transportEquipments)
				{
					WriteRow(table, Captions.FullContainerNumber, transportEquipment.Id.Value);
				}
			}
		}

		ZString GetTransportModeCaption(ZString typeCode)
		{
			var result = GetTransportContractType(typeCode);
			var isDelivery = !IsAir(typeCode);
			if (IsMasterBill(typeCode))
			{
				result += isDelivery ? Captions.GetMasterDeliveryOrderNumber : Captions.GetMasterAirWaybill;
			}
			else
			{
				result += isDelivery ? Captions.GetHouseDeliveryOrderNumber : Captions.GetHouseAirWaybillNumber;
			}
			return result;
		}

		ZString GetTransportContractType(string code)
		{
			return FormattableString.Invariant($"{Lookups.TransportContractTypeList.GetDescriptionFromCode(code)}({code})");
		}

		bool IsMasterBill(ZString typeCode)
		{
			return typeCode == TransportContractTypeList.Codes.T704 || typeCode == TransportContractTypeList.Codes.T741;
		}

		bool IsAir(ZString typeCode)
		{
			return typeCode == TransportContractTypeList.Codes.T703 || typeCode == TransportContractTypeList.Codes.T741;
		}

		string GetN5108ResponseCodeList(string code) => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, code, Core.Constants.CountryCodes.Taiwan, Codes.CustomsManifestStatus, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;

		public string GetN5108_ErrorCodeList(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.N5108_ErrorCodeList.GetDescriptionFromCode(itemCode));
		}

		public ZString TypeCode => consignmentItem?.PreviousDocument?.TypeCode?.Value ?? ZString.Empty;

		public ZString StatusNameCode => response?.Status?.NameCode?.Value ?? ZString.Empty;

		public new static class Captions
		{
			public static string ErrorValidationCode => Res.GetString("AEFAAB1A-F524-4D9E-BA8B-7319DCF8CC5E", "駁回原因代碼Error code");

			public static string StatusResponseCode => Res.GetString("BC538BD3-6A3D-47BC-9032-A77825B9DED7", "回應別Response code");

			public static string ArrivalDateTime => Res.GetString("84E53217-4940-4186-B11F-BA1F6A12BB48", "航機抵達日期Date of arrival");

			public static string TransportID => Res.GetString("0CB98A6D-1FE6-4E8A-92C0-A6E507AD3EE4", "船（機）代碼Transport ID, coded");

			public static string JourneyID => Res.GetString("A6B3FC09-C282-4DF3-A71A-0B84E0562D28", "船舶航次（海）/航機班次（空）Voyage No./Flight No.");

			public static string Registration => Res.GetString("A779862F-ED64-4F77-8551-EBB51A825C0E", "海關通關號碼Vessel registration No.");

			public static string CategoriesOfTransport => Res.GetString("D82E9482-6781-43D5-B261-80B84C546420", "海空運別Categories of transport");

			public static string BoardedQuantity => Res.GetString("7B4A3798-0717-4596-BCE9-598E97C96DA2", "艙號筆數Total number of consignments");

			public static string ManifestSerialNumber => Res.GetString("B41981F3-B2E2-4CA3-85B1-D2619BEF8390", "艙單號碼Manifest serial No.");

			public static string ShippingOrderNumber => Res.GetString("47BEF29D-07D5-4083-A731-8D138D56AB87", "裝貨單號碼Shipping order No.");

			public static string OriginalMessageIdentifier => Res.GetString("0C2EDDE0-2317-4F18-B306-7A6CF5C46F2F", "原訊息編號Original message identifier");

			public static string OriginalMessageType => Res.GetString("9CC1EB04-F522-429C-828A-458AD9173091", "原訊息類別Original message type");

			public static string OriginalSenderCode => Res.GetString("9BDD3011-04F0-4B19-8A1D-B4F1F263F7E9", "原訊息傳送者代碼Original sender code");

			public static string GetMasterDeliveryOrderNumber => Res.GetString("BCFB2295-2324-47CA-8F28-398F3D76D1E3", "Master delivery order number/Bill of lading number");

			public static string GetHouseDeliveryOrderNumber => Res.GetString("F4AF9083-2F93-492E-8259-3E29C1300CAB", "House delivery order number/Bill of lading number");

			public static string GetMasterAirWaybill => Res.GetString("56015D5F-64C0-4ACC-BDA7-2630EFCC4F21", "Master air waybill/Bill of lading number");

			public static string GetHouseAirWaybillNumber => Res.GetString("87FDF46D-444B-47C7-81C3-ABFAB03810C6", "House air waybill number/Bill of lading number");

			public static string FullContainerNumber => Res.GetString("E9819C26-720C-4EFE-AFD1-72EA89478E77", "實櫃號碼Full container No.");

			public static string TransportTypeCode => Res.GetString("E0C75E07-67B0-4D9F-B92E-B5EF1C9C46E0", "進出口別代碼Import/Export, coded");

			public static string TotalNumberOfEmptyContainers => Res.GetString("0889CDD5-46D9-4859-B535-39B9FDAFF5A9", "空櫃總數Total number of empty containers");

			public static string TotalNumberOfFullContainers => Res.GetString("087B8F44-EFB2-4CC9-8887-5E3DF5A87659", "實櫃總數Total number of full containers");
		}
	}
}
