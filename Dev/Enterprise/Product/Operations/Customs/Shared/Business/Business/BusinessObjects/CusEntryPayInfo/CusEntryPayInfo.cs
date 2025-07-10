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
	public class CusEntryPayInfo : AutoCusEntryPayInfo, ITypeDeciderContext, IClusterKeyWorker
	{
		public CusEntryPayInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusEntryPayInfoTypeDecider TypeDecider = new CusEntryPayInfoTypeDecider();

		public bool IsPending
		{
			get { return C9_PaymentStatus == CusEntryPayInfoStatusList.Codes.Pending; }
		}

		[RelatedBusinessObject("EntryHeader")]
		public override ZGuid C9_CH
		{
			get { return base.C9_CH; }
			set { base.C9_CH = value; }
		}

		public CusEntryHeader EntryHeader
		{
			get { return Factory.Load<CusEntryHeader>(C9_CH); }
		}

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (EntryHeader as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)C9_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(CusEntryHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)C9_CHInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
