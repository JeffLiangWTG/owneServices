using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ConfirmTypes = Enterprise.Core.Constants.PickupDeliveryConfirmTypes;

namespace Enterprise.Freight.Business
{
	public class ConfirmationPackLineConcurrencyCheck : ConcurrencyChecker
	{
		ConfirmationPackLineConcurrencyCheck(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static void Register(BusinessObjectFactory factory)
		{
			if (!IsRegistered(factory))
			{
				var participant = new ConfirmationPackLineConcurrencyCheck(factory);
				factory.SaveInTransactionActions.Add(participant);
			}
		}

		public static bool IsRegistered(BusinessObjectFactory factory)
		{
			return factory != null && factory.SaveInTransactionActions.OfType<ConfirmationPackLineConcurrencyCheck>().Any();
		}

		protected override void SaveInTransactionCore()
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = true };

			var confirms = Factory.Load<CommonPickupDeliveryConfirm>(query)
				.Where(IsNewPackingLineConfirmation)
				.ToArray();

			if (confirms.Any())
			{
				RunConcurrencyCheck(confirms);
			}
		}

		void RunConcurrencyCheck(IEnumerable<CommonPickupDeliveryConfirm> confirms)
		{
			var shipments = confirms
				.Select(c => c.FirstShipment)
				.Where(c => c != null && c.IsInDatabase)
				.Distinct()
				.ToArray();

			var shipmentPks = shipments
				.Select(c => c.PK)
				.ToList();

			if (!shipmentPks.Any())
			{
				return;
			}

			var sql = @"SELECT Distinct JS_PK, JL_PK FROM dbo.JobShipment
INNER JOIN dbo.JobPackLines ON JL_JS = JS_PK
WHERE
	JS_PK IN (SELECT Value FROM @ShipmentPks)
	AND JL_FreightMode = @OuterPackType";

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@OuterPackType", FreightConstants.OuterPackType, JobPackLinesSchema.JL_FreightMode),
				ZSqlParameter.New("@ShipmentPks", shipmentPks, JobShipmentSchema.PK, true)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, sqlParams);

			var groupings = collection
				.Select(c => new { ShipmentPkInDb = (ZGuid)c[JobShipmentSchema.PK], PackLinePksInDb = (ZGuid)c[JobPackLinesSchema.PK] })
				.GroupBy(c => c.ShipmentPkInDb);

			foreach (var grouping in groupings)
			{
				var shipment = shipments.FirstOrDefault(c => c.PK == grouping.Key);
				if (shipment != null)
				{
					if (grouping.Any(c => shipment.OuterPackLines.GetPKs().All(d => d != c.PackLinePksInDb)))
					{
						var additionalInfo = GenerateReportPickupConfirmsConcurrencyDebugLog(confirms);
						var message = Res.GetString("6b451772-6e61-4849-8a76-533f8d726a3f", "Another user has made changes on the Packing tab that conflicts with your own changes. You must reopen this form again before saving.");
						var heading = Res.GetString("7ea41132-259e-4d0d-bc8d-beb8e568f48d", "Confirmation And Packing Line concurrency error, you must reopen this form again before saving");

						throw new ZCannotSaveException(message + additionalInfo, heading, shouldReprocess: true);
					}
				}
			}
		}

		string GenerateReportPickupConfirmsConcurrencyDebugLog(IEnumerable<CommonPickupDeliveryConfirm> confirms)
		{
			if (Globals.IsUserInteractive)
			{
				return ZString.Empty;
			}
			var log = new ZStringBuilder();
			var showStackTrace = true;
			foreach (var confirm in confirms)
			{
				log.Append(confirm.GetConfirmConcurrencyDebugLog(showStackTrace));
				showStackTrace = false;
			}

			return $" {log.ToStringWithNewLineBetweenAppends()}";
		}

		bool IsNewPackingLineConfirmation(CommonPickupDeliveryConfirm confirm)
		{
			return !confirm.IsInDatabase && !confirm.IsDeleted && confirm.IsSavedByFactory
					&& (confirm.EU_PickupDeliveryType == ConfirmTypes.DestinationDelivery || confirm.EU_PickupDeliveryType == ConfirmTypes.OriginPickup)
					&& confirm.Divots.All(d => !d.J8_JL.IsEmpty);
		}
	}
}
