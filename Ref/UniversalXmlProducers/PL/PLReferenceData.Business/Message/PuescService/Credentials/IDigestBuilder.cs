using System;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;

interface IDigestBuilder
{
	public (string Nonce, string Digest, string Created) Build(string hashedPassword, Guid? nonce = null, DateTime? creationDateTime = null);
}
