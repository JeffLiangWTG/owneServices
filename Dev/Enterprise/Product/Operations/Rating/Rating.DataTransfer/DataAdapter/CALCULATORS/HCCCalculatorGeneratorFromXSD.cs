using System.Linq;
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
	public class HCCCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<HighestChargeCalculator, Xsd.HCCCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, HighestChargeCalculator calculator, Xsd.HCCCalculator calculatorXSD, IValueObjectImportContext context)
		{
			foreach (Xsd.HCCRateItem rateItem in calculatorXSD.RateItems)
			{
				RateLineItem item = calculator.RateLineItems.AddNew();
				item.TM_Type = CalculatorConstants.Type.ApplyTo;
				item.TM_Text = CalculatorConstants.Text.ChargeCode;

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

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, HighestChargeCalculator calculator, Xsd.HCCCalculator calculatorXSD, INotifications notifications)
		{
			foreach (var item in calculator.RateLineItems.Cast<RateLineItem>())
			{
				Xsd.HCCRateItem rateItem = calculatorXSD.RateItems.AddNew();
				rateItem.Chrg = item.ChargeCode.AC_Code;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.HCCCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return HighestChargeCalculator.Code; }
		}

		#endregion
	}
}

