using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FTZJobComInvoiceLineValidation : CommonImportJobComInvoiceLineValidation
	{
		public FTZJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		#region JI_CustomsQuantity

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();

			if (IsFTZAdmissionValidationMode)
			{
				if (Parent.JI_CustomsQuantity >= 0m)
				{
					CheckJI_CustomsQuantityBoundary(NotificationType.MessageError);
				}
			}
		}

		#endregion

		#region JI_LinePrice

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();

			if (IsFTZAdmissionValidationMode)
			{
				var priceValidator = new ImportLinePriceValidator();

				if (Parent.IsSetXLine)
				{
					priceValidator.ValidateForSetXLine(Parent);
				}
				else
				{
					CheckJI_LinePriceBoundary();
				}
			}
		}

		#endregion

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();

			if (Parent.JI_InvoiceQuantity > ZDecimal.Zero && Parent.JI_PartNo_CanBeSetByCustomer && Parent.SupplierPart != null)
			{
				var declaration = Parent.Declaration;
				var isInwardBondedWarehousingEnabled = declaration != null && declaration.IsInwardBondedWarehousingEnabled;
				if (isInwardBondedWarehousingEnabled && Parent.JI_BondedWhsQuantity.IsEmpty && Parent.WHSPackLines.Count > 0 && Parent.JI_InvoiceQuantity != Parent.JI_Calc_AllocatedQty)
				{
					Parent.JI_InvoiceQuantityInfo.AddMessageError(ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty(declaration.TermNameForBondedWarehouse));
				}
			}
		}

		protected override void CheckJI_ParentID()
		{
			base.CheckJI_ParentID();
			ValidateJI_InvoiceQuantity();
			ValidateJI_BondedWhsQuantity();
		}

		protected override void CheckJI_PartNo()
		{
			base.CheckJI_PartNo();
			ValidateJI_InvoiceQuantity();
			ValidateJI_BondedWhsQuantity();
		}

		protected override void CheckBondedWhsQuantityForGUI()
		{
			base.CheckBondedWhsQuantityForGUI();
			ValidateJI_BondedWhsQuantity();
			Parent.BondedWhsQuantityForGUIInfo.AddAllNotificationsFrom(Parent.JI_BondedWhsQuantityInfo);
		}

		protected override void CheckJI_BondedWhsQuantity()
		{
			base.CheckJI_BondedWhsQuantity();
			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsInwardBondedWarehousingEnabled && Parent.JI_PartNo_CanBeSetByCustomer && Parent.SupplierPart != null && Parent.JI_Calc_BondedWhsQuantity.IsEmpty)
			{
				Parent.JI_BondedWhsQuantityInfo.AddMessageError(ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired(declaration.TermNameForBondedWarehouse));
			}
			ValidateBondedWhsQuantityForGUI();
		}

		protected override void CheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			if (IsFTZAdmissionValidationMode)
			{
				CheckMIDAgainstCountryOfOrigin();
				CheckZoneIDForFDAReporting();
				TariffValidator.Validate(Parent, Parent.ImportTariff, Parent.JI_TariffInfo);
			}
		}

		protected override bool IsDescriptionRequired
		{
			get { return IsFTZAdmissionValidationMode; }
		}

		bool IsFTZAdmissionValidationMode
		{
			get { return Parent.Declaration.IsFTZAdmissionValidationMode; }
		}

		protected void CheckZoneIDForFDAReporting()
		{
			if (Parent.HasFDAReportingRequirement && !Parent.Declaration.IsZoneIDInFDAApprovedZonesList)
			{
				Parent.JI_TariffInfo.AddWarning(ZoneIDNotInFDAList);
			}
		}
		internal const string ZoneIDNotInFDAList = "Tariff is flagged for FDA reporting, but the Zone ID is not in the list of FDA approved zones.";

		protected override bool ForceMIDToBeEntered
		{
			get { return false; }
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();

			if (IsFTZAdmissionValidationMode)
			{
				CheckJI_CustomsSecondQuantityAgainstTariff();
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			Parent.AddInfoValidation.ValidateUS_ZoneStatus();
		}
	}
}
