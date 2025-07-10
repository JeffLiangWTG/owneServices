using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(EntryFilerRegistryItem))]
	sealed class EntryFilerRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<EntryFiler>
	{
		public void TestRegistryOptions()
		{
			AssertEquals("Options", RegistryOptions.Default, new EntryFilerRegistryItem("DUMMY", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hello World").Options);
		}

		protected override StronglyTypedRegistryItem<EntryFiler, EntryFiler> GetNewRegistryItem()
		{
			return new EntryFilerRegistryItem("", null, null, null);
		}

		protected override EntryFiler ValidValue
		{
			get
			{
				EntryFiler filer = new EntryFiler();
				filer.companyPK = Env.CurrentCompany.PK;
				filer.EntryFilerCode = "SV1";
				filer.IsABICertified = true;
				return filer;
			}
		}
	}
}
