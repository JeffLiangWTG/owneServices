using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(TransitOperationProvider))]
public abstract class TransitOperationProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : TransitOperationProvider
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>("Null nctsHeader", () => new TransitOperationProvider(null));

	public virtual void TestLRN()
	{
		header.MovementHeader.BM_PaperlessInbondNum = "LRNTEST";
		AssertEquals("LRNTEST", provider.LRN);
	}

	public void TestDeclarationType()
	{
		header.MovementHeader.BM_InBondEntryType = "TIR";
		AssertEquals("TIR", provider.DeclarationType);
	}

	public void TestTIRCarnetNumber() => CombineAssertions(() =>
	{
		header.MovementHeader.BM_InBondEntryType = "TIR";
		header.MovementHeader.TirCarnetNumber = "TirC";
		AssertEquals("TirC", provider.TIRCarnetNumber);

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalProvider = (T)Activator.CreateInstance(typeof(T), arrivalHeader);
		AssertNullOrEmpty(arrivalProvider.TIRCarnetNumber);
	});

	public void TestSecurity() => CombineAssertions(() =>
	{
		header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertEquals(GetMessage(), 0, provider.Security);

		header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		AssertEquals(GetMessage(), 1, provider.Security);

		header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
		AssertEquals(GetMessage(), 2, provider.Security);

		header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		AssertEquals(GetMessage(), 3, provider.Security);

		header.MovementHeader.BM_TypeOfSecurity = "123";
		AssertEquals(GetMessage(), null, provider.Security);

		string GetMessage() => $"{nameof(header.MovementHeader.BM_TypeOfSecurity)} = {header.MovementHeader.BM_TypeOfSecurity}";
	});

	public void TestAdditionalDeclarationType()
	{
		const string additionalDeclarationTypeValue = "4";
		header.MovementHeader.BM_AdditionalDeclarationType = additionalDeclarationTypeValue;

		AssertEquals(additionalDeclarationTypeValue, provider.AdditionalDeclarationType);
	}

	public void TestReducedDatasetIndicator() => CombineAssertions(() =>
	{
		header.MovementHeader.BM_ReducedDatasetIndicator = false;
		AssertEquals(false, provider.ReducedDatasetIndicator);

		header.MovementHeader.BM_ReducedDatasetIndicator = true;
		AssertEquals(true, provider.ReducedDatasetIndicator);
	});

	public void TestSpecificCircumstanceIndicator()
	{
		const string specificCircumstanceValue = "456";
		header.MovementHeader.BM_SpecificCircumstance = specificCircumstanceValue;

		AssertEquals(specificCircumstanceValue, provider.SpecificCircumstanceIndicator);
	}

	public void TestCommunicationLanguageAtDeparture()
	{
		header.BH_CommunicationLanguage = "EN";
		AssertEquals("en", provider.CommunicationLanguageAtDeparture);
	}

	public virtual void TestBindingItinerary()
	{
		header.CountriesOfRouting.AddNew();
		AssertEquals(true, provider.BindingItinerary);
	}

	public void TestLimitDate() => CombineAssertions(() =>
	{
		header.MovementHeader.IsSimplifiedNctsProcedure = true;

		header.MovementHeader.BM_ExportDate = DateTime.FromOADate(1234);
		AssertEquals(System.DateTime.FromOADate(1234), provider.LimitDate);

		header.MovementHeader.BM_ExportDate = ZDateTime.Empty;
		AssertNull("LimitDate should be null from Empty", provider.LimitDate);

		header.MovementHeader.BM_ExportDate = new ZDateTime(DateTime.MinValue);
		AssertNull("LimitDate should be null from MinValue", provider.LimitDate);
	});

	public void TestMRN()
	{
		header.ArrivalMrnFromUser = "MRN";
		AssertEquals("MRN", provider.MRN);
	}

	public void TestPresentationDateAndTime() => CombineAssertions(() =>
	{
		header.MovementHeader.BM_ArrivalDate = new DateTime(2022, 04, 01, 12, 34, 00);
		AssertEquals(new DateTime(2022, 04, 01, 12, 34, 00), provider.PresentationDateAndTime);

		header.MovementHeader.BM_ArrivalDate = ZDateTime.Empty;
		AssertNull("PresentationDateAndTime should be null from Empty", provider.PresentationDateAndTime);

		header.MovementHeader.BM_ArrivalDate = new ZDateTime(DateTime.MinValue);
		AssertNull("PresentationDateAndTime should be null from MinValue", provider.PresentationDateAndTime);
	});

	[TestDate(2024, 08, 06, 11, 06, 13)]
	public void TestArrivalNotificationDateAndTime() => CombineAssertions(() =>
	{
		AssertEquals(new DateTime(2024, 08, 06, 11, 06, 13), provider.ArrivalNotificationDateAndTime);

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.ArrivalMovementHeader.BM_ArrivalDate = ZDateTime.BrettsBirthday;
		var arrivalProvider = (T)Activator.CreateInstance(typeof(T), arrivalHeader);
		AssertEquals(ZDateTime.BrettsBirthday.ToDateTime(), arrivalProvider.ArrivalNotificationDateAndTime);
	});

	public void TestIncidentFlag() => CombineAssertions(() =>
	{
		AssertEquals("BH_ExportFlag = empty/N", false, provider.IncidentFlag);
		header.BH_ExportFlag = EventFlagList.Codes.Yes;
		AssertEquals("BH_ExportFlag = Y", true, provider.IncidentFlag);
	});

	public void TestSimplifiedProcedure() => CombineAssertions(() =>
	{
		AssertEquals("Departure - no CusAuthorization", false, provider.SimplifiedProcedure);
		header.MovementHeader.CusAuthorizationUsages.AddNew();
		AssertEquals("Departure - 1 CusAuthorization", true, provider.SimplifiedProcedure);

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.CusAuthorizationUsages.AddNew();
		AssertEquals("Arrival - 1 CusAuthorization", true, provider.SimplifiedProcedure);
	});

	public void TestAmendmentTypeFlag() => AssertEquals(false, provider.AmendmentTypeFlag);

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		provider = (T)Activator.CreateInstance(typeof(T), header);
	}

	T provider;
	protected NctsHeader header;

	protected override T GetProvider() => provider;
}
