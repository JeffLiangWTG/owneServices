using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(TestVesselRoutingPortPairCollection))]
	sealed class VesselRoutingPortPairCollectionOneStopTest : VesselRoutingPortPairCollectionTestBase
	{
		public void TestSelectPortPairsMatchingPort()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageIn", "VoyageOut");
			Factory.Save();

			VoyageCollection.PortPairTypeFilter = PortPairTypes.Domestic;
			VesselRoutingVoyage voyage = LoadVoyageByLloyds("Lloyds", DataProvider);
			AssertPortPairCount(voyage, 9);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[0], "", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Domestic);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[1], "", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[2], "", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[3], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[4], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[5], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[6], "AUSYD", "", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[7], "AUMEL", "", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("Expected port pair for test", voyage.PortPairs[8], "AUPER", "", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Domestic);

			voyage.PortPairs.SelectPortPairsMatchingPort("AUSYD");
			Assert(!voyage.PortPairs[0].E9_IsSelected);
			Assert(!voyage.PortPairs[1].E9_IsSelected);
			Assert(!voyage.PortPairs[2].E9_IsSelected);
			Assert(voyage.PortPairs[3].E9_IsSelected);
			Assert(voyage.PortPairs[4].E9_IsSelected);
			Assert(!voyage.PortPairs[5].E9_IsSelected);
			Assert(!voyage.PortPairs[6].E9_IsSelected);
			Assert(!voyage.PortPairs[7].E9_IsSelected);
			Assert(!voyage.PortPairs[8].E9_IsSelected);
		}

		#region Loading Port Pairs

		public void TestPortPairs_FilterDomesticAndExport()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageIn", "VoyageOut");
			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Export | PortPairTypes.Domestic;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageIn", DataProvider);

			voyage.ForeignPorts.Add("MYPKG");
			voyage.ForeignPorts.Add("SGSIN");

			AssertPortPairCount(voyage, 15);

			AssertPortPairValues("AU->AU", voyage.PortPairs[0], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[1], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[2], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);

			AssertPortPairValues("OS->AU", voyage.PortPairs[3], "MYPKG", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[4], "SGSIN", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[5], "MYPKG", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[6], "SGSIN", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[7], "MYPKG", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[8], "SGSIN", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Import);

			AssertPortPairValues("OS->AU", voyage.PortPairs[9], "AUSYD", "MYPKG", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("OS->AU", voyage.PortPairs[10], "AUSYD", "SGSIN", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("OS->AU", voyage.PortPairs[11], "AUMEL", "MYPKG", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("OS->AU", voyage.PortPairs[12], "AUMEL", "SGSIN", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("OS->AU", voyage.PortPairs[13], "AUPER", "MYPKG", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("OS->AU", voyage.PortPairs[14], "AUPER", "SGSIN", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Export);
		}

		public void TestPortPairs_FilterImportAndDomestic()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageIn", "VoyageOut");
			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Import | PortPairTypes.Domestic;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageIn", DataProvider);

			voyage.ForeignPorts.Add("MYPKG");
			voyage.ForeignPorts.Add("SGSIN");

			AssertPortPairCount(voyage, 15);
			AssertPortPairValues("AU->AU", voyage.PortPairs[0], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[1], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[2], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);

			AssertPortPairValues("OS->AU", voyage.PortPairs[3], "MYPKG", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[4], "SGSIN", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[5], "MYPKG", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[6], "SGSIN", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[7], "MYPKG", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[8], "SGSIN", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Import);
		}

		public void TestPortPairs_FilterImportDomesticExport_WhenVoyageInVoyageOutSame()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageIn");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageIn");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageIn", "VoyageIn");
			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.All;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageIn", DataProvider);

			voyage.ForeignPorts.Add("MYPKG");
			voyage.ForeignPorts.Add("SGSIN");

			AssertPortPairCount(voyage, 15);

			AssertPortPairValues("AU->AU", voyage.PortPairs[0], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[1], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[2], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);

			AssertPortPairValues("OS->AU", voyage.PortPairs[3], "MYPKG", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[4], "SGSIN", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[5], "MYPKG", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[6], "SGSIN", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[7], "MYPKG", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[8], "SGSIN", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Import);

			AssertPortPairValues("AU->OS", voyage.PortPairs[9], "AUSYD", "MYPKG", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[10], "AUSYD", "SGSIN", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[11], "AUMEL", "MYPKG", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[12], "AUMEL", "SGSIN", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[13], "AUPER", "MYPKG", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[14], "AUPER", "SGSIN", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Export);
		}

		public void TestPortPairs_FilterImportDomesticExport_WithOnly1DomesticPort()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.All;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageIn", DataProvider);

			voyage.ForeignPorts.Add("MYPKG");
			voyage.ForeignPorts.Add("SGSIN");

			AssertPortPairCount(voyage, 4);
			AssertPortPairValues("OS->AU", voyage.PortPairs[0], "MYPKG", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[1], "SGSIN", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("AU->OS", voyage.PortPairs[2], "AUSYD", "MYPKG", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[3], "AUSYD", "SGSIN", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
		}

		public void TestPortPairs_ForExportOutVoyage()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageIn", "VoyageOut");
			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Domestic | PortPairTypes.Export;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageIn", DataProvider);

			voyage.ForeignPorts.Add("NZAKL");
			voyage.ForeignPorts.Add("USLAX");

			AssertPortPairCount(voyage, 15);

			AssertPortPairValues("AU->OS", voyage.PortPairs[0], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[1], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[2], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Export);

			AssertPortPairValues("AU->OS", voyage.PortPairs[3], "NZAKL", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[4], "USLAX", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[5], "NZAKL", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[6], "USLAX", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[7], "NZAKL", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[8], "USLAX", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Export);

			AssertPortPairValues("AU->OS", voyage.PortPairs[9], "AUSYD", "NZAKL", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[10], "AUSYD", "USLAX", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[11], "AUMEL", "NZAKL", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[12], "AUMEL", "USLAX", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[13], "AUPER", "NZAKL", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[14], "AUPER", "USLAX", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Export);
		}

		public void TestPortPairs_ForExportVoyageThatDoesntGoOverseas()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageOut", "ReallyOut");
			NewJobVesselSchedule("AUBNE", ZDateTime.Today.AddDays(7), ZDateTime.Today.AddDays(8), "Lloyds", "VoyageOut", "ReallyOut");

			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Domestic;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageOut", DataProvider);
			AssertPortPairCount(voyage, 13);
			AssertPortPairValues("AU->AU", voyage.PortPairs[0], "", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[1], "", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[2], "", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[3], "", "AUBNE", ZDateTime.Empty, ZDateTime.Today.AddDays(7), PortPairTypes.Domestic);

			AssertPortPairValues("AU->AU", voyage.PortPairs[4], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[5], "AUSYD", "AUBNE", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(7), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[6], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[7], "AUMEL", "AUBNE", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(7), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[8], "AUPER", "AUBNE", ZDateTime.Today.AddDays(6), ZDateTime.Today.AddDays(7), PortPairTypes.Domestic);

			AssertPortPairValues("AU->AU", voyage.PortPairs[9], "AUSYD", "", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[10], "AUMEL", "", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[11], "AUPER", "", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[12], "AUBNE", "", ZDateTime.Today.AddDays(8), ZDateTime.Empty, PortPairTypes.Domestic);
		}

		public void TestPortPairs_ForStrangeVoyageThatHasInVoyageOnlyThenInAndOutVoyage()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageOut", "VoyageOut");
			NewJobVesselSchedule("AUBNE", ZDateTime.Today.AddDays(7), ZDateTime.Today.AddDays(8), "Lloyds", "VoyageOut", "VoyageOut");

			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Domestic | PortPairTypes.Export;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageOut", DataProvider);

			AssertPortPairCount(voyage, 13);

			AssertPortPairValues("AU->AU", voyage.PortPairs[0], "", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[1], "", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[2], "", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[3], "", "AUBNE", ZDateTime.Empty, ZDateTime.Today.AddDays(7), PortPairTypes.Domestic);

			AssertPortPairValues("AU->AU", voyage.PortPairs[4], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[5], "AUSYD", "AUBNE", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(7), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[6], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[7], "AUMEL", "AUBNE", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(7), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[8], "AUPER", "AUBNE", ZDateTime.Today.AddDays(6), ZDateTime.Today.AddDays(7), PortPairTypes.Domestic);

			AssertPortPairValues("AU->OS", voyage.PortPairs[9], "AUSYD", "", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[10], "AUMEL", "", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[11], "AUPER", "", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Export);
			AssertPortPairValues("AU->OS", voyage.PortPairs[12], "AUBNE", "", ZDateTime.Today.AddDays(8), ZDateTime.Empty, PortPairTypes.Export);
		}

		public void TestPortPairs_NoDuplicatesWhenReloadingDueToNewidPK()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageIn", "VoyageOut");
			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Domestic;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageIn", DataProvider);

			AssertPortPairCount(voyage, 9);

			AssertPortPairValues("AU->AU", voyage.PortPairs[0], "", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[1], "", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[2], "", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[3], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[4], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[5], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[6], "AUSYD", "", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[7], "AUMEL", "", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[8], "AUPER", "", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Domestic);

			Factory.ClearQueryCache(); // the data refresh bus will do this when any other factory saves
			voyage.PortPairs.Load();
			voyage.PortPairs.Load();

			AssertPortPairCount(voyage, 9);
			AssertPortPairValues("AU->AU", voyage.PortPairs[0], "", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[1], "", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[2], "", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[3], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[4], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[5], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[6], "AUSYD", "", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[7], "AUMEL", "", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Domestic);
			AssertPortPairValues("AU->AU", voyage.PortPairs[8], "AUPER", "", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Domestic);
		}

		public void TestPortPairs_IncludeEmptyLoadPortsWhenNoForeignPortCodesAvailable()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageIn", "VoyageOut");
			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Import;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageIn", DataProvider);

			AssertPortPairCount(voyage, 9);
			AssertPortPairValues("OS->AU", voyage.PortPairs[0], "", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[1], "", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[2], "", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[3], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[4], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[5], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[6], "AUSYD", "", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[7], "AUMEL", "", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[8], "AUPER", "", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Import);
		}

		public void TestPortPairs_IncludeEmptyDischargePortsWhenNoForeignPortCodesAvailable()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "VoyageIn", "VoyageOut");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds", "VoyageIn", "VoyageOut");
			Factory.Save();
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Export;
			VesselRoutingVoyage voyage = LoadVoyageByVoyageNumber("VoyageIn", DataProvider);

			AssertPortPairCount(voyage, 9);
			AssertPortPairValues("OS->AU", voyage.PortPairs[0], "", "AUSYD", ZDateTime.Empty, ZDateTime.Today.AddDays(1), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[1], "", "AUMEL", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[2], "", "AUPER", ZDateTime.Empty, ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[3], "AUSYD", "AUMEL", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[4], "AUSYD", "AUPER", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[5], "AUMEL", "AUPER", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[6], "AUSYD", "", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[7], "AUMEL", "", ZDateTime.Today.AddDays(4), ZDateTime.Empty, PortPairTypes.Import);
			AssertPortPairValues("OS->AU", voyage.PortPairs[8], "AUPER", "", ZDateTime.Today.AddDays(6), ZDateTime.Empty, PortPairTypes.Import);
		}

		public void TestPortPairs_AddForeignPort()
		{
			NewJobVesselSchedule("AUBNE", ZDateTime.Empty, ZDateTime.Today.AddDays(2), "Lloyds", "", "13002");
			NewJobVesselSchedule("AUBNE", ZDateTime.Today.AddDays(3), ZDateTime.Empty, "Lloyds", "13002", "");
			Factory.Save();

			VoyageCollection.PortPairTypeFilter = PortPairTypes.Import;
			var voyage = LoadVoyageByVoyageNumber("13002", DataProvider);

			voyage.ForeignPorts.Add("USLAX");
			voyage.PortPairs.Load();

			AssertPortPairCount(voyage, 2);

			AssertPortPairValues("US->AU", voyage.PortPairs[0], "USLAX", "AUBNE", ZDateTime.Empty, ZDateTime.Today.AddDays(3), PortPairTypes.Import);
			AssertPortPairValues("US->AU", voyage.PortPairs[1], "AUBNE", "USLAX", ZDateTime.Today.AddDays(2), ZDateTime.Empty, PortPairTypes.Import);
		}

		#endregion

		protected override ZString DataProvider { get { return FreightConstants.VesselDataProviders.OneStop; } }
	}
}
