using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5302;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5302MessageHelper : TWMessageHelper
	{
		public N5302MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as Response;
			declaration = response?.Declaration;
			borderTransportMeans = declaration?.BorderTransportMeans;
			consignment = declaration?.Consignment;
			departureTransportMeans = consignment?.DepartureTransportMeans;
			applicant = declaration?.TwApplicant;
			packaging = consignment?.ConsignmentItem?.Packaging;
		}

		readonly Response response;
		readonly ResponseDeclaration declaration;
		readonly ResponseDeclarationBorderTransportMeans borderTransportMeans;
		readonly ResponseDeclarationConsignment consignment;
		readonly ResponseDeclarationConsignmentDepartureTransportMeans departureTransportMeans;
		readonly ResponseDeclarationTwApplicant applicant;
		readonly ResponseDeclarationConsignmentConsignmentItemPackaging packaging;

		bool IsMasterBill(ZString typeCode)
		{
			return typeCode == TransportContractTypeList.Codes.T704 || typeCode == TransportContractTypeList.Codes.T741;
		}

		ZString GetTransportModeCaption(ZString typeCode, ZBool isDelivery)
		{
			ZString result;
			var codeDescription = GetTransportContractType(typeCode);
			if (IsMasterBill(typeCode))
			{
				result = isDelivery ? Captions.GetMasterDeliveryOrderNumber(codeDescription) : Captions.GetMasterAirWaybill(codeDescription);
			}
			else
			{
				result = isDelivery ? Captions.GetHouseDeliveryOrderNumber(codeDescription) : Captions.GetHouseAirWaybillNumber(codeDescription);
			}
			return result;
		}

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				WriteRow(table, Captions.FunctionCode, GetFunctionCode(FunctionCode));
				WriteRow(table, Captions.StatementCode, GetCPT_025_ExtraCondition(ExtraRequirement));
				WriteRow(table, Captions.ModeOfCustomsClearance, GetModeofCustomsClearance(ModeOfCustomsClearance));
				WriteRow(table, Captions.TransitApplicationNumber, TransitApplicationNumber);
				WriteRow(table, Captions.Declaration_TotalPackageQuantity, DeclarationTotalPackageQuantity);
				WriteRow(table, Captions.TypeOfTransitApplication, GetTypeOfTransitApplication(TypeOfTransitApplication));
				WriteReservedFields(table, response.Declaration);
				WriteRow(table, Captions.Agent_ID, CustomsBrokerBoxNo);
				WriteRow(table, Captions.Agent_tw_SubBoxID, CustomsBrokerSubBoxNo);
				WriteRow(table, Captions.BorderTransportMeans_ArrivalDateTime, DateOfImportation);
				WriteRow(table, Captions.BorderTransportMeans_ID, TransportID);
				WriteRow(table, Captions.BorderTransportMeans_JourneyID, VoyageNoFlightNo);
				WriteRow(table, Captions.BorderTransportMeans_Name, VesselName);
				WriteRow(table, Captions.BorderTransportMeans_tw_Registration, VesselRegistrationNo);
				WriteRow(table, Captions.BorderTransportMeans_TypeCode, GetInBondTransportMode(CategoriesOfTransport));
				WriteRow(table, Captions.Carrier_ID, CarrierAgencyIdentificationCoded);
				WriteRow(table, Captions.Consignment_tw_ManifestSerialNumber, ManifestSerialNumber);
				WriteRow(table, Captions.Consignment_tw_ShippingOrderNumber, ShippingOrderNumber);
				WriteRow(table, Captions.Consignment_StatementCode, GetTranshipmentExaminationNote(ExaminationNote));
				WriteRow(table, Captions.Packaging_tw_Combination, CoPackageNote);
				WriteRow(table, Captions.Packaging_TypeCode, PackageUnit);
				WriteTransportContractDocuments(table, consignment?.ConsignmentItem?.TransportContractDocument);
				WriteRow(table, Captions.ExportTransportID, ExportTransportIdCoded);
				WriteRow(table, Captions.ExportVesselName, ExportVesselName);
				WriteRow(table, Captions.VoyageNoFlightNo, ExportVoyageNoFlightNo);
				WriteRow(table, Captions.VesselRegistrationNo, ExportVesselRegistrationNo);
				WriteRow(table, Captions.PlaceOfLoadingCoded, PlaceOfLoadingCoded);
				WriteTransportContractDocuments(table, consignment?.TransportContractDocument);
				WriteTransportEquipments(table, consignment?.TransportEquipment);
				WriteRow(table, Captions.Deconsolidator_ID, DeconsolidatorCoded);
				WriteRow(table, Captions.DeparturePlaceCoded, DeparturePlaceCoded);
				WriteRow(table, Captions.Applicant_ChineseName, ApplicantNameInChinese);
				WriteRow(table, Captions.Applicant_CustomsControlID, BondedBusinessControlNumberCoded);
				WriteRow(table, Captions.Applicant_ID, Applicant_BAN_ID_PassportNo);
				WriteRow(table, Captions.Applicant_Name, ApplicantNameInEnglish);
				WriteRow(table, Captions.Applicant_TypeCode, PartyIdentifierCoded);
				WriteRow(table, Captions.UnloadingLocation_ID, ArrivalPlaceCoded);
			}
		}

		void WriteReservedFields(HtmlTableCreator table, ResponseDeclaration declaration)
		{
			var reservedFields = declaration?.AdditionalInformation;
			if (reservedFields != null && reservedFields.Any())
			{
				foreach (var reservedField in reservedFields)
				{
					var cell = new CellWithFormatting() { CellValue = FormattableString.Invariant($"<span style='font-weight:bold;text-decoration:underline;'>{Captions.ReservedFieldIdentifierCoded}:</span><span>&nbsp;{reservedField.StatementCode.Value}</span>&nbsp;&nbsp;<span style='font-weight:bold;text-decoration:underline;'>{Captions.ReservedField}:</span><span>&nbsp;{reservedField.StatementDescription.Value}</span>") };
					cell.HtmlAttributes.Add((NoResString)"style", (NoResString)"font-size: 12px;color: #000000;border:0px;margin: 10px;");
					table.WriteRow(cell);
				}
			}
		}

		void WriteTransportContractDocuments(HtmlTableCreator table, Collection<ResponseDeclarationConsignmentConsignmentItemTransportContractDocument> transportContractDocuments)
		{
			if (transportContractDocuments != null && transportContractDocuments.Any())
			{
				var firstTransportContact = transportContractDocuments.First();
				var typeCode = firstTransportContact.TypeCode.Value;
				var transportModeCaption = GetTransportModeCaption(typeCode, false);
				WriteRow(table, transportModeCaption, firstTransportContact.Id.Value);

				if (transportContractDocuments.Count > 1)
				{
					var secondTransportContact = transportContractDocuments.ElementAt(1);
					typeCode = secondTransportContact.TypeCode.Value;
					transportModeCaption = GetTransportModeCaption(typeCode, false);
					WriteRow(table, transportModeCaption, secondTransportContact.Id.Value);
				}
			}
		}

		void WriteTransportContractDocuments(HtmlTableCreator table, Collection<ResponseDeclarationConsignmentTransportContractDocument> transportContractDocuments)
		{
			if (transportContractDocuments != null && transportContractDocuments.Any())
			{
				var firstTransportContact = transportContractDocuments.First();
				var typeCode = firstTransportContact.TypeCode.Value;
				var transportModeCaption = GetTransportModeCaption(typeCode, true);
				WriteRow(table, transportModeCaption, firstTransportContact.Id.Value);

				if (transportContractDocuments.Count > 1)
				{
					var secondTransportContact = transportContractDocuments.ElementAt(1);
					typeCode = secondTransportContact.TypeCode.Value;
					transportModeCaption = GetTransportModeCaption(typeCode, true);
					WriteRow(table, transportModeCaption, secondTransportContact.Id.Value);
				}
			}
		}

		void WriteTransportEquipments(HtmlTableCreator table, Collection<ResponseDeclarationConsignmentTransportEquipment> transportEquipments)
		{
			if (transportEquipments != null && transportEquipments.Any())
			{
				foreach (var transportEquipment in transportEquipments)
				{
					WriteRow(table, Captions.ContainerNumber, transportEquipment.Id.Value);
					var governmentProcedureCollection = transportEquipment.GovernmentProcedure;
					if (governmentProcedureCollection.Any())
					{
						foreach (var processCode in governmentProcedureCollection)
						{
							WriteRow(table, Captions.GovernmentProcedure_ProcessCode, processCode.CurrentCode.Value);
						}
					}
				}
			}
		}

		ZString GetTypeOfTransitApplication(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.EntryTypeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetInBondTransportMode(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.InBondTransportModeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetTranshipmentExaminationNote(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.TranshipmentExaminationNoteList.GetDescriptionFromCode(itemCode));
		}

		ZString GetTransportContractType(string code)
		{
			return FormattableString.Invariant($"{Lookups.TransportContractTypeList.GetDescriptionFromCode(code)}({code})");
		}

		#region Fields Mapping
		ZString FunctionCode => response?.FunctionCode?.Value ?? ZString.Empty;

		ZString ExtraRequirement => response?.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;

		ZString ModeOfCustomsClearance => response?.Status?.NameCode?.Value ?? ZString.Empty;

		ZString TransitApplicationNumber => declaration?.Id?.Value ?? ZString.Empty;

		ZDecimal DeclarationTotalPackageQuantity => declaration?.TotalPackageQuantity?.Value ?? ZDecimal.Zero;

		ZString TypeOfTransitApplication => declaration?.TypeCode?.Value ?? ZString.Empty;

		ZString CustomsBrokerBoxNo => declaration?.Agent?.Id?.Value ?? ZString.Empty;

		ZString CustomsBrokerSubBoxNo => declaration?.Agent?.TwSubBoxId?.Value ?? ZString.Empty;

		ZString DateOfImportation => borderTransportMeans?.ArrivalDateTime ?? ZString.Empty;

		ZString TransportID => borderTransportMeans?.Id?.Value ?? ZString.Empty;

		ZString VoyageNoFlightNo => borderTransportMeans?.JourneyId?.Value ?? ZString.Empty;

		ZString VesselName => borderTransportMeans?.Name?.Value ?? ZString.Empty;

		ZString VesselRegistrationNo => borderTransportMeans?.TwRegistration?.Value ?? ZString.Empty;

		ZString CategoriesOfTransport => borderTransportMeans?.TypeCode?.Value ?? ZString.Empty;

		ZString CarrierAgencyIdentificationCoded => declaration?.Carrier?.Id?.Value ?? ZString.Empty;

		ZString ManifestSerialNumber => consignment?.TwManifestSerialNumber?.Value ?? ZString.Empty;

		ZString ShippingOrderNumber => consignment?.TwShippingOrderNumber?.Value ?? ZString.Empty;

		ZString ExaminationNote => consignment?.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;

		ZString CoPackageNote => packaging?.TwCombination?.Value ?? ZString.Empty;

		ZString PackageUnit => packaging?.TypeCode?.Value ?? ZString.Empty;

		ZString ExportTransportIdCoded => departureTransportMeans?.Id?.Value ?? ZString.Empty;

		ZString ExportVesselName => departureTransportMeans?.Name?.Value ?? ZString.Empty;

		ZString ExportVoyageNoFlightNo => departureTransportMeans?.TwJourneyId?.Value ?? ZString.Empty;

		ZString ExportVesselRegistrationNo => departureTransportMeans?.TwRegistration?.Value ?? ZString.Empty;

		ZString PlaceOfLoadingCoded => consignment?.LoadingLocation?.Id?.Value ?? ZString.Empty;

		ZString DeconsolidatorCoded => declaration?.Deconsolidator?.Id?.Value ?? ZString.Empty;

		ZString DeparturePlaceCoded => declaration?.LoadingLocation?.Id?.Value ?? ZString.Empty;

		ZString ApplicantNameInChinese => applicant?.TwChineseName?.Value ?? ZString.Empty;

		ZString BondedBusinessControlNumberCoded => applicant?.TwCustomsControlId?.Value ?? ZString.Empty;

		ZString Applicant_BAN_ID_PassportNo => applicant?.TwId?.Value ?? ZString.Empty;

		ZString ApplicantNameInEnglish => applicant?.TwName?.Value ?? ZString.Empty;

		ZString PartyIdentifierCoded => applicant?.TwTypeCode?.Value ?? ZString.Empty;

		ZString ArrivalPlaceCoded => declaration?.UnloadingLocation?.Id?.Value ?? ZString.Empty;
		#endregion

		#region Captions
		public new static class Captions
		{
			public static string FunctionCode => Res.GetString("{A7113326-D8AF-4DBA-8BDF-4D980A38BB6A}", "Message function, coded 訊息功能代碼");

			public static string StatementCode => Res.GetString("{811981E2-1F97-43F6-81B0-FAFC8EBCF4E0}", "Extra requirement, coded 放行附帶條件代碼");

			public static string ModeOfCustomsClearance => Res.GetString("{5FD1CC3E-A6FD-43FB-AAC6-110211AF4B65}", "Mode of Customs clearance 通關方式");

			public static string TransitApplicationNumber => Res.GetString("{472E6AE5-3207-4720-AE77-53AF3215D9FF}", "Transit application number 轉運申請書編號");

			public static string Declaration_TotalPackageQuantity => Res.GetString("{C58BA6DF-FD39-48D1-86C6-0793E91F8471}", "Total number of packages 總件數");

			public static string TypeOfTransitApplication => Res.GetString("{B01704CC-05D5-483B-ACBC-37A07BF2BFB2}", "Type of transit application 轉運申請書類別");

			public static string ReservedFieldIdentifierCoded => Res.GetString("{495B4482-3CD8-4E02-8D28-20BB3A840DD6}", "Reserved field identifier, coded 備用欄位識別代碼");

			public static string ReservedField => Res.GetString("{F60E653B-257A-48F4-B8FD-938379D22DE8}", "Reserved field 備用欄位");

			public static string Agent_ID => Res.GetString("{BA43022A-A6B1-4882-84F4-F94B69FAFA48}", "Customs broker box No. 報關業者箱號");

			public static string Agent_tw_SubBoxID => Res.GetString("{317D0122-0B86-426B-BC4C-7BBDD4FC86D7}", "Customs broker sub-box No. 報關業者箱號附碼");

			public static string BorderTransportMeans_ArrivalDateTime => Res.GetString("{6CFD3973-BE7C-4F9B-B19F-4D517D435554}", "Date of importation 進口日期");

			public static string BorderTransportMeans_ID => Res.GetString("{2C2E356B-7F50-463C-A24E-DDE5922C5DE2}", "Transport ID, coded 船（機）代碼");

			public static string BorderTransportMeans_JourneyID => Res.GetString("{3F164765-AD5F-4541-965A-379840BA70EB}", "Voyage No./Flight No. 船舶航次（海）/航機班次（空）");

			public static string BorderTransportMeans_Name => Res.GetString("{406CA7F4-EB17-4A36-9F30-23B123676C06}", "Vessel name 船舶名稱");

			public static string BorderTransportMeans_tw_Registration => Res.GetString("{28301E26-F36B-4944-BA7C-33E3E7980096}", "Vessel registration No. 海關通關號碼");

			public static string BorderTransportMeans_TypeCode => Res.GetString("{03A4F094-66F1-483D-8729-3CAF5C9FDB4D}", "Categories of transport 海空運別");

			public static string Carrier_ID => Res.GetString("{84B8C989-EEC0-4863-86F4-C7BDAD1F1D6F}", "Carrier/agency identification, coded 運輸業者/代理行代碼");

			public static string Consignment_tw_ManifestSerialNumber => Res.GetString("{8695674C-BE45-48C3-B7C5-F9CF835E396C}", "Manifest serial No. 艙單號碼");

			public static string Consignment_tw_ShippingOrderNumber => Res.GetString("{B0EA782C-44FF-46D4-96AC-A1A3D1D2B3E0}", "Shipping order No. 裝貨單號碼");

			public static string Consignment_StatementCode => Res.GetString("{E040E0D4-9510-435F-8F3F-3AE6E46DA6CB}", "Examination note 有否查驗");

			public static string Packaging_tw_Combination => Res.GetString("{1B130DB8-A5CC-421B-9E3B-2E771ACA1BEC}", "Co-package note 合成註記");

			public static string Packaging_TypeCode => Res.GetString("{6996EDDB-9A2E-4D1A-BFFB-954C8B355D90}", "Package unit 件數單位");

			public static string GetMasterAirWaybill(string transportMode) => Res.GetString("{FCF97444-8D8B-4B9A-8617-D6CF72A71BB9}", "Master air waybill / Bill of lading number {0}", transportMode);

			public static string GetHouseAirWaybillNumber(string transportMode) => Res.GetString("{3E711231-456A-4C07-9C31-68E3B1237E5C}", "House air waybill number / Bill of lading number {0}", transportMode);

			public static ZString ExportTransportID => Res.GetString("{AA75F2A8-F694-46F8-B0AD-B8DDD6603535}", "Export transport ID, coded 出口船（機）代碼");

			public static ZString ExportVesselName => Res.GetString("{D612680E-EE59-4FB1-8DD5-76183B8314AE}", "Export vessel name 出口船舶名稱");

			public static ZString VoyageNoFlightNo => Res.GetString("{849DF49B-F52F-45AC-BD5E-E4F022963921}", "Voyage No./Flight No. 船舶航次（海）/航機班次（空）");

			public static ZString VesselRegistrationNo => Res.GetString("{7527D906-64F2-4555-8F59-5F44A186C037}", "Vessel registration No. 海關通關號碼");

			public static ZString PlaceOfLoadingCoded => Res.GetString("{423E08D8-789B-4D52-91B5-B2A0ED56106D}", "Place of loading, coded 裝貨港代碼");

			public static string GetMasterDeliveryOrderNumber(string transportMode) => Res.GetString("{236628A4-94ED-4C2B-BF9C-63F3899D5BBA}", "Master delivery order number/Bill of lading number {0}", transportMode);

			public static string GetHouseDeliveryOrderNumber(string transportMode) => Res.GetString("{FED49A37-CD1E-461C-BA6D-E1D3B707D98B}", "House delivery order number/Bill of lading number {0}", transportMode);

			public static ZString ContainerNumber => Res.GetString("{CA749822-DBC6-4094-A373-3682BD86F771}", "Container number 貨櫃號碼");

			public static ZString GovernmentProcedure_ProcessCode => Res.GetString("{11B0137B-FFF4-479D-A0A6-AEAC69F5D188}", "Process code 處理註記");

			public static ZString Deconsolidator_ID => Res.GetString("{F3F2F185-C555-4CB4-9641-92EB5893FAED}", "Deconsolidator, coded 承攬運送業者代碼");

			public static ZString DeparturePlaceCoded => Res.GetString("{6B195AF8-6D80-4A18-AB42-A0604AE1C695}", "The departure place, coded 起運地點代碼");

			public static ZString Applicant_ChineseName => Res.GetString("{4821F30F-6778-4DF3-89E3-CD8512DEC981}", "Applicant name in Chinese 申請人中文名稱");

			public static ZString Applicant_CustomsControlID => Res.GetString("{5ADA01BE-340E-4EC6-B827-BF1E5EE98998}", "Bonded business control number, coded 海關監管編號");

			public static ZString Applicant_ID => Res.GetString("{98FBC294-7B5E-4A09-8CDE-3AB03E3C93C6}", "Applicant BAN/ID/passport No. 申請人統一編號");

			public static ZString Applicant_Name => Res.GetString("{F8D889E1-1B52-4105-8A8A-800F421713BD}", "Applicant name in English 申請人英文名稱");

			public static ZString Applicant_TypeCode => Res.GetString("{591AE2C0-F426-469A-97C0-DCCCA788812E}", "Party identifier, coded 身分識別代碼");

			public static ZString UnloadingLocation_ID => Res.GetString("{B935FB34-924C-4DE8-A6A7-44B5CA51DCA1}", "The arrival place, coded 運往地點代碼");
		}
		#endregion
	}
}
