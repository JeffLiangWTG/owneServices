using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[DependentBusinessObject(typeof(JobDeclaration), "JobComInvoiceGroupHeaders")]
	public class JobComInvoiceGroupHeader : BaseJobComInvoiceGroupHeader, Integration.Customs.NZ.IJobComInvoiceGroupHeader, IAddInfoManager
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static JobComInvoiceGroupHeader New(BusinessObjectFactory factory)
		{
			return factory.New<JobComInvoiceGroupHeader>();
		}

		#region Strongly Typed Collections and BO's
		#region JobComInvoiceHeaders
		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders
		{
			get { return (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders; }
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this, true);
		}

		#endregion

		#region JobComInvoiceGroupHeaders

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		#endregion

		#region GroupHeader
		public new JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.GroupHeader; }
		}
		#endregion

		#region JobDeclaration
		public new JobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration as JobDeclaration; }
		}

		protected override BaseJobDeclaration GetJobDeclaration(ZGuid guidToLoad)
		{
			return (JobDeclaration)Factory.Load(typeof(JobDeclaration), guidToLoad);
		}
		#endregion

		#region Charges

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection()
		{
			return new JobComInvChargeCollection<GroupInvoiceCharge>(this);
		}

		public new JobComInvChargeCollection<GroupInvoiceCharge> Charges
		{
			get { return (JobComInvChargeCollection<GroupInvoiceCharge>)base.Charges; }
		}

		#endregion

		#endregion

		#region AddInfo
		public NZAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new NZAddInfo(this, JZ_AddInfoInfo);
					fAddInfo.LoadPropertiesFromString(JZ_AddInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		NZAddInfo fAddInfo;
		#endregion

		NZCurrencyConverterWithTestExchangeRates fCurrencyConverter;
		public override Enterprise.MasterFiles.Business.CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null && JobDeclaration != null)
				{
					fCurrencyConverter = new NZCurrencyConverterWithTestExchangeRates(JobDeclaration);
				}
				return fCurrencyConverter;
			}
		}

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
