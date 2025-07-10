using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanySignatureCredentialCollection))]
	sealed class GlbCompanySignatureCredentialCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIsEditAllowed()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				var collection = (GlbCompanySignatureCredentialCollection)GetCollectionToTest();
				AssertEquals(nameof(collection.IsEditAllowed), false, collection.IsEditAllowed);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var collection = (GlbCompanySignatureCredentialCollection)GetCollectionToTest();
				collection.Master.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals(nameof(collection.IsEditAllowed), false, collection.IsEditAllowed);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var collection = (GlbCompanySignatureCredentialCollection)GetCollectionToTest();
				AssertEquals(nameof(collection.IsEditAllowed), true, collection.IsEditAllowed);
			}
		}

		public void TestAllowNew()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				var collection = (GlbCompanySignatureCredentialCollection)GetCollectionToTest();
				AssertEquals(nameof(collection.IsLoaded), false, collection.IsLoaded);
				AssertEquals(nameof(collection.AllowNew), false, collection.AllowNew);

				collection.Load();
				AssertEquals(nameof(collection.AllowNew), false, collection.AllowNew);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var collection = (GlbCompanySignatureCredentialCollection)GetCollectionToTest();
				collection.Master.SetCountry(Core.Constants.CountryCodes.Portugal);
				AssertEquals(nameof(collection.IsLoaded), false, collection.IsLoaded);
				AssertEquals(nameof(collection.AllowNew), false, collection.AllowNew);

				collection.Load();
				AssertEquals(nameof(collection.AllowNew), false, collection.AllowNew);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var collection = (GlbCompanySignatureCredentialCollection)GetCollectionToTest();
				AssertEquals(nameof(collection.IsLoaded), false, collection.IsLoaded);
				AssertEquals(nameof(collection.AllowNew), false, collection.AllowNew);

				collection.Load();
				AssertEquals(nameof(collection.AllowNew), true, collection.AllowNew);

				collection.AddNew();
				AssertEquals(nameof(collection.AllowNew), false, collection.AllowNew);
			}
		}

		public void TestIsEditAllowedForCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				AssertEquals("GlbCompanySignatureCredentialCollection.IsEditAllowedForCompany", false, GlbCompanySignatureCredentialCollection.IsEditAllowedForCompany(GlbCompany.CurrentCompany));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				AssertEquals("GlbCompanySignatureCredentialCollection.IsEditAllowedForCompany", false, GlbCompanySignatureCredentialCollection.IsEditAllowedForCompany(GlbCompany.CurrentCompany));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				AssertEquals("GlbCompanySignatureCredentialCollection.IsEditAllowedForCompany", true, GlbCompanySignatureCredentialCollection.IsEditAllowedForCompany(GlbCompany.CurrentCompany));
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry(Core.Constants.CountryCodes.Turkey);

			return new GlbCompanySignatureCredentialCollection(company);
		}
	}
}
