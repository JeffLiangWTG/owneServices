using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolDGRestrictionsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJKD_Calc_Substance_Length()
		{
			var consolDGRestrictions = Factory.NewWithValidTestData<ConsolDGRestrictions>();
			consolDGRestrictions.JKD_Calc_Substance = "THISISTOOLONG";
			consolDGRestrictions.Validation.ValidateAll();
			AssertHasErrors(consolDGRestrictions.JKD_Calc_SubstanceInfo);
		}

		public void TestValidateJKD_Calc_Substance_Invalid_Code()
		{
			var consolDGRestrictions = Factory.NewWithValidTestData<ConsolDGRestrictions>();
			consolDGRestrictions.JKD_Calc_Substance = "AAAAA";
			consolDGRestrictions.Validation.ValidateAll();
			AssertHasError("Invalid Substance", consolDGRestrictions.JKD_Calc_SubstanceInfo, "Enter a valid selection.");

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a").FirstOrDefault();
			AssertNotNull(substance);

			substance.DG_UNNO = "AAAA";
			AssertEquals("Precondition: DG_UNNO", "AAAA", substance.DG_UNNO);

			substance.DG_Variant = "";
			AssertEquals("Precondition: DG_Variant", "", substance.DG_Variant);

			consolDGRestrictions.JKD_Calc_Substance = "AAAA";
			consolDGRestrictions.Validation.ValidateAll();
			AssertNoErrors(consolDGRestrictions.JKD_Calc_SubstanceInfo);

			substance.DG_Variant = "A";
			AssertEquals("Precondition: DG_Variant", "A", substance.DG_Variant);

			consolDGRestrictions.Validation.ValidateAll();
			AssertHasError("Substance without variant", consolDGRestrictions.JKD_Calc_SubstanceInfo, "Enter a valid selection.");

			consolDGRestrictions.JKD_Calc_Substance = "AAAAA";
			consolDGRestrictions.Validation.ValidateAll();
			AssertNoErrors(consolDGRestrictions.JKD_Calc_SubstanceInfo);
		}

		public void TestComfirmNoForbidenDangerousGoodsSubstancesForPassengerFlight()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Class = "8";
			substance.DG_UNNO = "1855";
			substance.DG_Code = "1855X";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			substance.DG_UniqueRecordId = "1855X";
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport = consol.Transports.AddNew();
			transport.JW_IsCargoOnly = false;

			var consolDGRestrictions = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions.JKD_Calc_Substance = "1855X";
			consolDGRestrictions.Validation.ValidateAll();
			AssertHasError(consolDGRestrictions.JKD_Calc_SubstanceInfo, "The substance you have selected is forbidden for passenger flight. Please ensure all flights on this Consol are 'Is Cargo Only' flights.");

			consolDGRestrictions.JKD_Class = "8";
			consolDGRestrictions.Validation.ValidateAll();
			AssertHasError(consolDGRestrictions.JKD_Calc_SubstanceInfo, "The substance you have selected is forbidden for passenger flight. Please ensure all flights on this Consol are 'Is Cargo Only' flights.");
		}

		public void TestValidateJKD_Class_Invalid_Code()
		{
			var consolDGRestrictions = Factory.NewWithValidTestData<ConsolDGRestrictions>();
			consolDGRestrictions.JKD_Class = "AAAA";
			consolDGRestrictions.Validation.ValidateAll();
			AssertHasError("Invalid Class", consolDGRestrictions.JKD_ClassInfo, "Enter a valid selection.");

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "3";
			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			AssertNotNull(dgSubstance);

			consolDGRestrictions.JKD_Class = dgSubstance.DG_Class;
			consolDGRestrictions.Validation.ValidateAll();
			AssertNoErrors(consolDGRestrictions.JKD_ClassInfo);
		}

		public void TestValidateDGRestriction_Duplicate()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolDGRestrictions = consol.ConsolDGRestrictionCollection.AddNew();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "3";
			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			AssertNotNull(dgSubstance);

			consolDGRestrictions.JKD_Class = dgSubstance.DG_Class;
			consolDGRestrictions.Validation.ValidateAll();
			AssertNoErrors(consolDGRestrictions.JKD_ClassInfo);

			var consolDGRestrictions2 = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions2.JKD_Class = dgSubstance.DG_Class;
			consolDGRestrictions2.Validation.ValidateAll();
			AssertHasError(consolDGRestrictions2.JKD_ClassInfo, "Invalid Duplicate Restriction.");
		}

		public void TestValidateJDK_Class_Substance_Mismatch()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolDGRestrictions = consol.ConsolDGRestrictionCollection.AddNew();

			consolDGRestrictions.JKD_Calc_Substance = "AAAA";
			consolDGRestrictions.Validation.ValidateAll();
			AssertNoErrors("A substance is only checked that it is in the correct class if there is a class listed.", consolDGRestrictions.JKD_ClassInfo);

			consolDGRestrictions.JKD_Class = "1950";
			consolDGRestrictions.Validation.ValidateAll();
			AssertHasError(consolDGRestrictions.JKD_ClassInfo, "The substance AAAA does not belong to class 1950.");

			var consolDGRestrictionShouldWork = consol.ConsolDGRestrictionCollection.AddNew();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a").FirstOrDefault();
			AssertNotNull(dgSubstance);

			consolDGRestrictionShouldWork.JKD_Calc_Substance = dgSubstance.DG_Code;
			consolDGRestrictionShouldWork.JKD_Class = dgSubstance.DG_Class;
			AssertNoErrors(consolDGRestrictionShouldWork.JKD_ClassInfo);
		}

		public void TestJKD_Class_Empty_Rows()
		{
			var consolDGRestrictions = Factory.NewWithValidTestData<ConsolDGRestrictions>();
			consolDGRestrictions.Validation.ValidateAll();
			AssertHasError(consolDGRestrictions.JKD_ClassInfo, "Invalid Empty Row.");
		}
	}
}
