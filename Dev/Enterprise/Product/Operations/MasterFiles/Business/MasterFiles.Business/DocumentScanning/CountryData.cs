using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CountryData),
	Enterprise.Core.Constants.DocManagerCodes.Country)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class CountryData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefCountry); } }
		protected override Type CollectionType
		{
			get { return typeof(RefCountryCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefCountryCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefCountry; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("fab53377-0365-4354-864a-3a6f3decf40a", "Country/Region"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
