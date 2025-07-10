using System;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(PkgPalletTransactionDocumentData), Enterprise.Core.Constants.DocManagerCodes.PalletTransaction)]

namespace Enterprise.Packing.Business
{
	internal sealed class PkgPalletTransactionDocumentData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(PkgPalletTransaction); }
		}

		protected override Type CollectionType
		{
			get { return typeof(PkgPalletTransactionCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new PkgPalletTransactionCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.PalletTransaction; }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("8DB56B0E-6F8B-4EC9-8CEE-FC1A3A347958", "Pallet Transactions"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
