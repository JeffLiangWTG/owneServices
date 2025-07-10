using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AgencyVoyageAccountingData),
	Enterprise.Core.Constants.DocManagerCodes.AgencyVoyageAccounting)]

namespace Enterprise.Freight.Agency.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	class AgencyVoyageAccountingData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(VoyageAccount); } }
		protected override Type CollectionType
		{
			get { return typeof(VoyageAccountCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new VoyageAccountCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AgencyVoyageAccounting; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("b45296ab-a177-43af-a2ad-2e53556ad039", "Voyage Account"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new AgencyVoyageAccountingEDocsViaUniversalXmlSupport();
	}
}
