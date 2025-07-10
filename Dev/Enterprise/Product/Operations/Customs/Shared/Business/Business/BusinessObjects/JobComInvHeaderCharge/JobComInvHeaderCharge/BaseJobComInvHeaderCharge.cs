using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class BaseJobComInvHeaderCharge : Common.JobComInvCharge
		, Integration.Customs.IBaseJobComInvHeaderCharge
	{
		protected BaseJobComInvHeaderCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new readonly TypeDecider TypeDecider = new BaseJobComInvHeaderChargeTypeDecider();

		[List(nameof(Lookups) + "." + nameof(JobComInvHeaderChargeLookups.ExchangeRateTypeList))]
		public override ZString J7_ExchangeRateType
		{
			get { return base.J7_ExchangeRateType; }
			set { base.J7_ExchangeRateType = value; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new JobComInvHeaderChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new JobComInvHeaderChargeLookups(this);
		}

		public new JobComInvHeaderChargeLookups Lookups
		{
			get { return (JobComInvHeaderChargeLookups)base.Lookups; }
		}

		public new ICommonInvoice Parent
		{
			get { return (ICommonInvoice)base.Parent; }
			set { base.Parent = value; }
		}

		ZBool Integration.Customs.IBaseJobComInvHeaderCharge.NeedCheckChargeType => GetNeedCheckChargeType();

		protected virtual ZBool GetNeedCheckChargeType()
		{
			return true;
		}

		protected override void RegisterParentType(System.Collections.Generic.List<Type> parentTypes)
		{
			base.RegisterParentType(parentTypes);

			parentTypes.Add(typeof(CommonJobComInvoiceHeader));
			parentTypes.Add(typeof(BaseJobComInvoiceLine));
		}
	}
}
