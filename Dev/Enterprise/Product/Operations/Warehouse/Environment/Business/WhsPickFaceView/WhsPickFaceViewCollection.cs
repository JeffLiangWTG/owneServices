using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceViewCollection : ActiveBusinessObjectCollection<WhsPickFaceView>
	{
		public WhsPickFaceViewCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public WhsPickFaceViewCollection(BusinessObjectFactory factory, IModuleGridCollectionRefreshable pickFaceRefreshable)
			: base(factory)
		{
			new ModuleGridCollectionSynchronisationManager(this, pickFaceRefreshable);
		}

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			IComparer comparer;
			if (property.Name == WhsPickFaceView.Schema.WPV_WL)
			{
				comparer = new LocationComparer<WhsPickFaceView>(property, direction, pickFaceView => pickFaceView.Location);
			}
			else
			{
				comparer = base.GetSortComparerForProperty(property, direction);
			}

			return comparer;
		}

		protected override bool AllowNew => false;
	}
}
