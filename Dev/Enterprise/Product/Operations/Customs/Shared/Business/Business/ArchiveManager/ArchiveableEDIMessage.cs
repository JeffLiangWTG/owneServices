using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.ArchiveManager
{
	class ArchiveableEDIMessage : IArchiveableBusinessObjectWithOwnImageGenerationLogic
	{
		public ArchiveableEDIMessage(EDIMessage message)
		{
			this.message = message;
		}

		#region IArchiveableBusinessObject Members

		public ArchiveReferenceKey NaturalKey
		{
			get { return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.EDIMessage, message.EM_SystemCreateTimeUtc.ToString("yyyyMMdd-HHmmss") + "|" + message.EM_ApplicationCode + "|" + message.EM_MessageType + "|" + message.EM_MessageSubType); }
		}

		public IEnumerable<ArchiveReferenceKey> AdditionalKeys
		{
			get
			{
				yield return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.EDIMessageReceiveTransmit, message.EM_ReceiveTransmit);
				yield return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.EDIMessageApplicationReference, message.EM_ApplicationReference);
				yield return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.EDIMessageNum, message.EM_MessageNum);
				yield return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.EDIMessageSystemCreateUser, message.EM_SystemCreateUser);
				yield return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.EDIInterchangeNumber, message.EM_InterchangeNumber);
			}
		}

		public IEnumerable<ArchiveDocumentDescriptor> ArchiveDocuments
		{
			get { yield break; }
		}

		public BusinessObject ArchiveableBusinessObject
		{
			get { return message; }
		}

		public Guid BranchPK
		{
			get { return message.Branch.PK.ToGuid(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message builder")]
		public IEnumerable<ArchiveImageDescriptor> GenerateArchiveImages()
		{
			StringBuilder messagetext = new StringBuilder();

			if (message.Interchange != null)
			{
				messagetext.Append("Interchange Create Time: ");
				messagetext.AppendLine(message.Interchange.EI_SystemCreateTimeUtc.ToString("yyyyMMdd-HHmmss"));
				messagetext.Append("Interchange Headers: ");
				messagetext.AppendLine(message.Interchange.EI_HeaderText);
			}

			messagetext.Append("Message Text: ");
			messagetext.AppendLine(message.EM_MessageText);

			yield return new ArchiveImageDescriptor(ZBlob.FromAscii(messagetext.ToString()), "message.txt", "MSC");
		}

		#endregion

		readonly EDIMessage message;
	}
}
