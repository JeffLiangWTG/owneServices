using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	class ContainerPackLineConcurrencyCheck : ConcurrencyChecker
	{
		ContainerPackLineConcurrencyCheck(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static void Register(BusinessObjectFactory factory)
		{
			if (!IsRegistered(factory))
			{
				var participant = new ContainerPackLineConcurrencyCheck(factory);
				factory.SaveInTransactionActions.Add(participant);
			}
		}

		public static bool IsRegistered(BusinessObjectFactory factory)
		{
			return factory != null && factory.SaveInTransactionActions.OfType<ContainerPackLineConcurrencyCheck>().Any();
		}

		protected override void SaveInTransactionCore()
		{
			RunPacklineConcurrencyCheck();
			RunJobConShipLinkConcurrencyCheck();
		}

		void RunPacklineConcurrencyCheck()
		{
			const string sqlTemplate =
@"SELECT TOP 1
			J6_JL
		FROM dbo.JobContainerPackPivot
		JOIN dbo.JobContainer on J6_JC = JC_PK
		JOIN
		(
			SELECT DISTINCT
				PK = JC_JK
			FROM
				dbo.JobContainer
			WHERE
				JC_PK IN ({0})
		) ConsolsToCheck ON PK = JC_JK 
		GROUP BY
			J6_JL, JC_JK
		HAVING COUNT(J6_PK) > 1";

			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;

			var containersToCheck = Factory.Load<JobContainerPackPivot>(query)
				.Where(p => !p.IsInDatabase)
				.Select(p => p.J6_JC)
				.Distinct();

			foreach (var containerBatch in containersToCheck.Batch(batchSize))
			{
				var collection = GetRows(sqlTemplate, containerBatch);
				if (collection.Any())
				{
					var message = Res.GetString("93373770-55c7-4ad4-907d-c536475841ca", "Another user has made changes to the packing that conflict with your own changes.");
					var heading = Res.GetString("770a82fd-766c-4c1b-a87c-bd33eecc3373", "Packing concurrency error");

					throw new ZCannotSaveException(message, heading, shouldReprocess: true);
				}
			}
		}

		void RunJobConShipLinkConcurrencyCheck()
		{
			const string sqlTemplate =
@"SELECT TOP 1 JL_PK, JC_ContainerNum, JS_UniqueConsignRef, JK_UniqueConsignRef
FROM dbo.JobContainerPackPivot
	JOIN dbo.JobPackLines ON J6_JL = JL_PK
	JOIN dbo.JobContainer ON JC_PK = J6_JC
	JOIN dbo.JobShipment ON JL_JS = JS_PK
	JOIN dbo.JobConsol ON JC_JK = JK_PK
	LEFT JOIN dbo.JobConShipLink ON JC_JK = JN_JK AND JL_JS = JN_JS
WHERE JC_PK IN ({0})
AND JC_JK IS NOT NULL
AND JK_AgentType <> 'CLM'
AND JN_PK IS NULL
";

			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;

			var containersToCheck = Factory.Load<JobContainerPackPivot>(query)
				.Where(p => !p.IsInDatabase)
				.Select(p => p.J6_JC)
				.Distinct();

			foreach (var containerBatch in containersToCheck.Batch(batchSize))
			{
				var collection = GetRows(sqlTemplate, containerBatch);
				if (collection.Any())
				{
					var message = Res.GetString("3ec20c25-ef1b-446b-9b17-c3539591ebbd", @"Another user has made changes that conflict with your own changes.
Shipment '{0}' has Packs allocated to Container '{1}' but the Shipment has been removed from the Container's parent Consol '{2}'.",
						collection[0][JobShipmentSchema.JS_UniqueConsignRef.Name],
						collection[0][JobContainerSchema.JC_ContainerNum.Name],
						collection[0][JobConsolSchema.JK_UniqueConsignRef.Name]);
					var heading = Res.GetString("7d45ac43-6ee5-4bc7-9a2d-438e1ab23480", "Packing concurrency error");

					throw new ZConcurrencyCheckFailureException(message, heading, shouldReprocess: true);
				}
			}
		}

		DynamicBusinessObjectCollection GetRows(ZString sqlTemplate, IEnumerable<ZGuid> containerPKs)
		{
			var parameters = containerPKs
				.Select((pk, index) => new { ParameterName = "@P" + index, Value = pk.ToGuid() })
				.ToArray();

			var sql = string.Format(CultureInfo.InvariantCulture, sqlTemplate, string.Join(",", parameters.Select(t => t.ParameterName)));

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, parameters.Select(p => ZSqlParameter.New(p.ParameterName, p.Value, JobContainerSchema.PK)).ToArray());
			return collection;
		}

		const int batchSize = 200;
	}
}
