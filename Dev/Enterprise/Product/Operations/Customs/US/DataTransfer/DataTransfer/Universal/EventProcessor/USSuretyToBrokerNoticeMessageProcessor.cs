using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class USeBondMessageToSuretyAgent : IJobDeclarationEventProcessor
	{
		public bool ProcessMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var result = false;
			var eventData = eventDataObject as Event;
			if (eventData != null && businessObject != null && SuretyToBrokerNoticeMessageProcessorHelper.IsSuretyToBrokerNoticeMessage(eventData))
			{
				var processor = new USSuretyToBrokerNoticeMessageProcessor(eventData, businessObject as JobDeclaration);
				processor.SendAcknowledgementReport();
				result = true;
			}

			return result;
		}
	}

	class USSuretyToBrokerNoticeMessageProcessor : SuretyToBrokerNoticeMessageProcessor
	{
		public USSuretyToBrokerNoticeMessageProcessor(Event eventDataObject, JobDeclaration declaration)
			: base(eventDataObject)
		{
			this.Declaration = declaration;
			this.Factory = Declaration.Factory;
		}

		protected readonly JobDeclaration Declaration;
		protected readonly BusinessObjectFactory Factory;

		protected override void GenerateHtmlEmailAndSendToBrokerOrGroup(string body)
		{
			string jobNumber = "Unknown";
			string url = "";
			var branch = GlbBranch.CurrentBranch;

			if (Declaration != null)
			{
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(Declaration);
				jobNumber = Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
				branch = Declaration.Branch ?? GlbBranch.CurrentBranch;
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
					var bondStatusNotificationGroupPk = USCustomsDataRegistry.Instance.BondStatusNotificationGroup.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					if (bondStatusNotificationGroupPk != ZGuid.Empty)
					{
						Env.OutgoingCustomsMailManager.Create(Factory, email, bondStatusNotificationGroupPk, GroupSourceLocator.GetFromRegistryItem(USCustomsDataRegistry.Instance.BondStatusNotificationGroup));
					}
				}
			}
		}
	}
}

#region

#endregion
