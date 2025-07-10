using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5168MessageHelper : TWMessageHelper
	{
		public N5168MessageHelper(TWMessage message) : base(message)
		{
			declaration = Message.IncomingMessageKeyInfomation.Result as CargoWise.Customs.TW.MessageDefinitions.N5168.Declaration;
		}

		readonly CargoWise.Customs.TW.MessageDefinitions.N5168.Declaration declaration;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (declaration != null)
			{
				WriteRow(table, Captions.ReasonForUnableToHandleContainerCoded, GetUnabletoHandleContainer(ReasonForUnableToHandleContainerCoded));
				WriteRow(table, Captions.ExaminationDispatchedDateAndTime, ExaminationDispatchedDateAndTime);
			}
		}

		ZString ReasonForUnableToHandleContainerCoded => declaration?.AdditionalInformation?.RequestOverrideCode?.Value ?? ZString.Empty;

		ZString ExaminationDispatchedDateAndTime => declaration?.Consignment?.Control?.InspectionStartDateTime ?? ZString.Empty;
	}
}
