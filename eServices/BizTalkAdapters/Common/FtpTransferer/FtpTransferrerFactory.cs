
namespace CargoWise.eHub.BizTalkAdapters.Common
{
    public class FtpTransferrerFactory : ITransferrerFactory
    {
        public virtual ITransferrer CreateTransferrer()
        {
            return new FtpTransferrer();
        }
    }
}
