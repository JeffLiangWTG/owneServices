using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DtbConsignmentJobData),
	Constants.DocManagerCodes.LandTransportConsignment)]

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentJobData : AssemblyData
	{
		#region BusinessObjectType

		public override Type BusinessObjectType
		{
			get { return typeof(DtbConsignment); }
		}

		#endregion

		#region CollectionType

		protected override Type CollectionType
		{
			get { return typeof(DtbConsignmentCollection); }
		}

		#endregion

		#region GetBusinessObjectCollection

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new DtbConsignmentCollection(factory);
		}

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbConsignment; }
		}

		#endregion

		#region ReferenceType

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		#endregion

		#region HumanReadableName

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("ea363834-9624-441d-b23b-e19b3dd6a9c7", "Land Transport Consignment"); }
		}

		#endregion

		#region IsAllowedForUnallocatedeDocs

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}

		#endregion
	}
}
