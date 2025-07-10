using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingFilterBuilderTest_ToDeclarationFilter : SailingFilterBuilderTest<BusinessObject>
	{
		public override void TestDateFilter()
		{
			ZDateTime today = ZDateTime.Today;

			ExportSailing.Origin.JA_E_DEP = today.AddDays(-1);
			ExportSailing.Origin.JA_A_DEP = today.AddDays(-1);
			ExportSailing.Destination.JB_E_ARV = today.AddDays(1);
			ExportSailing.Destination.JB_A_ARV = today.AddDays(1);

			Factory.Save();

			BusinessObject[] bOs = CreateBusinessObjects(ExportSailing);

			Factory.Save();

			Builder.Reset();
			BusinessObject[] found;

			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs, found);

			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasDateInRange, today.AddDays(-1), today.AddDays(-1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATD, DateComparisonOperator.HasDateInRange, today.AddDays(-1), today.AddDays(-1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETA, DateComparisonOperator.HasDateInRange, today.AddDays(1), today.AddDays(1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATA, DateComparisonOperator.HasDateInRange, today.AddDays(1), today.AddDays(1));
		}

		#region Implementation

		protected override BusinessObject[] Load(ZQuery query)
		{
			return (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(query);
		}

		protected override BusinessObject[] CreateBusinessObjects(JobSailing sailing)
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol1.Transports[0].JW_JX = sailing.PK;
			consol1.Transports[0].JW_IsLinked = true;
			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			shipment1.Consols.Add(consol1);
			var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration1[JobDeclarationSchema.Constants.JE_JS] = shipment1.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration2[JobDeclarationSchema.JE_TransportMode] = sailing.JX_TransportMode;
			declaration2[JobDeclarationSchema.JE_RL_NKPortOfLoading] = sailing.JX_JA_RL_NKPortOfLoading;
			declaration2[JobDeclarationSchema.JE_RL_NKPortOfArrival] = sailing.JX_JB_RL_NKPortOfDischarge;
			declaration2[JobDeclarationSchema.JE_VesselName] = sailing.JX_JV_NKVessel;
			declaration2[JobDeclarationSchema.JE_VoyageFlightNo] = sailing.JX_JV_VoyageFlight;
			declaration2[JobDeclarationSchema.JE_ExportDate] = sailing.JX_JA_A_DEP;
			declaration2[JobDeclarationSchema.JE_DateOfArrival] = sailing.JX_JB_A_ARV;

			return new BusinessObject[] { declaration1, declaration2 };
		}

		protected override ZQuery GetFilter(SailingFilterBuilder builder)
		{
			return builder.ToDeclarationFilter();
		}

		protected override bool IsLinked(BusinessObject bo)
		{
			if (proxiedJobsAreLinked)
			{
				var shipment = Factory.Load<CommonShipment>((ZGuid)bo[JobDeclarationSchema.JE_JS]);
				return shipment != null && (!shipment.JS_JX.IsEmpty ||
					(shipment.TransportsIncludingRelated.Count > 0 && shipment.TransportsIncludingRelated[0].JW_IsLinked));
			}

			return false; //Declaration doesn't default or proxy its dates from linked shipment/schedule
		}

		protected override void MarkProxiedJobsAsLinked()
		{
			proxiedJobsAreLinked = true;
		}

		bool proxiedJobsAreLinked;

		#endregion
	}
}
