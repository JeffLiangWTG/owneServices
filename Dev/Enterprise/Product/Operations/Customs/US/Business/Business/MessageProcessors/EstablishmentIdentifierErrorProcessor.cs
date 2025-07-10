using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse)]
	[TopLevel(typeof(OGQFDPP10), typeof(OGQFDER))]
	class EstablishmentIdentifierQueryErrorProcessor : EstablishmentIdentifierErrorProcessor
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse)]
	[TopLevel(typeof(OGQFDPP20), typeof(OGQFDPP21), typeof(OGQFDPP22), typeof(OGQFDER), typeof(OGQFDPT30))]
	class EstablishmentIdentifierErrorProcessor : EstablishmentIdentifierProcessor
	{
		public override void Process()
		{
			var linkedObject = OriginalMessageLinker.Link(Message);
			var address = linkedObject as OrgAddress;
			var organisation = address == null ? linkedObject as OrgHeader : address.Header;
			var originalMessage = Message.OriginalMessage;
			if (originalMessage != null)
			{
				var errorBlocks = messageBlocks.OfType<OGQFDER>();
				if (!errorBlocks.IsNullOrEmpty())
				{
					List<string> body = new List<string>();
					body.Add("Your " + (organisation != null ? "request" : "query") + " for an FDA Establishment Identifier failed.");
					body.Add("");
					if (organisation != null)
					{
						body.Add("Organisation : " + organisation.OH_FullNameTruncated);
						Message.EM_LinkedObject = organisation;
					}

					foreach (MessageBlock block in messageBlocks)
					{
						OGQFDPP10 p10Block = block as OGQFDPP10;
						if (p10Block != null)
						{
							body.Add("FDA Establishment Identifier : " + p10Block.FDAEstablishmentIdentifier);
						}

						OGQFDER fder = block as OGQFDER;
						if (fder != null)
						{
							body.Add("Error : " + fder.ErrorMessageIdentifier + " - " + fder.NarrativeMessage);
						}
					}

					ZString subject = organisation != null ? IdentifierFailedSubject : QueryFailedSubject;
					SendEmail(subject, body);
				}
				else
				{
					var ogqfdpt30 = messageBlocks.OfType<OGQFDPT30>().FirstOrDefault();
					if (ogqfdpt30 != null)
					{
						Process(linkedObject, ogqfdpt30);
					}
				}
			}
		}

		void SendEmail(ZString subject, List<string> body)
		{
			ZString emailAddress = GetEmailAddress(Message.OriginalMessage.EM_SystemCreateUser);
			if (!emailAddress.IsEmpty)
			{
				HtmlEmailDef email = new HtmlEmailDef();
				email.Subject = subject;
				email.LoadPlainTextUsingTemplate(body);
				email.AddRecipientForSystemCommunication(emailAddress);
				Env.OutgoingCustomsMailManager.Create(Factory, email);
			}
		}

		ZString GetEmailAddress(ZString staffInitials)
		{
			GlbStaff staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffInitials);
			return staff != null ? staff.GS_EmailAddress : ZString.Empty;
		}

		internal const string IdentifierFailedSubject = "Failure to create FDA Establishment Identifier";
		internal const string QueryFailedSubject = "FDA Establishment Identifier Query failed";
	}
}
