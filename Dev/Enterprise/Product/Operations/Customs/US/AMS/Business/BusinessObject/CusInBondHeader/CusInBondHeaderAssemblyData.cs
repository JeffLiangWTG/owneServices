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
	typeof(Enterprise.Customs.US.AMS.Business.CusInBondHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.USAMS)]

namespace Enterprise.Customs.US.AMS.Business
{
	class CusInBondHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CusInBondHeader); } }
		protected override Type CollectionType
		{
			get { return typeof(CusInBondHeaderCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusInBondHeaderCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.US.AMS; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("0ed67311-9b63-4509-a868-7aea38442b65", "US AMS"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
