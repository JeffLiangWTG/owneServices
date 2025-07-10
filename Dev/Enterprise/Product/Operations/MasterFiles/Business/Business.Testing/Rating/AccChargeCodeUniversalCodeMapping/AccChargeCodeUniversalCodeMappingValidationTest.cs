using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Moq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.MasterFiles.Business.Rating.Testing
{
	sealed class AccChargeCodeUniversalCodeMappingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAUP_Code_Universal_CheckEntered()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("GLB");
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();

				mapping.AUP_Type = "UCC";
				mapping.AUP_Code = string.Empty;
				mapping.Validation.ValidateAUP_Code();

				AssertHasErrors("Universal Charge Code can not be empty", mapping.AUP_CodeInfo);
				AssertHasError(mapping.AUP_CodeInfo, "Please enter a Code.");

				mapping.AUP_Code = "BAF";
				mapping.Validation.ValidateAUP_Code();

				AssertNoErrors("Universal Charge Code can not be empty - no errors", mapping.AUP_CodeInfo);
			}
		}

		public void TestValidateAUP_Code_Universal_DuplicateMapping_AddError()
		{
			using (MockUniversalChargeCodeList())
			{
				CreateChargeCode("CHC", false, Env.CurrentCompanyPK, "BAF"); //Local Charge
				Factory.Save();

				CreateChargeCode("CHC", universalCode: "BAF"); //Global Charge
				Factory.Save();

				var newlocal = CreateChargeCode("CHC2", false, Env.CurrentCompanyPK);
				var newLocalMapping = newlocal.UniversalChargeCodeMappingsCollection.AddNew();
				newLocalMapping.AUP_Type = "UCC";
				newLocalMapping.AUP_Code = "BAF";
				newLocalMapping.Validation.ValidateAUP_Code();
				AssertHasErrors("Same universal charge code and same company so should give errors", newLocalMapping.AUP_CodeInfo);
				AssertHasError(newLocalMapping.AUP_CodeInfo, "\"BAF\" cannot be selected as it has been mapped as the \"Universal Charge Code\" for Charge Code \"CHC\".");

				var newGlobal = CreateChargeCode("CHC3");
				var newGlobalMapping = newGlobal.UniversalChargeCodeMappingsCollection.AddNew();
				newGlobalMapping.AUP_Code = "BAF";
				newGlobalMapping.Validation.ValidateAUP_Code();
				AssertHasErrors("Same universal charge code for global charge should give errors", newGlobalMapping.AUP_CodeInfo);
				AssertHasError(newGlobalMapping.AUP_CodeInfo, "\"BAF\" cannot be selected as it has been mapped as the \"Universal Charge Code\" for Charge Code \"CHC\".");
			}
		}

		public void TestValidateAUP_Code_Universal_UniqueMapping_PassesValidation()
		{
			using (MockUniversalChargeCodeList())
			{
				CreateChargeCode("CHC", false, Env.CurrentCompanyPK, "BAF"); //Local Charge
				Factory.Save();

				CreateChargeCode("CHC", universalCode: "BAF"); //Global Charge
				Factory.Save();

				var newLocal = CreateChargeCode("CHC2", false, ZGuid.NewZGuid());
				var newLocalMapping = newLocal.UniversalChargeCodeMappingsCollection.AddNew();
				newLocalMapping.AUP_Type = "UCC";
				newLocalMapping.AUP_Code = "BAF";
				newLocalMapping.Validation.ValidateAUP_Code();
				AssertNoErrors("Same universal charge code but different company - no errors", newLocalMapping.AUP_CodeInfo);

				newLocal.AC_GC = Env.CurrentCompanyPK;
				newLocalMapping.AUP_Code = "CAF";
				newLocalMapping.Validation.ValidateAUP_Code();
				AssertNoErrors("Different universal charge codes for same company - no errors", newLocalMapping.AUP_CodeInfo);

				var newGlobal = CreateChargeCode("CHC2");
				var newGlobalMapping = newGlobal.UniversalChargeCodeMappingsCollection.AddNew();
				newGlobalMapping.AUP_Code = "CAF";
				newGlobalMapping.Validation.ValidateAUP_Code();
				AssertNoErrors("Different universal charge codes for global charge - no errors", newGlobalMapping.AUP_CodeInfo);
			}
		}

		public void TestValidateAUP_Code_Universal_ValidChargeCode()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "UCC";
				mapping.AUP_Code = "XXX";
				mapping.Validation.ValidateAUP_Code();

				AssertHasErrors("Universal charge code is not in list provided by Rates Service, so should give errors", mapping.AUP_CodeInfo);
				AssertHasError(mapping.AUP_CodeInfo, "Universal Charge Code is not valid.");

				mapping.AUP_Code = "BAF";
				mapping.Validation.ValidateAUP_Code();
				AssertNoErrors("Universal charge code is in the list provided by Rates Service - no errors", mapping.AUP_CodeInfo);
			}
		}

		public void TestValidateAUP_Code_Carrier_CheckEntered()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("GLB");
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();

				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = string.Empty;
				mapping.Validation.ValidateAUP_Code();

				AssertHasErrors("Carrier Charge Code can not be empty", mapping.AUP_CodeInfo);
				AssertHasError(mapping.AUP_CodeInfo, "Please enter a Code.");

				mapping.AUP_Code = "BAFTEST";
				mapping.Validation.ValidateAUP_Code();

				AssertNoErrors("Carrier Charge Code can not be empty - no errors", mapping.AUP_CodeInfo);
			}
		}

		public void TestValidateAUP_Code_Carrier_DuplicateMapping_AddError()
		{
			using (MockUniversalChargeCodeList())
			{
				CreateChargeCode("CHC", false, Env.CurrentCompanyPK, "BAFTEST", "CAR"); //Local Charge
				Factory.Save();

				CreateChargeCode("CHC", universalCode: "BAFTEST", aupType: "CAR"); //Global Charge
				Factory.Save();

				var newlocal = CreateChargeCode("CHC2", false, Env.CurrentCompanyPK);
				var newLocalMapping = newlocal.UniversalChargeCodeMappingsCollection.AddNew();
				newLocalMapping.AUP_Type = "CAR";
				newLocalMapping.AUP_Code = "BAFTEST";
				newLocalMapping.Validation.ValidateAUP_Code();
				AssertHasErrors("Same Carrier charge code and same company so should give errors", newLocalMapping.AUP_CodeInfo);
				AssertHasError(newLocalMapping.AUP_CodeInfo, "\"BAFTEST\" cannot be selected as it has been mapped as the \"Carrier Charge Code\" for Charge Code \"CHC\".");

				var newGlobal = CreateChargeCode("CHC3");
				var newGlobalMapping = newGlobal.UniversalChargeCodeMappingsCollection.AddNew();
				newGlobalMapping.AUP_Type = "CAR";
				newGlobalMapping.AUP_Code = "BAFTEST";
				newGlobalMapping.Validation.ValidateAUP_Code();
				AssertHasErrors("Same universal charge code for global charge should give errors", newGlobalMapping.AUP_CodeInfo);
				AssertHasError(newGlobalMapping.AUP_CodeInfo, "\"BAFTEST\" cannot be selected as it has been mapped as the \"Carrier Charge Code\" for Charge Code \"CHC\".");
			}
		}

		public void TestValidateAUP_Code_Carrier_UniqueMapping_PassesValidation()
		{
			using (MockUniversalChargeCodeList())
			{
				CreateChargeCode("CHC", false, Env.CurrentCompanyPK, "BAFTEST"); //Local Charge
				Factory.Save();

				CreateChargeCode("CHC", universalCode: "BAFTEST"); //Global Charge
				Factory.Save();

				var newLocal = CreateChargeCode("CHC2", false, ZGuid.NewZGuid());
				var newLocalMapping = newLocal.UniversalChargeCodeMappingsCollection.AddNew();
				newLocalMapping.AUP_Type = "CAR";
				newLocalMapping.AUP_Code = "BAFTEST";
				newLocalMapping.Validation.ValidateAUP_Code();
				AssertNoErrors("Same Carrier charge code but comparing Local to Global - no errors", newLocalMapping.AUP_CodeInfo);

				newLocal.AC_GC = Env.CurrentCompanyPK;
				newLocalMapping.AUP_Code = "CAFTEST";
				newLocalMapping.Validation.ValidateAUP_Code();
				AssertNoErrors("Different Carrier charge codes for same company - no errors", newLocalMapping.AUP_CodeInfo);

				var newGlobal = CreateChargeCode("CHC2");
				var newGlobalMapping = newGlobal.UniversalChargeCodeMappingsCollection.AddNew();
				newGlobalMapping.AUP_Type = "CAR";
				newGlobalMapping.AUP_Code = "CAFTEST";
				newGlobalMapping.Validation.ValidateAUP_Code();
				AssertNoErrors("Different Carrier charge codes for global charge - no errors", newGlobalMapping.AUP_CodeInfo);
			}
		}

		public void TestValidateAUP_Code_Carrier_ValidChargeCode()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = "XXX";
				mapping.Validation.ValidateAUP_Code();

				AssertHasErrors("Carrier charge code is not in list provided by Rates Service, so should give errors", mapping.AUP_CodeInfo);
				AssertHasError(mapping.AUP_CodeInfo, "Carrier Charge Code is not valid.");

				mapping.AUP_Code = "BAFTEST";
				mapping.Validation.ValidateAUP_Code();
				AssertNoErrors("Carrier charge code is in the list provided by Rates Service - no errors", mapping.AUP_CodeInfo);
			}
		}

		public void TestChargeCodeMapping_CarrierValidation_TypeUCC_FilledCarrier()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "UCC";
				mapping.AUP_Code = "BAF";
				mapping.AUP_OH_Carrier = orgHeader.PK;

				mapping.Validation.ValidateAUP_OH_Carrier();

				AssertHasErrors("Invalid AUP Type, so should give errors", mapping.AUP_OH_CarrierInfo);
				AssertHasError(mapping.AUP_OH_CarrierInfo, "Carrier must be empty for mapping of type UCC.");
			}
		}

		public void TestChargeCodeMapping_CarrierValidation_TypeCAR_EmptyCarrier()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = "BAFTEST";

				mapping.Validation.ValidateAUP_OH_Carrier();

				AssertHasErrors("Invalid carrier, so should give errors", mapping.AUP_OH_CarrierInfo);
				AssertHasError(mapping.AUP_OH_CarrierInfo, "No Universal Charge Code mapping has been set up for charge code \"BAFTEST\" for carrier \"\". Please raise an eRequest if a mapping must be added to Rates Service.");
			}
		}

		public void TestChargeCodeMapping_CarrierValidation_TypeCAR_InvalidCarrier()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = "BAFTEST";
				mapping.AUP_OH_Carrier = orgHeader.PK;

				mapping.Validation.ValidateAUP_OH_Carrier();

				AssertHasErrors("Invalid carrier, so should give errors", mapping.AUP_OH_CarrierInfo);
				AssertHasError(mapping.AUP_OH_CarrierInfo, $"Organization \"{orgHeader.OH_Code}\" cannot be selected as it is not linked to a shipping line connected to the Carrier Charge Code \"{mapping.AUP_Code}\".");
			}
		}

		public void TestChargeCodeMapping_CarrierValidation_TypeCAR_ValidCarrier()
		{
			using (MockUniversalChargeCodeList())
			{
				var orgHeader = CreateValidCarrier("AAAA"); // Mapped in the MockUniversalChargeCodeList
				var charge = CreateChargeCode("CHC");
				Factory.Save();

				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = "BAFTEST";
				mapping.AUP_TransportMode = "SEA";
				mapping.AUP_OH_Carrier = orgHeader.PK;

				mapping.Validation.ValidateAUP_OH_Carrier();

				AssertNoErrors("Carrier provided is linked to the Shipping Line and the Carrier Code - no errors", mapping.AUP_OH_CarrierInfo);
			}
		}

		public void TestChargeCodeMapping_CarrierValidation_TypeCAR_ValidEmptyCarrier()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				Factory.Save();

				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = "HATTEST";
				mapping.AUP_TransportMode = "SEA";
				mapping.AUP_OH_Carrier = ZGuid.Empty;

				mapping.Validation.ValidateAUP_OH_Carrier();

				AssertNoErrors("Empty carrier is valid for a Carrier Charge Code mapped to an empty carrier - no errors", mapping.AUP_OH_CarrierInfo);
			}
		}

		public void TestChargeCodeMapping_TransportModeValidation_TypeUCC_FilledTransportMode()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "UCC";
				mapping.AUP_Code = "BAF";
				mapping.AUP_TransportMode = "SEA";

				mapping.Validation.ValidateAUP_TransportMode();

				AssertHasErrors("Invalid AUP Type, so should give errors", mapping.AUP_TransportModeInfo);
				AssertHasError(mapping.AUP_TransportModeInfo, "The transport mode must be empty for type UCC");
			}
		}

		public void TestChargeCodeMapping_TransportModeValidation_TypeCAR_InvalidTransportMode()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = "BAFTEST";
				mapping.AUP_TransportMode = ZString.Empty;

				mapping.Validation.ValidateAUP_TransportMode();

				AssertHasErrors("Invalid Transport mode, so should give errors", mapping.AUP_TransportModeInfo);
				AssertHasError(mapping.AUP_TransportModeInfo, "Only AIR and SEA transport modes are supported");
			}
		}

		public void TestChargeCodeMapping_TransportModeValidation_TypeCAR_ValidTransportMode()
		{
			using (MockUniversalChargeCodeList())
			{
				var charge = CreateChargeCode("CHC");
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = "BAFTEST";
				mapping.AUP_TransportMode = "SEA";

				mapping.Validation.ValidateAUP_TransportMode();

				AssertNoErrors("Transport mode provided is valid - no errors", mapping.AUP_TransportModeInfo);
			}
		}

		IDisposable MockUniversalChargeCodeList()
		{
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.GetAllChargeCodesWithMappingInfo(It.IsAny<string>()))
				.Returns(new[]
				{
					new ChargeCodeWithMappingInfo { Code = "BAF", Description = "BAF Desc", ForeignCode = "BAFTEST", Carrier = "AAAA", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "DSS", Description = "DSS Desc", ForeignCode = "DSSTEST", Carrier = "AAAA", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "FRT", Description = "FRT Desc", ForeignCode = "FRTTEST", Carrier = "AAAA", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "CAF", Description = "CAF Desc", ForeignCode = "CAFTEST", Carrier = "AAAA", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "HAT", Description = "HAT Desc", ForeignCode = "HATTEST", Carrier = "", Provider = WRConstants.RateProviders.CargoSphere },
				});

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, null));

			return ObjectFactory.Substitute(wiseRatesClientFactoryMock.Object);
		}

		AccChargeCode CreateChargeCode(ZString code, bool isGlobal = true, ZGuid? companyPK = null, ZString universalCode = default, string aupType = "UCC")
		{
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			if (isGlobal)
			{
				charge.AC_GC = ZGuid.Empty;
			}
			else
			{
				charge.AC_GC = companyPK ?? ZGuid.Empty;
			}

			charge.AC_Code = code;
			charge.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			charge.AC_Desc = code + " Desc";

			if (!universalCode.IsEmpty)
			{
				var mapping = charge.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Code = universalCode;
				mapping.AUP_Type = aupType;

				if (aupType.Equals("CAR"))
				{
					mapping.AUP_TransportMode = "SEA";
				}
			}

			return charge;
		}

		OrgHeader CreateValidCarrier(string scac)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var refShippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			orgHeader.OH_IsShippingLine = true;
			orgHeader.OH_RSL_ShippingLine = refShippingLine.PK;
			refShippingLine.RSL_StandardCarrierAlphaCode = scac;

			return orgHeader;
		}
	}
}
