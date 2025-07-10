using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AgencySundryChargesData),
	Enterprise.Core.Constants.DocManagerCodes.AgencySundryCharges)]

namespace Enterprise.Freight.Agency.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	internal sealed class AgencySundryChargesData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(SundryCharges); } }
		protected override Type CollectionType
		{
			get { return typeof(SundryChargesCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new SundryChargesCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AgencySundryCharges; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("03d57f0e-57be-45b5-b53e-21061c598818", "Sundry Charges"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
