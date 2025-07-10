using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Test.Consol
{
	public class ForwardingConsolAutoratingPreferenceTest : TestCaseWithFactory
	{
		#region Autorating preferences (overrides)

		public void TestAutoratingDate()
		{
			var consol = Factory.New<ForwardingConsol>();
			Assert(consol.AutoratingDate.IsEmpty);

			var testDate1 = ZDate.Today;
			consol.AutoratingPreferences.JRP_AutoratingDate = testDate1;
			AssertEquals(testDate1, consol.AutoratingDate);

			var testDate2 = testDate1.AddDays(1);
			consol.AutoratingDate = testDate2;
			AssertEquals(testDate2, consol.AutoratingPreferences.JRP_AutoratingDate);
		}

		public void TestAutoratingDateWhenChanged_ShouldMarkConsolAsDirty()
		{
			var consol = Factory.New<ForwardingConsol>();
			Assert("Precondition: when a consol has just been created for test, it should not be dirty", !consol.HasChanges);

			consol.AutoratingDate = new ZDate(2024, 3, 15);
			Assert("When AutoratingDate changed, the consol should be dirty", consol.HasChanges);
		}

		public void TestAutoratingPreferencesReadOnly_WithSecurityEnabled()
		{
			AssertAutoratingPreferencesReadOnly_WithSecurity(true, false, false);
		}

		public void TestAutoratingPreferencesReadOnly_WithSecurityDisabled()
		{
			AssertAutoratingPreferencesReadOnly_WithSecurity(false, true, true);
		}

		void AssertAutoratingPreferencesReadOnly_WithSecurity(bool isAllowed, bool expectedRatesDateOverrideReadOnly, bool expectedRatesIsDateOverriddenReadOnly)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.AutoratingPreferences.JRP_IsDateOverridden = true;
			consol.AutoratingPreferences.JRP_AutoratingDate = ZDateTime.Today;

			Env.Security.MaintainConsolTariffsAndRatesOverrideRateDate.IsAllowed = isAllowed;
			AssertEquals("AutoratingDate ReadOnly", expectedRatesDateOverrideReadOnly, consol.AutoratingDateInfo.ReadOnly);
		}

		public void TestDeleteJobRatingPreferences_WhenConsolIsDeleted()
		{
			var consol = Factory.New<ForwardingConsol>();
			var preferences = consol.AutoratingPreferences;

			consol.Delete();
			Assert(preferences.IsDeleted);
		}

		[TestDate(2023, 4, 4)]
		public void TestLoadAutoratingPreferences()
		{
			var consol = Factory.New<ForwardingConsol>();
			var preferences = consol.AutoratingPreferences;
			preferences.JRP_IsDateOverridden = true;
			preferences.JRP_AutoratingDate = ZDate.Today;

			AssertEquals(GlbCompany.CurrentCompany.PK, preferences.JRP_GC_Company);
			Factory.Save();

			var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var factory2 = Factory.CreateNewFactory();
			var anotherCompany = factory2.LoadTop1<GlbCompany>(query);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
				var preferences2 = consol2.AutoratingPreferences;

				AssertNotEquals("Should load different preferences for the new login company", preferences.PK, preferences2.PK);
				AssertEquals(anotherCompany.PK, preferences2.JRP_GC_Company);
				AssertEquals(ZDate.Empty, preferences2.JRP_AutoratingDate);
				Assert(!preferences2.JRP_IsDateOverridden);
				factory2.Save();
			}

			var factory3 = Factory.CreateNewFactory();
			var consol3 = factory3.Load<ForwardingConsol>(consol.PK);
			var preferences3 = consol3.AutoratingPreferences;

			AssertEquals(preferences.PK, preferences3.PK);
			AssertEquals(GlbCompany.CurrentCompany.PK, preferences3.JRP_GC_Company);
			AssertEquals(ZDate.Today, preferences3.JRP_AutoratingDate);
			Assert(preferences3.JRP_IsDateOverridden);
		}

		public void TestAutoratingPreferences_ShouldBeSynchronized_WhenRegistryIsOn()
		{
			using (RatingDataRegistry.Instance.AllowConsolAutoratingDateSynchronizedAcrossCompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				var preferences = consol.AutoratingPreferences;
				var date = new ZDateTime(2024, 3, 15);
				preferences.JRP_AutoratingDate = date;

				AssertEquals(ZGuid.Empty, preferences.JRP_GC_Company);
				Factory.Save();

				var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
				var factory2 = Factory.CreateNewFactory();
				var anotherCompany = factory2.LoadTop1<GlbCompany>(query);
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
					var preferences2 = consol2.AutoratingPreferences;

					AssertEquals(ZGuid.Empty, preferences2.JRP_GC_Company);
					AssertEquals(date, preferences2.JRP_AutoratingDate);
				}
			}
		}

		public void TestAutoratingPreferences_ShouldBeUnSynchronized_WhenRegistryIsOff()
		{
			using (RatingDataRegistry.Instance.AllowConsolAutoratingDateSynchronizedAcrossCompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				var preferences = consol.AutoratingPreferences;
				var date = new ZDateTime(2024, 3, 15);
				preferences.JRP_AutoratingDate = date;

				AssertEquals(Env.CurrentCompanyPK, preferences.JRP_GC_Company);
				Factory.Save();

				var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
				var factory2 = Factory.CreateNewFactory();
				var anotherCompany = factory2.LoadTop1<GlbCompany>(query);
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
					var preferences2 = consol2.AutoratingPreferences;

					AssertNotEquals(ZGuid.Empty, preferences2.JRP_GC_Company);
					AssertEquals(ZDateTime.Empty, preferences2.JRP_AutoratingDate);
				}
			}
		}

		public void TestAutoratingPreferences_ShouldReturnFallbackDate_WhenRegistryTurnedOn()
		{
			var consol = Factory.New<ForwardingConsol>();
			var preferences = consol.AutoratingPreferences;
			var date = new ZDateTime(2024, 3, 15);
			preferences.JRP_AutoratingDate = date;

			AssertEquals(Env.CurrentCompanyPK, preferences.JRP_GC_Company);
			Factory.Save();

			using (RatingDataRegistry.Instance.AllowConsolAutoratingDateSynchronizedAcrossCompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory2 = Factory.CreateNewFactory();
				var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
				var preferences2 = consol2.AutoratingPreferences;

				AssertNotEquals(ZGuid.Empty, preferences2.JRP_GC_Company);
				AssertEquals(date, preferences2.JRP_AutoratingDate);

				// Test if this preference is "promoted" to shared
				var newDate = new ZDate(2024, 3, 20);
				consol2.AutoratingDate = newDate;
				AssertEquals(ZGuid.Empty, preferences2.JRP_GC_Company);
			}
		}

		public void TestAutoratingPreferences_ShouldReturnFallbackDate_WhenRegistryTurnedOff()
		{
			using (RatingDataRegistry.Instance.AllowConsolAutoratingDateSynchronizedAcrossCompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				var preferences = consol.AutoratingPreferences;
				var date = new ZDateTime(2024, 3, 15);
				preferences.JRP_AutoratingDate = date;

				AssertEquals(ZGuid.Empty, preferences.JRP_GC_Company);
				Factory.Save();

				using (RatingDataRegistry.Instance.AllowConsolAutoratingDateSynchronizedAcrossCompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var factory2 = Factory.CreateNewFactory();
					var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
					var preferences2 = consol2.AutoratingPreferences;

					AssertNotEquals(ZGuid.Empty, preferences2.JRP_GC_Company);
					AssertEquals(date, preferences2.JRP_AutoratingDate);
				}
			}
		}

		#endregion
	}
}
