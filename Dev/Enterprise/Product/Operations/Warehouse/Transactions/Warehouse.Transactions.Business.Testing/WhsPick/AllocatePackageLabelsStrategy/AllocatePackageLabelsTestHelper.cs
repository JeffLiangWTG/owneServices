#if DEBUG

using System;
using System.Linq;
using Moq;
using static NUnit.Framework.Assertion;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class AllocatePackageLabelsTestHelper
	{
		public static void TestAllocatePackageLabels(Action actionWhichShouldCallAllocatePackageLabels, WhsPick pick)
		{
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: false, pickPalletsByLabel: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: false, pickPalletsByLabel: true, initialValidationPassed: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: false, initialValidationPassed: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: false, pickPalletsByLabel: true, initialValidationPassed: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: false, initialValidationPassed: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: true, initialValidationPassed: false);

			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: true);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: true, pickPalletsCreatedPackages: false, pickCasesCreatedPackages: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: true, pickPalletsCreatedPackages: true, pickCasesCreatedPackages: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: true, pickPalletsCreatedPackages: false, pickCasesCreatedPackages: true);

			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: false, pickPalletsCreatedPackages: false, pickCasesCreatedPackages: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: false, pickPalletsCreatedPackages: true, pickCasesCreatedPackages: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: true, pickPalletsByLabel: false, pickPalletsCreatedPackages: false, pickCasesCreatedPackages: true);

			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: false, pickPalletsByLabel: true);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: false, pickPalletsByLabel: true, pickPalletsCreatedPackages: false, pickCasesCreatedPackages: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: false, pickPalletsByLabel: true, pickPalletsCreatedPackages: true, pickCasesCreatedPackages: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: false, pickCasesByLabel: false, pickPalletsByLabel: true, pickPalletsCreatedPackages: false, pickCasesCreatedPackages: true);

			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: false, pickPalletsByLabel: false, splitCaseResult: CartonisationResult.Error);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: false, splitCaseResult: CartonisationResult.Error);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: false, pickPalletsByLabel: true, splitCaseResult: CartonisationResult.Error);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: true, splitCaseResult: CartonisationResult.Error);

			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: false, pickPalletsByLabel: false, splitCaseResult: CartonisationResult.NothingToCartonise);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: false, splitCaseResult: CartonisationResult.NothingToCartonise);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: false, pickPalletsByLabel: true, splitCaseResult: CartonisationResult.NothingToCartonise);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: true, splitCaseResult: CartonisationResult.NothingToCartonise);

			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: true, splitCaseResult: CartonisationResult.NothingToCartonise, pickCasesCreatedPackages: false, pickPalletsCreatedPackages: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: true, splitCaseResult: CartonisationResult.NothingToCartonise, pickCasesCreatedPackages: false, pickPalletsCreatedPackages: true);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: true, splitCaseResult: CartonisationResult.NothingToCartonise, pickCasesCreatedPackages: true, pickPalletsCreatedPackages: false);

			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: false, pickPalletsByLabel: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: false);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: false, pickPalletsByLabel: true);
			SetUpPickAndMockAndAssert(actionWhichShouldCallAllocatePackageLabels, pick, splitCaseEnabled: true, pickCasesByLabel: true, pickPalletsByLabel: true);
		}

		static void SetUpPickAndMockAndAssert(Action actionWhichShouldCallAllocatePackageLabels, WhsPick pick, bool splitCaseEnabled, bool pickCasesByLabel, bool pickPalletsByLabel, bool initialValidationPassed = true,
			CartonisationResult splitCaseResult = CartonisationResult.Cartonised, bool pickPalletsCreatedPackages = true, bool pickCasesCreatedPackages = true)
		{
			// Setup Pick
			pick.WP_CartoniseSplitCases = splitCaseEnabled;
			pick.WP_PickCasesByLabel = pickCasesByLabel;
			pick.WP_PickPalletsByLabel = pickPalletsByLabel;

			// Setup Mocks
			var nothingCalledMock = new Mock<IAllocatePackageLabelsStrategy>();
			var strategyMock = new Mock<IAllocatePackageLabelsStrategy>();

			nothingCalledMock.Verify(m => m.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), It.IsAny<bool>()), Times.Never);
			nothingCalledMock.Verify(m => m.CartoniseSplitCases(It.IsAny<WhsPick>(), It.IsAny<bool>()), Times.Never);
			nothingCalledMock.Verify(m => m.PickCasesByLabel(It.IsAny<WhsPick>()), Times.Never);
			nothingCalledMock.Verify(m => m.PickPalletsByLabel(It.IsAny<WhsPick>()), Times.Never);

			if (!splitCaseEnabled && !pickCasesByLabel && !pickPalletsByLabel)
			{
				strategyMock = nothingCalledMock;
			}
			else
			{
				strategyMock.Setup(m =>
					m.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), It.IsAny<bool>())).Returns<WhsPick, bool>(
					(arg1, arg2) =>
					{
						AssertEquals("Should pass in pick.", pick, arg1);
						return initialValidationPassed;
					});

				if (initialValidationPassed && splitCaseEnabled)
				{
					strategyMock.Setup(m =>
						m.CartoniseSplitCases(It.IsAny<WhsPick>(), It.IsAny<bool>())).Returns<WhsPick, bool>(
						(arg1, arg2) =>
						{
							AssertEquals("Should pass in pick.", pick, arg1);
							return splitCaseResult;
						});
				}
				else
				{
					strategyMock.Verify(m => m.CartoniseSplitCases(It.IsAny<WhsPick>(), It.IsAny<bool>()), Times.Never);
				}

				if (initialValidationPassed && pickPalletsByLabel && (!splitCaseEnabled || splitCaseResult != CartonisationResult.Error))
				{
					strategyMock.Setup(m =>
						m.PickPalletsByLabel(It.IsAny<WhsPick>())).Returns<WhsPick>(
						(arg1) =>
						{
							AssertEquals("Should pass in pick.", pick, arg1);
							return pickPalletsCreatedPackages;
						});
				}
				else
				{
					strategyMock.Verify(m => m.PickPalletsByLabel(It.IsAny<WhsPick>()), Times.Never);
				}

				if (initialValidationPassed && pickCasesByLabel && (!splitCaseEnabled || splitCaseResult != CartonisationResult.Error))
				{
					strategyMock.Setup(m =>
						m.PickCasesByLabel(It.IsAny<WhsPick>())).Returns<WhsPick>(
						(arg1) =>
						{
							AssertEquals("Should pass in pick.", pick, arg1);
							return pickCasesCreatedPackages;
						});
				}
				else
				{
					strategyMock.Verify(m => m.PickCasesByLabel(It.IsAny<WhsPick>()), Times.Never);
				}
			}

			AssertEquals(string.Empty, pick.MockProgressForm.Status);
			AssertEquals(false, pick.MockProgressForm.IsDisposed);

			SetAllocatePackageLabelsMockAndVerify(actionWhichShouldCallAllocatePackageLabels, pick, strategyMock);
			AssertEquals("Should have released lock.", false, pick.IsCartonising);
			AssertEquals(
				expected:
				initialValidationPassed &&
				(!splitCaseEnabled || splitCaseResult != CartonisationResult.Error) && // Cartonisation not run, or did not have fatal errors, AND
				((splitCaseEnabled && splitCaseResult == CartonisationResult.Cartonised) // Cartonisation Created Packages, OR
				|| (pickPalletsByLabel && pickPalletsCreatedPackages)                   // Pick Pallets By Label Created Packages, OR
				|| (pickCasesByLabel && pickCasesCreatedPackages)),                     // Pick Cases By Label Created Packages

				actual:
				pick.WP_IsCartonised);

			AssertEquals("Allocating Package Labels...", pick.MockProgressForm.Status);
			AssertEquals(true, pick.MockProgressForm.IsDisposed);

			if (pick.WP_IsCartonised)
			{
				AssertEquals("Should have saved automatically.", false, pick.HasChanges);
				pick.WP_IsCartonised = false;
				pick.Factory.Save();
			}

			TemporarilyAddPackageAndSetMockAndVerify(actionWhichShouldCallAllocatePackageLabels, pick, nothingCalledMock);
			pick.MockProgressForm.ClearStatus();
		}

		static void SetAllocatePackageLabelsMockAndVerify(Action actionWhichShouldCallAllocatePackageLabels, WhsPick pick, Mock<IAllocatePackageLabelsStrategy> mock)
		{
			pick.SetAllocatePackageLabelsStrategyForTest(mock.Object);
			actionWhichShouldCallAllocatePackageLabels();
			mock.VerifyAll();
		}

		static void TemporarilyAddPackageAndSetMockAndVerify(Action actionWhichShouldCallAllocatePackageLabels, WhsPick pick, Mock<IAllocatePackageLabelsStrategy> mock)
		{
			var order = (WhsOrder)(pick.Orders.FirstOrDefault() ?? pick.Orders.AddNew());
			var pkg = order.PackageJob.Packages.AddNew();
			SetAllocatePackageLabelsMockAndVerify(actionWhichShouldCallAllocatePackageLabels, pick, mock);
			pkg.Delete();
		}
	}
}

#endif
