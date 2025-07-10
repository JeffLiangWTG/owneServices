using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class CusEntryHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusEntryHeaderFetchStrategy(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected new CusEntryHeader BusinessObject
		{
			get { return (CusEntryHeader)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(BaseJobDeclaration), BusinessObject.CH_JE);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusEntryLineSchema.CL_CH, BusinessObject.PK);
			Factory.AddFetchHint(CusEntryHeaderChargesSchema.C1_CH, BusinessObject.PK);
			Factory.AddFetchHint(CusEntryPayInfoSchema.C9_CH, BusinessObject.PK);
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			if (BusinessObject.Declaration?.SupportContainerEntryHeaderPivot ?? false)
			{
				Factory.AddFetchHint(CusContainerEntryHeaderPivotSchema.CCE_CH_EntryHeader, BusinessObject.PK);
			}
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			if (BusinessObject.IsInDatabase)
			{
				Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, BusinessObject.PK);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			foreach (var column in columns)
			{
				var columnName = column.ColumnName;
				switch (columnName)
				{
					case CusEntryHeader.Schema.CustomsValue:
					case CusEntryHeader.Schema.EffectiveValuationDate:
					case CusEntryHeader.Schema.PackagesCount:
					case CusEntryHeader.Schema.GSTAmount:
						{
							Factory.AddFetchHint(CusEntryLineSchema.CL_CH, BusinessObject.PK);
							break;
						}
					case CusEntryHeader.Schema.TotalAmountPayable:
						Factory.AddFetchHint(CusEntryLineSchema.CL_CH, BusinessObject.PK);
						Factory.AddFetchHint(CusEntryLineFeeSchema.CF_ClusterKey, BusinessObject.CH_ClusterKey);
						break;
					case CusEntryHeader.Schema.Duty:
					case CusEntryHeader.Schema.VAT:
						Factory.AddFetchHint(CusEntryHeaderChargesSchema.C1_CH, BusinessObject.PK);
						break;
					case CusEntryHeader.Schema.EntryNumber:
					case CusEntryHeader.Schema.MovementReferenceNumber:
					case CusEntryHeader.Schema.MovementReferenceNumberEntryStatus:
					case CusEntryHeader.Schema.MovementReferenceNumberIssueDate:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						break;
					case CusEntryHeader.Schema.CH_CustomsDeliveryInstructions:
					case CusEntryHeader.Schema.CH_CustomsMessageRemarks:
						Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
						break;
					case CusEntryHeader.Schema.ClearanceDate:
					case CusEntryHeader.Schema.DeclarationDate:
						Factory.AddFetchHint(StmALogSchema.SL_Parent, BusinessObject.PK);
						break;
				}
			}

			base.FetchForViewCore(columns);
		}
	}
}
