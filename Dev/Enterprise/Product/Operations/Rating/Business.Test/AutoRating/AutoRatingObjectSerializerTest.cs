using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using NUnit.Framework;
using static Enterprise.Rating.Business.Testing.FreightAutoRaterJobUpdatingTests;

namespace Enterprise.Rating.Business.Testing
{
	sealed class AutoRatingObjectSerializerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetXml()
		{
			var testObject = new AutoRatingObject();
			testObject.Creditors = Creditors.New(OrgWithSource.New(Factory.NewWithValidTestData<OrgHeader>(), new List<string> { "TransportProvider" }));
			testObject.JobDatesProvider = new JobDatesProvider<DummyBusinessObject>(Factory.NewWithValidTestData<DummyBusinessObject>());
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetQuantity(MeasureType.Weight, 27m, "");
			measures.SetQuantity(MeasureType.Volume, 3m, "");
			testObject.RateableMeasures = measures;

			var serializer = new RatingObjectSerializer();
			var serializedLength = serializer.GetXML(testObject).Length;
			AssertGreaterThan("Just to make sure it serializes something", serializedLength, 1000);
		}

		public void TestGetJson_ObjectIsNull_ReturnNull()
		{
			var serialiser = new RatingObjectSerializer();
			var json = serialiser.GetJSON(null);

			AssertMultilineASCIIEquals("Json", null, json);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 7, 7)]
		public void TestGetJson_ObjectNotNull_ReturnJson()
		{
			var testObject = new AutoRatingObject();
			testObject.Creditors = Creditors.New(OrgWithSource.New(Factory.NewWithValidTestData<OrgHeader>(), new List<string> { "TransportProvider" }));
			testObject.JobDatesProvider = new JobDatesProvider<DummyBusinessObject>(Factory.NewWithValidTestData<DummyBusinessObject>());
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetQuantity(MeasureType.Weight, 27m, "");
			measures.SetQuantity(MeasureType.Volume, 3m, "");
			testObject.RateableMeasures = measures;

			var serializer = new RatingObjectSerializer();
			var serializedJsonLength = serializer.GetJSON(testObject).Length;

			AssertGreaterThan("Just to make sure it serializes something", serializedJsonLength, 1000);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 7, 7)]
		public void TestGetXmlWithAdaptersAndAdditionalJobs()
		{
			var supporter = new NonPersistentRatingSupporter
			{
				AdaptersGetter = () =>
				{
					var autoRating = new AutoRatingObject();
					autoRating.JobDatesProvider = new JobDatesProvider<DummyBusinessObject>(Factory.NewWithValidTestData<DummyBusinessObject>());
					return new List<IAutoRating> { autoRating };
				}
			};

			var jobInvoicingPlugin = new AutoRatingObjectWithUpdatePlugin("AUSYD", "USLAX", FreightMode.SEA, null, 22m, 30m, null);
			jobInvoicingPlugin.JobDatesProvider = new JobDatesProvider<DummyBusinessObject>(Factory.NewWithValidTestData<DummyBusinessObject>());

			var addtitionalJobSupporter = new NonPersistentRatingSupporter
			{
				AdaptersGetter = () => new List<IAutoRating> { jobInvoicingPlugin }
			};
			jobInvoicingPlugin.AdaptersProviderGetter = () => new NonPersistentRatingSupporterAdaptersProvider(addtitionalJobSupporter, JobInvoicingConsumerTypes.Shipment);
			jobInvoicingPlugin.StatusInformation = new AutoRatingStatusInfo(true, "");

			supporter.AdditionalJobsGetter =
				() => new ReadOnlyCollection<IJobInvoicingPlugIn>(new List<IJobInvoicingPlugIn> { jobInvoicingPlugin });

			var serializer = new RatingObjectSerializer();
			var serializedResult = serializer.GetXML(supporter);

			Assert("Just to make sure it serializes something", serializedResult.Length > 1000);
		}

		[TestDate(2014, 10, 10)]
		public void TestJobServicesEnabledOnly()
		{
			var serialiser = new RatingObjectSerializer();
			var supporter = new NonPersistentRatingSupporter
			{
				AdaptersGetter = () =>
				{
					var autoRating = new AutoRatingObject();
					autoRating.StatusInformation = new AutoRatingStatusInfo(true);
					autoRating.JobServices = new JobServicesCollection();
					autoRating.JobServices.Add(new JobServiceInfo(true, "enabledChargeCodeGroup", "code1", "description1"));
					autoRating.JobServices.Add(new JobServiceInfo(false, "disabledChargeCodeGroup", "code2", "description2"));
					return new List<IAutoRating> { autoRating };
				}
			};

			var serialised = serialiser.GetXML(supporter);
			AssertContains("An enabled job service should be serialised", "enabledChargeCodeGroup", serialised);
			AssertContains("An enabled job service should be serialised", "code1", serialised);
			AssertContains("An enabled job service should be serialised", "description1", serialised);
			AssertNotContains("A disabled job service should not be serialised", "disabledChargeCodeGroup", serialised);
			AssertNotContains("A disabled job service should not be serialised", "code2", serialised);
			AssertNotContains("A disabled job service should not be serialised", "description2", serialised);
		}

		public void TestJobServicesAllPublicProperties()
		{
			var serialiser = new RatingObjectSerializer();
			var info = new JobServiceInfo(true, "enabledChargeCodeGroup", "code1", "description1");

			var supporter = new NonPersistentRatingSupporter
			{
				AdaptersGetter = () =>
				{
					var autoRating = new AutoRatingObject();
					autoRating.StatusInformation = new AutoRatingStatusInfo(true);
					autoRating.JobServices = new JobServicesCollection();
					autoRating.JobServices.Add(info);
					return new List<IAutoRating> { autoRating };
				}
			};

			var serialised = serialiser.GetXML(supporter);
			var expectedStrings = new List<string>
			{
				nameof(info.ChargeCodeGroup),
				nameof(info.CompletedDate),
				nameof(info.Container),
				nameof(info.ContainerCount),
				nameof(info.ContainerType),
				nameof(info.Contractor),
				nameof(info.Currency),
				nameof(info.FaultMessage),
				nameof(info.IsCostForSpotRate),
				nameof(info.IsEnabled),
				nameof(info.IsHiddenService),
				nameof(info.LocationCountryCode),
				nameof(info.Rate),
				nameof(info.ServiceCode),
				nameof(info.ServiceCount),
				nameof(info.ServiceDescription),
				nameof(info.ServiceDuration),
				nameof(info.ServiceReference),
				nameof(info.Unit),
				nameof(info.TotalCost)
			};

			foreach (var expectedString in expectedStrings)
			{
				AssertContains(expectedString, serialised);
			}
		}

		public void TestGetJson_ObjectHasErrors_ReturnErrors()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_PackingMode = "FCL";
			shipment.JS_TransportMode = "SEA";

			var serialiser = new RatingObjectSerializer();
			var json = serialiser.GetJSON(shipment);

			var errorMessage = "Error: A Consol must be attached to this Shipment before you can do FCL Autorating";
			AssertContains("Error should exist in JSON", errorMessage, json);
		}
	}
}
