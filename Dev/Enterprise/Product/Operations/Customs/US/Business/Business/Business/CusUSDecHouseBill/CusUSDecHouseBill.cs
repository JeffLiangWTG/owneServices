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
	public class CusUSDecHouseBill : AutoCusUSDecHouseBill, ICusUSDecHouseBill, IClusterKeyWorker, IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		public CusUSDecHouseBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public Bill Bill => Factory.Load<Bill>(USB_CU);

		[RelatedBusinessObject(nameof(Bill))]
		public override ZGuid USB_CU
		{
			get => base.USB_CU;
			set => base.USB_CU = value;
		}

		public override bool IsSavedByFactory => base.IsSavedByFactory && (IsDeleted || Bill.IsInDatabase || Bill.IsSavedByFactory);

		#region IClusterKeyWorker Members
		Type IClusterKeyWorker.ParentBizObjType => typeof(Bill);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)USB_CUInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;
		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)USB_ClusterKeyInfo;
		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter Members
		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueIndexFailureHandler(this); }
		}

		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => CusUSDecHouseBillSchema.Constants.Indexes.FK_UX__USB_CU;
		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => Bill;
		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => USB_SystemLastEditUser;
		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => USB_SystemLastEditTimeUtc;
		#endregion
	}
}
