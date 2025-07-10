using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AnimalWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAnimalWrapper()
		{
			NUnit.Framework.Assert.That(animal.AgeMonthNumeric, NUnit.Framework.Is.EqualTo(9).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(animal.AgeYearNumeric, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(animal.FemaleQuantity, NUnit.Framework.Is.EqualTo(1000).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(animal.MaleQuantity, NUnit.Framework.Is.EqualTo(600).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(animal.MicrochipID, NUnit.Framework.Is.EqualTo("9912030001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(animal.Vaccination, NUnit.Framework.Is.EqualTo("2011-08-02").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_AnimalAgeMonth = 9;
			invoiceLine.JI_AnimalAgeYear = 5;
			invoiceLine.JI_AnimalFemaleQty = 1000;
			invoiceLine.JI_AnimalMaleQty = 600;
			invoiceLine.JI_MicrochipID = "9912030001";
			invoiceLine.JI_VaccinationTypeDate = "2011-08-02";
			animal = new AnimalWrapper(invoiceLine);
			base.SetUp();
		}

		IAnimal animal;
	}
}
