using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public abstract class SalesProductFieldLayoutDefinitionCollection : NonPersistentBusinessObjectCollection<SalesProductFieldLayoutDefinition>
	{
		protected SalesProductFieldLayoutDefinitionCollection(OrgSalesProduct salesProduct)
		{
			this.salesProduct = salesProduct;
		}

		protected OrgSalesProduct salesProduct;

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((SalesProductFieldLayoutDefinition)child).Order = this.Any() ? this.Cast<SalesProductFieldLayoutDefinition>().Max(x => x.Order) + 1 : 1;
		}

		#endregion

		#region Import / Export

		public void Import(SalesProductFormLayoutData data)
		{
			using (SuspendSettingHasChanges())
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				var definitionDataCollection = GetDefinitionDataCollection(data);
				foreach (SalesProductFieldLayoutDefinitionData definitionData in definitionDataCollection)
				{
					ZGuid fk;
					if (ZGuid.TryParse(definitionData.GenCustomColumnDefinitionFk, out fk))
					{
						var definition = AddNew();
						using (definition.SuspendSettingHasChanges())
						{
							definition.GenCustomColumnDefinitionFk = fk;
							definition.Order = definitionData.Order;
						}
					}
				}
			}
		}

		public void Export(SalesProductFormLayoutData data)
		{
			var definitionDataCollection = GetDefinitionDataCollection(data);
			foreach (SalesProductFieldLayoutDefinition definition in this)
			{
				var dataDefinition = definitionDataCollection.AddNew();
				dataDefinition.GenCustomColumnDefinitionFk = definition.GenCustomColumnDefinitionFk.ToString();
				dataDefinition.Order = definition.Order;
			}
		}

		#endregion

		#region Abstract

		protected abstract SalesProductFieldLayoutDefinitionDataCollection GetDefinitionDataCollection(SalesProductFormLayoutData data);

		#endregion
	}
}
