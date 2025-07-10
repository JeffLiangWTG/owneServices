using System;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.SailingScheduleDataVendor.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	OneStopContainerEventRequestServiceTask.Code,
	"ComTrac Container Request Sender",
	"FRT",
	typeof(OneStopContainerEventRequestServiceTask),
	MinimumPeriod = "5minutes",
	IsMandatory = true,
	DefaultScheduleRunEvery = "15minutes",
	CanRunInAnyBranch = true,
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	OneStopContainerEventRequestServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status            + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive          + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit   + "=" + ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_ApplicationCode   + "=" + ApplicationCodeList.Codes.ComTrac
	},
	"ComTrac")]

namespace Enterprise.Freight.SailingScheduleDataVendor.ServiceTasks
{
	public class OneStopContainerEventRequestServiceTask : ServiceProviderImpl
	{
		public const string Code = "CTR";

		public override void RunTask(CancellationToken token)
		{
			if (CheckForUnsentEDIMessages())
			{
				foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						if (FreightDataRegistry.OneStopAUContainerIntegrationIsEnabled ||
							FreightDataRegistry.OneStopNZContainerIntegrationIsEnabled)
						{
							using (var sender = new OneStopContainerEventRequestInterchangeSender())
							{
								sender.Logger.OnLogInfoAdded += (logMessage, logType) => ServiceLogger.Log(logType, logMessage);
								sender.ExecuteBatch(token);
							}
						}
					}
				}
			}
		}

		static bool CheckForUnsentEDIMessages()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ComTrac);
			return factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(EDIMessage)), filter);
		}
	}

	class OneStopContainerEventRequestInterchangeSender : BaseInterchangeSender
	{
		protected override bool SendInt(EDIInterchange interchange)
		{
			try
			{
				string emailSubject = GlbCompany.CurrentCompany.OrgProxy != null
																? Enterprise.Freight.SailingScheduleDataVendor.ServiceTasks.Res.GetString("0186441f-955d-4957-88c2-14e9da72cce7",
																								"{0}(EDI License='{1}' ABN='{2}' Name='{3}')",
																								ServiceTaskConstants.ContainerEventSubscriptionEmailSubjectPrefix,
																								GlbCompany.CurrentCompany.LicenceKeyIdentifier,
																								GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number,
																												GlbCompany.CurrentCompany.OrgProxy.OH_FullNameTruncated)
																: string.Empty;
				string attachmentFileName = "ALERT_" + ZDateTime.Now.ToString("yyyyMMddHHmm") + ".csv";

				EmailDef email = new EmailDef();
				email.AddRecipientForSystemCommunication(ServiceTaskConstants.ContainerEventSubscriptionEmailAddress);
				email.Subject = emailSubject;
				email.Attachments.Add(new AttachmentDef(attachmentFileName, Encoding.UTF8.GetBytes(interchange.EI_BodyText)));
				Env.OutgoingMailManager.CreateAndSave(email);

				foreach (EDIMessage message in interchange.ContainedMessages)
				{
					message.EM_Status = EDIInterchange.Status.Sent;
				}

				interchange.EI_Status = EDIInterchange.Status.Sent;
			}
			catch (Exception e)
			{
				if (e.IsCriticalException())
				{
					throw;
				}

				if (Logger != null)
				{
					Logger.LogWarning("Failed to send ComTrac container request: " + e.Message);
				}
				return false;
			}

			return true;
		}

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			SendOutboundInterchanges(ApplicationCodeList.Codes.ComTrac, token);
		}

		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			var interchanges = new OneStopContainerEventRequestInterchangeProvider(messages).Interchanges;
		}
	}

	class OneStopContainerEventRequestInterchangeProvider : InterchangeProviderBase
	{
		public OneStopContainerEventRequestInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages) { }

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return string.Empty; }
		}

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			return ZString.Empty;
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			SetInterchangeValuesForTransmit(interchange, messages, ApplicationCodeList.Codes.ComTrac, ServiceTaskConstants.ContainerEventSubscriptionEmailAddress, GlbCompany.CurrentCompany.GC_Code);
		}

		protected override string GetCollationKey(EDIMessage message)
		{
			return message.EM_ApplicationReference;
		}
	}
}
