using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItemConditionCollection : ActiveBusinessObjectCollection<OrgCommissionAgreementItemCondition>
	{
		readonly OrgCommissionAgreementItem agreementItem;
		public OrgCommissionAgreementItemConditionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCommissionAgreementItemConditionCollection(OrgCommissionAgreementItem parentItem)
			: base(parentItem.Factory, parentItem, new ZQuery(OrgCommissionAgreementItemConditionSchema.CIC_CAI, parentItem.PK),
				  OrgCommissionAgreementItemConditionSchema.CIC_CAI)
		{
			agreementItem = parentItem;
		}

		protected override bool AllowNew
		{
			get
			{
				return agreementItem == null || agreementItem.CanHaveConditions;
			}
		}

		#region Defaults

		protected override void SetDefaultsForNewElementCore(OrgCommissionAgreementItemCondition newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (Relationship.Master != null)
			{
				newElement.CIC_CAI = Relationship.Master.PK;
			}
		}

		#endregion
	}
}
