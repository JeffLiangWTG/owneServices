using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(NZAirCargoECIData),
	Enterprise.Core.Constants.DocManagerCodes.AirCargoMaster,
	Country = "NZ")]

namespace Enterprise.Customs.NZ.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.NZ.Business.Express;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public class NZAirCargoECIData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Customs.Business.CusMAWB); } }
		protected override Type CollectionType
		{
			get { return typeof(CusMAWBCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusMAWBCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.NZ.ExpressECI; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("253f77cf-c92a-49fe-8f52-bc07e8685bf7", "AirCargo ECI"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
