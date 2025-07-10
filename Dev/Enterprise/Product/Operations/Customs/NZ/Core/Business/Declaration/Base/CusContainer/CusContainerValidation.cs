using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusContainerValidation : AutoNZCusContainerValidation
	{
		public CusContainerValidation(CusContainer container)
			: base(container)
		{
		}

		#region Container
		protected CusContainer Container
		{
			get { return (CusContainer)base.Parent; }
		}
		#endregion

		#region CheckCO_Seal

		protected override void CheckCO_Seal()
		{
			base.CheckCO_Seal();
			JobDeclaration declaration = Container.Declaration;
			if (declaration != null && declaration.IsExport && declaration.IsExportedUnderSecureExportPartnershipScheme)
			{
				if (Container.IsFullContainer && Container.CO_Seal.IsEmpty)
				{
					Container.CO_SealInfo.AddMessageError(MessageErrorContainersExportedUnderSEPMustHaveSealNumber);
				}
			}
		}

		public const string MessageErrorContainersExportedUnderSEPMustHaveSealNumber = "Container Must Have Seal Number - Containers exported under the Secure Export Partnership scheme must have a seal number.";

		#endregion

		#region CheckCO_RC

		protected override void CheckCO_RC()
		{
			JobDeclaration declaration = Container.Declaration;
			if (declaration != null && !declaration.IsExport)
			{
				base.CheckCO_RC();
			}
			else if (declaration == null)
			{
				base.CheckCO_RC();
			}
		}

		#endregion

		#region CheckCO_FCL_LCL_AIR
		protected override void CheckCO_FCL_LCL_AIR()
		{
			base.CheckCO_FCL_LCL_AIR();
			if (Container.CO_FCL_LCL_AIR.IsEmpty)
			{
				Container.CO_FCL_LCL_AIRInfo.AddMessageError(MessageErrorContainerModeRequired);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Container.CO_FCL_LCL_AIRInfo, Container.Lookups.CO_FCL_LCL_NCT_List);
			}

			var declaration = Container.Declaration;
			if (declaration != null)
			{
				if (declaration.IsFormalEntry)
				{
					ValidateCO_Seal();
					declaration.Validation.ValidateJE_SendMCDContainerQuarantineDeclaration();
				}
				else if (declaration.IsECIWriteoff)
				{
					declaration.Validation.ValidateJE_ECI_InvoiceAmount();
					ValidateCO_Weight();
				}

				if (declaration.IsTSWCREWriteOff)
				{
					if (ConsignmentHasEmptyAndNonEmptyContainers)
					{
						Container.CO_FCL_LCL_AIRInfo.AddError(ConsignmentCannotMixContainers);
					}
				}
			}
		}

		public const string MessageErrorContainerModeRequired = "Container Mode Required  - Container Mode is Required for each Container Entered.";
		public const string ConsignmentCannotMixContainers = "For TSW write-off (CRE), you may not mix empty and non-empty containers in the same consignment.";

		bool ConsignmentHasEmptyAndNonEmptyContainers
		{
			get
			{
				var declaration = Container.Declaration;
				bool hasEmptyContainers = declaration.CusContainers.Cast<CusContainer>().FirstOrDefault(x => x.CO_FCL_LCL_AIR == ContainerModeList.Codes.Empty) != null;
				bool hasNonEmptyContainers = declaration.CusContainers.Cast<CusContainer>().FirstOrDefault(x => !x.CO_FCL_LCL_AIR.IsEmpty && x.CO_FCL_LCL_AIR != ContainerModeList.Codes.Empty) != null;
				return hasEmptyContainers && hasNonEmptyContainers;
			}
		}

		#endregion

		#region CheckCO_ContainerNumber and associated overrides/virtuals

		protected override void CheckCO_ContainerNumber()
		{
			if (Container.CO_ContainerNumber.IsEmpty)
			{
				Container.CO_ContainerNumberInfo.AddMessageError(MessageErrorMustHaveContainerNumber);
			}
			else
			{
				base.CheckCO_ContainerNumber();
				var declaration = Container.Declaration;
				if (declaration != null && declaration.IsTSWCREWriteOff && declaration.InvoiceLines.Count > 0)
				{
					if (declaration.HasContainersAndTheyreAllEmpty)
					{
						Container.CO_ContainerNumberInfo.AddError(EmptyContainerEntryCannotHaveInvoiceLines);
					}
					else
					{
						CheckContainerIsLinkedToAnInvoiceLine(Container.CO_ContainerNumber);
					}
				}
			}
		}

		public override void ContainerPackageNotFoundMessage()
		{
			var notificationType = (Container.Declaration?.IsTSWMessagingValidation ?? false) ? NotificationType.Error : NotificationType.MessageError;
			Parent.CO_ContainerNumberInfo.AddNotification(notificationType, ErrorMustHaveContainerPackingLine);
		}

		protected override void CheckContainerNoHasPackages()
		{
			if (!Container.IsEmptyContainer)
			{
				base.CheckContainerNoHasPackages();
			}
		}

		protected virtual void CheckContainerIsLinkedToAnInvoiceLine(string containerNo)
		{
			foreach (JobComInvoiceLine invLine in Parent.Declaration.InvoiceLines)
			{
				if (invLine.ContainersForInvoiceLinesForBindingOnly.Count > 0)
				{
					foreach (NonPersistentCusContainer lineContainer in invLine.ContainersForInvoiceLinesForBindingOnly)
					{
						if (lineContainer.ContainerNumber == containerNo && lineContainer.IsForInvoiceLine)
						{
							return;
						}
					}
				}
			}

			Parent.CO_ContainerNumberInfo.AddMessageError(CREContainerRequiresInvoiceLine);
		}

		public const string CREContainerRequiresInvoiceLine = "When reporting (non-empty) containers with consignment invoice lines, an invoice line must link the consignment item to a respective container.\r\n(Use: Inv Lines > Containers > Is for Invoice Line?)";
		public const string MessageErrorMustHaveContainerNumber = "Container No Required  - Container No is Required for each Container Entered.";
		public const string EmptyContainerEntryCannotHaveInvoiceLines = "Invoice Lines are not allowed for empty container(s) write-off declaration.";
		public const string ErrorMustHaveContainerPackingLine = "Container has no packing lines - You must enter some packing details for this container on the Packing tab, or remove this container from the entry.";

		#endregion

		#region CheckContainerNoHasValidCheckDigit
		protected override void CheckContainerNoHasValidCheckDigit()
		{
			JobDeclaration declaration = Container.Declaration;
			if (declaration != null && declaration.IsFormalEntry && Container.CO_ContainerNumber.Left(1) == "P" && Container.CO_ContainerNumber.Length <= 6)
			{
				if (!Container.ContainerNumberIsValidPalletNumber())
				{
					Container.CO_ContainerNumberInfo.AddMessageError(MessageErrorInvalidPalletNumberEntered);
				}
			}
			else
			{
				base.CheckContainerNoHasValidCheckDigit();
			}
		}

		public const string MessageErrorInvalidPalletNumberEntered = "Invalid Pallet Number - Must be a 'P' followed by a 1 or 2 digit number.";
		#endregion

		#region CheckCO_ContainerSize
		protected override void CheckCO_ContainerSize()
		{
			base.CheckCO_ContainerSize();
			JobDeclaration declaration = Container.Declaration;
			if (declaration != null && declaration.IsECIWriteoff && !declaration.IsTSWDeclaration)
			{
				if (Container.CO_ContainerSize.IsEmpty)
				{
					Container.CO_ContainerSizeInfo.AddMessageError(MessageErrorMustHaveContainerSize);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Container.CO_ContainerSizeInfo, declaration.Lookups.ContainerSizeList);
				}
			}
		}
		public const string MessageErrorMustHaveContainerSize = "Enter a valid Container Size.";
		#endregion

		#region CheckCO_Weight
		protected override void CheckCO_Weight()
		{
			base.CheckCO_Weight();
			JobDeclaration declaration = Container.Declaration;
			if (declaration != null && declaration.IsECIWriteoff)
			{
				if (Container.IsEmptyContainer)
				{
					if (Container.CO_Weight != ZDecimal.Zero)
					{
						Container.CO_WeightInfo.AddMessageError(MessageErrorCannotHaveWeightOnAnEmptyContainer);
					}
				}
				else
				{
					if (Container.CO_Weight <= ZDecimal.Zero)
					{
						Container.CO_WeightInfo.AddMessageError(MessageErrorMustHaveWeightOnNonEmptyContainer);
					}
				}
				declaration.Validation.ValidateJE_TotalWeight();
			}
		}
		public const string MessageErrorCannotHaveWeightOnAnEmptyContainer = "The Weight of the Goods in an Empty Container must be 0 Kg.";
		public const string MessageErrorMustHaveWeightOnNonEmptyContainer = "Please enter the Weight of the Goods contained within this container.";

		#endregion

		#region CO_OA_PackingLocation
		public void ValidateCO_OA_PackingLocation()
		{
			ValidateCalculatedProperty(Container.CO_OA_PackingLocationInfo);
		}

		protected virtual void CheckCO_OA_PackingLocation()
		{
			if (Container != null && !Container.IsEmptyContainer)
			{
				var declaration = Container.Declaration;
				if (declaration != null && declaration.IsTSWICRWriteOff)
				{
					if (Container.PackingLocationOrgPK.IsEmpty)
					{
						Container.PackingLocationOrgPKInfo.AddWarning(StuffingEstablishmentRecommended);
					}
					else if (!HasAllRequiredAddressDetails(Container.CO_OA_PackingLocation_ZAddress))
					{
						Container.CO_OA_PackingLocationInfo.AddMessageError(StuffingEstablishmentAddressRequired);
					}
				}
			}
		}

		bool HasAllRequiredAddressDetails(ZAddress stuffingAddress)
		{
			var result = false;
			if (stuffingAddress != null && stuffingAddress.OrgAddress != null)
			{
				var packingLocationAddress = stuffingAddress.OrgAddress as OrgAddress;
				result = !packingLocationAddress.OA_Address1.IsEmpty
					&& !packingLocationAddress.OA_City.IsEmpty
					&& !(packingLocationAddress.OA_RN_NKCountryCode.IsEmpty && packingLocationAddress.OA_RL_NKRelatedPortCode.IsEmpty);
			}

			return result;
		}

		public const string StuffingEstablishmentRecommended = "Container Pack Location is critical for MPI risk assessment. A profile will be in place to hold all shipments where this data is not provided.";
		public const string StuffingEstablishmentAddressRequired = "Container Pack Location requires a valid address - please select and appropriate address for this organisation.";
		#endregion

		#region CheckCO_MAF_ContainerType
		protected override void CheckCO_MAF_ContainerType()
		{
			base.CheckCO_MAF_ContainerType();
			if (Container?.Declaration?.IsTSWDeclaration ?? false)
			{
				if (Container.CO_MAF_ContainerType.IsEmpty)
				{
					Container.CO_MAF_ContainerTypeInfo.AddMessageError(EquipmentSizeAndTypeDescriptionCode);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Container.CO_MAF_ContainerTypeInfo, Container.Lookups.MAFContainerTypeList);
				}
			}
		}

		public const string EquipmentSizeAndTypeDescriptionCode = "Enter a valid Equipment size and type description code from the drop down list.";
		#endregion

		#region CheckCO_SealingParty
		protected override void CheckCO_SealingParty()
		{
			base.CheckCO_SealingParty();
			if (Container?.CO_SealingParty.IsEmpty ?? false)
			{
				var declaration = Container.Declaration;
				if (declaration != null
					&& declaration.IsTSWDeclaration
					&& declaration.IsExport
					&& declaration.IsExportedUnderSecureExportPartnershipScheme)
				{
					Container.CO_SealingPartyInfo.AddMessageError(SealingPartyRequired);
				}
			}
		}

		public const string SealingPartyRequired = "Sealing Party must be transmitted when containers are exported under the New Zealand Customs Secure Export Scheme.\r\nState the name of the party affixing the Customs-approved seal.";
		#endregion
	}
}
