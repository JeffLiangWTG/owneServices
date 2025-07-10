using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class ExportCustomsManifestHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ExportCustomsManifestHeaderFetchStrategy(ExportCustomsManifestHeader exportCustomsManifestHeader)
			: base(exportCustomsManifestHeader)
		{
		}

		protected new ExportCustomsManifestHeader BusinessObject
		{
			get { return (ExportCustomsManifestHeader)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				if (column.ColumnName == ExportCustomsManifestHeader.Schema.MessageStatus)
				{
					Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
				}
			}
		}
	}
}
