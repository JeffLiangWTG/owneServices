using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.US.ISF.Business.ImporterSecurityFilingAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.ImporterSecurityFiling)]

namespace Enterprise.Customs.US.ISF.Business
{
	class ImporterSecurityFilingAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CusISFHeader); } }
		protected override Type CollectionType
		{
			get { return typeof(CusISFHeaderCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusISFHeaderCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ImporterSecurityFiling; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("09f22c87-354c-4d3f-8a66-5b47801e77b6", "Importer Security Filing"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
