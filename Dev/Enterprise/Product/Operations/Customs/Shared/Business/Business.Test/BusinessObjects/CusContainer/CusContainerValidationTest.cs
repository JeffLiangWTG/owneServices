using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusContainerValidationBaseOnlyTest : CusContainerValidationTest<BaseJobDeclaration>
	{
	}

	public abstract class CusContainerValidationTest<TJobDeclaration> : BusinessObjectValidationTestCase
		where TJobDeclaration : BaseJobDeclaration
	{
		public virtual void TestContainersRequirePackagesValidation()
		{
			var mockDeclaration = Factory.NewMoq<TJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			var declaration = mockDeclaration.Object;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HouseBill";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			AssertEquals("HasMessageError(CusContainerValidation.ContainersRequirePackages)", true, container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));

			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_HouseBill;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("HasMessageError(CusContainerValidation.ContainersRequirePackages)", false, container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));

			package.CW_ContainerNoOrEquipmentNo = "";
			AssertEquals("HasMessageError(CusContainerValidation.ContainersRequirePackages)", true, container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));
		}

		public void TestNonAlphanumericCharacterInContainerNumberIsMessageError()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			AssertEquals("HasMessageError(CusContainerValidation.MustOnlyContainAlphaNumerics)", false, container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.MustOnlyContainAlphaNumerics));
			container.CO_ContainerNumber = "CRXU1234567*";
			AssertEquals("HasMessageError(CusContainerValidation.MustOnlyContainAlphaNumerics)", true, container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.MustOnlyContainAlphaNumerics));
		}

		public void TestDuplicateContainerIsError()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			BaseCusContainer container2 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRXU1234568";
			container2.CO_ContainerNumber = "CRXU1234568";
			AssertHasError(container2.CO_ContainerNumberInfo, CusContainerValidation.DuplicateContainerNumber);
		}

		public virtual void TestAirContainerIsMessageError()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			AssertEquals(false, declaration.ContainersAlwaysRequired);
			AssertHasMessageError(container.CO_ContainerNumberInfo, CusContainerValidation.CannotHaveContainersOnAirJob);
		}

		public void TestInvalidContainerNumberIsWarning()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234561";
			container.CO_FCL_LCL_AIR = "FCL";
			AssertHasWarningContaining(container.CO_ContainerNumberInfo, "Container number does not have a valid check (last) digit. The check digit should be");
			container.CO_ContainerNumber = "ABC12345DE";
			AssertHasWarning(container.CO_ContainerNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			container.CO_FCL_LCL_AIR = "ULD";
			container.CO_ContainerNumber = "ABC12345DF";
			AssertNoWarningContaining(container.CO_ContainerNumberInfo, "Container number does not have a valid check (last) digit. The check digit should be");
			AssertNoWarning(container.CO_ContainerNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
		}

		public void TestValidationProxying()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			ForwardingContainer forwardingContainer = consol.Containers.AddNew();
			forwardingContainer.JC_ContainerNum = "123";
			ForwardingPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.Containers.Add(forwardingContainer);

			forwardingContainer.JC_ContainerMode = "XXX";
			forwardingContainer.JC_RC = ZGuid.Invalid;

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_JS = shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			BaseCusContainer customsContainer = declaration.CusContainers.AddNew();
			customsContainer.CO_ContainerNumber = "123";    // performs auto linkup
			AssertEquals("Precondition", forwardingContainer, customsContainer.JobContainer);

			customsContainer.RunPreSaveValidation();

			CheckProxy(forwardingContainer.JC_ContainerNumInfo, customsContainer.CO_ContainerNumberInfo);
			CheckProxy(forwardingContainer.JC_ContainerModeInfo, customsContainer.CO_FCL_LCL_AIRInfo);
			CheckProxy(forwardingContainer.JC_SealNumInfo, customsContainer.CO_SealInfo);
			CheckProxy(forwardingContainer.JC_AdditionalSealNumInfo, customsContainer.CO_SecondSealInfo);
			CheckProxy(forwardingContainer.JC_RCInfo, customsContainer.CO_RCInfo);
		}

		public void TestCheckCO_RC()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			RefContainer masterContainer = Factory.New<RefContainer>();
			masterContainer.SetCountrySpecificContainerCode("R", Enterprise.Core.Constants.CountryCodes.UnitedStates);

			CommonContainer testContainer = Factory.New<CommonContainer>();
			testContainer.JC_RC = masterContainer.PK;
			AssertNoErrors(testContainer.JC_RCInfo);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_RC = masterContainer.PK;
			container.CO_ContainerNumber = "CRXU1234568";
			container.CO_RC = ZGuid.Empty;

			AssertNoErrors(container.CO_RCInfo);
			AssertHasWarningContaining(container.CO_RCInfo, MandatoryValidation.YouHaveNotEntered);

			container.CO_RC = masterContainer.PK;
			AssertNoErrors(container.CO_RCInfo);
			AssertNoWarningContaining(container.CO_RCInfo, MandatoryValidation.YouHaveNotEntered);

			RefContainer uLD = RefContainer.New(Factory);
			uLD.RC_ShippingMode = "AIR";
			uLD.RC_IATARateClass = "ZZ1";
			uLD.SetCountrySpecificContainerCode("R1", Enterprise.Core.Constants.CountryCodes.China);

			RefContainer seaContainer = RefContainer.New(Factory);
			seaContainer.RC_ShippingMode = "SEA";
			seaContainer.SetCountrySpecificContainerCode("R2", Enterprise.Core.Constants.CountryCodes.China);

			testContainer.JC_RC = seaContainer.PK;
			container.CO_JC = testContainer.PK;
			container.CO_RC = seaContainer.PK;
			AssertNoNotifications(testContainer.JC_RCInfo);
			AssertNoNotifications(container.CO_RCInfo);

			testContainer.JC_RC = uLD.PK;
			container.CO_RC = uLD.PK;
			AssertNoNotifications(testContainer.JC_RCInfo);
			AssertNoNotifications(container.CO_RCInfo);

			testContainer.JC_ContainerMode = "FCL";
			testContainer.JC_RC = uLD.PK;
			AssertHasErrors(testContainer.JC_RCInfo);
			container.Validation.ValidateCO_RC();
			AssertHasErrors(container.CO_RCInfo);

			testContainer.JC_RC = seaContainer.PK;
			AssertNoNotifications(testContainer.JC_RCInfo);
			container.Validation.ValidateCO_RC();
			AssertNoNotifications(container.CO_RCInfo);

			testContainer.JC_ContainerMode = "ULD";
			testContainer.JC_RC = seaContainer.PK;
			AssertHasErrors(testContainer.JC_RCInfo);
			container.Validation.ValidateCO_RC();
			AssertHasErrors(container.CO_RCInfo);

			testContainer.JC_RC = uLD.PK;
			AssertNoNotifications(testContainer.JC_RCInfo);
			container.Validation.ValidateCO_RC();
			AssertNoNotifications(container.CO_RCInfo);
		}

		void CheckProxy(ZPropertyInfo source, ZPropertyInfo destination)
		{
			foreach (INotification notification in source.GetErrors())
			{
				Assert(destination.HasError(notification.Message));
			}
			foreach (INotification notification in source.GetMessageErrors())
			{
				Assert(destination.HasMessageError(notification.Message));
			}
			foreach (INotification notification in source.GetWarnings())
			{
				Assert(destination.HasWarning(notification.Message));
			}
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}
	}
}
