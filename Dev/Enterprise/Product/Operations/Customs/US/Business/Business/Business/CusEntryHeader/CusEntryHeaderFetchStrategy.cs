using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class CusEntryHeaderFetchStrategy : Customs.Business.FetchStrategies.CusEntryHeaderFetchStrategy
	{
		public CusEntryHeaderFetchStrategy(CusEntryHeader entry)
			: base(entry)
		{
		}

		protected new CusEntryHeader BusinessObject
		{
			get { return (CusEntryHeader)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			if (BusinessObject.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry)
			{
				Factory.AddFetchHint(CusEntryHeaderSchema.PK, BusinessObject.CH_CH_PrimeEntry);
			}

			Factory.AddFetchHint(CusDispositionSchema.CDI_ParentID, BusinessObject.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			bool isBGMReferenceUserEnterable = BusinessObject.IsExport && !BusinessObject.CH_BGMReferenceInfo.ReadOnly || BusinessObject.IsReconImportEntry;
			if (isBGMReferenceUserEnterable && !BusinessObject.CH_BGMReference.IsEmpty)
			{
				ZQuery secondQuery;
				ZQuery mainQuery;

				GetReconOriginalEntryOrExportDuplicateQuery(out mainQuery, out secondQuery, BusinessObject.CH_BGMReference, BusinessObject.PK, BusinessObject.CH_MessageType);
				Factory.AddFetchHint(CusEntryHeaderSchema.Instance, mainQuery, secondQuery);
			}
		}

		internal static ZQuery GetReconOriginalEntryOrExportDuplicateQuery(ZString reference, ZGuid pk, ZString messageType)
		{
			ZQuery secondQuery;
			ZQuery mainQuery;
			GetReconOriginalEntryOrExportDuplicateQuery(out mainQuery, out secondQuery, reference, pk, messageType);
			mainQuery.AddToFilter(secondQuery);
			return mainQuery;
		}

		static void GetReconOriginalEntryOrExportDuplicateQuery(out ZQuery mainQuery, out ZQuery secondQuery, ZString reference, ZGuid pk, ZString messageType)
		{
			secondQuery = new ZQuery(CusEntryHeaderSchema.PK, SQLComparisonOperator.NotEqual, pk);
			secondQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, reference);
			mainQuery = new ZQuery(CusEntryHeaderSchema.CH_MessageType, messageType);
		}
	}
}
