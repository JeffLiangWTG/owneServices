using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal
{
	public class ISFSuretyToBrokerNoticeMessageProcessor : SuretyToBrokerNoticeMessageProcessor
	{
		public ISFSuretyToBrokerNoticeMessageProcessor(CusISFHeader header, Event eventDataObject) : base(eventDataObject)
		{
			this.Header = Argument.NotNull(header, "header");
			this.Factory = Header.Factory;
		}
		protected readonly BusinessObjectFactory Factory;
		protected readonly CusISFHeader Header;

		protected override void GenerateHtmlEmailAndSendToBrokerOrGroup(string body)
		{
			string jobNumber = "Unknown";
			string url = "";
			var branch = GlbBranch.CurrentBranch;

			if (Header != null)
			{
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(Header);
				jobNumber = Header.BF_JobReference;
				branch = Header.Branch ?? GlbBranch.CurrentBranch;
			}
			SendEmailToBrokerOrGroupIfSenderInvalid(url, jobNumber, body, branch);
		}

		void SendEmailToBrokerOrGroupIfSenderInvalid(string uri, string jobNumber, string body, GlbBranch branch)
		{
			EmailDef email;
			new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, MessageType, body, false, out email, branch);

			if (email != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					var securityNotificationGroupPk = ISFRegistry.Instance.ImporterSecurityFilingMessagesGroup.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					if (securityNotificationGroupPk != ZGuid.Empty)
					{
						Env.OutgoingCustomsMailManager.Create(Factory, email, securityNotificationGroupPk, GroupSourceLocator.GetFromRegistryItem(ISFRegistry.Instance.ImporterSecurityFilingMessagesGroup));
					}
				}
			}
		}
	}
}
