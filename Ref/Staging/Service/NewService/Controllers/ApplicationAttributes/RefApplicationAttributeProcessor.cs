using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.Staging.NewService.Test")]
namespace CargoWise.RefDbRepo.Staging.NewService.Controllers
{
	static class RefApplicationAttributeProcessor
	{
		public static IQueryable<RefApplicationAttribute> GetWithoutSecretContent(IEnumerable<RefApplicationAttribute> attributes)
		{
			return attributes.Select(x => new RefApplicationAttribute
			{
				RAA_PK = x.RAA_PK,
				RAA_ConfigFilePath = x.RAA_ConfigFilePath,
				RAA_AttributeName = x.RAA_AttributeName,
				RAA_Value = x.RAA_Value,
				RAA_RAT_NKType = x.RAA_RAT_NKType,
				RAA_JobGroup = x.RAA_JobGroup,
				RAA_Content = x.RAA_RAT_NKType.Equals(SecretFileAttributeType, StringComparison.OrdinalIgnoreCase) ? null : x.RAA_Content,
			}).AsQueryable();
		}

		public static void EncryptCredentialValue(RefApplicationAttribute attribute, IRefDbRepoCrypto refDbRepoCrypto)
		{
			if (attribute.RAA_RAT_NKType.Equals(CredentialAttributeType, StringComparison.OrdinalIgnoreCase))
			{
				attribute.RAA_Content = refDbRepoCrypto.EncryptRSA(attribute.RAA_Value);
				attribute.RAA_Value = string.Empty;
			}
			if(attribute.RAA_RAT_NKType.Equals(SecretFileAttributeType, StringComparison.OrdinalIgnoreCase))
			{
				attribute.RAA_Content = refDbRepoCrypto.EncryptAES(attribute.RAA_Content);
			}
		}

		const string CredentialAttributeType = "Credential";
		const string SecretFileAttributeType = "SecretFile";
	}
}
