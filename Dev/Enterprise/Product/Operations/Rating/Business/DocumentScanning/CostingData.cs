using System;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CostingData),
	Enterprise.Core.Constants.DocManagerCodes.Costing)]

namespace Enterprise.Rating.Business
{
	public class CostingData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Costing); } }
		protected override Type CollectionType
		{
			get { return typeof(CostingCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CostingCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Costing; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("46f7fddf-1a30-48fc-b7ce-77410dd3a389", "Costing"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new RatingHeaderEDocsViaUniversalXmlSupport(this, RatingConstants.RatingHeaderTypes.Costing);
	}
}
