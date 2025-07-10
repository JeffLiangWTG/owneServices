using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobSGDeclaration : AutoJobSGDeclaration, IClusterKeyWorker, IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter
	{
		public JobSGDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		public JobDeclaration Parent => Factory.Load<JobDeclaration>(SGE_JE);

		public override bool IsSavedByFactory => base.IsSavedByFactory && (IsDeleted || Parent.IsInDatabase || Parent.IsSavedByFactory);

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueClusterKeyIndexFailureHandler(this); }
		}

		#region IClusterKeyWorker
		Type IClusterKeyWorker.ParentBizObjType => typeof(JobDeclaration);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)SGE_JEInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)SGE_ClusterKeyInfo;
		#endregion

		#region IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter
		string IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.UniqueClusterIndexName => JobSGDeclarationSchema.Constants.Indexes.NR_UC__SGE_ClusterKey;

		IClusterKeyMaster IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyMaster => Parent;

		SchemaIntColumn IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyColumn => JobSGDeclarationSchema.SGE_ClusterKey;
		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter
		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => JobSGDeclarationSchema.Constants.Indexes.FK_UX__SGE_JE;

		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => Parent;

		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => SGE_SystemLastEditUser;

		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => SGE_SystemLastEditTimeUtc;
		#endregion
	}
}
