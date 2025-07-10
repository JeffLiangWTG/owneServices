using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public abstract class JobComInvoiceLineValidation : Customs.Business.BaseJobComInvoiceLineValidation
	{
		protected JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateBondedWhsQuantityForGUI();
			ValidateFreightInLocalCurrency();
			if (InvoiceLine.HasFDAData)
			{
				ValidateFDAValueUSDRunningTotalString();
			}
			ValidateUS_ADDDepositRateDescription();
			ValidateUS_CVDDepositRateDescription();
			ValidateJI_OA_FDAShipperAddress();
			ValidateSupFormattedAdditionalTariff1();
			ValidateSupFormattedAdditionalTariff2();
			ValidateSupFormattedAdditionalTariff3();
			ValidateSupFormattedAdditionalTariff4();
			ValidateSupFormattedAdditionalTariff5();
		}

		protected override void CheckJI_OA_ConsigneeAddress()
		{
			base.CheckJI_OA_ConsigneeAddress();
			if (!Parent.JI_OA_ConsigneeAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JI_OA_ConsigneeAddressInfo, Parent.ConsigneeAddress);
			}
		}

		protected override void CheckJI_OA_ManufacturerAddress()
		{
			base.CheckJI_OA_ManufacturerAddress();

			if (!Parent.JI_OA_ManufacturerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JI_OA_ManufacturerAddressInfo, Parent.ManufacturerAddress);
			}
		}

		protected override void CheckJI_OA_ExporterAddress()
		{
			base.CheckJI_OA_ExporterAddress();
			if (!Parent.JI_OA_ExporterAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JI_OA_ExporterAddressInfo, Parent.ExporterAddress);
			}
		}

		protected override void CheckJI_OA_Seller()
		{
			base.CheckJI_OA_Seller();
			if (!Parent.JI_OA_Seller.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JI_OA_SellerInfo, Parent.SellerAddress);
			}
		}

		protected override void CheckJI_OA_ShipToPartyAddress()
		{
			base.CheckJI_OA_ShipToPartyAddress();
			if (!Parent.JI_OA_ShipToPartyAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JI_OA_ShipToPartyAddressInfo, Parent.ShipToPartyAddress);
			}
		}

		protected override void CheckJI_OA_SoldToPartyAddress()
		{
			base.CheckJI_OA_SoldToPartyAddress();
			if (!Parent.JI_OA_SoldToPartyAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JI_OA_SoldToPartyAddressInfo, Parent.SoldToPartyAddress);
			}
		}

		public void ValidateJI_OA_FDAShipperAddress()
		{
			ValidateCalculatedProperty(Parent.JI_OA_FDAShipperAddressInfo);
		}

		protected virtual void CheckJI_OA_FDAShipperAddress()
		{
			var invoiceline = Parent;
			if (!invoiceline.JI_OA_FDAShipperAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(invoiceline.JI_OA_FDAShipperAddressInfo, invoiceline.FDAShipperAddress);
			}
		}

		public void ValidateBondedWhsQuantityForGUI()
		{
			ValidateCalculatedProperty(Parent.BondedWhsQuantityForGUIInfo);
		}

		protected virtual void CheckBondedWhsQuantityForGUI()
		{
		}

		protected override void CheckJI_Procedure()
		{
		}

		protected override void CheckJI_LineNo()
		{
			base.CheckJI_LineNo();
			Parent.AddInfoValidation.ValidateSetIndicator();
		}

		protected override void CheckJI_PartNo()
		{
			if (Parent.JI_PartNo_CanBeSetByCustomer)
			{
				base.CheckJI_PartNo();
				var part = Parent.Part;
				if (part != null && Parent.Pivot == null)
				{
					if (Parent.IsImport)
					{
						var importer = Parent.Importer;
						var supplier = Parent.Supplier;
						var partAttrib1 = ZString.Empty;
						var partAttrib2 = ZString.Empty;
						var partAttrib3 = ZString.Empty;
						if (importer != null)
						{
							partAttrib1 = importer.PartAttributeManager.PartAttributeName1;
							partAttrib2 = importer.PartAttributeManager.PartAttributeName2;
							partAttrib3 = importer.PartAttributeManager.PartAttributeName3;
						}

						Parent.JI_PartNoInfo.AddWarning(ValidationConstants.InvoiceLine.ImportTariff.CannotMatchClassificationForPart(part.OP_PartNum, importer == null ? ZString.Empty : importer.OH_Code, supplier == null ? ZString.Empty : supplier.OH_Code, partAttrib1, Parent.JI_PartAttrib1, partAttrib2, Parent.JI_PartAttrib2, partAttrib3, Parent.JI_PartAttrib3, Parent.JI_SerialNumber, Parent.EffectiveDateForDutyRate));
					}
					else if (Parent.IsExport)
					{
						var importer = Parent.Importer;
						var supplier = Parent.Supplier;
						Parent.JI_PartNoInfo.AddWarning(ValidationConstants.InvoiceLine.ExportTariff.CannotMatchClassificationForPart(part.OP_PartNum, importer == null ? ZString.Empty : importer.OH_Code, supplier == null ? ZString.Empty : supplier.OH_Code));
					}
				}

				else if (part == null)
				{
					if (Parent.IsImport && (Parent.Declaration?.IsFTZWeeklyEstimateIntegrationEnabled ?? false) && (!Parent.HasEmptySupTariff || Parent.IsParentLine))
					{
						Parent.JI_PartNoInfo.AddMessageError(ValidationConstants.InvoiceLine.ImportTariff.ProductCannotBeEmpty);
					}
				}
			}
		}

		protected override void CheckJI_PartAttrib1()
		{
			base.CheckJI_PartAttrib1();
			ValidateJI_PartNo();
		}

		protected override void CheckJI_PartAttrib2()
		{
			base.CheckJI_PartAttrib2();
			ValidateJI_PartNo();
		}

		protected override void CheckJI_PartAttrib3()
		{
			base.CheckJI_PartAttrib3();
			ValidateJI_PartNo();
		}

		protected override void CheckJI_SerialNumber()
		{
			base.CheckJI_SerialNumber();
			ValidateJI_PartNo();
		}

		public void ValidateFreightInLocalCurrency()
		{
			ValidateCalculatedProperty(Parent.FreightInLocalCurrencyInfo);
		}

		protected virtual void CheckFreightInLocalCurrency()
		{
		}

		public void ValidateFDAValueUSDRunningTotalString()
		{
			ValidateCalculatedProperty(Parent.FDAValueUSDRunningTotalStringInfo);
		}

		protected void CheckFDAValueUSDRunningTotalString()
		{
			if (InvoiceLine.Declaration.IsImport && InvoiceLine.IsOGAValueUpToDate)
			{
				var roundedCV = InvoiceLine.GetEnteredValueForOGA();
				if (InvoiceLine.FDAValueUSDRunningTotal.Round(0) != roundedCV)
				{
					string balanceWarning = string.Format("The total USD FDA Value entered does not balance with the Customs Value ({0} USD).", roundedCV);
					InvoiceLine.FDAValueUSDRunningTotalStringInfo.AddWarning(balanceWarning);
				}
			}
		}

		protected override void CheckJI_CC()
		{
			base.CheckJI_CC();
			ValidateJI_Tariff();
		}

		protected override void CheckJI_Tariff()//unsealed because there are cases where it should call other fields' validations even if empty
		{
			base.CheckJI_Tariff();

			if (Parent.JI_Tariff.IsEmpty)
			{
				if (!Parent.JI_PartNo.IsEmpty && Parent.Part == null && Parent.JI_CC.IsEmpty)
				{
					Parent.JI_TariffInfo.AddWarning(TariffOrLookUpIsRequiredForAutoCreateProduct);
				}
			}
			else
			{
				CheckJI_TariffIsValidWhenItIsNotEmpty();
				Parent.AddInfoValidation.ValidateSetIndicator();
			}

			if (!Parent.TariffCalculateExceptionMessage.IsEmpty)
			{
				Parent.JI_TariffInfo.AddError(Parent.TariffCalculateExceptionMessage);
			}
		}
		internal const string TariffOrLookUpIsRequiredForAutoCreateProduct = "Either this field or the lookup field should be specified to auto-create a Product.";

		protected abstract void CheckJI_TariffIsValidWhenItIsNotEmpty();

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_WeightUQInfo, Parent.Lookups.WeightUQList, (NoResString)WeightUQShouldBeInList);
		}

		internal const string WeightUQShouldBeInList = "Please enter a valid Weight UQ Code. The code you have selected is not in the Weight UQ codes List.";

		protected override void CheckJI_ParentID()
		{
			base.CheckJI_ParentID();
			if (Parent.PK == Parent.JI_ParentID)
			{
				Parent.JI_ParentIDInfo.AddError(YouHaveSetItselfToParent);
			}

			if (Parent.IsSecondaryTariffLine) // Maximum of 8 tariffs per line only - Parent & 7 additional tariffs
			{
				JobComInvoiceLine parentLine = Parent.ParentTariffLine;

				if (parentLine != null && parentLine.ChildLines.IsCountMoreThan(7))
				{
					Parent.JI_ParentIDInfo.AddMessageError(MaxSecondaryLinesCount);
				}
			}
		}
		#region US_ADD/CVDDepositRateDescription
		public void ValidateUS_ADDDepositRateDescription()
		{
			ValidateCalculatedProperty(Parent.US_ADDDepositRateDescriptionInfo);
		}

		protected void CheckUS_ADDDepositRateDescription()
		{
			CheckDescription(Parent.US_ADDDepositRateIndicator, Parent.US_ADDDepositRateDescription, Parent.US_ADDDepositRateDescriptionInfo);
		}

		public void ValidateUS_CVDDepositRateDescription()
		{
			ValidateCalculatedProperty(Parent.US_CVDDepositRateDescriptionInfo);
		}

		protected void CheckUS_CVDDepositRateDescription()
		{
			CheckDescription(Parent.US_CVDDepositRateIndicator, Parent.US_CVDDepositRateDescription, Parent.US_CVDDepositRateDescriptionInfo);
		}

		void CheckDescription(ZString indicator, ZString description, ZPropertyInfo info)
		{
			if (indicator == DepositRateIndicatorList.Codes.OverrideAdValorem)
			{
				if (!CheckDescriptionIsPercentage(description))
				{
					info.AddError(MustEnterInPercent);
				}
			}
			if (indicator == DepositRateIndicatorList.Codes.OverrideSpecific)
			{
				if (description.Contains("%"))
				{
					info.AddError(PercentIsNotAllowed);
				}
			}
		}

		bool CheckDescriptionIsPercentage(ZString description)
		{
			description = description.Trim();
			if (!description.EndsWith("%"))
			{
				return false;
			}
			var subString = description.Substring(0, description.Length - 1);
			return ZDecimal.ParseSafe(subString, new ZDecimal(-1m)) >= 0;
		}
		#endregion

		public const string PercentIsNotAllowed = "Percent is not allowed.";
		public const string MustEnterInPercent = "Must enter in percent format (% at the end).";
		public const string YouHaveSetItselfToParent = "You have just set this line to itself as a parent.";
		public const string MaxSecondaryLinesCount = "An entry line can only have a maximum of 8 Tariff Numbers. (i.e. Parent can only have at most 7 linked tariff lines)";

		protected override bool ClassificationIsRequiredForAutoCreationOfProduct
		{
			get { return false; }
		}

		public override void MatchToProductValidation(ZPropertyInfo propertyInfo, Func<IZType> getPivotValue)
		{
			var newGetPivotValue = () =>
			{
				using (Parent.SuspendPreviousPivotPKUpdate())
				{
					return getPivotValue();
				}
			};

			base.MatchToProductValidation(propertyInfo, newGetPivotValue);
		}

		public void ValidateSupFormattedAdditionalTariff1()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff1Info);
		}

		protected virtual void CheckSupFormattedAdditionalTariff1()
		{
		}

		public void ValidateSupFormattedAdditionalTariff2()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff2Info);
		}

		protected virtual void CheckSupFormattedAdditionalTariff2()
		{
		}

		public void ValidateSupFormattedAdditionalTariff3()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff3Info);
		}

		protected virtual void CheckSupFormattedAdditionalTariff3()
		{
		}

		public void ValidateSupFormattedAdditionalTariff4()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff4Info);
		}

		protected virtual void CheckSupFormattedAdditionalTariff4()
		{
		}

		public void ValidateSupFormattedAdditionalTariff5()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff5Info);
		}

		protected virtual void CheckSupFormattedAdditionalTariff5()
		{
		}
	}
}
