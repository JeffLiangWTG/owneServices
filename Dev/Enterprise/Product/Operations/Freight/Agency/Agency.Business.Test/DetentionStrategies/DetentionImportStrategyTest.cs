using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DetentionImportStrategy))]
	internal class DetentionImportStrategyTest : DetentionStrategyBaseTest<DetentionImportStrategy>
	{
		[TestDate(2013, 3, 8)]
		public void TestDefaultDetentionDays()
		{
			ImportContainer.JC_ContainerNum = Stock.R6_ContainerNum;
			ImportContainer.JC_EmptyReturnedBy = ZDateTime.Now.AddDays(-10);
			Factory.Save();
			Movement.E9_MovementDate = ZDateTime.Now.AddDays(-2);
			Movement.E9_JV = ZGuid.Empty;
			Movement.E9_DetentionDays = 4;
			AssertEquals("No voyage", (short)0, MovementType.GetDefaultDetentionDays(Movement));
			Movement.E9_JV = Voyage.PK;
			AssertEquals("Match", (short)8, MovementType.GetDefaultDetentionDays(Movement));
			Movement.E9_MovementDate = ZDateTime.Empty;
			AssertEquals("No movement date", (short)0, MovementType.GetDefaultDetentionDays(Movement));
		}

		public void TestDetentionPeriod()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				var factory = new BusinessObjectFactory();
				OrgHeader client = factory.NewWithValidTestData<OrgHeader>();
				client.OH_Code = "Client";
				OrgHeader principal = factory.NewWithValidTestData<OrgHeader>();
				principal.OH_Code = "Principal";
				OrgHeader depot = factory.NewWithValidTestData<OrgHeader>();
				depot.OH_Code = "Depot";
				depot.OH_RL_NKClosestPort = "AUBNE";
				OrgContainerDetention detentionFree = principal.CarrierContainerPenalties.AddNew();
				detentionFree.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detentionFree.PD_OH_Client = client.PK;
				detentionFree.PD_FreeDays = 15;
				detentionFree.PD_ContainerType = "20F";
				detentionFree.PD_DetentionPortOrCountry = "AU";
				ZDateTime available = ZDateTime.Today.AddDays(-30);
				ZDateTime lastfreeday = available.AddDays(15 - 1); // the last of 15 free days including the availability date.
				ZDateTime returned = lastfreeday.AddDays(15); // 15 days in detention
				var voyage = factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				var importShipment = factory.New<BillOfLading>();
				importShipment.JS_JX = voyage.Sailings[0].PK;
				importShipment.JS_OH_DeliveryAgent = principal.PK;
				importShipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
				importShipment.Sailing.Destination.JB_AvailabilityDate = available;
				var containerStock = factory.New<RefContainerStock>();
				containerStock.R6_ContainerNum = "TEST4100013";
				containerStock.R6_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				var importContainer = importShipment.RealContainers.AddNew();
				importContainer.JC_ContainerNum = containerStock.R6_ContainerNum;
				importContainer.JC_EmptyReturnedBy = lastfreeday;
				factory.Save();
				Movement.E9_OA_Depot = depot.MainAddress.PK;
				Movement.E9_JV = voyage.PK;
				Movement.E9_MovementDate = returned;
				AssertEquals(available, MovementType.GetStartOfDetentionFreePeriod(Movement));
				AssertEquals(lastfreeday.AddDays(1), MovementType.GetStartOfDetentionPeriod(Movement));
				AssertEquals((short)15, MovementType.GetDetentionFreeDays(Movement));
			}
		}

		#region Implementation
		protected override string[] GetDetentionTypes()
		{
			return new string[] { DetentionInvoiceType.Codes.Import, };
		}
		#endregion
	}
}
