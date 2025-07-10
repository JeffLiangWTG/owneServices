using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GLAccountCommonPropertyReadOnlyGetterTest : TestCaseWithFactory
	{
		public void TestPropertyReadOnlyAndClearValueForGLHeader()
		{
			var glHeader = Factory.New<AccGLHeader>();
			setInfoValue(glHeader);
			glHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			AssertReadOnlyAndValue(false, true, false, false, false, true, null, true, glHeader);

			setInfoValue(glHeader);
			glHeader.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			AssertReadOnlyAndValue(true, true, false, false, true, true, null, true, glHeader);

			setInfoValue(glHeader);
			glHeader.AG_AccountType = Core.Constants.AccountType.Total;
			AssertReadOnlyAndValue(true, false, false, false, true, true, null, true, glHeader);

			setInfoValue(glHeader);
			glHeader.AG_AccountType = Core.Constants.AccountType.Header;
			AssertReadOnlyAndValue(true, true, true, true, true, false, null, true, glHeader);

			setInfoValue(glHeader);
			glHeader.AG_AccountType = Core.Constants.AccountType.Consolidation;
			AssertReadOnlyAndValue(true, true, false, false, true, true, null, true, glHeader);

			setInfoValue(glHeader);
			glHeader.AG_AccountType = Core.Constants.AccountType.Alternate;
			AssertReadOnlyAndValue(true, true, false, false, true, true, null, true, glHeader);

			setInfoValue(glHeader);
			glHeader.AG_AccountType = Core.Constants.AccountType.Note;
			AssertReadOnlyAndValue(true, true, true, true, true, true, null, false, glHeader);
		}

		public void TestPropertyReadOnlyAndClearValueForGLAccountDescriptor()
		{
			var glDescriptor = Factory.New<AccGLAccountDescriptor>();
			setInfoValue(glDescriptor);
			glDescriptor.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			AssertReadOnlyAndValue(null, true, false, false, false, true, true, null, glDescriptor);

			setInfoValue(glDescriptor);
			glDescriptor.AJ_ReportCategory = Core.Constants.AccountType.ProfitAndLossAccount;
			AssertReadOnlyAndValue(null, true, false, false, true, true, true, null, glDescriptor);

			setInfoValue(glDescriptor);
			glDescriptor.AJ_ReportCategory = Core.Constants.AccountType.Total;
			AssertReadOnlyAndValue(null, false, false, false, true, true, false, null, glDescriptor);

			setInfoValue(glDescriptor);
			glDescriptor.AJ_ReportCategory = Core.Constants.AccountType.Header;
			AssertReadOnlyAndValue(null, true, true, true, true, false, true, null, glDescriptor);

			setInfoValue(glDescriptor);
			glDescriptor.AJ_ReportCategory = Core.Constants.AccountType.Consolidation;
			AssertReadOnlyAndValue(null, true, false, false, true, true, true, null, glDescriptor);

			setInfoValue(glDescriptor);
			glDescriptor.AJ_ReportCategory = Core.Constants.AccountType.Alternate;
			AssertReadOnlyAndValue(null, true, false, false, true, true, true, null, glDescriptor);

			setInfoValue(glDescriptor);
			glDescriptor.AJ_ReportCategory = "CFW";
			AssertReadOnlyAndValue(null, true, false, true, true, true, true, null, glDescriptor);

			setInfoValue(glDescriptor);
			glDescriptor.AJ_ReportCategory = Core.Constants.AccountType.Note;
			AssertReadOnlyAndValue(null, true, true, true, true, true, true, null, glDescriptor);
		}

		void setInfoValue(IGLAccount glAccount)
		{
			if (glAccount.ControlAccountInfo != null)
			{
				glAccount.ControlAccountInfo.Value = new ZBool(true);
			}
			if (glAccount.TotalLevelInfo != null && glAccount.TotalLevelInfo.Value.GetType() == typeof(ZInt))
			{
				glAccount.TotalLevelInfo.Value = new ZInt(1);
			}
			if (glAccount.TotalLevelInfo != null && glAccount.TotalLevelInfo.Value.GetType() == typeof(ZShort))
			{
				glAccount.TotalLevelInfo.Value = new ZShort(1);
			}
			if (glAccount.PercentNumInfo != null)
			{
				glAccount.PercentNumInfo.Value = percentNum;
			}
			if (glAccount.ConsolidationNumInfo != null)
			{
				glAccount.ConsolidationNumInfo.Value = consolidationNum;
			}
			if (glAccount.AlternateNumInfo != null)
			{
				glAccount.AlternateNumInfo.Value = alternateNum;
			}
			if (glAccount.HeaderDependsOnTotalInfo != null)
			{
				glAccount.HeaderDependsOnTotalInfo.Value = headerDependsOnTotal;
			}
			if (glAccount.CarriedForwardInfo != null)
			{
				glAccount.CarriedForwardInfo.Value = carriedForward;
			}
			if (glAccount.StatisticalUnitsInfo != null)
			{
				glAccount.StatisticalUnitsInfo.Value = new ZString("111");
			}
		}

		void AssertReadOnlyAndValue(bool? controlAccountReadOnly, bool totalLevelReadOnly, bool percentReadOnly
			, bool consolidationNumReadOnly, bool alternateNumReadOnly, bool headerDependsOnTotalReadOnly
			, bool? carriedForwardReadOnly, bool? statisticalUnitsReadOnly, IGLAccount glAccount)
		{
			AssertEquals(controlAccountReadOnly, glAccount.ControlAccountInfo?.ReadOnly);
			if (controlAccountReadOnly == null)
			{
				AssertNull(glAccount.ControlAccountInfo?.Value);
			}
			else if (controlAccountReadOnly == true)  // Nullable boolean comparison
			{
				AssertEquals(false, glAccount.ControlAccountInfo.Value);
			}
			else
			{
				AssertEquals(true, glAccount.ControlAccountInfo.Value);
			}

			AssertEquals(totalLevelReadOnly, glAccount.TotalLevelInfo.ReadOnly);
			if (totalLevelReadOnly)
			{
				AssertEquals(0, ZInt.Parse(glAccount.TotalLevelInfo.Value.ToString()));
			}
			else
			{
				AssertEquals(1, ZInt.Parse(glAccount.TotalLevelInfo.Value.ToString()));
			}

			AssertEquals(percentReadOnly, glAccount.PercentNumInfo.ReadOnly);
			if (percentReadOnly)
			{
				AssertEquals(ZGuid.Empty, glAccount.PercentNumInfo.Value);
			}
			else
			{
				AssertEquals(percentNum, glAccount.PercentNumInfo.Value);
			}

			AssertEquals(consolidationNumReadOnly, glAccount.ConsolidationNumInfo.ReadOnly);
			if (consolidationNumReadOnly)
			{
				AssertEquals(ZGuid.Empty, glAccount.ConsolidationNumInfo.Value);
			}
			else
			{
				AssertEquals(consolidationNum, glAccount.ConsolidationNumInfo.Value);
			}

			AssertEquals(alternateNumReadOnly, glAccount.AlternateNumInfo.ReadOnly);
			if (alternateNumReadOnly)
			{
				AssertEquals(ZGuid.Empty, glAccount.AlternateNumInfo.Value);
			}
			else
			{
				AssertEquals(alternateNum, glAccount.AlternateNumInfo.Value);
			}

			AssertEquals(headerDependsOnTotalReadOnly, glAccount.HeaderDependsOnTotalInfo.ReadOnly);
			if (headerDependsOnTotalReadOnly)
			{
				AssertEquals(ZGuid.Empty, glAccount.HeaderDependsOnTotalInfo.Value);
			}
			else
			{
				AssertEquals(headerDependsOnTotal, glAccount.HeaderDependsOnTotalInfo.Value);
			}

			AssertEquals(carriedForwardReadOnly, glAccount.CarriedForwardInfo?.ReadOnly);
			if (carriedForwardReadOnly == null)
			{
				AssertNull(glAccount.CarriedForwardInfo?.Value);
			}
			else if (carriedForwardReadOnly == true)  // Nullable boolean comparison
			{
				AssertEquals(ZGuid.Empty, glAccount.CarriedForwardInfo.Value);
			}
			else
			{
				AssertEquals(carriedForward, glAccount.CarriedForwardInfo.Value);
			}

			AssertEquals(statisticalUnitsReadOnly, glAccount.StatisticalUnitsInfo?.ReadOnly);
			if (statisticalUnitsReadOnly == null)
			{
				AssertNull(glAccount.StatisticalUnitsInfo?.Value);
			}
			else if (statisticalUnitsReadOnly == true)  // Nullable boolean comparison
			{
				AssertEquals(ZString.Empty, glAccount.StatisticalUnitsInfo.Value);
			}
			else
			{
				AssertEquals("111", glAccount.StatisticalUnitsInfo.Value);
			}
		}

		readonly ZGuid percentNum = ZGuid.NewZGuid();
		readonly ZGuid consolidationNum = ZGuid.NewZGuid();
		readonly ZGuid alternateNum = ZGuid.NewZGuid();
		readonly ZGuid headerDependsOnTotal = ZGuid.NewZGuid();
		readonly ZGuid carriedForward = ZGuid.NewZGuid();
	}
}
