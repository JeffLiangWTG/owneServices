using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceLineTax : AutoJobComInvoiceLineTax, ITypeDeciderContext, IClusterKeyWorker
	{
		public JobComInvoiceLineTax(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region JLT_JI

		[RelatedBusinessObject("InvoiceLine")]
		public override ZGuid JLT_JI
		{
			get { return base.JLT_JI; }
			set { base.JLT_JI = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.TypeList))]
		public override ZString JLT_Type { get => base.JLT_Type; set => base.JLT_Type = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.MethodOfCalculationList))]
		public override ZString JLT_MethodOfCalculation { get => base.JLT_MethodOfCalculation; set => base.JLT_MethodOfCalculation = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.MOPList))]
		public override ZString JLT_MethodOfPayment { get => base.JLT_MethodOfPayment; set => base.JLT_MethodOfPayment = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.RateOverrideList))]
		public override ZString JLT_RateOverrideReasonCode { get => base.JLT_RateOverrideReasonCode; set => base.JLT_RateOverrideReasonCode = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.BaseQuantityUQList))]
		public override ZString JLT_BaseQuantityUQ { get => base.JLT_BaseQuantityUQ; set => base.JLT_BaseQuantityUQ = value; }

		public BaseJobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<BaseJobComInvoiceLine>(JLT_JI); }
		}

		#endregion

		public static readonly JobComInvoiceLineTaxTypeDecider TypeDecider = new JobComInvoiceLineTaxTypeDecider();

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (InvoiceLine as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)JLT_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobComInvoiceLine);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)JLT_JIInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
