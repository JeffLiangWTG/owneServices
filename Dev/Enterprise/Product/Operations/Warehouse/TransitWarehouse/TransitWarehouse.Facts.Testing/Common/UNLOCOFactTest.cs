using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transit.Facts.Testing
{
	public class UNLOCOFactTest : TestCaseWithFactory
	{
		public void TestNullUNLOCO_ThrowsException()
		{
			var countryFactMock = new Mock<ICountryFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new UNLOCOFact(null, countryFactMock.Object));
		}

		public void TestNullCountryFact_ThrowsException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new UNLOCOFact(validUNLOCO, null));
		}

		public void TestNullUNLOCO_Country_ThrowsException()
		{
			var refUNLOCO = Factory.New<RefUNLOCO>();
			var countryFactMock = new Mock<ICountryFact>();
			Assert("Precondition", refUNLOCO.Country == null);
			AssertExceptionThrown<ArgumentNullException>(() => new UNLOCOFact(refUNLOCO, countryFactMock.Object));
		}

		public void TestPK()
		{
			var countryFactMock = new Mock<ICountryFact>();
			var unlocoFact = new UNLOCOFact(validUNLOCO, countryFactMock.Object);

			AssertEquals(validUNLOCO.PK, unlocoFact.PK);
		}

		public void TestUNLOCO()
		{
			var unloco = "AUSYD";
			validUNLOCO.RL_Code = unloco;
			var countryFactMock = new Mock<ICountryFact>();
			var unlocoFact = new UNLOCOFact(validUNLOCO, countryFactMock.Object);

			AssertEquals(validUNLOCO.RL_Code, unlocoFact.UNLOCO);
			AssertEquals(unloco, unlocoFact.UNLOCO);
		}

		public void TestCountryFact_NotNull()
		{
			var countryFactMock = new Mock<ICountryFact>();
			var unlocoFact = new UNLOCOFact(validUNLOCO, countryFactMock.Object);

			AssertNotNull(unlocoFact.Country);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var pk = Guid.NewGuid();
			var refCountry = Factory.New<RefCountry>();
			refCountry.Code = "AU";

			validUNLOCO = Factory.NewWithPrimaryKey<RefUNLOCO>(pk);
			validUNLOCO.RL_RN_NKCountryCode = refCountry.Code;
		}

		RefUNLOCO validUNLOCO;
	}
}
