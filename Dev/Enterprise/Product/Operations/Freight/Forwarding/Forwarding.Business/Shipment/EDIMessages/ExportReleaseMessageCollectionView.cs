using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ExportReleaseMessageCollectionView : BusinessObjectCollectionView<EDIMessage>
	{
		public ExportReleaseMessageCollectionView(EDIMessageCollection completeCollection) : base(completeCollection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var ediMessage = element as EDIMessage;

			return ExcludeILCustomMessages(ediMessage);
		}

		bool ExcludeILCustomMessages(EDIMessage ediMessage)
		{
			return ediMessage.EM_ApplicationCode != ILCustomsApplicationCode
				|| (ediMessage.EM_ApplicationCode == ILCustomsApplicationCode && ediMessage.EM_MessageType != ILDLOMessageType && ediMessage.EM_MessageType != ILGPMMessageType);
		}

		const string ILCustomsApplicationCode = "ILC";
		const string ILDLOMessageType = "DLO";
		const string ILGPMMessageType = "GPM";
	}
}
