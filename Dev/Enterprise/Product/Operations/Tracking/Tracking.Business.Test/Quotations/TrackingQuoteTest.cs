using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Quotations.Testing
{
	[TestedType(typeof(TrackingQuote))]
	public class TrackingQuoteTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.Quote);

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<TrackingQuote>();
		}

		public void TestNewPropertiesForWeb()
		{
			Quote quote = Factory.NewWithValidTestData<Quote>();
			RateOneOffShipment oneOffQuote = Factory.NewWithValidTestData<RateOneOffShipment>();
			TrackingQuote trackingQuote = Factory.Load<TrackingQuote>(quote.PK);

			quote.TH_GC = ZGuid.Empty;

			AssertEquals("TransportMode", "", trackingQuote.TransportMode);
			AssertEquals("Origin", "", trackingQuote.Origin);
			AssertEquals("Destination", "", trackingQuote.Destination);
			AssertEquals("Volume", 0m, trackingQuote.Volume);
			AssertEquals("Volume Units", "", trackingQuote.VolumeUnits);
			AssertEquals("Weight", 0m, trackingQuote.Weight);
			AssertEquals("Weight Units", "", trackingQuote.WeightUnits);
			AssertEquals("Company", "", trackingQuote.CompanyName);

			quote.TH_GC = GlbCompany.CurrentCompany.PK;
			quote.TH_OneTimeQuote = ZBool.True;
			oneOffQuote.TT_TH = trackingQuote.PK;
			oneOffQuote.TT_TransportMode = "AIR";
			oneOffQuote.TT_ContainerMode = "LSE";
			oneOffQuote.TT_RL_NKReceivalLocation = "HKHKG";

			oneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			oneOffQuote.TT_ActualVolume = 1m;
			oneOffQuote.TT_UnitOfVolume = "M3";
			oneOffQuote.TT_ActualWeight = 2m;
			oneOffQuote.TT_UnitOfWeight = "KG";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			trackingQuote = newFactory.Load<TrackingQuote>(quote.PK);

			AssertNotNull("Precondition: CurrentOneOffQuote shouldn't be null", trackingQuote.CurrentOneOffQuote);
			AssertEquals("TransportMode", "LSE", trackingQuote.TransportMode);
			AssertEquals("Origin", "HKHKG", trackingQuote.Origin);
			AssertEquals("Destination", "AUSYD", trackingQuote.Destination);
			AssertEquals("Volume", 1m, trackingQuote.Volume);
			AssertEquals("Volume Units", "M3", trackingQuote.VolumeUnits);
			AssertEquals("Weight", 2m, trackingQuote.Weight);
			AssertEquals("Weight Units", "KG", trackingQuote.WeightUnits);
			AssertEquals("Company", GlbCompany.CurrentCompany.GC_Name, trackingQuote.CompanyName);
			AssertEquals("Quote should be Active", Quote.QuoteStatusOptions.Active, trackingQuote.QuoteStatus);
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<Costing>(new ZQuery());
		}
	}
}
