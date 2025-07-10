using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Packing.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.DataTransfer.Testing
{
	[TestedType(typeof(PkgPackageContainerCollectionDataObjectReader))]
	public class PkgPackageContainerCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		#region TestConstructor

		public void TestConstructor_LoggerIsNull()
		{
			Data.CreatePackingData();

			ArgumentNullExceptionAssertUtil.Assert("logger", () => new PkgPackageContainerCollectionDataObjectReader(new DataObjectList<Container>(), null, Factory, Data.PackageJob.Packages));
		}

		public void TestConstructor_FactoryIsNull()
		{
			Data.CreatePackingData();

			ArgumentNullExceptionAssertUtil.Assert("factory", () => new PkgPackageContainerCollectionDataObjectReader(new DataObjectList<Container>(), Logger, null, Data.PackageJob.Packages));
		}

		public void TestConstructor_PackageCollectionIsNull()
		{
			Data.CreatePackingData();

			ArgumentNullExceptionAssertUtil.Assert("packageCollection", () => new PkgPackageContainerCollectionDataObjectReader(new DataObjectList<Container>(), Logger, Factory, null));
		}

#endregion

		#region TestReadIntoCollection

		public override void TestReadIntoCollection()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var ref40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var pkgPackageJob = Data.PackageJob;
			var containerDO1 = AddContainerDataObject(1, "CON1", "20GP");
			var containerDO2 = AddContainerDataObject(1, "", "40GP");

			AssertEquals("Precondition:", 0, pkgPackageJob.Packages.Count);

			var reader = new PkgPackageContainerCollectionDataObjectReader(new DataObjectList<Container>(new[] { containerDO1, containerDO2 }), Logger, Factory, pkgPackageJob.Packages);
			reader.ReadIntoCollection();

			AssertEquals(2, pkgPackageJob.Packages.Count);
			AssertContainer(pkgPackageJob.Packages.Single(c => c.KP_PackageID == "CON1"), "CON1", Data.Container20GP.PK, 1);
			AssertContainer(pkgPackageJob.Packages.Single(c => c.KP_PackageID != "CON1"), "", ref40GP.PK, 1);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_Matches

		public void TestReadIntoCollection_Empty_Matches()
		{
			TestReadIntoCollection_MatchesCore();
		}

		public void TestReadIntoCollection_Partial_Matches()
		{
			TestReadIntoCollection_MatchesCore(CollectionContent.Partial);
		}

		public void TestReadIntoCollection_Complete_Matches()
		{
			TestReadIntoCollection_MatchesCore(CollectionContent.Complete);
		}

		void TestReadIntoCollection_MatchesCore(CollectionContent? content = null)
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var ref40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var ref20FR = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			var packageJob = Data.PackageJob;
			var container1 = AddContainer(packageJob.Packages, "CON1", 1, Data.Container20GP.PK);
			var container2 = AddContainer(packageJob.Packages, "CON2", 1, ref40GP.PK, weightUQ: Constants.Weight.Grams);
			var container3 = AddContainer(packageJob.Packages, "", 1, Data.Container20GP.PK, weight: 10m);
			var container4 = AddContainer(packageJob.Packages, "", 1, Data.Container20GP.PK, volume: 10m);
			var container5 = AddContainer(packageJob.Packages, "", 5, Data.Container20GP.PK, volume: 10m);

			var containerDO1 = AddContainerDataObject(1, "CON1", "20FR");
			var containerDO2 = AddContainerDataObject(1, "CON2", "40GP", weightUQ: Constants.Weight.Kilograms);
			var containerDO3 = AddContainerDataObject(1, "", "20GP", weight: 20m);
			var containerDO4 = AddContainerDataObject(1, "", "20GP", volume: 15m, volumeUQ: Constants.Volume.Litre);
			var containerDO5 = AddContainerDataObject(5, "", "20FR");

			AssertEquals("Precondition:", 5, packageJob.Packages.Count);

			var containerDOList = new DataObjectList<Container>(new[] { containerDO1, containerDO2, containerDO3, containerDO4, containerDO5 });
			containerDOList.Content = content;
			var reader = new PkgPackageContainerCollectionDataObjectReader(containerDOList, Logger, Factory, packageJob.Packages);
			reader.ReadIntoCollection();

			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.", Logger.Logs);

			var expectedCount = content == CollectionContent.Complete ? 5 : 6;
			AssertEquals(expectedCount, packageJob.Packages.Count);

			PkgPackage[] existingPackges;
			if (content != CollectionContent.Complete)
			{
				existingPackges = new PkgPackage[] { container1, container2, container3, container4, container5 };
				AssertContainer(packageJob.Packages.Single(c => c.PK == container1.PK), "CON1", ref20FR.PK, 1);
				AssertContainer(packageJob.Packages.Single(c => c.PK == container2.PK), "CON2", ref40GP.PK, 1);
				AssertContainer(packageJob.Packages.Single(c => c.PK == container3.PK), "", Data.Container20GP.PK, 1, expectWeight: 20m);
				AssertContainer(packageJob.Packages.Single(c => c.PK == container4.PK), "", Data.Container20GP.PK, 1, expectVolume: 15m, expectVolumeUQ: Constants.Volume.Litre);
				AssertContainer(packageJob.Packages.Single(c => c.PK == container5.PK), "", Data.Container20GP.PK, 5, expectVolume: 10m);

				AssertContainer(packageJob.Packages.Except(existingPackges).Single(), "", ref20FR.PK, 5);
			}
			else
			{
				AssertContainer(packageJob.Packages.Single(c => c.KP_PackageID == "CON1"), "CON1", ref20FR.PK, 1);
				AssertContainer(packageJob.Packages.Single(c => c.KP_PackageID == "CON2"), "CON2", ref40GP.PK, 1);
				AssertContainer(packageJob.Packages.Single(c => c.KP_Weight == 20m), "", Data.Container20GP.PK, 1, expectWeight: 20m);
				AssertContainer(packageJob.Packages.Single(c => c.KP_Volume == 15m), "", Data.Container20GP.PK, 1, expectVolume: 15m, expectVolumeUQ: Constants.Volume.Litre);
				AssertContainer(packageJob.Packages.Single(c => c.KP_PackageQty == 5), "", ref20FR.PK, 5);
			}
		}

		#endregion

		#region TestReadIntoCollection_CannotDeleteWhenIsUsed

		public void TestReadIntoCollection_CannotDeleteWhenIsUsed()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var container1 = packageJob.Packages.AddNew("CNT", "CON1");
			var container3 = packageJob.Packages.AddNew("CNT", "CON3");

			var actionStrategyNoDelete = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ container1, new PackageActionStrategy(container1, PackageAction.Delete, "Cannot delete.") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyNoDelete, true);

			var containerDO2 = AddContainerDataObject(1, "CON2", "20GP");
			var containerDOList = new DataObjectList<Container>(new[] { containerDO2 }) { Content = CollectionContent.Complete };
			var reader = new PkgPackageContainerCollectionDataObjectReader(containerDOList, Logger, Factory, packageJob.Packages);
			reader.ReadIntoCollection();

			AssertEquals(2, packageJob.Packages.Count);
			AssertEquals("Existing container should not be deleted", container1, packageJob.Packages.Single(p => p.KP_PackageID == "CON1"));
			AssertContainer(packageJob.Packages.Single(p => p.KP_PackageID != "CON1"), "CON2", Data.Container20GP.PK, 1);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.", Logger.Logs);
		}

		#endregion

		#region Implementation

		PkgPackage AddContainer(PkgPackageCollection packages, ZString packageID, ZInt packageQty, ZGuid containerTypePK, decimal weight = 5m, string weightUQ = Constants.Weight.Kilograms, decimal volume = 5m, string volumeUQ = Constants.Volume.CubicMetres)
		{
			var container = packages.AddNew("CNT");
			container.KP_PackageID = packageID;
			container.KP_PackageQty = packageQty;
			container.Container.K0_RC_ContainerType = containerTypePK;
			container.KP_Weight = weight;
			container.KP_WeightUQ = weightUQ;
			container.KP_Volume = volume;
			container.KP_VolumeUQ = volumeUQ;

			return container;
		}

		Container AddContainerDataObject(ZInt count, ZString containerNumber, ZString containerType, decimal weight = 5m, string weightUQ = Constants.Weight.Kilograms, decimal volume = 5m, string volumeUQ = Constants.Volume.CubicMetres)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			container.ContainerCount = count;
			container.ContainerNumber = containerNumber;
			container.ContainerType = new ContainerType { Code = containerType };
			container.GrossWeight = weight;
			container.WeightUnit = new UnitOfWeight { Code = weightUQ };
			container.VolumeCapacity = volume;
			container.VolumeUnit = new UnitOfVolume { Code = volumeUQ };

			return container;
		}

		void AssertContainer(PkgPackage container, string expectNumber, ZGuid expectTypePK, ZInt expectCount, decimal expectWeight = 5m, string expectWeightUQ = Constants.Weight.Kilograms, decimal expectVolume = 5m, string expectVolumeUQ = Constants.Volume.CubicMetres)
		{
			AssertEquals("Container Number", expectNumber, container.KP_PackageID);
			AssertEquals("Container PK", expectTypePK, container.Container.K0_RC_ContainerType);
			AssertEquals("Container Qty", expectCount, container.KP_PackageQty);
			AssertEquals("Weight", expectWeight, container.KP_Weight);
			AssertEquals("WeightUQ", expectWeightUQ, container.KP_WeightUQ);
			AssertEquals("Volume", expectVolume, container.KP_Volume);
			AssertEquals("VolumeUQ", expectVolumeUQ, container.KP_VolumeUQ);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}
		protected TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory.BOFactory));

		protected NotifyForPacking Notify => notify ?? (notify = new NotifyForPacking());

		protected PackingTestHelper Helper => helper ?? (helper = new PackingTestHelper(Factory.BOFactory));

		TestDataForPacking data;
		NotifyForPacking notify;
		PackingTestHelper helper;

		#region Logger

		protected TestErrorLogger Logger => logger ?? (logger = new TestErrorLogger());
		TestErrorLogger logger;

		#endregion

		#endregion
	}
}
