using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.GUI
{
	public static class NewItemHelper
	{
		public static void AddNewItem(ZController lastController, IWorkTaskRelatedItemSource relatedItemSource, WorkTaskRelatedItemCollection relatedItems, string relatedItemType, bool markHasChanges, EnterpriseBusinessObject businessObject = null, bool shouldAddEvent = false)
		{
			ZForm form = (ZForm)lastController.ShowNewForm();
			if (form != null)
			{
				using (((BusinessObject)form.BusinessEntity).SuspendSettingHasChanges())
				{
					relatedItemSource.PopulateNewRelatedItem(relatedItemType, (IWorkTaskRelatedItem)form.BusinessEntity);
				}

				if (markHasChanges)
				{
					form.BusinessEntity.HasChanges = true;
				}

				_ = new NewItemTracker(form.BusinessEntity, relatedItems, businessObject, shouldAddEvent);
			}
		}

		public static void SetAdditionalFilter(WorkTaskRelatedItemModuleInfo relatedItem)
		{
			IBusinessObjectCollection lookupsCollection = relatedItem.FindBoxList;
			ZQuery additionalFilter = relatedItem.AdditionalFilterForFindBox;

			if (additionalFilter != null && lookupsCollection is ILegacyBusinessObjectCollectionInternals legacyLookupsCollection)
			{
				legacyLookupsCollection.SetOverriddenAdditionalFilter(additionalFilter);
			}
		}
	}
}
