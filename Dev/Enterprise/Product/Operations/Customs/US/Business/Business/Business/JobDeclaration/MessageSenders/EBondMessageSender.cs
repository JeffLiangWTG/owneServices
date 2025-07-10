using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class EBondMessageSender : MainMessageSender
	{
		public EBondMessageSender(JobDeclaration declaration, ImportMessageSendingMessageType orgAmdWithdrawal, ImportMessageSendingMessageTypeAdditionalFilter additionalFilter)
			: base(declaration, orgAmdWithdrawal, additionalFilter)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		protected override MessageSendingValidation GetMessageSendingValidation(BusinessObject businessObjectForNotifications, IEnumerable<INotification> msgErrors)
		{
			return new EBondMessageSendingValidation(declaration);
		}

		protected override bool Prepare()
		{
			var bondContactName = declaration.CurrentUserFullName;
			var bondContactEmail = declaration.CurrentUserEmailAddress;
			var bondContactPhone = declaration.CurrentUserWorkPhone;

			if (bondContactName.IsEmpty || bondContactEmail.IsEmpty || bondContactPhone.IsEmpty)
			{
				var message = Res.GetString("c1e74c48-31ca-4055-bed2-f7a2cb7839fb"
					, "The bond contact details must be entered and have the following details: contact name, email and phone.");

				declaration.MessageInitiator.WarnUserAboutSomething(message, MessageCaption);

				return false;
			}

			var lookups = declaration.AddInfoLookups;
			if (!lookups.InsuranceAgentList.ContainsCode(declaration.US_InsuranceAgent))
			{
				declaration.MessageInitiator.WarnUserAboutSomething(Res.GetString("AE3F6B56-4EEC-4968-95BB-88B5ED54A043", "Please choose a valid Insurance Agent."), MessageCaption);

				return false;
			}

			var credential = declaration.GetCredential();
			if (credential == null)
			{
				var message = Res.GetString("DDC77912-5536-4818-A8BA-F7056C14EBCC"
					, "Please create a valid eBond Insurance Agent Credential for {0} on {1} - Brokerage."
					, declaration.US_InsuranceAgent
					, GlbCompany.CurrentCompany.CompanyName);

				declaration.MessageInitiator.WarnUserAboutSomething(message, MessageCaption);

				return false;
			}

			if (declaration.IsFormalImport && !declaration.US_EnableENS && declaration.ActiveEntryHeaders.EntrySummaryEntry == null)
			{
				var message = Res.GetString("601FCA60-9301-4C37-8D3A-594446227615", "Enable Entry Summary must be selected to merge.");
				declaration.MessageInitiator.WarnUserAboutSomething(message, MessageCaption);

				return false;
			}

			return base.Prepare();
		}

		protected override bool IsCreditCheckRequired => false;

		protected override bool ShouldNotifyUserOfASuccessfulSend => true;

		protected override string SuccessfulSendNotification => "eBond Request message has been generated";
	}
}
