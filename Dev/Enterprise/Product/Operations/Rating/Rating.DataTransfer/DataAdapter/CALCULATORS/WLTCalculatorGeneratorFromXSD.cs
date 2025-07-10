using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class WLTCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<WarehouseLocationTypeCalculator, Xsd.WLTCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, WarehouseLocationTypeCalculator calculator, Xsd.WLTCalculator calculatorXSD, IValueObjectImportContext context)
		{
			ImportRateItemWithDescAmtOrRate(calculator, calculatorXSD.RateItems, true, context);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, WarehouseLocationTypeCalculator calculator, Xsd.WLTCalculator calculatorXSD, INotifications notifications)
		{
			ExportRateItemWithDescAmtOrRate(calculator, calculatorXSD.RateItems, true, notifications);
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.WLTCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return WarehouseLocationTypeCalculator.Code; }
		}

		#endregion
	}
}

