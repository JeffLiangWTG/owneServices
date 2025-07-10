using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Rating;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Test.Shipment;

public class ForwardingShipmentAutoratingPreferenceTest : TestCaseWithFactory
{
	[TestDate(2025, 1, 1)]
	public void TestRevenueAutoratingDate()
	{
		var shipment = Factory.New<ForwardingShipment>();
		Assert(shipment.RevenueAutoratingDate.IsEmpty);

		var testDate1 = ZDate.Today;
		shipment.AutoratingPreferences.JRP_AutoratingDate = testDate1;
		AssertEquals(testDate1, shipment.RevenueAutoratingDate);

		var testDate2 = testDate1.AddDays(1);
		shipment.RevenueAutoratingDate = testDate2;
		AssertEquals(testDate2, shipment.AutoratingPreferences.JRP_AutoratingDate);

		var preference = Factory.LoadTop1<JobRatingPreference>(new ZQuery());
		AssertEquals(testDate2, preference.JRP_AutoratingDate);
		AssertEquals(shipment.PK, preference.JRP_ParentID);
		AssertEquals(ZGuid.Empty, preference.JRP_GC_Company);
	}

	public void TestRevenueAutoratingDateWhenChange_ShouldMarkShipmentAsDirty()
	{
		var shipment = Factory.New<ForwardingShipment>();
		Assert("Precondition: when a shipment has just been created, it should not be dirty", !shipment.HasChanges);

		shipment.RevenueAutoratingDate = new ZDate(2024, 3, 15);
		Assert("When RevenueAutoratingDate changed, the shipment should be dirty", shipment.HasChanges);
	}

	public void TestRevenueAutoratingPreferencesReadOnly_WithSecurityEnabled() =>
		AssertRevenueAutoratingPreferencesReadOnly_WithSecurity(isAllowed: true, expectedRatesDateOverrideReadOnly: false);

	public void TestRevenueAutoratingPreferencesReadOnly_WithSecurityDisabled() =>
		AssertRevenueAutoratingPreferencesReadOnly_WithSecurity(isAllowed: false, expectedRatesDateOverrideReadOnly: true);

	void AssertRevenueAutoratingPreferencesReadOnly_WithSecurity(bool isAllowed, bool expectedRatesDateOverrideReadOnly)
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.AutoratingPreferences.JRP_AutoratingDate = ZDateTime.Today;

		Env.Security.MaintainShipmentTariffsAndRatesRevenueAutoratingDate.IsAllowed = isAllowed;
		AssertEquals("AutoratingDate ReadOnly", expectedRatesDateOverrideReadOnly, shipment.RevenueAutoratingDateInfo.ReadOnly);
	}

	public void TestDeleteJobRatingPreferences_WhenShipmentIsDeleted()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var preferences = shipment.AutoratingPreferences;

		shipment.Delete();
		Assert(preferences.IsDeleted);
	}
}
