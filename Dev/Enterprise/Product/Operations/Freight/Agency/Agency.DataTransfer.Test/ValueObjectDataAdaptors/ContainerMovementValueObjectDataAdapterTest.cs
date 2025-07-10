using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(ContainerMovementValueObjectDataAdapter))]
	internal class ContainerMovementValueObjectDataAdapterTest : ValueObjectDataAdapterResourceAgnosticTest<ContainerMovement, Xsd.ContainerMovement>
	{
		#region Implementation
		protected override string ExpectedRootCollectionElementName
		{
			get
			{
				return "ContainerMovements";
			}
		}

		protected override string ExpectedRootElementName
		{
			get
			{
				return "ContainerMovement";
			}
		}

		protected override BusinessObjectSampleAndExpectedOutput GetEmptyBusinessObjectSampleAndExpectedOutput()
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			stock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			stock.R6_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementType = ContainerMovementTypes.Codes.Load;
			movement.E9_MovementDate = new ZDateTime(2009, 05, 11, 12, 15, 00);
			return new BusinessObjectAndExpectedOutputResource(movement, "Enterprise.Freight.Agency.DataTransfer.Test.TestFiles.EmptyContainerMovement.xml", ValidationKind.None, "Empty");
		}

		protected override BusinessObjectSampleAndExpectedOutput GetFullyPopulatedBusinessObjectSampleAndExpectedOutput()
		{
			ZGuid voyagePK;
			ZString containerNum;
			ZGuid depotPK;
			CreateBackingData(out voyagePK, out containerNum, out depotPK);
			RefContainerStock stock = RefContainerStock.Load(Factory, containerNum);
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementType = ContainerMovementTypes.Codes.Load;
			movement.E9_MovementDate = new ZDateTime(2009, 05, 11, 12, 15, 00);
			movement.E9_ContainerIsEmpty = true;
			movement.E9_ContainerCondition = "DAM";
			movement.E9_JV = voyagePK;
			movement.E9_OA_Depot = depotPK;
			return new BusinessObjectAndExpectedOutputResource(movement, "Enterprise.Freight.Agency.DataTransfer.Test.TestFiles.FullContainerMovement.xml", ValidationKind.None, "Fully Populated");
		}

		protected override ValueObjectDataAdapter<ContainerMovement, Xsd.ContainerMovement> GetNewBizObjXmlDataAdapter()
		{
			return new ContainerMovementValueObjectDataAdapter();
		}

		protected override BusinessObjectSampleAndExpectedOutput GetPopulatedBusinessObjectWithEmptyFieldsSampleAndExpectedOutput()
		{
			return GetEmptyBusinessObjectSampleAndExpectedOutput();
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get
			{
				return false;
			}
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[] { "Depot", // child elements of the organisations are covered by another adapter.
 "RelatedBills", // child elements of the bill are covered by another adapter.
 "Stock" };
			}
		}

		void CreateBackingData(out ZGuid voyagePK, out ZString containerNum, out ZGuid depotPK)
		{
			if (!backingDataCreated)
			{
				backingDataCreated = true;
				CreateBackingDataCore(out backingDataVoyagePK, out backingDataContainerNum, out backingDataDepotPK);
			}

			voyagePK = backingDataVoyagePK;
			containerNum = backingDataContainerNum;
			depotPK = backingDataDepotPK;
		}

		static void CreateBackingDataCore(out ZGuid voyagePK, out ZString containerNum, out ZGuid depotPK)
		{
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			OrgHeader principal = otherFactory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";
			OrgHeader carrier = otherFactory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			OrgHeader consignor = otherFactory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			OrgHeader consignee = otherFactory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			OrgHeader bookingParty = otherFactory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BPARTY";
			OrgHeader depot = otherFactory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "DEPOT";
			JobVoyage voyage = otherFactory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", otherFactory).First().RV_FK;
			voyage.JV_VoyageFlight = "001N";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			BillOfLading bill1 = otherFactory.New<BillOfLading>();
			bill1.JS_UniqueConsignRef = "V00000101";
			bill1.JS_HouseBill = "Bill1";
			bill1.JS_CFSReference = "BookingRef1";
			bill1.JS_JX = voyage.Sailings[0].PK;
			bill1.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			bill1.JS_OH_DeliveryAgent = principal.PK;
			bill1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			bill1.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			bill1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			bill1.CustomsEntryNumberType = "CCN";
			bill1.CustomsEntryNumber = "FEFEFE";
			BillOfLadingContainer container1a = bill1.RealContainers.AddNew();
			container1a.JC_ContainerNum = "TEST4100029";
			container1a.JC_RC = otherFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container1a.JC_SetPointTempUnit = "C";
			BillOfLadingContainer container1b = bill1.RealContainers.AddNew();
			container1b.JC_ContainerNum = "TEST4100013";
			container1b.JC_RC = otherFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1b.JC_SetPointTempUnit = "C";
			BillOfLading bill2 = otherFactory.New<BillOfLading>();
			bill2.JS_UniqueConsignRef = "V00000102";
			bill2.JS_HouseBill = "Bill2";
			bill2.JS_CFSReference = "BookingRef2";
			bill2.JS_JX = voyage.Sailings[0].PK;
			bill2.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			bill2.JS_OH_DeliveryAgent = principal.PK;
			bill2.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			bill2.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			bill2.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			BillOfLadingContainer container2a = bill2.RealContainers.AddNew();
			container2a.JC_ContainerNum = "TEST4100034";
			container2a.JC_RC = otherFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container2a.JC_SetPointTempUnit = "C";
			BillOfLading bill3 = otherFactory.New<BillOfLading>();
			bill3.JS_UniqueConsignRef = "V00000103";
			bill3.JS_HouseBill = "Bill3";
			bill3.JS_CFSReference = "BookingRef3";
			bill3.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			bill3.JS_OH_DeliveryAgent = principal.PK;
			bill3.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			bill3.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			bill3.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			BillOfLadingContainer container3a = bill3.RealContainers.AddNew();
			container3a.JC_ContainerNum = "TEST4100029";
			container3a.JC_RC = otherFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container3a.JC_SetPointTempUnit = "C";
			otherFactory.Save();
			voyagePK = voyage.PK;
			containerNum = container1a.JC_ContainerNum;
			depotPK = depot.MainAddress.PK;
		}

		bool backingDataCreated;
		ZGuid backingDataVoyagePK;
		ZString backingDataContainerNum;
		ZGuid backingDataDepotPK;

		#endregion
	}
}
