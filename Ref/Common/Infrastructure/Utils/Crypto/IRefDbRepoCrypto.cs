namespace CargoWise.RefDbRepo.Common.Utils
{
	public interface IRefDbRepoCrypto
	{
		byte[] EncryptRSA(string plainText);
		string DecryptRSA(byte[] encryptedData);
		byte[] EncryptAES(byte[] originalData);
		byte[] DecryptAES(byte[] encryptedData);
	}
}
