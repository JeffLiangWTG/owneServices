using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContainerDetentionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCollections()
		{
			OrgContainerDetention cdl = Factory.New<OrgContainerDetention>();
			cdl.PD_Direction = "XXX";

			AssertType(typeof(ShippingProviderCollection), cdl.Lookups.Carriers);
			AssertType(typeof(LocationCollection), cdl.Lookups.Locations);
			AssertType(typeof(OrgHeaderCollection), cdl.Lookups.Clients);
			AssertType(typeof(CodeDescriptionPairList), cdl.Lookups.CreditorTypes);
			AssertType(typeof(CodeDescriptionPairList), cdl.Lookups.PenaltyTypes);
			AssertType(typeof(CodeDescriptionPairList), cdl.Lookups.Directions);

			cdl.PD_Direction = Constants.ContainerDetentionDirection.Export;
			AssertType(typeof(ConsignorCollection), cdl.Lookups.Clients);

			cdl.PD_Direction = Constants.ContainerDetentionDirection.Import;
			AssertType(typeof(ConsigneeCollection), cdl.Lookups.Clients);
		}

		public void TestStorageCTOCollection()
		{
			var cdl = Factory.New<OrgContainerDetention>();
			cdl.PD_Direction = "TST";

			AssertType(typeof(StorageCTOCollection), cdl.Lookups.StorageCTOs);
		}

		public void TestFreeDayTypes()
		{
			var cdl = Factory.New<OrgContainerDetention>();

			cdl.PD_Direction = Constants.ContainerDetentionDirection.Import;

			cdl.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.DET;
			AssertContainsExactElementsInAnyOrder(new string[] { "CTD", "FCD", "FC1", "VSD", "OUT" }, cdl.Lookups.FreeDayTypes.GetAllCodes());
			cdl.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.STO;
			AssertContainsExactElementsInAnyOrder(new string[] { "CTD", "FCD", "FC1", "VSD" }, cdl.Lookups.FreeDayTypes.GetAllCodes());
			cdl.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.MDD;
			AssertContainsExactElementsInAnyOrder(new string[] { "CTD", "OUT", "FCD", "FC1", "VSD" }, cdl.Lookups.FreeDayTypes.GetAllCodes());

			cdl.PD_Direction = Constants.ContainerDetentionDirection.Export;
			cdl.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.DET;
			AssertContainsExactElementsInAnyOrder(new string[] { "WGI", "FCL", "1FC", "VED", "1VE" }, cdl.Lookups.FreeDayTypes.GetAllCodes());
			cdl.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.STO;
			AssertContainsExactElementsInAnyOrder(new string[] { "FCL", "1FC", "VED", "1VE" }, cdl.Lookups.FreeDayTypes.GetAllCodes());
			cdl.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.MDD;
			AssertContainsExactElementsInAnyOrder(new string[] { "FCL", "1FC", "VED", "1VE" }, cdl.Lookups.FreeDayTypes.GetAllCodes());
		}

		public void TestFirstFreeDayTypes()
		{
			var detention = Factory.New<OrgContainerDetention>();

			detention.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.DET;
			AssertContainsExactElementsInAnyOrder("FirstFreeDayTypes for DET",
				new[] { "CTD", "FCD", "FC1", "VSD", "OUT" }, detention.Lookups.FirstFreeDayTypes.GetAllCodes());

			detention.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.STO;
			AssertContainsExactElementsInAnyOrder("FirstFreeDayTypes for STO",
				new[] { "CTD", "FCD", "FC1", "VSD" }, detention.Lookups.FirstFreeDayTypes.GetAllCodes());

			detention.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.MDD;
			AssertContainsExactElementsInAnyOrder("FirstFreeDayTypes for MDD",
				new[] { "CTD", "FCD", "FC1", "VSD", "OUT" }, detention.Lookups.FirstFreeDayTypes.GetAllCodes());
		}

		public void TestLastFreeDayTypes()
		{
			var detention = Factory.New<OrgContainerDetention>();

			detention.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.DET;
			AssertContainsExactElementsInAnyOrder("LastFreeDayTypes for DET",
				new[] { "WGI", "FCL", "1FC", "VED", "1VE" }, detention.Lookups.LastFreeDayTypes.GetAllCodes());

			detention.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.STO;
			AssertContainsExactElementsInAnyOrder("LastFreeDayTypes for STO",
				new[] { "FCL", "1FC", "VED", "1VE" }, detention.Lookups.LastFreeDayTypes.GetAllCodes());

			detention.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.MDD;
			AssertContainsExactElementsInAnyOrder("LastFreeDayTypes for MDD",
				new[] { "FCL", "1FC", "VED", "1VE" }, detention.Lookups.LastFreeDayTypes.GetAllCodes());
		}
	}
}
