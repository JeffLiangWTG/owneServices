using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ContainerMovementDataContextManager))]
	internal class ContainerMovementDataContextManagerTest : DataContextManagerTestCase<ContainerMovementDataContextManager, ContainerMovement>
	{
		public void TestEventContextValues()
		{
			var context = new ContainerMovementDataContextManager();
			var stock = Factory.New<RefContainerStock>();
			var movement = stock.Movements.AddNew();
			movement.E9_MovementDate = ZDateTime.Now;
			movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "AA1918AB";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_RadioCallSign = "HEEEELP!!!";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "S18181818";
			shipment.JS_HouseBill = "BNESYD01000";
			shipment.JS_CFSReference = "BOOKINGNUM";
			shipment.JS_JX = voyage.Sailings[0].PK;
			shipment.JS_E_ARV = new ZDateTime(2012, 07, 01);
			shipment.JS_E_DEP = new ZDateTime(2012, 07, 11);
			shipment.JS_RL_NKDestination = "UAODS";
			shipment.JS_GoodsDescription = "desc";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "AAAAA aaaa";
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "BBBB bbbbb";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			var num = shipment.CusEntryNumbers.AddNew();
			num.CE_EntryNum = "EntryNum";
			num.CE_EntryType = CusEntryNumberTypes.Australia.CRN;
			num.CE_ParentTable = shipment.TableName;
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";
			container.JC_SealNum = "SealNum";
			container.JC_SetPointTemp = 12.5d;
			container.JC_SetPointTempUnit = "C";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_OwnerType = "CAR";
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			owner.OH_FullName = "Blah blah";
			stock.R6_OH_Owner = owner.PK;
			var depot = Factory.LoadTop1<OrgHeader>(new ZQuery());
			depot.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			movement.E9_JV = voyage.PK;
			movement.E9_OA_Depot = depot.MainAddress.PK;
			movement.E9_ContainerCondition = DefaultContainerDamageList.Codes.Available;
			movement.E9_ContainerQuality = DefaultContainerCleanList.Codes.Cotton;
			movement.E9_LeaseNumber = "CONTRACT1";
			Factory.SaveForTesting();
			((IDataContextManager)context).Init(movement);
			var values = context.EventContextValues.ToArray();
			AssertEquals(32, values.Length);
			AssertContains(values, "VoyageNumber", "AA1918AB");
			AssertContains(values, "LloydsNumber", vessel.RV_LloydsNumber);
			AssertContains(values, "VesselName", vessel.RV_Name);
			AssertContains(values, "VesselCallSign", "HEEEELP!!!");
			AssertContains(values, "LegOriginUNLOCO", "UAIEV");
			AssertContains(values, "ContainerNumber", "TEST4100013");
			AssertContains(values, "ContainerOwnershipType", "CAR");
			AssertContains(values, "ContainerISOCode", "22G0");
			AssertContains(values, "ContainerGrossWeight", "24000.000 KG");
			AssertContains(values, "ContainerMovementType", "WGI");
			AssertContains(values, "ContainerDamageCode", DefaultContainerDamageList.Codes.Available);
			AssertContains(values, "ContainerConditionCode", DefaultContainerCleanList.Codes.Cotton);
			AssertContains(values, "ContainerLeaseNumber", "CONTRACT1");
			AssertContains(values, "IsEmptyContainer", ZBool.False);
			AssertContains(values, "CarriersBookingReference", "BOOKINGNUM");
			AssertContains(values, "EntryNumber", "EntryNum");
			AssertContains(values, "EntryNumberType", CusEntryNumberTypes.Australia.CRN);
			AssertContains(values, "EntryNumberCountryOfIssue", new ZString(Constants.CountryCodes.Australia));
			AssertContains(values, "MBOLOriginUNLOCO", "UAIEV");
			AssertContains(values, "MBOLDestinationUNLOCO", "AUSYD");
			AssertContains(values, "EventActionUNLOCO", "USLAX");
			AssertContains(values, "ContainerSealNo", "SealNum");
			AssertContains(values, "MBOLNumber", "BNESYD01000");
			AssertContains(values, "EstimatedTimeOfArrival", new ZDateTime(2012, 07, 01));
			AssertContains(values, "EstimatedTimeOfDeparture", new ZDateTime(2012, 07, 11));
			AssertContains(values, "ContainerDestinationUNLOCO", "UAODS");
			AssertContains(values, "ContainerFinalDestinationUNLOCO", "UAODS");
			AssertContains(values, "ContainerConsigneeName", "AAAAA aaaa");
			AssertContains(values, "ContainerConsignorName", "BBBB bbbbb");
			AssertContains(values, "ContainerTemperatureSetting", "12.5 C");
			AssertContains(values, "ContainerGoodsDescription", "desc");
			AssertContains(values, "ContainerOwnerName", "Blah blah");
		}

		void AssertContains(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> array, ZString type, object value)
		{
			var pair = array.FirstOrDefault(a => a.Key.Type.Equals(type));
			AssertNotNull(pair);
			AssertEquals(value, pair.Value);
		}
	}
}
