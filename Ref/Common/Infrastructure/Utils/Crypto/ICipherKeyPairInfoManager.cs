using System.Security.Cryptography;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public interface ICipherKeyPairInfoManager
	{
		RSAParameters GetPublicKey();
		RSAParameters GetPrivateKey();
		byte[] GetAeskey();
	}
}
