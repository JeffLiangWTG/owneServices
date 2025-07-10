//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDrawbackAdditionalImportTariffNumberAddInfoValidation
//
//    This class should be used for overriding validation in AutoDrawbackAdditionalImportTariffNumberAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackAdditionalImportTariffNumberAddInfoValidation : AutoDrawbackAdditionalImportTariffNumberAddInfoValidation
	{
		public DrawbackAdditionalImportTariffNumberAddInfoValidation(AutoDrawbackAdditionalImportTariffNumberAddInfo parent)
			: base(parent)
		{
		}

		protected new DrawbackAdditionalImportTariffNumberAddInfo Parent
		{
			get { return (DrawbackAdditionalImportTariffNumberAddInfo)base.Parent; }
		}

		protected override void CheckUS_Tariff()
		{
			base.CheckUS_Tariff();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_TariffInfo, Parent.Lookups.ImportTariffs);
		}

		protected override void CheckUS_Description()
		{
			base.CheckUS_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DescriptionInfo);
		}

		protected override void CheckUS_Quantity1()
		{
			base.CheckUS_Quantity1();

			var parent = Parent;
			if (!parent.US_Quantity1.IsEmpty)
			{
				parent.US_Quantity1Info.AddMessageError(QuantityShouldNotBeEntered);
			}
		}

		protected override void CheckUS_AllowableQty1()
		{
			base.CheckUS_AllowableQty1();

			var parent = Parent;
			if (!parent.US_AllowableQty1.IsEmpty)
			{
				parent.US_AllowableQty1Info.AddMessageError(QuantityShouldNotBeEntered);
			}
		}

		public const string QuantityShouldNotBeEntered = "Quantity should not be entered against additional tariff numbers.";

		protected override void CheckUS_UQ1()
		{
			base.CheckUS_UQ1();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.US_UQ1Info, parent.Lookups.UnitOfMeasureCodes);
			if (!parent.US_UQ1.IsEmpty && parent.US_UQ1 != (parent.InvoiceLine?.DRWImportUQ ?? ZString.Empty))
			{
				parent.US_UQ1Info.AddMessageError(UQNotMatch);
			}
		}

		internal const string UQNotMatch = "Unit of Measure must match the UOM of the main tariff number.";

		protected override void CheckUS_ValuePerUnit1()
		{
			base.CheckUS_ValuePerUnit1();

			var parent = Parent;
			if (!parent.US_ValuePerUnit1.IsEmpty && parent.InvoiceLine is JobComInvoiceLine invoiceLine && invoiceLine.IsValuePerUQNotMatch)
			{
				parent.US_ValuePerUnit1Info.AddWarning(invoiceLine.ValuePerUQNotMatchNotification);
			}
		}

		protected override void CheckUS_Quantity2()
		{
			base.CheckUS_Quantity2();

			if (!Parent.US_Quantity2.IsEmpty)
			{
				Parent.US_Quantity2Info.AddWarning(QuantityShouldNotBeEntered);
			}
		}

		protected override void CheckUS_UQ2()
		{
			base.CheckUS_UQ2();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.US_UQ2Info, parent.Lookups.UnitOfMeasureCodes);

			if (!parent.US_Quantity2.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.US_UQ2Info);
			}

			if (!parent.US_UQ2.IsEmpty && parent.US_UQ2 != (parent.InvoiceLine?.DRWImportUQ2 ?? ZString.Empty))
			{
				parent.US_UQ2Info.AddMessageError(UQNotMatch);
			}
		}

		protected override void CheckUS_ValuePerUnit2()
		{
			base.CheckUS_ValuePerUnit2();

			if (!Parent.US_Quantity2.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ValuePerUnit2Info);
			}
		}

		protected override void CheckUS_Quantity3()
		{
			base.CheckUS_Quantity3();

			if (!Parent.US_Quantity3.IsEmpty)
			{
				Parent.US_Quantity3Info.AddWarning(QuantityShouldNotBeEntered);
			}
		}

		protected override void CheckUS_UQ3()
		{
			base.CheckUS_UQ3();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.US_UQ3Info, parent.Lookups.UnitOfMeasureCodes);

			if (!parent.US_Quantity3.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.US_UQ3Info);
			}

			if (!parent.US_UQ3.IsEmpty && parent.US_UQ3 != (parent.InvoiceLine?.DRWImportUQ3 ?? ZString.Empty))
			{
				parent.US_UQ3Info.AddMessageError(UQNotMatch);
			}
		}

		protected override void CheckUS_ValuePerUnit3()
		{
			base.CheckUS_ValuePerUnit3();

			if (!Parent.US_Quantity3.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ValuePerUnit3Info);
			}
		}

		protected override void CheckUS_SubstitutedValue1()
		{
			base.CheckUS_SubstitutedValue1();

			var parent = Parent;
			if (IsSubstitutedValueRequired)
			{
				if (parent.US_SubstitutedValue1.IsEmpty && !(parent?.InvoiceLine.DRWImportQuantity.IsEmpty ?? true))
				{
					parent.US_SubstitutedValue1Info.AddMessageError(ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
				}
			}
			else if (!parent.US_SubstitutedValue1.IsEmpty)
			{
				parent.US_SubstitutedValue1Info.AddMessageError(ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			}
		}

		protected override void CheckUS_SubstitutedValue2()
		{
			base.CheckUS_SubstitutedValue2();

			var parent = Parent;
			if (IsSubstitutedValueRequired)
			{
				if (parent.US_SubstitutedValue2.IsEmpty && !(parent?.InvoiceLine.DRWImportQuantity2.IsEmpty ?? true))
				{
					parent.US_SubstitutedValue2Info.AddMessageError(ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
				}
			}
			else if (!parent.US_SubstitutedValue2.IsEmpty)
			{
				parent.US_SubstitutedValue2Info.AddMessageError(ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			}
		}

		protected override void CheckUS_SubstitutedValue3()
		{
			base.CheckUS_SubstitutedValue3();

			var parent = Parent;
			if (IsSubstitutedValueRequired)
			{
				if (parent.US_SubstitutedValue3.IsEmpty && !(parent?.InvoiceLine.DRWImportQuantity3.IsEmpty ?? true))
				{
					parent.US_SubstitutedValue3Info.AddMessageError(ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
				}
			}
			else if (!parent.US_SubstitutedValue3.IsEmpty)
			{
				parent.US_SubstitutedValue3Info.AddMessageError(ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			}
		}

		#region Flags

		ZBool IsSubstitutedValueRequired
		{
			get
			{
				var additionalImportTariff = Parent.Parent as DrawbackAdditionalImportTariffNumber;
				var declaration = additionalImportTariff?.InvoiceLine?.Declaration;
				return declaration != null && ACEDrawbackProvisionsList.IsSubstitutedValueRequired(declaration.US_EntryType);
			}
		}

		#endregion
	}
}
