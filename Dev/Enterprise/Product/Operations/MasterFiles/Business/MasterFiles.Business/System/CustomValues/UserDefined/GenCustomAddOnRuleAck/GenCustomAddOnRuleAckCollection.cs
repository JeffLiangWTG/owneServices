using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenCustomAddOnRuleAckCollection : ActiveBusinessObjectCollection<GenCustomAddOnRuleAck>
	{
		public GenCustomAddOnRuleAckCollection(BusinessObject parent)
			: base(parent.Factory, new ZQuery(GenCustomAddOnRuleAckSchema.XK_ParentID, parent.PK))
		{
			this.parent = parent;
		}
		readonly BusinessObject parent;

		public GenCustomAddOnRuleAckCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void SetDefaultsForNewElementCore(GenCustomAddOnRuleAck newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (parent != null)
			{
				newElement.XK_ParentTableCode = this.parent.TablePrefix;
				newElement.XK_ParentID = this.parent.PK;
			}
		}
	}
}
