using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public interface IRateCalculatorGenerator
	{
		void ImportFromValueObject(RateLine rateLine, IValueObject calculatorXSD, IValueObjectImportContext context);

		void ExportToValueObject(RateLine rateLine, IValueObject calculatorXSD, INotifications notifications);

		ZString CalculatorType { get; }

		Type CalculatorSchemaType { get; }
	}
}
