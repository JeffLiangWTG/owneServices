using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	class CusVehicleUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public CusVehicleUniqueIndexFailureHandler(CusVehicle vehicle)
		{
			this.vehicle = vehicle;
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return CusVehicleSchema.Constants.Indexes.NR_UX__CVH_ParentID; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var existingItem = GetExistingCusVehicle();
			if (existingItem != null)
			{
				var message = GenerateMessage(existingItem);
				var parentCollections = ((IBusinessObjectInternals)vehicle).ParentCollections;
				vehicle.Delete();
				parentCollections.ForEach(c =>
				{
					c.Reload(true);
					c.RefreshBindingIncludingChildren();
				});
				notifier.ReportInformation(message, Res.GetString("D6E1C011-2B3B-4403-965E-C6F2196A9285", "Save Error"));
			}
		}

		string GenerateMessage(CusVehicle existingItem)
		{
			var lastEditUserAndTime = existingItem.CVH_SystemLastEditUser + " @ " + existingItem.CVH_SystemLastEditTimeUtc;
			return Res.GetString("0943CAC4-6B8B-4459-B39B-BD44197A1236",
				"While you were working, Vehicle details have been entered for '{0}' by another user ({1}). Your changes have been merged, please review your changes and save again.",
				existingItem.Parent?.HumanReadableName ?? existingItem.HumanReadableName,
				lastEditUserAndTime);
		}

		CusVehicle GetExistingCusVehicle()
		{
			var query = new ZDBOnlyQuery(typeof(CusVehicle));
			query.AddToFilter(CusVehicleSchema.CVH_ParentID, vehicle.Parent.PK);
			return vehicle.Factory.LoadTop1<CusVehicle>(query);
		}

		readonly CusVehicle vehicle;
	}
}
