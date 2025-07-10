using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(DefaultFilerContactInformationRegistryItem))]
	sealed class DefaultFilerContactInformationRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<DefaultFilerContactInformation>
	{
		protected override StronglyTypedRegistryItem<DefaultFilerContactInformation, DefaultFilerContactInformation> GetNewRegistryItem()
		{
			return new DefaultFilerContactInformationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		protected override DefaultFilerContactInformation ValidValue
		{
			get
			{
				var filer = new DefaultFilerContactInformation();
				filer.ContactPhone = Env.CurrentUser.WorkPhone;
				filer.ContactName = Env.CurrentUser.FullName;
				return filer;
			}
		}
	}
}
