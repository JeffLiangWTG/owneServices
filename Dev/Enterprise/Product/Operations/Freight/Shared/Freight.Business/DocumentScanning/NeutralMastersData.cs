using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(NeutralMastersData),
	Enterprise.Core.Constants.DocManagerCodes.NeutralMasters)]

namespace Enterprise.Freight.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using ResString = Enterprise.Freight.ResString;

	public class NeutralMastersData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(JobMawb); } }
		protected override Type CollectionType
		{
			get { return typeof(JobMawbCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new JobMawbCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.JobMawb; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("0d958b27-4784-4a77-afd7-a54ff16623b9", "Neutral Masters"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new NeutralMastersEDocsViaUniversalXmlSupport();
	}
}
