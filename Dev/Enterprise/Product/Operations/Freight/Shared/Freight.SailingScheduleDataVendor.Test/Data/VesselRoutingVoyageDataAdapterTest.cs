using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingVoyageDataAdapter))]
	sealed class VesselRoutingVoyageDataAdapterTest : ValueObjectDataAdapterResourceAgnosticTest<VesselRoutingVoyage, Xsd.Schedule>
	{
		#region Overrides for base test

		protected override ValueObjectDataAdapter<VesselRoutingVoyage, Xsd.Schedule> GetNewBizObjXmlDataAdapter()
		{
			return new VesselRoutingVoyageDataAdapter(Factory);
		}

		protected override BusinessObjectSampleAndExpectedOutput GetEmptyBusinessObjectSampleAndExpectedOutput()
		{
			VesselRoutingVoyageCollection voyageCollection = new VesselRoutingVoyageCollection(Factory);
			VesselRoutingVoyage voyage = voyageCollection.AddNew();
			return new BusinessObjectAndExpectedOutputResource(voyage, "Enterprise.Freight.SailingScheduleDataVendor.Test.Data.TestFiles.VesselRoutingVoyageEmpty.xml", ValidationKind.None, "Empty business object");
		}

		protected override BusinessObjectSampleAndExpectedOutput GetPopulatedBusinessObjectWithEmptyFieldsSampleAndExpectedOutput()
		{
			return GetEmptyBusinessObjectSampleAndExpectedOutput();
		}

		protected override BusinessObjectSampleAndExpectedOutput GetFullyPopulatedBusinessObjectSampleAndExpectedOutput()
		{
			VesselRoutingVoyageCollection voyageCollection = new VesselRoutingVoyageCollection(Factory);
			VesselRoutingVoyage voyage = voyageCollection.AddNew();

			voyage.E8_OH_LineOperator = LineOperator.PK;
			voyage.E8_LloydsNumber = Vessel.RV_LloydsNumber;
			voyage.E8_Voyage = "Voyage";

			VesselRoutingPortPair decoyPortPair = voyage.PortPairs.AddNew();
			decoyPortPair.E9_DataProviderReference = "providerRef";
			decoyPortPair.E9_RL_NKLoadPort = "MYPKG";
			decoyPortPair.E9_RL_NKDischargePort = "AUSYD";
			decoyPortPair.E9_IsSelected = false;

			VesselRoutingPortPair portPair = voyage.PortPairs.AddNew();
			portPair.E9_DataProviderReference = "providerRef";
			portPair.E9_RL_NKLoadPort = "AUSYD";
			portPair.E9_RL_NKDischargePort = "AUMEL";
			portPair.E9_IsSelected = true;

			portPair.E9_ETD = new ZDateTime(2000, 1, 11);
			portPair.E9_ATD = new ZDateTime(2000, 1, 12);
			portPair.E9_ETA = new ZDateTime(2000, 1, 13);
			portPair.E9_ATA = new ZDateTime(2000, 1, 14);

			portPair.E9_CargoCutOff = new ZDateTime(2000, 1, 1);
			portPair.E9_ExportReceivalCommences = new ZDateTime(2000, 1, 2);
			portPair.E9_ImportAvailability = new ZDateTime(2000, 1, 21);
			portPair.E9_ImportStorageCommences = new ZDateTime(2000, 1, 22);

			return new BusinessObjectAndExpectedOutputResource(voyage, "Enterprise.Freight.SailingScheduleDataVendor.Test.Data.TestFiles.VesselRoutingVoyageFullyPopulated.xml", ValidationKind.Xsd | ValidationKind.FactorySave, "Populated business object");
		}

		protected override BusinessObjectSampleAndExpectedOutput[] GetMiscBusinessObjectSamplesAndExpectedOutputs()
		{
			return System.Array.Empty<BusinessObjectSampleAndExpectedOutput>();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"Carrier/EDICode",
					"Carrier/OwnerCode",
					"Carrier/OrganisationDetails",
					"Carrier/Notes/CustomNoteTypeName",
					"Carrier/Notes/NoteData",
					"Carrier/Notes/NoteCreatedDateTime"
				};
			}
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Schedules"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Schedule"; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		RefVessel Vessel
		{
			get
			{
				if (fVessel == null)
				{
					fVessel = Factory.New<RefVessel>();
					fVessel.RV_LloydsNumber = "Lloyds";
					fVessel.RV_Name = "VesselName";
				}
				return fVessel;
			}
		}
		RefVessel fVessel;

		OrgHeader LineOperator
		{
			get
			{
				if (fLineOperator == null)
				{
					fLineOperator = Factory.NewWithValidTestData<OrgHeader>();
					fLineOperator.OH_FullName = "Line Operator";
				}
				return fLineOperator;
			}
		}
		OrgHeader fLineOperator;

		#endregion
	}
}
