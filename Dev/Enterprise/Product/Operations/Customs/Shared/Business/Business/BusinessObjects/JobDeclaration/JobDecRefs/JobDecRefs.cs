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
	[DependentBusinessObject(typeof(BaseJobDeclaration), "DeclarationRefs")]
	public class JobDecRefs : AutoJobDecRefs, Integration.Customs.Shared.IJobDecRefs, ITypeDeciderContext, IClusterKeyWorker
	{
		public JobDecRefs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly JobDecRefsTypeDecider TypeDecider = new JobDecRefsTypeDecider();

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("Declaration")]
		public override ZGuid J3_JE
		{
			get { return base.J3_JE; }
			set { base.J3_JE = value; }
		}

		public BaseJobDeclaration Declaration
		{
			get { return Factory.Load<BaseJobDeclaration>(J3_JE); }
		}

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Declaration as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)J3_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)J3_JEInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
