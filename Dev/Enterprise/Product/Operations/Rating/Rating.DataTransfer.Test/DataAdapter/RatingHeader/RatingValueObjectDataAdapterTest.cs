using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	sealed class RatingValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestUnmatchedOrganisationCanBeUsedOnRateEntries()
		{
			var stream = resourceRetriever.Value.GetStream("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.CostingWithUnmatchedOrg.xml");
			XmlValueObjectSerializer serial = new XmlValueObjectSerializer(typeof(Xsd.Rate));
			CostingValueObjectDataAdapter adapter = new CostingValueObjectDataAdapter();
			RatingHeaderCollection collection = new RatingHeaderCollection(Factory);
			NotificationBuffer buffer = new NotificationBuffer();
			serial.ImportXmlData(stream, adapter, collection, null, buffer);

			AssertEquals(1, collection.Count);
			RateEntryCollection rateEntryCollection = collection[0].EntryCollections[RatingConstants.RateCategory.FCL].LazyLoadingCollection;

			AssertEquals(1, rateEntryCollection.Count);
			AssertNotNull("TransportProvider", rateEntryCollection[0].TransportProvider);
		}

		public void TestPerformanceFixesDoesntBreakImport()
		{
			var stream = resourceRetriever.Value.GetStream("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.CostingFullyPopulatedWithOrgLines.xml");

			XmlValueObjectSerializer serial = new XmlValueObjectSerializer(typeof(Xsd.Rate));
			CostingValueObjectDataAdapter adapter = new CostingValueObjectDataAdapter();
			RatingHeaderCollection collection = new RatingHeaderCollection(Factory);
			NotificationBuffer buffer = new NotificationBuffer();
			serial.ImportXmlData(stream, adapter, collection, null, buffer);

			AssertEquals(1, collection.Count);
			RatingHeader header = collection[0];

			bool headerIsGloballTariff = header.IsTariff();
			ZByte tariffLevel = headerIsGloballTariff ? header.TH_GlobalRateLevel : (ZByte)0;
			var rateEntrycollection = header.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;

			AssertEquals(false, header.IsValidationSuspended);
			AssertEquals(0, ((IXMLImportOrgCache)header).CachedOrgsForXMLImport.Count);
			AssertEquals(2, rateEntrycollection.Count);

			foreach (RateEntry entry in rateEntrycollection)
			{
				AssertEquals("Line order not set when importing rates. Performance issues and not critical", (ZShort)0, entry.TI_LineOrder);
				AssertEquals(false, entry.SuspendSettingRateLineTariff);
				AssertEquals(true, entry.HasErrors);
				foreach (RateLine line in entry.RateLines)
				{
					line.TL_WeightVolume = "KK";
					line.TL_CompanyTariffLevel = tariffLevel;
					AssertEquals(true, line.HasErrors);
				}
			}
		}

		public void TestErrorsPreventSave()
		{
			var stream = resourceRetriever.Value.GetStream("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.CostingFullyPopulatedWithOrgLinesAndErrors.xml");

			XmlValueObjectSerializer serial = new XmlValueObjectSerializer(typeof(Xsd.Rate));
			CostingValueObjectDataAdapter adapter = new CostingValueObjectDataAdapter();
			RatingHeaderCollection collection = new RatingHeaderCollection(Factory);
			NotificationBuffer buffer = new NotificationBuffer();
			serial.ImportXmlData(stream, adapter, collection, null, buffer);

			AssertEquals(1, collection.Count);
			RatingHeader header = collection[0];

			bool headerIsGloballTariff = header.IsTariff();
			ZByte tariffLevel = headerIsGloballTariff ? header.TH_GlobalRateLevel : (ZByte)0;

			AssertEquals(false, header.IsValidationSuspended);
			AssertEquals(0, ((IXMLImportOrgCache)header).CachedOrgsForXMLImport.Count);
			AssertNotEquals("Not all records created due to error", 3, header.GetRateEntryCollectionForCategory(Enterprise.Rating.Business.RatingConstants.RateCategory.AIR).Count);
		}

		public void TestSavingInBatchWithDefaultFactoryProviderSavesAllData()
		{
			var stream = resourceRetriever.Value.GetStream("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.CostingFullyPopulatedWithOrgLines.xml");

			XmlValueObjectSerializer serial = new XmlValueObjectSerializer(typeof(Xsd.Rate));
			CostingValueObjectDataAdapter adapter = new CostingValueObjectDataAdapterForBatchTest();
			RatingHeaderCollection collection = new RatingHeaderCollection(Factory);
			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();
			serial.ImportXmlData(stream, adapter, collection, factoryProvider, new NotificationBuffer());
			factoryProvider.SaveCurrentAndUpdateRecordCounts();

			AssertEquals(1, collection.Count);

			RateEntry[] savedEntries = new BusinessObjectFactory().Load<RateEntry>(new ZQuery(RateEntrySchema.TI_TH, collection[0].PK));

			AssertEquals("All rating entries should be saved", 3, savedEntries.Length);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		internal class CostingValueObjectDataAdapterForBatchTest : CostingValueObjectDataAdapter
		{
			protected override int BatchSize
			{
				get { return 2; }
			}
		}
	}

	abstract class RatingValueObjectDataAdapterTest<TBusinessObject> : ValueObjectDataAdapterTest<TBusinessObject, Xsd.Rate>
		where TBusinessObject : RatingHeader
	{
		public void TestImportFromValueObject()
		{
			TestImportCore();
		}
		protected abstract void TestImportCore();

		public void TestExpectedRateType()
		{
			TestExpectedRateTypeCore();
		}
		protected abstract void TestExpectedRateTypeCore();

		public void TestShouldCheckOwnerIsSpecified()
		{
			TestShouldCheckOwnerIsSpecifiedCore();
		}
		protected abstract void TestShouldCheckOwnerIsSpecifiedCore();

		public virtual void TestImportRateInWrongModule()
		{
			Xsd.Rate rateXSD = new Xsd.Rate();
			rateXSD.RateType = OtherRateTypeForErrorTesitng;
			rateXSD.Owner = TestHelper.ConsigneeXSD;

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			GetDataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			AssertEquals("notification has errors", true, buffer.HasErrors);
			AssertEquals("buffer should have this error message", true, TestHelper.AssertContainsErrorMesg(ExpectedInvalidModuleMesg, buffer));
		}

		#region Implementation

		protected abstract ZString ExpectedRateType { get; }
		protected abstract bool RateShouldCheckOwnerIsSpecified { get; }
		protected abstract ZString OtherRateTypeForErrorTesitng { get; }
		protected abstract RatingValueObjectDataAdapter<TBusinessObject> GetDataAdapter { get; }
		protected abstract ZString ExpectedInvalidModuleMesg { get; }

		protected override ValueObjectDataAdapter<TBusinessObject, Xsd.Rate> GetNewBizObjXmlDataAdapter()
		{
			return GetDataAdapter;
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return true; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Rates"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Rate"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		#endregion

		#region Containers

		protected ContainerFactory Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new ContainerFactory(Factory);
				}

				return fContainers;
			}
		}

		ContainerFactory fContainers;

		protected class ContainerFactory
		{
			public ContainerFactory(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}

			public RefContainer this[string container]
			{
				get { return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container); }
			}

			readonly BusinessObjectFactory Factory;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper = new RatingXSDTestHelper();
			TestHelper.Consignee.Factory.Save();
		}

		protected RatingXSDTestHelper TestHelper;

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
