using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefDocType)]
	public class RefDocTypeCollection : ActiveBusinessObjectCollection<RefDocType>, IRefDocTypeCollection
	{
		public RefDocTypeCollection(BusinessObjectFactory factory) : base(factory)
		{
			ExcludeFreightReferenceTypesWhenApplicable();
		}

		public RefDocTypeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
			ExcludeFreightReferenceTypesWhenApplicable();
		}

		void ExcludeFreightReferenceTypesWhenApplicable()
		{
			if (DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				this.AdditionalFilter.AddToFilter(RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship);
				this.AdditionalFilter.AddToFilter(RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics);
			}
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
