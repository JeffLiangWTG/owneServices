using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business.Testing;
public class XTMsgAttrProviderForGlbExternalPasswordTest : TestCaseWithFactory
{
	public void TestGetMessageAttrDictionary_ReturnsEmptyDictionaryWhenInvalidType()
	{
		var testCases = new List<Func<Dictionary<string, string>>>
		{
			() =>
			{
				var provider = (IxTMessageAttributeProvider)new InvalidCredentialType();
				return provider.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();
			},
			() => 
			{
				var provider = (IxTMessageAttributeProvider)new InvalidCredentialType();
				return provider.GetXTMsgAttrProviderForGlbExternalPasswordSendingHttpUserAndPassword(false);
			},
			() => 
			{
				var provider = (IxTMessageAttributeProvider)new InvalidCredentialType();
				return provider.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificateThumbprint();
			}
		};

		foreach (var testCase in testCases)
		{
			var dictionary = testCase();

			AssertNotNull("The dictionary should not be null.", dictionary);
			AssertEquals("The dictionary should be empty when the object is not of type GlbExternalPassword.", 0, dictionary.Count);
		}
	}

	public void TestGetMessageAttrDictionaryCertificatePEM()
	{
		var testPassword = Factory.New<GlbExternalPasswordForTest>();
		testPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		testPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

		var provider = (IxTMessageAttributeProvider)testPassword;
		var result = provider.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();

		(var certKey, var cert) = testPassword.ExportRSAKeyAndCertificateStringAsPEM();
		var expected = new Dictionary<string, string>
		{
			{ xTMessaging.Shared.Constants.xTMsgAttributes.cw1key, X509Certificate2TestHelper.ValidCertificate_KeyPEM },
			{ xTMessaging.Shared.Constants.xTMsgAttributes.cw1certificate, X509Certificate2TestHelper.ValidCertificate_CertPEM }
		};
		AssertContainsExactElementsInAnyOrder(expected, result);
	}

	public void TestGetMessageAttrDictionaryHttpUserAndPasswordNoEncrypt()
	{
		var testPassword = Factory.New<GlbExternalPasswordForTest>();
		testPassword.GP_UserID = "user";
		testPassword.GP_CurrentPassword = "password";

		var provider = (IxTMessageAttributeProvider)testPassword;
		var result = provider.GetXTMsgAttrProviderForGlbExternalPasswordSendingHttpUserAndPassword(false);

		var expected = new Dictionary<string, string>
		{
			{ xTMessaging.Shared.Constants.xTMsgAttributes.HttpClientUser, "user" },
			{ xTMessaging.Shared.Constants.xTMsgAttributes.HttpClientPassword, "password" }
		};
		AssertContainsExactElementsInAnyOrder(expected, result);
	}

	public void TestGetMessageAttrDictionaryHttpUserAndPasswordEncrypt()
	{
		var testPassword = Factory.New<GlbExternalPasswordForTest>();
		testPassword.GP_UserID = "user";
		testPassword.CurrentDecryptedPassword = "password";

		var provider = (IxTMessageAttributeProvider)testPassword;
		var result = provider.GetXTMsgAttrProviderForGlbExternalPasswordSendingHttpUserAndPassword(true);
		AssertEquals(xTMessaging.Shared.Constants.xTMsgAttributes.HttpClientUser, result.ElementAt(0).Key);
		AssertEquals("user", result.ElementAt(0).Value);

		string decodedPassword = DecodePassword(result.ElementAt(1).Value);
		AssertEquals(xTMessaging.Shared.Constants.xTMsgAttributes.HttpClientPassword, result.ElementAt(1).Key);
		AssertEquals("password", decodedPassword);
	}
	
	public void TestGetMessageAttrDictionaryCertificateThumbprint()
	{
		var testPassword = Factory.New<GlbExternalPasswordForTest>();
		testPassword.GP_UserID = "user";

		var provider = (IxTMessageAttributeProvider)testPassword;
		var result = provider.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificateThumbprint();

		var expected = new Dictionary<string, string>
		{
			{ xTMessaging.Shared.Constants.xTMsgAttributes.anycertificate,"xt-certificate:" + "user" }
		};
		AssertContainsExactElementsInAnyOrder(expected, result);
	}

	#region Implementation

	public class GlbExternalPasswordForTest : GlbExternalPassword, IxTMessageAttributeProvider
	{
		public GlbExternalPasswordForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			throw new NotImplementedException();
		}
	}

	public class InvalidCredentialType : IxTMessageAttributeProvider
	{
		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			throw new NotImplementedException();
		}
	}

	string DecodePassword(string password)
	{
		using (var rsa = new RSACryptoServiceProvider(2048))
		{
			var keyXml = "<RSAKeyValue><Modulus>1XmSsYj8fup2QDmo2Yp0nKVzNsonnECcN2OcZXjF+eIEgQyLydnwngBcdW7UOwqQ0E+ffFcdoW44IaTVZ/A7yNC/pRJQy42BBcXteSo7SqgHt7yvPUph+oFpEWLIe0WnPJDmZCRFNoZnMUoOzK89oA/v0I3neveYHjM0bRgtVBE=</Modulus><Exponent>AQAB</Exponent><P>5sXEF2kBcvgWJ1FSo1rahov1KiqogxqBOJio3FmsLsjafwV9O1xKGmjuL1gdQbgd7rPI7x8b42PTfGctlAIPdQ==</P><Q>7M+6YC9HcPeNvr6/S+jX7tP+OV6Xljqe/U9/j7fisEG/9cfFz9lh+H+oItB1MpOD/dKB3RNW5/ODA5TSsHsarQ==</Q><DP>PQtja63jLD5j3dKtQXjvBVhQae8O1F9Wf1oikOdHnLiU07ToA6POFl5bYzqzwoappFL6fAaGogfuEaJZdCV3YQ==</DP><DQ>5iFYtXA8tQNdtCgaLuKwNV++hnHuTgfZycEf7cJ9gVvj+C2ThlFya9NiybJasjO46UlQ+k54/iAfCbPuq6J2YQ==</DQ><InverseQ>k3hB5HGpUKFMYBO+Dh7dkJJ6z7TGKevKWHw3Rhv6aYFOqVvDBKQyW52pbh4aHv5eX15QrBJNPnXYmovSYj47oQ==</InverseQ><D>A/CjvA+bcMCP5f+6cFNtc2OxWe+cELaiO3p6QpGFU+dE7oMmWazM/lmNhfmsRJpdZwmFLR9oKQMsBDZIMwwnH5/WD+gso3VmPRsg5BIdoOs/F0ZkR1qeUlwbjmaD+fPV4b8Tg/3JVeSWT7K8xtKGMLWDcdLMA3vaRo/1/g+KfD0=</D></RSAKeyValue>";
			var text = Convert.FromBase64String(password);
			rsa.FromXmlString(keyXml);
			return new string(System.Text.Encoding.UTF8.GetChars(rsa.Decrypt(text, true)));
		}
	}

	#endregion
}
