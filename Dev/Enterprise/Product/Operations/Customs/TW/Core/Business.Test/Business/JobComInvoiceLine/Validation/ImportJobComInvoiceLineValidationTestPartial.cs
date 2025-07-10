using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	partial class ImportJobComInvoiceLineValidationTest
	{
		public void TestCheckJI_Compositions()
		{
			var warningMessage = "System will automatically declare 'NIL' when 'Specification' is empty.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Compositions = "XX";
			AssertNoWarning(invoiceLine.JI_CompositionsInfo, warningMessage);

			invoiceLine.JI_Compositions = ZString.Empty;
			AssertHasWarning(invoiceLine.JI_CompositionsInfo, warningMessage);

			invoiceLine.JI_Compositions = "X2";
			AssertNoWarning(invoiceLine.JI_CompositionsInfo, warningMessage);

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			invoiceLine.JI_Compositions = ZString.Empty;
			AssertNoWarning(invoiceLine.JI_CompositionsInfo, warningMessage);
		}

		public void TestCheckJI_ModelYear()
		{
			var messageError = "Model Year should be 1000-9999.";
			InvoiceLine.JI_Tariff = "86";
			InvoiceLine.JI_ModelYear = 2;
			AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, messageError);
			InvoiceLine.JI_ModelYear = 20;
			AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, messageError);
			InvoiceLine.JI_ModelYear = 201;
			AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, messageError);
			InvoiceLine.JI_ModelYear = 2014;
			AssertNoMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, messageError);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				InvoiceLine.JI_Tariff = "86";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertNoMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_CarType = "X";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_CarType = ZString.Empty;
				InvoiceLine.JI_CarCondition = "A";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				InvoiceLine.JI_BrandName = "FERRARI";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_BrandName = ZString.Empty;
				InvoiceLine.JI_Model = "458 ITALIA";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_Model = ZString.Empty;
				InvoiceLine.JI_Transmission = "A";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_Transmission = ZString.Empty;
				InvoiceLine.JI_EngineType = "CG";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_HasCatalystConverter = ZString.Empty;
				InvoiceLine.JI_EquipmentPrintMode = "EEC";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_EquipmentPrintMode = ZString.Empty;
				InvoiceLine.JI_Displacement = "3000";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_Displacement = ZString.Empty;
				InvoiceLine.JI_NumberOfDoor = 3;
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_NumberOfDoor = ZShort.Zero;
				InvoiceLine.JI_Seats = 2;
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_Seats = ZShort.Zero;
				InvoiceLine.JI_Cylinders = 12;
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_Cylinders = ZShort.Zero;
				InvoiceLine.JI_Gears = 7;
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "9";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertHasMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_Tariff = "40";
				InvoiceLine.JI_ModelYear = ZShort.Zero;
				AssertNoMessageErrorContaining(InvoiceLine.JI_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckJI_NumberOfDoor()
		{
			var messageError = "Number of Door should be 0-9.";
			var info = InvoiceLine.JI_NumberOfDoorInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 9, messageError);
			InvoiceLine.JI_NumberOfDoor = 6;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_Displacement()
		{
			AssertEquals(9, InvoiceLine.JI_DisplacementInfo.MaxLength);
			InvoiceLine.JI_Displacement = "A";
			AssertHasMessageErrorContaining(InvoiceLine.JI_DisplacementInfo, ValidationConstants.InvoiceLine.DisplacementShouldBeOnlyNumerics);
			InvoiceLine.JI_Displacement = "-1";
			AssertHasMessageErrorContaining(InvoiceLine.JI_DisplacementInfo, ValidationConstants.InvoiceLine.DisplacementOutOfRange);
			InvoiceLine.JI_Displacement = "0";
			AssertHasMessageErrorContaining(InvoiceLine.JI_DisplacementInfo, ValidationConstants.InvoiceLine.DisplacementOutOfRange);
			InvoiceLine.JI_Displacement = "999999.99";
			AssertNoMessageErrorContaining(InvoiceLine.JI_DisplacementInfo, ValidationConstants.InvoiceLine.DisplacementOutOfRange);
			InvoiceLine.JI_Displacement = "1000000";
			AssertHasMessageErrorContaining(InvoiceLine.JI_DisplacementInfo, ValidationConstants.InvoiceLine.DisplacementOutOfRange);
		}

		public void TestCheckJI_Cylinders()
		{
			var messageError = "Number of Cylinder should be 0-99.";
			var info = InvoiceLine.JI_CylindersInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 99, messageError);
			InvoiceLine.JI_Cylinders = 66;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_Gears()
		{
			var messageError = "Number of Gear should be 0-99.";
			var info = InvoiceLine.JI_GearsInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 99, messageError);
			InvoiceLine.JI_Gears = 66;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_Seats()
		{
			var messageError = "Number of Seat should be 0-99.";
			var info = InvoiceLine.JI_SeatsInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 99, messageError);
			InvoiceLine.JI_Seats = 66;
			AssertNoMessageErrors(info);
		}

		void AssertNumberBetweenMinValueAndMaxValue(ZPropertyInfo info, ZShort minValue, ZShort maxValue, string messageError)
		{
			info.Value = new ZShort(minValue - 1);
			AssertHasMessageError(info, messageError);
			info.Value = minValue;
			AssertNoMessageError(info, messageError);
			info.Value = new ZShort(maxValue + 1);
			AssertHasMessageError(info, messageError);
			info.Value = maxValue;
			AssertNoMessageError(info, messageError);
		}

		public void TestCheckJI_CarType()
		{
			InvoiceLine.JI_Tariff = "86";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_CarCondition = "X";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_CarCondition = ZString.Empty;
			InvoiceLine.JI_ModelYear = 2019;
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_ModelYear = ZShort.Zero;
			InvoiceLine.JI_BrandName = "FERRARI";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_BrandName = ZString.Empty;
			InvoiceLine.JI_Model = "458 ITALIA";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Model = ZString.Empty;
			InvoiceLine.JI_Transmission = "A";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Transmission = ZString.Empty;
			InvoiceLine.JI_EngineType = "CG";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_HasCatalystConverter = ZString.Empty;
			InvoiceLine.JI_EquipmentPrintMode = "EEC";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_EquipmentPrintMode = ZString.Empty;
			InvoiceLine.JI_Displacement = "3000";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Displacement = ZString.Empty;
			InvoiceLine.JI_NumberOfDoor = 3;
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_NumberOfDoor = ZShort.Zero;
			InvoiceLine.JI_Seats = 2;
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Seats = ZShort.Zero;
			InvoiceLine.JI_Cylinders = 12;
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Cylinders = ZShort.Zero;
			InvoiceLine.JI_Gears = 7;
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_ModelYear = ZShort.Zero;
			InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "9";
			InvoiceLine.JI_CarType = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_CarType = "X";
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_CarType = CarTypeCodeList.Codes.A1;
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_EquipmentPrintMode()
		{
			InvoiceLine.JI_EquipmentPrintMode = "X";
			AssertHasMessageErrorContaining(InvoiceLine.JI_EquipmentPrintModeInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_EquipmentPrintMode = EquipmentPrintModeList.Codes.EEC;
			AssertNoMessageErrorContaining(InvoiceLine.JI_EquipmentPrintModeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_LHD()
		{
			InvoiceLine.JI_LHD = "X";
			AssertHasMessageErrorContaining(InvoiceLine.JI_LHDInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_LHD = LeftSideSteeringCodeList.Codes.Left;
			AssertNoMessageErrorContaining(InvoiceLine.JI_LHDInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_CarType = CarTypeCodeList.Codes.A1;
			InvoiceLine.JI_LHD = ZString.Empty;
		}

		public void TestCheckJI_EngineType()
		{
			InvoiceLine.JI_EngineType = "X";
			AssertHasMessageErrorContaining(InvoiceLine.JI_EngineTypeInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_EngineType = EngineTypeCodeList.Codes.DE;
			AssertNoMessageErrorContaining(InvoiceLine.JI_EngineTypeInfo, ListValidation.InvalidCodeMessageError);
			string carTypeMessageError = "Engine type cannot be 'GA' when car type is 'J1'";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				InvoiceLine.JI_CarType = CarTypeCodeList.Codes.J1;
				InvoiceLine.JI_EngineType = EngineTypeCodeList.Codes.OT;
				InvoiceLine.JI_Tariff = "86";
				AssertEquals(false, InvoiceLine.JI_EngineTypeInfo.HasMessageError(carTypeMessageError));
				InvoiceLine.JI_EngineType = EngineTypeCodeList.Codes.OT;
				AssertEquals(false, InvoiceLine.JI_EngineTypeInfo.HasMessageError(carTypeMessageError));
				InvoiceLine.JI_EngineType = EngineTypeCodeList.Codes.GA;
				AssertEquals(true, InvoiceLine.JI_EngineTypeInfo.HasMessageError(carTypeMessageError));
			}
		}

		public void TestCheckJI_HasCatalystConverter()
		{
			InvoiceLine.JI_HasCatalystConverter = "X";
			AssertHasMessageErrorContaining(InvoiceLine.JI_HasCatalystConverterInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_HasCatalystConverter = CatalystConverterPrintModeList.Codes.No;
			AssertNoMessageErrorContaining(InvoiceLine.JI_HasCatalystConverterInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_Transmission()
		{
			InvoiceLine.JI_Transmission = "X";
			AssertHasMessageErrorContaining(InvoiceLine.JI_TransmissionInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_Transmission = TransmissionCodeList.Codes.Auto;
			AssertNoMessageErrorContaining(InvoiceLine.JI_TransmissionInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_CarCondition()
		{
			InvoiceLine.JI_Tariff = "86";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_CarType = "X";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_CarType = ZString.Empty;
			InvoiceLine.JI_ModelYear = 2019;
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_ModelYear = ZShort.Zero;
			InvoiceLine.JI_BrandName = "FERRARI";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_BrandName = ZString.Empty;
			InvoiceLine.JI_Model = "458 ITALIA";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Model = ZString.Empty;
			InvoiceLine.JI_Transmission = "A";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Transmission = ZString.Empty;
			InvoiceLine.JI_EngineType = "CG";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_HasCatalystConverter = ZString.Empty;
			InvoiceLine.JI_EquipmentPrintMode = "EEC";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_EquipmentPrintMode = ZString.Empty;
			InvoiceLine.JI_Displacement = "3000";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Displacement = ZString.Empty;
			InvoiceLine.JI_NumberOfDoor = 3;
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_NumberOfDoor = ZShort.Zero;
			InvoiceLine.JI_Seats = 2;
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Seats = ZShort.Zero;
			InvoiceLine.JI_Cylinders = 12;
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_Cylinders = ZShort.Zero;
			InvoiceLine.JI_Gears = 7;
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_ModelYear = ZShort.Zero;
			InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "9";
			InvoiceLine.JI_CarCondition = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_CarCondition = "X";
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_CarCondition = CarConditionCodeList.Codes.CrashTruck;
			AssertNoMessageErrorContaining(InvoiceLine.JI_CarConditionInfo, ListValidation.InvalidCodeMessageError);
		}

		public override void TestCheckJI_GoodsType()
		{
			base.TestCheckJI_GoodsType();
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "DN" });
			var header = jobDeclartion.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			line.JI_GoodsType = "222";
			AssertHasMessageErrorContaining(line.JI_GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			line.JI_GoodsType = CPT_107_301_GoodsTypeList.Codes._201;
			AssertNoMessageErrorContaining(line.JI_GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF" });
			header = jobDeclartion.Invoices.AddNew();
			line = header.JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			line.JI_GoodsType = CPT_107_301_GoodsTypeList.Codes._200;
			AssertHasMessageErrorContaining(line.JI_GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			line.JI_GoodsType = CPT_107_601_GoodsTypeList.Codes._1;
			AssertNoMessageErrorContaining(line.JI_GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "DN", "IF", "DH" });
			header = jobDeclartion.Invoices.AddNew();
			line = header.JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().ForEach(x => x.IsLinkedCMHeader = true);
			line.JI_GoodsType = "222";
			AssertHasMessageErrorContaining(line.JI_GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			line.JI_GoodsType = CPT_107_301_GoodsTypeList.Codes._201;
			AssertNoMessageErrorContaining(line.JI_GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			line.JI_GoodsType = CPT_107_601_GoodsTypeList.Codes._1;
			AssertNoMessageErrorContaining(line.JI_GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_ProductGrade()
		{
			InvoiceLine.JI_ProductGrade = "!XXX";
			AssertHasMessageError(InvoiceLine.JI_ProductGradeInfo, TW.Business.ValidationConstants.InvoiceLine.GradeOnlyAllowsAlphanumericCharacters);
			InvoiceLine.JI_ProductGrade = "X123";
			AssertNoMessageErrors(InvoiceLine.JI_ProductGradeInfo);
			InvoiceLine.JI_ProductGrade = "1A123123";
			AssertNoMessageErrors(InvoiceLine.JI_ProductGradeInfo);
		}

		public void TestCheckJI_ProductThickness()
		{
			InvoiceLine.JI_ProductThickness = "!XXX";
			AssertHasMessageError(InvoiceLine.JI_ProductThicknessInfo, TW.Business.ValidationConstants.InvoiceLine.ThicknessOnlyAllowsAlphanumericCharacters);
			InvoiceLine.JI_ProductThickness = "X123";
			AssertNoMessageErrors(InvoiceLine.JI_ProductThicknessInfo);
			InvoiceLine.JI_ProductGrade = "1A123123";
			AssertNoMessageErrors(InvoiceLine.JI_ProductThicknessInfo);
		}

		public void TestCheckJI_TariffAdditionalCode()
		{
			InvoiceLine.JI_TariffAdditionalCode = "!XXX";
			AssertHasMessageError(InvoiceLine.JI_TariffAdditionalCodeInfo, TW.Business.ValidationConstants.InvoiceLine.WarningTariffAdditionalCodeNotAlphanumericCharacters);
			InvoiceLine.JI_TariffAdditionalCode = "X123";
			AssertNoMessageErrors(InvoiceLine.JI_TariffAdditionalCodeInfo);
			InvoiceLine.JI_TariffAdditionalCode = "1A12";
			AssertNoMessageErrors(InvoiceLine.JI_TariffAdditionalCodeInfo);
		}

		public void TestCheckJI_CusValueConvRatio()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_CusValueConvRatioInfo, TW.Business.ValidationConstants.InvoiceLine.CusValueConvRatioNegative);
			invoiceLine.JI_CusValueConvRatio = 0m;
			AssertNoMessageErrorContaining(invoiceLine.JI_CusValueConvRatioInfo, TW.Business.ValidationConstants.InvoiceLine.RangeMustBeBetween0And1);
			invoiceLine.JI_CusValueConvRatio = 0.7m;
			AssertNoMessageErrorContaining(invoiceLine.JI_CusValueConvRatioInfo, TW.Business.ValidationConstants.InvoiceLine.RangeMustBeBetween0And1);
			invoiceLine.JI_CusValueConvRatio = 1m;
			AssertNoMessageErrorContaining(invoiceLine.JI_CusValueConvRatioInfo, TW.Business.ValidationConstants.InvoiceLine.RangeMustBeBetween0And1);
			invoiceLine.JI_CusValueConvRatio = 1.3m;
			AssertHasMessageErrorContaining(invoiceLine.JI_CusValueConvRatioInfo, TW.Business.ValidationConstants.InvoiceLine.RangeMustBeBetween0And1);
			invoiceLine.JI_CusValueConvRatio = -0.0001M;
			AssertHasMessageErrorContaining(invoiceLine.JI_CusValueConvRatioInfo, TW.Business.ValidationConstants.InvoiceLine.CusValueConvRatioNegative);
			invoiceLine.JI_CusValueConvRatio = 0M;
			AssertNoMessageErrorContaining(invoiceLine.JI_CusValueConvRatioInfo, TW.Business.ValidationConstants.InvoiceLine.CusValueConvRatioNegative);
			invoiceLine.JI_CusValueConvRatio = 0.0001M;
			AssertNoMessageErrorContaining(invoiceLine.JI_CusValueConvRatioInfo, TW.Business.ValidationConstants.InvoiceLine.CusValueConvRatioNegative);
		}

		public void TestCheckJI_AlcoholAge()
		{
			var info = InvoiceLine.JI_AlcoholAgeInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 9999);
			InvoiceLine.JI_AlcoholAge = 666;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_BarCode()
		{
			InvoiceLine.JI_BarCode = "!XXXXXXXXXXXX";
			AssertHasMessageError(InvoiceLine.JI_BarCodeInfo, ValidationConstants.InvoiceLine.InvalidValue(InvoiceLine.JI_BarCodeInfo.HumanReadableName));
			InvoiceLine.JI_BarCode = "1XXXXXXXXXXXX";
			AssertNoMessageErrors(InvoiceLine.JI_BarCodeInfo);
			InvoiceLine.JI_BarCode = "1XX";
			AssertHasMessageError(InvoiceLine.JI_BarCodeInfo, ValidationConstants.InvoiceLine.BarCodeShouldBe13Characters);
		}

		public void TestCheckJI_PHValue()
		{
			InvoiceLine.JI_PHValue = "XX";
			AssertHasMessageError(InvoiceLine.JI_PHValueInfo, ValidationConstants.InvoiceLine.InvalidValue(InvoiceLine.JI_PHValueInfo.HumanReadableName));
			AssertHasMessageError(InvoiceLine.JI_PHValueNumericInfo, ValidationConstants.InvoiceLine.InvalidValue(InvoiceLine.JI_PHValueInfo.HumanReadableName));
			InvoiceLine.JI_PHValue = "9.6";
			AssertNoMessageErrors(InvoiceLine.JI_PHValueInfo);
			AssertNoMessageErrors(InvoiceLine.JI_PHValueNumericInfo);
			InvoiceLine.JI_PHValue = "0.6";
			AssertNoMessageErrors(InvoiceLine.JI_PHValueInfo);
			AssertNoMessageErrors(InvoiceLine.JI_PHValueNumericInfo);
			InvoiceLine.JI_PHValue = "5..3";
			AssertHasMessageError(InvoiceLine.JI_PHValueInfo, ValidationConstants.InvoiceLine.InvalidValue(InvoiceLine.JI_PHValueInfo.HumanReadableName));
			AssertHasMessageError(InvoiceLine.JI_PHValueNumericInfo, ValidationConstants.InvoiceLine.InvalidValue(InvoiceLine.JI_PHValueInfo.HumanReadableName));
			InvoiceLine.JI_PHValue = "-1";
			AssertHasMessageError(InvoiceLine.JI_PHValueInfo, ValidationConstants.InvoiceLine.PHScaleRanges);
			AssertHasMessageError(InvoiceLine.JI_PHValueNumericInfo, ValidationConstants.InvoiceLine.PHScaleRanges);
			InvoiceLine.JI_PHValue = "14";
			AssertNoMessageErrors(InvoiceLine.JI_PHValueInfo);
			AssertNoMessageErrors(InvoiceLine.JI_PHValueNumericInfo);
			InvoiceLine.JI_PHValue = "15";
			AssertHasMessageError(InvoiceLine.JI_PHValueInfo, ValidationConstants.InvoiceLine.PHScaleRanges);
			AssertHasMessageError(InvoiceLine.JI_PHValueNumericInfo, ValidationConstants.InvoiceLine.PHScaleRanges);
		}

		public void TestCheckJI_SterilizationValue()
		{
			InvoiceLine.JI_SterilizationValue = "XX";
			AssertHasMessageError(InvoiceLine.JI_SterilizationValueInfo, ValidationConstants.InvoiceLine.InvalidValue(InvoiceLine.JI_SterilizationValueInfo.HumanReadableName));
			InvoiceLine.JI_SterilizationValue = "9.6";
			AssertNoMessageErrors(InvoiceLine.JI_SterilizationValueInfo);
			InvoiceLine.JI_SterilizationValue = "0.6";
			AssertNoMessageErrors(InvoiceLine.JI_SterilizationValueInfo);
			InvoiceLine.JI_SterilizationValue = "5..3";
			AssertHasMessageError(InvoiceLine.JI_SterilizationValueInfo, ValidationConstants.InvoiceLine.InvalidValue(InvoiceLine.JI_SterilizationValueInfo.HumanReadableName));
			InvoiceLine.JI_SterilizationValue = "-1";
			AssertHasMessageError(InvoiceLine.JI_SterilizationValueInfo, ValidationConstants.InvoiceLine.SterilizationScaleRanges);
			InvoiceLine.JI_SterilizationValue = "9.9";
			AssertNoMessageErrors(InvoiceLine.JI_SterilizationValueInfo);
			InvoiceLine.JI_SterilizationValue = "10";
			AssertHasMessageError(InvoiceLine.JI_SterilizationValueInfo, ValidationConstants.InvoiceLine.SterilizationScaleRanges);
		}

		public override void TestCheckJI_InnerPackType()
		{
			base.TestCheckJI_InnerPackType();
			InvoiceLine.JI_InnerPackType = "X";
			AssertHasMessageErrorContaining(InvoiceLine.JI_InnerPackTypeInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_InnerPackType = CPT_115_InnerPackageTypeList.Codes._2;
			AssertNoMessageErrorContaining(InvoiceLine.JI_InnerPackTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public override void TestCheckJI_InnerPackingMaterial()
		{
			base.TestCheckJI_InnerPackingMaterial();
			InvoiceLine.JI_InnerPackingMaterial = "X";
			AssertHasMessageErrorContaining(InvoiceLine.JI_InnerPackingMaterialInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_InnerPackingMaterial = CPT_114_InnerPackingMaterialList.Codes._19;
			AssertNoMessageErrorContaining(InvoiceLine.JI_InnerPackingMaterialInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_EPTDigit1()
		{
			var messageErrorInvalidCode = ListValidation.InvalidCodeMessageError;
			var targetInfo = InvoiceLine.JI_EPTDigit1Info;
			InvoiceLine.JI_EPTDigit1 = "0";
			AssertHasMessageErrorContaining(targetInfo, messageErrorInvalidCode);
			InvoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.A;
			AssertNoMessageErrorContaining(targetInfo, messageErrorInvalidCode);
			SetUpEnvironmentalProtectionTariff();
			var messageErrorYouHaveNotEntered = MandatoryValidation.YouHaveNotEntered;
			InvoiceLine.JI_EPTDigit1 = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
			InvoiceLine.JI_Tariff = "2713200001";
			InvoiceLine.Validation.ValidateJI_EPTDigit1();
			AssertHasMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
			InvoiceLine.JI_EPTDigit1 = "X";
			AssertNoMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
		}

		public void TestCheckJI_EPTDigit2()
		{
			var targetInfo = InvoiceLine.JI_EPTDigit2Info;
			var messageErrorInvalidCode = ListValidation.InvalidCodeMessageError;
			InvoiceLine.JI_EPTDigit2 = "X";
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(targetInfo, messageErrorInvalidCode);
				InvoiceLine.JI_EPTDigit2 = ContainerCapacityList.Codes._1;
				AssertNoMessageErrorContaining(targetInfo, messageErrorInvalidCode);
				SetUpEnvironmentalProtectionTariff();
				var messageErrorYouHaveNotEntered = MandatoryValidation.YouHaveNotEntered;
				InvoiceLine.JI_EPTDigit2 = ZString.Empty;
				AssertNoMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
				InvoiceLine.JI_Tariff = "2713200001";
				InvoiceLine.Validation.ValidateJI_EPTDigit2();
				AssertHasMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
				InvoiceLine.JI_EPTDigit2 = "X";
				AssertNoMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
			});
		}

		public void TestCheckJI_EPTDigit3()
		{
			var messageErrorInvalidCode = ListValidation.InvalidCodeMessageError;
			var targetInfo = InvoiceLine.JI_EPTDigit3Info;
			InvoiceLine.JI_EPTDigit3 = "X";
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(targetInfo, messageErrorInvalidCode);
				InvoiceLine.JI_EPTDigit3 = ContainerMaterialNumberList.Codes._1;
				AssertNoMessageErrorContaining(targetInfo, messageErrorInvalidCode);
				SetUpEnvironmentalProtectionTariff();
				var messageErrorYouHaveNotEntered = MandatoryValidation.YouHaveNotEntered;
				InvoiceLine.JI_EPTDigit3 = ZString.Empty;
				AssertNoMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
				InvoiceLine.JI_Tariff = "2713200001";
				InvoiceLine.Validation.ValidateJI_EPTDigit3();
				AssertHasMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
				InvoiceLine.JI_EPTDigit3 = "X";
				AssertNoMessageErrorContaining(targetInfo, messageErrorYouHaveNotEntered);
			});
		}

		void SetUpEnvironmentalProtectionTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.EnvironmentalProtectionTariff, "TRUE", tariff2);
			Factory.Save();
		}

		public void TestCheckJI_RAPPrice()
		{
			var targetInfo = InvoiceLine.JI_RAPPriceInfo;
			var messageValueCannotBeNegative = MandatoryValidation.ValueCannotBeNegative;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			InvoiceLine.JI_RAPPrice = -1;
			AssertHasMessageErrorContaining(targetInfo, messageValueCannotBeNegative);
			InvoiceLine.JI_RAPPrice = 0;
			AssertNoMessageErrorContaining(targetInfo, messageValueCannotBeNegative);
			InvoiceLine.JI_RAPPrice = 1;
			AssertNoMessageErrorContaining(targetInfo, messageValueCannotBeNegative);

			AssertNoMessageErrors(targetInfo);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			InvoiceLine.JI_UseOneTenthCV = false;
			var warningMessage = "the duties for the remainder of the Customs Value, excluding the ROR Price, will be calculated as a Non-Cash payment.";
			AssertHasWarningContaining(targetInfo, warningMessage);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			AssertNoWarningContaining(targetInfo, warningMessage);

			warningMessage = "and the RAP Price is 0, duties will be calculated using 0 as the base value.";
			AssertHasWarningContaining(targetInfo, warningMessage);

			InvoiceLine.JI_RAPPrice = 1;
			AssertNoWarningContaining(targetInfo, warningMessage);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			InvoiceLine.JI_UseOneTenthCV = false;
			AssertNoWarningContaining(targetInfo, warningMessage);
		}

		public void TestCheckJI_RAPCurr()
		{
			var messageInvalidCodeMessage = ListValidation.InvalidCodeMessageError;
			var messageYouHaveNotEntered = MandatoryValidation.YouHaveNotEntered;
			var targetInfo = InvoiceLine.JI_RAPCurrInfo;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._31;
			InvoiceLine.JI_RAPPrice = 2m;
			InvoiceLine.JI_RAPCurr = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "XXX";
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "TWD";
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			AssertNoMessageErrors(targetInfo);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			InvoiceLine.JI_RAPPrice = 2m;
			InvoiceLine.JI_RAPCurr = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertHasMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "XXX";
			AssertHasMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "TWD";
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			AssertNoMessageErrors(targetInfo);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3F;
			InvoiceLine.JI_RAPPrice = 2m;
			InvoiceLine.JI_RAPCurr = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertHasMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "XXX";
			AssertHasMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "TWD";
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			AssertNoMessageErrors(targetInfo);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			InvoiceLine.JI_RAPPrice = 2m;
			InvoiceLine.JI_RAPCurr = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertHasMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "XXX";
			AssertHasMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "TWD";
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			AssertNoMessageErrors(targetInfo);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
			InvoiceLine.JI_RAPPrice = 2m;
			InvoiceLine.JI_RAPCurr = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertHasMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPPrice = 0m;
			InvoiceLine.Validation.ValidateJI_RAPCurr();
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPPrice = 2m;
			InvoiceLine.JI_RAPCurr = "XXX";
			AssertHasMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			InvoiceLine.JI_RAPCurr = "TWD";
			AssertNoMessageErrorContaining(targetInfo, messageInvalidCodeMessage);
			AssertNoMessageErrorContaining(targetInfo, messageYouHaveNotEntered);
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_AntiDumpingDutyRate()
		{
			var targetInfo = InvoiceLine.JI_AntiDumpingDutyRateInfo;
			InvoiceLine.JI_AntiDumpingDutyRate = -1m;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_AntiDumpingDutyRate = ZDecimal.Zero;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_AntiDumpingDutyRate = 1m;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_CountervailingDutyRate()
		{
			var targetInfo = InvoiceLine.JI_CountervailingDutyRateInfo;
			InvoiceLine.JI_CountervailingDutyRate = -1m;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_CountervailingDutyRate = ZDecimal.Zero;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_CountervailingDutyRate = 1m;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_AdditionalDutyRate()
		{
			var targetInfo = InvoiceLine.JI_AdditionalDutyRateInfo;
			InvoiceLine.JI_AdditionalDutyRate = -1m;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_AdditionalDutyRate = ZDecimal.Zero;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_AdditionalDutyRate = 1m;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_RetaliatoryDutyRate()
		{
			var targetInfo = InvoiceLine.JI_RetaliatoryDutyRateInfo;
			InvoiceLine.JI_RetaliatoryDutyRate = -1m;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_RetaliatoryDutyRate = ZDecimal.Zero;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_RetaliatoryDutyRate = 1m;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_AlcoholPercentageWhenLinkToControllingMessageHeaderNX301_DN()
		{
			var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX301_DN";
			InvoiceLine.JI_AlcoholPercentage = 0;
			AssertNoMessageErrorContaining(InvoiceLine.JI_AlcoholPercentageInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_AlcoholPercentage = 1;
			AssertNoMessageErrorContaining(InvoiceLine.JI_AlcoholPercentageInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_AlcoholPercentage = -1;
			AssertNoMessageErrorContaining(InvoiceLine.JI_AlcoholPercentageInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			InvoiceLine.JI_AlcoholPercentage = 0;
			AssertHasMessageErrorContaining(InvoiceLine.JI_AlcoholPercentageInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_AlcoholPercentage = 1;
			AssertNoMessageErrorContaining(InvoiceLine.JI_AlcoholPercentageInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_AlcoholPercentage = -1;
			AssertNoMessageErrorContaining(InvoiceLine.JI_AlcoholPercentageInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
