using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationTWTest : SupplyChainSecurityConfigurationTest
	{
		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationTW();
		}

		public override void TestUsesGenericScheme()
		{
			AssertEquals(false, SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public void TestSupplyChainSecurityConfigurationIsEnabledForTaiwan()
		{
			// Act
			var actualIsEnabled = SupplyChainSecurityConfiguration.IsEnabled;

			// Assert
			AssertEquals(true, actualIsEnabled);
		}

		public override void TestApprovalCodesList()
		{
			// Act
			var actualApprovalCodesList = SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes();

			// Assert
			AssertContainsExactElementsInAnyOrder(new[] { "RA", "KC", "NO" }, actualApprovalCodesList);
		}

		public override void TestApprovalCodesWithRequiredDocumentValidation()
		{
			var testCases = new[]
			{
				new
				{
					ApprovalCode = "NO",
					ExpectedApprovalCodeHasRequiredDocumentValidation = false,
				},
				new
				{
					ApprovalCode = "KC",
					ExpectedApprovalCodeHasRequiredDocumentValidation = false,
				},
				new
				{
					ApprovalCode = "RA",
					ExpectedApprovalCodeHasRequiredDocumentValidation = false,
				}
			};

			AssertContainsExactElementsInAnyOrder("Prerequisite - Missing Test Cases",
				SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes(),
				testCases.Select(testCase => testCase.ApprovalCode));

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					// Act
					var actualApprovalCodeHasRequiredDocumentValidation =
						SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(
							testCase.ApprovalCode);

					// Assert
					AssertEquals(
						GetAssertionMessageForApprovalCode(
							nameof(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation),
							testCase.ApprovalCode),
						testCase.ExpectedApprovalCodeHasRequiredDocumentValidation,
						actualApprovalCodeHasRequiredDocumentValidation);
				}
			});
		}

		public override void TestApprovalNumberFormat()
		{
			var testCases = new[]
			{
				new
				{
					ApprovalCode = "NO",
					ExpectedApprovalCodeApprovalNumberFormat = string.Empty,
					ExpectedApprovalCodeErrorForApprovalNumberFormatNotMet = string.Empty
				},
				new
				{
					ApprovalCode = "KC",
					ExpectedApprovalCodeApprovalNumberFormat = @"^\d{5}$",
					ExpectedApprovalCodeErrorForApprovalNumberFormatNotMet = "The Approval Number must be in the following format: “NNNNN”, for example 00069."
				},
				new
				{
					ApprovalCode = "RA",
					ExpectedApprovalCodeApprovalNumberFormat = @"^\d{5}$",
					ExpectedApprovalCodeErrorForApprovalNumberFormatNotMet = "The Approval Number must be in the following format: “NNNNN”, for example 00069."
				}
			};

			AssertContainsExactElementsInAnyOrder("Prerequisite - Missing Test Cases",
				SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes(),
				testCases.Select(testCase => testCase.ApprovalCode));

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					// Act
					var actualApprovalCodeApprovalNumberFormat =
						SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat(testCase.ApprovalCode);
					var actualApprovalCodeErrorForApprovalNumberFormatNotMet =
						SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet(
							testCase.ApprovalCode);

					// Assert
					AssertEquals(GetAssertionMessageForApprovalCode(nameof(SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat), testCase.ApprovalCode),
						testCase.ExpectedApprovalCodeApprovalNumberFormat,
						actualApprovalCodeApprovalNumberFormat);
					AssertEquals(GetAssertionMessageForApprovalCode(nameof(SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet), testCase.ApprovalCode),
						testCase.ExpectedApprovalCodeErrorForApprovalNumberFormatNotMet,
						actualApprovalCodeErrorForApprovalNumberFormatNotMet);
				}
			});
		}

		public override void TestIsApprovedToShipOnPassengerFlights()
		{
			var testCases = new[]
			{
				new
				{
					ApprovalCode = "NO",
					ExpectedApprovalCodeIsApprovedToShipOnPassengerFlights = false
				},
				new
				{
					ApprovalCode = "KC",
					ExpectedApprovalCodeIsApprovedToShipOnPassengerFlights = true
				},
				new
				{
					ApprovalCode = "RA",
					ExpectedApprovalCodeIsApprovedToShipOnPassengerFlights = true
				}
			};

			AssertContainsExactElementsInAnyOrder("Prerequisite - Missing Test Cases",
				SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes(),
				testCases.Select(testCase => testCase.ApprovalCode));

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					// Act
					var actualApprovalCodeIsApprovedToShipOnPassengerFlights =
						SupplyChainSecurityConfiguration.ApprovalCodeIsApprovedToShipOnPassengerFlights(
							testCase.ApprovalCode);

					// Assert
					AssertEquals(GetAssertionMessageForApprovalCode(nameof(SupplyChainSecurityConfiguration.ApprovalCodeIsApprovedToShipOnPassengerFlights), testCase.ApprovalCode),
						testCase.ExpectedApprovalCodeIsApprovedToShipOnPassengerFlights,
						actualApprovalCodeIsApprovedToShipOnPassengerFlights);
				}
			});
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			var testCases = new[]
			{
				new
				{
					ApprovalCode = "NO",
					ExpectedApprovalCodeIsValidForAviationSecurityApproval = false,
				},
				new
				{
					ApprovalCode = "KC",
					ExpectedApprovalCodeIsValidForAviationSecurityApproval = true,
				},
				new
				{
					ApprovalCode = "RA",
					ExpectedApprovalCodeIsValidForAviationSecurityApproval = true,
				}
			};

			AssertContainsExactElementsInAnyOrder("Prerequisite - Missing Test Cases",
				SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes(),
				testCases.Select(testCase => testCase.ApprovalCode));

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					// Act
					var actualApprovalCodeIsValidForAviationSecurityApproval =
						SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(
							testCase.ApprovalCode);

					// Assert
					AssertEquals(GetAssertionMessageForApprovalCode(nameof(SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval), testCase.ApprovalCode),
						testCase.ExpectedApprovalCodeIsValidForAviationSecurityApproval,
						actualApprovalCodeIsValidForAviationSecurityApproval);
				}
			});
		}

		public override void TestApprovalCodeExpiryDateValidation()
		{
			var testCases = new[]
			{
				new
				{
					ApprovalCode = "NO",
					ExpectedApprovalCodeRequiresExpiryDate = false,
					ExpectedApprovalCodeMaximumValidityInYears = 0,
				},
				new
				{
					ApprovalCode = "KC",
					ExpectedApprovalCodeRequiresExpiryDate = true,
					ExpectedApprovalCodeMaximumValidityInYears = 2,
				},
				new
				{
					ApprovalCode = "RA",
					ExpectedApprovalCodeRequiresExpiryDate = true,
					ExpectedApprovalCodeMaximumValidityInYears = 3,
				}
			};

			AssertContainsExactElementsInAnyOrder("Prerequisite - Missing Test Cases",
				SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes(),
				testCases.Select(testCase => testCase.ApprovalCode));

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					// Act
					var actualApprovalCodeRequiresExpiryDate =
						SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(testCase.ApprovalCode);
					var actualApprovalCodeMaximumValidityInYears =
						SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(testCase.ApprovalCode);

					// Assert
					AssertEquals(GetAssertionMessageForApprovalCode(nameof(SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate), testCase.ApprovalCode),
						testCase.ExpectedApprovalCodeRequiresExpiryDate,
						actualApprovalCodeRequiresExpiryDate);
					AssertEquals(GetAssertionMessageForApprovalCode(nameof(SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears), testCase.ApprovalCode),
						testCase.ExpectedApprovalCodeMaximumValidityInYears,
						actualApprovalCodeMaximumValidityInYears);
				}
			});
		}

		static string GetAssertionMessageForApprovalCode(string methodName, string approvalCode) =>
			$"{methodName} result is not expected for {approvalCode} approval code.";

		#endregion

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TW"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"TWTPE";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TW"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"TWTPE";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		#region Business Object Validations

		#region OV_EXApprovalNumber
		public void TestOV_EXApprovalNumberValidation()
		{
			var testCases = new[]
			{
				new
				{
					OV_EXApprovedOrMajorExporter = "RA",
					OV_EXApprovalNumber = "00111",
					ExpectedErrorMessage = (string)null
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "RA",
					OV_EXApprovalNumber = "RA123",
					ExpectedErrorMessage = "The Approval Number must be in the following format: “NNNNN”, for example 00069."
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "RA",
					OV_EXApprovalNumber = string.Empty,
					ExpectedErrorMessage = "Please enter an Approval Number."
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "KC",
					OV_EXApprovalNumber = "00222",
					ExpectedErrorMessage = (string)null
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "KC",
					OV_EXApprovalNumber = "RA321",
					ExpectedErrorMessage = "The Approval Number must be in the following format: “NNNNN”, for example 00069."
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "KC",
					OV_EXApprovalNumber = string.Empty,
					ExpectedErrorMessage = "Please enter an Approval Number."
				}
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TW"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var countryData = org.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = org.PK;

				CombineAssertions(() =>
				{
					foreach (var testCase in testCases)
					{
						// Arrange
						countryData.OV_EXApprovedOrMajorExporter = testCase.OV_EXApprovedOrMajorExporter;
						countryData.OV_EXApprovalNumber = testCase.OV_EXApprovalNumber;

						// Act
						var actualApprovalNumberInfo = countryData.OV_EXApprovalNumberInfo;

						// Assert
						if (testCase.ExpectedErrorMessage != null)
						{
							AssertHasError(actualApprovalNumberInfo, testCase.ExpectedErrorMessage);
						}
						else
						{
							AssertNoErrors(actualApprovalNumberInfo);
						}
					}
				});
			}
		}

		#endregion

		#region OV_EXApprovalExpiryDate

		[TestDate(2022, 1, 1)]
		public void TestOV_EXApprovalExpiryDateValidation()
		{
			var testCases = new[]
			{
				new
				{
					OV_EXApprovedOrMajorExporter = "RA",
					OV_EXApprovalExpiryDate = new ZDate(2025, 1, 1),
					ExpectedErrorMessage = (string)null
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "RA",
					OV_EXApprovalExpiryDate = new ZDate(2025, 1, 2),
					ExpectedErrorMessage = "The Expiry Date cannot be more than 3 years in the future."
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "RA",
					OV_EXApprovalExpiryDate = ZDate.Empty,
					ExpectedErrorMessage = "Please enter an Approval Expiry Date."
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "KC",
					OV_EXApprovalExpiryDate = new ZDate(2024, 1, 1),
					ExpectedErrorMessage = (string)null
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "KC",
					OV_EXApprovalExpiryDate = new ZDate(2024, 1, 2),
					ExpectedErrorMessage = "The Expiry Date cannot be more than 2 years in the future."
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "KC",
					OV_EXApprovalExpiryDate = ZDate.Empty,
					ExpectedErrorMessage = "Please enter an Approval Expiry Date."
				}
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TW"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var countryData = org.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = org.PK;

				CombineAssertions(() =>
				{
					foreach (var testCase in testCases)
					{
						// Arrange
						countryData.OV_EXApprovedOrMajorExporter = testCase.OV_EXApprovedOrMajorExporter;
						countryData.OV_EXApprovalExpiryDate = testCase.OV_EXApprovalExpiryDate;

						// Act
						var actualApprovalExpiryDateInfo = countryData.OV_EXApprovalExpiryDateInfo;

						// Assert
						if (testCase.ExpectedErrorMessage != null)
						{
							AssertHasError(actualApprovalExpiryDateInfo, testCase.ExpectedErrorMessage);
						}
						else
						{
							AssertNoErrors(actualApprovalExpiryDateInfo);
						}
					}
				});
			}
		}

		#endregion

		#region OV_OA_ApprovedLocationInfo

		public void TestOV_OA_ApprovedLocationInfo()
		{
			var testCases = new[]
			{
				new
				{
					OV_EXApprovedOrMajorExporter = "RA",
					IsApprovedLocationSelected = true,
					ExpectedErrorMessage = (string)null
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "RA",
					IsApprovedLocationSelected = false,
					ExpectedErrorMessage = "Please enter an Address."
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "KC",
					IsApprovedLocationSelected = true,
					ExpectedErrorMessage = (string)null
				},
				new
				{
					OV_EXApprovedOrMajorExporter = "KC",
					IsApprovedLocationSelected = false,
					ExpectedErrorMessage = "Please enter an Address."
				},
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TW"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var countryData = org.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = org.PK;

				CombineAssertions(() =>
				{
					foreach (var testCase in testCases)
					{
						// Arrange
						countryData.OV_EXApprovedOrMajorExporter = testCase.OV_EXApprovedOrMajorExporter;
						countryData.OV_OA_ApprovedLocation = testCase.IsApprovedLocationSelected
							? countryData.PK : ZGuid.Empty;

						// Act
						var actualApprovedLocationInfo = countryData.OV_OA_ApprovedLocationInfo;

						// Assert
						if (testCase.ExpectedErrorMessage != null)
						{
							AssertHasError(actualApprovedLocationInfo, testCase.ExpectedErrorMessage);
						}
						else
						{
							AssertNoErrors(actualApprovedLocationInfo);
						}
					}
				});
			}
		}

		#endregion

		#endregion
	}
}
