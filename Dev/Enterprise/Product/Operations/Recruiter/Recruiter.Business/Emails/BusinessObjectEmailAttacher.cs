using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public static class BusinessObjectEmailAttacher
	{
		public static void AttachEmail(HRJobApplication jobApplication, string docType, params MailItem[] mailItems)
		{
			if (string.IsNullOrEmpty(docType))
			{
				docType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			}

			var docSupportInfo = jobApplication.DocManagerInfo;

			foreach (var mailItem in mailItems)
			{
				docSupportInfo.AddFileOrDocument(BuildMessage(mailItem), GenerateLegalFileName(mailItem.MI_Subject), docType);
				mailItem.MI_Status = MailStatus.Processed;
			}

			var container = new SavedEventHandlerContainer();
			container.Handler = delegate(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					var factoriesToSave =
						(from mailItem in mailItems
						 where mailItem.HasChanges && !docSupportInfo.MasterFactory.ChildParticipants.Contains(mailItem.Factory)
						 select (ITransactionParticipant)mailItem.Factory).Distinct().Union(new ITransactionParticipant[] { docSupportInfo.MasterFactory });
					BusinessObjectFactory.SaveTogether(factoriesToSave.ToArray());
					factory.Saved -= container.Handler;
				}
			};
			jobApplication.Factory.Saved += container.Handler;
		}

		class SavedEventHandlerContainer
		{
			public BusinessObjectFactory.SavedEventHandler Handler { get; set; }
		}

		static string GenerateLegalFileName(ZString subject)
		{
			var result = subject.IsEmpty ? "Email.eml" : subject.SubstringSafe(0, 256) + ".eml";

			foreach (var invalidChar in Path.GetInvalidFileNameChars())
			{
				result = result.Replace(new string(invalidChar, 1), string.Empty);
			}

			return result;
		}

		public static byte[] BuildMessage(MailItem mailItem)
		{
			return MailKitMailBuilder.BuildMimeMessageCore(mailItem).GetData();
		}
	}
}
