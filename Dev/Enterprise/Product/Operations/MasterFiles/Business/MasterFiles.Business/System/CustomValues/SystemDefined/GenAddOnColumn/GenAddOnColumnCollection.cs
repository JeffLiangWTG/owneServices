using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public sealed class GenAddOnColumnCollection : ActiveBusinessObjectCollection<GenAddOnColumn>
	{
		public GenAddOnColumnCollection(BusinessObject parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		readonly BusinessObject parent;

		public GenAddOnColumn Find(ZString fieldName)
		{
			foreach (GenAddOnColumn addOnColumn in this)
			{
				if (addOnColumn.XA_Name == fieldName)
				{
					return addOnColumn;
				}
			}
			return null;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();

			result.AddToFilter(GenAddOnColumnSchema.XA_ParentID, parent.PK);

			return result;
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { parent };
		}

		protected override void SetDefaultsForNewElementCore(GenAddOnColumn newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.XA_ParentID = parent.PK;
			newElement.XA_ParentTableCode = parent.TablePrefix;
		}
	}
}
