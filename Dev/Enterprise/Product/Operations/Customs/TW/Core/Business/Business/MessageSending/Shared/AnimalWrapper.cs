using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class AnimalWrapper : IAnimal
	{
		public AnimalWrapper(JobComInvoiceLine invoiceLine)
		{
			Argument.NotNull(invoiceLine, nameof(invoiceLine));
			ageMonthNumeric = invoiceLine.JI_AnimalAgeMonth;
			ageYearNumeric = invoiceLine.JI_AnimalAgeYear;
			femaleQuantity = invoiceLine.JI_AnimalFemaleQty;
			maleQuantity = invoiceLine.JI_AnimalMaleQty;
			microchipID = invoiceLine.JI_MicrochipID;
			vaccination = invoiceLine.JI_VaccinationTypeDate;
		}

		readonly ZInt ageMonthNumeric;

		readonly ZInt ageYearNumeric;

		readonly ZInt femaleQuantity;

		readonly ZInt maleQuantity;

		readonly ZString microchipID;

		readonly ZString vaccination;

		ZInt IAnimal.AgeMonthNumeric => ageMonthNumeric;

		ZInt IAnimal.AgeYearNumeric => ageYearNumeric;

		ZInt IAnimal.FemaleQuantity => femaleQuantity;

		ZInt IAnimal.MaleQuantity => maleQuantity;

		ZString IAnimal.MicrochipID => microchipID;

		ZString IAnimal.Vaccination => vaccination;
	}
}
