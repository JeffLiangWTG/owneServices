using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItemCollection : ActiveBusinessObjectCollection<OrgCommissionAgreementItem>
	{
		public OrgCommissionAgreementItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCommissionAgreementItemCollection(OrgCommissionAgreement parentAgreement)
			: base(parentAgreement.Factory, parentAgreement, new ZQuery(OrgCommissionAgreementItemSchema.CAI_Type, OrgCommissionAgreementItemTypes.Codes.Product), OrgCommissionAgreementItemSchema.CAI_ParentID)
		{
			Type = OrgCommissionAgreementItemTypes.Codes.Product;
		}

		public OrgCommissionAgreementItemCollection(OrgCommissionAgreementItem parentAgreementItem, ZString type)
			: base(parentAgreementItem.Factory, parentAgreementItem, new ZQuery(OrgCommissionAgreementItemSchema.CAI_Type, type), OrgCommissionAgreementItemSchema.CAI_ParentID)
		{
			Type = type;
		}

		public readonly ZString Type;

		#region Defaults

		protected override void SetDefaultsForNewElementCore(OrgCommissionAgreementItem newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.CAI_Type = Type;
			var allItem = this.FirstOrDefault(x => x.CAI_Code == OrgCommissionAgreementItem.AllItemCode);
			newElement.CAI_IsInclude = allItem == null || !allItem.CAI_IsInclude;
		}

		#endregion

		#region Add New

		public OrgCommissionAgreementItem AddNew(ZBool isInclude, ZString code)
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.CAI_Code = code;
				result.CAI_IsInclude = isInclude;
			}

			return result;
		}

		#endregion
	}
}
