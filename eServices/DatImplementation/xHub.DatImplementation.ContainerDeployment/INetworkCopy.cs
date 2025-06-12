using Dat.Integration;

namespace xHub.DatImplementation.ContainerDeployment
{
    public interface INetworkCopy
    {
        void CopyFiles(List<string> zipfiles, Server settings, string userTestPK);
    }
}
