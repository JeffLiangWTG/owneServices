using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public class GOVGIOMessageHeader : IEDIFACTMessageAttachee
	{
		public GOVGIOMessageHeader(AsycudaManifestHeader manifestHeader)
		{
			ManifestHeader = manifestHeader;
		}

		public AsycudaManifestHeader ManifestHeader { get; }

		#region IEDIFACTMessageAttachee

		Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages => ((IEDIMessageCollectionProvider)ManifestHeader).Messages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => ManifestHeader.Factory;

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			ManifestHeader.Messages.Add(message);
		}

		ZString IEDIFACTMessageAttachee.MessageStatus { get; set; } = ZString.Empty;

		ZString IEDIFACTMessageAttachee.JobStatus { get; set; } = ZString.Empty;

		bool IEDIFACTMessageAttachee.HasChanges => ManifestHeader.HasChanges;

		ZString IEDIFACTMessageAttachee.JobIdentification => ZString.Empty;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => ManifestHeader;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion
	}
}
