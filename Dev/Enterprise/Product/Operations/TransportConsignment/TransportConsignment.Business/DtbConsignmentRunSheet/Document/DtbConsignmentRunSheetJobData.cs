using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DtbConsignmentRunSheetJobData),
	Constants.DocManagerCodes.DomesticTransportRunSheet)]

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetJobData : AssemblyData
	{
		#region BusinessObjectType

		public override Type BusinessObjectType
		{
			get { return typeof(DtbConsignmentRunSheet); }
		}

		#endregion

		#region CollectionType

		protected override Type CollectionType
		{
			get { return typeof(DtbConsignmentRunSheetCollection); }
		}

		#endregion

		#region GetBusinessObjectCollection

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new DtbConsignmentRunSheetCollection(factory);
		}

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbConsignmentRunSheet; }
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
			get { return ResString.GetMultilingualString("DtbConsignmentRunSheetJobData|HumanReadableName", "Land Transport Run Sheet"); }
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
