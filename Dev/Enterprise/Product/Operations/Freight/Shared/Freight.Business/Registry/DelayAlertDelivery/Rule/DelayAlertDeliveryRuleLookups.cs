using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class DelayAlertDeliveryRuleLookups : ZLookups
	{
		public DelayAlertDeliveryRuleLookups(DelayAlertDeliveryRule parent)
			: base(parent) { }

		public CodeDescriptionPairList TransportModes
		{
			get
			{
				if (transportModes == null)
				{
					transportModes = new CodeDescriptionPairList();
					transportModes.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
					transportModes.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
					transportModes.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
					transportModes.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
					transportModes.AddPair(Constants.TransportModes.All, Constants.TransportModeDescriptions.All);
				}
				return transportModes;
			}
		}
		CodeDescriptionPairList transportModes;

		public CodeDescriptionPairList Modules
		{
			get { return modules ?? (modules = new DelayAlertDeliveryModules()); }
		}
		CodeDescriptionPairList modules;

		public CodeDescriptionPairList Directions
		{
			get { return directions ?? (directions = new DelayAlertDeliveryDirections()); }
		}
		CodeDescriptionPairList directions;

		#region Implementation

		protected new DelayAlertDeliveryRule Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DelayAlertDeliveryRule)base.Parent; }
		}

		#endregion
	}
}
