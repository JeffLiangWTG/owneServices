using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class WPKCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<WarehousePackCalculator, Xsd.WPKCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, WarehousePackCalculator calculator, Xsd.WPKCalculator calculatorXSD, IValueObjectImportContext context)
		{
			ImportRateItemWithDescAmtOrRate(calculator, calculatorXSD.RateItems, true, context);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, WarehousePackCalculator calculator, Xsd.WPKCalculator calculatorXSD, INotifications notifications)
		{
			ExportRateItemWithDescAmtOrRate(calculator, calculatorXSD.RateItems, true, notifications);
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.WPKCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return WarehousePackCalculator.Code; }
		}

		#endregion
	}
}

