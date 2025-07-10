using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconRefundedCharge : CusCodeData
	{
		public ReconRefundedCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ReconRefundedCharge;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(new Type[] { typeof(JobDeclaration), typeof(CusEntryHeader), typeof(JobComInvoiceLine) }); }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new ReconRefundedChargeLookups(this);
		}
	}
}
