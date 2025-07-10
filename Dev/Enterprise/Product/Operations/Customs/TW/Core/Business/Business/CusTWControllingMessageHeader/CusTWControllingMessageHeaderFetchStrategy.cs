using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	class CusTWControllingMessageHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusTWControllingMessageHeaderFetchStrategy(CusTWControllingMessageHeader controllingMessageHeader)
				: base(controllingMessageHeader)
		{
		}

		protected new CusTWControllingMessageHeader BusinessObject => (CusTWControllingMessageHeader)base.BusinessObject;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var tableColumn in columns)
			{
				switch (tableColumn.ColumnName)
				{
					case CusTWControllingMessageHeader.Schema.ProcessingUnitDescription:
					case CusTWControllingMessageHeader.Schema.BusinessTypeDescription:
					case CusTWControllingMessageHeader.Schema.ControllingMessageTypeDescription:
						{
							Factory.AddFetchHint(CusEntryInstructionSchema.Constants.TableName, BusinessObject.TW1_CEI);
							var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_JE);
							entryInstructionSubQuery.AddToFilter(CusEntryInstructionSchema.PK, BusinessObject.TW1_CEI);
							var declQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
							declQuery.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);
							Factory.AddFetchHint(typeof(JobDeclaration), declQuery);
							break;
						}
					case CusTWControllingMessageHeader.Schema.TW1_ECFAPrintedRemarks:
					case CusTWControllingMessageHeader.Schema.TW_Notes:
						Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
						break;
					case CusTWControllingMessageHeader.Schema.PermitNoExpirationDate:
					case CusTWControllingMessageHeader.Schema.CustomsMessageIdentifier:
					case CusTWControllingMessageHeader.Schema.ProcessingNumber:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						break;
					case CusTWControllingMessageHeader.Schema.PermitNumber:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						Factory.AddFetchHint(RefCountrySchema.RN_Code, (ZString)Core.Constants.CountryCodes.Taiwan);
						break;
				}
			}
		}
	}
}
