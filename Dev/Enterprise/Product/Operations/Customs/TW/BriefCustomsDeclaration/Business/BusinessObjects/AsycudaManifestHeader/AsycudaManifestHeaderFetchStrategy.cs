using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaManifestHeaderFetchStrategy : ASYCUDA.Business.AsycudaManifestHeaderFetchStrategy
	{
		public AsycudaManifestHeaderFetchStrategy(AsycudaManifestHeader header) : base(header)
		{
		}

		protected new AsycudaManifestHeader BusinessObject => (AsycudaManifestHeader)base.BusinessObject;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case AsycudaManifestHeader.Schema.BagNumber:
					case AsycudaManifestHeader.Schema.DeclarationDate:
					case AsycudaManifestHeader.Schema.DeclarationNumber:
					case AsycudaManifestHeader.Schema.DeclarationNumberDisplay:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						break;
				}
			}
		}
	}
}
