using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using State = Enterprise.Freight.Agency.GUI.PortAuthorityFilterControl.State;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed partial class PortAuthorityFilterDialog : ZChildForm
	{
		internal PortAuthorityFilterDialog(PortAuthority sender)
			: base(sender)
		{
			InitializeComponent();
			warningLabel.Text = Res.GetString("PortAuthority|WarningLabel", "Please ensure all relevant bills have been correctly entered before attempting to send a Port Authority Message as you may not be able to send an amendment.");
		}

		public PortMessageIssue CurrentIssue
		{
			get { return filterControl.CurrentIssue; }
		}

		public static void ShowDialog(JobVoyage voyage)
		{
			if (voyage.HasChanges || !voyage.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("ead9855c-b416-4c65-8cac-1cc6f40f5c12", "This schedule has changes, please save and try again."), Res.GetString("2c4f27bb-ce75-4ca1-a465-7d9a5da91923", "Unsaved Changes"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (!voyage.IsMainVoyage && voyage.FindOtherVoyagesWithSameVesselVoyageCombination().Any(v => v.IsMainVoyage))
			{
				Globals.Message.ShowInformation(Res.GetString("5cef03fb-bba4-49dd-8513-9843a42351c1", "You have Main and Slot schedules for this vessel. Please send Port Authority message from the Main schedule."));
			}
			else if (!Env.Security.SailingSchedulePortMessaging.IsAllowed)
			{
				Env.Security.SailingSchedulePortMessaging.ShowError();
			}
			else
			{
				voyage.RunPreSaveValidation();

				if (voyage.HasErrors)
				{
					Globals.Message.Show(Res.GetString("76d060ed-40ad-4118-928d-2c06e6447dc3", "This schedule has errors, please fix and try again."), Res.GetString("7a095504-5988-4971-bb26-2d0081272637", "Errors"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					PortAuthority filter = new PortAuthority(voyage);

					using (filter.SuspendSettingHasChanges())
					using (filter.GetValidationSuspender())
					{
						filter.Restore3rdPartySettings();
					}

					using (PortAuthorityFilterDialog form = new PortAuthorityFilterDialog(filter))
					{
						ZFormModaliser.ShowDialogWithoutDispose(form);
					}
				}
			}
		}

		public void PerformClickSend()
		{
			filterControl.PerformClickSend();
		}

		public void PerformClickCancel()
		{
			filterControl.PerformClickCancel();
		}

		#region Implementation

		protected override void PopulateDevTools(List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new PortAuthorityIssueDetailDevTool());
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("2d174c91-a6d4-4373-b246-e1da68451e4c", "message"), Res.GetString("7e1be6dd-001f-44dc-bd48-920a30e2d1a3", "send"), Res.GetString("05134ac9-c41d-41c6-939a-bd34976bf3fc", "sent"), includeIgnoreOption);
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		PortAuthority Filter
		{
			get { return (PortAuthority)BusinessEntity; }
		}

		bool ValidateAndSend()
		{
			bool success = false;

			try
			{
				filterControl.SetState(State.Locked);

				Filter.RunPreSaveValidation();
				if (Filter.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					filterControl.SetState(State.CollectingData);

					IPortAuthorityMessagingData data = Filter.ExtractData();

					filterControl.SetState(State.Locked);

					if (Filter.Issues.Count > 0)
					{
						string caption = Res.GetString("55913eee-06fa-4dba-9d2e-9c016821bf37", "Error");
						string message = Res.GetString("f729c4e4-6d8b-4a52-a096-c4905798cd92", "Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", Filter.Issues.Count);

						Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					else if (data.Consignments.Any() || QuerySendEmpty())
					{
						filterControl.SetState(State.BuildingMessage);

						EDIMessage message = null;
						if (Filter.DeliverTo3rdParty)
						{
							GenerateEmail(data);
							Filter.Persist3rdPartySettings();
						}
						else
						{
							message = PortAuthorityMessage.New(Filter.GetEndPoint(), data, Filter.Setting.Version, Filter.MessageType, Filter.Factory.Load<OrgHeader>(Filter.PrincipalPK)?.OH_Code ?? ZString.Empty);
						}

						filterControl.SetState(State.Saving);

						try
						{
							ZExceptionReporting.ProcessWithSaveExceptionHandling(Filter.Factory.Save, null);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							message?.Delete();
							throw;
						}
						success = true;

						var logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();

						for (int i = 0; i < data.Consignments.Count(); i++)
						{
							logger.CreateLog(Env.Licence.ShippingManagerPortAuthorityMessagingPerTransaction, true);
						}
					}
				}
			}
			finally
			{
				filterControl.SetState(State.None);
			}

			return success;
		}

		void GenerateEmail(IPortAuthorityMessagingData data)
		{
			IPortAuthorityMessageBuilder builder = PortAuthorityMessageBuilderFactory.GetBuilder(Filter.Version);
			string messageText = builder.GetInterchangeText(data, ZDateTime.Now, Filter.SenderId, Filter.RecipientId);

			EmailDef def = new EmailDef();
			def.Subject = MessagingHelper.GetPortAuthoritySubject(Filter.GetEndPoint(), Filter.SenderId);
			def.AddRecipientForSystemCommunication(Filter.EmailAddress);
			def.Attachments.Add(new AttachmentDef("message.edi", Encoding.UTF8.GetBytes(messageText)));

			Env.OutgoingMailManager.Create(Filter.Factory, def);
		}

		bool QuerySendEmpty()
		{
			string caption = Res.GetString("2c7b4ab1-b761-4543-a500-adf032b2e8bf", "Warning");
			string message;

			switch (Filter.Direction)
			{
				case Constants.PortDirection.Load:
					message = Res.GetString("ada69df8-48f4-4762-99fa-376dc2eac21f", "There are no shipments on this voyage loading in {0}. Are you sure you want to send an empty manifest?", Filter.Port);
					break;

				case Constants.PortDirection.Discharge:
					message = Res.GetString("25f471b5-c9b9-463a-abcc-34bb8ccb1e56", "There are no shipments on this voyage discharging in {0}. Are you sure you want to send an empty manifest?", Filter.Port);
					break;

				default:
					throw new InvalidOperationException("unknown direction: " + Filter.Direction);
			}

			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes;
		}

		void filterControl_SendClicked(object sender, EventArgs e)
		{
			if (ValidateAndSend())
			{
				Globals.Message.Show(Res.GetString("b41be89a-7f9d-4f03-a1ca-2f0833e84fba", "Message successfully created and queued for delivery."), Res.GetString("3f707300-d982-4d1d-8e94-97ed4438484e", "Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				Close();
			}
		}

		void filterControl_CancelClicked(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}



