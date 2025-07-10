using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public void TestParent()
		{
			AssertEquals(Validation.Parent, CusContainer);
		}

		public void TestContainerNumber()
		{
			Validation.ValidateCO_ContainerNumber();
			AssertEquals(true, CusContainer.CO_ContainerNumberInfo.HasMessageErrors());
			CusContainer.CO_ContainerNumber = "123456";
			Validation.ValidateCO_ContainerNumber();
			AssertEquals(false, CusContainer.CO_ContainerNumberInfo.HasMessageErrors());
		}

		public override void TestAirContainerIsMessageError()
		{
			Assert(true);
		}

		public void TestContainerOnAirJobDoesNotMessageError()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			AssertEquals(true, declaration.ContainersAlwaysRequired);
			AssertNoMessageErrors(container.CO_ContainerNumberInfo);
		}

		public void TestContainerMode()
		{
			Validation.ValidateCO_FCL_LCL_AIR();
			AssertEquals(true, CusContainer.CO_FCL_LCL_AIRInfo.HasMessageErrors());
			CusContainer.CO_FCL_LCL_AIR = ContainerTypeCodeList.Codes.FCL;
			Validation.ValidateCO_FCL_LCL_AIR();
			AssertEquals(false, CusContainer.CO_FCL_LCL_AIRInfo.HasMessageErrors());
			CusContainer.CO_FCL_LCL_AIR = ContainerTypeCodeList.Codes.LCL;
			Validation.ValidateCO_FCL_LCL_AIR();
			AssertEquals(false, CusContainer.CO_FCL_LCL_AIRInfo.HasMessageErrors());
			CusContainer.CO_FCL_LCL_AIR = "AIR";
			Validation.ValidateCO_FCL_LCL_AIR();
			AssertEquals(true, CusContainer.CO_FCL_LCL_AIRInfo.HasMessageErrors());
		}

		public void TestContainerSize()
		{
			Validation.ValidateCO_ContainerSize();
			AssertEquals(true, CusContainer.CO_ContainerSizeInfo.HasMessageErrors());
			CusContainer.CO_ContainerSize = "0";
			Validation.ValidateCO_ContainerSize();
			AssertEquals(true, CusContainer.CO_ContainerSizeInfo.HasMessageErrors());
			CusContainer.CO_ContainerSize = "15";
			Validation.ValidateCO_ContainerSize();
			AssertEquals(true, CusContainer.CO_ContainerSizeInfo.HasMessageErrors());
			CusContainer.CO_ContainerSize = "20";
			Validation.ValidateCO_ContainerSize();
			AssertEquals(false, CusContainer.CO_ContainerSizeInfo.HasMessageErrors());
		}

		public void TestContainerWeight()
		{
			Validation.ValidateCO_Weight();
			AssertEquals("Weight will always be sent with minimum value of 1 if not entered", true, CusContainer.CO_WeightInfo.HasWarning("Container Weight has not been entered."));
			CusContainer.CO_Weight = -1m;
			Validation.ValidateCO_Weight();
			AssertEquals(true, CusContainer.CO_WeightInfo.HasWarning("Container Weight has not been entered."));
			CusContainer.CO_Weight = 1m;
			Validation.ValidateCO_Weight();
			AssertEquals(false, CusContainer.CO_WeightInfo.HasWarning("Container Weight has not been entered."));
		}

		public void TestContainerWeightUQ()
		{
			CusContainer.CO_WeightUQ = "";
			Validation.ValidateCO_WeightUQ();
			AssertEquals(true, CusContainer.CO_WeightUQInfo.HasMessageErrors());
			CusContainer.CO_WeightUQ = "KG";
			Validation.ValidateCO_WeightUQ();
			AssertEquals(false, CusContainer.CO_WeightUQInfo.HasMessageErrors());
		}

		#region Implementation
		CusContainer CusContainer
		{
			get
			{
				return cusContainer ?? (cusContainer = Factory.New<CusContainer>());
			}
		}

		CusContainer cusContainer;
		CusContainerValidation Validation
		{
			get
			{
				return CusContainer.Validation;
			}
		}
		#endregion
	}
}
