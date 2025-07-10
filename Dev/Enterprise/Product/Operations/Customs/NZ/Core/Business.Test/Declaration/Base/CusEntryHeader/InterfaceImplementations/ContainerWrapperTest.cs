using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	sealed class ContainerWrapperTest : TestCaseWithFactory
	{
		public void TestContainerWrapper()
		{
			var container = Factory.NewWithValidTestData<CusContainer>();
			container.CO_ContainerNumber = "HKFU0029385";
			container.CO_ContainerSize = ContainerSizeTypeList.Codes.C40;
			container.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C40;
			container.CO_ContainerUQ = "TNE";
			container.CO_Seal = "F32904TR";
			container.CO_SecondSeal = "27389Z";
			container.CO_FCL_LCL_AIR = "BLK";

			Declaration.JE_HouseBill = "TESTBILL123";
			var packingGroup = Declaration.Bills[0].PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;

			var wrappedContainer = new ContainerWrapper(packingGroup);
			AssertEquals("ContainerNumber", "HKFU0029385", wrappedContainer.ContainerNumber);
			AssertEquals("ContainerMode", "", wrappedContainer.ContainerMode);
			AssertEquals("ContainerSize", "40", wrappedContainer.Size);
			AssertEquals("ContainerStatus", ContainerStatusList.Codes.C8, wrappedContainer.Status);
			var sealNumbers = ZString.Empty;
			foreach (ZString sealNo in wrappedContainer.SealNumbers)
			{
				sealNumbers += sealNo;
			}
			AssertEquals("ContainerSealsNumbers", "F32904TR27389Z", sealNumbers);
			AssertEquals("IsPallet", false, wrappedContainer.IsPallet);
		}

		public void TestStowPosition()
		{
			var container = Factory.NewWithValidTestData<CusContainer>();
			container.CO_ContainerNumber = "HKFU0029385";
			container.CO_ContainerSize = ContainerSizeTypeList.Codes.C40;

			Declaration.JE_HouseBill = "TESTBILL123";
			var packingGroup = Declaration.Bills[0].PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;

			var wrappedContainer = new ContainerWrapper(packingGroup);
			AssertEquals("default when no forwarding container attached", ZString.Empty, wrappedContainer.StowPosition);

			var jobContainer = Factory.NewWithValidTestData<CommonContainer>();
			jobContainer.JC_StowagePosition = "120319";
			container.CO_JC = jobContainer.PK;
			wrappedContainer = new ContainerWrapper(packingGroup);
			AssertEquals("Stowage position should now be picked up from dbo.JobContainer value", "0120319", wrappedContainer.StowPosition);
		}

		public void TestContainerIsPallet()
		{
			var container = Factory.NewWithValidTestData<CusContainer>();
			container.CO_ContainerNumber = "P12";

			Declaration.JE_HouseBill = "TESTBILL123";
			var packingGroup = Declaration.Bills[0].PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;

			var wrappedContainer = new ContainerWrapper(packingGroup);
			AssertEquals("IsPallet", true, wrappedContainer.IsPallet);
		}

		public void TestRelatedPackages()
		{
			var container = Factory.NewWithValidTestData<CusContainer>();
			container.CO_ContainerNumber = "HKFU0029385";
			container.CO_ContainerSize = ContainerSizeTypeList.Codes.C40;

			Declaration.JE_HouseBill = "TESTBILL123";
			var packingGroup = Declaration.Bills[0].PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			var package1 = packingGroup.Packages.AddNew();
			package1.CW_PackType = "CT";
			var package2 = packingGroup.Packages.AddNew();
			package2.CW_PackType = "BX";

			var wrappedContainer = new ContainerWrapper(packingGroup);
			AssertEquals("RelatedPackages", true, wrappedContainer.RelatedPackages.IsCountEqualTo(2));
		}

		public void TestContainerSealingParty()
		{
			var container = Factory.NewWithValidTestData<CusContainer>();
			container.CO_ContainerNumber = "YKKU7584641";
			container.CO_ContainerSize = ContainerSizeTypeList.Codes.C40;
			container.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C40;
			container.CO_ContainerUQ = "TNE";
			container.CO_Seal = "F32904TR";
			container.CO_SecondSeal = "27389Z";
			container.CO_FCL_LCL_AIR = "BLK";
			container.CO_SealingParty = "SECURE FREIGHT PTY. LTD.";

			Declaration.JE_HouseBill = "TESTBILL123";
			var packingGroup = Declaration.Bills[0].PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;

			var wrappedContainer = new ContainerWrapper(packingGroup);
			AssertEquals("SealingParty", "SECURE FREIGHT PTY. LTD.", wrappedContainer.SealingParty);
		}

		#region Implementation

		public JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.NewWithValidTestData<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		#endregion
	}
}
