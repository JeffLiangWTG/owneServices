using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.ForwardAir
{
	internal class ForwardAirRatesFlatFileConverter : FlatFileConverter
	{
		public ForwardAirRatesFlatFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void ImportFlatFileCore(IValueObject valueObject, IFlatFileFormat fileFormat, System.IO.TextReader flatFileReader)
		{
			flatFileReader.ReadLine();

			base.ImportFlatFileCore(valueObject, fileFormat, flatFileReader);
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.Rate rate = (Xsd.Rate)valueObject;
			rate.RateType = RatingConstants.RatingHeaderTypes.Costing;

			Xsd.RateEntry entry = null;
			Xsd.RateLine line = null;
			foreach (ForwardAirRatesFlatFileDataRow dataRow in fileLines)
			{
				entry = NewEntry(rate, dataRow);
				line = NewFrtLine(entry, dataRow);
				NewCalculator(line, dataRow);
			}
		}

		#region Implementation

		void NewCalculator(Xsd.RateLine line, ForwardAirRatesFlatFileDataRow dataRow)
		{
			Xsd.CMBCalculator calculator = new Xsd.CMBCalculator();

			calculator.SimpleRate.Minimum = dataRow.Minimum;

			Xsd.RateItemWithOperatorAndBreak rateItem = calculator.RateItems.AddNew();
			rateItem.Operator = new ZString("-");
			rateItem.BreakAmount = 100m;
			rateItem.PerUnit = dataRow.W100;

			rateItem = calculator.RateItems.AddNew();
			rateItem.Operator = new ZString("+");
			rateItem.BreakAmount = 100m;
			rateItem.PerUnit = dataRow.W100;

			rateItem = calculator.RateItems.AddNew();
			rateItem.Operator = new ZString("+");
			rateItem.BreakAmount = 500m;
			rateItem.PerUnit = dataRow.W500;

			rateItem = calculator.RateItems.AddNew();
			rateItem.Operator = new ZString("+");
			rateItem.BreakAmount = 1000m;
			rateItem.PerUnit = dataRow.W1000;

			rateItem = calculator.RateItems.AddNew();
			rateItem.Operator = new ZString("+");
			rateItem.BreakAmount = 3000m;
			rateItem.PerUnit = dataRow.W3000;

			rateItem = calculator.RateItems.AddNew();
			rateItem.Operator = new ZString("+");
			rateItem.BreakAmount = 5000m;
			rateItem.PerUnit = dataRow.W5000;

			rateItem = calculator.RateItems.AddNew();
			rateItem.Operator = new ZString("+");
			rateItem.BreakAmount = 7500m;
			rateItem.PerUnit = dataRow.W7500;

			line.RateCalculator.Item = calculator;
		}

		Xsd.RateEntry NewEntry(Xsd.Rate rate, ForwardAirRatesFlatFileDataRow dataRow)
		{
			Xsd.RateEntry entry = rate.RateEntries.AddNew();
			entry.Category = Core.Constants.RateMode.LCL;
			entry.Mode = Core.Constants.RateMode.LRO;
			entry.Origin = dataRow.Origin;
			entry.Destination = dataRow.Destination;
			entry.TransitTime = dataRow.Days.ToString();
			entry.StartDate = ZDate.Today;
			entry.Units = Core.Constants.Weight.Pounds;

			return entry;
		}

		Xsd.RateLine NewFrtLine(Xsd.RateEntry entry, ForwardAirRatesFlatFileDataRow dataRow)
		{
			Xsd.RateLine line = entry.RateLines.AddNew();
			line.ChargeCode = FrtCode;
			line.Currency = "USD";
			line.Units = Core.Constants.Weight.Pounds;
			line.Rounding = "DEF";
			line.UnitsMultiple = 100m;

			return line;
		}

		ZString FrtCode
		{
			get
			{
				if (fFrtCode.IsEmpty)
				{
					var frt = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
					fFrtCode = frt != null ? frt.AC_Code : (ZString)"FRT";
				}

				return fFrtCode;
			}
		}

		ZString fFrtCode;

		#endregion
	}
}
