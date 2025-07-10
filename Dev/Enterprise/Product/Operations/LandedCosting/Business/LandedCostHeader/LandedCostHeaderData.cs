using System;
using Enterprise.LandedCosting.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(LandedCostHeaderData), Enterprise.Core.Constants.DocManagerCodes.LandedCostHeader)]

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostHeaderData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(LandedCostHeader); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("5E2D582D-A801-4943-B9F1-61CD5347662D", "LC Header"); } }
	}
}
