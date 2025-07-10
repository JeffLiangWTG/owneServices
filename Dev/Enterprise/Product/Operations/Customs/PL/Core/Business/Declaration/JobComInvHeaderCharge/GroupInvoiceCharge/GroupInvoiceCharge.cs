
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class GroupInvoiceCharge : EU.Business.Declaration.GroupInvoiceCharge, Integration.Customs.PL.IGroupInvoiceCharge
{
	public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void UpdateIsDutiableRelatedFieldsIfNecessary() { } // not needed to change any related field when IsDutiable changes
	protected override void UpdateIsStatisticalValueApplicableRelatedFieldsIfNecessary() { } // not needed to change any related field when IsStatisticalValueApplicable changes

	public new JobComInvoiceGroupHeader GroupInvoice => (JobComInvoiceGroupHeader)base.GroupInvoice;
}
