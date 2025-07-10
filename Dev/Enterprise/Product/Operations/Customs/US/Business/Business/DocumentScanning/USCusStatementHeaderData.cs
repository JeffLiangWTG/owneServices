using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

[assembly: AssemblyDataProvider(
	typeof(USCusStatementHeaderData),
	Enterprise.Core.Constants.DocManagerCodes.CusStatementHeader,
	Country = "US")]

namespace Enterprise.Customs.US.Business
{
	class USCusStatementHeaderData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Customs.Business.BaseCusStatementHeader); } }

		protected override Type CollectionType
		{
			get { return typeof(CusStatementHeaderCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusStatementHeaderCollection(factory);
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.US.USCustomsStatement; } }

		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }

		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ad891c67-9750-41a6-8525-35a732569ed9", "Statement Header"); } }

		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
