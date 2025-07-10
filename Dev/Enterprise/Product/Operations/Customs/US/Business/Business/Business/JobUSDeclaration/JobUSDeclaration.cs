using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class JobUSDeclaration : AutoJobUSDeclaration, IClusterKeyWorker, IJobUSDeclaration, IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter
	{
		public JobUSDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Override

		[RelatedBusinessObject(nameof(Declaration))]
		public override ZGuid USD_JE
		{
			get => base.USD_JE;
			set => base.USD_JE = value;
		}

		public override bool IsSavedByFactory => isPersistent && base.IsSavedByFactory;

		#endregion

		public bool IsPersistent => isPersistent;
		bool isPersistent = true;

		public void MakeNonPersistent()
		{
			isPersistent = false;
		}

		public JobDeclaration Declaration => Factory.Load<JobDeclaration>(USD_JE);

		#region IClusterKeyWorker Members

		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)USD_JEInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)USD_ClusterKeyInfo;

		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter Members
		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueClusterKeyIndexFailureHandler(this); }
		}

		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => JobUSDeclarationSchema.Constants.Indexes.FK_UX__USD_JE;
		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => Declaration;
		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => USD_SystemLastEditUser;
		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => USD_SystemLastEditTimeUtc;
		#endregion

		#region IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter Members
		string IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.UniqueClusterIndexName => JobUSDeclarationSchema.Constants.Indexes.NR_UC__USD_ClusterKey;
		IClusterKeyMaster IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyMaster => Declaration;
		SchemaIntColumn IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyColumn => JobUSDeclarationSchema.USD_ClusterKey;
		#endregion
	}
}
