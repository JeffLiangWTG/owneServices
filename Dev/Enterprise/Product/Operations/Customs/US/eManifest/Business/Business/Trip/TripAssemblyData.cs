using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.US.eManifest.Business.TripAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.USeManifest,
	Country = Enterprise.Core.Constants.CountryCodes.UnitedStates)]

namespace Enterprise.Customs.US.eManifest.Business
{
	class TripAssemblyData : AssemblyData
	{
		#region Overrides of AssemblyData

		public override Type BusinessObjectType
		{
			get { return typeof(Trip); }
		}

		protected override Type CollectionType
		{
			get { return typeof(TripCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new TripCollection(factory);
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.US.eManifest; } }

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("215552fd-61b4-4738-aab5-1ec706fcb0df", "e-Manifest"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}

		#endregion
	}
}
