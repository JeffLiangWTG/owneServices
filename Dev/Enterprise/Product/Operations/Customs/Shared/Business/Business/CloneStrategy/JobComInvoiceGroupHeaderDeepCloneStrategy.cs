using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Copies group invoice, its chldren group headers and charges
	/// </summary>
	public class JobComInvoiceGroupHeaderDeepCloneStrategy : CustomsBusinessObjectCloneStrategy
	{
		public JobComInvoiceGroupHeaderDeepCloneStrategy(BaseJobComInvoiceGroupHeader groupHeader, CloneType cloneType, BaseJobDeclaration clonedDeclaration, BaseJobComInvoiceGroupHeader clonedParentGroup)
			: base(groupHeader, cloneType, clonedDeclaration.Factory)
		{
			this.clonedDeclaration = clonedDeclaration;
			this.clonedParentGroup = clonedParentGroup;
		}

		public JobComInvoiceGroupHeaderDeepCloneStrategy(Dictionary<ZGuid, ZGuid> groupInvoicePKs, BaseJobComInvoiceGroupHeader groupHeader, CloneType cloneType, BaseJobDeclaration clonedDeclaration, BaseJobComInvoiceGroupHeader clonedParentGroup)
			: this(groupHeader, cloneType, clonedDeclaration, clonedParentGroup)
		{
			this.groupInvoicePKs = groupInvoicePKs;
		}

		protected readonly BaseJobDeclaration clonedDeclaration;
		protected readonly BaseJobComInvoiceGroupHeader clonedParentGroup;

		readonly Dictionary<ZGuid, ZGuid> groupInvoicePKs;

		BaseJobComInvoiceGroupHeader GroupHeader
		{
			get { return (BaseJobComInvoiceGroupHeader)bizObjToClone; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BaseJobComInvoiceGroupHeader clonedResult = (BaseJobComInvoiceGroupHeader)base.CloneInternal(args);

			if (groupInvoicePKs != null)
			{
				groupInvoicePKs.Add(GroupHeader.PK, clonedResult.PK);
			}

			using (clonedResult.GetValidationSuspender())
			using (clonedResult.SuspendSettingHasChanges())
			{
				clonedResult.JZ_JE = clonedDeclaration.PK;

				if (clonedParentGroup != null)
				{
					clonedResult.JZ_JZ_GroupInvoiceFK = clonedParentGroup.PK;
					clonedParentGroup.JobComInvoiceGroupHeaders.Add(clonedResult);
				}
			}

			//copy charges
			new JobComInvChargeCloneHelper().CopyCharges(GroupHeader, clonedResult, cloneType);

			foreach (BaseJobComInvoiceGroupHeader childGroupHeader in GroupHeader.JobComInvoiceGroupHeaders)
			{
				new JobComInvoiceGroupHeaderDeepCloneStrategy(groupInvoicePKs, childGroupHeader, cloneType, clonedDeclaration, clonedResult).Clone();
			}

			return clonedResult;
		}
	}
}
