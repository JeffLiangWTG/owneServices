using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Customs.TW.Business.N5167;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public sealed class N5167MessageSendingDocumentWrapper : DocumentWrapper, IDocumentWrapper
	{
		public N5167MessageSendingDocumentWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory) : base(entryHeader, factory)
		{
			this.entryHeader = entryHeader;
			TransportEquipments = new BusinessObjectCollectionWrapper<TransportEquipment>();
			if (this.entryHeader != null)
			{
				this.provider = new N5167MessageSendingObject(entryHeader);
				SetTransportEquipments();

				var message = (TWMessage)entryHeader.Messages.GetLastMessage(ApplicationCodeList.Codes.TWCustoms, MessageTypeList.Codes.RFM);
				N5107Message = message != null ? new N5107MessageHelper(message) : null;
			}
		}

		void SetTransportEquipments()
		{
			if (transportEquipments != null)
			{
				ZInt lineNo = 1;
				foreach (var transportEquipment in transportEquipments)
				{
					TransportEquipments.Add(TransportEquipment.New(transportEquipment, lineNo++, Factory));
				}
			}
		}

		#region Fields
		N5107MessageHelper N5107Message { get; }
		readonly CusEntryHeader entryHeader;
		readonly IN5167Declaration provider;
		IGoodsShipment GoodsShipment => provider?.GoodsShipment;
		IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => GoodsShipment?.GovernmentAgencyGoodsItems?.FirstOrDefault();

		public ZString ModeOfCustomsClearance => N5107StatusNameCode == ClearanceStatusCodeList.Codes.C3M ? "1" : N5107StatusNameCode == ClearanceStatusCodeList.Codes.C3X ? "2" : string.Empty;

		public ZString DeclarationOfficeID => provider?.DeclarationOfficeID ?? ZString.Empty;

		public IPartyDetails Agent => provider?.Agent;

		public ZString AgentChineseName => Agent?.ChineseName ?? ZString.Empty;

		public ZString AgentID => Agent?.ID ?? ZString.Empty;

		public ZString ExaminationPlaceID => GovernmentAgencyGoodsItem?.ExaminationPlace ?? ZString.Empty;

		public ZString AdditionalDeclarationID => CommonHelper.RemoveEntryNumberPlaceHolder(GovernmentAgencyGoodsItem?.AdditionalDeclaration?.ID ?? ZString.Empty);

		public ZString AdditionalDeclarationIDFormat => DocumentWrapperHelper.GetDeclarationIDFormat(AdditionalDeclarationID);

		IEnumerable<ITransportEquipment> transportEquipments => GovernmentAgencyGoodsItem?.TransportEquipments;

		public BusinessObjectCollectionWrapper<TransportEquipment> TransportEquipments { get; }

		public ZDateTime ControlInspectionStartDateTime => GovernmentAgencyGoodsItem?.ControlInspectionStartDateTime ?? ZDateTime.Empty;

		ZString N5107StatusNameCode => N5107Message?.ModeOfCustomsClearance ?? ZString.Empty;

		public ZString DeclarationID => entryHeader.EntryNumberForSendingObject;
		#endregion
	}
}
