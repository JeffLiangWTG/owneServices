using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationDocumentGoodsDescriptionConfigCollection : NonPersistentBusinessObjectCollection<JobDeclarationDocumentGoodsDescriptionConfig>
	{
		public JobDeclarationDocumentGoodsDescriptionConfigCollection(JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig)
			: base(jobDeclarationDocumentAddressConfig.Factory)
		{
			this.jobDeclarationDocumentAddressConfig = Argument.NotNull(jobDeclarationDocumentAddressConfig, nameof(jobDeclarationDocumentAddressConfig));
		}

		readonly JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobDeclarationDocumentGoodsDescriptionConfig(jobDeclarationDocumentAddressConfig);
		}
	}
}
