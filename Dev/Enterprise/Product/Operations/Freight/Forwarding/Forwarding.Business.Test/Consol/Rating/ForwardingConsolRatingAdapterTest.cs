using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ContractManagement.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Business.Testing.RatingAdapterTestHelper;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolRatingAdapterTest : TestCaseWithFactory
	{
		#region Carrier Contract Numbers

		public void TestCarrierContractNumbers()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertContainsExactElementsInAnyOrder("new consol has no number and CONs has no item", Array.Empty<ZString>(), ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);

			var newNumber = consol.Numbers.AddNew();
			newNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			newNumber.CE_EntryNum = ZString.Empty;
			AssertContainsExactElementsInAnyOrder("new consol has no number and CONs has blank item", new[] { ZString.Empty }, ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);

			newNumber.CE_EntryNum = "SomethingElse";
			AssertContainsExactElementsInAnyOrder("new consol has no number and CONs has non-blank item", Array.Empty<ZString>(), ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);

			consol.Numbers.RemoveAndDeleteAll();
			consol.JK_CarrierContractNumber = "";
			AssertContainsExactElementsInAnyOrder("blank CarrierContractNumber and CONs has no item", Array.Empty<ZString>(), ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);

			newNumber = consol.Numbers.AddNew();
			newNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			newNumber.CE_EntryNum = ZString.Empty;
			AssertContainsExactElementsInAnyOrder("blank CarrierContractNumber and CONs has blank item", new[] { ZString.Empty }, ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);

			newNumber.CE_EntryNum = "SomethingElse";
			AssertContainsExactElementsInAnyOrder("blank CarrierContractNumber and CONs has non-blank item", Array.Empty<ZString>(), ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);

			consol.JK_CarrierContractNumber = "CON1";
			AssertContainsExactElementsInAnyOrder(new[] { "CON1" }, ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);

			consol.JK_CarrierContractNumber = "CON2";
			AssertContainsExactElementsInAnyOrder(new[] { "CON2" }, ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);

			newNumber.CE_EntryNum = ZString.Empty;
			AssertContainsExactElementsInAnyOrder(new[] { "CON2", string.Empty }, ((ForwardingConsolRatingAdapter)consol.RatingAdapter).CarrierContractNumbers);
		}

		public void TestCanUpdateCarrierContractNumber_UserNotInteractive_ShouldReturnTrueWithNoMessage()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var consol = Factory.New<ForwardingConsol>();

				// Consol number empty...
				//
				// New empty numbers has no confirmation
				var adapter = (ForwardingConsolRatingAdapter)consol.RatingAdapter;
				var checkResult = adapter.CanUpdateCarrierContractNumber(Array.Empty<string>());
				checkResult.CanUpdate.Should().BeTrue();
				checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().BeNull();

				// New blank number has no confirmation
				checkResult = adapter.CanUpdateCarrierContractNumber(new[] { "" });
				checkResult.CanUpdate.Should().BeTrue();
				checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().BeNull();

				// New number has no confirmation
				checkResult = adapter.CanUpdateCarrierContractNumber(new[] { "CON1" });
				checkResult.CanUpdate.Should().BeTrue();
				checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().BeNull();

				// Consol with number
				consol.JK_CarrierContractNumber = "AAA";
				adapter = (ForwardingConsolRatingAdapter)consol.RatingAdapter;
				// New empty numbers has no confirmation
				checkResult = adapter.CanUpdateCarrierContractNumber(Array.Empty<string>());
				checkResult.CanUpdate.Should().BeTrue();
				checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().BeNull();

				// New blank number has no confirmation
				checkResult = adapter.CanUpdateCarrierContractNumber(new[] { "" });
				checkResult.CanUpdate.Should().BeTrue();
				checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().BeNull();

				// New number has no confirmation
				checkResult = adapter.CanUpdateCarrierContractNumber(new[] { "XXX" });
				checkResult.CanUpdate.Should().BeTrue();
				checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().BeNull();

				// Same number has no confirmation
				checkResult = adapter.CanUpdateCarrierContractNumber(new[] { "AAA" });
				checkResult.CanUpdate.Should().BeTrue();
				checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().BeNull();

				// Multiple numbers has no confirmation
				checkResult = adapter.CanUpdateCarrierContractNumber(new[] { "BBB", "CCC" });
				checkResult.CanUpdate.Should().BeTrue();
				checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().BeNull();
			}
			Assert(true);
		}

		void TestCanUpdateCarrierContractNumber_UserInteractive(
			string message,
			string consolCarrierContractNumber,
			bool addBlankCON,
			IEnumerable<string> newNumbers,
			SingleCarrierContractNumberSelectionResult? singleNumberResultFromDialog,
			bool isManualCostSelected,
			CanUpdateCarrierContractNumberResult expectedResult)
		{
			var consol = Factory.New<ForwardingConsol>();
			var adapter = (ForwardingConsolRatingAdapter)consol.RatingAdapter;
			var mockDialogService = new Mock<IDialogService>();
			if (singleNumberResultFromDialog != null)
			{
				mockDialogService
					.Setup(x => x.SelectSingleCarrierContractNumber(newNumbers))
					.Returns(singleNumberResultFromDialog.Value);
			}

			consol.JK_CarrierContractNumber = consolCarrierContractNumber;
			if (addBlankCON && consol.JK_CarrierContractNumber.IsEmpty)
			{
				var blankCON = consol.Numbers.AddNew();
				blankCON.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
				blankCON.CE_EntryNum = ZString.Empty;
			}

			var checkResult = adapter.CanUpdateCarrierContractNumber(newNumbers, mockDialogService.Object, isManualCostSelected);
			checkResult.Should().BeEquivalentTo(expectedResult, message);

			if (singleNumberResultFromDialog != null)
			{
				mockDialogService.Verify(x => x.SelectSingleCarrierContractNumber(newNumbers), Times.Once, "Dialog should popup");
			}
			else
			{
				mockDialogService.Verify(x => x.SelectSingleCarrierContractNumber(newNumbers), Times.Never, "There should be no dialog");
			}
		}

		void TestCanUpdateCarrierContractNumber_UserInteractive(
			string message,
			string consolCarrierContractNumber,
			bool addBlankCON,
			IEnumerable<string> newNumbers,
			SingleCarrierContractNumberSelectionResult? singleNumberResultFromDialog,
			bool isManualCostSelected,
			bool expectedCanUpdate,
			string expectedConfirmMessage,
			UpdateCarrierContractNumberToken expectedUpdateToken)
		{
			var consol = Factory.New<ForwardingConsol>();
			var adapter = (ForwardingConsolRatingAdapter)consol.RatingAdapter;
			var mockDialogService = new Mock<IDialogService>();
			if (singleNumberResultFromDialog != null)
			{
				mockDialogService
					.Setup(x => x.SelectSingleCarrierContractNumber(newNumbers))
					.Returns(singleNumberResultFromDialog.Value);
			}

			consol.JK_CarrierContractNumber = consolCarrierContractNumber;
			if (addBlankCON && consol.JK_CarrierContractNumber.IsEmpty)
			{
				var blankCON = consol.Numbers.AddNew();
				blankCON.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
				blankCON.CE_EntryNum = ZString.Empty;
			}

			var checkResult = adapter.CanUpdateCarrierContractNumber(newNumbers, mockDialogService.Object, isManualCostSelected);
			// FluentAssertions does not exclude property for structs while we need to assert part of confirmation message
			// Assert individual props of the result instead.
			checkResult.CanUpdate.Should().Be(expectedCanUpdate, message);
			checkResult.ConfirmationMessageForOverridingJobContractNumber.Should().StartWith(expectedConfirmMessage, message);
			checkResult.Token.Should().BeEquivalentTo(expectedUpdateToken, message);

			if (singleNumberResultFromDialog != null)
			{
				mockDialogService.Verify(x => x.SelectSingleCarrierContractNumber(newNumbers), Times.Once, "Dialog should popup");
			}
			else
			{
				mockDialogService.Verify(x => x.SelectSingleCarrierContractNumber(newNumbers), Times.Never, "There should be no dialog");
			}
		}

		void TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(
			bool isIgnoreAndReplaceCarrierContractNumber,
			bool isPopulateClientContractNumbersIfBlank,
			bool isManualCostSelected)
		{
			var registryPopulateNumber = FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating;
			var registryIgnoreAndReplace = FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating;
			using (registryPopulateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isPopulateClientContractNumbersIfBlank))
			using (registryIgnoreAndReplace.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isIgnoreAndReplaceCarrierContractNumber))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				const string consolCarrierContractNumber = "";
				var shouldPopulateNumber = isPopulateClientContractNumbersIfBlank || isManualCostSelected;

				var sampleNumbers = Array.Empty<string>();
				var singleNumberResultFromDialog = (SingleCarrierContractNumberSelectionResult?)null;
				var expectedResult = new CanUpdateCarrierContractNumberResult(sampleNumbers);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New empty numbers - no dialog - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New empty numbers - no dialog - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: true,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				sampleNumbers = new[] { "" };
				singleNumberResultFromDialog = null;
				expectedResult = shouldPopulateNumber
					? new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult("")
						}
					}
					: default(CanUpdateCarrierContractNumberResult);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New single blank number - no dialog - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				singleNumberResultFromDialog = null;
				expectedResult = isManualCostSelected
					? new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult("")
						}
					}
					: default(CanUpdateCarrierContractNumberResult);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New single blank number - no dialog - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: true,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				sampleNumbers = new[] { "XXX" };
				singleNumberResultFromDialog = shouldPopulateNumber
					? new SingleCarrierContractNumberSelectionResult("XXX")
					: null;
				expectedResult = shouldPopulateNumber
					? new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult("XXX")
						}
					}
					: default(CanUpdateCarrierContractNumberResult);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New single non-blank number - show dialog when populating registry is on - choose the number - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: false, // only false. Why? Adding blank one will filter out rates and only ones with blank number left.
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				singleNumberResultFromDialog = shouldPopulateNumber
					? new SingleCarrierContractNumberSelectionResult
					{
						Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
					}
					: null;
				expectedResult = shouldPopulateNumber
					? new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult
							{
								Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
							}
						}
					}
					: default(CanUpdateCarrierContractNumberResult);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New single non-blank number - show dialog when populating registry is on - choose 'Not populating' - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: false, // only false because the case with true does not make sense
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				singleNumberResultFromDialog = shouldPopulateNumber
					? new SingleCarrierContractNumberSelectionResult
					{
						Result = ContractNumberSelectionResult.Cancelled
					}
					: null;
				expectedResult = default(CanUpdateCarrierContractNumberResult);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New single non-blank number - show dialog when populating registry is on - cancel - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: false, // only false because the case with true does not make sense
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				sampleNumbers = new[] { "", "XXX" };
				singleNumberResultFromDialog = shouldPopulateNumber
					? new SingleCarrierContractNumberSelectionResult("")
					: null;
				expectedResult = shouldPopulateNumber
					? new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult("")
						}
					}
					: default(CanUpdateCarrierContractNumberResult);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New multiple numbers - show dialog when populating registry is on - choose '' - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: false, // only false because the case with true does not make sense
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				singleNumberResultFromDialog = shouldPopulateNumber
					? new SingleCarrierContractNumberSelectionResult("XXX")
					: null;
				expectedResult = shouldPopulateNumber
					? new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult("XXX")
						}
					}
					: default(CanUpdateCarrierContractNumberResult);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New multiple numbers - show dialog when populating registry is on - choose non-blank number - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: false, // only false because the case with true does not make sense
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				singleNumberResultFromDialog = shouldPopulateNumber
					? new SingleCarrierContractNumberSelectionResult
					{
						Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
					}
					: null;
				expectedResult = shouldPopulateNumber
					? new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult
							{
								Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
							}
						}
					}
					: default(CanUpdateCarrierContractNumberResult);
				TestCanUpdateCarrierContractNumber_UserInteractive(
					FormatMessage("New multiple numbers - show dialog - choose 'Not populating' - no confirmation"),
					consolCarrierContractNumber,
					addBlankCON: false, // only false because the case with true does not make sense
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog,
					isManualCostSelected,
					expectedResult);

				string FormatMessage(string message)
				{
					return $@"{message}
GIVEN
sampleNumbers = {(sampleNumbers.IsNullOrEmpty() ? "Empty" : string.Join(",", sampleNumbers))}
WHEN
{nameof(isIgnoreAndReplaceCarrierContractNumber)} = {isIgnoreAndReplaceCarrierContractNumber}
{nameof(isPopulateClientContractNumbersIfBlank)} = {isPopulateClientContractNumbersIfBlank}
{nameof(isManualCostSelected)} = {isManualCostSelected}";
				}
			}

			Assert("This test uses FluentAssertions", true);
		}

		public void TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber()
		{
			// no difference in results so let's put the tests together
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(false, false, false);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(false, false, true);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(false, true, false);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(false, true, true);

			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(true, false, false);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(true, false, true);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(true, true, false);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithBlankNumber(true, true, true);
		}

		void TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber(
			bool isIgnoreAndReplaceCarrierContractNumber,
			bool isPopulateNumbersIfBlank,
			bool isManualCostSelected)
		{
			// PopulateForwardingConsolContractNumbersIfBlankDuringAutorating should not have any affect when carrier contract number is not blank
			// but let's test its different values to cover all cases
			using (FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isPopulateNumbersIfBlank))
			using (FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isIgnoreAndReplaceCarrierContractNumber))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				const string consolCarrierContractNumber = "AAA";

				var sampleNumbers = Array.Empty<string>();
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New empty numbers - no dialog - no confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: null,
					isManualCostSelected,
					expectedResult: new CanUpdateCarrierContractNumberResult(sampleNumbers));

				sampleNumbers = new[] { "" };
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New single blank number - show dialog - choose the number - has confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: new SingleCarrierContractNumberSelectionResult(""),
					isManualCostSelected,
					expectedCanUpdate: true,
					expectedConfirmMessage: "During the operation, Contract Number '' of the chosen rates will be populated",
					expectedUpdateToken: new UpdateCarrierContractNumberToken(sampleNumbers)
					{
						SelectionResult = new SingleCarrierContractNumberSelectionResult("")
					});

				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New single blank number - show dialog - choose 'Not populating' - no confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: new SingleCarrierContractNumberSelectionResult
					{
						Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
					},
					isManualCostSelected,
					expectedResult: new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult
							{
								Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
							}
						}
					});

				sampleNumbers = new[] { "XXX" };
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New single non-blank number - show dialog - choose the number - has confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: new SingleCarrierContractNumberSelectionResult("XXX"),
					isManualCostSelected,
					expectedCanUpdate: true,
					expectedConfirmMessage: "During the operation, Contract Number 'XXX' of the chosen rates will be populated",
					expectedUpdateToken: new UpdateCarrierContractNumberToken(sampleNumbers)
					{
						SelectionResult = new SingleCarrierContractNumberSelectionResult("XXX")
					});

				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New single non-blank number - show dialog - choose 'Not populating' - no confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: new SingleCarrierContractNumberSelectionResult
					{
						Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
					},
					isManualCostSelected,
					expectedResult: new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult
							{
								Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
							}
						}
					});

				sampleNumbers = new[] { "AAA" };
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"Same single number number - no dialog - no confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: null,
					isManualCostSelected,
					expectedResult: new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult("AAA")
						}
					});

				sampleNumbers = new[] { "", "AAA" };
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New multiple contract numbers - show dialog - choose cancel - no confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: new SingleCarrierContractNumberSelectionResult
					{
						Result = ContractNumberSelectionResult.Cancelled
					},
					isManualCostSelected,
					expectedResult: default(CanUpdateCarrierContractNumberResult));

				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New multiple contract numbers - show dialog - choose '' - has confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: new SingleCarrierContractNumberSelectionResult(""),
					isManualCostSelected,
					expectedCanUpdate: true,
					expectedConfirmMessage: "During the operation, Contract Number '' of the chosen rates will be populated",
					expectedUpdateToken: new UpdateCarrierContractNumberToken(sampleNumbers)
					{
						SelectionResult = new SingleCarrierContractNumberSelectionResult("")
					});

				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New multiple contract numbers - show dialog - choose non-blank - no confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: new SingleCarrierContractNumberSelectionResult("AAA"),
					isManualCostSelected,
					expectedResult: new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult("AAA")
						}
					});

				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New multiple contract numbers - show dialog - choose 'Not populating' - no confirmation",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: new SingleCarrierContractNumberSelectionResult
					{
						Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
					},
					isManualCostSelected,
					expectedResult: new CanUpdateCarrierContractNumberResult(sampleNumbers)
					{
						Token = new UpdateCarrierContractNumberToken(sampleNumbers)
						{
							SelectionResult = new SingleCarrierContractNumberSelectionResult
							{
								Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
							}
						}
					});
			}

			Assert("This test uses FluentAssertions", true);
		}

		public void TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber()
		{
			// no difference in results so let's put the tests together
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber(true, false, false);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber(true, false, true);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber(false, false, true);

			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber(true, true, false);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber(true, true, true);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber(false, true, true);
		}

		void TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber_IgnoreAndReplaceCarrierContractNumberIsNo_NoRateSelector(bool isPopulateNumbersIfBlank)
		{
			// PopulateForwardingConsolContractNumbersIfBlankDuringAutorating should not have any affect when carrier contract number is not blank
			// but let's test its different values to cover all cases
			using (FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isPopulateNumbersIfBlank))
			using (FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				// GIVEN
				// - IgnoreAndReplaceCarrierContractNumbersDuringAutorating = NO
				// - Consol has a non-blank CON
				// WHEN Checking for CanUpdateCarrierContractNumber
				// THEN should always honor consol's CON, do not ask for populating back from new contract numbers.
				const string consolCarrierContractNumber = "AAA";
				var expectedResult = new CanUpdateCarrierContractNumberResult { CanUpdate = false };

				var sampleNumbers = Array.Empty<string>();
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"No numbers",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: null,
					isManualCostSelected: false,
					expectedResult: new CanUpdateCarrierContractNumberResult(sampleNumbers));

				sampleNumbers = new[] { "" };
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New single blank number",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: null,
					isManualCostSelected: false,
					expectedResult);

				sampleNumbers = new[] { "XXX" };
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New single non-blank number",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: null,
					isManualCostSelected: false,
					expectedResult);

				sampleNumbers = new[] { "AAA" };
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"Same single number number",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: null,
					isManualCostSelected: false,
					expectedResult);

				sampleNumbers = new[] { "", "AAA" };
				TestCanUpdateCarrierContractNumber_UserInteractive(
					"New multiple contract numbers",
					consolCarrierContractNumber,
					addBlankCON: false,
					newNumbers: sampleNumbers,
					singleNumberResultFromDialog: null,
					isManualCostSelected: false,
					expectedResult);
			}

			Assert("This test uses FluentAssertions", true);
		}

		/// <summary>
		/// This test is separated because it's expected behaviour is different from the ones above.
		/// </summary>
		public void TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber_IgnoreAndReplaceCarrierContractNumberIsNo_NoRateSelector()
		{
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber_IgnoreAndReplaceCarrierContractNumberIsNo_NoRateSelector(isPopulateNumbersIfBlank: false);
			TestCanUpdateCarrierContractNumber_UserInteractive_ConsolWithNonBlankNumber_IgnoreAndReplaceCarrierContractNumberIsNo_NoRateSelector(isPopulateNumbersIfBlank: true);
		}

		public void TestUpdateCarrierContractNumber_UserInteractive()
		{
			var consol = Factory.New<ForwardingConsol>();
			var adapter = (ForwardingConsolRatingAdapter)consol.RatingAdapter;

			consol.JK_CarrierContractNumber = "";
			var updateToken = new UpdateCarrierContractNumberToken(Array.Empty<string>());
			AssertEquals("empty list applied to empty", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("empty list applied to empty", "", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "" });
			AssertEquals("empty applied to empty", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("empty applied to empty", "", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1" }) { SelectionResult = new SingleCarrierContractNumberSelectionResult("CON1") };
			AssertEquals("CON1 applied to empty", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON1 applied to empty", "CON1", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1" })
			{
				SelectionResult = new SingleCarrierContractNumberSelectionResult
				{
					Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
				}
			};
			AssertEquals("CON1 applied to empty - not populating", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON1 applied to empty - not populating", "", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON2" }) { SelectionResult = new SingleCarrierContractNumberSelectionResult("CON1") };
			AssertEquals("CON1, CON2 applied to empty", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON1, CON2 applied to empty", "CON1", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON2" })
			{
				SelectionResult = new SingleCarrierContractNumberSelectionResult
				{
					Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber
				}
			};
			AssertEquals("CON1, CON2 applied to empty - not populating", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON1, CON2 applied to empty - not populating", "", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "CON1";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "" });
			AssertEquals("empty applied to CON1", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("empty applied to CON1", "CON1", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "CON1";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "" }) { SelectionResult = new SingleCarrierContractNumberSelectionResult("") };
			AssertEquals("empty applied to CON1", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("empty applied to CON1", "", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "CON1";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1" }) { SelectionResult = new SingleCarrierContractNumberSelectionResult("CON1") };
			AssertEquals("CON1 applied to CON1", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON1 applied to CON1", "CON1", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "CON1";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON2" }) { SelectionResult = new SingleCarrierContractNumberSelectionResult("CON2") };
			AssertEquals("CON2 applied to CON1", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON2 applied to CON1", "CON2", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "CON3";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON2" })
			{
				SelectionResult = new SingleCarrierContractNumberSelectionResult("CON1")
			};
			AssertEquals("CON1, CON2 applied to CON3", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON1, CON2 applied to CON3", "CON1", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "CON3";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON2" })
			{
				SelectionResult = new SingleCarrierContractNumberSelectionResult { Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber }
			};
			AssertEquals("CON1, CON2 applied to CON3 - not populating", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON1, CON2 applied to CON3 - not populating", "CON3", consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "";
			updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON1" }) { SelectionResult = new SingleCarrierContractNumberSelectionResult("CON1") };
			AssertEquals("CON1, CON1 applied to empty", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals("CON1, CON1 applied to empty", "CON1", consol.JK_CarrierContractNumber);

			// Test Contract Numbers with length of more than 50

			const string sampleContractNumber50 = "12345678901234567890123456789012345678901234567890";
			const string sampleContractNumber51 = "123456789012345678901234567890123456789012345678901";
			const string sampleContractNumber52 = "1234567890123456789012345678901234567890123456789012";

			consol.JK_CarrierContractNumber = "CON1";
			updateToken = new UpdateCarrierContractNumberToken(new[] { sampleContractNumber51, sampleContractNumber51 }) { SelectionResult = new SingleCarrierContractNumberSelectionResult(sampleContractNumber51) };
			AssertEquals(
				"should be updated",
				DataUpdateResult.Updated,
				adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals(
				"truncated number with length of 50 should have been applied",
				sampleContractNumber50,
				consol.JK_CarrierContractNumber);

			consol.JK_CarrierContractNumber = "CON1";
			updateToken = new UpdateCarrierContractNumberToken(new[] { sampleContractNumber51, sampleContractNumber52 }) { SelectionResult = new SingleCarrierContractNumberSelectionResult(sampleContractNumber52) };
			AssertEquals(
				"should be updated",
				DataUpdateResult.Updated,
				adapter.UpdateCarrierContractNumber(updateToken));
			AssertEquals(
				"truncated number with length of 50 should have been applied",
				sampleContractNumber50,
				consol.JK_CarrierContractNumber);
		}

		public void TestUpdateCarrierContractNumber_UserNotInteractive_IgnoreAndReplaceCarrierContractNumber()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			using (FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();

				consol.JK_CarrierContractNumber = "XXX";
				var adapter = (ForwardingConsolRatingAdapter)consol.RatingAdapter;
				var updateToken = new UpdateCarrierContractNumberToken(Array.Empty<string>());
				AssertEquals("empty list applied", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("empty list applied", "XXX", consol.JK_CarrierContractNumber);

				updateToken = new UpdateCarrierContractNumberToken(new[] { "" });
				AssertEquals("empty applied", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("empty applied", "", consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "CON1";
				updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1" });
				AssertEquals("CON1 applied to CON1", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("CON1 applied to CON1", "CON1", consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "CON1";
				updateToken = new UpdateCarrierContractNumberToken(new[] { "CON2" });
				AssertEquals("CON2 applied to CON1", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("CON2 applied to CON1", "CON2", consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "";
				updateToken = new UpdateCarrierContractNumberToken(new[] { "CON2" });
				AssertEquals("CON2 applied to empty", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("CON2 applied to empty", "CON2", consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "";
				updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON2" });
				AssertEquals("CON1, CON2 applied to empty", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("CON1 applied", "", consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "CON3";
				updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON2" });
				AssertEquals("CON1, CON2 applied", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("CON1, CON2 applied", "CON3", consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "";
				updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON1" });
				AssertEquals("CON1, CON1 applied to empty", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("CON1, CON1 applied to empty", "CON1", consol.JK_CarrierContractNumber);

				// Test Contract Numbers with length of more than 50

				const string sampleContractNumber50 = "12345678901234567890123456789012345678901234567890";
				const string sampleContractNumber51 = "123456789012345678901234567890123456789012345678901";
				const string sampleContractNumber52 = "1234567890123456789012345678901234567890123456789012";

				consol.JK_CarrierContractNumber = "CON1";
				updateToken = new UpdateCarrierContractNumberToken(new[] { sampleContractNumber51, sampleContractNumber51 }) { SelectionResult = new SingleCarrierContractNumberSelectionResult(sampleContractNumber51) };
				AssertEquals(
					"should be updated",
					DataUpdateResult.Updated,
					adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals(
					"truncated number with length of 50 should have been applied",
					sampleContractNumber50,
					consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "CON1";
				updateToken = new UpdateCarrierContractNumberToken(new[] { sampleContractNumber51, sampleContractNumber52 });
				AssertEquals(
					"multiple numbers applied",
					DataUpdateResult.NoAction,
					adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals(
					"multiple numbers applied",
					"CON1",
					consol.JK_CarrierContractNumber);
			}
		}

		public void TestUpdateCarrierContractNumber_UserNotInteractive_AutoPopulateCarrierContractNumberWhenBlank()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			using (FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();

				consol.JK_CarrierContractNumber = "XXX";
				var adapter = (ForwardingConsolRatingAdapter)consol.RatingAdapter;
				var updateToken = new UpdateCarrierContractNumberToken(Array.Empty<string>());
				AssertEquals("empty list applied", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("empty list applied", "XXX", consol.JK_CarrierContractNumber);

				updateToken = new UpdateCarrierContractNumberToken(new[] { "" });
				AssertEquals("empty applied", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("empty applied", "XXX", consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "";
				updateToken = new UpdateCarrierContractNumberToken(new[] { "" });
				AssertEquals("empty applied to empty", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("empty applied to empty", "", consol.JK_CarrierContractNumber);

				updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1" });
				AssertEquals("CON1 applied to CON1", DataUpdateResult.Updated, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("CON1 applied to CON1", "CON1", consol.JK_CarrierContractNumber);

				consol.JK_CarrierContractNumber = "";
				updateToken = new UpdateCarrierContractNumberToken(new[] { "CON1", "CON2" });
				AssertEquals("CON1, CON2 applied to empty", DataUpdateResult.NoAction, adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals("CON1, CON2 applied to empty", "", consol.JK_CarrierContractNumber);

				// Test Contract Numbers with length of more than 50

				const string sampleContractNumber50 = "12345678901234567890123456789012345678901234567890";
				const string sampleContractNumber51 = "123456789012345678901234567890123456789012345678901";
				consol.JK_CarrierContractNumber = "";
				updateToken = new UpdateCarrierContractNumberToken(new[] { sampleContractNumber51, sampleContractNumber51 });
				AssertEquals(
					"truncated number with length of 50 should have been applied",
					DataUpdateResult.Updated,
					adapter.UpdateCarrierContractNumber(updateToken));
				AssertEquals(
					"truncated number with length of 50 should have been applied",
					sampleContractNumber50,
					consol.JK_CarrierContractNumber);
			}
		}

		#endregion

		public void TestIAutoRatingStandardFreightCost()
		{
			var consol = Factory.New<ForwardingConsol>();
			var standardFreightCost = consol.RatingAdapter as IAutoRatingStandardFreightCost;
			var host = standardFreightCost.GetHost() as ForwardingConsol;

			AssertNotNull(host);
			AssertEquals(consol, host);

			consol.JK_TransportMode = TransportModes.Sea;
			AssertEquals("Should be disabled for non-AIR", false, standardFreightCost.Enabled);

			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			AssertEquals("Should be enabled for AIR LSE", true, standardFreightCost.Enabled);

			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCodes.Australia);
			standardFreightCost.Set(new Money(10m, aud));
			AssertEquals(10m, consol.JK_ConsolChargeableRate);

			standardFreightCost.Set(new Money(15m, aud));
			AssertEquals(15m, consol.JK_ConsolChargeableRate);

			consol.JK_ConsolMode = ContainerModes.ULD;
			AssertEquals("Should be enabled for AIR ULD", true, standardFreightCost.Enabled);
			standardFreightCost.Set(new Money(20m, aud));
			AssertEquals(20m, consol.JK_ConsolChargeableRate);

			consol.JK_TransportMode = TransportModes.Sea;
			AssertEquals("Should be disabled for non-AIR", false, standardFreightCost.Enabled);
			standardFreightCost.Set(new Money(30m, aud));
			AssertEquals("Rate not changed as freight cost is disabled", 20m, consol.JK_ConsolChargeableRate);
		}

		public void TestAWBCurrencyIsLocalWhenExRateCannotBeFound()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
				auCountry.RN_RX_NKAirWaybillCurrency = "EUR";
				Factory.Save();

				var testConsol = Factory.New<ForwardingConsol>();
				testConsol.JK_TransportMode = TransportModes.Air;
				testConsol.JK_ConsolMode = ContainerModes.Loose;

				var cost1 = (BusinessObject)Factory.New<IJobConsolCost>();
				cost1[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
				cost1.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					cost1[JobConsolCostSchema.E6_ParentID] = testConsol.PK;
					cost1[JobConsolCostSchema.E6_ParentTableCode] = "JK";
				}
				finally
				{
					cost1.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
				cost1[JobConsolCostSchema.E6_AC_ChargeCode] = Factory.NewWithValidTestData<AccChargeCode>().PK;
				cost1[JobConsolCostSchema.E6_LocalCostAmount] = 100m;
				cost1[JobConsolCostSchema.E6_RX_NKCurrency] = "UGX";
				cost1[JobConsolCostSchema.E6_ExchangeRate] = 2m;

				var cost2 = (BusinessObject)Factory.New<IJobConsolCost>();
				cost2[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
				cost2.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					cost2[JobConsolCostSchema.E6_ParentID] = testConsol.PK;
					cost2[JobConsolCostSchema.E6_ParentTableCode] = "JK";
				}
				finally
				{
					cost2.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
				cost2[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
				cost2[JobConsolCostSchema.E6_LocalCostAmount] = 500m;
				cost2[JobConsolCostSchema.E6_RX_NKCurrency] = "USD";
				cost2[JobConsolCostSchema.E6_ExchangeRate] = 1.5m;

				var cost3 = (BusinessObject)Factory.New<IJobConsolCost>();
				cost3[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
				cost3.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					cost3[JobConsolCostSchema.E6_ParentID] = testConsol.PK;
					cost3[JobConsolCostSchema.E6_ParentTableCode] = "JK";
				}
				finally
				{
					cost3.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
				cost3[JobConsolCostSchema.E6_AC_ChargeCode] = Factory.NewWithValidTestData<AccChargeCode>().PK;
				cost3[JobConsolCostSchema.E6_LocalCostAmount] = 150m;
				cost3[JobConsolCostSchema.E6_RX_NKCurrency] = "USD";
				cost3[JobConsolCostSchema.E6_ExchangeRate] = 1.4m;

				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var exRate1 = voyage.ExRates.AddNew();
				exRate1.E8_RX_NKExCurrency = "UGX";
				exRate1.E8_VoyageExchangeRate = 2.5m;

				var exRate2 = voyage.ExRates.AddNew();
				exRate2.E8_RX_NKExCurrency = "USD";
				exRate2.E8_VoyageExchangeRate = 3.0m;

				voyage.JV_AirSeaRoad = TransportModes.Air;

				var org = voyage.Origins.AddNew();
				org.JA_RL_NKPortOfLoading = "AUSYD";
				var dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "SGSIN";

				testConsol.Transports[0].JW_JX = voyage.Sailings[0].PK;

				AssertEquals("AUD", testConsol.AWBCurrency.RX_Code);

				exRate2.E8_RX_NKExCurrency = "EUR";
				AssertEquals("EUR", testConsol.AWBCurrency.RX_Code);
			}
		}

		public void TestIAutoRatingStandardFreightCostSetterHandlesAWBCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
				auCountry.RN_RX_NKAirWaybillCurrency = "USD";
				Factory.Save();

				ForwardingConsol testConsol = Factory.New<ForwardingConsol>();
				testConsol.JK_TransportMode = TransportModes.Air;
				testConsol.JK_ConsolMode = ContainerModes.Loose;

				var cost1 = (BusinessObject)Factory.New<IJobConsolCost>();
				cost1[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
				cost1.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					cost1[JobConsolCostSchema.E6_ParentID] = testConsol.PK;
					cost1[JobConsolCostSchema.E6_ParentTableCode] = "JK";
				}
				finally
				{
					cost1.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
				cost1[JobConsolCostSchema.E6_AC_ChargeCode] = Factory.NewWithValidTestData<AccChargeCode>().PK;
				cost1[JobConsolCostSchema.E6_LocalCostAmount] = 100m;
				cost1[JobConsolCostSchema.E6_RX_NKCurrency] = "UGX";
				cost1[JobConsolCostSchema.E6_ExchangeRate] = 2m;

				var cost2 = (BusinessObject)Factory.New<IJobConsolCost>();
				cost2[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
				cost2.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					cost2[JobConsolCostSchema.E6_ParentID] = testConsol.PK;
					cost2[JobConsolCostSchema.E6_ParentTableCode] = "JK";
				}
				finally
				{
					cost2.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
				cost2[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
				cost2[JobConsolCostSchema.E6_LocalCostAmount] = 500m;
				cost2[JobConsolCostSchema.E6_RX_NKCurrency] = "USD";
				cost2[JobConsolCostSchema.E6_ExchangeRate] = 1.5m;

				var cost3 = (BusinessObject)Factory.New<IJobConsolCost>();
				cost3[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
				cost3.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					cost3[JobConsolCostSchema.E6_ParentID] = testConsol.PK;
					cost3[JobConsolCostSchema.E6_ParentTableCode] = "JK";
				}
				finally
				{
					cost3.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
				cost3[JobConsolCostSchema.E6_AC_ChargeCode] = Factory.NewWithValidTestData<AccChargeCode>().PK;
				cost3[JobConsolCostSchema.E6_LocalCostAmount] = 150m;
				cost3[JobConsolCostSchema.E6_RX_NKCurrency] = "USD";
				cost3[JobConsolCostSchema.E6_ExchangeRate] = 1.4m;

				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var exRate1 = voyage.ExRates.AddNew();
				exRate1.E8_RX_NKExCurrency = "UGX";
				exRate1.E8_VoyageExchangeRate = 2.5m;

				var exRate2 = voyage.ExRates.AddNew();
				exRate2.E8_RX_NKExCurrency = "USD";
				exRate2.E8_VoyageExchangeRate = 3.0m;

				voyage.JV_AirSeaRoad = TransportModes.Air;

				var org = voyage.Origins.AddNew();
				org.JA_RL_NKPortOfLoading = "AUSYD";
				var dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "SGSIN";

				testConsol.Transports[0].JW_JX = voyage.Sailings[0].PK;

				var standardFreightCost = testConsol.RatingAdapter as IAutoRatingStandardFreightCost;

				standardFreightCost.Set(new Money(1000m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD")));
				AssertEquals(1000m * 1.5m, testConsol.JK_ConsolChargeableRate);

				standardFreightCost.Set(new Money(1000m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")));
				AssertEquals(1000m, testConsol.JK_ConsolChargeableRate);

				standardFreightCost.Set(new Money(1000m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "UGX")));
				AssertEquals((1000m / 2.5m) * 1.5m, testConsol.JK_ConsolChargeableRate);

				standardFreightCost.Set(new Money(1000m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "UAH")));
				AssertEquals(1000m, testConsol.JK_ConsolChargeableRate);
			}
		}

		public void TestAutoRatingTransportProviders_GatewayConsolCosting()
		{
			var creditor = Factory.New<OrgHeader>();
			var carrier = Factory.New<OrgHeader>();
			var cto = Factory.New<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = cto.MainAddress.PK;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			Assert("Pre-condition", consol.IsGateway());

			var adapter = consol.RatingAdapter;
			AssertEquals("Creditor is not branch org proxy", 3, adapter.Creditors.AllOrgs.Count);

			consol.JK_OA_CreditorAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			AssertEquals("Creditor IS branch org proxy, and performing consol costing so ONLY creditor", 1, adapter.Creditors.AllOrgs.Count);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.PK, adapter.Creditors.AllOrgs[0].PK);

			consol.JK_AgentType = AgentType.Courier;

			AssertEquals("No longer gateway, so normal behaviour", 2, adapter.Creditors.AllOrgs.Count);
		}

		public void TestIGateway()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var consolRatingAdapter = new ForwardingConsolRatingAdapter(consolRatingRoute);

			AssertEquals(((IGateway)consolRatingAdapter.Parent).GatewayBillingSupporter, consolRatingAdapter.GatewayBillingSupporter);

			AssertContainsExactElementsInAnyOrder(new List<ZGuid>(), consolRatingAdapter.SortedGatewayAgentPKs);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>(), consolRatingAdapter.GatewayAgentPKsForIntercompanyTariff);
			AssertContainsExactElementsInAnyOrder(new List<LocationWithSource>(), consolRatingAdapter.SortedOverridenPlannedLoad);
			AssertContainsExactElementsInAnyOrder(new List<LocationWithSource>(), consolRatingAdapter.SortedOverridenPlannedDischarge);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>(), consolRatingAdapter.SortedControllingCustomerPKs);

			var billingType = new BillingType();
			var agentType = "test";
			var gatewayAgentPk = new ZGuid();

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, consolRatingAdapter.IsIntercompanyTariffApplicable(billingType, CostSell.Revenue));
			}
			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(false, consolRatingAdapter.IsIntercompanyTariffApplicable(billingType, CostSell.Revenue));

				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_RL_NKLoadPort = "GBDTE";
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
				port.O5_PortOrCountry = "GBDTE";
				port.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				port.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

				consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

				Factory.Save();

				AssertEquals(true, ((IGateway)consol).GatewayBillingSupporter.IsGatewayBillingEnabled());

				AssertEquals(false, consolRatingAdapter.IsIntercompanyTariffApplicable(billingType, CostSell.Cost));
				AssertEquals(true, consolRatingAdapter.IsIntercompanyTariffApplicable(billingType, CostSell.Revenue));

				AssertEquals(ForwardingConsolExtensions.ContinueAutorateCosting(consol, billingType), consolRatingAdapter.ContinueWithDefaultCosting(billingType));
				AssertEquals(ZString.Empty, consolRatingAdapter.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, billingType, CostSell.Cost));
				AssertEquals(ZString.Empty, consolRatingAdapter.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, billingType, CostSell.Revenue));
				AssertEquals(false, consolRatingAdapter.ShouldRemoveNonIntercompanyTariffFRTEntries(billingType));
			}

			Assert("should be true for consol adapter", consolRatingAdapter.IsContainerNegotiatedCostApplicable(CostSell.Cost));
			Assert("should be true for consol adapter", consolRatingAdapter.IsContainerNegotiatedCostApplicable(CostSell.Revenue));
			Assert("should be true for consol adapter", consolRatingAdapter.IsGatewaySellApplicableToGatewayConsol(CostSell.Cost));
			Assert("should be true for consol adapter", consolRatingAdapter.IsGatewaySellApplicableToGatewayConsol(CostSell.Revenue));
		}

		public void TestAircraftType()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var consolRatingAdapter = new ForwardingConsolRatingAdapter(consolRatingRoute);

			consol.JK_TransportMode = TransportModes.Sea;
			AssertEquals(ZString.Empty, consolRatingAdapter.AircraftType);

			consol.JK_TransportMode = TransportModes.Air;

			var transport1 = consol.Transports[0];
			transport1.JW_VoyageFlight = "QF111";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.JW_IsCargoOnly = true;
			transport1.JW_ETA = new ZDateTime(2020, 06, 02);
			transport1.JW_ETD = new ZDateTime(2020, 06, 03);
			transport1.JW_CarrierBookingReference = "A";
			transport1.JW_IsLinked = true;

			var transport2 = consol.Transports.AddNew("AUMEL", "CNSHA");
			transport2.JW_IsCargoOnly = true;
			transport2.JW_TransportMode = TransportModes.Air;
			transport2.JW_ETA = new ZDateTime(2020, 06, 03);
			transport2.JW_ETD = new ZDateTime(2020, 06, 04);
			transport2.JW_CarrierBookingReference = "A";

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(AircraftType.CAO, consolRatingAdapter.AircraftType);
				transport2.JW_IsCargoOnly = false;
				AssertEquals(AircraftType.PAX, consolRatingAdapter.AircraftType);
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Consol's overall status", false, consol.JK_Calc_IsCargoOnly);
				AssertEquals(AircraftType.PAX, consolRatingAdapter.AircraftType);

				transport2.JW_IsCargoOnly = true;
				AssertEquals("Consol's overall status", true, consol.JK_Calc_IsCargoOnly);
				AssertEquals(AircraftType.CAO, consolRatingAdapter.AircraftType);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldAddContractNumberQueryFilter()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "AAA";
			var adapter = consol.RatingAdapter;

			var registryKey = FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = adapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldAddContractNumberQueryFilter);

				configuration = adapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldAddContractNumberQueryFilter);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = adapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldAddContractNumberQueryFilter);

				configuration = adapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldAddContractNumberQueryFilter);
			}

			registryKey = FreightConfigurationRegistry.Instance.IgnoreAndReplaceClientContractNumbersDuringAutorating;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = adapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldAddContractNumberQueryFilter);

				configuration = adapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldAddContractNumberQueryFilter);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = adapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldAddContractNumberQueryFilter);

				configuration = adapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldAddContractNumberQueryFilter);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldApplySpecificAdapterContractNumberFilter()
		{
			var consol = Factory.New<ForwardingConsol>();
			var ratingAdapter = consol.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(configuration.ShouldApplySpecificAdapterContractNumberFilter);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			Assert(configuration.ShouldApplySpecificAdapterContractNumberFilter);
		}

		public void TestGetContractNumberConfiguration_ShouldIgnoreJobCarrierContractNumbers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var ratingAdapter = consol.RatingAdapter;

			var registryKey = FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldIgnoreJobCarrierContractNumbers);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldIgnoreJobCarrierContractNumbers);
			}

			registryKey = FreightConfigurationRegistry.Instance.IgnoreAndReplaceClientContractNumbersDuringAutorating;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldIgnoreJobClientContractNumbers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var ratingAdapter = consol.RatingAdapter;

			var registryKey = FreightConfigurationRegistry.Instance.IgnoreAndReplaceClientContractNumbersDuringAutorating;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobClientContractNumbers);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobClientContractNumbers);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldIgnoreJobClientContractNumbers);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldIgnoreJobClientContractNumbers);
			}

			registryKey = FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobClientContractNumbers);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobClientContractNumbers);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobClientContractNumbers);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobClientContractNumbers);
			}
		}

		public void TestForwardingRateSearchCreditorPriority_WhenColoadDomestic_WithMultiRouteRegistryIsNo_CarrierExportCreditorBeforeColoadWith()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var consolRatingAdapter = new ForwardingConsolRatingAdapter(consolRatingRoute);

			var coloader = Factory.NewWithValidTestData<OrgHeader>();
			var carrierExportCreditor = Factory.NewWithValidTestData<OrgHeader>();
			coloader.OH_IsCreditor = true;
			carrierExportCreditor.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = coloader.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = carrierExportCreditor.MainAddress.PK;
			Factory.Save();

			var creditors = consolRatingAdapter.Creditors;
			var carrierExportCreditorOrg = creditors.AllOrgsWithSource.FirstOrDefault(o => o.Org.PK == carrierExportCreditor.PK);
			var coloadOrg = creditors.AllOrgsWithSource.FirstOrDefault(o => o.Org.PK == coloader.PK);

			Assert("Carrier Export Creditor should be in the rate searching list", carrierExportCreditorOrg != null);
			Assert("Coloader should be in the rate searching list", coloadOrg != null);

			foreach (var code in creditors.ChargeCodeGroups)
			{
				Assert($"Carrier Export Creditor should be above Coloader in rate searching list for Charge Code: {code}"
					, creditors[code].GetPriorities(carrierExportCreditorOrg)[0] < creditors[code].GetPriorities(coloadOrg)[0]);
			}
		}

		public void TestForwardingRateSearchCreditorPriority_WhenColoadDomestic_WithMultiRouteRegistryIsYesAndSingleRoute_CarrierExportCreditorBeforeColoadWith()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "NZBAL";
			consol.JK_AgentType = AgentType.CoLoad;
			var routes = consol.Transports;
			var firstRoute = routes[0];

			var routeSetRatingRoute = new RouteSetRatingRoute(new RouteSet(Factory, 0, firstRoute), consol);
			var consolRatingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			var coloader = Factory.NewWithValidTestData<OrgHeader>();
			var carrierExportCreditor = Factory.NewWithValidTestData<OrgHeader>();
			coloader.OH_IsCreditor = true;
			carrierExportCreditor.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = coloader.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = carrierExportCreditor.MainAddress.PK;
			Factory.Save();

			var creditors = consolRatingAdapter.Creditors;
			var carrierExportCreditorOrg = creditors.AllOrgsWithSource.FirstOrDefault(o => o.Org.PK == carrierExportCreditor.PK);
			var coloadOrg = creditors.AllOrgsWithSource.FirstOrDefault(o => o.Org.PK == coloader.PK);

			Assert("Carrier Export Creditor should be in the rate searching list", carrierExportCreditorOrg != null);
			Assert("Coloader should be in the rate searching list", coloadOrg != null);

			foreach (var code in creditors.ChargeCodeGroups)
			{
				Assert($"Carrier Export Creditor should be above Coloader in rate searching list for Charge Code: {code}"
					, creditors[code].GetPriorities(carrierExportCreditorOrg)[0] < creditors[code].GetPriorities(coloadOrg)[0]);
			}
		}

		public void TestForwardingRateSearchCreditorPriority_WhenColoadDomestic_WithMultiRouteRegistryIsYesAndMultiRoutes_CarrierExportCreditorBeforeColoadWith()
		{
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "NZBAL";
				var routes = consol.Transports;
				var firstRoute = routes[0];
				firstRoute.JW_RL_NKLoadPort = "NZAKL";
				firstRoute.JW_RL_NKDiscPort = "NZAHU";
				var newRoute = routes.AddNew();
				newRoute.JW_RL_NKLoadPort = "NZAHU";
				newRoute.JW_RL_NKDiscPort = "NZBAL";
				newRoute.JW_CarrierBookingReference = "Route2";

				var routeSetRatingRoute = new RouteSetRatingRoute(new RouteSet(Factory, 2, firstRoute, newRoute), consol);
				var consolRatingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

				var coloader = Factory.NewWithValidTestData<OrgHeader>();
				var carrierExportCreditor = Factory.NewWithValidTestData<OrgHeader>();
				coloader.OH_IsCreditor = true;
				carrierExportCreditor.OH_IsCreditor = true;
				Factory.Save();

				consol.JK_OA_CreditorAddress = coloader.MainAddress.PK;
				consol.CarrierExportCreditorAddress.E2_OA_Address = carrierExportCreditor.MainAddress.PK;
				Factory.Save();

				var creditors = consolRatingAdapter.Creditors;
				var carrierExportCreditorOrg = creditors.AllOrgsWithSource.FirstOrDefault(o => o.Org.PK == carrierExportCreditor.PK);
				var coloadOrg = creditors.AllOrgsWithSource.FirstOrDefault(o => o.Org.PK == coloader.PK);

				Assert("Carrier Export Creditor should not be in the rate searching list", carrierExportCreditorOrg == null);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldMatchJobBlankContractNumberFlag()
		{
			var consol = Factory.New<ForwardingConsol>();
			var ratingAdapter = consol.RatingAdapter;

			var registryKey = FreightConfigurationRegistry.Instance.IgnoreAndReplaceClientContractNumbersDuringAutorating;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldMatchJobBlankContractNumber);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldMatchJobBlankContractNumber);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldMatchJobBlankContractNumber);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldMatchJobBlankContractNumber);
			}

			registryKey = FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldMatchJobBlankContractNumber);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldMatchJobBlankContractNumber);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldMatchJobBlankContractNumber);

				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldMatchJobBlankContractNumber);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldUseCarrierContractDateFilterFlag()
		{
			var consol = Factory.New<ForwardingConsol>();
			var ratingAdapter = consol.RatingAdapter;

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var registryKey = FreightConfigurationRegistry.Instance.EnableCarrierContractTariffsAndRates;
				using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
					Assert(!configuration.ShouldUseCarrierContractDateFilter);

					configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
					Assert(!configuration.ShouldUseCarrierContractDateFilter);
				}

				using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
					Assert(configuration.ShouldUseCarrierContractDateFilter);

					configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
					Assert(!configuration.ShouldUseCarrierContractDateFilter);
				}
			}
		}

		public void TestAllowFreightSpotRate_WhenAllocationAllowsFreightSpotRate()
		{
			var consol = Factory.New<ForwardingConsol>();
			var contractAllocationLine = Factory.New<RatingContractAllocationLine>();

			consol.JK_RCA_AllocationLine = contractAllocationLine.PK;
			var ratingAdapter = consol.RatingAdapter;

			AssertEquals(false, ratingAdapter.SkipFreightCharge);

			contractAllocationLine.RCA_AllowFreightSpotRate = true;

			AssertEquals(true, ratingAdapter.SkipFreightCharge);
		}

		#region Measures

		public void TestMeasures_ContainerCommodity_OverriddenByConsolCommodity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RH_NKConsolCommodity = "CHEM";

			var gp20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = gp20.PK;
			container.JC_ContainerCount = 5;
			container.JC_IsGrossWeightOverridden = true;
			container.JC_TareWeight = 2000;
			container.JC_DunnageWeight = 200;
			container.JC_GrossWeight = 1000;
			container.JC_RH_NKContainerCommodityCode = "SHIP";
			container.JC_RH_NKRatingCommodityCode = "COAL";

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			measures
				.GetPartList(MeasureType.ContainerCount)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "CHEM" }, because: "Consol Rating Commodity overrides all");
			measures
				.GetCommodities()
				.Should()
				.BeEquivalentTo(new[] { "CHEM" }, because: "Chem is the only commodity");
			Assert("Fluent assertion in use", true);
		}

		public void TestMeasures_ContainerCommodity_WithoutGrossWeightOverride_OverriddenByContainerRatingCommodity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var gp20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = gp20.PK;
			container.JC_ContainerCount = 5;
			container.JC_IsGrossWeightOverridden = true;
			container.JC_TareWeight = 2000;
			container.JC_DunnageWeight = 200;
			container.JC_GrossWeight = 1000;
			container.JC_RH_NKContainerCommodityCode = "SHIP";
			container.JC_RH_NKRatingCommodityCode = "COAL";

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			measures
				.GetPartList(MeasureType.Weight)
				.Should().BeNullOrEmpty();
			measures
				.GetPartList(MeasureType.Volume)
				.Should().BeNullOrEmpty();
			measures
				.GetPartList(MeasureType.ContainerCount)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "COAL" }, because: "Container Rating Commodity overrides Container Commodity");
			measures
				.GetCommodities()
				.Should().
				BeEquivalentTo(new[] { "COAL" }, because: "Coal is the only commodity");
			Assert("Fluent assertion in use", true);
		}

		public void TestMeasures_ContainerCommodity_WithGrossWeightOverride_OverriddenByContainerRatingCommodity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var gp20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = gp20.PK;
			container.JC_ContainerCount = 5;
			container.JC_IsGrossWeightOverridden = true;
			container.JC_TareWeight = 2000;
			container.JC_DunnageWeight = 200;
			container.JC_GrossWeight = 1000;
			container.JC_IsGrossWeightOverridden = true;
			AssertEquals(true, container.IsGrossWeightOverrideAvailable);
			container.JC_RH_NKContainerCommodityCode = "SHIP";
			container.JC_RH_NKRatingCommodityCode = "COAL";

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			CombineAssertions(() =>
			{
				measures
					.GetPartList(MeasureType.ContainerCount)
					.Select(x => x.CommodityCode)
					.Should().BeEquivalentTo(new[] { "COAL" }, because: "Container Rating Commodity overrides Container Commodity");

				var aa = measures
					.GetPartList(MeasureType.Weight)
					.Select(x => x.CommodityCode);
				aa.Should().BeEquivalentTo(new[] { "COAL" }, because: "Container with GrossWeightOverride introduces its own weight");

				var bb = measures
					.GetPartList(MeasureType.Volume)
					.Select(x => x.CommodityCode);
				bb.Should().BeEquivalentTo(new[] { "COAL" }, because: "Container with GrossWeightOverride introduces its own volume");

				measures
					.GetCommodities()
					.Should().
					BeEquivalentTo(new[] { "COAL" }, because: "Coal is the only commodity");
				Assert("Fluent assertion in use", true);
			});
		}

		public void TestMeasures_ContainerCommodity_NoOverrides()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var gp20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = gp20.PK;
			container.JC_ContainerCount = 5;
			container.JC_IsGrossWeightOverridden = true;
			container.JC_TareWeight = 2000;
			container.JC_DunnageWeight = 200;
			container.JC_GrossWeight = 1000;
			container.JC_RH_NKContainerCommodityCode = "SHIP";

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			measures
				.GetPartList(MeasureType.ContainerCount)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "SHIP" }, because: "It is the container commodity");
			measures
				.GetCommodities()
				.Should()
				.BeEquivalentTo(new[] { "SHIP" }, because: "Ship is the only commodity");
			Assert("Fluent assertion in use", true);
		}

		public void TestMeasures_WeightAndVolumeCommodity_OverriddenByConsolCommodity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RH_NKConsolCommodity = "CHEM";

			var shipment = consol.Shipments.AddNew();
			shipment.AddPackLine(commodity: "SHIP");

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			measures
				.GetPartList(MeasureType.Weight)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "CHEM" }, because: "Consol Rating Commodity overrides all");
			measures
				.GetPartList(MeasureType.Volume)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "CHEM" }, because: "Consol Rating Commodity overrides all");
			measures
				.GetCommodities()
				.Should()
				.BeEquivalentTo(new[] { "CHEM" }, because: "Chem should be the only commodity");
			Assert("Fluent assertion in use", true);
		}

		public void TestMeasures_WeightAndVolumeCommodity_NoOverrides()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.AddPackLine(commodity: "SHIP");

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			measures
				.GetPartList(MeasureType.Weight)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "SHIP" }, because: "It is the packline commodity");
			measures
				.GetPartList(MeasureType.Volume)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "SHIP" }, because: "It is the packline commodity");
			measures.GetCommodities().Should().BeEquivalentTo(new[] { "SHIP" }, because: "Ship is the only commodity");
			Assert("Fluent assertion in use", true);
		}

		public void TestMeasures_IncorrectUnitOfVolume()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RH_NKConsolCommodity = "CHEM";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 100;
			shipment.JS_UnitOfVolume = "CM";

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			adapter.StatusInformation.CanExecute.Should().BeFalse();
			measures
			.GetPartList(MeasureType.Volume)
			.Should().BeNull();
			Assert("Fluent assertion in use", true);
		}

		public void TestMeasures_ContainerCount()
		{
			var gp20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = "FCL";

			var shipment = consol.Shipments.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = gp20.PK;
			container1.JC_ContainerCount = 5;
			container1.JC_IsGrossWeightOverridden = true;
			container1.JC_TareWeight = 2000;
			container1.JC_DunnageWeight = 200;
			container1.JC_GrossWeight = 1000;
			container1.AddPackLines(new[] { shipment.AddPackLine(weight: 500) });

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			var containers = measures.GetPartList(MeasureType.ContainerCount);
			var actualContainers = containers.Cast<IRateableContainer>().Select(c => new
			{
				c.ContainerTypePk,
				c.ContainerCount,
				c.ContainerWeightInKG,
				c.ContainerGrossWeight,
			});

			actualContainers.Should().BeEquivalentTo(new[]
			{
				new
				{
					ContainerTypePk = gp20.PK.ToGuid(),
					ContainerCount = 5,
					ContainerWeightInKG = 500m,
					ContainerGrossWeight = new Quantity(2700, "KG")
				}
			}, "Should use weight and gross weight based on pack lines weight. Gross weight is tare weight + pack lines weight + dunnage weight");

			Assert(true);
		}

		public void TestMeasures_ContainerCount_AIRULDConsol_GrossWeightIsOverriden_UseOverridenGrossWeight()
		{
			var ld6 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-6"));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "ULD";

			var shipment = consol.Shipments.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = ld6.PK;
			container1.JC_ContainerCount = 5;
			container1.JC_IsGrossWeightOverridden = true;
			container1.JC_TareWeight = 2000;
			container1.JC_DunnageWeight = 200;
			container1.JC_GrossWeight = 1000;
			container1.AddPackLines(new[] { shipment.AddPackLine(weight: 500) });

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			var containers = measures.GetPartList(MeasureType.ContainerCount);
			var actualContainers = containers.Cast<IRateableContainer>().Select(c => new
			{
				c.ContainerTypePk,
				c.ContainerCount,
				c.ContainerWeightInKG,
				c.ContainerGrossWeight,
			});

			actualContainers.Should().BeEquivalentTo(new[]
			{
				new
				{
					ContainerTypePk = ld6.PK.ToGuid(),
					ContainerCount = 5,
					ContainerWeightInKG = 1000m,
					ContainerGrossWeight = new Quantity(1000, "KG"),
				}
			}, "When user overrides gross weight it should use this weight as weight as well as a gross weight rather than calculate it from pack lines");

			Assert(true);
		}

		public void TestMeasures_BcnConsol_ReturnMeasuresFromLeadAndSubShipments()
		{
			var ld6 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-6"));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "BCN";

			var leadShipment = consol.Shipments.AddNew();
			leadShipment.JS_PackingMode = "BCN";
			leadShipment.JS_ShipmentType = "BCN";

			var subShipment = consol.Shipments.AddNew();
			subShipment.JS_PackingMode = "BCN";
			subShipment.JS_ShipmentType = "STD";
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = ld6.PK;
			container1.JC_ContainerCount = 5;
			container1.AddPackLines(new[]
			{
				leadShipment.AddPackLine(weight: 200, volume: 0.2m, count: 2),
				leadShipment.AddPackLine(weight: 300, volume: 0.3m, count: 3),
				subShipment.AddPackLine(weight: 400, volume: 0.4m, count: 4),
				subShipment.AddPackLine(weight: 500, volume: 0.5m, count: 5),
			});

			var adapter = consol.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			var actualWeights = measures.GetPartList(MeasureType.Weight).Select(p => p.WeightMeasure.Actual);
			AssertContainsExactElementsInAnyOrder("Should have weight from both - lead BCN shipment and sub shipment", new decimal[] { 200, 300, 400, 500 }, actualWeights);

			var actualVolumes = measures.GetPartList(MeasureType.Volume).Select(p => p.VolumeMeasure.Actual);
			AssertContainsExactElementsInAnyOrder("Should have volume from both - lead BCN shipment and sub shipment", new[] { 0.2m, 0.3m, 0.4m, 0.5m }, actualVolumes);

			var actualPackages = measures.GetPartList(MeasureType.Package).Select(p => p.PackageCount);
			AssertContainsExactElementsInAnyOrder("Should have packages from both - lead BCN shipment and sub shipment", new decimal?[] { 2, 3, 4, 5 }, actualPackages);
		}

		#endregion

		#region IJobDataUpdater

		public void TestUpdateCarrierConfirmationIsNeeded_SingleRoute()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			AssertEquals("Precondition: consol's carrier should be empty", ZGuid.Empty, consol.ShippingLinePK);
			Assert("Updating carrier should be required", ratingAdapter.UpdateCarrierConfirmationIsNeeded("Carrier", out var confirmationMessage));
			AssertNullOrEmpty("Confirmation message should not have value because of empty job's carrier", confirmationMessage);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertNotEquals("Precondition: consol's carrier should not be empty", ZGuid.Empty, consol.ShippingLinePK);
			Assert("Updating carrier should be required", ratingAdapter.UpdateCarrierConfirmationIsNeeded("Carrier", out confirmationMessage));
			AssertNotNullOrEmpty("Confirmation message should have value because job's carrier presents", confirmationMessage);
		}

		public void TestUpdateCarrierConfirmationIsNeeded_MultiRoute()
		{
			var consol = Factory.New<ForwardingConsol>();
			var routes = consol.Transports;
			var firstRoute = routes[0];
			var newRoute = routes.AddNew();
			newRoute.JW_ETD = ZDate.Today.AddDays(4);
			newRoute.JW_ETA = ZDate.Today.AddDays(6);
			newRoute.JW_RL_NKLoadPort = "HKHKG";
			newRoute.JW_RL_NKDiscPort = "SGSGN";

			var routeSet = new RouteSetRatingRoute(new RouteSet(Factory, 1, firstRoute, newRoute), consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSet));

			Assert("Precondition: routes' carriers should be empty", routes.OfType<Transport>().All(x => x.Carrier == null));
			AssertEquals("Precondition: consol's carrier should be empty", ZGuid.Empty, consol.ShippingLinePK);
			Assert("Updating carrier should be required", ratingAdapter.UpdateCarrierConfirmationIsNeeded("Carrier", out var confirmationMessage));
			AssertNullOrEmpty("Confirmation message should not have value because of empty job's carriers", confirmationMessage);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			firstRoute.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			Assert("Updating carrier should be required", ratingAdapter.UpdateCarrierConfirmationIsNeeded("Carrier", out confirmationMessage));
			AssertNotNullOrEmpty("Confirmation message should have value because job's carrier presents", confirmationMessage);
		}

		public void TestUpdateOriginConfirmationIsNeeded()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			Assert("Confirmation is needed to update origin on consol", ratingAdapter.UpdateOriginConfirmationIsNeeded("Origin", out var confirmationMessage));
			AssertNotNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateDestinationConfirmationIsNeeded()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			Assert("Confirmation is needed to update destination on consol", ratingAdapter.UpdateDestinationConfirmationIsNeeded("Destination", out var confirmationMessage));
			AssertNotNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateServiceLevel_ConsolMode_ShouldPopulateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AWBServiceLevel = "STD";

			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));
			AssertEquals("STD", ratingAdapter.ServiceLevel.ServiceLevelData.First().ServiceLevel);

			ratingAdapter.UpdateServiceLevel("EXP");
			AssertEquals("EXP", consol.JK_AWBServiceLevel);
			AssertEquals("EXP", ratingAdapter.ServiceLevel.ServiceLevelData.First().ServiceLevel);
		}

		public void TestUpdateServiceLevel_RouteMode_ShouldNotPopulateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AWBServiceLevel = "STD";

			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));
			AssertEquals("STD", ratingAdapter.ServiceLevel.ServiceLevelData.First().ServiceLevel);

			ratingAdapter.UpdateServiceLevel("EXP");
			AssertEquals("STD", consol.JK_AWBServiceLevel);
			AssertEquals("STD", ratingAdapter.ServiceLevel.ServiceLevelData.First().ServiceLevel);
		}

		public void TestUpdateCarrier_ConsolMode_ShouldPopulateCarrierOnConsol()
		{
			var oldCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var newCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = oldCarrier.MainAddress.PK;

			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));
			AssertEquals(oldCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);

			ratingAdapter.UpdateCarrier(newCarrier);
			AssertEquals(newCarrier.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			AssertEquals(newCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);
		}

		public void TestUpdateCarrier_RouteMode_ShouldPopulateCarrierOnRoute()
		{
			var oldCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var newCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.Transports[0].CarrierPK = oldCarrier.PK;
			consol.Transports[0].JW_IsLinked = true;

			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));
			AssertEquals(oldCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);

			ratingAdapter.UpdateCarrier(newCarrier);
			AssertEquals(newCarrier.PK, consol.Transports[0].CarrierPK);
			AssertEquals(false, consol.Transports[0].JW_IsLinked);
			AssertEquals(newCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);
		}

		public void TestUpdateLocation_ConsolMode_ShouldPopulateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));
			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);

			ratingAdapter.UpdateOrigin("SGSIN");
			ratingAdapter.UpdateDestination("USLAX");
			AssertEquals("SGSIN", consol.JK_RL_NKLoadPort);
			AssertEquals("USLAX", consol.JK_RL_NKDischargePort);
			AssertEquals("SGSIN", ratingAdapter.Origin.Code);
			AssertEquals("USLAX", ratingAdapter.Destination.Code);
		}

		public void TestUpdateLocation_RouteMode_ShouldNotPopulateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_RL_NKLoadPort = "UAIEV";
			consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";

			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));
			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);

			ratingAdapter.UpdateOrigin("SGSIN");
			ratingAdapter.UpdateDestination("USLAX");
			AssertEquals("UAIEV", consol.JK_RL_NKLoadPort);
			AssertEquals("AUSYD", consol.JK_RL_NKDischargePort);
			AssertEquals("UAIEV", consol.Transports[0].JW_RL_NKLoadPort);
			AssertEquals("AUSYD", consol.Transports[0].JW_RL_NKDiscPort);
			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);
		}

		public void TestUpdatePaymentTerms_ConsolMode_ShouldPopulateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = "PPD";

			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));
			ratingAdapter.PaymentTerm.PaymentTermInfoCollection.ForEach(t => AssertEquals("PPD", t.Value));

			ratingAdapter.UpdatePaymentTerms("CCL");
			AssertEquals("CCL", consol.JK_PrepaidCollect);
			ratingAdapter.PaymentTerm.PaymentTermInfoCollection.ForEach(t => AssertEquals("CCL", t.Value));
		}

		public void TestUpdatePaymentTerms_RouteMode_ShouldNotPopulateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = "PPD";

			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));
			ratingAdapter.PaymentTerm.PaymentTermInfoCollection.ForEach(t => AssertEquals("PPD", t.Value));

			ratingAdapter.UpdatePaymentTerms("CCL");
			AssertEquals("PPD", consol.JK_PrepaidCollect);
			ratingAdapter.PaymentTerm.PaymentTermInfoCollection.ForEach(t => AssertEquals("PPD", t.Value));
		}

		public void TestUpdateAutoratingDate()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.AutoratingDate = ZDate.Empty;

			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			var testDate1 = new ZDate(2023, 4, 4);
			ratingAdapter.UpdateAutoratingDate(testDate1, isCosting: false);
			Assert("should not change", consol.AutoratingDate.IsEmpty);
			ratingAdapter.UpdateAutoratingDate(testDate1, isCosting: true);
			AssertEquals("should change", testDate1, consol.AutoratingDate);

			var testDate2 = new ZDate(2023, 4, 5);
			ratingAdapter.UpdateAutoratingDate(testDate2, isCosting: false);
			AssertEquals("should not change", testDate1, consol.AutoratingDate);
			ratingAdapter.UpdateAutoratingDate(testDate2, isCosting: true);
			AssertEquals("should change", testDate2, consol.AutoratingDate);
		}

		#region Spot Rate

		public void TestUpdateTransports_RouteMode_ShouldDeleteRelatedRouteSetLegsAndCreateTransportsWithCorrectInformationBasedOnNewTransports()
		{
			var consol = Factory.New<ForwardingConsol>();

			var currentConsolRoutes = new[]
				{
					consol.Transports[0], //leg 1
					consol.Transports.AddNew(), //leg 2 - RouteSet Leg1
					consol.Transports.AddNew(), //leg 3 - RouteSet Leg2
					consol.Transports.AddNew(), //leg 4 - RouteSet Leg3
					consol.Transports.AddNew(), //leg 5
					consol.Transports.AddNew(), //leg 6
				};

			currentConsolRoutes[1].JW_CarrierBookingReference = "REF1";
			currentConsolRoutes[2].JW_CarrierBookingReference = "REF1";
			currentConsolRoutes[3].JW_CarrierBookingReference = "REF1";

			var route = new RouteSet(Factory, 0, currentConsolRoutes[1], currentConsolRoutes[2], currentConsolRoutes[3]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			var transportsToUpdate = new[]
				{
					new DummyTransport()
					{
						JW_Status = TransportStatus.Planned,
						JW_IsLinked = true,
						JW_LegOrder = 1,
						JW_TransportMode = TransportModes.Sea,
						JW_VoyageFlight = "V9849384",
						JW_Vessel = "ADRIANA D",
						JW_RL_NKLoadPort = "SEGVX",
						JW_RL_NKDiscPort = "DEBRV",
						JW_ETA = new ZDateTime(2021, 01, 15, 14, 0, 0),
						JW_ETD = new ZDateTime(2021, 01, 10, 07, 0, 0),
						JW_LegNotes = "Some Notes",
						JW_DocumentaryCutOff = new ZDateTime(2021, 01, 08, 10, 0, 0),
						JW_TerminalCutOff = new ZDateTime(2021, 01, 08, 19, 0, 0),
						JW_VGMCutOff = new ZDateTime(2021, 01, 08, 17, 0, 0),
					},
					new DummyTransport()
					{
						JW_LegOrder = 2,
						JW_RL_NKLoadPort = "DEBRV",
						JW_RL_NKDiscPort = "ESALG",
					}
				};

			var expectedTransports = new ITransport[]
			{
				currentConsolRoutes[0],
				new DummyTransport()
				{
					JW_Status = TransportStatus.Planned,
					JW_IsLinked = true,
					JW_LegOrder = 2, //Leg Orders should be updated based on RouteSet's First Leg
					JW_TransportMode = TransportModes.Sea,
					JW_VoyageFlight = "V9849384",
					JW_Vessel = "ADRIANA D",
					JW_RL_NKLoadPort = "SEGVX",
					JW_RL_NKDiscPort = "DEBRV",
					JW_ETA = new ZDateTime(2021, 01, 15, 14, 0, 0),
					JW_ETD = new ZDateTime(2021, 01, 10, 07, 0, 0),
					JW_LegNotes = "Some Notes",
					JW_DocumentaryCutOff = new ZDateTime(2021, 01, 08, 10, 0, 0),
					JW_TerminalCutOff = new ZDateTime(2021, 01, 08, 19, 0, 0),
					JW_VGMCutOff = new ZDateTime(2021, 01, 08, 17, 0, 0),
				},
				new DummyTransport()
				{
					JW_LegOrder = 3, //Leg Orders should be updated based on RouteSet's First Leg
					JW_RL_NKLoadPort = "DEBRV",
					JW_RL_NKDiscPort = "ESALG",
				},
				currentConsolRoutes[4],
				currentConsolRoutes[5]
			};

			ratingAdapter.UpdateTransports(transportsToUpdate);

			var orderedTransportLegs =
				consol
				.Transports
				.Select(t => t)
				.OrderBy(t => t.JW_LegOrder)
				.ToArray();

			orderedTransportLegs
			.Should()
			.BeEquivalentTo
				(
					expectedTransports
					, options => options.ComparingByMembers<ITransport>().Excluding(i => i.ParentType).Excluding(i => i.JW_ParentGUID).Excluding(i => i.JW_VesselScreeningStatus)
				);

			//New legs's Carrier Reference should be the same as deleted legs
			AssertEquals("REF1", orderedTransportLegs[1].JW_CarrierBookingReference);
			AssertEquals("REF1", orderedTransportLegs[2].JW_CarrierBookingReference);

			//re-ordering of legs that will remain
			AssertEquals(currentConsolRoutes[0].JW_LegOrder, new ZByte(1));
			AssertEquals(currentConsolRoutes[4].JW_LegOrder, new ZByte(4));
			AssertEquals(currentConsolRoutes[5].JW_LegOrder, new ZByte(5));

			//legs that should still remain on job
			AssertEquals(currentConsolRoutes[0].PK, orderedTransportLegs[0].PK);
			AssertEquals(currentConsolRoutes[4].PK, orderedTransportLegs[3].PK);
			AssertEquals(currentConsolRoutes[5].PK, orderedTransportLegs[4].PK);
		}

		public void TestUpdateTransports_ConsolMode_ShouldDeleteExistingAndCreateTransportsWithCorrectInformationBasedOnNewTransports()
		{
			var consol = Factory.New<ForwardingConsol>();

			var currentConsolTransportLegPKs = new[]
				{
					consol.Transports[0].PK,
					consol.Transports.AddNew().PK,
					consol.Transports.AddNew().PK
				};

			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			var transports = new[]
			{
				new DummyTransport()
				{
					JW_Status = TransportStatus.Planned,
					JW_IsLinked = true,
					JW_LegOrder = 1,
					JW_TransportMode = TransportModes.Sea,
					JW_VoyageFlight = "V9849384",
					JW_Vessel = "ADRIANA D",
					JW_RL_NKLoadPort = "SEGVX",
					JW_RL_NKDiscPort = "DEBRV",
					JW_ETA = new ZDateTime(2021, 01, 15, 14, 0, 0),
					JW_ETD = new ZDateTime(2021, 01, 10, 07, 0, 0),
					JW_LegNotes = "Some Notes",
					JW_DocumentaryCutOff = new ZDateTime(2021, 01, 08, 10, 0, 0),
					JW_TerminalCutOff = new ZDateTime(2021, 01, 08, 19, 0, 0),
					JW_VGMCutOff = new ZDateTime(2021, 01, 08, 17, 0, 0),
				},
				new DummyTransport()
				{
					JW_LegOrder = 2,
					JW_RL_NKLoadPort = "DEBRV",
					JW_RL_NKDiscPort = "ESALG",
				}
			};

			ratingAdapter.UpdateTransports(transports);

			AssertEquals(2, consol.Transports.Count);

			Assert(!currentConsolTransportLegPKs.Contains(consol.Transports[0].PK));
			Assert(!currentConsolTransportLegPKs.Contains(consol.Transports[1].PK));

			consol
				.Transports
				.Select(t => (ITransport)t)
				.Should()
				.BeEquivalentTo
					(
						transports
						, options => options.ComparingByMembers<ITransport>().Excluding(i => i.ParentType).Excluding(i => i.JW_ParentGUID).Excluding(i => i.JW_VesselScreeningStatus)
					);
		}

		public void TestUpdateCarrierQuoteNumber_ConsolMode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			ratingAdapter.UpdateCarrierQuoteNumber("PI0001");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0003");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals(3, values.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "PI0001", "PI0002", "PI0003" }, values);
		}

		public void TestUpdateCarrierQuoteNumber_RouteMode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			ratingAdapter.UpdateCarrierQuoteNumber("PI0001");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0003");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals(3, values.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "PI0001", "PI0002", "PI0003" }, values);
		}

		public void TestUpdateSpotBookingTerms_RouteMode_ShouldOverrideExisting()
		{
			var consol = Factory.New<ForwardingConsol>();
			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			var existingNote = consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description, "Existing Note");

			ratingAdapter.UpdateSpotBookingTerms("Terms");

			consol.Notes.GetAllNotes().Should().BeEquivalentTo(new[]
			{
				new
				{
					ST_Description = new ZString(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description),
					ST_NoteText = new ZString("Terms")
				},
			});

			Assert(true);
		}

		public void TestUpdateSpotBookingTerms_RouteMode_ShouldPopulateNotesCorrectly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			var existingNote = consol.Notes.AddNew(true, "Some Description", "Some notes");

			ratingAdapter.UpdateSpotBookingTerms("Terms");

			consol.Notes.GetAllNotes().Should().BeEquivalentTo(new[]
			{
				new
				{
					ST_Description = new ZString("Some Description"),
					ST_NoteText = new ZString("Some notes")
				},
				new
				{
					ST_Description = new ZString(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description),
					ST_NoteText = new ZString("Terms")
				},
			});

			Assert(true);
		}

		public void TestUpdateSpotBookingTerms_ConsolMode_ShouldPopulateNotesCorrectly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			var existingNote = consol.Notes.AddNew(true, "Some Description", "Some notes");

			ratingAdapter.UpdateSpotBookingTerms("Terms");

			consol.Notes.GetAllNotes().Should().BeEquivalentTo(new[]
			{
				new
				{
					ST_Description = new ZString("Some Description"),
					ST_NoteText = new ZString("Some notes")
				},
				new
				{
					ST_Description = new ZString(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description),
					ST_NoteText = new ZString("Terms")
				},
			});

			Assert(true);
		}

		public void TestUpdateSpotBookingTerms_ConsolMode_ShouldOverrideExisting()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			var existingNote = consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description, "Existing Note");

			ratingAdapter.UpdateSpotBookingTerms("Terms");

			consol.Notes.GetAllNotes().Should().BeEquivalentTo(new[]
			{
				new
				{
					ST_Description = new ZString(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description),
					ST_NoteText = new ZString("Terms")
				},
			});

			Assert(true);
		}

		public void TestUpdateContainerPenaltiesConfirmationIsNeeded_RouteMode_WhenDuplicates_ConfirmationIsNeeded()
		{
			var consol = Factory.New<ForwardingConsol>();
			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol20GPContainer = consol.Containers.AddNew();
			consol20GPContainer.JC_RC = containerType20GP.PK;
			consol20GPContainer.JC_ContainerCount = 1;
			var consolContainerPenalty = consol20GPContainer.ImportPenalties.AddNew();
			consolContainerPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			consolContainerPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;

			var confirmationIsNeeded = ratingAdapter.UpdateContainerPenaltiesConfirmationIsNeeded(containerPenalties, out var confirmationMessage);
			Assert(confirmationIsNeeded);
			AssertNotNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateContainerPenaltiesConfirmationIsNeeded_RouteMode_WhenNoDuplicate_NoConfirmation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var confirmationIsNeeded = ratingAdapter.UpdateContainerPenaltiesConfirmationIsNeeded(containerPenalties, out var confirmationMessage);
			Assert(!confirmationIsNeeded);
			AssertNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateContainerPenaltiesConfirmationIsNeeded_ConsolMode_WhenDuplicates_ConfirmationIsNeeded()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol20GPContainer = consol.Containers.AddNew();
			consol20GPContainer.JC_RC = containerType20GP.PK;
			consol20GPContainer.JC_ContainerCount = 1;
			var consolContainerPenalty = consol20GPContainer.ImportPenalties.AddNew();
			consolContainerPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			consolContainerPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;

			var confirmationIsNeeded = ratingAdapter.UpdateContainerPenaltiesConfirmationIsNeeded(containerPenalties, out var confirmationMessage);
			Assert(confirmationIsNeeded);
			AssertNotNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateContainerPenaltiesConfirmationIsNeeded_ConsolMode_WhenNoDuplicate_NoConfirmation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var confirmationIsNeeded = ratingAdapter.UpdateContainerPenaltiesConfirmationIsNeeded(containerPenalties, out var confirmationMessage);
			Assert(!confirmationIsNeeded);
			AssertNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateContainerPenalties_RouteMode_ShouldAddPenaltiesCorrectly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol20GPContainer = consol.Containers.AddNew();
			consol20GPContainer.JC_RC = containerType20GP.PK;
			consol20GPContainer.JC_ContainerCount = 1;
			var consolContainerPenalty = consol20GPContainer.ImportPenalties.AddNew();
			consolContainerPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			consolContainerPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			consolContainerPenalty.CPY_PerUnitCost = 45;
			consolContainerPenalty.CPY_RX_NKCurrency = "EUR";
			consolContainerPenalty.CPY_FreeTime = new DateTime(ZDateTime.Now.Year, 1, 17);
			consolContainerPenalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;

			var expectedPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO,
					CPY_FreeTime = TimeSpan.FromDays(16),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 45,
					CPY_RX_NKCurrency = "EUR"
				},
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				},
			};

			ratingAdapter.UpdateContainerPenalties(containerPenalties, false);
			consol20GPContainer.ImportPenalties.Should().BeEquivalentTo(expectedPenalties);

			Assert(true);
		}

		public void TestUpdateContainerPenalties_RouteMode_DeleteExistingDuplicates()
		{
			var consol = Factory.New<ForwardingConsol>();
			var route = new RouteSet(Factory, 0, consol.Transports[0]);
			var routeSetRatingRoute = new RouteSetRatingRoute(route, consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(routeSetRatingRoute));

			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol20GPContainer = consol.Containers.AddNew();
			consol20GPContainer.JC_RC = containerType20GP.PK;
			consol20GPContainer.JC_ContainerCount = 1;
			var consolContainerPenalty = consol20GPContainer.ImportPenalties.AddNew();
			consolContainerPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			consolContainerPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			consolContainerPenalty.CPY_PerUnitCost = 45;
			consolContainerPenalty.CPY_RX_NKCurrency = "EUR";
			consolContainerPenalty.CPY_FreeTime = TimeSpan.FromDays(16);
			consolContainerPenalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;

			var expectedPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				},
			};

			ratingAdapter.UpdateContainerPenalties(containerPenalties, true);
			consol20GPContainer.ImportPenalties.Should().BeEquivalentTo(expectedPenalties);
			Assert(true);
		}

		public void TestUpdateContainerPenalties_ConsolMode_ShouldAddPenaltiesCorrectly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol20GPContainer = consol.Containers.AddNew();
			consol20GPContainer.JC_RC = containerType20GP.PK;
			consol20GPContainer.JC_ContainerCount = 1;
			var consolContainerPenalty = consol20GPContainer.ImportPenalties.AddNew();
			consolContainerPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			consolContainerPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			consolContainerPenalty.CPY_PerUnitCost = 45;
			consolContainerPenalty.CPY_RX_NKCurrency = "EUR";
			consolContainerPenalty.CPY_FreeTime = TimeSpan.FromDays(16);
			consolContainerPenalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;

			var expectedPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO,
					CPY_FreeTime = TimeSpan.FromDays(16),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 45,
					CPY_RX_NKCurrency = "EUR"
				},
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				},
			};

			ratingAdapter.UpdateContainerPenalties(containerPenalties, false);
			consol20GPContainer.ImportPenalties.Should().BeEquivalentTo(expectedPenalties);

			Assert(true);
		}

		public void TestUpdateContainerPenalties_ConsolMode_DeleteExistingDuplicates()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolRatingRoute = new ConsolRatingRoute(consol);
			var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol20GPContainer = consol.Containers.AddNew();
			consol20GPContainer.JC_RC = containerType20GP.PK;
			consol20GPContainer.JC_ContainerCount = 1;
			var consolContainerPenalty = consol20GPContainer.ImportPenalties.AddNew();
			consolContainerPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			consolContainerPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			consolContainerPenalty.CPY_PerUnitCost = 45;
			consolContainerPenalty.CPY_RX_NKCurrency = "EUR";
			consolContainerPenalty.CPY_FreeTime = TimeSpan.FromDays(16);
			consolContainerPenalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;

			var expectedPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				},
			};

			ratingAdapter.UpdateContainerPenalties(containerPenalties, true);
			consol20GPContainer.ImportPenalties.Should().BeEquivalentTo(expectedPenalties);
			Assert(true);
		}

		#endregion

		#endregion

		#region IAutoratingAccountingInfo

		public void TestGetExistingCharges()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "CCC1";
			chargeCode1.AC_GC = company1.PK;

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "CCC2";
			chargeCode2.AC_GC = company1.PK;

			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_Code = "CCC3";
			chargeCode3.AC_GC = company2.PK;

			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_Code = "CCC4";
			chargeCode4.AC_GC = GlbCompany.CurrentCompany.PK;

			var chargeCode5 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode5.AC_Code = "CCC5";
			chargeCode5.AC_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			var cost1 = (BusinessObject)Factory.New<IJobConsolCost>();
			cost1.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			cost1[JobConsolCostSchema.E6_GC] = company1.PK;
			cost1[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCode1.PK;
			cost1[JobConsolCostSchema.E6_ParentID] = consol.PK;
			cost1[JobConsolCostSchema.E6_ParentTableCode] = "JK";

			var cost2 = (BusinessObject)Factory.New<IJobConsolCost>();
			cost2.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			cost2[JobConsolCostSchema.E6_GC] = company1.PK;
			cost2[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCode2.PK;
			cost2[JobConsolCostSchema.E6_ParentID] = consol.PK;
			cost2[JobConsolCostSchema.E6_ParentTableCode] = "JK";

			var cost3 = (BusinessObject)Factory.New<IJobConsolCost>();
			cost3.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			cost3[JobConsolCostSchema.E6_GC] = company2.PK;
			cost3[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCode3.PK;
			cost3[JobConsolCostSchema.E6_ParentID] = consol.PK;
			cost3[JobConsolCostSchema.E6_ParentTableCode] = "JK";

			var cost4 = (BusinessObject)Factory.New<IJobConsolCost>();
			cost4.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			cost4[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			cost4[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCode4.PK;
			cost4[JobConsolCostSchema.E6_ParentID] = consol.PK;
			cost4[JobConsolCostSchema.E6_ParentTableCode] = "JK";

			// A cost for another consol. Just to make sure it doesn't load costs belonging to other consols.
			var cost5 = (BusinessObject)Factory.New<IJobConsolCost>();
			cost5.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			cost5[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			cost5[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCode5.PK;
			cost5[JobConsolCostSchema.E6_ParentID] = consol2.PK;
			cost5[JobConsolCostSchema.E6_ParentTableCode] = "JK";

			var adapter = (IAutoRatingAccountingInfo)consol.RatingAdapter;

			var charges = adapter.GetExistingCharges();
			charges.Select(c => (string)c.ChargeCode.AC_Code).Should().BeEquivalentTo(new[] { "CCC4" },
				"Only charges from the current company must be returned if fromAllCompanies flag is false");

			charges = adapter.GetExistingCharges(fromAllCompanies: true);
			charges.Select(c => (string)c.ChargeCode.AC_Code).Should().BeEquivalentTo(new[] { "CCC1", "CCC2", "CCC3", "CCC4" },
				"Charges from the companies must be returned if fromAllCompanies flag is true");

			Assert(true);
		}

		#endregion
	}
}
