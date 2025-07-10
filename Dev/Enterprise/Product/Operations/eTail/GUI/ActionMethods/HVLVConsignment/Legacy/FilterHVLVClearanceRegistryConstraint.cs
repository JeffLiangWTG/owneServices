using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Business;

namespace Enterprise.eTail.GUI
{
	public class FilterHVLVClearanceRegistryConstraint : IFilterConstraint
	{
		public string Name => "HasHVLVClearance";

		public string PluralValueName => Res.GetString("464deaf6-4d15-4177-b8df-902a2275b7ec", "values");

		public string SingularValueName => Res.GetString("1d0f4da9-b773-4cca-beed-0d929c1a3d5b", "value");

		public string Description => Res.GetString("OperationalActionsFilter|HasHVLVClearanceConstraint|Description", "Matches if the 'eTail' legacy functionality is enabled.");

		public object GetValue()
		{
			return GetDefaultStringValue();
		}

		public string GetDefaultStringValue()
		{
			return HVLVDataRegistry.HasHVLVClearance ? "Y" : "N";
		}
	}
}
