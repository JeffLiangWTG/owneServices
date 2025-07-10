using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsDynamicPickFaceViewCollection : ActiveBusinessObjectCollection<WhsDynamicPickFaceView>
	{
		public WhsDynamicPickFaceViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsDynamicPickFaceViewCollection(BusinessObjectFactory factory, IModuleGridCollectionRefreshable dynamicPickFaceRefreshable)
			: base(factory)
		{
			new ModuleGridCollectionSynchronisationManager(this, dynamicPickFaceRefreshable);
		}

		protected override bool AllowNew => false;

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			switch (property.Name)
			{
				case WhsDynamicPickFaceViewSchema.Constants.WDP_LocationString:
					return new LocationComparer<WhsDynamicPickFaceView>(property, direction, dynamicPickFace => dynamicPickFace.Location);
				default:
					return base.GetSortComparerForProperty(property, direction);
			}
		}
	}
}
