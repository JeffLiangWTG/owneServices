using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestsSubclassesOf(typeof(CusInBondMoveHeaderValidation))]
	public abstract class CusInBondMoveHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateInBondNumber()
		{
			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch otherBranch = currentCompany.Branches.AddNew();
			otherBranch.GB_Code = "~Z~";
			InBondNumberRange numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty);
			numberRange.StartNumber = 0;
			AssertEquals(false, numberRange.IsRangeValidForNumberFountain);
			string branchErrorMessage = InBondNumberAvailabilityChecker.InBondNumberRangeNotSetup(((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
			Header.BH_GB = otherBranch.PK;
			MoveHeader.InBondNumber = ZString.Empty;
			AssertHasMessageError(MoveHeader.InBondNumberInfo, branchErrorMessage);
			MoveHeader.InBondNumber = "153598466";
			AssertNoMessageError(MoveHeader.InBondNumberInfo, branchErrorMessage);
			DbConnection connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(Env.CurrentBranch.PK);
				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
				{
				}

				string warningMessage = InBondNumberSetting.BranchInBondNumberHasReachedLimitWarning(2, GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName);
				var notEnoughAvailableInBondNumbersMessageError = InBondNumberAvailabilityChecker.NotEnoughAvailableInBondNumbersForBranchForJob(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName);
				Header.BH_GB = GlbBranch.CurrentBranch.PK;
				MoveHeader.InBondNumber = ZString.Empty;
				AssertNoMessageError(MoveHeader.InBondNumberInfo, branchErrorMessage);
				AssertHasMessageError(MoveHeader.InBondNumberInfo, notEnoughAvailableInBondNumbersMessageError);
				MoveHeader.InBondNumber = "153598466";
				AssertNoMessageError(MoveHeader.InBondNumberInfo, notEnoughAvailableInBondNumbersMessageError);
				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				long numberToCheck = (long)(numberRange.LastNumber - 1);
				while (numberToCheck != branchNumberFountain.PeekPreliminary(Factory))
				{
					branchNumberFountain.GetNext(Factory);
				}

				MoveHeader.InBondNumber = ZString.Empty;
				AssertNoMessageError(MoveHeader.InBondNumberInfo, branchErrorMessage);
				AssertNoMessageError(MoveHeader.InBondNumberInfo, notEnoughAvailableInBondNumbersMessageError);
				AssertHasWarning(MoveHeader.InBondNumberInfo, warningMessage);
			}
			catch
			{
				throw;
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		Customs.Business.CusInBondHeader header;
		protected Customs.Business.CusInBondHeader Header => header ?? (header = CreateNewCusInBondHeader());

		CusInBondMoveHeader moveHeader;
		protected CusInBondMoveHeader MoveHeader => moveHeader ?? (moveHeader = CreateNewCusInBondMoveHeader(Header));

		protected override void SetUp()
		{
			header = null;
			moveHeader = null;
			base.SetUp();
		}

		protected abstract CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header);

		protected abstract Customs.Business.CusInBondHeader CreateNewCusInBondHeader();
	}
}
