using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffAttributeName))]
	internal class RefCusTariffAttributeNameTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			return helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "Test", "Description", "Col Caption", dataGrouping.ZZZ_DataGrouping, tariffType.ZZI_TariffType);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override void SetUp()
		{
			helper = new UniversalReferenceTestDataHelper(Factory);
			dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, "Australia");
			tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, "ABC");
			Factory.Save();
			base.SetUp();
		}

		RefCusTariffType tariffType;
		RefDataGrouping dataGrouping;
		UniversalReferenceTestDataHelper helper;
	}
}
