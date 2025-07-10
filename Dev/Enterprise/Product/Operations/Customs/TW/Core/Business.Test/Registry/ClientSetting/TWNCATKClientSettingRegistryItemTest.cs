using System;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWNCATKClientSettingRegistryItem))]
	sealed class TWNCATKClientSettingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<TWNCATKClientSetting>
	{
		protected override StronglyTypedRegistryItem<TWNCATKClientSetting, TWNCATKClientSetting> GetNewRegistryItem()
		{
			return new TWNCATKClientSettingRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		[ExpectNoExceptions]
		public void TestOnAllValuesSavedAction()
		{
			var expectedUserName = "WTLDTWMXE_TCA";
			var interchangeXmlTemplate = @"
<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TWCustomsNCATK"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""WTLMXE"">
		<Group Type=""Company"" Reference=""DTW"">
			<Credential Name=""Current"">
				<UserName>{expectedUserName}</UserName>
				<Password />
			</Credential>
		</Group>
	</Group>
</Configuration>
";
			var registryItem = TWCustomsDataRegistry.Instance.TWNCATKClientSetting;
			registryItem.Options &= RegistryOptions.NotCached;
			var factory = registryItem.Factory;
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DTW";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "MXE";
			factory.Save();
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
			var setting = new TWNCATKClientSetting(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), factory);
			setting.MachineName = "Machine1";
			setting.SendToFolder = @"D:\Folders\SendFolder";
			setting.RunningIntervalInSeconds = 120;
			registryItem.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
			registryItem.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
			registryItem.OnAllValuesSaved();
			NUnit.Framework.Assert.That(setting.EHubClientStatus, NUnit.Framework.Is.EqualTo("OK").Using(CustomComparers.TypeComparison), "Registry status should have been set to OK");
			var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub")
			{ OrderBy = EDIInterchangeSchema.EI_InterchangeNum.Name + " DESC" };
			var interchange = factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertEqualsIgnoreLineBreaksAndIndent("Message text", interchangeXmlTemplate.Replace("{expectedUserName}", expectedUserName), interchange.EI_BodyText);
			interchange.Delete();
			setting = registryItem.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			setting.MachineName = ZString.Empty;
			setting.SendToFolder = ZString.Empty;
			registryItem.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
			registryItem.OnAllValuesSaved();
			NUnit.Framework.Assert.That(setting.EHubClientStatus, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Registry status should have been cleared");
			interchange = factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertEqualsIgnoreLineBreaksAndIndent("Message text", interchangeXmlTemplate.Replace("<UserName>{expectedUserName}</UserName>", ""), interchange.EI_BodyText);
			interchange.Delete();
			((IRegistryItemInternals)registryItem).DeleteValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			registryItem.OnAllValuesSaved();
			var previousInterchangPk = interchange.PK;
			interchange = factory.LoadTop1<EDIInterchange>(interchangeQuery);
			NUnit.Framework.Assert.That(interchange.PK, NUnit.Framework.Is.Not.EqualTo(previousInterchangPk), "Should have created another interchange when deleting");
			AssertEqualsIgnoreLineBreaksAndIndent("Message text", interchangeXmlTemplate.Replace("<UserName>{expectedUserName}</UserName>", ""), interchange.EI_BodyText);
		}

		[ExpectNoExceptions]
		void AssertEqualsIgnoreLineBreaksAndIndent(string message, string expected, string actuall)
		{
			var ignoreRegex = @"[\r\n]{1,2}\s+";
			NUnit.Framework.Assert.That(Regex.Replace(actuall, ignoreRegex, ""), CustomConstraints.MultilineASCIIEquals(Regex.Replace(expected, ignoreRegex, "")), message);
		}
	}
}
