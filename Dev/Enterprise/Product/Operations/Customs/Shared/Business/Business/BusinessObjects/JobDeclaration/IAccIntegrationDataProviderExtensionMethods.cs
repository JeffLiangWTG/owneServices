// -----------------------------------------------------------------------
// <copyright file="IAccIntegrationDataProviderExtensionMethods.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	using System.Linq;
	using CargoWise.Application;
	using CargoWise.Types;
	using Enterprise.Accounting.Integration;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Modules;

	public static class IAccIntegrationDataProviderExtensionMethods
	{
		public static void SendEmailOnException(this IAccIntegrationDataProvider dataProvider, string details)
		{
			if ((dataProvider.Action & ChargePosterBehaviours.SendEmail) == ChargePosterBehaviours.SendEmail)
			{
				try
				{
					var sender = new HtmlNotificationEmailSender();
					var subject = Res.GetString("3d514f8f-f730-4f2a-ba40-b3f6b3b3bfb6", "Auto-Billing failure for {0} {1}", dataProvider.JobType, dataProvider.ReferenceID);
					var body = GetEmailBodyForException(dataProvider, details);
					body = body.Replace("\r\n", "<br />");
					body = body.Replace("\n", "<br />");
					body = body.Replace(System.Environment.NewLine, "<br />");

					var fk = dataProvider.AutoPostingEmailRecipient;
					var group = dataProvider.Factory.Load<GlbGroup>(fk);
					if (group != null)
					{
						var email = sender.CreateEmail(subject, body);
						Env.OutgoingCustomsMailManager.CreateAndSave(email, fk.ToGuid(), GroupSourceLocator.GetFromGroup(group));
					}
					else
					{
						var staff = dataProvider.Factory.Load<GlbStaff>(fk);
						if (staff != null)
						{
							var email = sender.CreateEmail(subject, body);
							email.AddRecipientForSystemCommunication(staff.GS_EmailAddress);
							Env.OutgoingCustomsMailManager.CreateAndSave(email);
						}
					}
				}
				catch (EmailHasNoRecipientsException)
				{
				}
			}
		}

		static string GetEmailBodyForException(IAccIntegrationDataProvider dataProvider, string details)
		{
			var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(dataProvider.ControllerID, dataProvider.BusinessObjectPK);
			var hyperLinkToJob = (NoResString)"<br /><strong>Auto-Billing failure for " + dataProvider.JobType + (NoResString)" <a href=\"" + url + (NoResString)"\">" + dataProvider.ReferenceID + (NoResString)"</a></strong>";

			return hyperLinkToJob + (NoResString)"<br /><br />" + details + (NoResString)"<br /><br /><hr>";
		}

		public static string ConcurrencyExceptionUserExplanation
		{
			get { return Res.GetString("4f83e23e-ba29-4b1d-a94a-503e07d4d278", "While system is trying to perform auto-billing and save changes, another user modified the same record and system could not proceed. No billing changes have been made. Please check the record."); }
		}

		public static string SaveExceptionUserExplanation
		{
			get { return Res.GetString("C2662C5F-BDF2-4DDA-A9F5-F6E198E62E7C", "While system is trying to perform auto-billing there was a problem while attempting to save. No billing changes have been made. Please check the record."); }
		}

		public static ZBool HasCustomsCharges(this IAccIntegrationDataProvider dataProvider)
		{
			return dataProvider != null && dataProvider.InvDataProviders != null && dataProvider.InvDataProviders.Any(p => p != null && p.CustomsCharges.Any(c => c != null && c.GetCustomsCharges(null).Any()));
		}
	}
}
