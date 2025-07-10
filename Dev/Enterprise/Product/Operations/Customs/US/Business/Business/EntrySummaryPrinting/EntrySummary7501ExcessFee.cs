using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public abstract class EntrySummary7501ExcessFee : NonPersistentBusinessObject, IDocumentDeliveredLogSupporter, IParentDocManagerSupport
	{
		protected EntrySummary7501ExcessFee(CusEntryHeader entry, IFee fee)
			: base(entry.Factory)
		{
			this.entry = entry;
			this.fee = fee;
		}

		protected readonly CusEntryHeader entry;
		protected readonly IFee fee;

		#region IEntrySummaryExcessFees Members

		public abstract ZString SummaryFeeDesc { get; }
		public abstract ZDecimal SummaryFee { get; }

		#endregion

		#region IDocumentDeliveredLogSupporter Members

		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst
		{
			get { return entry.GetType(); }
		}

		ZGuid IDocumentDeliveredLogSupporter.Identifier
		{
			get { return entry.PK; }
		}

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get { return entry.PK; }
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
