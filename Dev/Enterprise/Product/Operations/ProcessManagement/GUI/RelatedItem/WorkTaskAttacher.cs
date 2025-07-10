using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.GUI
{
	/// <summary>
	/// A RecordAttacher that uses the lookups collection to determine the type of element being attached.
	/// The base class tries to determine types from the destination collection, 
	/// but for a WorkTaskRelatedItemCollection this is an interface and not a business object.
	/// </summary>
	public class WorkTaskAttacher : ZRecordAttacher
	{
		public WorkTaskAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection lookupsCollection, ModuleIdentifier moduleId)
			: base(destinationCollection, lookupsCollection, moduleId)
		{
			this.typeOfElements = lookupsCollection.TypeOfElements;
		}

		readonly Type typeOfElements;

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var pk = bizO.PK;
			bool result = DestinationCollection.FindByPK(pk) != null;
			if (!result)
			{
				var loaded = DestinationCollection.Factory.Load(typeOfElements, pk);
				if (loaded != null)
				{
					if (bizO is IWorkTaskRelatedItemSource source && loaded is IWorkTaskRelatedItemSource loadedSource)
					{
						loadedSource.ShouldAddRelatedItemAsParent = source.ShouldAddRelatedItemAsParent;
					}
					listToBulkAdd.Add(loaded);
					result = true;
				}
			}
			return result;
		}

		public event EventHandler<EventArgs> OnItemAttached;

		protected override void OnAttached()
		{
			OnItemAttached?.Invoke(this, null);
		}

		public IBusinessObjectCollection AttachedItemsCollection => DestinationCollection;
	}
}
