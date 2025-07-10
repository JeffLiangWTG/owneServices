using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CertificateTypeRegistryItem))]
	sealed class CertificateTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<CertificateTypeCollection>
	{
		public void TestTranslatable()
		{
			var registryItem = new CertificateTypeRegistryItem("", null, null, null, RegistryStorageFlags.System, new CertificateTypeCollection());
			var value = registryItem.Value;
			var certType = value.AddNew();
			certType.DescriptionMultilingual = (NoResString)"Type 1";
			certType.Code = "ABC";
			certType.AlertType = AlertTypeList.Codes.NoAlert;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				AssertType(typeof(ResourceString), registryItem.Value[0].DescriptionMultilingual);
				var key = ((ResourceString)registryItem.Value[0].DescriptionMultilingual).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "类型1"));
				AssertEquals("类型1", registryItem.Value[0].DescriptionMultilingual);
			}
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<CertificateTypeCollection, CertificateTypeCollection> GetNewRegistryItem()
		{
			return new CertificateTypeRegistryItem("", null, null, null, RegistryStorageFlags.System, new CertificateTypeCollection());
		}

		#endregion
	}
}
