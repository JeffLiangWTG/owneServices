using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.NX5901;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;

namespace Enterprise.Customs.TW.Business
{
	public class NX5901MessageSendingObject : AdditionalDocumentMessageSendingObject, INX5901Declaration
	{
		public NX5901MessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		ZString INX5901Declaration.FunctionalReferenceID => MessageConstants.FunctionalReferenceIDPlaceHolder;

		ZString INX5901Declaration.ID => Header.EntryNumberForSendingObject;

		ZString INX5901Declaration.TypeCode => entryInstruction.CEI_Style;

		IAdditionalDocument INX5901Declaration.AdditionalDocument => null;

		ZString INX5901Declaration.ContactOffice => ContactOffice;

		IGoodsShipment INX5901Declaration.GoodsShipment => new GoodsShipment(this);

		IGovernmentProcedure INX5901Declaration.GovernmentProcedure => TransportTypeCode.IsEmpty ? null : new GovernmentProcedureWrapper(null, TransportTypeCode);

		IPreviousDocument INX5901Declaration.PreviousDocument => null;

		ZString INX5901Declaration.ResponsibleGovernmentAgency => "CU";

		ZString TransportTypeCode
		{
			get
			{
				var result = ZString.Empty;
				switch (Header.Declaration?.JE_MessageType)
				{
					case Common.Shared.SharedJobMessageTypeList.Codes.Export:
						result = "2";
						break;
					case Common.Shared.SharedJobMessageTypeList.Codes.Import:
						result = "1";
						break;
				}
				return result;
			}
		}

		public override ZString SerializeToMessageString()
		{
			return new NX5901MessageBuilder().SerializeToMessageString(this);
		}

		[BusinessObjectTestExclude]
		public override ZString MessageType { get => MessageTypeList.Codes.ADM; }
	}
}
