using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// GroupHeaders for a declaration
	/// </summary>
	public class GroupHeaderCollection : BusinessObjectCollection<BaseJobComInvoiceGroupHeader>
	{
		public GroupHeaderCollection(BaseJobDeclaration jobDeclaration) : base(jobDeclaration.Factory)
		{
			this.JobDeclaration = jobDeclaration;
		}

		public GroupHeaderCollection(BaseJobComInvoiceHeader invoice) : base(invoice.Factory)
		{
			this.JobDeclaration = invoice.JobDeclaration;
		}

		#region Implementation

		protected readonly BaseJobDeclaration JobDeclaration;

		protected override bool FetchOnlyFromLocalCache
		{
			get { return JobDeclaration == null || !JobDeclaration.IsInDatabase; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (JobDeclaration == null)
			{
				return new ZQuery() { IsNoResultQuery = true };
			}

			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.True);
			if (!JobDeclaration.SupportAdditionalInvoices)
			{
				result.AddToFilter(JobComInvoiceHeaderSchema.JZ_ClusterKey, JobDeclaration.JE_ClusterKey);
				result.AddToFilter(JobComInvoiceHeaderSchema.JZ_JE, JobDeclaration.PK);
			}
			else
			{
				result.AddToFilter(GetSupportAdditionalInvoicesQuery());
			}

			return result;
		}

		ZQuery GetSupportAdditionalInvoicesQuery()
		{
			var result = new ZQuery();
			List<ZGuid> invoicePKs = new List<ZGuid>();
			invoicePKs.AddRange(Factory.Load<BaseJobComInvoiceGroupHeader>(new BaseJobComInvoiceGroupHeader.Loader(Factory).GetQuery(JobDeclaration))
				.Select(p => p.PK));
			additionalGroupHeaderPKs = new List<ZGuid>(Factory.Load<GroupRelatedDeclarationGenPivot>(new GroupRelatedDeclarationGenPivot.Loader(Factory).GetQuery(JobDeclaration))
				.Select(p => p.XX_Relation1ID));
			invoicePKs.AddRange(additionalGroupHeaderPKs);
			result.AddToFilter(JobComInvoiceHeaderSchema.PK, invoicePKs);

			return result;
		}

		protected List<ZGuid> additionalGroupHeaderPKs;

		public bool IsAdditionalGroupHeader(BusinessObject groupHeader)
		{
			return additionalGroupHeaderPKs != null && additionalGroupHeaderPKs.Contains(groupHeader.PK);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (!IsAdditionalGroupHeader(child))
			{
				BaseJobComInvoiceGroupHeader groupInvoice = (BaseJobComInvoiceGroupHeader)child;
				groupInvoice.JZ_JE = JobDeclaration == null ? ZGuid.Empty : JobDeclaration.PK;
			}
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			if (!IsAdditionalGroupHeader(bizO))
			{
				((BaseJobComInvoiceGroupHeader)bizO).HiddenOriginalParentGuid = JobDeclaration == null ? ZGuid.Empty : JobDeclaration.PK;
			}
		}

		#endregion
	}
}
