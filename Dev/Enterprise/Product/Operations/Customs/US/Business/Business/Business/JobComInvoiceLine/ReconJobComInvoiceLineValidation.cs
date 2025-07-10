namespace Enterprise.Customs.US.Business
{
	public class ReconJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ReconJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateMaximumNumberOfEntryLines();
		}

		public void ValidateMaximumNumberOfEntryLines()
		{
			var lineCount = Parent.Declaration.InvoiceLines.Count;

			if (lineCount > 9999)
			{
				var firstLine = Parent.Declaration.InvoiceLines[0];
				if (Parent.PK == firstLine.PK)
				{
					firstLine.AddRowMessageError(MaximumNumberOfEntryLinesExceeded);
				}
			}
		}
		public const string MaximumNumberOfEntryLinesExceeded = "Total number of entry lines cannot exceed 9,999.";

		protected override void CheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			TariffValidator.Validate(Parent, Parent.ImportTariff, Parent.JI_TariffInfo);
			TariffValidator.Ensure98_99IsNotEntered(Parent.JI_TariffInfo, Parent.ImportTariff);
			TariffValidator.ValidateBasedOnAdditionalTariffNumberIndicator(Parent, Parent.ImportTariff, Parent.JI_TariffInfo, Parent.HasSecondaryTariffLines);

			USCTariff tariff = Parent.ParentTariffLine != null ? Parent.ParentTariffLine.ImportTariff : Parent.ImportTariff;
			TariffValidator.CheckSTNRule(Parent, tariff, Parent.JI_TariffInfo, false);
		}

		protected override bool IsTariffMandatory
		{
			get { return !Parent.IsCombinedLine() && base.IsTariffMandatory; }
		}

		protected override void CheckJI_ParentID()
		{
			base.CheckJI_ParentID();
			if (Parent.ParentTariffLine != null)
			{
				JobComInvoiceLineValidation validationObject = Parent.ParentTariffLine.Validation;

				validationObject.ValidateJI_Tariff();
				validationObject.ValidateJI_LinePrice();
			}
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			if (Parent.ImportTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.JI_CustomsQuantityInfo, Parent.JI_CustomsUnitQty,
					() => Parent.ImportTariff.RequiresFirstQuantity(), QuantityCode.FirstQuantity);
			}

			if (Parent.JI_CustomsQuantity != Parent.US_R_OrigFirstQty)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(ValidationConstants.ShouldTheSameAsOriginalCustomsQuantity);
			}
		}

		protected override void CheckJI_Description()
		{
			//not required
		}

		protected override void CheckJI_HazMatCodeQualifier()
		{
			//not required
		}

		protected override void CheckFreightInLocalCurrency()
		{
			//not required
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			//not required
		}

		protected override void CheckJI_InvoiceUQ()
		{
			//not required
		}

		protected override void CheckJI_Weight()
		{
			//not required
		}

		protected override void CheckJI_NetWeight()
		{
			//not required
		}

		protected override void CheckJI_WeightUQ()
		{
			//not required
		}

		protected override void CheckJI_NetWeightUQ()
		{
			//not required
		}

		protected override void CheckJI_HazMatCode()
		{
			//not required
		}

		protected override void CheckJI_Volume()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib1()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib2()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib3()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib4()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib5()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib6()
		{
			//not required
		}

		protected override void CheckJI_CustomTextBlob1()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal1()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal2()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal3()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal4()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal5()
		{
			//not required
		}

		protected override void CheckJI_CustomDate1()
		{
			//not required
		}

		protected override void CheckJI_CustomDate2()
		{
			//not required
		}

		protected override void CheckJI_CustomDate3()
		{
			//not required
		}

		protected override void CheckJI_CustomDate4()
		{
			//not required
		}

		protected override void CheckJI_CustomDate5()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag1()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag2()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag3()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag4()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag5()
		{
			//not required
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (Parent.ImportTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.JI_CustomsSecondQuantityInfo, Parent.JI_CustomsSecondUnitQty,
					() => Parent.ImportTariff.RequiresSecondQuantity(), QuantityCode.SecondQuantity);
			}
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();
			if (Parent.ImportTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.JI_CustomsThirdQuantityInfo, Parent.JI_CustomsThirdUnitQty,
					() => Parent.ImportTariff.RequiresThirdQuantity(), QuantityCode.ThirdQuantity);
			}
		}
	}
}
