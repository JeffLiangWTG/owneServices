using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USDeliveryOrderContainerAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ContainerNumber()
		{
			Header.DeliveryOrderContainers.RemoveAndDeleteAll();
			container = null;
			CusContainer cusContainer = Declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "TURE234322";
			Container.US_ContainerNumber = "~";
			AssertHasWarningContaining(Container.US_ContainerNumberInfo, USDeliveryOrderContainerAddInfoValidation.ContainerNumberShouldBeInList);
			Container.US_ContainerNumber = "TURE234322";
			AssertNoWarningContaining(Container.US_ContainerNumberInfo, USDeliveryOrderContainerAddInfoValidation.ContainerNumberShouldBeInList);
			AssertNoWarningContaining(Container.US_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
			Container.US_ContainerNumber = ZString.Empty;
			AssertHasWarningContaining(Container.US_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ContainerType()
		{
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZ43";
			Container.US_ContainerType = "~";
			AssertHasWarningContaining(Container.US_ContainerTypeInfo, USDeliveryOrderContainerAddInfoValidation.ContainerTypeShouldBeInList);
			Container.US_ContainerType = "ZZ43";
			AssertNoWarningContaining(Container.US_ContainerTypeInfo, USDeliveryOrderContainerAddInfoValidation.ContainerTypeShouldBeInList);
		}

		public void TestCheckUS_ContainerMode()
		{
			Container.US_ContainerMode = "~";
			AssertHasWarningContaining(Container.US_ContainerModeInfo, USDeliveryOrderContainerAddInfoValidation.ContainerModeShouldBeInList);
			foreach (CodeDescriptionPair pair in Container.AddInfoLookups.ContainerModeList)
			{
				Container.US_ContainerMode = pair.Code;
				AssertNoWarningContaining(Container.US_ContainerModeInfo, USDeliveryOrderContainerAddInfoValidation.ContainerModeShouldBeInList);
			}
		}

		public void TestCheckUS_PackageType()
		{
			Container.US_PackageType = "~";
			AssertHasWarningContaining(Container.US_PackageTypeInfo, USDeliveryOrderContainerAddInfoValidation.PackageTypeShouldBeInList);
			foreach (CodeDescriptionPair pair in Container.AddInfoLookups.PackageTypeList)
			{
				Container.US_PackageType = pair.Code;
				AssertNoWarningContaining(Container.US_PackageTypeInfo, USDeliveryOrderContainerAddInfoValidation.PackageTypeShouldBeInList);
			}

			Container.US_NoOfPackages = 10;
			Container.US_PackageType = ZString.Empty;
			AssertHasWarningContaining(Container.US_PackageTypeInfo, USDeliveryOrderContainerAddInfoValidation.PackageTypeShouldBeEnteredIfValueIsEntered);
			Container.US_NoOfPackages = 0;
			AssertNoWarningContaining(Container.US_PackageTypeInfo, USDeliveryOrderContainerAddInfoValidation.PackageTypeShouldBeEnteredIfValueIsEntered);
		}

		public void TestCheckUS_WeightUQ()
		{
			Container.US_WeightUQ = "~";
			AssertHasWarningContaining(Container.US_WeightUQInfo, USDeliveryOrderContainerAddInfoValidation.WeightUQShouldBeInList);
			foreach (CodeDescriptionPair pair in Container.AddInfoLookups.WeightUQList)
			{
				Container.US_WeightUQ = pair.Code;
				AssertNoWarningContaining(Container.US_WeightUQInfo, USDeliveryOrderContainerAddInfoValidation.WeightUQShouldBeInList);
			}

			Container.US_Weight = 10m;
			Container.US_WeightUQ = ZString.Empty;
			AssertHasWarningContaining(Container.US_WeightUQInfo, USDeliveryOrderContainerAddInfoValidation.WeightUQShouldBeEnteredIfWeightIsEntered);
			Container.US_Weight = 0m;
			AssertNoWarningContaining(Container.US_WeightUQInfo, USDeliveryOrderContainerAddInfoValidation.WeightUQShouldBeEnteredIfWeightIsEntered);
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		DeliveryOrderHeader header;
		DeliveryOrderHeader Header => header ?? (header = Declaration.DeliveryOrderHeaders.AddNew());

		DeliveryOrderContainer container;
		DeliveryOrderContainer Container => container ?? (container = Header.DeliveryOrderContainers.AddNew());
	}
}
