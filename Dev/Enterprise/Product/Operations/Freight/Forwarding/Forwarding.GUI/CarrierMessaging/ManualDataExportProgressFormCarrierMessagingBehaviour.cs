using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.GUI;
using Enterprise.NumberFountain;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed class ManualDataExportProgressFormCarrierMessagingBehaviour : ManualDataExportProgressFormBehaviour
	{
		public ManualDataExportProgressFormCarrierMessagingBehaviour(string caption, IWorkflowProvider data, ICarrierMessagingValidation validation = null)
		{
			Argument.NotNull(data, "data");
			this.caption = caption;
			this.validation = Argument.NotNull(validation, "validation");
			this.consol = data as ForwardingConsol;
		}

		readonly string caption;
		readonly ICarrierMessagingValidation validation;
		readonly ForwardingConsol consol;

		protected override void ApplyCore(IManualDataExportProgressForm progressForm)
		{
			progressForm.TitleLabel.Text = validation.IsForwardAir()
				? Res.GetString("6fb0b210-b881-41df-82af-2aa1150ceaa0", "Send Forward Air Booking Request")
				: caption;

			progressForm.Form.Shown += (s, e) => CheckIfMessageCanBeCreated(progressForm);
			progressForm.SendButton.Click += (s, e) => SendData(progressForm);
		}

		void CheckIfMessageCanBeCreated(IManualDataExportProgressForm progressForm)
		{
			validation.Validate(this);

			if (HasNotificationErrors)
			{
				progressForm.SendButton.Enabled = false;
				progressForm.CloseButton.Focus();
			}
			else
			{
				progressForm.SendButton.Focus();
			}
		}

		void SendData(IManualDataExportProgressForm progressForm)
		{
			progressForm.SendButton.Enabled = false;

			var messageCaption = Res.GetString("5ca10a0c-e98f-4212-aa8a-f8d945743a84", "Unable to complete action");
			try
			{
				if (consol.JK_MasterBillNum.IsEmpty)
				{
					consol.Factory.Saving += UpdateMasterBillNumberOnSaving;
					consol.Factory.Saved += EmptyMasterBillNumberIfNeedOnSaved;

					consol.Factory.Save();
				}

				validation.ValidateMasterBillNumber(this);
				if (!HasNotificationErrors)
				{
					ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
					{
						var logsFactory = new BusinessObjectFactory();
						using (logsFactory.AddDisposableService())
						{
							var consolOnLogsFactory = (ForwardingConsol)logsFactory.ImportFromAnotherFactory(consol);
							using (var dataExport = new ManualDataExport(logsFactory, new[] { consolOnLogsFactory }, UniversalDataType.UniversalShipment, schema: UniversalXmlSchema.Version_2011_11))
							{
								dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Carrier;
								dataExport.SendData(this);
							}

							if (validation.IsForwardAir())
							{
								consolOnLogsFactory.Logs.CreateOrRecreateEventLog(
									AutoEvents.MessageSent,
									EstimateActual.Actual,
									ZDateTimeOffset.UtcNow,
									ZString.Empty,
									GetParametersForEvent());

								logsFactory.Save();
							}
							else
							{
								logsFactory.Save();
							}
						}
						((INotifications)this).Add(new InfoNotification((NoResString)"Delivery succeeded.")); // Service task logs aren't res strings
					}, () => this.AddWarning((NoResString)"Error during delivery. Retrying.")); // Service task logs aren't res strings
				}
			}
			catch (NumberFountainException)
			{
				var message = Res.GetString("4ad2a440-f7eb-42c5-af69-b36067d9c360"
					, @"Forward Air bill number cannot be allocated as no numbers are left in range.
Please close this form and contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges");

				Globals.Message.Show(message, messageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (ZSaveConcurrencyException)
			{
				var message = Res.GetString("58e5d9c8-44ae-4c98-8bf4-a3aa2c5674fc"
					, @"While you have been working with this form attempting to send an electronic Booking Request message, another user has made changes which cannot be merged.{0}
Your electronic message was not sent.{0}
Please close and re-open the Consol form to continue."
					, System.Environment.NewLine);

				Globals.Message.Show(message, messageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				consol.Factory.Saving -= UpdateMasterBillNumberOnSaving;
				consol.Factory.Saved -= EmptyMasterBillNumberIfNeedOnSaved;
			}

			progressForm.CloseButton.Focus();
		}

		void UpdateMasterBillNumberOnSaving(BusinessObjectFactory factory)
		{
			var stmNums = consol.ForwardAirBillStmNums;
			if (stmNums != null)
			{
				var fountain = stmNums.TryGetNumberFountain();
				if (fountain != null)
				{
					consol.JK_MasterBillNum = fountain.GetNextFormatted(factory);
				}
			}
		}

		void EmptyMasterBillNumberIfNeedOnSaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!savedSuccessfully)
			{
				consol.JK_MasterBillNum = string.Empty;
			}
		}

		static KeyValuePair<string, string>[] GetParametersForEvent()
		{
			return new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, (NoResString)"Carrier Booking Request"), // Message for event reference
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, (NoResString)"Forward Air") // Message for event reference
			};
		}
	}
}
