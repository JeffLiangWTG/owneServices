using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerBOValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJC_ContainerNum()
		{
			container.JC_ContainerCount = -2;
			container.JC_ContainerNum = "";
			AssertHasErrors("Negative count invalid if blank container num.", container.JC_ContainerCountInfo);

			container.JC_ContainerCount = 0;
			AssertHasErrors("Error expected (blank container number and Count = 0)", container.JC_ContainerCountInfo);

			consol.Containers.Add(container);
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "XXXX9999999";
			container.JC_ContainerNum = "XXXX9999999";
			AssertHasErrors("Error expected (Duplicate)", container.JC_ContainerNumInfo);

			container.JC_ContainerMode = Constants.ContainerModes.LCL;
			container.JC_ContainerNum = "ABCD";
			container.JC_ContainerCount = 1;
			AssertHasWarnings("Warning expected (ABCD)", container.JC_ContainerNumInfo);

			container.JC_ContainerNum = "ABCD1234567";
			AssertHasWarnings("Warning expected (ABCD1234567)", container.JC_ContainerNumInfo);

			container.JC_ContainerNum = "ABCD1234560";
			AssertNoWarnings("No warnings expected (ABCD1234560)", container.JC_ContainerNumInfo);

			container.JC_ContainerMode = Constants.ContainerModes.AIR;
			container.JC_ContainerNum = "ABCD";
			AssertNoWarnings("No warnings expected (AIR Cont. Mode)", container.JC_ContainerNumInfo);

			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerNum = "ABCD1234567";
			AssertHasWarning(container.JC_ContainerNumInfo, "This is not a valid ULD number.");

			container.JC_ContainerNum = "ABCD1234XX";
			AssertNoWarnings("No warnings expected (UDL Cont. Mode)", container.JC_ContainerNumInfo);

			container.JC_ContainerCount = 1;
			container.JC_ContainerNum = "";
			AssertNoErrors("Error expected (num blank, count = 1)", container.JC_ContainerNumInfo);

			container.JC_ContainerNum = "ABCD1234567";
			AssertNoErrors("No error expected (ABCD1234567 and Count = 1)", container.JC_ContainerNumInfo);
		}

		public void TestValidateJC_ContainerNum_ForRORMode()
		{
			container.JC_ContainerCount = -2;
			container.JC_ContainerNum = "";
			AssertHasErrors("Negative count invalid if blank container num.", container.JC_ContainerCountInfo);

			container.JC_ContainerCount = 0;
			AssertHasErrors("Error expected (blank container number and Count = 0)", container.JC_ContainerCountInfo);

			consol.Containers.Add(container);
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "XXXX9999999";
			container.JC_ContainerNum = "XXXX9999999";
			AssertHasErrors("Error expected (Duplicate)", container.JC_ContainerNumInfo);

			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;

			container.JC_ContainerNum = "ABCD";
			container.JC_ContainerCount = 1;
			AssertNoWarnings("No warning expected (ABCD)", container.JC_ContainerNumInfo);

			container.JC_ContainerNum = "ABCD1234567";
			AssertNoWarnings("No warning expected (ABCD1234567)", container.JC_ContainerNumInfo);

			container.JC_ContainerNum = "ABCD1234560";
			AssertNoWarnings("No warnings expected (ABCD1234560)", container.JC_ContainerNumInfo);
		}

		public void TestValidateJC_ContainerNum_VGM()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_ContainerNum = "";
			container.Validation.ValidateJC_ContainerNum();
			AssertHasError(container.JC_ContainerNumInfo, "Container number is blank. Container number is required for gross weight verification. Enter the container number or change VGM Verification Method to NON.");

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.Validation.ValidateJC_ContainerNum();
			AssertNoError(container.JC_ContainerNumInfo, "Container number is blank. Container number is required for gross weight verification. Enter the container number or change VGM Verification Method to NON.");

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.Validation.ValidateJC_ContainerNum();
			AssertHasError(container.JC_ContainerNumInfo, "Container number is blank. Container number is required for gross weight verification. Enter the container number or change VGM Verification Method to NON.");
		}

		public void TestValidateJC_ContainerCount()
		{
			container.JC_ContainerNum = "";
			container.JC_ContainerCount = 0;
			AssertHasErrors("Error expected (Count = 0 and blank container number)", container.JC_ContainerCountInfo);

			container.JC_ContainerCount = 1;
			AssertNoErrors("No error expected (Count = 1 and blank container number)", container.JC_ContainerCountInfo);

			container.JC_ContainerNum = "ABCD1234567";
			container.JC_ContainerCount = 1;
			AssertNoErrors("No Error expected (Count = 1 and container number is entered)", container.JC_ContainerCountInfo);

			container.JC_ContainerCount = 13;
			AssertHasErrors("Error expected (container num entered, count != 1)", container.JC_ContainerCountInfo);
		}

		public void TestValidateJC_ContainerMode()
		{
			consol.JK_TransportMode = Constants.TransportModes.Air;
			container.JC_ContainerMode = "ABC";
			AssertHasErrors("Error expected", container.JC_ContainerModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			AssertNoErrors("No error expected", container.JC_ContainerModeInfo);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			container.JC_ContainerMode = Constants.ContainerModes.LCL;
			AssertNoErrors("No error expected", container.JC_ContainerModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			AssertHasErrors("Error expected", container.JC_ContainerModeInfo);
		}

		public void TestJC_ContainerMode_Air()
		{
			consol.JK_TransportMode = Constants.TransportModes.Air;
			container.JC_ContainerMode = Constants.ContainerModes.AIR;
			AssertHasErrors(container.JC_ContainerModeInfo);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var reloadedContainer = anotherFactory.Load<CommonContainer>(container.PK);

			reloadedContainer.Validation.ValidateJC_ContainerMode();
			AssertNoErrors(reloadedContainer.JC_ContainerModeInfo);

			reloadedContainer.JC_ContainerMode = Constants.ContainerModes.ULD;
			AssertNoErrors(reloadedContainer.JC_ContainerModeInfo);

			anotherFactory.Save();

			reloadedContainer.JC_ContainerMode = Constants.ContainerModes.AIR;
			AssertHasErrors(reloadedContainer.JC_ContainerModeInfo);
		}

		public void TestValidateJC_DeliveryMode()
		{
			consol.JK_TransportMode = Constants.TransportModes.Air;
			container.JC_DeliveryMode = "ABC";
			AssertHasErrors("Error expected", container.JC_DeliveryModeInfo);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;
			AssertNoErrors("No error expected", container.JC_DeliveryModeInfo);

			container.JC_DeliveryMode = "ABC";
			AssertHasErrors("Error expected", container.JC_DeliveryModeInfo);
		}

		public void TestValidateContainerModeAgainstConsolMode()
		{
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			container.JC_ContainerMode = Constants.ContainerModes.AIR;
			AssertNoWarnings("Consol AIR/LSE, Container AIR. No warning", container.JC_ContainerModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			AssertNoWarnings("Consol AIR/LSE, Container ULD. No Warning.", container.JC_ContainerModeInfo);

			consol.JK_ConsolMode = Constants.ContainerModes.ULD;
			container.Validation.ValidateJC_ContainerMode();
			AssertNoWarnings("Consol ULD, Container ULD. No warning.", container.JC_ContainerModeInfo);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			container.JC_ContainerMode = Constants.ContainerModes.LCL;
			AssertNoWarnings("Consol SEA/BLK. No warning.", container.JC_ContainerModeInfo);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			container.Validation.ValidateJC_ContainerMode();
			AssertHasWarnings("Consol FCL. Container LCL. Warning.", container.JC_ContainerModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertNoWarnings("Consol FCL. Container LCL. No warning.", container.JC_ContainerModeInfo);
		}

		public void TestJC_VehicleTransmission()
		{
			container.JC_VehicleTransmission = Constants.VehicleTransmissionType.Automatic;
			AssertNoErrors(container.JC_VehicleTransmissionInfo);

			container.JC_VehicleTransmission = ZString.Empty;
			AssertNoErrors(container.JC_VehicleTransmissionInfo);

			container.JC_VehicleTransmission = "XX";
			AssertHasErrors(container.JC_VehicleTransmissionInfo);
		}

		public void TestJC_RX_NKGoodsCurrency()
		{
			container.JC_RX_NKGoodsCurrency = Constants.CurrencyCodes.Australia;
			AssertNoErrors(container.JC_RX_NKGoodsCurrencyInfo);

			container.JC_RX_NKGoodsCurrency = ZString.Empty;
			AssertNoErrors(container.JC_RX_NKGoodsCurrencyInfo);

			container.JC_RX_NKGoodsCurrency = "XX1";
			AssertHasErrors(container.JC_RX_NKGoodsCurrencyInfo);
		}

		public void TestValidateIsNotRemovedByCarrier()
		{
			var containerNumberLoopIndex = 100;
			container.JC_ContainerNum = containerNumberLoopIndex.ToString();

			var declaration = (EnterpriseBusinessObject)Factory.New<IBaseJobDeclaration>();
			var cusContainer = (EnterpriseBusinessObject)Factory.New<Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC] = container.PK;

			var parameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>("DEP", "Carrier"),
				new KeyValuePair<string, string>("EQN", container.JC_ContainerNum),
				new KeyValuePair<string, string>("TYP", "Container Number removed from the Booking"),
			};

			Factory.Save();

			foreach (var containerParent in new[] { consol, declaration })
			{
				AssertNoRowWarnings("Pre-condition - should have no row errors", containerParent);

				parameters[1] = new KeyValuePair<string, string>("EQN", "Some other container number");

				AddLogAndValidate("Only logs matching container number should be considered", containerParent, parameters, false);

				parameters[1] = new KeyValuePair<string, string>("EQN", container.JC_ContainerNum);
				parameters[2] = new KeyValuePair<string, string>("TYP", "Some other event type");

				AddLogAndValidate("Only logs of the correct type should be considered", containerParent, parameters, false);

				parameters[0] = new KeyValuePair<string, string>("DEP", "Some other department");
				parameters[2] = new KeyValuePair<string, string>("TYP", "Container Number removed from the Booking");

				AddLogAndValidate("Only logs created by carrier should be considered", containerParent, parameters, false);

				parameters[0] = new KeyValuePair<string, string>("DEP", "Carrier");

				AddLogAndValidate("Since a removal log exists on the parent matching the container number, it should have a warning", containerParent, parameters, true);

				container.ClearRowNotifications();

				containerNumberLoopIndex++;
				container.JC_ContainerNum = containerNumberLoopIndex.ToString();
			}
		}

		public void TestJC_Calc_DepartureContainerYardAddressOrg()
		{
			container.JC_Calc_DepartureContainerYardAddressOrg = ZGuid.Invalid;
			AssertHasError(container.JC_Calc_DepartureContainerYardAddressOrgInfo, "Enter a valid selection.");
		}

		public void TestJC_Calc_ArrivalContainerYardAddressOrg()
		{
			container.JC_Calc_ArrivalContainerYardAddressOrg = ZGuid.Invalid;
			AssertHasError(container.JC_Calc_ArrivalContainerYardAddressOrgInfo, "Enter a valid selection.");
		}

		public void TestJC_Calc_ArrivalUnpackAddressOrg()
		{
			container.JC_Calc_ArrivalUnpackAddressOrg = ZGuid.Invalid;
			AssertHasError(container.JC_Calc_ArrivalUnpackAddressOrgInfo, "Enter a valid selection.");
		}

		public void TestJC_Calc_DeparturePackAddressOrg()
		{
			container.JC_Calc_DeparturePackAddressOrg = ZGuid.Invalid;
			AssertHasError(container.JC_Calc_DeparturePackAddressOrgInfo, "Enter a valid selection.");
		}

		public void TestJC_Calc_DepartureCTOAddressOrg()
		{
			container.JC_Calc_DepartureCTOAddressOrg = ZGuid.Invalid;
			AssertHasError(container.JC_Calc_DepartureCTOAddressOrgInfo, "Enter a valid selection.");
		}

		public void TestJC_Calc_ArrivalCTOAddressOrg()
		{
			container.JC_Calc_ArrivalCTOAddressOrg = ZGuid.Invalid;
			AssertHasError(container.JC_Calc_ArrivalCTOAddressOrgInfo, "Enter a valid selection.");
		}

		public void TestJC_TareWeight()
		{
			container.JC_TareWeight = 0;
			AssertNoErrors("Should allow zero value", container.JC_TareWeightInfo);

			container.JC_TareWeight = -1;
			AssertHasError("Should forbid negative value", container.JC_TareWeightInfo, "Tare Weight cannot be less than 0.");
		}

		void AddLogAndValidate(string message, EnterpriseBusinessObject containerParent, KeyValuePair<string, string>[] parametres, bool shouldHaveWarnings)
		{
			var notDetectedMessage = "Carrier removed this container from the booking.";
			containerParent.Logs.AddNew(Events.StatusUpdated, parametres);
			container.Validation.ValidateAll();

			if (shouldHaveWarnings)
			{
				AssertHasRowWarning(message, container, notDetectedMessage);
			}
			else
			{
				AssertNoRowWarningContaining(message, container, notDetectedMessage);
			}
		}

		#region Implementation

		CommonContainer container;
		CommonConsol consol;

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<CommonConsol>();
			container = consol.Containers.AddNew();
		}

		#endregion
	}
}
