using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.EUTaxIDDefaultingRegistryItem;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EUTaxIDDefaultingRegistryItem))]
	sealed class EUTaxIDDefaultingRegistryItemTest : StronglyTypedRegistryItemTestCase<EUTaxIDDefaultingRuleCollection>
	{
		protected override StronglyTypedRegistryItem<EUTaxIDDefaultingRuleCollection, EUTaxIDDefaultingRuleCollection> GetNewRegistryItem()
		{
			return new EUTaxIDDefaultingRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}

		[ExpectNoExceptions]
		public void TestGetDefaultValue_NoCrossThreadAccessException()
		{
			ExceptionReporterTestListener.Instance.Clear();
			for (int i = 0; i < 10; i++)
			{
				var th = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						registryItemImpl.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty);
					}
				});
				th.Start();
				th.Join();
			}
			AssertEquals("Expect no exception", 0, ExceptionReporterTestListener.Instance.Count);
		}

		readonly EUTaxIDDefaultingRegistryItemImpl registryItemImpl = new EUTaxIDDefaultingRegistryItemImpl(string.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
	}
}
