using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class JobUSComInvoiceLine : AutoJobUSComInvoiceLine, IJobUSComInvoiceLine, IClusterKeyWorker, IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		public JobUSComInvoiceLine(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[RelatedBusinessObject(nameof(InvoiceLine))]
		public override ZGuid USI_JI
		{
			get => base.USI_JI;
			set => base.USI_JI = value;
		}

		public JobComInvoiceLine InvoiceLine => Factory.Load<JobComInvoiceLine>(USI_JI);

		protected override JobUSComInvoiceLineValidation GetNewValidation()
		{
			return (InvoiceLine?.IsACEDrawback ?? false) ? new DrawbackJobUSComInvoiceLineValidation(this) : base.GetNewValidation();
		}

		#region IClusterKeyWorker Implementation

		Type IClusterKeyWorker.ParentBizObjType => typeof(JobComInvoiceLine);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)USI_JIInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)USI_ClusterKeyInfo;

		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter Members
		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueIndexFailureHandler(this); }
		}

		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => JobUSComInvoiceLineSchema.Constants.Indexes.FK_UX__USI_JI;
		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => InvoiceLine;
		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => InvoiceLine?.JI_SystemLastEditUser ?? ZString.Empty;
		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => InvoiceLine?.JI_SystemLastEditTimeUtc ?? ZDateTime.Empty;
		#endregion
	}
}
