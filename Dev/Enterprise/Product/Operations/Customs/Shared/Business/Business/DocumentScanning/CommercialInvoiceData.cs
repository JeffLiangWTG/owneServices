using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CommercialInvoiceData),
	Enterprise.Core.Constants.DocManagerCodes.CommercialInvoice)]

namespace Enterprise.Customs.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public class CommercialInvoiceData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(BaseJobComInvoiceHeader); } }
		protected override Type CollectionType
		{
			get { return typeof(InvoiceHeaderWithNoDeclarationCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new InvoiceHeaderWithNoDeclarationCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.CommercialInvoice; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("abd72e4a-036b-4c56-80b9-6ab9e31f55a3", "Commercial Invoice"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
