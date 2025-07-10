using System;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class CommonImportJobDeclarationValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckJE_TotalNoOfPacksWhenExceedIntegerMaxValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var pack1 = declaration.Packages.AddNew();
			pack1.CW_PackQty = int.MaxValue;
			var pack2 = declaration.Packages.AddNew();
			pack2.CW_PackQty = 1;
			AssertNoExceptionThrown(() =>
			{
				declaration.Validation.ValidateJE_TotalNoOfPacks();
			});
		}

		public void TestCheckAirOrHandCarryFlightNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EnableCRL = false;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "!Y&BA";
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "AB001";
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "!Y&BA";
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "AB001";
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
		}

		public void TestCheckJE_GB()
		{
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2786", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2720", "Test Name", startDate, endDate);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.Validation.ValidateJE_GB();
			AssertHasMessageError(declaration.JE_GBInfo, CommonImportJobDeclarationValidation.HasNoProcessingPort);

			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, "1101");
			Factory.Save();
			declaration.Validation.ValidateJE_GB();
			AssertNoMessageError(declaration.JE_GBInfo, CommonImportJobDeclarationValidation.HasNoProcessingPort);

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, "");

			var coll = new EntryProcessingPortsMappingCollection();
			var mapping = coll.AddNew();
			mapping.EntryPort = "2786";
			mapping.ProcessingPort = "2720";
			USCustomsDataRegistry.Instance.EntryProcessingPortMappings.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, coll);
			declaration.US_SchDEntry = "2786";
			Factory.Save();
			declaration.Validation.ValidateJE_GB();
			AssertNoMessageError(declaration.JE_GBInfo, CommonImportJobDeclarationValidation.HasNoProcessingPort);
		}
	}
}
