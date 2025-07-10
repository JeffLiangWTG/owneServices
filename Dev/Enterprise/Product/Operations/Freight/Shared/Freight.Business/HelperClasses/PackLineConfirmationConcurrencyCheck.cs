using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;
using ConfirmType = Enterprise.Freight.Business.ConfirmTimesSyncHelper.ConfirmType;

namespace Enterprise.Freight.Business
{
	public class PackLineConfirmationConcurrencyCheck : ConcurrencyChecker
	{
		PackLineConfirmationConcurrencyCheck(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static void Register(BusinessObjectFactory factory)
		{
			if (!IsRegistered(factory))
			{
				var participant = new PackLineConfirmationConcurrencyCheck(factory);
				factory.SaveInTransactionActions.Add(participant);
			}
		}

		public static bool IsRegistered(BusinessObjectFactory factory)
		{
			return factory != null && factory.SaveInTransactionActions.OfType<PackLineConfirmationConcurrencyCheck>().Any();
		}

		protected override void SaveInTransactionCore()
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			query.AddToFilter(JobPackLinesSchema.JL_FreightMode, FreightConstants.OuterPackType);

			var packLines = Factory.Load<PackLine>(query)
				.Where(c => !c.IsInDatabase && !c.IsDeleted)
				.ToArray();

			if (packLines.Any())
			{
				RunConcurrencyCheck(packLines, ConfirmType.Pickup);
				RunConcurrencyCheck(packLines, ConfirmType.Delivery);
			}
		}

		void RunConcurrencyCheck(IEnumerable<PackLine> packLines, ConfirmType confirmType)
		{
			var shipments = packLines
				.Where(c => ShowPackingLineConfirms(c, confirmType))
				.Select(c => c.Shipment)
				.Distinct()
				.ToArray();

			var shipmentPks = shipments.Select(c => c.PK).ToList();
			var pickupDeliveryType = confirmType == ConfirmType.Pickup
				? Constants.PickupDeliveryConfirmTypes.OriginPickup
				: Constants.PickupDeliveryConfirmTypes.DestinationDelivery;

			var sql = @"SELECT Distinct JS_PK, EU_PK FROM dbo.JobPickupDeliveryConfirm
INNER JOIN dbo.JobTransportLegPackLineDivot ON J8_EU_PickupDeliverConfirm = EU_PK
INNER JOIN dbo.JobPackLines ON JL_PK = J8_JL
INNER JOIN dbo.JobShipment ON JS_PK = JL_JS
WHERE
	JS_PK IN (SELECT Value FROM @ShipmentPks)
	AND EU_PickupDeliveryType = @PickupDeliveryType
	AND JL_FreightMode = @OuterPackType";

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@OuterPackType", FreightConstants.OuterPackType, JobPackLinesSchema.JL_FreightMode),
				ZSqlParameter.New("@PickupDeliveryType", pickupDeliveryType, JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType),
				ZSqlParameter.New("@ShipmentPks", shipmentPks, JobShipmentSchema.PK, true)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, sqlParams);

			var groupings = collection
				.Select(c => new { ShipmentPkInDb = (ZGuid)c[JobShipmentSchema.PK], ConfirmPkInDb = (ZGuid)c[JobPickupDeliveryConfirmSchema.PK] })
				.GroupBy(c => c.ShipmentPkInDb);

			foreach (var grouping in groupings)
			{
				var shipment = shipments.FirstOrDefault(c => c.PK == grouping.Key);
				if (shipment != null)
				{
					var confirmationPksInCurrentSession = confirmType == ConfirmType.Pickup
						? shipment.PickupConfirms.GetPKs()
						: shipment.DeliveryConfirms.GetPKs();

					if (grouping.Any(c => confirmationPksInCurrentSession.All(d => d != c.ConfirmPkInDb)))
					{
						var message = Res.GetString("9cb4bb1b-82ce-4f29-8687-f1c621bf3330", "Another user has made changes on the {0} tab - Confirmations that conflicts with your own changes. You must reopen this form again before saving.", confirmType);
						var heading = Res.GetString("89206c70-2232-4b60-951c-a5fcaa976006", "Packing Line And Confirmation concurrency error, you must reopen this form again before saving");

						throw new ZCannotSaveException(message, heading, shouldReprocess: true);
					}
				}
			}
		}

		bool ShowPackingLineConfirms(PackLine packLine, ConfirmType confirmType)
		{
			var shipment = packLine.Shipment;
			return shipment != null && shipment.IsInDatabase && !shipment.ShowContainerisedConfirms(confirmType);
		}
	}
}
