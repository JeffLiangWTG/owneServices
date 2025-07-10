using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.StmMenuItem)]
	public class DocAutoDeliverStmMenuItemCollection : StmMenuItemCollection
	{
		public DocAutoDeliverStmMenuItemCollection(BusinessObjectFactory factory) : this(factory, null)
		{
		}

		public DocAutoDeliverStmMenuItemCollection(BusinessObjectFactory factory, ZQuery filter, bool shouldAddContactFilter = true) : base(factory, filter)
		{
			this.shouldAddContactFilter = shouldAddContactFilter;
		}

		readonly bool shouldAddContactFilter;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			if (DocumentsDataRegistry.Instance.EnableFormSupportforDocumentDeliveryCompletionActions.Value)
			{
				result.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Forms);
			}

			if (shouldAddContactFilter)
			{
				result.AddToFilter(StmMenuItemSchema.SU_PreventAutoDelivery, "N");
				result.AddToFilter(StmMenuItemSchema.SU_ContactType, SQLComparisonOperator.NotEqual, ContactType.NoContactType.Code);
				result.AddToFilter(StmMenuItemSchema.SU_ContactType, SQLComparisonOperator.NotEqual, string.Empty);
			}

			return result;
		}
	}
}
