using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public abstract class EntrySummary7501Bill : NonPersistentBusinessObject, IDocumentDeliveredLogSupporter, IParentDocManagerSupport
	{
		protected EntrySummary7501Bill(ZGuid entryPK, BusinessObjectFactory factory)
			: base(factory)
		{
			this.entryPK = entryPK;
		}

		protected readonly ZGuid entryPK;

		#region IEntrySummaryBills Members

		public abstract ZDateTime ITDate { get; }
		public abstract ZString ITNO { get; }
		public abstract ZString EffectiveMasterBillIssuerSCAC { get; }
		public abstract ZString MasterBill { get; }
		public abstract ZString EffectiveHouseBillIssuerSCAC { get; }
		public abstract ZString HouseBill { get; }
		public abstract ZString EffectiveSubHouseBillIssuerSCAC { get; }
		public abstract ZString SubHouseBill { get; }
		public abstract ZInt PkgQty { get; }
		public abstract ZString PkgType { get; }

		#endregion

		#region IDocumentDeliveredLogSupporter Members

		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst
		{
			get { return typeof(CusEntryHeader); }
		}

		ZGuid IDocumentDeliveredLogSupporter.Identifier
		{
			get { return entryPK; }
		}

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get { return entryPK; }
		}

		ZString IParentDocManagerSupport.ParentTableName
		{
			get { return CusEntryHeaderSchema.Constants.TableName; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsEntry)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
