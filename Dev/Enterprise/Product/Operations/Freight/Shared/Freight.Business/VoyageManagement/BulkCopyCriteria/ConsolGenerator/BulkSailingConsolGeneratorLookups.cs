using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class BulkSailingConsolGeneratorLookups : ZLookups
	{
		public BulkSailingConsolGeneratorLookups(BulkSailingConsolGenerator parent)
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

		protected new BulkSailingConsolGenerator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkSailingConsolGenerator)base.Parent; }
		}

		#endregion
	}
}
