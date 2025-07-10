using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public abstract class CusIntegrationProvider : ICusIntegration
	{
		#region ICusIntegration Members

		public ZString Execute(Integration.Customs.IBaseJobDeclaration declaration)
		{
			Argument.NotNull(declaration, "Declaration");
			this.declaration = declaration as BaseJobDeclaration;

			ZString result = ValidateCanSubmit();

			if (string.IsNullOrEmpty(result))
			{
				result = CheckDeniedParty(this.declaration);
			}

			if (string.IsNullOrEmpty(result))
			{
				var submittedData = GetDataToSubmit();
				if (submittedData.IsEmpty && !ErrorMessageFromToSubmitData.IsEmpty)
				{
					result = ErrorMessageFromToSubmitData;
					return result;
				}
				var submitResult = Submit(submittedData);

				result = ProcessSubmissionResult(submitResult, submittedData);
				SaveFactory();
			}

			return result;
		}

		protected BaseJobDeclaration declaration;

		#endregion

		#region GetDataFromBusinessObject

		protected ZString ErrorMessageFromToSubmitData { get; set; }

		protected virtual ZString GetDataToSubmit()
		{
			var shipment = Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration, DataContextType.CustomsDeclaration);
			var actionInfo = new ActionInfo(RecipientRoleType.ORP, declaration);
			new DataContextDataObjectWriter().PopulateDataObject(actionInfo, declaration, shipment.DataContext);

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var xmlWriter = new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter();
				xmlWriter.WriteXML(shipment, stream, WriteXMLDeclaration);

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		protected virtual ZBool WriteXMLDeclaration => true;

		#endregion

		#region Process Submission Result

		protected ZString ProcessSubmissionResult(XElement submissionResult, ZString submittedData)
		{
			ZString result = "";

			if (SubmitSucceeded(submissionResult))
			{
				OnSubmitSucceed(submittedData);
				result = SubmitSucceededMessage;
			}
			else
			{
				result = GetSubmitFailedReasons(submissionResult);
			}

			return result;
		}

		protected virtual string SubmitSucceededMessage => Res.GetString("e3619b03-3e1a-4ad3-aafe-902c3ef17245", "Submit Succeeded.");

		protected abstract bool SubmitSucceeded(XElement submissionResult);

		protected void OnSubmitSucceed(ZString submittedData)
		{
			declaration.JE_EntryStatus = EntryStatus;
			if (ShouldLogCustomsCommenced)
			{
				declaration.LogCustomsCommencedIfNeeded();
			}
			CreateEDIMessage(submittedData);
			declaration.DiscardedMessages.Load();
		}

		protected virtual ZString EntryStatus => CustomsWareEntryStatusList.Codes.Submitted;

		protected virtual bool ShouldLogCustomsCommenced => true;

		protected virtual EDIMessage CreateEDIMessage(ZString submittedData)
		{
			//create an edimessage and save it as a record of the submission
			var submitMessage = declaration.Messages.AddNew();
			submitMessage.MessageNumberStrategy = new SubmitMessageNumberStrategy();
			submitMessage.EM_ApplicationCode = ApplicationCode;
			submitMessage.EM_MessageType = ApplicationCode;
			submitMessage.EM_MessageText = submittedData;
			submitMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			submitMessage.EM_Status = SubmitMessageStatus;

			// create a dataexport event
			if (ShouldCreateDataExportEventAfterSubmitMessageCreation)
			{
				new MessageDataExportImportLogLinker(Events.DataExport, declaration.Factory).LinkMessageToParentBOLogs(submitMessage, declaration);
			}
			return submitMessage;
		}

		protected virtual ZString SubmitMessageStatus => EDIMessage.Status.Sent;

		protected virtual bool ShouldCreateDataExportEventAfterSubmitMessageCreation => true;

		protected virtual ZString GetSubmitFailedReasons(XElement submissionResult)
		{
			ZString result = Res.GetString("e3619b03-3e1a-4ad5-aafe-902c3ef17245", "Submit Failed.");
			var response = submissionResult.Descendants("RequestResponse").FirstOrDefault();
			return response == null ? result : (ZString)(result + "\n" + response.Value);
		}

		#endregion

		protected abstract XElement Submit(ZString submittedData);
		protected abstract ZString ApplicationCode { get; }

		void SaveFactory()
		{
			try
			{
				declaration.Factory.Save();
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}

		protected virtual ICollection<SettingDetail> SettingsToValidate
		{
			get { return new List<SettingDetail>(); }
		}

		ZString ValidateCanSubmit()
		{
			var errorList = new ZStringBuilder();

			foreach (SettingDetail setting in SettingsToValidate)
			{
				errorList.AppendIfNotEmpty(CheckSettingIsNotEmpty(setting));
			}

			ZString result = errorList.ToString();

			return result.IsEmpty ? string.Empty : result + string.Format(RegistrySetCaption, RegistryLocation);
		}

		ZString CheckSettingIsNotEmpty(SettingDetail setting)
		{
			return string.IsNullOrEmpty(setting.FieldValue) ? ZString.Format(SettingEmptyErrorMessage, setting.FieldName, ProviderName) : ZString.Empty;
		}

		string CheckDeniedParty(BaseJobDeclaration dec)
		{
			var creditCheckManager = new MessageManagerCreditCheckWithSecurityHelper(dec);
			return creditCheckManager.IsDeniedPartyOKToSend ? "" : creditCheckManager.ReasonForNotAllowed;
		}

		protected abstract ZString ProviderName { get; }
		protected abstract ZString RegistryLocation { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Can be different because of ProviderName")]
		internal const string SettingEmptyErrorMessage = "{0} of {1} should not be empty or invalid.\n";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Can be different because of RegistryLocation.")]
		internal const string RegistrySetCaption = "\nThis can be entered in the Registry: {0}.";

		class SubmitMessageNumberStrategy : IMessageNumberStrategy
		{
			string IMessageNumberStrategy.GetMessageReferenceNumber()
			{
				return ZDateTime.Now.Ticks.ToString();//long is 19, message num is 20. Just allocating a number for the sake of it. This number is not used in the message so really it does nothing.
			}
		}

		internal protected class SettingDetail
		{
			public SettingDetail(ZString fieldValue, ZString fieldName)
			{
				this.FieldValue = fieldValue;
				this.FieldName = fieldName;
			}

			internal ZString FieldValue { get; set; }
			internal ZString FieldName { get; set; }
		}
	}
}
