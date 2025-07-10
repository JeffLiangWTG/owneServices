using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.Business.Testing
{
	public class CusContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryList()
		{
			AssertType<RefCountryCollection>(GetCusContainerLookups().CountryList);
		}

		public virtual void TestCO_FCL_LCL_NCT_List()
		{
			CusContainerLookups lookups = GetCusContainerLookups();
			AssertNotNull("Lookups.CO_FCL_LCL_NCT_List", lookups.CO_FCL_LCL_NCT_List);
			Assert("CO_FCL_LCL_NCT_List.ContainsCode(BaseCusContainer.ContainerModes.FullContainerLoad)", lookups.CO_FCL_LCL_NCT_List.ContainsCode(BaseCusContainer.ContainerModes.FullContainerLoad));
			Assert("CO_FCL_LCL_NCT_List.ContainsCode(BaseCusContainer.ContainerModes.LessContainerLoad)", lookups.CO_FCL_LCL_NCT_List.ContainsCode(BaseCusContainer.ContainerModes.LessContainerLoad));
			Assert("CO_FCL_LCL_NCT_List.ContainsCode(BaseCusContainer.ContainerModes.FCX)", lookups.CO_FCL_LCL_NCT_List.ContainsCode(BaseCusContainer.ContainerModes.FCX));
		}

		public virtual void TestTotalPackagesUnit_List()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("BLAH", "HELLO BLAH");
			var mockPackage = Factory.NewMoq<BasePackage>();
			mockPackage.Setup(m => m.PackTypeList).Returns(list);
			var package = mockPackage.Object;
			var dec = Factory.New<BaseJobDeclaration>();
			var container = dec.CusContainers.AddNew();
			var lookups = container.Lookups;
			dec.Packages.RemoveAndDeleteAll();
			var totalPackagesUnit_List = lookups.TotalPackagesUnit_List;
			AssertEquals(0, totalPackagesUnit_List.Count);
			AssertEquals(typeof(CodeDescriptionPairList), totalPackagesUnit_List.GetType());

			container.Packages.Add(package);
			totalPackagesUnit_List = lookups.TotalPackagesUnit_List;
			AssertEquals(list, totalPackagesUnit_List);

			mockPackage.Reset();

			// ensure that if a country override the PackTypeList, our code will use that override
			var packTypeList = package.PackTypeList;
			totalPackagesUnit_List = lookups.TotalPackagesUnit_List;
			AssertEquals(packTypeList.Count, totalPackagesUnit_List.Count);
			AssertEquals(packTypeList.GetType(), totalPackagesUnit_List.GetType());
		}

		public void TestContainerTypeCollection()
		{
			CusContainerLookups lookups = GetCusContainerLookups();
			AssertNotNull("Lookups.ContainerTypeCollection", lookups.ContainerTypeCollection);
			AssertEquals(typeof(RefContainerCollection), lookups.ContainerTypeCollection.GetType());
		}

		public virtual void TestContainerSizeList()
		{
			CusContainerLookups lookups = GetCusContainerLookups();
			AssertEquals(0, lookups.ContainerSizeList.Count);
		}

		protected virtual CusContainerLookups GetCusContainerLookups()
		{
			return new CusContainerLookups(Factory.New<BaseCusContainer>());
		}
	}
}
