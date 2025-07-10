using System;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Message;

[TestFixture]
sealed class PuescSecurityTokenSerializerTest
{
	[Test]
	public void TestWriteTokenCore()
	{
		using var stringWriter = new StringWriter();
		using var xmlWriter = XmlWriter.Create(stringWriter);
		var userName = "TestUserName";
		var password = Convert.ToHexString(CreateSHA1Hash("TestPassword"));
		var tokenId = "TestTokenId";
		var token = new UserNameSecurityToken(userName, password, tokenId);

		new PuescSecurityTokenSerializerForTest().WriteTokenCoreExposed(xmlWriter, token);
		xmlWriter.Flush();
		Assert.Multiple(() =>
		{
			Assert.That(stringWriter.ToString(), Contains.Substring("Password"), "Contains password element");
			Assert.That(stringWriter.ToString(), Contains.Substring(userName), "Contains user name.");
			Assert.That(stringWriter.ToString(), Contains.Substring(tokenId), "Contains token id.");
		});
	}

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
	static byte[] CreateSHA1Hash(string password) => SHA1.HashData(Encoding.UTF8.GetBytes(password));
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
}


sealed class PuescSecurityTokenSerializerForTest : PuescSecurityTokenSerializer
{
	public void WriteTokenCoreExposed(XmlWriter writer, SecurityToken token) => base.WriteTokenCore(writer, token);
}
