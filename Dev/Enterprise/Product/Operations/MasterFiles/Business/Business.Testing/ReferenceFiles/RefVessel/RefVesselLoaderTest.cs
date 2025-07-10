using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefVessel.Loader))]
	class RefVesselLoaderTest : LoaderTestCase
	{
		public void TestLoadUnique_Name()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique("VESSEL", ZString.Empty, ZString.Empty, ZString.Empty).PK);
		}

		public void TestLoadUnique_LloydsNumber()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, "9832343", ZString.Empty, ZString.Empty).PK);
		}

		public void TestLoadUnique_RadioCallSign()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, ZString.Empty, "CALLME", ZString.Empty).PK);
		}

		public void TestLoadUnique_ConveyanceCountry()
		{
			CreateVessel();
			Factory.Save();

			AssertNull(new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Australia));
		}

		public void TestLoadUnique_Name_LloydsNumber()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique("VESSEL", "9832343", ZString.Empty, ZString.Empty).PK);
		}

		public void TestLoadUnique_Name_RadioCallSign()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique("VESSEL", ZString.Empty, "CALLME", ZString.Empty).PK);
		}

		public void TestLoadUnique_Name_ConveyanceCountry()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique("VESSEL", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Australia).PK);
		}

		public void TestLoadUnique_LloydsNumber_RadioCallSign()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, "9832343", "CALLME", ZString.Empty).PK);
		}

		public void TestLoadUnique_LloydsNumber_ConveyanceCountry()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, "9832343", ZString.Empty, Core.Constants.CountryCodes.Australia).PK);
		}

		public void TestLoadUnique_RadioCallSign_ConveyanceCountry()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, ZString.Empty, "CALLME", Core.Constants.CountryCodes.Australia).PK);
		}

		public void TestLoadUnique_Name_LloydsNumber_RadioCallSign()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique("VESSEL", "9832343", "CALLME", ZString.Empty).PK);
		}

		public void TestLoadUnique_Name_LloydsNumber_ConveyanceCountry()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique("VESSEL", "9832343", ZString.Empty, Core.Constants.CountryCodes.Australia).PK);
		}

		public void TestLoadUnique_Name_RadioCallSign_ConveyanceCountry()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique("VESSEL", ZString.Empty, "CALLME", Core.Constants.CountryCodes.Australia).PK);
		}

		public void TestLoadUnique_LloydsNumber_RadioCallSign_ConveyanceCountry()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, "9832343", "CALLME", Core.Constants.CountryCodes.Australia).PK);
		}

		public void TestLoadUnique_Name_LloydsNumber_RadioCallSign_ConveyanceCountry()
		{
			var vessel = CreateVessel();
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique("VESSEL", "9832343", "CALLME", Core.Constants.CountryCodes.Australia).PK);
		}

		public void TestLoadUnique_OnlyInactiveOne()
		{
			var vessel = CreateVessel();
			vessel.RV_IsActive = ZBool.False;
			Factory.Save();

			AssertEquals(vessel.PK, new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, "9832343", "CALLME", Core.Constants.CountryCodes.Australia).PK);
		}

		public void TestLoadUnique_NullIfMultiple()
		{
			CreateVessel();
			var vessel2 = CreateVessel();
			vessel2.RV_Name = "VESSEL2";
			var vessel3 = CreateVessel();
			vessel3.RV_Name = "VESSEL3";
			Factory.Save();

			AssertNull(new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, "9832343", "CALLME", Core.Constants.CountryCodes.Australia));
		}

		public void TestLoadUnique_OneActiveAndOthersInactive()
		{
			CreateVessel().RV_IsActive = ZBool.False;
			var vessel2 = CreateVessel();
			vessel2.RV_Name = "VESSEL2";
			var vessel3 = CreateVessel();
			vessel3.RV_Name = "VESSEL3";
			vessel3.RV_IsActive = ZBool.False;
			Factory.Save();

			AssertEquals(vessel2.PK, new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, "9832343", "CALLME", Core.Constants.CountryCodes.Australia).PK);
		}

		RefVessel CreateVessel()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			vessel.RV_LloydsNumber = "9832343";
			vessel.RV_RadioCallSign = "CALLME";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Australia;
			return vessel;
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new RefVessel.Loader(Factory);
	}
}
