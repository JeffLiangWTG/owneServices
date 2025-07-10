using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AdditionalServiceDataObjectCollectionReader))]
	internal class AdditionalServiceDataObjectCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var dummy = PrepareAndReadAdditionalServices(addCollectionContent: false);

			AssertEquals(3, dummy.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, dummy.Services[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Cleaning, dummy.Services[1].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, dummy.Services[2].ES_ServiceCode);
		}

		public void TestReadIntoCollection_Partial()
		{
			var dummy = PrepareAndReadAdditionalServices(addCollectionContent: true, CollectionContent.Partial);

			AssertEquals(3, dummy.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, dummy.Services[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Cleaning, dummy.Services[1].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, dummy.Services[2].ES_ServiceCode);
		}

		public void TestReadIntoCollection_Complete()
		{
			var dummy = PrepareAndReadAdditionalServices(addCollectionContent: true, CollectionContent.Complete);

			AssertEquals(2, dummy.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, dummy.Services[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, dummy.Services[1].ES_ServiceCode);
		}

		public void TestDefaultCollectionContent_ShouldBePartial()
		{
			var dummy = PrepareAndReadAdditionalServices(addCollectionContent: false);

			AssertEquals(3, dummy.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, dummy.Services[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Cleaning, dummy.Services[1].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, dummy.Services[2].ES_ServiceCode);
		}

		public void TestReadIntoCollection_ShouldAddTheServiceTwice_WhenItIsAlreadyInServicesAndContentIsPartialAndServiceTypeDoesNotNeedToBeUnique()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var dummy = PrepareAndReadAdditionalServices(addCollectionContent: false, CollectionContent.Partial, addNonUniqueServiceType: true);

				AssertEquals(5, dummy.Services.Count);
				AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, dummy.Services[0].ES_ServiceCode);
				AssertEquals(Core.Constants.FreightServiceType.Codes.Cleaning, dummy.Services[1].ES_ServiceCode);
				AssertEquals("Existing non-unique service (ICI) has not been removed from the list.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, dummy.Services[2].ES_ServiceCode);
				AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, dummy.Services[3].ES_ServiceCode);
				AssertEquals("New non-unique service (ICI) has added to the list, without removing the old one.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, dummy.Services[4].ES_ServiceCode);
			}
		}

		public void TestRemoveFromCollection_JobService_Invokes_JobServiceDeleted_InParent()
		{
			var dummy = PrepareAndReadAdditionalServices(addCollectionContent: true, CollectionContent.Complete, addNonUniqueServiceType: true);

			AssertEquals(true,dummy.JobServiceDeletedCalled());
		}

		DummyWithServices PrepareAndReadAdditionalServices(bool addCollectionContent = true, CollectionContent collectionContent = CollectionContent.Complete, bool addNonUniqueServiceType = false)
		{
			var logger = new TestErrorLogger();

			var dummy = Factory.New<DummyWithServices>();

			var fumService = ((IHaveServices)dummy).Services.AddNew();
			fumService.ShouldPopulateServiceId = false;
			fumService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var clnService = ((IHaveServices)dummy).Services.AddNew();
			clnService.ShouldPopulateServiceId = false;
			clnService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Cleaning;

			if (addNonUniqueServiceType)
			{
				var inspectionService = ((IHaveServices)dummy).Services.AddNew();
				inspectionService.ShouldPopulateServiceId = false;
				inspectionService.ES_ServiceCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection;
			}

			Factory.SaveForTesting();

			var additionalServicesDataObject = new DataObjectList<AdditionalService>();

			var fumAdditionalService = new AdditionalService
			{
				ServiceCode = new CodeDescriptionPair { Code = Core.Constants.FreightServiceType.Codes.Fumigation, Description = Core.Constants.FreightServiceType.Descriptions.Fumigation },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = null,
			};

			var wshAdditionalService = new AdditionalService
			{
				ServiceCode = new CodeDescriptionPair { Code = Core.Constants.FreightServiceType.Codes.Washing, Description = Core.Constants.FreightServiceType.Descriptions.Washing },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = null,
			};

			additionalServicesDataObject.Add(fumAdditionalService);
			additionalServicesDataObject.Add(wshAdditionalService);

			if (addNonUniqueServiceType)
			{
				var iciAdditionalService = new AdditionalService
				{
					ServiceCode = new CodeDescriptionPair { Code = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, Description = "Commodity Inspection" },
					Booked = new ZDateTime(2011, 6, 1),
					Completed = new ZDateTime(2011, 6, 2),
					Contractor = null,
				};
				additionalServicesDataObject.Add(iciAdditionalService);
			}

			if (addCollectionContent)
			{
				additionalServicesDataObject.Content = collectionContent;
			}

			var reader = new AdditionalServiceDataObjectCollectionReader(additionalServicesDataObject, logger, Factory, dummy);
			reader.ReadIntoCollection();
			return dummy;
		}
	}
}
