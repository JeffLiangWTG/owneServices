using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USConstituentElementAddInfoValidation : AutoUSConstituentElementAddInfoValidation
	{
		public USConstituentElementAddInfoValidation(AutoUSConstituentElementAddInfo parent) : base(parent)
		{
		}

		internal ConstituentElement ConstituentElement
		{
			get { return ((ConstituentElementAddInfo)Parent).ConstituentElement as ConstituentElement; }
		}

		internal JobComInvoiceLine InvoiceLine
		{
			get { return ConstituentElement != null ? ConstituentElement.InvoiceLine : null; }
		}

		protected override void CheckUS_PGANameOfTheConstituentElement()
		{
			base.CheckUS_PGANameOfTheConstituentElement();

			if (IsNameRequired && IsCertifyCargoReleaseValidationMode)
			{
				Parent.US_PGANameOfTheConstituentElementInfo.AddMessageError(NameRequired);
			}
		}

		internal virtual string NameRequired
		{
			get { return "Name Of The Constituent Element is mandatory."; }
		}

		protected virtual bool IsNameRequired
		{
			get
			{
				return Parent.US_PGANameOfTheConstituentElement.IsEmpty;
			}
		}

		protected override void CheckUS_PGAPercentOfConstituentElement()
		{
			base.CheckUS_PGAPercentOfConstituentElement();

			if (IsCertifyCargoReleaseValidationMode)
			{
				if (Parent.US_PGAPercentOfConstituentElement > 100m)
				{
					Parent.US_PGAPercentOfConstituentElementInfo.AddMessageError(MoreThan100Percents);
				}
			}
		}
		internal const string MoreThan100Percents = "You cannot enter more than 100% of the ingredient in the product.";

		protected override void CheckUS_PGAQuantityOfConstituentElement()
		{
			base.CheckUS_PGAQuantityOfConstituentElement();

			if (IsCertifyCargoReleaseValidationMode && Parent.US_PGAQuantityOfConstituentElement.IsEmpty && IsQtyRequired)
			{
				Parent.US_PGAQuantityOfConstituentElementInfo.AddMessageError(QuantityRequired);
			}
			ValidateUS_PGAUnitOfMeasure();
		}
		internal const string QuantityRequired = "Quantity Of Constituent Element is mandatory.";

		protected virtual bool IsQtyRequired
		{
			get
			{
				return ConstituentElement is ConstituentElement constituentElement && constituentElement.Parent is PGA pga && pga.InvoiceLine != null;
			}
		}

		protected override void CheckUS_PGAUnitOfMeasure()
		{
			base.CheckUS_PGAUnitOfMeasure();

			if (!(ConstituentElement?.Parent is ACEFDA))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_PGAUnitOfMeasureInfo, Parent.Lookups.UnitsOfMeasureList);
			}

			if (IsCertifyCargoReleaseValidationMode)
			{
				if (Parent.US_PGAQuantityOfConstituentElement.IsEmpty)
				{
					if (!Parent.US_PGAUnitOfMeasure.IsEmpty)
					{
						if (InvoiceLine != null && !IsQtyRequired)
						{
							Parent.US_PGAUnitOfMeasureInfo.AddMessageError(UQNotRequired);
						}
						else
						{
							IngredientUQ(Parent.US_PGAUnitOfMeasure, Parent.US_PGAUnitOfMeasureInfo);
						}
					}
				}
				else if (Parent.US_PGAUnitOfMeasure.IsEmpty)
				{
					Parent.US_PGAUnitOfMeasureInfo.AddMessageError(UQRequired);
				}
				else
				{
					IngredientUQ(Parent.US_PGAUnitOfMeasure, Parent.US_PGAUnitOfMeasureInfo);
				}
			}
		}
		void IngredientUQ(string unitOfMeasure, ZPropertyInfo unitOfMeasureInfo)
		{
			var rexAny5Characters = new Regex(@"^[a-z0-9]{1,5}$", RegexOptions.IgnoreCase);
			if (!rexAny5Characters.IsMatch(unitOfMeasure))
			{
				Parent.US_PGAUnitOfMeasureInfo.AddMessageError(UQAllow5Characters);
			}
		}
		internal const string UQAllow5Characters = "Unit Of Measure should be 5 characters.";
		internal const string UQRequired = "Unit Of Measure is mandatory.";
		internal const string UQNotRequired = "Unit Of Measure is not required if no quantity entered.";

		protected virtual bool IsCertifyCargoReleaseValidationMode
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine == null || invoiceLine.IsCargoReleaseValidationMode;
			}
		}

		protected override void CheckUS_OA_ProducerAddress()
		{
			base.CheckUS_OA_ProducerAddress();
			if (ConstituentElement != null)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(ConstituentElement.US_OA_ProducerAddressInfo, ConstituentElement.ProducerAddress);
			}
		}
	}
}
