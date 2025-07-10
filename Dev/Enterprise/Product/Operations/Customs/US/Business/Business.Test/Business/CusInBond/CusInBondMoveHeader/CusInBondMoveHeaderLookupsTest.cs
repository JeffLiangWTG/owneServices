using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusInBondMoveHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestYesNoList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.YesNoList;
				AssertEquals("Codes", "N, Y", list.CodesAsString);
				AssertSame("Cached", YesNoDefaultList.GetCachedYesNoList(Factory), list);
			});
		}

		public void TestMessageStatusList()
		{
			var list = (CodeDescriptionPairList)lookups.MessageStatusList;
			AssertEquals("Empty list", string.Empty, list.CodesAsString);
		}

		public void TestUSStatesList()
		{
			AssertSame(Factory.GetCachedValue<USStatesList>(), lookups.USStatesList);
		}

		public void TestTransportModeCodes()
		{
			CombineAssertions(() =>
			{
				var list = lookups.TransportModeCodes;
				AssertEquals("Codes", "10, 11, 20, 30, 40", list.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<InBondTransportModeCodes>(), list);
			});
		}

		public void TestConveyanceList()
		{
			AssertType<RefVesselCollection>(lookups.ConveyanceList);
		}

		public void TestShippingProviders()
		{
			AssertType<ShippingProviderCollection>(lookups.ShippingProviders);
		}

		public void TestEntryTypeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.EntryTypeList;
				AssertEquals("Codes", "61, 62, 63", list.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<InbondCommonTypeList>(), list);
			});
		}

		public void TestCarrierCollection()
		{
			AssertType<USCarrierCombinedCollection>(lookups.CarrierCollection);
		}

		public void TestForeignPorts()
		{
			CombineAssertions(() =>
			{
				var list = lookups.ForeignPorts;
				AssertType<ZZRefCusCodeListCombinedCollection>(list);
			});
		}

		public void TestRegionDistrictPorts()
		{
			AssertType<ZZRefCusCodeListCombinedCollection>(lookups.RegionDistrictPorts);
		}

		public void TestBondedWarehouseCollection()
		{
			AssertType<BondedWarehouseCollection>(lookups.BondedWarehouseCollection);
		}

		CusInBondMoveHeaderLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			var moveHeader = Factory.New<CusInBondMoveHeaderForTest>();
			lookups = new CusInBondMoveHeaderLookups(moveHeader);
		}

		sealed class CusInBondMoveHeaderForTest : CusInBondMoveHeader
		{
			public CusInBondMoveHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override Type MovementDetailTypeCore => throw new NotImplementedException();

			protected override Customs.Business.ICusInBondMoveDetailCollection CreateMovementDetails() => throw new NotImplementedException();

			protected override IEnumerable<CusInBondMoveHeader> GetMovementHeaders(Customs.Business.CusInBondHeader header) => throw new NotImplementedException();
		}
	}
}
