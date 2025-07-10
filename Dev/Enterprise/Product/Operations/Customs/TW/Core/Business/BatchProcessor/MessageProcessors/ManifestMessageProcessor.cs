using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	class ManifestMessageProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			if (message.EM_LinkedObject is AsycudaManifestHeader manifestHeader)
			{
				message.EM_MessageInterpretation = TWMessageHelper.NewIncomingHelper(message)?.ToHtml() ?? ZString.Empty;
				UpdateAsycudaBillMessageStatus(manifestHeader);
				UpdateAsycudaBillCustomsStatus(message.Factory, message, manifestHeader.PK);
				TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills(manifestHeader);
				TWMessageHelper.UpdateAsycudaHeaderCustomsStatusFromBills(manifestHeader);
				successful = true;
			}
			message.EM_Status = successful ? TWMessage.Status.ProcessedOK : TWMessage.Status.Discarded;
			return successful;
		}

		void UpdateAsycudaBillMessageStatus(AsycudaManifestHeader manifestHeader)
		{
			manifestHeader.AMA_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
			manifestHeader.Bills.Cast<AsycudaBill>().ForEach(b => b.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged);
		}

		void UpdateAsycudaBillCustomsStatus(BusinessObjectFactory factory, TWMessage twmessage, ZGuid headerPK)
		{
			var functionalReferenceID = twmessage.FunctionalReferenceID;

			var bills = TWMessageHelper.LookForAsycudaManifestBillsWithHeaderPK(factory, functionalReferenceID, headerPK);
			if (bills.Any())
			{
				var helper = new N5108MessageHelper(twmessage);
				var statusCode = helper.StatusNameCode;
				if (helper.Lookups.N5108ResponseCodeList.ContainsCode(statusCode))
				{
					bills.ForEach(b => b.ABL_BillStatus = statusCode);
				}
			}
		}

		public override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			AsycudaManifestHeader linkedObject = null;
			MultilingualString discardReason = (NoResString)string.Empty;
			if (message.IncomingMessageKeyInfomation.Result is CargoWise.Customs.TW.MessageDefinitions.N5108.Response result)
			{
				var factory = message.Factory;
				var helper = new N5108MessageHelper(message);
				var journeyID = result.Declaration?.BorderTransportMeans?.JourneyId?.Value;
				var transportContractDocuments = result.Declaration?.Consignment?.TransportContractDocument;
				var airId = transportContractDocuments?.FirstOrDefault(c => c.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._741)?.Id?.Value;
				var transportContractDocumentID = string.IsNullOrEmpty(airId) ? transportContractDocuments?.FirstOrDefault(c => c.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._704)?.Id?.Value : airId;
				var transportMode = string.IsNullOrEmpty(airId) ? TransportTypeList.Codes.Sea : TransportTypeList.Codes.Air;

				linkedObject = TWMessageHelper.LookForAsycudaManifestHeader(factory, journeyID, transportContractDocumentID, transportMode);
				if (linkedObject == null)
				{
					discardReason = GetUnableToFindTheLinkedJobMessage(message);
					Logger.LogWarning(Res.GetString("CF3D385A-4218-49CE-867C-1E8350626FA1", "Can not find the corresponding Manifest Header for Message Number: {0}", message.EM_MessageNum));
				}
			}
			return (message.EM_GB, linkedObject, discardReason);
		}
	}
}
