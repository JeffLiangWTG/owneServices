using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsNonExposedFeature : IWhsNonExposedFeature
	{
		bool IWhsNonExposedFeature.Enabled(WhsNonExposedFeatureName featureName)
		{
			switch (featureName)
			{
				case WhsNonExposedFeatureName.None:
					return GlbStaff.CurrentUser.IsSupportUser;
				case WhsNonExposedFeatureName.SchemaRedesignChanges:
					// We don't want to access the registry when designing the form
					return DesignModeFinder.IsDesigning || WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value;
				default:
					throw new NotImplementedException(string.Format(Culture.Invariant, "Check if feature {0} is enabled has not been implemented.", featureName));
			}
		}
	}
}
