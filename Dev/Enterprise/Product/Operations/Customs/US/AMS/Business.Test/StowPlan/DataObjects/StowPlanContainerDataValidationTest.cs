using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class StowPlanContainerDataValidationTest : TestCaseWithFactory
	{
		public void TestNoHazardCodes()
		{
			containerData.Validation.ValidateAll();
			AssertHasRowWarning(containerData, StowPlanContainerDataValidation.NoHazardCodes);
			var packLine = bill.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;
			packLine.UNDGs.UNDGSubstanceManagerGuid.Value = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			containerData.Validation.ValidateAll();
			AssertNoRowWarningContaining(containerData, StowPlanContainerDataValidation.NoHazardCodes);
		}

		public void TestNoContainerReferenceMsg()
		{
			container.JC_ContainerNum = "APLU1223";
			containerData.Validation.ValidateAll();
			AssertHasRowMessageError(containerData, string.Format(StowPlanContainerDataValidation.NoContainerReferenceMsg, "APLU1223"));
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "APLU1223";
			containerData.Validation.ValidateAll();
			AssertNoRowMessageError(containerData, string.Format(StowPlanContainerDataValidation.NoContainerReferenceMsg, "APLU1223"));
		}

		public void TestCheckEquipmentNumber()
		{
			containerData.Validation.ValidateEquipmentNumber();
			AssertHasMessageErrorContaining(containerData.EquipmentNumberInfo, MandatoryValidation.YouHaveNotEntered);
			container.JC_ContainerNum = "APLU1234";
			containerData.Validation.ValidateEquipmentNumber();
			AssertNoMessageErrorContaining(containerData.EquipmentNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckStowPosition()
		{
			containerData.Validation.ValidateStowPosition();
			AssertHasMessageErrorContaining(containerData.StowPositionInfo, MandatoryValidation.YouHaveNotEntered);
			container.JC_StowagePosition = "ZZZ11222";
			containerData.Validation.ValidateStowPosition();
			AssertNoMessageErrorContaining(containerData.StowPositionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckGrossWeightInKG()
		{
			containerData.Validation.ValidateGrossWeightInKG();
			AssertHasMessageErrorContaining(containerData.GrossWeightInKGInfo, MandatoryValidation.YouHaveNotEntered);
			container.JC_GrossWeight = 24m;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			containerData.Validation.ValidateGrossWeightInKG();
			AssertNoMessageErrorContaining(containerData.StowPositionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		StowPlanContainerData containerData;
		BillOfLadingContainer container;
		BillOfLading bill;
		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.New<BillOfLading>();
			container = bill.RealContainers.AddNew();
			containerData = new StowPlanContainerData(container);
		}
	}
}
