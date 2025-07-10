using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class DINCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<DisbursementInterestCalculator, Xsd.DINCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, DisbursementInterestCalculator calculator, Xsd.DINCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.UpliftSpecified)
			{
				calculator.Uplift = calculatorXSD.Uplift;
			}
			if (calculatorXSD.ApplyToOutstandingDaysOnlySpecified)
			{
				calculator.OutstandingDays = (calculatorXSD.ApplyToOutstandingDaysOnly == Xsd.TrueFalse.@true);
			}
			if (calculatorXSD.AdjustmentDaysSpecified)
			{
				calculator.AdjustmentDays = calculatorXSD.AdjustmentDays;
			}

			if (calculatorXSD.RateItems.Count > 0)
			{
				foreach (Xsd.DINRateItem rateItem in calculatorXSD.RateItems)
				{
					ZString type = rateItem.Type;

					RateLineItem item = calculator.RateLineItems.AddNew();
					item.TM_Text = type;

					if (type == CalculatorConstants.Text.ChargeCode && !rateItem.Chrg.IsEmpty)
					{
						AccChargeCode charge = RateImportHelper.Instance.FindChargeCode(rateItem.Chrg, context);

						if (charge != null)
						{
							item.TM_AC = charge.PK;
						}
						else
						{
							context.Notify(new ErrorNotification(RateErrorType.InvalidChargeCode, rateItem.Chrg));
						}
					}
				}
			}
			else if (!calculatorXSD.ApplyTo.IsEmpty)
			{
				calculator.RateLineItems.AddNew().TM_Text = calculatorXSD.ApplyTo;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, DisbursementInterestCalculator calculator, Xsd.DINCalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.Uplift.IsEmpty)
			{
				calculatorXSD.Uplift = calculator.Uplift;
			}

			calculatorXSD.ApplyToOutstandingDaysOnly = calculator.OutstandingDays ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;

			if (!calculator.AdjustmentDays.IsEmpty)
			{
				calculatorXSD.AdjustmentDays = calculator.AdjustmentDays;
			}

			foreach (RateLineItem item in calculator.RateLineItems)
			{
				Xsd.DINRateItem rateItem = calculatorXSD.RateItems.AddNew();

				rateItem.Type = item.TM_Text;

				if (item.TM_Text == CalculatorConstants.Text.ChargeCode && item.ChargeCode != null)
				{
					rateItem.Chrg = item.ChargeCode.AC_Code;
				}
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.DINCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return DisbursementInterestCalculator.Code; }
		}

		#endregion
	}
}

