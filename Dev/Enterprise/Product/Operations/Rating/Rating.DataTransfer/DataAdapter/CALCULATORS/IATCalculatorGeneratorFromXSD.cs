using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class IATCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<PackageCountCalculator, Xsd.IATCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, PackageCountCalculator calculator, Xsd.IATCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.BaseChargeSpecified)
			{
				calculator.BaseRate = calculatorXSD.BaseCharge;
			}
			if (calculatorXSD.RatePerKGSpecified)
			{
				calculator.PerKG = calculatorXSD.RatePerKG;
			}
			if (calculatorXSD.FirstPackageSpecified)
			{
				calculator.FirstPackageRate = calculatorXSD.FirstPackage;
			}
			if (calculatorXSD.AdditionalPackageSpecified)
			{
				calculator.AddtionalPackageRate = calculatorXSD.AdditionalPackage;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, PackageCountCalculator calculator, Xsd.IATCalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.BaseRate.IsEmpty)
			{
				calculatorXSD.BaseCharge = calculator.BaseRate;
			}
			if (!calculator.PerKG.IsEmpty)
			{
				calculatorXSD.RatePerKG = calculator.PerKG;
			}
			if (!calculator.FirstPackageRate.IsEmpty)
			{
				calculatorXSD.FirstPackage = calculator.FirstPackageRate;
			}
			if (!calculator.AddtionalPackageRate.IsEmpty)
			{
				calculatorXSD.AdditionalPackage = calculator.AddtionalPackageRate;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.IATCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return PackageCountCalculator.Code; }
		}

		#endregion
	}
}

