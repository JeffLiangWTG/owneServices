using System.Security.Cryptography;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AESCryptoTest : TestCaseWithFactory
	{
		public void TestEncryptAndDecryptStringWithAES()
		{
			var aesCrypto = new AESCrypto();
			var originalText = "<RSAKeyValue><Modulus>zFp32wGZy5AStfmdHddNrafBm0kkmUj/LosWb0/2GtCDXs++Y0tmaS/t7PicP1Uozon6k9FXTdjncc/Rb7Zl9hJJjpjoWyesoXEH/raurd3BMMsByfS2tkv5uu02fcABVkwPiImBkJXpIWKxubxbiZMq9qDwwCZaPHuWbBesPUU=</Modulus><Exponent>AQAB</Exponent><P>0tF3WCmxHYSzul/DbI99tol8X5HxQylMeskkb9Y+X+JWc+ou8MbI5i/ujMAuWT7xhunta/2b/gsLQXr57JFFSw==</P><Q>+CZPHDmKli+gkLZLvNORA/BgXqOG9q7AvrjIG9HuuyGSQf4h+iXk+WWpHE67HgScFZQ6fkODz6wehHCVTrE9rw==</Q><DP>oAMrEXi7nUsO26Q6AVk8MuNRynxMYgyjpwKqrFQyDbcOpXaXYFuROt4gsyZfR4/15NADTBc3YnBhf7bmoX911w==</DP><DQ>1lVQc1KID1yn1RZ/qGMcmEhhFJ0uD5e1R4aW4OCc0OkYSQPWGRfuBDP4s0iVekIFBuZ292QdT5yc50vOyp9wZQ==</DQ><InverseQ>Ge27b4riL8GtcNMYgSBEGTYi+n0vEJLvjZNKGJqSAz/XSqfkA4R6NKkw4267Bb6JLmPkOmtPS7MoJJeQx4uTqg==</InverseQ><D>jCajuuUfKFg4LOvz0KqAENBT3P9OBX7l3HLxwQfTHtLQtm6+AXWN2ChSAksDRgBOy1AgNc7GFJLlMM45smcjB2T+dshtMIQl0CttAUEjkxHr8n4QhyElYklTN4z6wN5XHRh07xVowRAQvQ0+1537dAfavq7WRp9jwAouE+lq7aU=</D></RSAKeyValue>";
			var encryptedText = aesCrypto.EncryptStringAES(originalText, AESCrypto.RANDOM_SHAREDSECRET);
			var decryptedText = aesCrypto.DecryptStringAES(encryptedText, AESCrypto.RANDOM_SHAREDSECRET);
			AssertEquals(originalText, decryptedText);
		}

		public void TestEncryptAndDecryptStringWithAESAlgorithm()
		{
			var aesCrypto = new AESCrypto(HashAlgorithmName.SHA256);
			var originalText = "<RSAKeyValue><Modulus>zFp32wGZy5AStfmdHddNrafBm0kkmUj/LosWb0/2GtCDXs++Y0tmaS/t7PicP1Uozon6k9FXTdjncc/Rb7Zl9hJJjpjoWyesoXEH/raurd3BMMsByfS2tkv5uu02fcABVkwPiImBkJXpIWKxubxbiZMq9qDwwCZaPHuWbBesPUU=</Modulus><Exponent>AQAB</Exponent><P>0tF3WCmxHYSzul/DbI99tol8X5HxQylMeskkb9Y+X+JWc+ou8MbI5i/ujMAuWT7xhunta/2b/gsLQXr57JFFSw==</P><Q>+CZPHDmKli+gkLZLvNORA/BgXqOG9q7AvrjIG9HuuyGSQf4h+iXk+WWpHE67HgScFZQ6fkODz6wehHCVTrE9rw==</Q><DP>oAMrEXi7nUsO26Q6AVk8MuNRynxMYgyjpwKqrFQyDbcOpXaXYFuROt4gsyZfR4/15NADTBc3YnBhf7bmoX911w==</DP><DQ>1lVQc1KID1yn1RZ/qGMcmEhhFJ0uD5e1R4aW4OCc0OkYSQPWGRfuBDP4s0iVekIFBuZ292QdT5yc50vOyp9wZQ==</DQ><InverseQ>Ge27b4riL8GtcNMYgSBEGTYi+n0vEJLvjZNKGJqSAz/XSqfkA4R6NKkw4267Bb6JLmPkOmtPS7MoJJeQx4uTqg==</InverseQ><D>jCajuuUfKFg4LOvz0KqAENBT3P9OBX7l3HLxwQfTHtLQtm6+AXWN2ChSAksDRgBOy1AgNc7GFJLlMM45smcjB2T+dshtMIQl0CttAUEjkxHr8n4QhyElYklTN4z6wN5XHRh07xVowRAQvQ0+1537dAfavq7WRp9jwAouE+lq7aU=</D></RSAKeyValue>";
			var encryptedText = aesCrypto.EncryptStringAES(originalText, AESCrypto.RANDOM_SHAREDSECRET);
			var decryptedText = aesCrypto.DecryptStringAES(encryptedText, AESCrypto.RANDOM_SHAREDSECRET);
			AssertEquals(originalText, decryptedText);
		}
	}
}
