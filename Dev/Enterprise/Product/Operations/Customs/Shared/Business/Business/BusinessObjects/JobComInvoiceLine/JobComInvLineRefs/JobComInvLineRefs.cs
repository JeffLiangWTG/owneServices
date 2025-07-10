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
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(BaseJobComInvoiceLine), "InvoiceLineRefs")]
	public class JobComInvLineRefs : AutoJobComInvLineRefs, Integration.Customs.Shared.IJobComInvLineRefs, ITypeDeciderContext, IClusterKeyWorker
	{
		public JobComInvLineRefs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly JobComInvLineRefsTypeDecider TypeDecider = new JobComInvLineRefsTypeDecider();

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("InvoiceLine")]
		public override ZGuid JG_JI
		{
			get { return base.JG_JI; }
			set { base.JG_JI = value; }
		}

		public BaseJobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<BaseJobComInvoiceLine>(JG_JI); }
		}

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (InvoiceLine as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)JG_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobComInvoiceLine);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)JG_JIInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
