using System;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Message;

[TestFixture]
sealed class DigestBuilderTests
{
	[Test]
	public void TestBuild()
	{
		var guid = new Guid("33223dea-66d6-401d-94e8-13c29a2681a7");
		var utcNow = new DateTime(2022, 6, 20, 6, 20, 3);
		var (nouce, digest, created) = new DigestBuilder().Build("e2898ae71012671e0ff1d70747fa620958022cd8", guid, utcNow);
		Assert.Multiple(() =>
		{
			Assert.That(nouce, Is.EqualTo("FMkbl0dxVeoNttMqS8lMA1IR0yA="), "Nonce");
			Assert.That(digest, Is.EqualTo("FEOJix5+hq+ORblivn/ra2Ap7Rc="), "Digest");
			Assert.That(created, Is.EqualTo("2022-06-20T06:20:03.000Z"), "Created");

			Assert.Throws<ArgumentException>(() => new DigestBuilder().Build("Password", guid, utcNow), "No sha1 password");

			Assert.Throws<ArgumentException>(() => new DigestBuilder().Build("e28", guid, utcNow), "Wrong sha1 length");
		});
	}
}
