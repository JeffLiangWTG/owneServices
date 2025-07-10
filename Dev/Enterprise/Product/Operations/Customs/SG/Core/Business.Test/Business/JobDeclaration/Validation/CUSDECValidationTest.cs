using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class CUSDECValidationTest : JobDeclarationValidationTest
	{
		public abstract void TestMessageSubType();
		public void TestCargoPackingType()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_ContainerMode = "";
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(true, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType1;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType2;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType3;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType4;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = "A";
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(true, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType3;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Code values for TN4 should error if change to TN4.1", true, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType5;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("TN4.1 Code values", false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType9;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("TN4.1 Code values", false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Code values for TN4.1 should error if changed back to TN4", true, Declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public void TestCargoPackingTypeIsValidWhenContainersAreEntered()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType2;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.CusContainers.AddNew();
			AssertEquals("Precondition - container added", true, Declaration.JE_ContainerCount == 1);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType2;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(true, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			AssertEquals(true, Declaration.JE_ContainerModeInfo.HasMessageError("Cargo Packing Type must be 3 if containers have been entered"));
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType3;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Packing Code is different for TN4.1 - TN4.0 container code should error here", true, Declaration.JE_ContainerModeInfo.HasMessageErrors());
			AssertEquals(true, Declaration.JE_ContainerModeInfo.HasMessageError("Cargo Packing Type must be 9 if containers have been entered"));
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType9;
			Declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals(false, Declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public void TestContainerCount()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType2;
			Declaration.Validation.ValidateJE_ContainerCount();
			AssertEquals(false, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType3;
			Declaration.Validation.ValidateJE_ContainerCount();
			AssertEquals(true, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			AssertEquals(true, Declaration.JE_ContainerCountInfo.HasMessageError("Total number of containers should be greater than 0 for Containerised Cargo."));
			Declaration.CusContainers.AddNew();
			Declaration.Validation.ValidateJE_ContainerCount();
			AssertEquals(false, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			for (int i = 0; i < 99; i++)
			{
				Declaration.CusContainers.AddNew();
			}

			Declaration.Validation.ValidateJE_ContainerCount();
			AssertEquals(true, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			AssertEquals(true, Declaration.JE_ContainerCountInfo.HasMessageError("Total number of containers must be less than 100. Singapore Customs messaging only allows for upto 99 containers to be sent in any 1 message."));
		}

		public void TestContainerCountTN41()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType5;
			Declaration.Validation.ValidateJE_ContainerCount();
			AssertEquals(false, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType9;
			Declaration.Validation.ValidateJE_ContainerCount();
			AssertEquals(true, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			AssertEquals(true, Declaration.JE_ContainerCountInfo.HasMessageError("Total number of containers should be greater than 0 for Containerised Cargo."));
			Declaration.CusContainers.AddNew();
			Declaration.Validation.ValidateJE_ContainerCount();
			AssertEquals(false, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			for (int i = 0; i < 99; i++)
			{
				Declaration.CusContainers.AddNew();
			}

			Declaration.Validation.ValidateJE_ContainerCount();
			AssertEquals(true, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			AssertEquals(true, Declaration.JE_ContainerCountInfo.HasMessageError("Total number of containers must be less than 100. Singapore Customs messaging only allows for upto 99 containers to be sent in any 1 message."));
		}

		public void TestJE_TotalNoOfPacksDecimal()
		{
			Declaration.JE_TotalNoOfPacksDecimal = -2;
			Declaration.Validation.ValidateJE_TotalNoOfPacksDecimal();
			AssertEquals(true, Declaration.JE_TotalNoOfPacksDecimalInfo.HasErrors());
			Declaration.JE_TotalNoOfPacksDecimal = 0;
			Declaration.Validation.ValidateJE_TotalNoOfPacksDecimal();
			AssertEquals(false, Declaration.JE_TotalNoOfPacksDecimalInfo.HasErrors());
			AssertEquals(true, Declaration.JE_TotalNoOfPacksDecimalInfo.HasMessageErrors());
			Declaration.JE_TotalNoOfPacksDecimal = 3;
			Declaration.Validation.ValidateJE_TotalNoOfPacksDecimal();
			AssertEquals(false, Declaration.JE_TotalNoOfPacksDecimalInfo.HasMessageErrors());
			Declaration.JE_TotalNoOfPacksDecimal = 145995000;
			Declaration.Validation.ValidateJE_TotalNoOfPacksDecimal();
			AssertEquals("Maximum number of packs value allowed in message exceeded", true, Declaration.JE_TotalNoOfPacksDecimalInfo.HasErrors());
			Declaration.JE_TotalNoOfPacksDecimal = 95995000;
			Declaration.Validation.ValidateJE_TotalNoOfPacksDecimal();
			AssertEquals(false, Declaration.JE_TotalNoOfPacksDecimalInfo.HasErrors());
		}

		public void TestJE_TotalNoOfPacksPackType()
		{
			Declaration.JE_TotalNoOfPacksPackType = "";
			Declaration.Validation.ValidateJE_TotalNoOfPacksPackType();
			AssertEquals(true, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());
			Declaration.JE_TotalNoOfPacksPackType = UnitOfQuantityCodeList.Codes.BAG;
			AssertEquals(false, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());
			Declaration.JE_TotalNoOfPacksPackType = "PTE";
			AssertEquals(true, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());
			Declaration.JE_TotalNoOfPacksPackType = UnitOfQuantityCodeList.Codes.PAT;
			AssertEquals(false, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());
		}

		public void TestJE_TotalWeight()
		{
			Declaration.JE_TotalWeight = 0;
			Declaration.Validation.ValidateJE_TotalWeight();
			AssertEquals(true, Declaration.JE_TotalWeightInfo.HasMessageErrors());
			Declaration.JE_TotalWeight = 3;
			Declaration.Validation.ValidateJE_TotalWeight();
			AssertEquals(false, Declaration.JE_TotalWeightInfo.HasMessageErrors());
		}

		public void TestJE_TotalWeightUnit()
		{
			Declaration.JE_TotalWeightUnit = "";
			Declaration.Validation.ValidateJE_TotalWeightUnit();
			AssertEquals(true, Declaration.JE_TotalWeightUnitInfo.HasMessageErrors());
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(false, Declaration.JE_TotalWeightUnitInfo.HasMessageErrors());
		}

		public void TestPortOfLoading()
		{
			Declaration.Lookups.SGLocoList.Load();
			Declaration.JE_TransportMode = "";
			Declaration.JE_RL_NKPortOfLoading = "";
			Declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertEquals(false, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertEquals(true, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
			Declaration.JE_MessageType = "";
			Declaration.JE_RL_NKPortOfLoading = "ZACPT";
			Declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertEquals(false, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
		}

		public void TestForwarder()
		{
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			Declaration.JE_HouseBill = "HB-001";
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(true, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			OrgHeader forwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			Declaration.JE_HouseBill = "";
			Declaration.SG_OutwardHAWB = "OUT-HB";
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertEquals(true, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			Declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Declaration.SG_OutwardHAWB = "";
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			invoiceLine.SG_OutwardHAWB = "TEST";
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(true, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			invoiceLine.SG_OutwardHAWB = "";
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			invoiceLine.SG_InwardHAWB = "TEST";
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(true, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			Declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			AddInfoCUSDECValidationTest.CreatePortCusCodeList(helper, "ZACPT", "CAPETOWN");
			Factory.Save();
		}
	}
}
