using Enterprise.Customs.Business.Testing;

namespace Enterprise.BatchProcessor.Customs.Testing
{
	class BaseInterchangeRetrieverTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		#region Implementation

		protected const string TestMessage = @"UNH+345600+CUSRES:002:912:UN'
BGM+803:119:95++9:200404080923:203+9+ZZ:9227340*4011'
LOC+5:D027K:156:95+8:D015K:156:95'
RFF+CNN:INBU3235287'
RFF+NP:000000000000000001'
RFF+CNN:PONU0726869'
RFF+NP:000000000000000001'
UNT+8+345600'";

		#endregion
	}
}
