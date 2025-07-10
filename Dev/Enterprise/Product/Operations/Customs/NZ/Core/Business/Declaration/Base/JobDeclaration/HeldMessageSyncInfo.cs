namespace Enterprise.Customs.NZ.Business.Declaration
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business;

	public class HeldMessageSyncInfo : NonPersistentBusinessObject, IObsoleteValidation
	{
		public HeldMessageSyncInfo(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		public CusEntryHeader EntryHeader { get { return entryHeader; } }

		readonly CusEntryHeader entryHeader;

		#region ShouldRecreateMessage

		public ZBool ShouldRecreateMessage
		{
			get { return shouldRecreateMessage; }
			set
			{
				if (value != shouldRecreateMessage)
				{
					SetNonPersistentPropertyValue(ShouldRecreateMessageInfo, ref shouldRecreateMessage, value);
				}
			}
		}
		ZBool shouldRecreateMessage;

		public ZPropertyInfo ShouldRecreateMessageInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldRecreateMessage)); }
		}

		#endregion

		#region ShouldLeaveMessageUnchanged

		public ZBool ShouldLeaveMessageUnchanged
		{
			get { return shouldLeaveMessageUnchanged; }
			set
			{
				if (value != shouldLeaveMessageUnchanged)
				{
					SetNonPersistentPropertyValue(ShouldLeaveMessageUnchangedInfo, ref shouldLeaveMessageUnchanged, value);
				}
			}
		}
		ZBool shouldLeaveMessageUnchanged;

		public ZPropertyInfo ShouldLeaveMessageUnchangedInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldLeaveMessageUnchanged)); }
		}

		#endregion

		#region RemarksForLeavingMessageUnchanged

		[MaxLength(128)]
		public ZString RemarksForLeavingMessageUnchanged
		{
			get { return remarksForLeavingMessageUnchanged; }
			set
			{
				if (value != remarksForLeavingMessageUnchanged)
				{
					SetNonPersistentPropertyValue(RemarksForLeavingMessageUnchangedInfo, ref remarksForLeavingMessageUnchanged, value);
				}
			}
		}
		ZString remarksForLeavingMessageUnchanged;

		public ZPropertyInfo RemarksForLeavingMessageUnchangedInfo
		{
			get { return GetZPropertyInfo(nameof(RemarksForLeavingMessageUnchanged)); }
		}

		#endregion

		#region Check/Save Message Changes

		/// <summary>
		/// Check if Dec has a pending send message.
		/// Check if it has relevant changes (a black box that can be adjusted without affecting the rest of the implementation).
		/// Check if the user hasn't already acknowledged this scenario (check for DeclarationAmendedPermitApproved event log).
		/// </summary>
		public bool SetHasChangesToHeldMessages()
		{
			shouldRecreateMessage = (
				entryHeader != null
				&& entryHeader.CH_EntryStatus == FormalEntryStatusList.Codes.QueuedForSending
				&& entryHeader.DateMessageQueuedToBeSentOn > ZDateTime.Now
				&& new HasMessageChangeHunter(entryHeader.Declaration).HasChanges()
			);

			return shouldRecreateMessage;
		}

		/// <summary>
		/// If current changes affect a held message, performs actions as defined by the user:
		///   - Cancels message to be re-created after save
		///   - Leaves the message as is and logs user acknowledgement
		/// </summary>
		public NZCMessage[] PerformHeldMessageActions()
		{
			NZCMessage[] cancelledMessages = null;

			if (ShouldRecreateMessage)
			{
				entryHeader.SetMessagingStatusToNotSent();
				cancelledMessages = entryHeader.CancelQueuedMessages();
			}
			else if (ShouldLeaveMessageUnchanged)
			{
				entryHeader.Logs.AddNew(Events.DeclarationAmendedPermitApproved, RemarksForLeavingMessageUnchanged);
			}

			return cancelledMessages;
		}

		public void RevertCancelHeldMessage(NZCMessage[] cancelledMessages)
		{
			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;

			if (cancelledMessages != null)
			{
				foreach (var message in cancelledMessages)
				{
					message.EM_Status = NZCMessage.Status.Queued;
				}
			}
		}

		#endregion

		#region HasMessageChangeHunter

		class HasMessageChangeHunter
		{
			public HasMessageChangeHunter(JobDeclaration declaration)
			{
				this.declaration = declaration;
			}

			readonly JobDeclaration declaration;

			public bool HasChanges()
			{
				if (declaration.HasChanges)
				{
					if (
						CheckDeclarationProperties()
						|| EntryHeaderHasChanges(declaration.CusEntryHeader)
						|| OtherDeclarationChildrenHaveChanges()
					)
					{
						return true;
					}
				}

				return false;
			}

			bool CheckDeclarationProperties()
			{
				if (
					declaration.JE_OriginalEntryNumberInfo.HasChanges
					|| declaration.DeclarationNumberInfo.HasChanges
					|| declaration.JE_MessageTypeInfo.HasChanges
					|| declaration.JE_MessageSubTypeInfo.HasChanges
					|| declaration.JE_RL_NKPortOfLoadingInfo.HasChanges
					|| declaration.JE_RL_NKPortOfArrivalInfo.HasChanges
					|| declaration.JE_RL_NKProcessingPortInfo.HasChanges
					|| declaration.JE_RL_NKFinalDestinationInfo.HasChanges
					|| declaration.JE_EntryAuthorisationDateInfo.HasChanges
					|| declaration.JE_DateOfArrivalInfo.HasChanges
					|| declaration.JE_ExportDateInfo.HasChanges
					|| declaration.JE_SoldOrConsignedInfo.HasChanges
					|| declaration.JE_DeclaredWeightInfo.HasChanges
					|| declaration.JE_MasterBillInfo.HasChanges
					|| declaration.JE_VoyageFlightNoInfo.HasChanges
					|| declaration.JE_VesselNameInfo.HasChanges
					|| declaration.JE_OH_SupplierInfo.HasChanges
					|| declaration.JE_OH_ImporterInfo.HasChanges
					|| declaration.JE_OH_NotifyPartyInfo.HasChanges
				)
				{
					return true;
				}

				if (declaration.WarehouseAddress != null && declaration.WarehouseAddress.HasChanges)
				{
					return true;
				}

				return false;
			}

			bool EntryHeaderHasChanges(CusEntryHeader entryHeader)
			{
				if (entryHeader.HasChanges)
				{
					if (CheckEntryHeaderProperties(entryHeader))
					{
						return true;
					}

					if (entryHeader.MergedLines.HasChanges)
					{
						foreach (CusEntryLine entryLine in entryHeader.MergedLines)
						{
							if (EntryLineHasChanges(entryLine))
							{
								return true;
							}
						}
					}
				}

				return false;
			}

			bool CheckEntryHeaderProperties(CusEntryHeader entryHeader)
			{
				if (
					entryHeader.EntryNumberInfo.HasChanges
					|| entryHeader.CH_CustomsMessageRemarksInfo.HasChanges
					|| entryHeader.CH_LastNumberOfLinesSentToCustomsInfo.HasChanges
					|| entryHeader.GSTAmountInfo.HasChanges
					|| entryHeader.TotalAmountPayableInfo.HasChanges
				)
				{
					return true;
				}

				if (entryHeader is FormalEntry.CusEntryHeader)
				{
					var formalEntry = entryHeader as FormalEntry.CusEntryHeader;
					if (formalEntry.CH_OverrideIndicatorInfo.HasChanges)
					{
						return true;
					}
				}

				return false;
			}

			bool EntryLineHasChanges(CusEntryLine entryLine)
			{
				if (entryLine.HasChanges)
				{
					if (
						entryLine.CL_LineNumberInfo.HasChanges
						|| entryLine.CL_AdValoremTariffInfo.HasChanges
						|| entryLine.DescriptionInfo.HasChanges
						|| entryLine.DutyAmountInfo.HasChanges
						|| entryLine.OSCustomsValueInfo.HasChanges
						|| entryLine.CL_CustomsValueInfo.HasChanges
						|| entryLine.Fees.HasChanges
						|| entryLine.PermitCodes.HasChanges
						|| entryLine.ProhibitedCodes.HasChanges
						|| entryLine.OtherInfos.HasChanges
					)
					{
						return true;
					}

					if (entryLine.RandomLine != null)
					{
						if (
							entryLine.RandomLine.JI_EffectiveQualifiesForPreferentialDutyInfo.HasChanges
							|| entryLine.RandomLine.JI_ConcessionCodeInfo.HasChanges
							|| entryLine.RandomLine.JI_SupplementaryUQInfo.HasChanges
							|| entryLine.RandomLine.JI_SupplementaryQtyInfo.HasChanges
							|| entryLine.RandomLine.JI_RN_NKEffectiveCountryOfOriginInfo.HasChanges
							|| entryLine.RandomLine.JI_RN_NKEffectiveCountryOfExportInfo.HasChanges
						)
						{
							return true;
						}
					}
				}

				return false;
			}

			bool OtherDeclarationChildrenHaveChanges()
			{
				if (declaration.OtherInfos.HasChanges || declaration.PermitCodes.HasChanges)
				{
					return true;
				}

				if (declaration.CusContainers.HasChanges)
				{
					foreach (CusContainer container in declaration.CusContainers)
					{
						if (
							container.CO_ContainerNumberInfo.HasChanges
							|| container.CO_FCL_LCL_AIRInfo.HasChanges
							|| container.CO_SealInfo.HasChanges
						)
						{
							return true;
						}
					}
				}

				if (declaration.LowestBills.HasChanges)
				{
					foreach (Bill bill in declaration.LowestBills)
					{
						if (bill.CU_HouseBillInfo.HasChanges)
						{
							return true;
						}

						if (bill.PackingGroups.HasChanges)
						{
							foreach (PackingGroup packGroup in bill.PackingGroups)
							{
								if (packGroup.CR_CO_ContainerInfo.HasChanges)
								{
									return true;
								}

								if (packGroup.Packages.HasChanges)
								{
									foreach (Package pack in packGroup.Packages)
									{
										if (pack.CW_PackQtyInfo.HasChanges || pack.CW_PackTypeInfo.HasChanges)
										{
											return true;
										}
									}
								}
							}
						}
					}
				}

				if (declaration.JobComInvoiceGroupHeaders.HasChanges)
				{
					foreach (JobComInvoiceHeader invHeader in declaration.JobComInvoiceGroupHeaders as JobComInvoiceHeader)
					{
						if (
							invHeader.JZ_InvoiceNumberInfo.HasChanges
							|| invHeader.JZ_IncoTermInfo.HasChanges
							|| invHeader.JZ_RelationshipIndicatorInfo.HasChanges
							|| invHeader.JZ_RX_NKInvoice_CurrencyInfo.HasChanges
						)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		#endregion
	}
}
