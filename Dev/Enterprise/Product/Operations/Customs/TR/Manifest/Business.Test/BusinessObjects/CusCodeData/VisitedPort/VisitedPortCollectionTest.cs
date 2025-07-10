using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(VisitedPortCollection))]
	public class VisitedPortCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<VisitedPort>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(VisitedPortCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.VisitedPorts;
		}

		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<VisitedPort> GetCusCodeDataCollection()
		{
			return new VisitedPortCollection(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var newPort = Factory.New<VisitedPort>();
			return newPort;
		}

		AsycudaManifestHeader Header
		{
			get
			{
				return header ?? (header = Factory.New<AsycudaManifestHeader>());
			}
		}

		AsycudaManifestHeader header;
	}
}
