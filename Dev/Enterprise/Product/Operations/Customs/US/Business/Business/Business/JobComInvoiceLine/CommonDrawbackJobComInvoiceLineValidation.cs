using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CommonDrawbackJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public CommonDrawbackJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			if (Parent.US_DRWIsForImportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_TariffInfo);
			}
		}

		protected override void CheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			if (Parent.US_DRWIsForImportSection)
			{
				if (Parent.JI_Tariff.Length >= 8)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.JI_TariffInfo, InvoiceLine.Lookups.ImportTariffs);
				}
				if (Parent.Declaration.US_PetroleumClaimInd && Parent.JI_Tariff.Length < 8)
				{
					Parent.JI_TariffInfo.AddMessageError(PetroliumTariffValidation);
				}
			}
		}
		internal const string PetroliumTariffValidation = "If Petrolioum Claim Indicator is checked on the header then Tariff Number must be at the 8 or 10 digit level";

		#region Validation not required

		protected override void CheckJI_HazMatCode()
		{
		}

		protected override void CheckJI_HazMatCodeQualifier()
		{
		}

		protected override void CheckFreightInLocalCurrency()
		{
		}

		protected override void CheckJI_Weight()
		{
		}

		protected override void CheckJI_WeightUQ()
		{
		}

		protected override void CheckJI_NetWeight()
		{
		}

		protected override void CheckJI_NetWeightUQ()
		{
		}

		protected override void CheckJI_Volume()
		{
		}

		protected override void CheckJI_VolumeUQ()
		{
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
		#endregion
	}
}
