using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.ApplicationConfig
{
	public sealed class RepoConfigProvider : ConfigurationProvider
	{
		public override void Load()
		{
#if DEBUG
			//This setting is only for unit test purpose.
			if (isRunningTests && !Data.Any())
			{
				Data.TryAdd("ConfigFromRepoForTest", "ConfigValueFromRepoForTest");
				return;
			}
#endif
			using (var repo = StagingRepositoryFactory.GetStagingRepository())
			{
				if (!repo.DatabaseExists)
				{
					return;
				}
				var callingExePath = Assembly.GetEntryAssembly()?.Location;
				var configFilePath = Path.GetFileNameWithoutExtension(callingExePath) + ".config.json";
				var configAttributes = repo.Get<RefApplicationAttribute>().Where(x => x.RAA_ConfigFilePath == configFilePath).ToArray();
				Array.ForEach(configAttributes, attr =>
				{
					var configValue = attr.RAA_Value;
					if (attr.RAA_Content != null && attr.RAA_RAT_NKType.Equals(FileAttributeType, StringComparison.OrdinalIgnoreCase))
					{
						var fullFileName = Path.Combine(Path.GetDirectoryName(callingExePath), configValue);
						var attrSysStartTime = repo.GetTemporalStartAndEndTime(attr)?.sysStartTime;
						if (!File.Exists(fullFileName) || File.GetLastWriteTime(fullFileName) < attrSysStartTime)
						{
							File.WriteAllBytes(fullFileName, attr.RAA_Content);
							File.SetLastWriteTime(fullFileName, attrSysStartTime.Value);
						}
					}
					if (attr.RAA_Content != null && attr.RAA_RAT_NKType.Equals(SecretFileAttributeType, StringComparison.OrdinalIgnoreCase))
					{
						var fullFileName = Path.Combine(Path.GetDirectoryName(callingExePath), configValue);
						var attrSysStartTime = repo.GetTemporalStartAndEndTime(attr)?.sysStartTime;
						if (!File.Exists(fullFileName) || File.GetLastWriteTime(fullFileName) < attrSysStartTime)
						{
							File.WriteAllBytes(fullFileName, RefDbRepoCrypto.DecryptAES(attr.RAA_Content));
							File.SetLastWriteTime(fullFileName, attrSysStartTime.Value);
						}
					}
					if (attr.RAA_RAT_NKType.Equals(CredentialAttributeType, StringComparison.OrdinalIgnoreCase))
					{
						attr.RAA_Value = RefDbRepoCrypto.DecryptRSA(attr.RAA_Content);
					}
				});
				Data = configAttributes.ToDictionary(attr => attr.RAA_AttributeName, attr => attr.RAA_Value);
			}
		}

		const string FileAttributeType = "File";
		const string CredentialAttributeType = "Credential";
		const string SecretFileAttributeType = "SecretFile";

		IRefDbRepoCrypto RefDbRepoCrypto => refDbRepoCrypto.Value;
		readonly Lazy<IRefDbRepoCrypto> refDbRepoCrypto = new Lazy<IRefDbRepoCrypto>(() => new RefDbRepoCrypto(ApplicationConfig.CipherPublicKeyName, ApplicationConfig.CipherPrivateKeyName, ApplicationConfig.AesKeyName));

#if DEBUG
		static bool isRunningTests => UnitTestDetector.IsRunningTests.Value;
#endif
	}
}
