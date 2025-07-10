using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI
{
	internal static class PackedIntoMultipleContainersHandler
	{
		public static void Handle(AgencyShipment shipment)
		{
			ResolvePivots(shipment);
			Globals.Message.Show(Res.GetString("7e0b3f03-0b46-45fe-a904-b8ea0209827e",
				"Another user has made changes to the packing that conflicts with your own changes. Please verify that the packing is correct and try again."));
		}

		static void ResolvePivots(AgencyShipment shipment)
		{
			Dictionary<AgencyShipmentPackLine, ZGuid> linePKs = CollectCurrentPackingInfo(shipment);

			shipment.Factory.ClearQueryCache(JobContainerPackPivotSchema.Constants.TableName);

			foreach (AgencyShipmentPackLine line in linePKs.Keys)
			{
				shipment.Factory.AddFetchHint(JobContainerPackPivotSchema.J6_JL, line.PK);
			}

			foreach (KeyValuePair<AgencyShipmentPackLine, ZGuid> pair in linePKs)
			{
				AgencyShipmentPackLine line = pair.Key;
				line.Containers.Load();

				bool needsValidation = false;

				for (int i = line.Containers.Count - 1; i >= 0; i--)
				{
					CommonContainer container = line.Containers[i];

					if (pair.Value != container.PK)
					{
						line.Containers.Remove(container);
						needsValidation = true;

						ConflictNotificationTracker.AddWarning(line.JL_JCInfo, Res.GetString("cec3c50e-512d-4da5-b57e-8fb1c937349e", "Another user has set this line as packed into '{0}'.", container.JC_ContainerCode));
					}
				}

				if (needsValidation)
				{
					line.Validation.ValidateJL_JC();
				}
			}
		}

		static Dictionary<AgencyShipmentPackLine, ZGuid> CollectCurrentPackingInfo(AgencyShipment shipment)
		{
			Dictionary<AgencyShipmentPackLine, ZGuid> linePKs = new Dictionary<AgencyShipmentPackLine, ZGuid>();

			foreach (AgencyShipmentPackLine line in shipment.OuterPackLines)
			{
				linePKs.Add(line, line.JL_JC);
			}

			return linePKs;
		}
	}
}
