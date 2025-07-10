using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDShipmentWrapper : CMDWrapperBase
	{
		enum MessageFilter { ActiveOnly, All }

		public CMDShipmentWrapper(ForwardingShipment shipment)
			: base(shipment.Factory)
		{
			this.Shipment = shipment;
		}

		public override CMDShipmentWrapper[] CMDShipments
		{
			get { return new CMDShipmentWrapper[] { this }; }
		}

		public BaseJobComInvoiceLine[] GetShipmentInvoiceLines()
		{
			List<BaseJobComInvoiceLine> result = new List<BaseJobComInvoiceLine>();

			foreach (BaseJobDeclaration declaration in Shipment.Declarations)
			{
				BaseJobComInvoiceLine[] lines = (BaseJobComInvoiceLine[])declaration.InvoiceLines.ToArray(typeof(BaseJobComInvoiceLine));
				result.AddRange(lines);
			}

			return result.ToArray();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Filter = MessageFilter.ActiveOnly;
		}

		#region ShowActiveMessagesOnly

		[BusinessObjectTestExclude]
		public ZBool ShowActiveMessagesOnly
		{
			get { return (Filter == MessageFilter.ActiveOnly); }
			set
			{
				if (value)
				{
					Filter = MessageFilter.ActiveOnly;
				}
				ShowActiveMessagesOnlyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowActiveMessagesOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(ShowActiveMessagesOnly)); }
		}

		#endregion

		#region ShowAll

		[BusinessObjectTestExclude]
		public ZBool ShowAll
		{
			get { return (Filter == MessageFilter.All); }
			set
			{
				if (value)
				{
					Filter = MessageFilter.All;
				}
				ShowAllInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowAllInfo
		{
			get { return GetZPropertyInfo(nameof(ShowAll)); }
		}

		#endregion

		#region Messages

		public CMDEDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new CMDEDIMessageCollection(this);
					LoadMessages();
				}
				return fMessages;
			}
		}

		public void RefreshMessageList()
		{
			LoadMessages();
		}

		void LoadMessages()
		{
			if (fMessages != null)
			{
				if (Filter == MessageFilter.ActiveOnly)
				{
					Messages.Load(new ZQuery(EDIMessageSchema.EM_IsActive, ZBool.True));
				}
				else
				{
					Messages.Load();
				}

				RefreshCurrentCMDStatus();
			}
		}

		void RefreshCurrentCMDStatus()
		{
			fCurrentCMDStatus = "";
			CurrentCMDStatusInfo.RefreshBinding();
		}

		MessageFilter Filter
		{
			get { return fFilter; }
			set
			{
				if (value != fFilter)
				{
					fFilter = value;
					LoadMessages();
				}
			}
		}

		MessageFilter fFilter;
		CMDEDIMessageCollection fMessages;

		#endregion

		#region Current CMD Status

		public ZString CurrentCMDStatus
		{
			get
			{
				if (fCurrentCMDStatus.IsEmpty)
				{
					CurrentCMDStatusInfo.ClearAllNotifications();
					CMDEDIMessage lastCMD = GetLastCMDSent();
					if (lastCMD == null)
					{
						fCurrentCMDStatus = NoMessagesSent;
					}
					else if (lastCMD.Reply == null || !lastCMD.Reply.IsValid)
					{
						fCurrentCMDStatus = NoRepliesReceived;
					}
					else if (lastCMD.Reply.IsErrorMessage)
					{
						fCurrentCMDStatus = MessageError;
						if (!IsValidationSuspended)
						{
							CurrentCMDStatusInfo.AddMessageError(MessageError);
						}
					}
					else
					{
						fCurrentCMDStatus = MessageAcknowledged;
					}
				}
				return fCurrentCMDStatus;
			}
		}

		public ZPropertyInfo CurrentCMDStatusInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentCMDStatus)); }
		}

		public CMDEDIMessage GetLastCMDSent()
		{
			CMDEDIMessage result = null;

			ZQuery filter = new ZQuery(EDIMessageSchema.EM_MessageSubType, CMDGenerator.Constants.GHA);
			filter.AddToFilter(EDIMessageSchema.EM_IsActive, ZBool.True);

			CMDEDIMessage[] messagesToGHA = (CMDEDIMessage[])Messages.Find(filter);
			ZDateTime lastDateTime = ZDateTime.Empty;
			foreach (CMDEDIMessage message in messagesToGHA)
			{
				if (message.EM_StatusDateTime > lastDateTime || lastDateTime.IsEmpty)
				{
					lastDateTime = message.EM_StatusDateTime;
					result = message;
				}
			}

			return result;
		}

		ZString fCurrentCMDStatus;
		const string MessageError = "The current CMD message has an error";
		const string NoRepliesReceived = "No replies have been received for the current CMD";
		const string NoMessagesSent = "No messages have been sent";
		const string MessageAcknowledged = "The current CMD has been succesfully sent and acknowledged";

		#endregion

		#region Permit and Exemptions

		[ChildEditable(true)]
		public CMDPermitNumberCollection CMDDataValues
		{
			get
			{
				if (fCMDDataValues == null)
				{
					fCMDDataValues = new CMDPermitNumberCollection(this.Shipment);
					RegisterEditableChildObject(fCMDDataValues);
					fCMDDataValues.Load();
				}

				return fCMDDataValues;
			}
		}
		CMDPermitNumberCollection fCMDDataValues;

		public ZString PermitAndExemptionDetails
		{
			get
			{
				if (fPermitAndExemptionDetails == null)
				{
					fPermitAndExemptionDetails = ConstructPermitAndExemptionDetails();
				}
				return fPermitAndExemptionDetails;
			}
		}

		public ZPropertyInfo PermitAndExemptionDetailsInfo
		{
			get { return GetZPropertyInfo(nameof(PermitAndExemptionDetails)); }
		}

		public void ResetPermitAndExemptionDetails()
		{
			fPermitAndExemptionDetails = null;
			PermitAndExemptionDetailsInfo.RefreshBinding();
			if (PermitAndExemptionDetails.Length > 0 && Shipment.CustomsEntryNumber.IsEmpty)
			{
				Shipment.CustomsEntryNumberType = PermitAndExemptionDetails.SubstringSafe(0, 3);
				Shipment.CustomsEntryNumber = PermitAndExemptionDetails.SubstringSafe(4);
				Shipment.CustomsEntryNumberTypeInfo.RefreshBinding();
				Shipment.CustomsEntryNumberInfo.RefreshBinding();
			}
		}

		public CMDPermitNumber[] GetTDBPermits()
		{
			ZQuery filter = new ZQuery(CusCodeDataSchema.CY_ParentID, Shipment.PK);
			filter.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			filter.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.CMD);
			filter.AddToFilter(CusCodeDataSchema.CY_Code, CustomsEntryTypeList.Singapore.Permit);
			return Shipment.Factory.Load<CMDPermitNumber>(filter);
		}

		public CMDPermitNumber[] GetTDBPermitsAndExemptions()
		{
			ZQuery filter = new ZQuery(CusCodeDataSchema.CY_ParentID, Shipment.PK);
			filter.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			filter.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.CMD);
			return Shipment.Factory.Load<CMDPermitNumber>(filter);
		}

		public CMDPermitNumber GetTDBExemption()
		{
			CMDPermitNumber[] cmdColl = GetTDBPermitsAndExemptions();
			foreach (CMDPermitNumber number in cmdColl)
			{
				if (CustomsEntryTypeList.Singapore.SGExemption.IsTDBExemption(number.CY_Code))
				{
					return number;
				}
			}

			return null;
		}

		public bool HasTDBPermitFromDeclarations()
		{
			foreach (var declarationBO in Shipment.Declarations)
			{
				BaseJobDeclaration declaration = declarationBO as BaseJobDeclaration;
				if (declaration != null && declaration.Branch != null && declaration.Branch.Company != null)
				{
					if (declaration.Branch.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore)
					{
						if (declaration.DeclarationNumber != "")
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		ZString ConstructPermitAndExemptionDetails()
		{
			List<string> entryNumberDetails = new List<string>();

			foreach (CMDPermitNumber permit in GetTDBPermitsAndExemptions())
			{
				entryNumberDetails.Add(string.Format("{0}: {1}", permit.CY_Code, permit.CY_Data));
			}

			return string.Join(" | ", entryNumberDetails.ToArray());
		}
		string fPermitAndExemptionDetails;

		#endregion

		#region CMDDataValuesForBinding

		public CMDDataValueWrapperCollection CMDDataValuesForBinding
		{
			get
			{
				if (fCusEntryNumbersForBinding == null)
				{
					fCusEntryNumbersForBinding = new CMDDataValueWrapperCollection(this);
					fCusEntryNumbersForBinding.Load();
				}
				return fCusEntryNumbersForBinding;
			}
		}
		CMDDataValueWrapperCollection fCusEntryNumbersForBinding;

		#endregion

		#region Send / Delete CMD

		public override void SendMessage(INotifications notifications)
		{
			CMDGenerator generator = new CMDGenerator(this);
			generator.GenerateSendMessages();
			RefreshMessageListAndNotify(notifications, generator);
		}

		public override void SendMessage(string recipient, INotifications notifications)
		{
			CMDGenerator generator = new CMDGenerator(this, recipient);
			generator.GenerateSendMessages();
			RefreshMessageListAndNotify(notifications, generator);
		}

		public override void DeleteExistingCMDMessages(INotifications notifications)
		{
			CMDGenerator generator = new CMDGenerator(this);
			generator.GenerateWithdrawMessages();
			RefreshMessageListAndNotify(notifications, generator);
		}

		void RefreshMessageListAndNotify(INotifications notifications, CMDGenerator generator)
		{
			RefreshMessageList();
			NotifyNoOfMessagesSent(notifications, generator.NoOfMessagesSent);
		}

		void NotifyNoOfMessagesSent(INotifications notifications, int noOfMessages)
		{
			string message = (noOfMessages > 0)
					? string.Format("{0} CMD Message(s) sent", noOfMessages)
					: "No CMD Messages sent, CMD data unchanged since last submission";

			NotifyCMDInfo(notifications, message, Shipment);
		}

		#endregion

		#region Validation

		public override void RunPreSendValidation(INotifications notifications)
		{
			ValidatePermitsOrExemptionCode(notifications);
			ValidateConsols(notifications);
			if (!HasTDBPermitFromDeclarations())
			{
				ValidateOuterPackLineDescription(notifications);
				ValidateTotalOuterPacks(notifications);
			}
		}

		public override void RunPreDeleteValidation(INotifications notifications)
		{
			if (Messages.Count < 1)
			{
				NotifyCMDError(notifications, "There is no previously sent CMD Messages to be deleted", Shipment);
			}
		}

		void ValidatePermitsOrExemptionCode(INotifications notifications)
		{
			CMDPermitNumber[] tDBPermits = GetTDBPermits();
			CMDPermitNumber tDBExemption = GetTDBExemption();
			if (tDBPermits.Length == 0 && tDBExemption == null)
			{
				NotifyCMDError(notifications, "There is no Exemption Code or Permit Numbers entered to send", Shipment);
			}
		}

		void ValidateConsols(INotifications notifications)
		{
			bool hasAtLeastOneValidConsol = false;
			foreach (ForwardingConsol consol in Shipment.Consols)
			{
				if (consol.IsAir && (consol.IsExport() || consol.IsImport()))
				{
					hasAtLeastOneValidConsol = true;
					break;
				}
			}

			if (!hasAtLeastOneValidConsol)
			{
				NotifyCMDError(notifications, "This shipment is not attached to a Consolidation. MAWB details are required for CMD", Shipment);
			}
		}

		void ValidateOuterPackLineDescription(INotifications notifications)
		{
			foreach (PackLine packLine in Shipment.OuterPackLines)
			{
				if (packLine.JL_Description.IsEmpty || packLine.JL_HarmonisedCode.IsEmpty)
				{
					string message = string.Format("Both {0} and {1} are required for CMD", packLine.JL_DescriptionInfo.HumanReadableName, "Harmonised Code");
					NotifyCMDError(notifications, message, packLine);
				}
			}
		}

		void ValidateTotalOuterPacks(INotifications notifications)
		{
			if (Shipment.TotalOuterPacks.IsEmpty)
			{
				string message = string.Format("{0} has to be greater than zero", Shipment.TotalOuterPacksInfo.HumanReadableName);
				NotifyCMDError(notifications, message, Shipment);
			}
		}

		#endregion

		public readonly ForwardingShipment Shipment;
	}
}
