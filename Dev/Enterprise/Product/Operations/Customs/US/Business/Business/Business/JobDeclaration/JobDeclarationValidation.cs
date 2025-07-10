using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationValidation : Customs.Business.BaseJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.RefreshDISDocumentIDList();
			base.ValidateAll();
			ValidateJE_MasterBillIssuerSCAC();
			ValidateJE_HouseBillIssuerSCAC();
			ValidateJE_PrimaryITNumber();
			ValidateDecEntryNumber();
			ValidateBrokerToPayIndicator();
			ValidateFTZAdmissionNumber();
			ValidateFTZZoneID();
			ValidateFTZControlNumber();
			ValidateFTZYear();
			ValidateIOR();
			ValidateJE_OH_FDASubmitter();
			ValidateJE_OA_InvoicerAddress();
		}

		public void ValidateJE_OA_InvoicerAddress()
		{
			ValidateCalculatedProperty(Parent.JE_OA_InvoicerAddressInfo);
		}

		protected virtual void CheckJE_OA_InvoicerAddress()
		{
			var declaration = Parent;
			if (!declaration.JE_OA_InvoicerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(declaration.JE_OA_InvoicerAddressInfo, declaration.InvoicerAddress);
			}
		}

		void ValidateIOR()
		{
			_ = ((ZWrappedPropertyInfo)Parent.IOROrgPKInfo).InnerInfo; // cause wrapped property to be loaded
			Parent.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
		}

		protected override void CheckJE_OA_ConsigneeAddress()
		{
			base.CheckJE_OA_ConsigneeAddress();
			var declaration = Parent;
			if (!declaration.JE_OA_ConsigneeAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(declaration.JE_OA_ConsigneeAddressInfo, declaration.ConsigneeAddress);
			}
		}

		public void ValidateJE_OH_CBPBroker()
		{
			ValidateCalculatedProperty(Parent.JE_OH_CBPBrokerInfo);
		}

		protected virtual void CheckJE_OH_CBPBroker()
		{
		}

		public void ValidateJE_OH_FDASubmitter()
		{
			ValidateCalculatedProperty(Parent.JE_OH_FDASubmitterInfo);
		}

		protected virtual void CheckJE_OH_FDASubmitter()
		{
		}

		protected override void CheckJE_OH_NotifyParty()
		{
			base.CheckJE_OH_NotifyParty();
			if (Parent.IsEntrySummaryValidationMode || Parent.IsDrawback)
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.JE_OH_NotifyPartyInfo, OrgMatchedCustomsRegNoType.EIN, string.Format(CultureInfo.CurrentCulture, OrganisationValidation.EIN_SSN_CBNCodeRequired, "Notify Party"), false, false);
			}
		}

		protected override void CheckJE_OA_SellerAddress()
		{
			base.CheckJE_OA_SellerAddress();
			if (!Parent.JE_OA_SellerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JE_OA_SellerAddressInfo, Parent.SellerAddress);
			}
		}

		protected override void CheckJE_OA_ShipToPartyAddress()
		{
			base.CheckJE_OA_ShipToPartyAddress();
			if (!Parent.JE_OA_ShipToPartyAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JE_OA_ShipToPartyAddressInfo, Parent.ShipToPartyAddress);
			}
		}

		protected override void CheckJE_OA_SoldToPartyAddress()
		{
			base.CheckJE_OA_SoldToPartyAddress();
			if (!Parent.JE_OA_SoldToPartyAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JE_OA_SoldToPartyAddressInfo, Parent.SoldToPartyAddress);
			}
		}

		protected override void CheckJE_OA_ManufacturerAddress()
		{
			base.CheckJE_OA_ManufacturerAddress();

			if (!Parent.JE_OA_ManufacturerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JE_OA_ManufacturerAddressInfo, Parent.ManufacturerAddress);
			}
		}
		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();

			ValidateWHSInvLineFilter();
			ValidateWHSPackageFilter();
			ValidateWHSProductFilter();
		}

		protected override string CannotChangeMessageTypeErrorText => Res.GetString("E104EF94-AB4E-4254-91FE-D2EA28E8F8D6", "Shipment Type cannot be changed because customs transactions exist.");

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			ValidateWHSInvLineFilter();
			ValidateWHSPackageFilter();
			ValidateWHSProductFilter();
		}

		internal protected virtual void CheckIOROrgPK(ZPropertyInfo info)
		{
			// sub-classes should override to implement custom validation
			info.AddAllNotificationsFrom(Parent.JE_OA_DeclarantAddress_ZAddress.AddressFKInfo);
		}

		public void ValidateDecEntryNumber()
		{
			ValidateCalculatedProperty(Parent.DecEntryNumberInfo);
		}

		public void ValidateFTZAdmissionNumber()
		{
			ValidateCalculatedProperty(Parent.FTZAdmissionNumberInfo);
		}

		protected virtual void CheckFTZAdmissionNumber()
		{
		}

		public void ValidateFTZZoneID()
		{
			ValidateCalculatedProperty(Parent.FTZZoneIDInfo);
		}

		protected virtual void CheckFTZZoneID()
		{
		}

		public void ValidateFTZYear()
		{
			ValidateCalculatedProperty(Parent.FTZYearInfo);
		}

		protected virtual void CheckFTZYear()
		{
		}

		public void ValidateFTZControlNumber()
		{
			ValidateCalculatedProperty(Parent.FTZControlNumberInfo);
		}

		protected virtual void CheckFTZControlNumber()
		{
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			Parent.AddInfoValidation.ValidateUS_SchDArrival();
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			Parent.AddInfoValidation.ValidateUS_SchDLoading();
		}

		protected virtual void CheckDecEntryNumber()
		{
		}

		protected override void CheckPackagesActualPackageCount()
		{
			//do not want to validate at all
		}

		protected override void CheckJE_HouseBill()
		{
			base.CheckJE_HouseBill();

			var billValidator = (BillValidator)GetBillValidator();
			billValidator.CheckInvalidLength(Parent.JE_HouseBillInfo, "House Bill", BillValidator.Constants.MaximumBillLength);
			billValidator.CheckInvalidCharacters(Parent.JE_HouseBillInfo, "House Bill");
			billValidator.CheckMandatory(IsHouseBillMandatoryForSCAC, Parent.JE_HouseBillInfo, "House Bill");
		}

		bool IsHouseBillMandatoryForSCAC
		{
			get { return !Parent.JE_HouseBillIssuerSCAC.IsEmpty; }
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();

			var billTerminology = Parent.JE_MasterBillInfo.HumanReadableName;
			var billValidator = (BillValidator)GetBillValidator();
			if (!Parent.IsExport)
			{
				billValidator.CheckInvalidLength(Parent.JE_MasterBillInfo, billTerminology, MasterBillMaxLength);
			}

			billValidator.CheckInvalidCharacters(Parent.JE_MasterBillInfo, billTerminology);
		}

		protected virtual int MasterBillMaxLength
		{
			get { return BillValidator.Constants.MaximumBillLength; }
		}

		#region All Port validation should be done through US_SchD etc on AddInfo (Unseal if required)

		protected override INotificationType GetPortNotificationType()
		{
			return CargoWise.EntityFramework.NotificationType.Warning;
		}

		#endregion

		#region ValidateJE_MasterBillIssuerSCAC

		public void ValidateJE_MasterBillIssuerSCAC()
		{
			ValidateCalculatedProperty(Parent.JE_MasterBillIssuerSCACInfo);
		}

		protected virtual void CheckJE_MasterBillIssuerSCAC()
		{
		}

		#endregion

		#region ValidateJE_HouseBillIssuerSCAC

		public void ValidateJE_HouseBillIssuerSCAC()
		{
			ValidateCalculatedProperty(Parent.JE_HouseBillIssuerSCACInfo);
		}

		protected virtual void CheckJE_HouseBillIssuerSCAC()
		{
		}

		#endregion

		#region ValidateJE_PrimaryITNumber

		public void ValidateJE_PrimaryITNumber()
		{
			ValidateCalculatedProperty(Parent.JE_PrimaryITNumberInfo);
		}

		protected virtual void CheckJE_PrimaryITNumber()
		{
		}

		#endregion

		#region ValidateBrokerToPayIndicator

		public void ValidateBrokerToPayIndicator()
		{
			ValidateCalculatedProperty(Parent.BrokerToPayIndicatorInfo);
		}

		protected virtual void CheckBrokerToPayIndicator()
		{
			bool shouldAddMessageError = (Parent.IsImport && !Parent.US_PaymentType.IsEmpty && Parent.BrokerToPayIndicator.IsEmpty) ||
				(Parent.IsRecon && !Parent.ReconDeclaration.US_PaymentType.IsEmpty && Parent.BrokerToPayIndicator.IsEmpty);

			if (shouldAddMessageError)
			{
				Parent.BrokerToPayIndicatorInfo.AddMessageError(BrokerToPayRequired);
			}
		}
		internal const string BrokerToPayRequired = "Please indicate whether payment will be made by the Broker.";

		#endregion

		#region ValidateWHSInvLineFilter
		public void ValidateWHSInvLineFilter()
		{
			ValidateCalculatedProperty(Parent.WHSInvLineFilterInfo);
		}

		protected virtual void CheckWHSInvLineFilter()
		{
		}
		#endregion

		#region ValidateWHSProductFilter
		public void ValidateWHSProductFilter()
		{
			ValidateCalculatedProperty(Parent.WHSProductFilterInfo);
		}

		protected virtual void CheckWHSProductFilter()
		{
		}
		#endregion

		#region ValidateWHSPackageFilter
		public void ValidateWHSPackageFilter()
		{
			ValidateCalculatedProperty(Parent.WHSPackageFilterInfo);
		}

		protected virtual void CheckWHSPackageFilter()
		{
		}
		#endregion

		#region Overriden Check

		protected override void CheckJE_TotalNoOfPacksPackTypeIsAValidCode()
		{
			ListValidation.WarnIfInvalidCode(Parent.JE_TotalNoOfPacksPackTypeInfo, Parent.Lookups.JE_TotalNoOfPacksPackType_List, ValidationConstants.InvalidPackTypeMessage);
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();

			if (Parent.IsSea && !Parent.IsExWarehouse)
			{
				if (VesselRequired)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);
				}
				else
				{
					if (Parent.JE_VesselName.Length > VesselNameLength)
					{
						Parent.JE_VesselNameInfo.AddWarning(string.Format(ValidationConstants.Declaration.VesselNameLength, VesselNameLength.ToString()));
					}
				}
			}
		}

		protected virtual bool VesselRequired
		{
			get { return Parent.JE_VesselName.IsEmpty; }
		}

		public virtual int VesselNameLength
		{
			get { return JobDeclaration.Schema.VesselNameLength; }
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (Parent.JE_VoyageFlightNo.IsEmpty && IsVoyageFlightNoRequired && Parent.IsVoyageFlightNumberVisible)
			{
				Parent.JE_VoyageFlightNoInfo.AddMessageError(VoyageFlightNoRequired);
			}
		}

		protected virtual bool IsVoyageFlightNoRequired
		{
			get { return (Parent.IsSea || Parent.IsAir) && !Parent.IsExWarehouse; }
		}

		internal const string VoyageFlightNoRequired = "Voyage/Flight/Reg. No is required for Air or Sea.";

		protected override void CheckJE_ContainerMode()
		{
			if (ShouldValidateContainerMode)
			{
				base.CheckJE_ContainerMode();

				if (Parent.ContainersAlwaysRequired && Parent.CusContainers.Count == 0)
				{
					if (Parent.IsExport)
					{
						Parent.JE_ContainerModeInfo.AddWarning(NoContainerEnteredWhenContainerModeIsSelected);
					}
					else
					{
						Parent.JE_ContainerModeInfo.AddMessageError(NoContainerEnteredWhenContainerModeIsSelected);
					}
				}

				if (!Parent.IsExWarehouse && !Parent.IsConsumptionFTZ && (Parent.IsAir || Parent.IsSea || Parent.IsTruck || Parent.IsRail))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ContainerModeInfo);
				}
			}
		}
		internal const string NoContainerEnteredWhenContainerModeIsSelected = "The container mode indicates this shipment is containerized, as yet, no containers have been entered.";

		protected virtual bool ShouldValidateContainerMode
		{
			get { return Parent.IsContainerSupported; }
		}

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();

			ListValidation.ErrorIfInvalidCode(Parent.JE_GS_NKCusAgentInfo, Parent.Lookups.CusAgents);

			if (Parent.IsEntrySummaryValidationMode)
			{
				if (Parent.Branch != null)
				{
					if (Parent.JE_GS_NKCusAgent.IsEmpty && USCustomsDataRegistry.Instance.EntryDeclarant.GetFallBackValueAtAllLevels(Parent.RegistryCompanyPK, Parent.RegistryBranchPK, Guid.Empty))
					{
						Parent.JE_GS_NKCusAgentInfo.AddError(BrokerRequired);
					}
				}
			}
			if (Parent.IsRecon && Parent.ReconDeclaration.IsACE && Parent.JE_GS_NKCusAgent.IsEmpty)
			{
				Parent.JE_GS_NKCusAgentInfo.AddMessageError(BrokerRequired);
			}
		}
		internal const string BrokerRequired = "Broker field is mandatory. The registry is currently set to require this field to be manually entered.";
		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();

			if (Parent.IsRecon)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JE_ApplicationCodeInfo, Parent.Lookups.ApplicationCodeList, (ZArchitecture.Core.NoResString)"Message Mode");
				MandatoryValidation.CheckEntered(Parent.JE_ApplicationCodeInfo, "Message Mode");
			}
		}

		#endregion

		protected override Customs.Business.BillValidator GetBillValidator()
		{
			return new BillValidator();
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;
	}
}
