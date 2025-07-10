namespace Enterprise.MasterFiles.Business
{
	public interface IAESCrypto
	{
		string DecryptStringAES(string cipherText, string sharedSecret);
		string EncryptStringAES(string plainText, string sharedSecret);
	}
}