using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(LocalCountryCustomsInterfaceRegistryItem))]
	sealed class LocalCountryCustomsInterfaceRegistryItemTest : StronglyTypedRegistryItemTestCase<LocalCountryCustomsInterface>
	{
		protected override StronglyTypedRegistryItem<LocalCountryCustomsInterface, LocalCountryCustomsInterface> GetNewRegistryItem()
		{
			return new LocalCountryCustomsInterfaceRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		protected override LocalCountryCustomsInterface ValidValue
		{
			get
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				customsInterface.InterfaceType = LocalCountryCustomsInterfaceTypeCodeList.Codes.WiseTechCustoms;
				return customsInterface;
			}
		}
	}
}
