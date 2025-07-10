using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusInBondMoveDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var list = (CodeDescriptionPairList)lookups.MessageStatusList;
			AssertEquals("Empty list", string.Empty, list.CodesAsString);
		}

		public void TestCustomsStatusList()
		{
			var list = (CodeDescriptionPairList)lookups.CustomsStatusList;
			AssertEquals("Empty list", string.Empty, list.CodesAsString);
		}

		public void TestConveyanceList()
		{
			AssertType<RefVesselCollection>(lookups.ConveyanceList);
		}

		public void TestForeignPorts()
		{
			CombineAssertions(() =>
			{
				var list = lookups.ForeignPorts;
				AssertType<ZZRefCusCodeListCombinedCollection>(list);
			});
		}

		CusInBondMoveDetailLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			var moveDetail = Factory.New<CusInBondMoveDetailForTest>();
			lookups = new CusInBondMoveDetailLookups(moveDetail);
		}

		sealed class CusInBondMoveDetailForTest : CusInBondMoveDetail
		{
			public CusInBondMoveDetailForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override Type ContainerTypeCore => throw new NotImplementedException();

			protected override Type MoveLineItemTypeCore => throw new NotImplementedException();

			protected override Type PackTypeCore => throw new NotImplementedException();

			protected override Customs.Business.ICusInBondContainerCollection GetContainersCollection() => throw new NotImplementedException();
		}
	}
}
