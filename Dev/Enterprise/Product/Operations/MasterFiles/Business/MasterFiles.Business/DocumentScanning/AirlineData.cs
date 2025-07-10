using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(AirlineData),
	Enterprise.Core.Constants.DocManagerCodes.Airline)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class AirlineData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefAirline); } }
		protected override Type CollectionType
		{
			get { return typeof(RefAirlineCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefAirlineCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefAirline; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("82c9799a-9f6c-4a2d-834d-1745ca1c327e", "Airline"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
