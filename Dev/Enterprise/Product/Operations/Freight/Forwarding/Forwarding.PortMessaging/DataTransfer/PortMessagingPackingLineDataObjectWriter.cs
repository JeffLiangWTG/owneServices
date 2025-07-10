using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer
{
	class PortMessagingPackingLineDataObjectWriter : ForwardingPackingLineDataObjectWriter
	{
		public PortMessagingPackingLineDataObjectWriter(IContainerLinkManager<ForwardingConsol> linkManager, IOrderLineLinkManager orderLineLinkManager, PackLineLinkManager packLineLinkManager, BindToLists listCache, IDataWritingManager manager)
			: base(linkManager, orderLineLinkManager, packLineLinkManager, listCache, manager)
		{
		}

		protected override PackingLine PopulateDataObject(ForwardingPackLine packLineBO)
		{
			var packLineDataObject = base.PopulateDataObject(packLineBO);

			var portMessaging = PackLinePortMessaging.LoadOrCreate(packLineBO);
			packLineDataObject.PortMessaging = new UniversalDataBuss.DataObjects.Universal.PortMessaging
			{
				TypeOfDeclaration = ListHelper.GetWithDescription<CodeDescriptionPair>(portMessaging.JLM_EntryType, portMessaging.Lookups.EntryTypeList),
				MRN = portMessaging.JLM_MovementReferenceNumber,
				MRNComplete = portMessaging.JLM_MovementReferenceNumberComplete ? "Y" : "N",
				ExemptionReason = ListHelper.GetWithDescription<CodeDescriptionPair>(portMessaging.JLM_ExemptionReason, portMessaging.Lookups.ExemptionReasonList),
				ATB = portMessaging.JLM_ATBNumber,
				Annex30AType = ListHelper.GetWithDescription<CodeDescriptionPair>(portMessaging.JLM_Annex30AType, portMessaging.Lookups.Annex30ATypeList),
				Annex30AFailureProcess = portMessaging.JLM_Annex30AFailureProcess,
				ExportDeclarationNumber = portMessaging.JLM_ExportDeclarationReference,
				CustomsReleaseDate = portMessaging.JLM_CustomsReleaseDate,
				LRN = portMessaging.JLM_LocalReferenceNumber,
				LRNComplete = portMessaging.JLM_LocalReferenceNumberComplete ? "Y" : "N"
			};

			// Legacy fields, should be dropped ASAP
			packLineDataObject.MRN = portMessaging.JLM_MovementReferenceNumber;
			packLineDataObject.MRNComplete = portMessaging.JLM_MovementReferenceNumberComplete ? "Y" : "N";
			packLineDataObject.EntryType = portMessaging.JLM_EntryType;
			packLineDataObject.ExemptionReason = portMessaging.JLM_ExemptionReason;

			return packLineDataObject;
		}
	}
}
