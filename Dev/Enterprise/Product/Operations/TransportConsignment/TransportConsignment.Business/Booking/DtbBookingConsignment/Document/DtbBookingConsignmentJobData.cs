using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DtbBookingConsignmentJobData),
	Constants.DocManagerCodes.DomesticTransportConsignment)]

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentJobData : AssemblyData
	{
		#region BusinessObjectType

		public override Type BusinessObjectType
		{
			get { return typeof(DtbBookingConsignment); }
		}

		#endregion

		#region CollectionType

		protected override Type CollectionType
		{
			get { return typeof(DtbBookingConsignmentCollection); }
		}

		#endregion

		#region GetBusinessObjectCollection

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new DtbBookingConsignmentCollection(factory);
		}

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBookingConsignment; }
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
			get { return ResString.GetMultilingualString("b18e0012-4de3-4fe9-8f36-59b71633d6e0", "Booking Consignment"); }
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
