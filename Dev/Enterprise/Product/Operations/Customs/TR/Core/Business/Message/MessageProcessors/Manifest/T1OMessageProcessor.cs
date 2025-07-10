using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.ASYCUDA.TRManifest;

namespace Enterprise.Customs.TR.Business
{
	public class T1OMessageProcessor : ManifestMessageProcessorBase
	{
		public T1OMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ProcessMessage(TRManifestMessage message, IMessageAttachee headerAttachee)
		{
			var isSuccess = true;
			TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass);

			var defaultMessageObject = message.MessageObject.InnerMessageObjects.FirstOrDefault();

			var status = TRMessageSendingHelper.ProcessCusPollingTransaction(message, TRMessageTypes.Codes.TRO, defaultMessageObject != null);
			if (status == Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR)
			{
				isSuccess = false;
				TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass);
				TableCreator.WriteRow(SoapMessageTextHelper.Row_EmptyResponse);
			}
			else if (status == Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS || defaultMessageObject != null)
			{
				if (defaultMessageObject is InnerXmlRegistrationNumberObject registrationNumberObject && !registrationNumberObject.RegistrationNumber.IsEmpty)
				{
					isSuccess = true;
					var registrationNumber = registrationNumberObject.RegistrationNumber;

					var header = message.EM_LinkedObject as IAsycudaManifestHeader;
					header.SuspendValidation();
					header.RegistrationNumber = registrationNumber;
					header.RegistrationDate = registrationNumberObject.RegistrationTime;
					header.AMA_GS_NKCustomsAgent = MessageProcessorHelper.GetMessageSignedBy(header);

					TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
					TableCreator.WriteRow(SoapMessageTextHelper.RowHeader_RegistrationNumber, registrationNumber);
					TableCreator.WriteRow(SoapMessageTextHelper.RowHeader_IssueDate, registrationNumberObject.RegistrationTime.ToString(InnerMessageObjectBase.Constants.DefaultDateTimeFormat));

					if (header.AMA_ManifestType == TRMessageConstants.ConsolidatedManifest && registrationNumberObject.GroupageAnswerObject != null)
					{
						ProcessGroupageAnswerMessage(header, registrationNumberObject.GroupageAnswerObject, message.Factory);
					}
					header.CalculateStampDuties();
					header.CreateManifestStatement();

					if (!registrationNumber.IsEmpty)
					{
						var origialMessage = TRInterchangeHelper.GetMainMessageByType(message, TRMessageTypes.Codes.TRO);
						TRMessageSendingHelper.SendManifestAutoReceiveResponseMessageForTRM(header, origialMessage);
					}
				}
				else if (defaultMessageObject is InnerXmlErrorMessagesObject errorMessagesObject)
				{
					TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, new[] { SoapMessageTextHelper.TableHeaderWithErrorMessage });
					foreach (var errorMessage in errorMessagesObject.ErrorMessages)
					{
						TableCreator.WriteRow(new string[] { errorMessage });
					}
					isSuccess = false;
				}
			}
			else if (status == Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN)
			{
				NeedToSendEmail = NeedToUpdateStatus = false; // if status still is OPN, do nothing, will send query again.
			}

			return isSuccess;
		}

		#region Update ManifestHeader

		void ProcessGroupageAnswerMessage(IAsycudaManifestHeader manifestHeader, InnerXmlGroupageAnswerObject messageObject, BusinessObjectFactory factory)
		{
			manifestHeader.MasterBill.SuspendValidation();

			var codeToCustomsCodeMapping = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(factory, Core.Constants.CountryCodes.Turkey, TRMessageConstants.CountryMapType, manifestHeader.RegistrationDate);
			manifestHeader.AMA_RN_NKConveyanceNationality = (ZString)codeToCustomsCodeMapping.Where(x => x.Value == messageObject.CountryCode)?.Select(x => x.Key).FirstOrDefault();
			manifestHeader.AMA_Voyage = messageObject.LicensePlateNo;

			if (messageObject.ArrivalDate.IsValid)
			{
				manifestHeader.AMA_DateAtCustomsOffice = messageObject.ArrivalDate;
			}
			if (messageObject.GdStartDate.IsValid)
			{
				manifestHeader.TemporaryStorageStartDate = messageObject.GdStartDate;
			}
			if (messageObject.GdTime.IsValid)
			{
				manifestHeader.TemporaryStorageDueDate = messageObject.GdTime;
			}

			manifestHeader.AMA_VesselName = messageObject.NameOfVehicle;
			manifestHeader.AMA_LloydsNumber = messageObject.ReferenceNumber;
			manifestHeader.AMA_OA_Carrier = GetCarrier(factory, messageObject.IdentificationNumber, messageObject.IdentityTour, messageObject.NameTitle, manifestHeader.AMA_TransportMode)?.MainAddress.PK ?? ZGuid.Empty;

			if (manifestHeader.AMA_TransportMode == TransportTypeList.Codes.Air)
			{
				var portOfLoadingCountryCode = (ZString)codeToCustomsCodeMapping.Where(x => x.Value == messageObject.CountryCodeYuk)?.Select(x => x.Key).FirstOrDefault();
				if (!portOfLoadingCountryCode.IsEmpty && !messageObject.PortLocationNameYuk.IsEmpty)
				{
					manifestHeader.AMA_RL_NKPortOfLoading = portOfLoadingCountryCode + messageObject.PortLocationNameYuk;
				}

				var portOfDischargeCountryCode = (ZString)codeToCustomsCodeMapping.Where(x => x.Value == messageObject.CountryCodeBos)?.Select(x => x.Key).FirstOrDefault();
				if (!portOfDischargeCountryCode.IsEmpty && !messageObject.PortLocationNameBos.IsEmpty)
				{
					manifestHeader.AMA_RL_NKPortOfDischarge = portOfDischargeCountryCode + messageObject.PortLocationNameBos;
				}
			}
			else if (manifestHeader.AMA_TransportMode == TransportTypeList.Codes.Sea)
			{
				manifestHeader.TransportType = messageObject.TransportType;
				manifestHeader.AMA_RL_NKPortOfLoading = messageObject.PortLocationNameYuk;
				manifestHeader.AMA_CustomsLoadPort = messageObject.PortLocationNameYuk.SubstringSafe(0, 5);
				manifestHeader.AMA_CustomsDischargePort = messageObject.PortLocationNameBos;
				manifestHeader.AMA_RL_NKPortOfDischarge = messageObject.PortLocationNameBos.SubstringSafe(0, 5);
			}

			manifestHeader.ManifestInternalInspectionNo = messageObject.InternalNoDso;
		}

		protected OrgHeader GetCarrier(BusinessObjectFactory factory, ZString vatNumber, ZString vatType, ZString carrierName, ZString transportMode)
		{
			OrgHeader carrier = null;

			var isSuitableForGetCarrier = (!vatNumber.IsEmpty && !vatType.IsEmpty) || !carrierName.IsEmpty;

			if (isSuitableForGetCarrier)
			{
				var query = GetCarrierFilterQuery(vatNumber, vatType, transportMode, carrierName);
				carrier = factory.LoadTop1<OrgHeader>(query);
			}

			return carrier;
		}

		ZDBOnlyQuery GetCarrierFilterQuery(ZString vatNumber, ZString vatType, ZString transportMode, ZString carrierName)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsShippingProvider, true);
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);

			if (transportMode == TransportTypeList.Codes.Sea)
			{
				query.AddToFilter(OrgHeaderSchema.OH_IsShippingLine, true);
			}

			if (transportMode == TransportTypeList.Codes.Air)
			{
				query.AddToFilter(OrgHeaderSchema.OH_IsAirLine, true);
			}

			if (vatType == TRMessageConstants.GrupajCompanyVatTypes.Diger && !carrierName.IsEmpty)
			{
				query.AddToFilter(OrgHeaderSchema.OH_FullName, carrierName.ToUpper());
			}

			if (vatType == TRMessageConstants.GrupajCompanyVatTypes.VergiNo && !vatNumber.IsEmpty)
			{
				var orgCusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Turkey);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.VATCode);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, vatNumber);
				query.AddSubQuery(orgCusCodeQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		protected override ZString GetInterpretationTitle(IMessageAttachee messageAttacheeBO, TRManifestMessage message, bool isSuccess)
		{
			if (isSuccess && message.MessageObject.InnerMessageObjects.FirstOrDefault() == null)
			{
				return Res.GetString("734C1355-DC02-46A6-A4C0-AF66FB6A8123", "The message text sent back from Customs is empty, System will try to query GUID again, please wait a moment.");
			}
			else
			{
				return base.GetInterpretationTitle(messageAttacheeBO, message, isSuccess);
			}
		}
	}
}
