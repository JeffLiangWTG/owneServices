using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class RoutingRequestConsolGeneratorLookups : ZLookups
	{
		public RoutingRequestConsolGeneratorLookups(RoutingRequestConsolGenerator parent)
			: base(parent) { }

		public CodeDescriptionPairList WeightUnitList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList VolumeUnitList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#region Implementation

		protected new RoutingRequestConsolGenerator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RoutingRequestConsolGenerator)base.Parent; }
		}

		#endregion
	}
}
