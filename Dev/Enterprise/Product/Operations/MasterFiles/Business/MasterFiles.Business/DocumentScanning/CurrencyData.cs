using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(CurrencyData),
	Enterprise.Core.Constants.DocManagerCodes.Currency)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class CurrencyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefCurrency); } }
		protected override Type CollectionType
		{
			get { return typeof(RefCurrencyCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefCurrencyCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefCurrency; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("bcc0e1f1-552c-43cd-8431-c69512519655", "Currency"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
