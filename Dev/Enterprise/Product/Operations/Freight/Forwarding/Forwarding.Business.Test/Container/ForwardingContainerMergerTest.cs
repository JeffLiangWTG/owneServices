using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingContainerMergerTest
			: BusinessObjectMergerTest<ForwardingContainer>
	{
		public override void TestMerge()
		{
			var refContaier = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var container1 = consol.Containers.AddNew();

			container1.JC_RC = refContaier.PK;
			container1.JC_ContainerCount = 3;
			container1.JC_GrossVolume = 110m;
			container1.JC_GrossVolumeUQ = "M3";
			container1.JC_GoodsValue = 900m;
			container1.JC_DunnageWeight = 350m;

			container1.JC_WeightCapacity = 120m;
			container1.JC_VolumeCapacity = 50m;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 10m;
			container1.AddPackLine(packLine1);

			var container2 = consol.Containers.AddNew();

			container2.JC_RC = refContaier.PK;
			container2.JC_ContainerCount = 4;
			container2.JC_GrossVolume = 3m;
			container2.JC_GrossVolumeUQ = "L";
			container2.JC_GoodsValue = 120m;
			container2.JC_DunnageWeight = 12m;

			container2.JC_WeightCapacity = 980m;
			container2.JC_VolumeCapacity = 12m;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 24m;
			container2.AddPackLine(packLine2);

			var container3 = consol.Containers.AddNew();
			container3.CopyPersistentValuesFrom(container1);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualWeight = 37m;
			container3.AddPackLine(packLine3);

			container1.JC_TareWeight = 9850m;
			container1.JC_GrossWeight = 12850m;

			container2.JC_TareWeight = 300m;
			container2.JC_GrossWeight = 900m;

			var list = new[]
			{
				container1, container2
			};

			var merger = new ForwardingContainerMerger(list);
			merger.DoMerge();

			AssertEquals(2, consol.Containers.Count);

			var newContainer = (ForwardingContainer)consol.Containers.FirstOrDefault(c => c.PK != container3.PK);
			AssertNotNull("Should merge into one new container!", newContainer);

			AssertEquals(refContaier.PK, newContainer.JC_RC);
			AssertEquals(new ZShort(7), newContainer.JC_ContainerCount);

			AssertEquals(10150m, newContainer.JC_TareWeight);
			AssertEquals(13750m, newContainer.JC_GrossWeight);
			AssertEquals("KG", newContainer.JC_GrossWeightUQ);

			AssertEquals(110.003m, newContainer.JC_GrossVolume);
			AssertEquals("M3", newContainer.JC_GrossVolumeUQ);

			AssertEquals(1020m, newContainer.JC_GoodsValue);
			AssertEquals(362m, newContainer.JC_DunnageWeight);

			AssertEquals(1100m, newContainer.JC_WeightCapacity);
			AssertEquals(62m, newContainer.JC_VolumeCapacity);
		}

		public override void TestCheckMerge()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = string.Empty;
			container1.JC_Description = string.Empty;
			container1.JC_ContainerCount = 3;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = string.Empty;
			container2.JC_Description = "TEST";
			container2.JC_ContainerCount = 5;

			var list = new[]
			{
				container1
			};
			AssertCheckMergerMessage(list, "Please choose at least two container lines!");

			list = new[]
			{
				container1, container2
			};

			container1.JC_ContainerNum = "Con001";
			AssertCheckMergerMessage(list, "Selected containers should not have any container numbers!");

			container1.JC_ContainerNum = string.Empty;
			container2.Services.AddNew();
			AssertCheckMergerMessage(list, "Selected containers should not have any services!");

			container2.Services.RemoveAndDeleteAll();
			AssertCheckMergerMessage(list, @"The following fields are different:

Description");

			container1.JC_Description = "TEST";
			AssertCheckMergerMessage(list, string.Empty);
		}

		void AssertCheckMergerMessage(IList<ForwardingContainer> list, string message)
		{
			var merger = new ForwardingContainerMerger(list);
			AssertEquals(message, merger.CheckMerger());
		}

		protected override IEnumerable<SchemaColumn> GetAllSchemaColumns()
		{
			return JobContainerSchema.All.Cast<SchemaColumn>();
		}

		protected override BusinessObjectMerger<ForwardingContainer> GetNewMerger()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var list = new[]
			{
				consol.Containers.AddNew(),
				consol.Containers.AddNew()
			};

			return new ForwardingContainerMerger(list);
		}
	}
}
