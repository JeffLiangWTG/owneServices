using CargoWise.eHub.Products.NZCustoms.PullService;

namespace CargoWise.eHub.Products.NZCustoms.Tests
{
	class TestFileManager : IFileManager
	{
        public string SaveReceivedMessage(Common.NZCustomsReply message, string messageReference)
        {
            return "fileName1";
        }
    }
}
