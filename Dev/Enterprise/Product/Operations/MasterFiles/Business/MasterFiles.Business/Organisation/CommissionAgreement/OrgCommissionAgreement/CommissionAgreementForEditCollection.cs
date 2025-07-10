using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionAgreementForEditCollection : OrgCommissionAgreementCollection
	{
		public CommissionAgreementForEditCollection(OrgOpportunity opportunity)
			: base(opportunity, GetAgreementsForEditFilter())
		{
			Opportunity = opportunity;

			AddUncommittedDraftAgreements();
			((IBindingList)opportunity.ApprovedCommissionAgreements).ListChanged += ApprovedCommissionAgreements_ListChanged;
		}

		protected readonly OrgOpportunity Opportunity;

		#region Filter

		public static ZQuery GetAgreementsForEditFilter()
		{
			return new ZQuery(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, null);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewElementCore(OrgCommissionAgreement newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var allProductsItem = newElement.ProductItems.AddNew();
			allProductsItem.CAI_IsInclude = true;
			allProductsItem.CAI_Code = OrgCommissionAgreementItemLookups.AllProductsCode;
		}

		#endregion

		#region AddUncommittedDraftAgreements

		void AddUncommittedDraftAgreements()
		{
			if (!Opportunity.IsDeleted)
			{
				var approvedAgreementsAlreadyWithDraft = new HashSet<ZGuid>(this.Select(x => x.ParentVersion).Where(x => x != null).Select(x => x.PK));
				foreach (var approvedAgreement in Opportunity.ApprovedCommissionAgreements.Where(x => !approvedAgreementsAlreadyWithDraft.Contains(x.PK)).ToArray())
				{
					approvedAgreement.CreateDraft(true);
				}
			}
		}

		void ApprovedCommissionAgreements_ListChanged(object sender, ListChangedEventArgs e)
		{
			AddUncommittedDraftAgreements();
		}

		#endregion

		#region UpdateAgreementsReadOnlyProperty

		public void UpdateAgreementsReadOnlyProperty()
		{
			foreach (var agreement in this)
			{
				agreement.SetReadOnlyIncludingChildren(!agreement.IsEditable);
			}
		}

		#endregion
	}
}
