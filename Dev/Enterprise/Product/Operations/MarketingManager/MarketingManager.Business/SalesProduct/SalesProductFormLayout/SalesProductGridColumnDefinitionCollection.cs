using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public abstract class SalesProductGridColumnDefinitionCollection<T> : NonPersistentBusinessObjectCollection<T>
		where T : SalesProductGridColumnDefinition
	{
		protected SalesProductGridColumnDefinitionCollection(OrgSalesProduct salesProduct)
		{
			this.SalesProduct = salesProduct;
		}

		protected readonly OrgSalesProduct SalesProduct;

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((T)child).Order = this.Any() ? this.Cast<T>().Max(x => x.Order) + 1 : 1;
		}

		#endregion

		#region Import / Export

		public void Import(SalesProductFormLayoutData data)
		{
			using (SuspendSettingHasChanges())
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				var gridColumnDefinitionData = GetGridColumnDefinitionDataCollection(data);
				foreach (SalesProductGridColumnDefinitionData definitionData in gridColumnDefinitionData)
				{
					ZGuid fk;
					if (ZGuid.TryParse(definitionData.GenCustomColumnDefinitionFk, out fk))
					{
						var definition = AddNew();
						using (definition.SuspendSettingHasChanges())
						{
							definition.GenCustomColumnDefinitionFk = fk;
							definition.Order = definitionData.Order;
							definition.IsVisible = definitionData.IsVisible;
						}
					}
				}
			}
		}

		public void Export(SalesProductFormLayoutData data)
		{
			var gridColumnDefinitionData = GetGridColumnDefinitionDataCollection(data);
			foreach (T definition in this)
			{
				var dataDefinition = gridColumnDefinitionData.AddNew();
				dataDefinition.GenCustomColumnDefinitionFk = definition.GenCustomColumnDefinitionFk.ToString();
				dataDefinition.Order = definition.Order;
				dataDefinition.IsVisible = definition.IsVisible;
			}
		}

		protected abstract SalesProductGridColumnDefinitionDataCollection GetGridColumnDefinitionDataCollection(SalesProductFormLayoutData data);

		#endregion
	}
}
