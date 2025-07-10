using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse)]
	[TopLevel(typeof(OGQFDPT30))]
	class EstablishmentIdentifierProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			var linkedObject = OriginalMessageLinker.Link(Message);
			var fdpt30 = messageBlocks.OfType<OGQFDPT30>().FirstOrDefault();
			Process(linkedObject, fdpt30);
		}

		protected void Process(BusinessObject linkedObject, OGQFDPT30 fdpt30)
		{
			var address = linkedObject as OrgAddress;
			var organisation = address == null ? linkedObject as OrgHeader : address.Header;
			if (fdpt30 != null)
			{
				var actionOrReturnCode = fdpt30.Message[0];
				switch (actionOrReturnCode)
				{
					case '1':
					case '2':
					case '5':
						{
							if (organisation != null)
							{
								var fdaEstablishmentIdentifier = fdpt30.Message.Substring(1, 12);
								var cusCode = GetExistingRegistrationRecord(organisation.PK, fdaEstablishmentIdentifier);
								if (cusCode == null)
								{
									if (address == null)
									{
										address = organisation.MainAddress;
									}

									if (address != null)
									{
										address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, fdaEstablishmentIdentifier, Core.Constants.CountryCodes.UnitedStates);
									}
								}
								else
								{
									if (address != null)
									{
										if (cusCode.OK_OA_PremisesAddress != address.PK)
										{
											address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, fdaEstablishmentIdentifier, Core.Constants.CountryCodes.UnitedStates);
										}
									}
									else
									{
										address = cusCode.PremisesAddress;
									}
								}

								SendEstablishmentIdentifierUpdatedEmail(address, fdaEstablishmentIdentifier, Message.OriginalMessage.EM_SystemCreateUser);
							}
							break;
						}
					case '3':
						{
							SendErrorEmail("Invalid broker request (Error Code 3)", organisation);
							break;
						}
					case '4':
						{
							SendErrorEmail("Add request denied (Error Code 4)", organisation);
							break;
						}
					case '6':
						{
							SendErrorEmail("Delete FEI from ACS (Error Code 6)", organisation);
							break;
						}
					case 'Q':
						{
							SendQueryResultEmail(fdpt30.Message.SubstringSafe(1, 12), fdpt30.Message.SubstringSafe(16, 33), Message.OriginalMessage.EM_SystemCreateUser);
							break;
						}
					case 'A':
						break;
				}
			}

			if (organisation != null)
			{
				OrgHeaderWrapper orgWrapper = OrgHeaderWrapper.New(organisation);
				orgWrapper.Messages.Add(Message);
			}
		}

		OrgCusCode GetExistingRegistrationRecord(ZGuid organisationPK, ZString fdaEstablishmentIdentifier)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_OH, organisationPK);
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.FDAEstablishmentIdentifier);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, fdaEstablishmentIdentifier);
			var cusCode = Factory.LoadTop1<OrgCusCode>(query);
			return cusCode;
		}

		void SendErrorEmail(string failureReason, OrgHeader organisation)
		{
			if (organisation != null)
			{
				string[] body =  { "Your request for an FDA Establishment Identifier failed.", "",
				"Organisation : " + organisation.OH_FullNameTruncated,
				"Reason : " + failureReason };
				SendHtmlMail(IdentifierFailedCreateSubject, body, Message.OriginalMessage.EM_SystemCreateUser);
			}
		}

		void SendEstablishmentIdentifierUpdatedEmail(OrgAddress address, ZString fdaEstablishmentIdentifier, ZString staffInitials)
		{
			string[] body = {
								"Your request for an FDA Establishment Identifier succeeded.",
								"This identifier has been automatically attached to the organisation.",
								"",
								"Organisation : " + address.EffectiveCompanyNameTruncated,
								"Address : " + address.OA_Address1,
								"City : " + address.OA_City,
								"State : " + address.OA_State,
								"Post Code : " + address.OA_PostCode,
								"Establishment Identifier : " + fdaEstablishmentIdentifier
							};
			SendHtmlMail(IdentifierCreatedSubject, body, staffInitials);
		}

		internal const string IdentifierCreatedSubject = "FDA Establishment Identifier created";
		internal const string IdentifierFailedCreateSubject = "Failure to create FDA Establishment Identifier";
		internal const string QuerySubject = "FDA Establishment Identifier Query result";

		void SendQueryResultEmail(ZString fEI, ZString narrativeMessage, ZString staffInitials)
		{
			string[] body = { "Your query for an FDA Establishment Identifier succeeded.",
								"",
								"FDA Establishment Identifier: " + fEI,
								"Response Message: " + narrativeMessage };

			SendHtmlMail(QuerySubject, body, staffInitials);
		}

		void SendHtmlMail(string subject, string[] body, string userInitials)
		{
			ZString emailAddress = GetEmailAddress(userInitials);
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
	}
}
