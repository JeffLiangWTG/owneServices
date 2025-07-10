using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.FetchStrategies
{
	public class JobDeclarationCollectionFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationCollectionFetchStrategy
	{
		public JobDeclarationCollectionFetchStrategy(JobDeclarationCollection collection)
			: base(collection)
		{
		}

		protected new JobDeclarationCollection Collection
		{
			get { return (JobDeclarationCollection)base.Collection; }
		}

		protected override bool IsCusEntryNumRelatedColumn(string columnName)
		{
			return base.IsCusEntryNumRelatedColumn(columnName) || columnName == JobDeclaration.Schema.CertificateNumber;
		}

		protected override bool IsCusContainerRelatedColumn(string columnName)
		{
			return base.IsCusContainerRelatedColumn(columnName) || columnName == JobDeclaration.Schema.JE_ContainerCount;
		}

		protected override bool IsCusDecHouseBillRelatedColumn(string columnName)
		{
			return base.IsCusDecHouseBillRelatedColumn(columnName) || columnName == SGAddInfoSchema.Constants.SG_OutwardTransportMode;
		}

		protected override bool IsJobDocAddressRelatedColumn(string columnName)
		{
			return base.IsJobDocAddressRelatedColumn(columnName) || columnName == "OutwardShippingLineForwarderDocAddress+Organisation+OH_Code";
		}
	}
}
