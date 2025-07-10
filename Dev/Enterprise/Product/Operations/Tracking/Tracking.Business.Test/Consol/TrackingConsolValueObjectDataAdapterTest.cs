using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(TrackingConsolValueObjectDataAdapter))]
	sealed class TrackingConsolValueObjectDataAdapterTest : ValueObjectDataAdapterTest<TrackingConsol, Xsd.WebConsol>
	{
		protected override ValueObjectDataAdapter<TrackingConsol, Xsd.WebConsol> GetNewBizObjXmlDataAdapter() => new TrackingConsolValueObjectDataAdapter();

		protected override string ExpectedRootCollectionElementName => "WebConsols";

		protected override string ExpectedRootElementName => "WebConsol";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyConsol = Factory.NewWithValidTestData<TrackingConsol>(TestBusinessObjectKind.NoData);
			emptyConsol.JK_ConsolMode = string.Empty;
			emptyConsol.JK_TransportMode = string.Empty;

			var emptyConsolXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyConsol.xml", "EmptyConsol.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyConsol, emptyConsolXmlPath, ValidationKind.None, "Empty Consol");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedConsol = Factory.New<TrackingConsol>();

			populatedConsol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			populatedConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var departureTransport = populatedConsol.Transports[0];
			departureTransport.JW_LegOrder = 1;
			departureTransport.JW_RL_NKLoadPort = "MYPKG";
			departureTransport.JW_RL_NKDiscPort = "AUSYD";
			departureTransport.JW_Vessel = "ADMIRALENGRACHT";
			departureTransport.JW_VoyageFlight = "voyage";
			departureTransport.JW_ATD = new ZDateTime(1900, 1, 1, 0, 0, 0);
			departureTransport.JW_ATA = new ZDateTime(1900, 1, 30, 0, 0, 0);

			var arrivalTransport = populatedConsol.Transports.AddNew();
			arrivalTransport.JW_LegOrder = 2;
			arrivalTransport.JW_ATD = new ZDateTime(1900, 2, 1, 0, 0, 0);
			arrivalTransport.JW_ATA = new ZDateTime(1901, 1, 1, 0, 0, 0);

			populatedConsol.JK_MasterBillNum = "MasterBillNum";

			AssertEquals("Departure Transport ATD", departureTransport.JW_ATD, populatedConsol.Transports.DepartureTransport.JW_ATD);
			AssertEquals("Departure Transport ATA", departureTransport.JW_ATA, populatedConsol.Transports.DepartureTransport.JW_ATA);

			AssertEquals("Arrival Transport ATD", arrivalTransport.JW_ATD, populatedConsol.Transports.ArrivalTransport.JW_ATD);
			AssertEquals("Arrival Transport ATA", arrivalTransport.JW_ATA, populatedConsol.Transports.ArrivalTransport.JW_ATA);

			AssertEquals("Precondition: Should be one MostInterestingTransportForBInding", 1, populatedConsol.MostInterestingTransportForBinding.Count);
			AssertEquals("Precondition: MostInterestingTransport should be Dparture Transport", departureTransport.PK, populatedConsol.MostInterestingTransportForBinding[0].PK);
			AssertEquals("Consol ATD should be date from the Departure Transport which is the Most Interesting Transport", departureTransport.JW_ATD, populatedConsol.JK_JX_JA_A_DEP);
			AssertEquals("Consol ATA should be date from the Departure Transport which is the Most Interesting Transport", departureTransport.JW_ATA, populatedConsol.JK_JX_JB_A_ARV);

			var exportedConsol = new Xsd.WebConsol();
			GetNewBizObjXmlDataAdapter().ExportToValueObject(populatedConsol, exportedConsol, null);
			AssertEquals("ValueObject ATD", departureTransport.JW_ATD, exportedConsol.ATD);
			AssertEquals("ValueObject ATA", arrivalTransport.JW_ATA, exportedConsol.ATA);

			var fullConsolXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullConsol.xml", "FullConsol.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedConsol, fullConsolXmlPath, ValidationKind.Xsd, "Populated Consol");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"LoadPort/Country",
					"LoadPort/City",
					"LoadPort/Value",
					"DischargePort/Country",
					"DischargePort/City",
					"DischargePort/Value"
				};
			}
		}

		protected override bool IsImportFromValueObjectSupported => false;

		protected override bool IsExportToCollectionSupported => false;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
	}
}
