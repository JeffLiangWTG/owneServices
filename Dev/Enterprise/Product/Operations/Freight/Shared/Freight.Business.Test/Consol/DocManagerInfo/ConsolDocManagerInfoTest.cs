using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ConsolDocManagerInfo))]
	public class ConsolDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CommonConsol>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var consol = (CommonConsol)GetEmptyParentBusinessObject();
			Shipment1 = consol.Shipments.AddNew();
			Shipment2 = consol.Shipments.AddNew();
			Container1 = consol.Containers.AddNew();
			Container2 = consol.Containers.AddNew();
			AsycudaManifestHeader1 = Factory.New<IAsycudaManifestHeader>();
			((BusinessObject)AsycudaManifestHeader1).FillWithValidTestData();
			AsycudaManifestHeader1.AMA_ParentId = consol.PK;
			AsycudaManifestHeader1.AMA_ParentTableCode = consol.TablePrefix;
			AsycudaManifestHeader2 = Factory.New<IAsycudaManifestHeader>();
			((BusinessObject)AsycudaManifestHeader2).FillWithValidTestData();
			AsycudaManifestHeader2.AMA_ParentId = consol.PK;
			AsycudaManifestHeader2.AMA_ParentTableCode = consol.TablePrefix;
			return consol;
		}

		public virtual void TestAllRelatedObjectsRetrieved()
		{
			var consol = (CommonConsol)GetPopulatedParentBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("Should contain CommonShipment 1 in related objects", true, ((IList)consol.DocManagerInfo.RelatedObjects).Contains(Shipment1));
				AssertEquals("Should contain CommonShipment 2 in related objects", true, ((IList)consol.DocManagerInfo.RelatedObjects).Contains(Shipment2));
				AssertEquals("Should contain container 1 in related objects", true, ((IList)consol.DocManagerInfo.RelatedObjects).Contains(Container1));
				AssertEquals("Should contain container 2 in related objects", true, ((IList)consol.DocManagerInfo.RelatedObjects).Contains(Container2));
				AssertEquals("Should contain AsycudaManifestHeader1 in related objects", true, ((IList)consol.DocManagerInfo.RelatedObjects).Contains(AsycudaManifestHeader1));
				AssertEquals("Should contain AsycudaManifestHeader2 in related objects", true, ((IList)consol.DocManagerInfo.RelatedObjects).Contains(AsycudaManifestHeader2));
				AssertEquals("Related object count should be 6", 6, consol.DocManagerInfo.RelatedObjects.Length);
			});
		}

		CommonShipment Shipment1;
		CommonShipment Shipment2;
		CommonContainer Container1;
		CommonContainer Container2;
		IAsycudaManifestHeader AsycudaManifestHeader1;
		IAsycudaManifestHeader AsycudaManifestHeader2;
	}
}
