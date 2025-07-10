using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.US.Business
{
	public class CommonImportJobDeclarationValidation : JobDeclarationValidation
	{
		public CommonImportJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		#region JE_VoyageFlightNo

		protected void CheckAirOrHandCarryFlightNo()
		{
			if (Parent.IsAir || Parent.IsHandCarryAir)
			{
				var validationMessage = USAirlineNumberValidator.CheckAirFlightNo(Parent.JE_VoyageFlightNo, Parent.Factory);
				if (!string.IsNullOrEmpty(validationMessage))
				{
					if (Parent.IsAir)
					{
						Parent.JE_VoyageFlightNoInfo.AddMessageError(validationMessage);
					}
					else if (Parent.IsHandCarry)
					{
						Parent.JE_VoyageFlightNoInfo.AddWarning(validationMessage);
					}
				}
			}
		}

		#endregion

		#region JE_OH_ExternalBroker
		protected override void CheckJE_OH_ExternalBroker()
		{
			base.CheckJE_OH_ExternalBroker();
			if (Parent.JE_OH_ExternalBroker == ZGuid.Invalid)
			{
				Parent.JE_OH_ExternalBrokerInfo.AddError(SelectValidExternalBroker);
			}
		}
		public const string SelectValidExternalBroker = "The selected organization is no longer valid. Please choose a new organization from the list.";
		#endregion

		#region JE_PrimaryITNumber

		protected override void CheckJE_PrimaryITNumber()
		{
			base.CheckJE_PrimaryITNumber();

			if (Parent.PrimaryMasterBill != null)
			{
				if (Parent.ITNumbersFromBills.Count == 0)
				{
					if (!Parent.US_ITDate.IsEmpty)
					{
						Parent.JE_PrimaryITNumberInfo.AddMessageError(ITNumberValidator.Constants.ITNumber.ITnumberIsRequiredWhenITDateIsEntered);
					}
				}
				else if (!Parent.JE_PrimaryITNumber.IsEmpty && Parent.JE_PrimaryITNumber != JobDeclaration.Constants.Multiple)
				{
					ITNumberValidator.ValidateITNumberFormat(Parent.JE_PrimaryITNumberInfo, Parent.IsAir);
				}
			}
		}

		#endregion

		#region JE_GB
		protected override void CheckJE_GB()
		{
			base.CheckJE_GB();
			if (Parent.IsInDatabase && Parent.JE_GBInfo.HasChanges && Parent.ActiveEntryHeaders.HasAtLeastOneEntryWithActiveMessages)
			{
				Parent.JE_GBInfo.AddError(ChangeToADifferentBranch);
			}

			var addInfoValidation = Parent.AddInfoValidation;

			addInfoValidation.ValidateUS_EntryFilerCode();
			addInfoValidation.ValidateUS_InbondType();

			if (Parent.JE_MessageType == JobMessageTypeList.Codes.Import || Parent.IsFTZAdmission)
			{
				if (Parent.IsACECargoReleaseValidationMode && !Parent.US_EnableENS && Parent.ProcessingDistrictPort.IsEmpty)
				{
					Parent.JE_GBInfo.AddMessageError(HasNoProcessingPort);
				}
			}
		}
		internal const string ChangeToADifferentBranch = "You cannot change a branch now as this job has lodged entries at Customs.";
		internal const string HasNoProcessingPort = "A processing port will be blank in a B block. Please enter a port code for this branch or its company in the Registry > Customs > United States of America > Import > ABI > Processing > District Port or Registry > Customs > United States of America > Import > ABI > Statement > Non-RLF Entry Port/Processing Port Mapping.";
		#endregion

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();

			if (Parent.US_EntryFilerCode.IsEmpty && Parent.IsEntryFilerCodeRequired)
			{
				if (!Parent.IsImportByExternalBroker)//if so, it is validated against US_EntryFilerCode because it becomes editable
				{
					var text = Parent.IsFTZAdmission ? WhyNeedEntryFilerCodeFTZ : WhyNeedEntryFilerCode;
					Parent.JE_MessageTypeInfo.AddError(string.Format(EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated, text) +
						((IRegistryItemInternals)USCustomsDataRegistry.Instance.EntryFiler).Location);
				}
			}
		}
		internal const string EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated = "An entry filer code has not been set up for this branch or company. {0}. Please set up one in Admin -> System -> Registry ";
		internal const string WhyNeedEntryFilerCode = "Without it, entry numbers cannot be generated for import jobs";
		internal const string WhyNeedEntryFilerCodeFTZ = "Without it, FTZ messages cannot be sent";

		#region JE_OH_FDASubmitter
		protected override void CheckJE_OH_FDASubmitter()
		{
			base.CheckJE_OH_FDASubmitter();

			if (Parent.IsFDAPriorNoticeValidationRequired)
			{
				var fdaSubmitter = Parent.FDASubmitter;
				if (!Parent.IsACE)
				{
					FDAOrganisationValidator.Validate(fdaSubmitter, Parent.JE_OH_FDASubmitterInfo);
				}

				if (fdaSubmitter != null)
				{
					OrgHeaderWrapper fdaSubmitterWrapped = OrgHeaderWrapper.New(fdaSubmitter);
					if (fdaSubmitterWrapped.ZO_SubmitterFirmType.IsEmpty)
					{
						Parent.JE_OH_FDASubmitterInfo.AddMessageError(ValidationConstants.PriorNotice.SubmitterFirmType);
					}
					else
					{
						if (!Parent.Factory.GetCachedValue<SubmitterFirmTypeList>().ContainsCode(fdaSubmitterWrapped.ZO_SubmitterFirmType))
						{
							Parent.JE_OH_FDASubmitterInfo.AddMessageError(ValidationConstants.PriorNotice.SubmitterFirmTypeValid);
						}
					}

					if (Parent.IsACE)
					{
						var contactDetails = fdaSubmitterWrapped as IPGAContactDetails;
						if (contactDetails.EmailAddress.IsEmpty && contactDetails.Fax.IsEmpty)
						{
							Parent.JE_OH_FDASubmitterInfo.AddMessageError(ValidationConstants.PriorNotice.OrganisationFAXOrEmailRequired);
						}
					}
				}
			}
		}
		#endregion

		#region JE_ExportDate

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (ShouldCheckJE_ExportDate)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExportDateInfo, JE_ExportDateMessageRequiredMessage);

				if (!Parent.JE_ExportDate.IsEmpty && !Parent.US_ITDate.IsEmpty)
				{
					if (Parent.JE_ExportDate.Date > Parent.US_ITDate.Date)
					{
						Parent.JE_ExportDateInfo.AddMessageError(ExportDateShouldBeLessThanITDate);
					}
				}
			}
		}

		protected virtual bool ShouldCheckJE_ExportDate
		{
			get { return true; }
		}

		protected virtual ZString JE_ExportDateMessageRequiredMessage
		{
			get { return "Date of Export."; }
		}

		internal const string ExportDateShouldBeLessThanITDate = "Date of Export should be less or equal to IT Date.";

		#endregion

		#region JE_MasterBillIssuerSCAC
		protected override void CheckJE_MasterBillIssuerSCAC()
		{
			base.CheckJE_MasterBillIssuerSCAC();

			var primaryMasterBill = Parent.PrimaryMasterBill;
			if (primaryMasterBill != null)
			{
				primaryMasterBill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
				Parent.JE_MasterBillIssuerSCACInfo.AddAllNotificationsFrom(primaryMasterBill.US_UI_NKBillIssuerSCACInfo);
			}
		}
		#endregion

		#region JE_HouseBillIssuerSCAC
		protected override void CheckJE_HouseBillIssuerSCAC()
		{
			base.CheckJE_HouseBillIssuerSCAC();
			if (Parent.PrimaryHouseBill != null)
			{
				Parent.PrimaryHouseBill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
				Parent.JE_HouseBillIssuerSCACInfo.AddAllNotificationsFrom(Parent.PrimaryHouseBill.US_UI_NKBillIssuerSCACInfo);
			}
		}
		#endregion

		#region JE_TotalNoOfPacks
		protected override void CheckJE_TotalNoOfPacks()
		{
			base.CheckJE_TotalNoOfPacks();

			var value = ZDecimal.Zero;

			foreach (Package package in Parent.Packages)
			{
				value += package.CW_PackQty;
			}

			if (Parent.JE_TotalNoOfPacks != value)
			{
				Parent.JE_TotalNoOfPacksInfo.AddWarning(PackagesDifferFromTotalPacks);
			}
		}

		internal const string PackagesDifferFromTotalPacks = "Total Number of Packages is  different to the total number of packages on the Packing Tab > Package Details.";
		#endregion
	}
}
