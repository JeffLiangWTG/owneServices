using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ExportJobDeclarationValidation))]
sealed class ExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ExportJobDeclarationValidation>
{
	public void TestCheckJE_CustomsOffice()
	{
		CombineAssertions(() =>
		{
			assertCustomsOfficeForTransportMode(Core.Constants.TransportModes.Sea, NOCustomsOfficesList.Codes._3260, NOCustomsOfficesList.Codes._3210);
			assertCustomsOfficeForTransportMode(Core.Constants.TransportModes.Rail, NOCustomsOfficesList.Codes._3270, NOCustomsOfficesList.Codes._3310);
			assertCustomsOfficeForTransportMode(Core.Constants.TransportModes.Road, NOCustomsOfficesList.Codes._3330, NOCustomsOfficesList.Codes._3230);
			assertCustomsOfficeForTransportMode(Core.Constants.TransportModes.Air, NOCustomsOfficesList.Codes._3280, NOCustomsOfficesList.Codes._3220);
			assertCustomsOfficeForTransportMode(Core.Constants.TransportModes.Mail, NOCustomsOfficesList.Codes._3290, NOCustomsOfficesList.Codes._3240);
			assertCustomsOfficeForTransportMode(Core.Constants.TransportModes.FixedTransportInstallations, NOCustomsOfficesList.Codes._3340, NOCustomsOfficesList.Codes._3320);
			assertCustomsOfficeForTransportMode(Core.Constants.TransportModes.InlandWaterwayTransport, NOCustomsOfficesList.Codes._3350, NOCustomsOfficesList.Codes._3380);
			assertCustomsOfficeForTransportMode(Core.Constants.TransportModes.OwnPropulsion, NOCustomsOfficesList.Codes._3430, NOCustomsOfficesList.Codes._3250);
		});
		void assertCustomsOfficeForTransportMode(ZString transportMode, ZString invalidOfficeCode, ZString validOfficeCode)
		{
			declaration.JE_TransportMode = transportMode;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsOfficeInfo, invalidOfficeCode, validOfficeCode);
		}
	}

	public void TestCheckJE_GoodsDestination()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_GoodsDestinationInfo, "XX", Core.Constants.CountryCodes.Norway);
	}

	public void TestCheckJE_GoodsNumber()
	{
		const string goodsnumberLocationBlankText = "Goods number is not required when goods location is blank.";
		const string goodsnumberLocationSetText = "Goods number is required when goods location is set.";
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_LocationOfGoods = ZString.Empty;
			declaration.JE_GoodsNumber = "TEST";
			AssertHasWarning("Empty location of goods when goods number is set should show warning", declaration.JE_GoodsNumberInfo, goodsnumberLocationBlankText);

			declaration.JE_LocationOfGoods = "A";
			declaration.Validation.ValidateJE_GoodsNumber();
			AssertNoMessageError("Given a location of goods when goods number is set should not show message error", declaration.JE_GoodsNumberInfo, goodsnumberLocationSetText);

			declaration.JE_GoodsNumber = ZString.Empty;
			AssertHasMessageError("Given a location of goods when goods number is empty should show message error", declaration.JE_GoodsNumberInfo, goodsnumberLocationSetText);

			declaration.JE_LocationOfGoods = ZString.Empty;
			declaration.Validation.ValidateJE_GoodsNumber();
			AssertNoWarning("Empty location of goods when goods number is empty should not show message error", declaration.JE_GoodsNumberInfo, goodsnumberLocationBlankText);
		});
	}

	public void TestCheckJE_GoodsOrigin()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_GoodsOriginInfo, "XX", Core.Constants.CountryCodes.Norway);
	}

	public void TestCheckJE_LocationOfGoods()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_LocationOfGoodsInfo, "XXX", ExportGoodsLocationCodeList.Codes.CustomsWarehouseA);
	}

	public void TestCheckJE_LocationOfGoods_CEI_Procedure()
	{
		const string messageErrorText = "Please enter Goods location for goods released from customs warehouse.";
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_Procedure = new ZString("1070");
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertHasMessageError("Procedure that ends with 70 requires that location of goods is set", declaration.JE_LocationOfGoodsInfo, messageErrorText);

			instruction.CEI_Procedure = new ZString("1071");
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageError("Procedure that ends with 71 requires that location of goods is set", declaration.JE_LocationOfGoodsInfo, messageErrorText);

			declaration.JE_LocationOfGoods = "A";
			AssertNoMessageError("Location of goods is set so there should be no error", declaration.JE_LocationOfGoodsInfo, messageErrorText);

			declaration.CustomsEntryInstructions.Clear();
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertNoMessageError("Only when having a procedure that ends with 71 is it required that location of goods is set", declaration.JE_LocationOfGoodsInfo, messageErrorText);
		});
	}

	public void TestCheckJE_OH_Supplier()
	{
		string personMessageText = "The exporter is a natural person but has no social security number.";
		string orgMessageText = "The exporter is an organization but has no ORG number or MVA registered number.";
		var supplier = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;
		CombineAssertions(() =>
		{
			supplier.OH_Category = Constants.OrgHeaderType.NaturalPerson;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError("Main supplier is a person without SSN.", declaration.JE_OH_SupplierInfo, personMessageText);

			var cusCode = supplier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = Constants.OrgCodeType.SocialSecurityNumber;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError("Main supplier is a person but has an empty SSN CusCode.", declaration.JE_OH_SupplierInfo, personMessageText);

			cusCode.OK_CustomsRegNo = "200001011234";
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError("Main supplier is a person with a valid SSN CusCode.", declaration.JE_OH_SupplierInfo, personMessageText);

			supplier.OH_Category = Constants.OrgHeaderType.Organization;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError("Main supplier is an organization without organization number.", declaration.JE_OH_SupplierInfo, orgMessageText);

			cusCode.OK_CodeType = Constants.OrgCodeType.OrganizationNumber;
			cusCode.OK_CustomsRegNo = string.Empty;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError("Main supplier is an organization but has an empty organization CusCode.", declaration.JE_OH_SupplierInfo, orgMessageText);

			cusCode.OK_CustomsRegNo = "1234567890";
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError("Main supplier is an organization with a valid organization CusCode.", declaration.JE_OH_SupplierInfo, orgMessageText);

			cusCode.OK_CodeType = Constants.OrgCodeType.MVARegistrationNumber;
			cusCode.OK_CustomsRegNo = string.Empty;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError("Main supplier is an organization but has an empty MVA CusCode.", declaration.JE_OH_SupplierInfo, orgMessageText);

			cusCode.OK_CustomsRegNo = "2345678901";
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError("Main supplier is an organization with a valid MVA CusCode.", declaration.JE_OH_SupplierInfo, orgMessageText);
		});
	}

	public void TestSupplierHasPOC()
	{
		var powerOfAttorneyWarningMessage = AuthorityToActValidator.GetNoPOADocumentForImporterString(new AuthorityToActValidator().CountrySpecificNameForPOA);
		var supplier = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;
		var targetInfo = declaration.JE_OH_SupplierInfo;

		CombineAssertions(() =>
		{
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasWarning("Supplier has no POC", targetInfo, powerOfAttorneyWarningMessage);

			var poc = supplier.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoWarning("Supplier has POC", targetInfo, powerOfAttorneyWarningMessage);
		});
	}

	#region Impl

	protected override string MessageType => JobMessageTypeList.Codes.Export;

	protected override ExportJobDeclarationValidation GetValidation() => new ExportJobDeclarationValidation(declaration);

	#endregion
}
