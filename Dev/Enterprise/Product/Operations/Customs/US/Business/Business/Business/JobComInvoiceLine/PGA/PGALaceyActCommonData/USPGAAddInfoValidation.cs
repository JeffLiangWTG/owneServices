using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class USPGAAddInfoValidation : AutoUSPGAAddInfoValidation
	{
		public USPGAAddInfoValidation(AutoUSPGAAddInfo parent) : base(parent)
		{
		}

		protected new USPGAAddInfo Parent
		{
			get { return (USPGAAddInfo)base.Parent; }
		}

		PGA PGA
		{
			get { return Parent.Parent; }
		}

		bool IsPgaRequiredValidation
		{
			get
			{
				var invoiceLine = PGA != null ? PGA.InvoiceLine : null;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		public override void ValidateAll()
		{
			PGA.ClearRowNotifications();
			base.ValidateAll();
			ValidateThatThereAreConsitituentElements();
			ValidateThatThereAreContainers();
		}

		void ValidateThatThereAreConsitituentElements()
		{
			if (PGA != null && PGA.PG04ConstituentElements.Count == 0)
			{
				PGA.AddRowMessageError(AtLeastOneConstElementShouldExist);
			}
		}
		internal const string AtLeastOneConstElementShouldExist = "There needs to be at least one Constituent Element for every Lacey Act line.";

		void ValidateThatThereAreContainers()
		{
			if (PGA != null && PGA.InvoiceLine != null && PGA.InvoiceLine.Declaration != null)
			{
				bool isSeaContDeclaration = PGA.InvoiceLine.Declaration.JE_Calc_USTransportMode == TransportModeCodes.Codes.VesselContainer;
				if (isSeaContDeclaration && PGA.ContainersCount == 0)
				{
					PGA.AddRowWarning(NoContainers);
				}
			}
		}
		internal const string NoContainers = "There are containers on this Declaration. If possible, you should specify the container(s) for the line.";

		protected override void CheckUS_PGACommercialDescription()
		{
			base.CheckUS_PGACommercialDescription();

			if (Parent.US_PGACommercialDescription.IsEmpty && IsPgaRequiredValidation)
			{
				Parent.US_PGACommercialDescriptionInfo.AddMessageError(CommercialDescriptionMandatory);
			}

			ValidateThatThereAreConsitituentElements();
		}
		internal const string CommercialDescriptionMandatory = "Lacey Commercial Description is mandatory.";

		protected override void CheckUS_PGALineValue()
		{
			base.CheckUS_PGALineValue();

			if (PGA != null)
			{
				JobComInvoiceLine invoiceLine = PGA.InvoiceLine;

				if (invoiceLine != null && invoiceLine.IsOGAValueUpToDate)
				{
					var roundedCV = invoiceLine.GetEnteredValueForOGA();

					if (Parent.US_PGALineValue > roundedCV)
					{
						Parent.US_PGALineValueInfo.AddMessageError(PGAValueShouldBeLessThanCustomsValue + roundedCV + ".");
					}
					else if (invoiceLine.LaceyActLines.TotalPGAValue > roundedCV)
					{
						string message = string.Format("The total of all PGA Values ({0}) should be less than or equal to Customs Value ({1}).", invoiceLine.LaceyActLines.TotalPGAValue, roundedCV);
						Parent.US_PGALineValueInfo.AddMessageError(message);
					}
				}
			}

			ValidateThatThereAreConsitituentElements();
		}

		protected override void CheckUS_InvCurrPGAValue()
		{
			base.CheckUS_InvCurrPGAValue();

			if (PGA != null)
			{
				var invoiceLine = PGA.InvoiceLine;

				if (invoiceLine != null)
				{
					if (invoiceLine.JI_LinePrice > 0m && Parent.US_InvCurrPGAValue == 0m)
					{
						Parent.US_InvCurrPGAValueInfo.AddMessageError(ValidationConstants.FDA.OGAInvValue);
					}
					else
					{
						ZDecimal totalValue = invoiceLine.LaceyActLines.TotalInvCurrPGAValue;

						if (totalValue > invoiceLine.JI_LinePrice)
						{
							Parent.US_InvCurrPGAValueInfo.AddMessageError(string.Format(TotalInvCurrValueGreaterThanLinePrice, totalValue));
						}
					}
				}
			}
		}

		internal const string TotalInvCurrValueGreaterThanLinePrice = "The total of Inv. Curr. Value ({0}) should be less than the line price.";
		internal const string PGAValueShouldBeLessThanCustomsValue = "PGA Value should be less than or equal to Customs Value, $";

		protected override void CheckUS_UnknownBreakdown()
		{
			base.CheckUS_UnknownBreakdown();

			if (Parent.US_UnknownBreakdown && Parent.US_UnknownBreakdownTotal)
			{
				Parent.US_UnknownBreakdownInfo.AddMessageError(OnlyOneBreakdownRequried);
			}

			ValidateUS_UnknownBreakdownTotal();
		}

		protected override void CheckUS_UnknownBreakdownTotal()
		{
			base.CheckUS_UnknownBreakdownTotal();

			if (Parent.US_UnknownBreakdownTotal)
			{
				if (Parent.US_UnknownBreakdown)
				{
					Parent.US_UnknownBreakdownTotalInfo.AddMessageError(OnlyOneBreakdownRequried);
				}

				if (Parent.US_UnknownBreakdownTotal)
				{
					if (PGA != null && PGA.LaceyCountries.Count == 0)
					{
						Parent.US_UnknownBreakdownTotalInfo.AddMessageError(AtLeastOneCountryShouldExist);
					}
				}
			}

			ValidateUS_UnknownBreakdown();
			ValidateUS_NameOfConstituentElement();
			ValidateUS_QuantityOfConstituentElement();
			ValidateUS_UnitOfMeasure();
		}
		internal const string OnlyOneBreakdownRequried = "Either Unknown With Qty/Country Breadkdown or Unknown Breakdown with Total Qty/Countries is required, not both.";
		internal const string AtLeastOneCountryShouldExist = "There needs to be at least one Country for every Lacey Act line since Unknown Breakdown with Total Qty/Countries is ticked.";

		protected override void CheckUS_NameOfConstituentElement()
		{
			base.CheckUS_NameOfConstituentElement();

			if (Parent.US_UnknownBreakdownTotal && IsPgaRequiredValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NameOfConstituentElementInfo);
			}
		}

		protected override void CheckUS_QuantityOfConstituentElement()
		{
			base.CheckUS_QuantityOfConstituentElement();

			if (Parent.US_UnknownBreakdownTotal && IsPgaRequiredValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_QuantityOfConstituentElementInfo);
			}
		}

		protected override void CheckUS_UnitOfMeasure()
		{
			base.CheckUS_UnitOfMeasure();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UnitOfMeasureInfo, Parent.Lookups.UnitOfMeasureList);
			if (Parent.US_UnknownBreakdownTotal && IsPgaRequiredValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UnitOfMeasureInfo);
			}
		}

		protected override void CheckUS_CertifyingIndividual()
		{
			base.CheckUS_CertifyingIndividual();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CertifyingIndividualInfo, Parent.Lookups.CertifyingIndividualList);

			if (IsPgaRequiredValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CertifyingIndividualInfo);
			}

			ValidateUS_PGAContactName();
			ValidateUS_PGAContactPhoneNo();
			ValidateUS_PGAContactEmail();
		}

		protected override void CheckUS_PGAContactName()
		{
			base.CheckUS_PGAContactName();

			if (IsPgaRequiredValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PGAContactNameInfo);
			}
		}

		protected override void CheckUS_PGAContactPhoneNo()
		{
			base.CheckUS_PGAContactPhoneNo();

			if (IsPgaRequiredValidation)
			{
				if (Parent.US_PGAContactPhoneNo.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PGAContactPhoneNoInfo);
				}
			}
		}

		protected override void CheckUS_PGAContactEmail()
		{
			base.CheckUS_PGAContactEmail();

			if (IsPgaRequiredValidation)
			{
				if (!Parent.US_PGAContactEmail.IsEmpty)
				{
					if (!EmailAddressValidation.IsEmailAddressValid(Parent.US_PGAContactEmail))
					{
						Parent.US_PGAContactEmailInfo.AddWarning("Invalid email format");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PGAContactEmailInfo);
				}
			}
		}
	}
}
