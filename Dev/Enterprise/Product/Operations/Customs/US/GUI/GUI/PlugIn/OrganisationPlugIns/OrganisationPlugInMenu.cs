using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class OrganisationPlugInMenu : KMenuItem
	{
		public OrganisationPlugInMenu(OrgHeaderWrapper organisation, Action licenceLogIn)
		{
			this.licenceLogIn = licenceLogIn;
			this.organisation = organisation;
			this.Text = Res.GetString("Customs.US.OrganisationPlugIn", "Customs Messaging");

			InitialiseMenu();
		}

		internal readonly Action licenceLogIn;
		readonly OrgHeaderWrapper organisation;

		internal MenuItem sendADDMessageToCustomsMenu;

		MenuItem requestEstablishmentIdentifierMenu;

		internal MenuItem addManufacturerIdentifierMenu;

		internal MenuItem queryImporterBondMenu;

		internal MenuItem globalBusinessIdentifierAddDeleteUpdateMenu;

		void InitialiseMenu()
		{
			sendADDMessageToCustomsMenu = new ZMenuItem(ResString.GetMultilingualString("Customs.US.OrganisationPlugIn.ImporterConsigneeFile", "Importer/Consignee File (CBPF-5106) Add/Update"), new EventHandler(SendImporterADDMessage_Click));
			this.MenuItems.Add(sendADDMessageToCustomsMenu);

			MenuItems.Add("-");

			requestEstablishmentIdentifierMenu = new ZMenuItem(ResString.GetMultilingualString("Customs.US.OrganisationPlugIn.EstablishmentIdentifier", "Establishment Identifier (FDA) Add"), new EventHandler(RequestEstablishmentIdentifierMessage_Click));
			MenuItems.Add(requestEstablishmentIdentifierMenu);

			MenuItems.Add("-");

			addManufacturerIdentifierMenu = new ZMenuItem(ResString.GetMultilingualString("Customs.US.OrganisationPlugIn.ManufacturerNameAddress", "Manufacturer Name/Address Add/Update/Query"), new EventHandler(AddManufacturerIdentifierMessage_Click));
			MenuItems.Add(addManufacturerIdentifierMenu);

			MenuItems.Add("-");

			queryImporterBondMenu = new ZMenuItem(ResString.GetMultilingualString("Customs.US.OrganisationPlugIn.ImporterBondQuery", "Importer Bond Query"), new EventHandler(QueryImporterBondMenu_Click));
			MenuItems.Add(queryImporterBondMenu);

			MenuItems.Add("-");

			globalBusinessIdentifierAddDeleteUpdateMenu = new ZMenuItem(ResString.GetMultilingualString("Customs.US.OrganisationPlugIn.GlobalBusinessIdentifierAddDeleteUpdate", "Global Business Identifier Add/Delete/Update"), new EventHandler(GlobalBusinessIdentifierAddDeleteUpdateMenu_Click));
			MenuItems.Add(globalBusinessIdentifierAddDeleteUpdateMenu);
		}

		internal const string QueryImporterBond_NoIRSNumber = "This Organization has no Employer Identification Number/CBP Assigned Number/Social Security Number.\nThese numbers can be entered on the Details->Config Tab.";
		internal const string QueryImporterBond_NoValidIRSNumber = "This Organization has an invalid {0} entered on the Details->Config Tab.";
		internal const string QueryImporterBond_MessageSent = "A request has been sent for importer number : {0}. You will receive a response email shortly.";
		internal const string QueryCannotBeSentBecauseNotConsignee = "A request cannot be sent because this Organisation is not flagged as a Consignee organisation.";
		internal const string SaveOrganisationAdvice = "The Organisation has not yet been saved. Do you want to save and proceed?";

		void QueryImporterBondMenu_Click(object sender, EventArgs e)
		{
			if (!organisation.organisation.OH_IsConsignee)
			{
				Globals.Message.ShowInformation(QueryCannotBeSentBecauseNotConsignee);
				return;
			}

			if (organisation.organisation.HasChanges && (Globals.Message.Show(SaveOrganisationAdvice, "Save Organisation", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes || FireSaveButton() != ContinueWithSave.Yes))
			{
				return;
			}

			var cusCode = organisation.organisation.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });
			if (cusCode == null)
			{
				Globals.Message.ShowError(QueryImporterBond_NoIRSNumber);
			}
			else
			{
				var errorMessage = ZString.Empty;
				var type = ZString.Empty;
				switch (cusCode.OK_CodeType)
				{
					case OrgCusCode.USACodeTypes.EmployerIdentificationNumber:
						errorMessage = EmployerIdentificationNumberValidator.Validate(cusCode.OK_CustomsRegNo);
						type = "Employer Identification Number";
						break;
					case OrgCusCode.USACodeTypes.CBPAssignedNumber:
						errorMessage = CBPAssignedNumberValidator.Validate(cusCode.OK_CustomsRegNo);
						type = "CBP Assigned Number";
						break;
					default:
						if (!Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed)
						{
							errorMessage = SocialSecurityNumberValidator.DoesNotHavePermissionToSendSSNErrorMessage;
						}
						else
						{
							errorMessage = SocialSecurityNumberValidator.Validate(cusCode.OK_CustomsRegNo);
							type = "Social Security Number";
						}
						break;
				}

				if (!errorMessage.IsEmpty)
				{
					var builder = type.IsEmpty ? new ZStringBuilder() : new ZStringBuilder(string.Format(QueryImporterBond_NoValidIRSNumber, type));
					builder.Append(errorMessage);
					Globals.Message.ShowError(builder.ToStringWithNewLineBetweenAppends());
				}
				else
				{
					using (QueryImporterBondForm queryImporterBondForm = GetQueryImporterBondForm(cusCode.OK_CustomsRegNo))
					{
						ZFormModaliser.ShowDialogWithoutDispose(queryImporterBondForm);
						bool sendMessage = queryImporterBondForm.IsOKToSendMessage;

						if (sendMessage && FireSaveButton() == ContinueWithSave.Yes)
						{
							licenceLogIn();
							new ImporterNumberRequester().RequestImporterBond(organisation.organisation, queryImporterBondForm.numberToQuery);
							Globals.Message.ShowInformation(string.Format(QueryImporterBond_MessageSent, cusCode.OK_CustomsRegNo));
						}
					}
				}
			}
		}

		protected virtual QueryImporterBondForm GetQueryImporterBondForm(ZString idNumber)
		{
			return new QueryImporterBondForm(idNumber);
		}

		void SendImporterADDMessage_Click(object sender, EventArgs e)
		{
			if (!organisation.organisation.SecurityProvider.HasModifyConfigFinancialRegistrationNumbersSecurity)
			{
				Globals.Message.ShowInformation(HaveNotSecurityRightsForImporterAdd);
				return;
			}

			if (organisation.organisation.HasChanges && (Globals.Message.Show(SaveOrganisationAdvice, "Save Organisation", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes || FireSaveButton() != ContinueWithSave.Yes))
			{
				return;
			}

			bool sendMessage = false;
			var messageData = GetOrgAddressMessageData();
			if (messageData.HasPermissionToSendImporterBondNumber)
			{
				using (var msgDataForm = GetOrgAddressMessageDataForm(messageData))
				{
					ZFormModaliser.ShowDialogWithoutDispose(msgDataForm);
					sendMessage = msgDataForm.IsOKToSendMessage;
				}
			}
			else
			{
				Globals.Message.ShowError(SocialSecurityNumberValidator.DoesNotHavePermissionToSendSSNErrorMessage);
			}

			if (sendMessage)
			{
				licenceLogIn();
				new ImporterADDBuilder(messageData).Generate();
				if (FireSaveButton() == ContinueWithSave.Yes)
				{
					Globals.Message.ShowInformation("The message has been sent.", "Message Sent");
				}
			}
		}
		internal const string HaveNotSecurityRightsForImporterAdd = "You do not have the appropriate security rights to run this function.\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: Admin -> Reference Files -> Organisation -> Modify Config -> Modify Registration Numbers -> Modify Financial Registration Numbers";

		protected virtual OrgAddressMessageData GetOrgAddressMessageData()
		{
			return new OrgAddressMessageData(organisation);
		}
		protected virtual OrgAddressMessageDataForm GetOrgAddressMessageDataForm(OrgAddressMessageData messageData)
		{
			return new OrgAddressMessageDataForm(messageData);
		}

		void RequestEstablishmentIdentifierMessage_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(EDIMenu.NotSupportedByCBP);
		}

		void GlobalBusinessIdentifierAddDeleteUpdateMenu_Click(object sender, EventArgs e)
		{
			if (organisation.organisation.HasChanges && (Globals.Message.Show(SaveOrganisationAdvice, "Save Organisation", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes || FireSaveButton() != ContinueWithSave.Yes))
			{
				return;
			}

			var sendMessageType = GlobalBusinessIdentifierMessageType.None;
			var messageData = GetGlobalBusinessIdentifierData();
			using (var form = GetGlobalBusinessIdentifierForm(messageData))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				sendMessageType = form.MessageType;
			}

			if (sendMessageType != GlobalBusinessIdentifierMessageType.None)
			{
				licenceLogIn();

				new GlobalBusinessIdentifierMessageBuilder(messageData, sendMessageType).Generate();
				messageData.SaveGlobalBusinessIdentifiers();
				if (FireSaveButton() == ContinueWithSave.Yes)
				{
					Globals.Message.ShowInformation("The message has been sent.", "Message Sent");
				}
			}
		}

		protected virtual GlobalBusinessIdentifierData GetGlobalBusinessIdentifierData()
		{
			return new GlobalBusinessIdentifierData(organisation, true);
		}

		protected virtual GlobalBusinessIdentifierForm GetGlobalBusinessIdentifierForm(GlobalBusinessIdentifierData messageData)
		{
			return new GlobalBusinessIdentifierForm(messageData);
		}

		void AddManufacturerIdentifierMessage_Click(object sender, EventArgs e)
		{
			if (!organisation.organisation.OH_IsConsignor)
			{
				Globals.Message.ShowInformation(RequestCannotBeSentBecauseNotSupplier);
				return;
			}

			if (organisation.organisation.HasChanges && (Globals.Message.Show(SaveOrganisationAdvice, "Save Organisation", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes || FireSaveButton() != ContinueWithSave.Yes))
			{
				return;
			}

			var messageData = GetManufacturerAddMessageData();
			using (var form = GetManufacturerAddForm(messageData))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				switch (form.Result)
				{
					case ManufacturerAddFormResult.SendAddMessage:
						licenceLogIn();
						ManufacturerIdentifierManager.GenerateAddMessage(messageData);
						break;
					case ManufacturerAddFormResult.SendUpdateMessage:
						licenceLogIn();
						ManufacturerIdentifierManager.GenerateUpdateMessage(messageData);
						break;
					case ManufacturerAddFormResult.SendQueryMessage:
						licenceLogIn();
						ManufacturerIdentifierQueryBuilder.GenerateMessageAndAttachToOrganisation(organisation.organisation, messageData.US_MID);
						break;
					default:
						return;
				}
				if (FireSaveButton() == ContinueWithSave.Yes)
				{
					Globals.Message.ShowInformation("The message has been sent.", "Message Sent");
				}
			}
		}
		internal const string RequestCannotBeSentBecauseNotSupplier = "A request cannot be sent because this Organisation is not flagged as a Supplier organisation.";

		protected virtual ManufacturerAddMessageData GetManufacturerAddMessageData()
		{
			return new ManufacturerAddMessageData(organisation);
		}

		protected virtual ManufacturerAddForm GetManufacturerAddForm(ManufacturerAddMessageData messageData)
		{
			return new ManufacturerAddForm(messageData);
		}

		ZForm GetParentForm()
		{
			MainMenu mainMenu = GetMainMenu();
			return mainMenu == null ? null : (ZForm)mainMenu.GetForm();
		}

		ContinueWithSave FireSaveButton()
		{
			ZForm parentForm = GetParentForm();
			return parentForm != null ? parentForm.FireSaveButton() : ContinueWithSave.No;
		}
	}
}
