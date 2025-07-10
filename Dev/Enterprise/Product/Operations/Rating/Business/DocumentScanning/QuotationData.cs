using System;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(QuotationData),
	Enterprise.Core.Constants.DocManagerCodes.Quotation)]

namespace Enterprise.Rating.Business
{
	public class QuotationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Quote); } }
		protected override Type CollectionType
		{
			get { return typeof(QuoteCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new QuoteCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Quotations; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("20cbcf0e-3322-4bcf-a076-059a5353b6ad", "Quotation"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
