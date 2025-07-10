using System.Data;
using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocumentForTestBoxNumber : JobRequiredDocument
	{
		public JobRequiredDocumentForTestBoxNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override IBoxNumberProvider GetBoxNumberProvider()
		{
			return new BoxNumberProviderForTest();
		}
	}
}
