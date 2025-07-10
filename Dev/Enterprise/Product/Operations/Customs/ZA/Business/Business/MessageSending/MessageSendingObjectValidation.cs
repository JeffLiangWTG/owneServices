using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business
{
	public class MessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(MessageSendingObject parent) : base(parent)
		{
		}

		public new MessageSendingObject Parent => base.Parent as MessageSendingObject;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidatePaymentMethod();
			ValidateCaseNumber();
		}

		public void ValidatePaymentMethod()
		{
			ValidateCalculatedProperty(Parent.PaymentMethodInfo);
		}

		protected void CheckPaymentMethod()
		{
			if (Parent != null)
			{
				var amountDueDifference = Parent.Header.AmountDueDifference;
				if (Parent.PaymentMethod != PaymentMethodCodeList.Codes.Free)
				{
					if (amountDueDifference.IsEmpty)
					{
						Parent.PaymentMethodInfo.AddMessageError(PaymentCodeOfFreeExpected);
					}

					var decType = Parent.DeclarationType;
					if (decType == DeclarationTypeList.Codes.RegularIncompleteDeclaration || decType == DeclarationTypeList.Codes.RegularProvisionalDeclaration)
					{
						Parent.PaymentMethodInfo.AddMessageError(PaymentCodeFreeRequiredForRIDorRPD);
					}
				}
			}
		}

		public void ValidateCaseNumber()
		{
			ValidateCalculatedProperty(Parent.CaseNumberInfo);
		}

		protected void CheckCaseNumber()
		{
			if (Parent != null)
			{
				var targetInfo = Parent.CaseNumberInfo;
				if (Parent.ShouldSend && !targetInfo.ReadOnly)
				{
					var sourceNumber = Parent.CaseNumber;
					if (sourceNumber.KeepAlphanumericCharacters() != sourceNumber)
					{
						targetInfo.AddMessageError(Res.GetString("12F965C6-84B4-4325-BA82-733D65871AB4", "The Case Number can only be alphanumeric."));
					}
					else if (sourceNumber.IsEmpty && Parent.Header.IsAmendmentNotificationReceived)
					{
						targetInfo.AddMessageError(Res.GetString("FEC9F6C6-0278-47D5-A608-17CE370BAE18", "Case Number is required when Amendment Notification is received."));
					}
				}
			}
		}

		protected override void CheckDeclarationType()
		{
			if (Parent.ShouldSend)
			{
				base.CheckDeclarationType();
				if (TwoStepDeclarationHelper.IsTwoStepClearingValid)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DeclarationTypeInfo);
					var declarationType = Parent.DeclarationType;
					if (declarationType == DeclarationTypeList.Codes.RegularIncompleteDeclaration || declarationType == DeclarationTypeList.Codes.RegularProvisionalDeclaration)
					{
						var declaration = Parent.Header.Declaration;
						if (declaration.JE_MarksAndNumbers.IsEmpty)
						{
							Parent.DeclarationTypeInfo.AddMessageError(Res.GetString("9ADFC2C6-E4DD-498A-975A-FE52FEF64A49", "Marks & Numbers is required for declaration types 'RID' and 'RPD'."));
						}
					}

					ValidatePaymentMethod();
				}
			}
		}

		protected override void CheckChangeAcknowledgementIndicator()
		{
			if (Parent != null)
			{
				var targetInfo = Parent.ChangeAcknowledgementIndicatorInfo;
				if (Parent.ShouldSend && !targetInfo.ReadOnly)
				{
					if (Parent.Header.IsAmendmentNotificationReceived)
					{
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCode(targetInfo);
					}
				}
			}
		}

		protected override void CheckMessageType()
		{
			if (Parent?.ShouldSend ?? ZBool.False)
			{
				var sourceValue = Parent.MessageType;
				var targetInfo = Parent.MessageTypeInfo;
				var parentMRN = Parent.MovementReferenceNumber;

				if (!targetInfo.Value.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(targetInfo);
					if (sourceValue == MessageSubTypeCodes.Codes.Change && parentMRN.IsEmpty && !Parent.IsMRNEditable)
					{
						targetInfo.AddMessageError(MRNIsRequiredForVOCMessage);
					}
					else if (sourceValue == MessageSubTypeCodes.Codes.Cancellation && !parentMRN.IsEmpty)
					{
						var currentCode = Parent.Header?.CH_PaymentMethod ?? ZString.Empty;
						if (!currentCode.IsEmpty && currentCode != PaymentMethodCodeList.Codes.Free)
						{
							targetInfo.AddWarning(PaymentMethodForcedToFreeForCancellation(currentCode));
						}
					}
					else if (sourceValue == MessageSubTypeCodes.Codes.Replace && (Parent.Header.Declaration?.IsExWarehouse ?? false))
					{
						targetInfo.AddMessageError(ReplacementEntryIsNotAllowedForExBond);
					}
				}
				else
				{
					targetInfo.AddError(MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckShouldSend()
		{
			if (Parent.ShouldSend)
			{
				var registryIntervalMinutes = ZACustomsRegistry.Instance.MessageSendingInterval.Value;
				var lastMessageSentTime = Parent.LastMessageSentTime;
				if (registryIntervalMinutes != 0 && lastMessageSentTime.IsValid)
				{
					var allowTimeUTC = lastMessageSentTime.AddMinutes(registryIntervalMinutes);
					if (allowTimeUTC > ZDateTime.UtcNow)
					{
						var timeProxy = EnvProxy.Instance.Time;
						Parent.ShouldSendInfo.AddMessageError(Res.GetString("F06D33A9-F70B-49A7-AC56-E59D819F1466", "LRN {0} was last submitted at {1:yyyy-MM-dd HH:mm:ss} please wait till {2:yyyy-MM-dd HH:mm:ss} before submitting again.", Parent.LocalReferenceNumber, timeProxy.GetLocalTimeFromUtc(lastMessageSentTime.ToDateTime()), timeProxy.GetLocalTimeFromUtc(allowTimeUTC.ToDateTime())));
					}
				}

				if (Parent.LocalReferenceNumber.IsEmpty && !Parent.IsLRNEditable)
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("1FC9374E-82D4-4DAC-A086-FDDC1A6A3034", "Message should not be selected to send with missing Local Reference Number.\r\nPlease check if the Agent Code and/or the Customs Office is missing from the declaration."));
				}

				var jobNumber = ValidationHelper.GetJobNumberOfDuplicateUCR(Parent.Factory, Parent.Header.UniqueConsignmentReference, Parent.Header.PK);
				if (!jobNumber.IsEmpty && Parent.MessageType == Common.Shared.MessageSubTypeCodes.Codes.Original)
				{
					Parent.ShouldSendInfo.AddError(ZString.Format(ValidationConstants.EntryInstruction.UCRNumberAlreadyUsedOnJob, Parent.Header.UniqueConsignmentReference, jobNumber));
				}
			}
		}

		#region LocalReferenceNumber

		protected override void CheckLocalReferenceNumber()
		{
			base.CheckLocalReferenceNumber();
			var targetInfo = Parent.LocalReferenceNumberInfo;
			if (Parent.ShouldSend && Parent.IsLRNEditable)
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidationHelper.ValidateLRNFormat(targetInfo);
			}
		}

		#endregion

		#region MovementReferenceNumber

		protected override void CheckMovementReferenceNumber()
		{
			base.CheckMovementReferenceNumber();
			var targetInfo = Parent.MovementReferenceNumberInfo;
			if (Parent.ShouldSend)
			{
				if (Parent.IsMRNEditable)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
					ValidationHelper.ValidateMRNFormat(targetInfo);
				}
				if (MessageDataProviderInstruction.ShouldOutputMRNToBeReplaced(Parent.MessageKeyFactor) && Parent.MovementReferenceNumber.IsEmpty)
				{
					targetInfo.AddMessageError(MRNIsRequiredForReplacementMessage);
				}
			}
		}

		#endregion

		#region Validation Message

		internal static ZString PaymentCodeOfFreeExpected
		{
			get { return Res.GetString("E5A0BCED-6E2C-4D48-B01D-A8A022F91973", "A Payment Method of Free is expected when there are no duties or taxes."); }
		}

		internal static ZString PaymentMethodForcedToFreeForCancellation(ZString currentCode)
		{
			return Res.GetString("7F0D2E35-DE57-4ADF-9336-133A7D8DADE9", "Payment method will be submitted as 'F - Free' since you are lodging a cancellation message even though it's specified as '{0}' on corresponding Entry Header.", currentCode);
		}

		internal static ZString MRNIsRequiredForVOCMessage
		{
			get { return Res.GetString("17ECA504-B5F6-4ABC-B0C8-4E7DD7526CFB", "MRN is required for VOC messages. If you are sending message for Entry originally submitted outside this job, please specify the Assessment Date on corresponding Entry Instruction."); }
		}

		internal static ZString MRNIsRequiredForReplacementMessage
		{
			get { return Res.GetString("AC1391B4-9B3F-4883-A04B-89BA261C5977", "MRN to be Replaced is required for Replacement messages. Please specify the MRN to be Replaced on corresponding Entry Instruction."); }
		}

		internal static ZString ReplacementEntryIsNotAllowedForExBond
		{
			get { return Res.GetString("{0DBBDD56-9DEE-4011-BAB0-08128FF96A37}", "Replacement entry is not allowed for Ex-Bond."); }
		}

		internal static ZString PaymentCodeFreeRequiredForRIDorRPD
		{
			get { return Res.GetString("00715BE8-AC2A-4C07-8848-8E5276252B9A", "Payment method must be Free for an incomplete declaration."); }
		}

		#endregion
	}
}
