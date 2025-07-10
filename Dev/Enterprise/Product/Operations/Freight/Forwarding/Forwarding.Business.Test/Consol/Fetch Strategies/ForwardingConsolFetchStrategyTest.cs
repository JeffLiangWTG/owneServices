using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewForJobColums()
		{
			var jobStatus = nameof(ForwardingConsol.Job) + "+" + nameof(ForwardingConsol.Job.JH_Status);
			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn(string.Empty, jobStatus) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var holdReason = nameof(ForwardingConsol.Job) + "+" + nameof(ForwardingConsol.Job.JH_HoldReason);
			forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn(string.Empty, holdReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));
		}

		public void TestFetchForView_HoldReason()
		{
			AssertFetchForView(nameof(ForwardingConsol.Job) + "+" + JobHeader.Schema.JH_HoldReason, 1);
		}

		public void TestFetchForView_JobStatus()
		{
			AssertFetchForView(nameof(ForwardingConsol.Job) + "+" + JobHeader.Schema.JH_Status, 1);
		}

		public void TestFetchForView()
		{
			var factory = new BusinessObjectFactory();

			var clmTotals1 = CreateCLM(factory, 10, 30, 2, 5, 20, 1.5, 2, 10, .5);
			var clmTotals2 = CreateCLM(factory, 12, 33, 3, 7, 22, 1.8, 3, 11, .8);
			var clmTotals3 = CreateCLM(factory, 9, 25, 4, 5, 19, 1.4, 2, 8, .9);

			factory.Save();

			CombineAssertions("", () =>
			{
				CheckDBHitCount(ForwardingConsol.Schema.JK_TotalShipmentChargeable, 14);
				CheckDBHitCount(ForwardingConsol.Schema.JK_TotalShipmentVolume, 14);
				CheckDBHitCount(ForwardingConsol.Schema.JK_TotalShipmentWeight, 14);
				CheckDBHitCount(ForwardingConsol.Schema.JK_TotalShipmentPackageCount, 14);
				CheckDBHitCount(ForwardingConsol.Schema.JK_TotalShipmentQuantity, 14);
				CheckDBHitCount(ForwardingConsol.Schema.JK_CorrectedConsolWeight, 14);
				CheckDBHitCount(ForwardingConsol.Schema.JK_CorrectedConsolVolume, 14);
				CheckDBHitCount(ForwardingConsol.Schema.JK_ConsolChargeable, 14);
				CheckDBHitCount(nameof(ForwardingConsol.NumbersAsString), 1);
				CheckDBHitCount(nameof(ForwardingConsol.Job) + "+" + nameof(ForwardingConsol.Job.JH_Status), 1);
				CheckDBHitCount(nameof(ForwardingConsol.Job) + "+" + nameof(ForwardingConsol.Job.JH_HoldReason), 1);
			});

			var clms = Factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_AgentType, Constants.AgentType.AWBMaster));

			AssertValues(clmTotals1, clms.FirstOrDefault(x => x.PK == clmTotals1.PK));
			AssertValues(clmTotals2, clms.FirstOrDefault(x => x.PK == clmTotals2.PK));
			AssertValues(clmTotals3, clms.FirstOrDefault(x => x.PK == clmTotals3.PK));
		}

		void AssertFetchForView(string propertyName, int expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateCLM(Factory, 10, 30, 2, 5, 20, 1.5, 2, 10, .5);
			}
			Factory.Save();

			CheckDBHitCount(propertyName, expectedDbHits);
		}

		void CheckDBHitCount(string columnName, int dbHits)
		{
			var factory = new BusinessObjectFactory();
			var clms = factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_AgentType, Constants.AgentType.AWBMaster));

			foreach (var clm in clms)
			{
				clm.FetchStrategy.FetchForView(new[] { new TableColumn("", columnName), });
			}

			factory.ResetDatabaseLoadCount();

			var values = new List<object>();

			foreach (var clm in clms)
			{
				values.Add(clm[columnName]);
			}

			var message = ZString.Format("{0}, max hit count:{1}, actual count:{2}", columnName, dbHits, factory.DatabaseLoadCount);

			Assert(message, dbHits >= factory.DatabaseLoadCount);
		}

		void AssertValues(CLMTotals clmTotals, ForwardingConsol clm)
		{
			AssertEquals("weight for " + clm.JK_UniqueConsignRef, clmTotals.Weight, clm.JK_TotalShipmentWeight);
			AssertEquals("volume for " + clm.JK_UniqueConsignRef, clmTotals.Volume, clm.JK_CorrectedConsolVolume);
			AssertEquals("packs for " + clm.JK_UniqueConsignRef, clmTotals.Packs, clm.JK_TotalShipmentQuantity);
		}

		static CLMTotals CreateCLM(BusinessObjectFactory newFactory, int packageCount11, int actualWeight11, int actualVolume11, int packageCount12, int actualWeight12, double actualVolume12, int packageCount21, int actualWeight21, double actualVolume21)
		{
			var clm = newFactory.NewWithValidTestData<ForwardingConsol>();
			clm.JK_TransportMode = Constants.TransportModes.Air;
			clm.JK_RL_NKLoadPort = "AUSYD";
			clm.JK_RL_NKDischargePort = "NZAKL";
			clm.JK_AgentType = Constants.AgentType.AWBMaster;

			var cla1 = clm.ColoadConsols.AddNew();
			var shipment11 = cla1.Shipments.AddNew();
			var packline11 = shipment11.OuterPackLines.AddNew();
			packline11.JL_PackageCount = packageCount11;
			packline11.JL_F3_NKPackType = "PLT";
			packline11.JL_ActualWeight = actualWeight11;
			packline11.JL_ActualWeightUQ = "KG";
			packline11.JL_ActualVolume = actualVolume11;
			packline11.JL_ActualVolumeUQ = "M3";

			var shipment12 = cla1.Shipments.AddNew();
			var packline12 = shipment12.OuterPackLines.AddNew();
			packline12.JL_PackageCount = packageCount12;
			packline12.JL_F3_NKPackType = "PLT";
			packline12.JL_ActualWeight = actualWeight12;
			packline12.JL_ActualWeightUQ = "KG";
			packline12.JL_ActualVolume = actualVolume12;
			packline12.JL_ActualVolumeUQ = "M3";

			var cla2 = clm.ColoadConsols.AddNew();
			var shipment21 = cla2.Shipments.AddNew();

			var packline21 = shipment21.OuterPackLines.AddNew();
			packline21.JL_PackageCount = packageCount21;
			packline21.JL_F3_NKPackType = "KEG";
			packline21.JL_ActualWeight = actualWeight21;
			packline21.JL_ActualWeightUQ = "KG";
			packline21.JL_ActualVolume = actualVolume21;
			packline21.JL_ActualVolumeUQ = "M3";

			shipment11.UpdateShipmentFromOuterPackLines();
			shipment12.UpdateShipmentFromOuterPackLines();
			shipment21.UpdateShipmentFromOuterPackLines();

			return new CLMTotals(
				clm.PK,
				packline11.JL_ActualWeight + packline12.JL_ActualWeight + packline21.JL_ActualWeight,
				packline11.JL_ActualVolume + packline12.JL_ActualVolume + packline21.JL_ActualVolume,
				packline11.JL_PackageCount + packline12.JL_PackageCount + packline21.JL_PackageCount);
		}

		class CLMTotals
		{
			public CLMTotals(ZGuid pk, ZDecimal weight, ZDecimal volume, ZDecimal packs)
			{
				PK = pk;
				Weight = weight;
				Volume = volume;
				Packs = packs;
			}

			public ZGuid PK { get; }
			public ZDecimal Weight { get; }
			public ZDecimal Volume { get; }
			public ZDecimal Packs { get; }
		}
	}
}
