using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;

namespace Enterprise.MasterFiles.Business
{
	class SaveSentEmailHandler : XmlMessageHandler<OutlookEmailSentMessage.OutlookEmailMessageForSave>
	{
		protected override void Handle(IEnterpriseChannel channel, OutlookEmailSentMessage.OutlookEmailMessageForSave message)
		{
			ApplicationDispatcher.Current.BeginInvoke(new SaveSentEmailHandlerDelegate(DoHandle), message);
		}

		void DoHandle(OutlookEmailSentMessage.OutlookEmailMessageForSave message)
		{
			string docType = message.header.Substring(0, 3);// length of SaveToEDocsDocumentType
			Guid parentPK = new Guid(message.header.Substring(3, 36));// length of Guid
			string parentType = message.header.Substring(39);
			var eDocFactory = new BusinessObjectFactory();
			BusinessObject parentBO = eDocFactory.Load(parentType, parentPK);
			var eDocSource = parentBO as IDocManagerSupport;
			if (eDocSource != null)
			{
				var eMailDoc = eDocSource.DocManagerInfo.AddFileOrDocument(message.mailFile, message.subject + ".msg", docType);
				if (string.IsNullOrEmpty(eMailDoc.Description))
				{
					eMailDoc.Description = message.subject;
				}
				eDocSource.DocManagerInfo.Save();
			}
		}

		delegate void SaveSentEmailHandlerDelegate(OutlookEmailSentMessage.OutlookEmailMessageForSave message);
	}
}