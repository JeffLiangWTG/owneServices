using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDeclarationValidation : Customs.Business.BaseJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Parent; }
		}

		protected override Customs.Business.BillValidator GetBillValidator()
		{
			return new BillValidator();
		}

		protected override void CheckJE_OH_NotifyParty()
		{
			base.CheckJE_OH_NotifyParty();
			var notifyParty = Declaration.NotifyParty;
			if (notifyParty != null)
			{
				if (Declaration.IsTSWDeclaration)
				{
					CheckNotifyPartyHasAllocatedContact(Parent.JE_OH_NotifyPartyInfo, notifyParty);
				}
				else if (Declaration.IsFormalEntry && notifyParty.LocalCustomsClientCode.IsEmpty)
				{
					Parent.JE_OH_NotifyPartyInfo.AddMessageError(MessageErrorDeliveryAuthorityMissingCustomsClientCode);
				}
			}
		}
		public const string MessageErrorDeliveryAuthorityMissingCustomsClientCode = "Delivery Authority Organization must have a Customs Client Code. (NZ CCD should be added on the Config tab of the selected Organization)";

		void CheckNotifyPartyHasAllocatedContact(ZPropertyInfo orgPropertyInfo, OrgHeader party)
		{
			var hasAllocatedContact = false;
			var hasContactEmail = false;
			foreach (OrgContact contact in party.Contacts)
			{
				foreach (OrgContactAttribute contactAllocation in contact.Allocations)
				{
					if (contactAllocation.PC_Type == OrgConstants.ContactAllocationType.NZCustoms)
					{
						hasAllocatedContact = true;
						if (!contact.Email.IsEmpty)
						{
							hasContactEmail = true;
						}

						break;
					}
				}

				if (hasAllocatedContact)
				{
					break;
				}
			}

			if (!hasAllocatedContact)
			{
				var contactRequired = Res.GetString("4BC79A5B-31FF-462E-A7DA-09D2392BEF1F", "This Organization needs an allocated contact person for New Zealand Customs.\r\nEdit the organization details to add or edit a contact to allocate that person as this organizations primary contact representative.");
				orgPropertyInfo.AddMessageError(contactRequired);
			}
			else if (!hasContactEmail)
			{
				var contactRequired = Res.GetString("1468A6FA-40AB-4F98-B4B8-153ED1DEFEF8", "The allocated customs contact for this Organization needs a valid email address.\r\nEdit the organization details to edit the allocated contact to include a valid email address.");
				orgPropertyInfo.AddMessageError(contactRequired);
			}
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();
			if (FieldContainsNonASCIICharacters(Declaration.JE_MasterBill))
			{
				Declaration.JE_MasterBillInfo.AddError(BillNumberHasNonASCIIValue);
			}

			if (FieldHasTabCharacters(Declaration.JE_MasterBill))
			{
				Declaration.JE_MasterBillInfo.AddError(BillNumberHasTabs);
			}
		}

		protected override void CheckJE_HouseBill()
		{
			base.CheckJE_HouseBill();
			if (FieldContainsNonASCIICharacters(Declaration.JE_HouseBill))
			{
				Declaration.JE_HouseBillInfo.AddError(BillNumberHasNonASCIIValue);
			}

			if (FieldHasTabCharacters(Declaration.JE_HouseBill))
			{
				Declaration.JE_HouseBillInfo.AddError(BillNumberHasTabs);
			}
		}

		protected override void CheckJE_MessageTypeIsEnteredOrValid()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_MessageTypeInfo, Parent.Lookups.MessageTypeList);
			MandatoryValidation.CheckEntered(Parent.JE_MessageTypeInfo);
		}

		bool FieldContainsNonASCIICharacters(string fieldValue)
		{
			int replaceCharCount = fieldValue.CountMatches('?');
			string valueWithAnyASCIICharsReplaces = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(fieldValue));
			int checkedCharCount = valueWithAnyASCIICharsReplaces.CountMatches('?');
			return checkedCharCount != replaceCharCount;
		}

		bool FieldHasTabCharacters(string fieldValue)
		{
			return fieldValue.Contains("\t");
		}

		public void ValidateJE_EDITransmitDate()
		{
			ValidateCalculatedProperty(Declaration.JE_EDITransmitDateInfo);

			foreach (HeaderOtherInfo headerOtherInfo in Declaration.OtherInfos)
			{
				headerOtherInfo.ValidateZO_Code();
			}
		}

		protected virtual void CheckJE_EDITransmitDate()
		{
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (Declaration.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_VesselNameInfo);
				if (!Declaration.IsExport || !Declaration.IsDrawback || !Declaration.VesselIndicatesPeriodic)
				{
					ListValidation.MessageErrorIfInvalidCode(Declaration.JE_VesselNameInfo, Declaration.Lookups.Vessels);
					var vesselName = Declaration.JE_VesselName;
					if (!vesselName.IsEmpty && !FlightsAndVesselsHelper.IsValidFlightOrVessel(Parent.Factory, vesselName))
					{
						Declaration.JE_VesselNameInfo.AddMessageError(VesselNotOnCustomsSupportedList);
					}
				}
			}
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (Declaration.IsAir)
			{
				var flightNo = Declaration.JE_VoyageFlightNo;
				if (flightNo.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_VoyageFlightNoInfo, "Flight Number");
				}
				else if (!Declaration.FlightNoIndicatesPeriodic && !FlightsAndVesselsHelper.IsValidFlightOrVessel(Parent.Factory, flightNo))
				{
					Declaration.JE_VoyageFlightNoInfo.AddMessageError(FlightNotOnCustomsSupportedList);
				}
			}
			else if (Declaration.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_VoyageFlightNoInfo, "Voyage Number");
				if (Declaration.JE_VoyageFlightNo.Length > 8)
				{
					Declaration.JE_VoyageFlightNoInfo.AddMessageError("Vessel Voyage exceeds 8 characters and will be truncated in entry message.");
				}
			}
		}

		public void ValidateJE_SendMCDContainerQuarantineDeclaration()
		{
			ValidateCalculatedProperty(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo);
		}

		protected void CheckJE_SendMCDContainerQuarantineDeclaration()
		{
			if (Declaration.IsFormalEntry && Declaration.IsImport && Declaration.IsSea && !Declaration.JE_SendMCDContainerQuarantineDeclaration && Declaration.CusContainers.HasContainerModeOf(ContainerModeList.Codes.FCL))
			{
				Declaration.JE_SendMCDContainerQuarantineDeclarationInfo.AddMessageError("Import FCL Entries must send an MCD Other Info Code with appropriate MCD Flags.");
			}
		}

		public void ValidateJE_ECI_InvoiceAmount()
		{
			ValidateCalculatedProperty(Declaration.JE_ECI_InvoiceAmountInfo);
		}

		protected virtual void CheckJE_ECI_InvoiceAmount()
		{
		}

		public void ValidateJE_ECI_InvoiceCurrency()
		{
			ValidateCalculatedProperty(Declaration.JE_ECI_InvoiceCurrencyInfo);
		}

		protected virtual void CheckJE_ECI_InvoiceCurrency()
		{
		}

		public void ValidateJE_OriginalEntryNumber()
		{
			ValidateCalculatedProperty(Declaration.JE_OriginalEntryNumberInfo);
		}

		protected virtual void CheckJE_OriginalEntryNumber()
		{
		}

		public void ValidateJE_OriginalEntryType()
		{
			ValidateCalculatedProperty(Declaration.JE_OriginalEntryTypeInfo);
		}

		protected virtual void CheckJE_OriginalEntryType()
		{
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			ValidateJE_OH_Supplier();
			ValidateJE_OH_Importer();
			ValidateJE_OriginalEntryNumber();
			ValidateJE_OriginalEntryType();
			Declaration.Bills.ValidateCU_BillType();
			ValidateJE_SendMCDContainerQuarantineDeclaration();
			CheckEntryStyle();
		}

		void CheckEntryStyle()
		{
			if (Declaration.IsTSWDeclaration && Declaration.JE_MessageSubType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_MessageSubTypeInfo);
			}

			if (Declaration.CustomsEntryHeaders.Count > 0)
			{
				var originalEntryStyle = Declaration.EntryHeaderForOriginalEntryNumber?.CH_LastEntryStyle ?? ZString.Empty;
				if (Declaration.IsFormalChangedToWriteOff)
				{
					Declaration.JE_MessageSubTypeInfo.AddError("This formal entry has already been sent to Customs and therefore cannot be changed to this type of entry. It must be cancelled first if you wish to now submit it as a different type of entry.");
				}
				else if (Declaration.IPIDeclarationFromFormalDec)
				{
					if (Declaration.JE_MessageSubType != MessageSubTypeCombinedList.Codes.IPI && Declaration.JE_MessageSubType != originalEntryStyle && !originalEntryStyle.IsEmpty)
					{
						Declaration.JE_MessageSubTypeInfo.AddError("Once you have added a Primary Industries Import entry to an existing declaration you cannot change the Entry Style, unless reverting it back to the original Entry Style.");
					}
					else if (Declaration.IsTSW_IPI_Declaration && (Declaration.EntryHeaderForIPIEntryNumber == null || Declaration.EntryHeaderForIPIEntryNumber.Messages.Count == 0))
					{
						Declaration.JE_MessageSubTypeInfo.AddWarning("You are adding a Primary Industries Import entry to this existing declaration.");
					}
				}
				else
				{
					var entryNumber = Declaration.CusEntryHeader.EntryNumber;
					if (!originalEntryStyle.IsEmpty && originalEntryStyle != Declaration.JE_MessageSubType && !entryNumber.IsEmpty)
					{
						Declaration.JE_MessageSubTypeInfo.AddMessageError("Entry Style cannot change after a message has been sent - You must cancel this entry before resubmitting as another type.");
					}
				}
			}
		}

		protected override void CheckJE_TotalNoOfPacksPackTypeIsAValidCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TotalNoOfPacksPackTypeInfo, Parent.Lookups.JE_TotalNoOfPacksPackType_List);
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();
			if (Declaration.IsImport && !Declaration.IsPeriodic)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_DateOfArrivalInfo);
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (Declaration.IsExport && !Declaration.IsPeriodic)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_ExportDateInfo);
			}
		}

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();
			if (Declaration.JE_ApplicationCode.IsEmpty)
			{
				Declaration.JE_ApplicationCodeInfo.AddError("Enter a messaging mode.");
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Declaration.JE_ApplicationCodeInfo, Declaration.Lookups.ApplicationCodeList);
			}
		}

		public void ValidateJE_Cal_GoodsLocation()
		{
			ValidateCalculatedProperty(Declaration.JE_Cal_GoodsLocationInfo);
		}

		protected virtual void CheckJE_Cal_GoodsLocation()
		{
			var info = Declaration.JE_Cal_GoodsLocationInfo;

			EnglishCharactersValidation.ErrorIfNotWesternEuropean(info);
			ListValidation.ErrorIfInvalidCode(info, Declaration.Lookups.GoodsLocationList);
		}

		public void ValidateJE_RL_NKPortOfDeliveryNotify()
		{
			ValidateCalculatedProperty(Declaration.JE_RL_NKPortOfDeliveryNotifyInfo);
		}

		protected virtual void CheckJE_RL_NKPortOfDeliveryNotify()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Declaration.JE_RL_NKPortOfDeliveryNotifyInfo);
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_RL_NKPortOfDeliveryNotifyInfo);
		}

		#region NZAddInfo wrapped properties
		protected virtual void CheckJE_SoldOrConsigned()
		{
		}

		public void ValidateJE_SoldOrConsigned()
		{
			ValidateCalculatedProperty(Declaration.JE_SoldOrConsignedInfo);
		}

		protected virtual void CheckJE_RL_NKProcessingPort()
		{
		}

		public void ValidateJE_RL_NKProcessingPort()
		{
			ValidateCalculatedProperty(Declaration.JE_RL_NKProcessingPortInfo);
		}

		#endregion

		#region Validate NZAddInfo wrapped properties

		#endregion

		public override void ValidateAll()
		{
			ValidateJE_EDITransmitDate();
			base.ValidateAll();
			ValidateJE_SendMCDContainerQuarantineDeclaration();
			ValidateJE_ECI_InvoiceAmount();
			ValidateJE_ECI_InvoiceCurrency();
			ValidateJE_OriginalEntryNumber();
			ValidateJE_OriginalEntryType();
			ValidateJE_OH_NotifyParty();
			ValidateJE_SoldOrConsigned();
			ValidateJE_RL_NKProcessingPort();
			ValidateJE_RL_NKPortOfDeliveryNotify();
			ValidateJE_Cal_GoodsLocation();
		}

		#region Overrides where validation is not wanted in NZ
		protected override void CheckJE_ContainerMode()
		{
			// There should be no validation on this as it's not used in NZ.
		}

		protected override void CheckJE_RL_NKPortOfFirstArrival()
		{
			// This field is not used in the NZ Implementation. Validation Overridden.
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();

			if (Declaration.PortOfLoading != null)
			{
				if ((Declaration.JE_TransportMode == Core.Constants.TransportModes.Air && !Declaration.PortOfLoading.RL_HasAirport)
					|| (Declaration.JE_TransportMode == Core.Constants.TransportModes.Sea && !Declaration.PortOfLoading.RL_HasSeaport))
				{
					Declaration.JE_RL_NKPortOfLoadingInfo.AddMessageError("This Port Code is invalid for the transport mode of this declaration.");
				}
			}
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
			// This field is not used in the NZ Implementation. Validation Overridden.
		}

		protected override void CheckJE_DateOfFirstArrivalIsValidZDateTime()
		{
			// This field is not used in the NZ Implementation. Validation Overridden.
		}

		protected override void CheckJE_DateOfFirstArrivalIsValidZDateTimeRange()
		{
			// This field is not used in the NZ Implementation. Validation Overridden.
		}

		#endregion

		protected override bool TotalWeightValidationCheckingAgainstInvoiceLinesShouldBeAMessageErrorInsteadOfAWarning
		{
			get { return true; }
		}

		protected internal void CheckHasAllocatedContact(ZPropertyInfo propertyInfo, OrgHeader party)
		{
			OrgContact allocatedContact = null;
			if (party != null && !Declaration.TSWSimplifiedMiscEntry)
			{
				foreach (OrgContact contact in party.Contacts)
				{
					foreach (OrgContactAttribute contactAllocation in contact.Allocations)
					{
						if (contactAllocation.PC_Type == OrgConstants.ContactAllocationType.NZCustoms)
						{
							allocatedContact = contact;
							break;
						}
					}
				}

				if (allocatedContact == null)
				{
					if (party.MainAddress.OA_Email.IsEmpty && party.MainAddress.OA_Mobile.IsEmpty && party.MainAddress.OA_Phone.IsEmpty && party.MainAddress.OA_Fax.IsEmpty)
					{
						var commsRequired = Res.GetString("C6114BD8-856F-4213-A799-B3F26E093316", "This Organization has no allocated contact person for New Zealand Customs and no communication information entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details and add or edit a contact as this organizations primary contact representative.");
						propertyInfo.AddMessageError(commsRequired);
					}
					else if (!Declaration.IsMiscellaneousImporter)
					{
						var contactRequired = Res.GetString("4BC79A5B-31FF-462E-A7DA-09D2392BEF1F", "This Organization needs an allocated contact person for New Zealand Customs.\r\nEdit the organization details to add or edit a contact to allocate that person as this organizations primary contact representative.");
						propertyInfo.AddMessageError(contactRequired);
					}
				}
				else
				{
					if (allocatedContact.OC_Email.IsEmpty && allocatedContact.OC_Mobile.IsEmpty && allocatedContact.OC_Phone.IsEmpty && allocatedContact.OC_Fax.IsEmpty)
					{
						if (party.MainAddress.OA_Email.IsEmpty && party.MainAddress.OA_Mobile.IsEmpty && party.MainAddress.OA_Phone.IsEmpty && party.MainAddress.OA_Fax.IsEmpty)
						{
							var contactCommsWarning = Res.GetString("5716C09E-3E14-425A-B323-0000ADF254CC", "This Organization has no mandatory communication information details, required by New Zealand Customs, entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details.\r\nAlternatively, update {0}, the allocated contact for this Organization, with their phone/fax/email details.", allocatedContact.OC_ContactName);
							propertyInfo.AddMessageError(contactCommsWarning);
						}
						else
						{
							var commsRequired = Res.GetString("2C821077-2851-4084-8A12-6A6818CB4876", "This Organization has {0} as the allocated contact person for New Zealand Customs but that contact has no communication information entered.\r\nCommunication information will fall back to the company organization values.\r\nEdit the allocated contact to send specific information for {0}, the primary contact representative if desired.", allocatedContact.OC_ContactName);
							propertyInfo.AddWarning(commsRequired);
						}
					}
				}
			}
		}

		const string BillNumberHasNonASCIIValue = "Bill number field can not have Non ASCII characters.";
		const string BillNumberHasTabs = "Bill number field can not contain Tab characters.";
		const string FlightNotOnCustomsSupportedList = "Flight number is not in the list of valid Flights supported by NZ Customs.";
		const string VesselNotOnCustomsSupportedList = "Vessel name is not in the list of valid Vessel names supported by NZ Customs.";
	}
}
