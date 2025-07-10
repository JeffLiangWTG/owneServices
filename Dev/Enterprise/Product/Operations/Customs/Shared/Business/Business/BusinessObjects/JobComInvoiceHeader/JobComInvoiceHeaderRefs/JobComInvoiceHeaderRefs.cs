using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseJobComInvoiceHeader), "InvoiceHeaderRefs")]
	public class JobComInvoiceHeaderRefs : AutoJobComInvoiceHeaderRefs, Integration.Customs.Shared.IJobComInvoiceHeaderRefs, ITypeDeciderContext, IClusterKeyWorker
	{
		public JobComInvoiceHeaderRefs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Constant
		public static class Constants
		{
			public const string CCN = "CCN";
			public const string CTR = "CTR";
		}
		#endregion

		public static readonly JobComInvoiceHeaderRefsTypeDecider TypeDecider = new JobComInvoiceHeaderRefsTypeDecider();

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("InvoiceHeader")]
		public override ZGuid J2_JZ
		{
			get { return base.J2_JZ; }
			set { base.J2_JZ = value; }
		}

		public BaseJobComInvoiceHeader InvoiceHeader
		{
			get { return Factory.Load<BaseJobComInvoiceHeader>(J2_JZ); }
		}

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (InvoiceHeader as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)J2_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobComInvoiceHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)J2_JZInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
