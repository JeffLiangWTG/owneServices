using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerFetchStrategy : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AgencyShipment[] shipments = factory.Load<AgencyShipment>(new ZQuery(JobShipmentSchema.PK, CreateContainers()));
			AssertMaxDbHits(1, factory);
		}

		public void TestFetchForView_Booking_BookingPartyNameOrPK()
		{
			// JobDocAddress: 1
			// JobShipment: 1
			// OrgAddress: 1
			// OrgHeader: 1
			BashFetchForView("Booking+" + AgencyShipment.Schema.BookingPartyNameOrPK, 4);
		}

		public void TestFetchForView_Booking_ConsigneeNameOrPK()
		{
			// JobDocAddress: 1
			// JobShipment: 1
			// OrgAddress: 1
			// OrgHeader: 1
			BashFetchForView("Booking+" + AgencyShipment.Schema.ConsigneeNameOrPK, 4);
		}

		public void TestFetchForView_Booking_ConsignorNameOrPK()
		{
			// JobDocAddress: 1
			// JobShipment: 1
			// OrgAddress: 1
			// OrgHeader: 1
			BashFetchForView("Booking+" + AgencyShipment.Schema.ConsignorNameOrPK, 4);
		}

		public void TestFetchForView_JC_EIDOStatus()
		{
			BashFetchForView(AgencyShipmentContainer.Schema.JC_ImportReleaseOrderStatus, 1);
		}

		public void TestFetchForView_JC_Calc_ExportDetentionFreeDays()
		{
			// OrgContainerDetention: 12 -- not resolvable
			// JobConsolTransport: 1
			// JobDocAddress: 1
			// JobSailing: 1
			// JobShipment: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// OrgAddress: 1
			// OrgHeader: 1
			BashFetchForView(AgencyShipmentContainer.Schema.JC_Calc_ExportDetentionFreeDays, 21);
		}

		public void TestFetchForView_JC_Calc_ImportDetentionFreeDays()
		{
			// OrgContainerDetention: 12 -- not resolvable
			// JobDocAddress: 1
			// JobShipment: 1
			// OrgAddress: 1
			// OrgHeader: 1
			BashFetchForView(AgencyShipmentContainer.Schema.JC_Calc_ImportDetentionFreeDays, 16);
		}

		public void TestFetchForView_JC_ContainerYardEmptyReturnGateIn()
		{
			// JobConsolTransport: 1
			// JobSailing: 1
			// JobShipment: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// RefContainerStock: 1
			BashFetchForView(AgencyShipmentContainer.Schema.JC_ContainerYardEmptyReturnGateIn, 7);
		}

		public void TestFetchForView_Booking_JS_UniqueConsignRef()
		{
			BashFetchForView("Booking+" + AgencyShipment.Schema.JS_UniqueConsignRef, 1);
		}

		public void TestFetchForView_BillContainersEntryNumberType()
		{
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// JobShipment: 1
			BashFetchForView(AgencyShipmentContainer.Schema.BillContainersEntryNumberType, 4);
		}

		public void TestFetchForView_BillContainersEntryNumber()
		{
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// JobShipment: 1
			BashFetchForView(AgencyShipmentContainer.Schema.BillContainersEntryNumber, 5);
		}

		public void TestFetchForView_CustomsEntryNumberType()
		{
			BashFetchForView(AgencyShipmentContainer.Schema.CustomsEntryNumberType, 1);
		}

		public void TestFetchForView_CustomsEntryNumber()
		{
			BashFetchForView(AgencyShipmentContainer.Schema.CustomsEntryNumber, 1);
		}

		public void TestFetchForView_AdditionalReferenceNumbersAsString()
		{
			BashFetchForView(nameof(AgencyShipmentContainer.AdditionalReferenceNumbersAsString), 1);
		}

		#region Implementation
		static void BashFetchForView(string bindToString, int maxDbHits)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AgencyShipmentContainer[] containers = factory.Load<AgencyShipmentContainer>(new ZQuery(JobContainerSchema.PK, CreateContainers()));
			factory.ResetDatabaseLoadCount();
			foreach (AgencyShipmentContainer container in containers)
			{
				container.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn("", bindToString) });
			}

			foreach (AgencyShipmentContainer container in containers)
			{
				HitProperty(container, bindToString);
			}

			AssertMaxDbHits(maxDbHits, factory);
		}

		static void HitProperty(object root, string pathStr)
		{
			string[] path = pathStr.Split(new char[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);
			object o = root;
			for (int i = 0; i < path.Length; i++)
			{
				if (o == null)
				{
					string message = string.Join("+", path, 0, i) + " returned null, you should populate your data better";
					throw new ApplicationException(message);
				}

				o = o.GetType().InvokeMember(path[i], BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, o, Array.Empty<object>());
			}
		}

		static List<ZGuid> CreateContainers()
		{
			List<ZGuid> result = new List<ZGuid>();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			for (int i = 0; i < 12; i++)
			{
				OrgHeader carrier = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader principal = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader bookingParty = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader consignor = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader consignee = factory.NewWithValidTestData<OrgHeader>();
				var vessel = factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;
				JobVoyage voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();
				AgencyShipment shipment = factory.NewWithValidTestData<AgencyShipment>();
				shipment.JS_UniqueConsignRef = string.Format("S000001{0:00}", i);
				shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
				shipment.JS_OH_DeliveryAgent = principal.PK;
				shipment.JS_JX = voyage.Sailings[0].PK;
				shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
				AgencyShipmentContainer container = shipment.RealContainers.AddNew();
				container.JC_ContainerNum = string.Format("TEST41000{0:00}", i);
				result.Add(container.PK);
			}

			factory.Save();
			return result;
		}
		#endregion
	}
}
