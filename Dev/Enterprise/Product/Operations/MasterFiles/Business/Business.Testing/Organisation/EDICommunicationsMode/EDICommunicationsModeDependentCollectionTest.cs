using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationsModeDependentCollection))]
	sealed class EDICommunicationsModeDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader parentHeader = OrgHeader.New(Factory);
			return new EDICommunicationsModeDependentCollection(parentHeader);
		}

		public void TestClientSpecificCommunicationTransport_Get()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			AssertEquals("ClientSpecificCommunicationTransport should be ZString.Empty with empty collections", ZString.Empty, collection.ClientSpecificCommunicationTransport);

			collection.AddNew();
			collection[0].EK_CommunicationsTransport = "xyz";
			collection.AddNew();
			collection[1].EK_CommunicationsTransport = "abc";
			AssertEquals("ClientSpecificCommunicationTransport should return empty when no client is set", ZString.Empty, collection.ClientSpecificCommunicationTransport);

			collection[1].EK_Module = EDICommunicationsMode.Modules.ClientSpecific;
			AssertEquals("ClientSpecificCommunicationTransport should return correct client", "abc", collection.ClientSpecificCommunicationTransport);
		}

		public void TestClientSpecificCommunicationTransport_Set()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.EDICommunicationsModes.ClientSpecificCommunicationTransport = "EML";
			header.EDICommunicationsModes.ClientSpecificDestination = "a@a.com";

			AssertEquals("Didn't return correct client mode", "EML", header.EDICommunicationsModes.ClientSpecificCommunicationTransport);
		}

		public void TestClientSpecificDestination_Get()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			AssertEquals("ClientSpecificDestination should be ZString.Empty with empty collections", ZString.Empty, collection.ClientSpecificDestination);

			collection.AddNew();
			collection[0].EK_Destination = "xyz";
			collection.AddNew();
			collection[1].EK_Destination = "abc";
			AssertEquals("ClientSpecificDestination should be ZString.Empty when no client is set", ZString.Empty, collection.ClientSpecificDestination);

			collection[1].EK_Module = EDICommunicationsMode.Modules.ClientSpecific;
			AssertEquals("ClientSpecificDestination should return correct client", "abc", collection.ClientSpecificDestination);
		}

		public void TestClientSpecificDestination_Set()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.EDICommunicationsModes.ClientSpecificCommunicationTransport = "EML";
			header.EDICommunicationsModes.ClientSpecificDestination = "a@a.com";

			AssertEquals("Didn't return correct client mode", "a@a.com", header.EDICommunicationsModes.ClientSpecificDestination);
		}

		public void TestGetXmlCommunicationMode_OnlyNonCLIMode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode = collection.AddNew();
			mode.EK_FileFormat = "XML";
			mode.EK_Module = "SAV";
			AssertNotNull("Couldn't find XmlCommunicationMode", collection.GetXmlCommunicationMode("SAV"));
		}

		public void TestGetXmlCommunicationMode_OnlyCLIMode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode = collection.AddNew();
			mode.EK_FileFormat = "XML";
			mode.EK_Module = "CLI";
			AssertNotNull("Couldn't find XmlCommunicationMode", collection.GetXmlCommunicationMode("CLI"));
		}

		public void TestGetXmlCommunicationMode_CLIandNonCLIModes()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode = collection.AddNew();
			mode.EK_FileFormat = "XML";
			mode.EK_Module = "SAV";

			mode = collection.AddNew();
			mode.EK_FileFormat = "XML";
			mode.EK_Module = "CLI";

			AssertEquals("Couldn't find correct XmlCommunicationMode", "SAV", collection.GetXmlCommunicationMode("SAV").EK_Module);
		}

		public void TestGetXmlCommunicationMode_GivenModeDoesntHaveXML()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode = collection.AddNew();
			mode.EK_FileFormat = "ALL";
			mode.EK_Module = "SAV";

			mode = collection.AddNew();
			mode.EK_FileFormat = "XML";
			mode.EK_Module = "CLI";

			AssertEquals("Couldn't find correct XmlCommunicationMode", "SAV", collection.GetXmlCommunicationMode("SAV").EK_Module);
		}

		public void TestGetXmlCommunicationMode_SameModeTwice()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode = collection.AddNew();
			mode.EK_FileFormat = "ALL";
			mode.EK_Module = "SAV";

			mode = collection.AddNew();
			mode.EK_FileFormat = "XML";
			mode.EK_Module = "SAV";

			AssertEquals("Couldn't find correct XmlCommunicationMode", "SAV", collection.GetXmlCommunicationMode("SAV").EK_Module);
			AssertEquals("Couldn't find correct FileFormat", "XML", collection.GetXmlCommunicationMode("SAV").EK_FileFormat);
		}

		public void TestGetXmlCommunicationMode_MultipleModes()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode = collection.AddNew();
			mode.EK_FileFormat = "ALL";
			mode.EK_Module = "SAV";

			mode = collection.AddNew();
			mode.EK_FileFormat = "XML";
			mode.EK_Module = "CLI";

			mode = collection.AddNew();
			mode.EK_FileFormat = "ALL";
			mode.EK_Module = "SHP";

			mode = collection.AddNew();
			mode.EK_FileFormat = "ALL";
			mode.EK_Module = "SHP";

			AssertEquals("Couldn't find correct XmlCommunicationMode", "SAV", collection.GetXmlCommunicationMode("SAV").EK_Module);
		}

		public void TestGetXmlCommunicationMode_SameModeTwiceNoMatchingFormat()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode = collection.AddNew();
			mode.EK_FileFormat = "";
			mode.EK_Module = "SAV";

			mode = collection.AddNew();
			mode.EK_FileFormat = "";
			mode.EK_Module = "SAV";

			AssertNull("Should return null with empty collection", collection.GetXmlCommunicationMode("SAV"));
		}

		public void TestGetXmlCommunicationMode_NoModes()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			AssertNull("Should return null with empty collection", collection.GetXmlCommunicationMode("CLI"));
		}

		public void TestGetXmlCommunicationMode_WithNonEmptyPurposeCanBeFound()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);

			EDICommunicationsMode mode1 = collection.AddNew();
			mode1.EK_FileFormat = "ALL";
			mode1.EK_Module = "CON";
			mode1.EK_MessagePurpose = "INV";

			AssertEquals("Should return mode1", mode1, collection.GetXmlCommunicationMode("CON"));

			EDICommunicationsMode mode2 = collection.AddNew();
			mode2.EK_FileFormat = "XML";
			mode2.EK_Module = "CON";
			mode2.EK_MessagePurpose = "INV";

			AssertEquals("Should return mode2, coz it's more specific", mode2, collection.GetXmlCommunicationMode("CON"));
		}

		public void TestQueryFilter()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			ZGuid guid = header.PK;
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode = collection.AddNew();
			mode.EK_Module = "SHN";
			mode = collection.AddNew();
			mode.EK_Module = "CLI";

			Factory.Save();
			header = Factory.Load<OrgHeader>(guid);
			AssertEquals("Shouldn't return shipnet modes", 1, header.EDICommunicationsModes.Count);
			AssertEquals("Should return correct mode", "CLI", header.EDICommunicationsModes[0].EK_Module);
		}

		public void TestFindByModule()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			EDICommunicationsModeDependentCollection modes = new EDICommunicationsModeDependentCollection(header);
			EDICommunicationsMode mode1 = modes.AddNew();
			EDICommunicationsMode mode2 = modes.AddNew();
			mode1.EK_Module = "SHP";
			mode2.EK_Module = "ORD";

			IList<EDICommunicationsMode> shipmentModes = modes.FindByModule("SHP");
			AssertCollectionContains(mode1, shipmentModes);
			AssertCollectionNotContains(mode2, shipmentModes);

			IList<EDICommunicationsMode> orderModes = modes.FindByModule("ORD");
			AssertCollectionContains(mode2, orderModes);
			AssertCollectionNotContains(mode1, orderModes);
		}

		public void TestFindByModuleAndFileFormat()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			EDICommunicationsModeDependentCollection collection = new EDICommunicationsModeDependentCollection(header);

			EDICommunicationsMode mode1 = collection.AddNew();
			mode1.EK_Module = "SHP";
			mode1.EK_FileFormat = "";
			EDICommunicationsMode mode2 = collection.AddNew();
			mode2.EK_Module = "SHP";
			mode2.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			EDICommunicationsMode mode3 = collection.AddNew();
			mode3.EK_Module = "SHP";
			mode3.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			EDICommunicationsMode mode4 = collection.AddNew();
			mode4.EK_Module = "SHP";
			mode4.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;

			IList<EDICommunicationsMode> xmlModes = collection.FindByModuleAndFileFormat("SHP", EDICommunicationsModeFileFormatList.Codes.XML);
			AssertEquals("Number of modes in the list", 2, xmlModes.Count);
			AssertEquals("Contains mode1?", false, xmlModes.Contains(mode1));
			AssertEquals("Contains mode2?", true, xmlModes.Contains(mode2));
			AssertEquals("Contains mode3?", false, xmlModes.Contains(mode3));
			AssertEquals("Contains mode4?", true, xmlModes.Contains(mode4));

			IList<EDICommunicationsMode> zzzModes = collection.FindByModuleAndFileFormat("SHP", "ZZZ");
			AssertEquals("Number of modes in the list", 1, zzzModes.Count);
			AssertEquals("Contains mode1?", false, zzzModes.Contains(mode1));
			AssertEquals("Contains mode2?", false, zzzModes.Contains(mode2));
			AssertEquals("Contains mode3?", false, zzzModes.Contains(mode3));
			AssertEquals("Contains mode4?", true, zzzModes.Contains(mode4));
		}
	}
}
