using Enterprise.Edifact.D98B.Elements;
using Enterprise.Edifact.D98B.Messages.IFCSUM;
using Enterprise.Edifact.D98B.Segments;

namespace Enterprise.Freight.Agency.Business
{
	/// <summary>
	/// IFCSUM v1.1 (D94B - faked)
	/// </summary>
	internal class IFCSUM11 : IFCSUM20
	{
		protected override IFCSUMMessage GenerateMessage(IPortAuthorityMessagingData data)
		{
			IFCSUMMessage message = base.GenerateMessage(data);

			UNHSegment uNH = message.UNH[0];
			uNH.MessageIdentifier.MessageReleaseNumber = MessageReleaseNumberList.Release1994B;
			uNH.MessageIdentifier.AssociationAssignedCode = "AU11";

			return message;
		}
	}
}


