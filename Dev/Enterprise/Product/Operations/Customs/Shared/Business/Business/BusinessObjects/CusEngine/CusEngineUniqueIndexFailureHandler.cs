using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business;

class CusEngineUniqueIndexFailureHandler : IUniqueIndexFailureHandler
{
	public CusEngineUniqueIndexFailureHandler(CusEngine engine)
	{
		this.engine = engine;
	}

	public IEnumerable<string> HandledUniqueIndexNames
	{
		get { yield return CusEngineSchema.Constants.Indexes.NR_UX__CEG_ParentID; }
	}

	public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
	{
		var existingItem = GetExistingCusEngine();
		if (existingItem != null)
		{
			var message = GenerateMessage(existingItem);
			var parentCollections = ((IBusinessObjectInternals)engine).ParentCollections;
			engine.Delete();
			parentCollections.ForEach(c =>
			{
				c.Reload(true);

				if (c is IDependentBusinessObjectCollection { Master: CusVehicle vehicle })
				{
					vehicle.RefreshBindingIncludingChildren();
				}
				else
				{
					c.RefreshBindingIncludingChildren();
				}
			});
			notifier.ReportInformation(message, Res.GetString("D6E1C011-2B3B-4403-965E-C6F2196A9285", "Save Error"));
		}
	}

	string GenerateMessage(CusEngine existingItem)
	{
		var lastEditUserAndTime = existingItem.CEG_SystemLastEditUser + " @ " + existingItem.CEG_SystemLastEditTimeUtc;
		return Res.GetString("12587124-70A7-4759-B8EA-2A230D6CAF99",
			"While you were working, Engine details have been entered for '{0}' by another user ({1}). Your changes have been merged, please review your changes and save again.",
			existingItem.Parent?.HumanReadableName ?? existingItem.HumanReadableName,
			lastEditUserAndTime);
	}

	CusEngine GetExistingCusEngine()
	{
		var query = new ZDBOnlyQuery(typeof(CusEngine));
		query.AddToFilter(CusEngineSchema.CEG_ParentID, engine.Parent.PK);
		return engine.Factory.LoadTop1<CusEngine>(query);
	}

	readonly CusEngine engine;
}
