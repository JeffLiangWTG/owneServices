using Enterprise.Freight.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ConsolContainerValueObjectDataAdapterForTest : ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>
	{
		public ConsolContainerValueObjectDataAdapterForTest(CommonConsol consol)
			: base(consol)
		{
		}

		public bool ValueOfRegistryDefaultForImporting()
		{
			return base.RegistryDefaultForImporting;
		}
	}
}
