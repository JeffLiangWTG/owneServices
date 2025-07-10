using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BillOfLadingNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(FreightDataRegistry.Instance.HouseBillNumberCustomisation, "", "HBL");

			BillOfLadingNumberGeneratorTarget target = new BillOfLadingNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the HouseBillNumberCustomisation", "HBL", target.NumberCustomisation);
			AssertLocation(FreightDataRegistry.Instance.HouseBillNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobShipmentSchema.JS_HouseBill.MaxLength, target.MaxLength);
			AssertEquals("bill of lading", target.Name);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNumberGeneratorByServiceLevelInMultipleThreads()
		{
			var setEvent = new ManualResetEvent(false);
			var task1 = Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					try
					{
						connection.BeginTransaction();
						var factory = new BusinessObjectFactory(connection);
						try
						{
							Set(FreightDataRegistry.Instance.HouseBillNumberCustomisation, "", "HBL");
							Env.NumberFountains.JobShipmentNumber.SetNext(factory, 1001);
						}
						finally
						{
							setEvent.Set();
						}

						for (int i = 0; i < 100; i++)
						{
							RunGenerate(factory);
						}
					}
					finally
					{
						connection.RollbackTransaction();
					}
				}
			});

			var task2 = Task.Run(() =>
			{
				setEvent.WaitOne();
				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					try
					{
						connection.BeginTransaction();
						var factory = new BusinessObjectFactory(connection);
						for (int i = 0; i < 100; i++)
						{
							RunGenerate(factory);
						}
					}
					finally
					{
						connection.RollbackTransaction();
					}
				}
			});

			Task.WaitAll(task1, task2);

			void RunGenerate(BusinessObjectFactory factory)
			{
				var primaryTarget = new BillOfLadingNumberGeneratorTarget();
				var additional = new BillOfLadingNumberGeneratorTarget();
				var generator = new NumberGenerator();
				generator.Factory = factory;
				generator.PrimaryTarget = primaryTarget;
				generator.Context = new NumberGeneratorContext(Guid.Empty, Guid.Empty, Guid.Empty);
				generator.BaseFountain = Env.NumberFountains.JobShipmentNumber;
				generator.FountainGetter = Env.NumberFountains.GetForwardingGeneratorFountain;
				generator.AdditionalTargets.Add(additional);
				generator.ValueProviders.AddRange(new StandardValueSource());

				generator.Generate();
			}
		}
	}
}
