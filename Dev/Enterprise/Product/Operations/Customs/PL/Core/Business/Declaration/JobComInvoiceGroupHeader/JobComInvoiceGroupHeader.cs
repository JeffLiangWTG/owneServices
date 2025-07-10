using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class JobComInvoiceGroupHeader : EU.Business.Declaration.JobComInvoiceGroupHeader, Integration.Customs.PL.IJobComInvoiceGroupHeader
{
	public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

	#region Charges

	public new GroupInvoiceChargeCollection<GroupInvoiceCharge> Charges => (GroupInvoiceChargeCollection<GroupInvoiceCharge>)base.Charges;

	protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection() => new GroupInvoiceChargeCollection<GroupInvoiceCharge>(this);

	#endregion

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);
}
