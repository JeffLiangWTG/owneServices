using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ProcessManagement.GUI
{
	public class NewItemTracker
	{
		public NewItemTracker(IBusiness bizo, WorkTaskRelatedItemCollection relatedItems, EnterpriseBusinessObject businessObject = null, bool shouldAddEvent = false)
		{
			ItemPK = bizo.Identifier;
			ItemType = bizo.GetType();
			relatedItemRef = new WeakReference(relatedItems);
			BusinessObject = businessObject;

			bizo.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			ShouldAddEvent = shouldAddEvent;
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			WorkTaskRelatedItemCollection relatedItems;
			if (relatedItemRef != null && (relatedItems = relatedItemRef.Target as WorkTaskRelatedItemCollection) != null)
			{
				var relatedItem = relatedItems.Factory.Load(ItemType, ItemPK);
				AddEventIfRequired(relatedItem);
				relatedItems.Add(relatedItem);
				relatedItemRef = null;
			}
			factory.Saved -= Factory_Saved;
		}

		void AddEventIfRequired(BusinessObject relatedItem)
		{
			if (ShouldAddEvent)
			{
				var workItem = (WorkItem)relatedItem;
				var parameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("DES", $"Work Item {workItem.WKI_WorkItemNumber} added to Incident Group")
				};
				BusinessObject.Logs.AddNew(AutoEvents.Attached, parameters.ToArray());
			}
		}

		readonly ZGuid ItemPK;
		readonly Type ItemType;
		WeakReference relatedItemRef;
		readonly EnterpriseBusinessObject BusinessObject;
		readonly bool ShouldAddEvent;
	}
}
