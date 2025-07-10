using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5ValidationTest : TestCaseWithFactory
{
	public void TestCheckBM_ActiveBorderIdentificationType_Mandatory_B2101()
	{
		const string message = "[B2101] Type of ID can't be empty.";
		var departureMovement = CreateDepartureMovement();

		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
			AssertNoMessageError("BM_ExportTransportMode is Empty and BM_ActiveBorderIdentificationType is empty", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
			AssertHasMessageError("BM_ExportTransportMode is not Empty and BM_ActiveBorderIdentificationType is empty", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.BM_ActiveBorderIdentificationType = "21";
			AssertNoMessageError("BM_ExportTransportMode is not Empty and BM_ActiveBorderIdentificationType is not empty", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
		});
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
			AssertNoMessageError("In Phase5: BM_ActiveBorderIdentificationType is empty", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
		});
	}

	public void TestCheckBM_CustomsOfficeAtBorder_Mandatory_B2101_WhenActive()
	{
		const string message = "[B2101] Customs Office for Transport Border must be declared.";
		var departureMovement = CreateDepartureMovement();

		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			departureMovement.BM_CustomsOfficeAtBorder = ZString.Empty;
			AssertNoMessageError("BM_ExportTransportMode is Empty and BM_CustomsOfficeAtBorder is empty", departureMovement.BM_CustomsOfficeAtBorderInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertHasMessageError("BM_ExportTransportMode is not Empty and BM_CustomsOfficeAtBorder is empty", departureMovement.BM_CustomsOfficeAtBorderInfo, message);

			departureMovement.BM_CustomsOfficeAtBorder = "PL1234";
			AssertNoMessageError("BM_ExportTransportMode is not Empty and BM_CustomsOfficeAtBorder is not empty", departureMovement.BM_CustomsOfficeAtBorderInfo, message);
		});
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			departureMovement.BM_CustomsOfficeAtBorder = ZString.Empty;
			AssertNoMessageError("In Phase5: BM_CustomsOfficeAtBorder is empty", departureMovement.BM_CustomsOfficeAtBorderInfo, message);
		});
	}

	public void TestCheckBM_RN_NKTOLCarrierNationality_Mandatory_B1850()
	{
		const string message = "[B1850] Nationality for Transport ID is empty. It must be declared.";
		var departureMovement = CreateDepartureMovement();

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertNoMessageError("BM_ExportTransportMode = 2 and BM_RN_NKTOLCarrierNationality is empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);

			departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
			AssertNoMessageError("BM_ExportTransportMode = 2 and BM_RN_NKTOLCarrierNationality is not empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertHasMessageError("BM_ExportTransportMode <> 2 and BM_RN_NKTOLCarrierNationality is empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);

			departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
			AssertNoMessageError("BM_ExportTransportMode <> 2 and BM_RN_NKTOLCarrierNationality is not empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);

			departureMovement.BM_ExportTransportMode = ZString.Empty;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertNoMessageError("BM_ExportTransportMode is Empty and BM_RN_NKTOLCarrierNationality is empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);
		});
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertNoMessageError("Outside Phase5: BM_ExportTransportMode <> 2 and BM_RN_NKTOLCarrierNationality is empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);
		});
	}

	public void TestCheckBM_RN_NKTOLCarrierNationality_Mandatory_B2101()
	{
		const string message = "[B2101] Nationality for Transport ID is empty. It must be declared.";
		var departureMovement = CreateDepartureMovement();

		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertNoMessageError("BM_ExportTransportMode is Empty and BM_RN_NKTOLCarrierNationality is empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertHasMessageError("BM_ExportTransportMode is not Empty and BM_RN_NKTOLCarrierNationality is empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);

			departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
			AssertNoMessageError("BM_ExportTransportMode is not Empty and BM_RN_NKTOLCarrierNationality is not empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);
		});
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertNoMessageError("In Phase5: BM_RN_NKTOLCarrierNationality is empty", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);
		});
	}

	public void TestCheckBM_TOLCarrierID_Mandatory_B2101()
	{
		const string message = "[B2101] Transport ID can't be empty.";
		var departureMovement = CreateDepartureMovement();
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			departureMovement.BM_TOLCarrierID = ZString.Empty;
			AssertNoMessageError("BM_ExportTransportMode is Empty and BM_TOLCarrierID is empty", departureMovement.BM_TOLCarrierIDInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertHasMessageError("BM_ExportTransportMode is not Empty and BM_TOLCarrierID is empty", departureMovement.BM_TOLCarrierIDInfo, message);

			departureMovement.BM_TOLCarrierID = "ABC123";
			AssertNoMessageError("BM_ExportTransportMode is not Empty and BM_TOLCarrierID is not empty", departureMovement.BM_TOLCarrierIDInfo, message);
		});
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			departureMovement.BM_TOLCarrierID = ZString.Empty;
			AssertNoMessageError("In Phase5: BM_TOLCarrierID is empty", departureMovement.BM_TOLCarrierIDInfo, message);
		});
	}

	public void TestCheckRuleRP54()
	{
		const string messageError = "[RP54] You have not entered Date Limit.";
		var departureMovement = CreateDepartureMovement();
		var propertyInfo = departureMovement.BM_ExportDateInfo;

		CombineAssertions(() =>
		{
			departureMovement.IsSimplifiedNctsProcedure = true;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			departureMovement.Validation.ValidateBM_ExportDate();
			AssertHasMessageError("Empty BM_ExportDate", propertyInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.UtcNow;
			AssertNoMessageError("ExportDate is present", propertyInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Empty;
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertNoMessageError("IsSimplifiedNctsProcedure is false", propertyInfo, messageError);

			departureMovement.IsSimplifiedNctsProcedure = true;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			AssertNoMessageError("BM_AdditionalDeclarationType is D", propertyInfo, messageError);
		});
	}

	public void TestCheckRuleRP56()
	{
		const string messageError = "[RP56] The entered Date Limit is earlier to today. It should be a current/future date.";
		var departureMovement = CreateDepartureMovement();
		var propertyInfo = departureMovement.BM_ExportDateInfo;

		CombineAssertions(() =>
		{
			departureMovement.IsSimplifiedNctsProcedure = true;
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			departureMovement.Validation.ValidateBM_ExportDate();
			AssertNoMessageError("Empty BM_ExportDate", propertyInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertHasMessageError("ExportDate is in the past", propertyInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Today;
			AssertNoMessageError("ExportDate is today", propertyInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(1);
			AssertNoMessageError("ExportDate is in the future", propertyInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Empty;
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertNoMessageError("IsSimplifiedNctsProcedure is false", propertyInfo, messageError);
		});
	}

	NctsDepartureMovementHeader CreateDepartureMovement()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}
}
