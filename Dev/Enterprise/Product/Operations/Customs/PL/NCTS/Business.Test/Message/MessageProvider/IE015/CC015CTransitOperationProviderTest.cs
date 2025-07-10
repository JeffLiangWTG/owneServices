using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC015CTransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC015CTransitOperationProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC015CTransitOperationProvider(null));
		});
	}

	public void TestLRN()
	{
		AssertEquals("LRN is replaced with PlaceHolder", EDIMessage.PL_NCTS_LRN_PlaceHolder, GetProvider().LRN);
	}

	public void TestDeclarationType()
	{
		AssertDeclarationType(NctsPhase5DeclarationTypeList.Codes.T);
		AssertDeclarationType(NctsPhase5DeclarationTypeList.Codes.T1);
		AssertDeclarationType(NctsPhase5DeclarationTypeList.Codes.T2);
		AssertDeclarationType(NctsPhase5DeclarationTypeList.Codes.T2F);
		AssertDeclarationType(NctsPhase5DeclarationTypeList.Codes.T2SM);
		AssertDeclarationType(NctsPhase5DeclarationTypeList.Codes.TIR);
	}

	void AssertDeclarationType(string declarationTypeCode)
	{
		movementHeader.BM_InBondEntryType = declarationTypeCode;
		AssertEquals("Type is " + declarationTypeCode, declarationTypeCode, GetProvider().DeclarationType);
	}

	public void TestAdditionalDeclarationType()
	{
		AssertAdditionalDeclarationType("A");
		AssertAdditionalDeclarationType("D");
	}

	void AssertAdditionalDeclarationType(string additionalDeclarationTypeCode)
	{
		movementHeader.BM_AdditionalDeclarationType = additionalDeclarationTypeCode;
		AssertEquals("Type is " + additionalDeclarationTypeCode, additionalDeclarationTypeCode, GetProvider().AdditionalDeclarationType);
	}

	public void TestTIRCarnetNumber()
	{
		var tirCarnetNumber = "123456789012";
		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
		movementHeader.TirCarnetNumber = tirCarnetNumber;
		AssertEquals("TirCarnetNumber is " + tirCarnetNumber, tirCarnetNumber, GetProvider().TIRCarnetNumber);
	}

	public void TestSecurity()
	{
		AssertSecurity(NctsTypeOfSecurityList.Codes.NON, "0");
		AssertSecurity(NctsTypeOfSecurityList.Codes.ENT, "1");
		AssertSecurity(NctsTypeOfSecurityList.Codes.EXI, "2");
		AssertSecurity(NctsTypeOfSecurityList.Codes.BTH, "3");
	}

	void AssertSecurity(string securityCode, string expectedValue)
	{
		movementHeader.BM_TypeOfSecurity = securityCode;
		AssertEquals("BM_TypeOfSecurity is " + securityCode, expectedValue, GetProvider().Security);
	}

	public void TestReducedDatasetIndicator()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_ReducedDatasetIndicator = true;
			AssertEquals("true", NCTSIndicator.YES, GetProvider().ReducedDatasetIndicator);

			movementHeader.BM_ReducedDatasetIndicator = false;
			AssertEquals("false", NCTSIndicator.NO, GetProvider().ReducedDatasetIndicator);
		});
	}

	public void TestSpecificCircumstanceIndicator()
	{
		CombineAssertions(() =>
		{
			AssertEquals(GetMessage(), string.Empty, GetProvider().SpecificCircumstanceIndicator);

			movementHeader.BM_SpecificCircumstance = "ABC";
			AssertEquals(GetMessage(), "ABC", GetProvider().SpecificCircumstanceIndicator);
		});

		string GetMessage() => $"{nameof(movementHeader.BM_SpecificCircumstance)} = {movementHeader.BM_SpecificCircumstance}";
	}

	public void TestCommunicationLanguageAtDeparture()
	{
		AssertNull(Provider.CommunicationLanguageAtDeparture);
	}

	public void TestBindingItinerary()
	{
		CombineAssertions(() =>
		{
			var cor = nctsHeader.CountriesOfRouting.AddNew();
			cor.CY_Data = CountryCodes.Poland;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("BindingItinerary should always be 0 (NON)", NCTSIndicator.NO, GetProvider().BindingItinerary);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals("BindingItinerary should always be 0 (ENT)", NCTSIndicator.NO, GetProvider().BindingItinerary);

			nctsHeader.CountriesOfRouting.RemoveAll();
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("BindingItinerary should always be 0 (NON)", NCTSIndicator.NO, GetProvider().BindingItinerary);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals("BindingItinerary should always be 0 (ENT)", NCTSIndicator.NO, GetProvider().BindingItinerary);
		});
	}

	[TestDate(2023, 6, 28)]
	public void TestPresentationOfTheGoodsDateAndTime()
	{
		CombineAssertions(() =>
		{
			AssertNull("Presentation Date and Time not set", Provider.PresentationOfTheGoodsDateAndTime);
			movementHeader.BM_PresentationDateTime = ZDateTime.Today.ToOffset();
			AssertEquals("Presentation Date and Time is set", new DateTime(2023, 6, 28), Provider.PresentationOfTheGoodsDateAndTime);
		});
	}

	public void TestLimitDate()
	{
		CombineAssertions(() =>
		{
			movementHeader.IsSimplifiedNctsProcedure = false;
			movementHeader.BM_ExportDate = ZDateTime.Empty;
			AssertNull("Should be null when BM_ExportDate is empty", GetProvider().LimitDate);

			var testDateTime = DateTime.Now.AddDays(3);
			movementHeader.BM_ExportDate = testDateTime;
			AssertNull("Should be null when is not simplified procedure", GetProvider().LimitDate);

			movementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals("The value should be equal to BM_ExportDate and is simplified procedure", testDateTime, GetProvider().LimitDate);
		});
	}

	protected override CC015CTransitOperationProvider GetProvider() => new CC015CTransitOperationProvider(movementHeader);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		movementHeader = nctsHeader.MovementHeader;
		nctsHeader.MovementHeader.CustomsOffices.RemoveAll();
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
}
