using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ContainerMovementFetchStrategy : TestCaseWithFactory
	{
		public void TestFetchForView_DepotPort()
		{
			// OrgAddress: 5
			// OrgAddressCapability: 4
			// OrgHeader: 4
			//Extra database hits caused by EffectiveRelatedPortCode Getter.
			BashFetchForView(ContainerMovement.Schema.DepotPort, 13);
		}

		public void TestFetchForView_E9_DetentionCompanyCode()
		{
			// JobContainerDetention: 1
			BashFetchForView(ContainerMovement.Schema.E9_DetentionCompanyCode, 1);
		}

		public void TestFetchForView_RelatedInfo_LocalClient_RequireRefContainerStock()
		{
			// JobContainer: 4
			// JobHeader: 4
			// JobShipment: 4
			// RefContainerStock: 1
			//Extra database hits caused by FallBackLocalClient Getter.
			BashFetchForView("RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.LocalClient, 13, false, false);
		}

		public void TestFetchForView_RelatedInfo_LocalClient_RequireResponsibleParty()
		{
			// OrgHeader: 1
			BashFetchForView("RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.LocalClient, 1);
		}

		public void TestFetchForView_RelatedInfo_Principal_RequireRefContainerStock()
		{
			// JobContainer: 4
			// JobShipment: 4
			// OrgHeader: 4
			// RefContainerStock: 1
			//Extra database hits caused by FallBackPrincipal Getter.
			BashFetchForView("RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.Principal, 13, false, false);
		}

		public void TestFetchForView_RelatedInfo_Principal_RequirePrincipal()
		{
			// OrgHeader: 1
			BashFetchForView("RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.Principal, 1);
		}

		public void TestFetchForView_RelatedInfo_ExportDetentionFreeDays()
		{
			// OrgAddress: 5				//Reduce 3 hits by requireDepotAddress
			// JobContainer: 4
			// JobShipment: 4
			// OrgAddressCapability: 4
			// OrgContainerDetention: 4
			// OrgHeader: 4					//Reduce 8 hits by requireResponsibleParty and requirePrincipal
			// RefContainer: 1
			// RefContainerStock: 1			//Reduce 3 hits by requireRefContainerStock
			//Extra database hits caused by CalculateExportDetentionFreeDays method.
			BashFetchForView("RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.ExportDetentionFreeDays, 27);
		}

		public void TestFetchForView_RelatedInfo_ImportDetentionFreeDays()
		{
			// OrgAddress: 5				//Reduce 3 hits by requireDepotAddress
			// JobContainer: 4
			// JobShipment: 4
			// OrgAddressCapability: 4
			// OrgContainerDetention: 4
			// OrgHeader: 4					//Reduce 8 hits by requireResponsibleParty and requirePrincipal
			// RefContainer: 1
			// RefContainerStock: 1			//Reduce 3 hits by requireRefContainerStock
			//Extra database hits caused by CalculateImportDetentionFreeDays method.
			BashFetchForView("RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.ImportDetentionFreeDays, 27);
		}

		public void TestFetchForView_Stock_R6_ContainerNum()
		{
			// RefContainerStock: 1
			BashFetchForView("Stock+" + RefContainerStock.Schema.R6_ContainerNum, 1);
		}

		public void TestFetchForView_RelatedInfo_Consignee()
		{
			// JobContainer: 4
			// JobDocAddress: 4
			// JobShipment: 4
			// OrgAddress: 4
			// OrgHeader: 4
			// RefContainerStock: 1
			//Extra database hits caused by GetConsignee method.
			BashFetchForView("RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.Consignee, 21);
		}

		public void TestFetchForView_Voyage_JV_RV_NKVessel()
		{
			// JobVoyage: 1
			BashFetchForView("Voyage+" + JobVoyage.Schema.JV_RV_NKVessel, 1);
		}

		public void TestFetchForView_E9_OA_Depot_ZAddress_OrgPK()
		{
			// OrgAddress: 1
			BashFetchForView(AutoJobContainerMove.Schema.E9_OA_Depot + ZAddress.Schema.OrgPK, 1);
		}

		#region Implementation
		static void BashFetchForView(string bindToString, int maxDbHits, bool needResponsibleParty = true, bool needPrincipal = true)
		{
			var factory = new BusinessObjectFactory();
			var movements = factory.Load<ContainerMovement>(new ZQuery(JobContainerMoveSchema.PK, CreateMovements(needResponsibleParty, needPrincipal)));
			factory.ResetDatabaseLoadCount();
			foreach (var movement in movements)
			{
				movement.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn("", bindToString) });
			}

			foreach (var movement in movements)
			{
				HitProperty(movement, bindToString);
			}

			AssertMaxDbHits(maxDbHits, factory);
		}

		static void HitProperty(object root, string pathStr)
		{
			var path = pathStr.Split(new char[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);
			var o = root;
			for (var i = 0; i < path.Length; i++)
			{
				if (o == null)
				{
					var message = string.Join("+", path, 0, i) + " returned null, you should populate your data better";
					throw new ApplicationException(message);
				}

				o = o.GetType().InvokeMember(path[i], BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, o, Array.Empty<object>());
			}
		}

		static List<ZGuid> CreateMovements(bool needResponsibleParty = true, bool needPrincipal = true)
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			for (var i = 0; i < 4; i++)
			{
				var carrier = factory.NewWithValidTestData<OrgHeader>();
				var principal = factory.NewWithValidTestData<OrgHeader>();
				var consignee = factory.NewWithValidTestData<OrgHeader>();
				var vessel = factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;
				var voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();
				var shipment = factory.NewWithValidTestData<AgencyShipment>();
				shipment.JS_UniqueConsignRef = string.Format("S000001{0:00}", i);
				shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
				shipment.JS_OH_DeliveryAgent = principal.PK;
				shipment.JS_JX = voyage.Sailings[0].PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
				var container = shipment.RealContainers.AddNew();
				var stock = factory.New<RefContainerStock>();
				stock.R6_ContainerNum = string.Format("TEST41000{0:00}", i);
				stock.R6_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_ContainerNum = stock.R6_ContainerNum;
				var detention = factory.NewWithValidTestData<ContainerDetention>();
				var depot = factory.NewWithValidTestData<OrgHeader>();
				detention.NC_GC = GlbCompany.CurrentCompany.PK;
				var movement = stock.Movements.AddNew();
				movement.E9_JV = voyage.PK;
				movement.E9_OA_Depot = depot.MainAddress.PK;
				movement.E9_NC = detention.PK;
				if (needResponsibleParty)
				{
					movement.E9_OH_ResponsibleParty = factory.NewWithValidTestData<OrgHeader>().PK;
				}

				if (needPrincipal)
				{
					movement.E9_OH_Principal = factory.NewWithValidTestData<OrgHeader>().PK;
				}

				result.Add(movement.PK);
			}

			factory.Save();
			return result;
		}
		#endregion
	}
}
