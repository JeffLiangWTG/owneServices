using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() => new Customs.Business.EntryCreationStrategy[] { new EntryCreationStrategy(Declaration) };

		protected override Customs.Business.ILineNumberAssigner GetLineNumberAssigner(Customs.Business.CusEntryHeader entryHeader) => new LineNumberAssigner((CusEntryHeader)entryHeader);

		#region Calculate Duties

		protected override void CalculateDuties()
		{
			foreach (CusEntryHeader cusEntryHeader in Declaration.ActiveEntryHeaders)
			{
				if (cusEntryHeader.ShouldCalculateDuty || cusEntryHeader.ShouldCalculateGST)
				{
					foreach (CusEntryLine cusEntryLine in cusEntryHeader.MergedLines)
					{
						ZDecimal dutyAmount = 0m;
						ZDecimal exciseAmount = 0m;
						ZDecimal otherTax = 0m;

						if (cusEntryHeader.ShouldCalculateDuty)
						{
							var tariff = cusEntryLine.Tariff;

							if (tariff != null)
							{
								if (!cusEntryLine.PreferenceRateApplies)
								{
									dutyAmount = CalculateDuty(cusEntryLine);
								}
								exciseAmount = CalculateExcise(cusEntryLine);
								otherTax = CalculateOtherTax(cusEntryLine);
							}
						}

						if (cusEntryHeader.ShouldCalculateGST)
						{
							ZDecimal gST = 0;

							if ((cusEntryHeader.Declaration.SG_DutyExempt && cusEntryHeader.EntrySubType == DeclarationTypeCodeList.Codes.BKT)
								|| cusEntryHeader.EntrySubType == DeclarationTypeCodeList.Codes.GST
								|| cusEntryHeader.EntrySubType == DeclarationTypeCodeList.Codes.GTR
								|| cusEntryHeader.Declaration.PlaceOfReceipt.IsExemptPlaceCodePresident())
							{
								gST = cusEntryLine.CustomsValue.Amount * cusEntryLine.GSTRate;
							}
							else
							{
								gST = (cusEntryLine.CustomsValue.Amount + dutyAmount + exciseAmount + otherTax) * cusEntryLine.GSTRate;
							}

							gST = gST.Round(2);
							cusEntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, gST);
						}
					}
				}
			}
		}

		ZDecimal CalculateDuty(CusEntryLine cusEntryLine)
		{
			ZDecimal dutyAmount = 0m;

			ZDecimal dutyAmountPercentageRateApplied = cusEntryLine.CustomsValue.Amount * (cusEntryLine.DutyPercentageRate / 100);
			ZDecimal dutyAmountPerUnitRateApplied = cusEntryLine.DutyUnitRate * cusEntryLine.DutiableWGTVOLUNIT;

			if (cusEntryLine.IsLiquor && cusEntryLine.PercAlcohol > 0)
			{
				dutyAmountPerUnitRateApplied *= (cusEntryLine.PercAlcohol / 100);
			}

			dutyAmount = dutyAmountPercentageRateApplied + dutyAmountPerUnitRateApplied;
			dutyAmount = dutyAmount.Round(2);
			cusEntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Duty, dutyAmount);

			cusEntryLine.CL_FlatAmount = cusEntryLine.DutyUnitRate;
			if (cusEntryLine.DutyUnitRate > 0m)
			{
				cusEntryLine.CL_FlatAmountUQ = cusEntryLine.DutiableWGTVOLUNITUQ;
			}
			cusEntryLine.CL_DutyPercent = (cusEntryLine.DutyPercentageRate / 100);

			return dutyAmount;
		}

		ZDecimal CalculateExcise(CusEntryLine cusEntryLine)
		{
			ZDecimal exciseAmount = 0m;

			ZDecimal excisePercentageRateApplied = cusEntryLine.CustomsValue.Amount * (cusEntryLine.ExcisePercentageRate / 100);
			ZDecimal exciseAmountPerUnitRateApplied = cusEntryLine.ExciseUnitRate * cusEntryLine.DutiableWGTVOLUNIT;

			if (cusEntryLine.IsTobacco && cusEntryLine.TobaccoMultiplier > 1)
			{
				exciseAmountPerUnitRateApplied *= cusEntryLine.TobaccoMultiplier;
			}

			if (cusEntryLine.IsLiquor && cusEntryLine.PercAlcohol > 0)
			{
				exciseAmountPerUnitRateApplied *= (cusEntryLine.PercAlcohol / 100);
			}

			exciseAmount = excisePercentageRateApplied + exciseAmountPerUnitRateApplied;
			exciseAmount = exciseAmount.Round(2);
			cusEntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, exciseAmount);

			return exciseAmount;
		}

		/// <summary>
		/// For SG Customs future use (TradeNet 4.1 requirement)
		/// </summary>
		ZDecimal CalculateOtherTax(CusEntryLine cusEntryLine)
		{
			var otherPercentageRateApplied = cusEntryLine.CustomsValue.Amount * (cusEntryLine.OtherTaxPercentageRate / 100);
			var otherAmountPerUnitRateApplied = cusEntryLine.OtherTaxUnitRate * cusEntryLine.DutiableWGTVOLUNIT;

			if (cusEntryLine.IsLiquor && cusEntryLine.PercAlcohol > 0)
			{
				otherAmountPerUnitRateApplied *= (cusEntryLine.PercAlcohol / 100);
			}

			ZDecimal otherAmount = otherPercentageRateApplied + otherAmountPerUnitRateApplied;
			otherAmount = otherAmount.Round(2);

			cusEntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.OtherTax, otherAmount);

			return otherAmount;
		}

		#endregion
	}
}
