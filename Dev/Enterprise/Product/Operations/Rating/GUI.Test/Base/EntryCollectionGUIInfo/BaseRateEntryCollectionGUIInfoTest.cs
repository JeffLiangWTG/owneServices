using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI.Testing
{
	public abstract class BaseRateEntryCollectionGUIInfoTest : RatingTestCase
	{
		protected abstract Type GUIInfoType { get; }
		protected abstract string RateCategory { get; }

		public virtual void TestIsType()
		{
			var info = RateEntryCollectionGUIInfo.GetInfo(RateCategory, typeof(ClientRate), false);
			AssertType("Pre-condition: info should be of the expectedType", GUIInfoType, info);
			Assert(info.IsClientRate);
			Assert(!info.IsTariff);

			info = RateEntryCollectionGUIInfo.GetInfo(RateCategory, typeof(CompanyTariff), false);
			AssertType("Pre-condition: info should be of the expectedType", GUIInfoType, info);
			Assert(!info.IsClientRate);
			Assert(info.IsTariff);
		}

		public virtual void TestTransportModeFilterAvailability()
		{
			var info = RateEntryCollectionGUIInfo.GetInfo(RateCategory, typeof(ClientRate), false);
			var transportModeColumnExists = info.Columns.Any(c => c.ColumnName == RateEntry.Schema.TI_Mode);

			AssertEquals(
				"Transport Mode filter should be available when Mode column exists on the RateEntryCollectionGUIInfo",
				RateEntryFilterProvider.CategoriesWithTransportModeColumn.Contains(RateCategory),
				transportModeColumnExists);
		}

		public virtual void TestSupplierFilterAvailabilityOnClientRate()
		{
			var info = RateEntryCollectionGUIInfo.GetInfo(RateCategory, typeof(ClientRate), false);
			var supplierColumnExists = info.Columns.Any(c => c.ColumnName == RateEntry.Schema.TI_OH_Supplier);

			AssertEquals(
				"Service Provider filter should be available when Supplier column exists on the RateEntryCollectionGUIInfo",
				!RateEntryFilterProvider.CategoriesWithoutSupplierColumn.Contains(RateCategory),
				supplierColumnExists);
		}

		public virtual void TestSupplierFilterAvailabilityOnCompanyTariff()
		{
			var info = RateEntryCollectionGUIInfo.GetInfo(RateCategory, typeof(CompanyTariff), false);
			var supplierColumnExists = info.Columns.Any(c => c.ColumnName == RateEntry.Schema.TI_OH_Supplier);

			AssertEquals(
				"Service Provider filter should be available when Supplier column exists on the RateEntryCollectionGUIInfo",
				!RateEntryFilterProvider.CategoriesWithoutSupplierColumn.Contains(RateCategory),
				supplierColumnExists);
		}

		public virtual void TestSupplierFilterAvailabilityOnQuote()
		{
			var info = RateEntryCollectionGUIInfo.GetInfo(RateCategory, typeof(Quote), false);
			var supplierColumnExists = info.Columns.Any(c => c.ColumnName == RateEntry.Schema.TI_OH_Supplier);

			AssertEquals(
				"Service Provider filter should be available when Supplier column exists on the RateEntryCollectionGUIInfo",
				!RateEntryFilterProvider.CategoriesWithoutSupplierColumn.Contains(RateCategory),
				supplierColumnExists);
		}

		public abstract void TestClientRateColumns();
		public abstract void TestQuotationRateColumns();
		public abstract void TestStandardCostingRateColumns();

		protected (string[] NormalUserColumns, string[] SupportUserColumns, RateEntryCollectionGUIInfo Infos)
					GetInfos(string rateCategory, Type dataSourceType, bool isGlobal, bool isStandardCosting = false)
		{
			GlbStaff.CurrentUser.GS_LoginName = "Jeeves";
			var infos = RateEntryCollectionGUIInfo.GetInfo(rateCategory, dataSourceType, isGlobal, isStandardCosting);
			var normalUserColumns = infos
				.Columns
				.Select(c => c.ColumnName)
				.ToArray();
			GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
			var supportUserColumns = infos
				.Columns
				.Select(c => c.ColumnName)
				.ToArray();

			return (normalUserColumns, supportUserColumns, infos);
		}

		protected void AssertCollectionColumnsExistInExpectedOrder(string[] observedColumnNames, string expected, string extraExpected) =>
			AssertCollectionColumnsExistInExpectedOrder(observedColumnNames, expected + extraExpected.Trim());

		protected void AssertCollectionColumnsExistInExpectedOrder(string[] observedColumnNames, string expectedColumns)
		{
			var message = "Columns do not appear in the expected order.";
			expectedColumns = expectedColumns.Trim();

			var builder = new ZStringBuilder();
			foreach (var columnName in observedColumnNames)
			{
				builder.AppendLine(columnName);
			}

			var actualColumns = builder.ToString();
			AssertMultilineASCIIEquals(message, expectedColumns, actualColumns);

			var splitBy = new string[] { System.Environment.NewLine };
			var duplicateColumns = actualColumns.Split(splitBy, StringSplitOptions.None)
				.GroupBy(x => x)
				.Where(x => x.Count() > 1)
				.Select(x => x.Key);

			message = $"Column appears more than once in the collection: " + string.Join(",", duplicateColumns);
			AssertEquals(message, false, duplicateColumns.Any());
		}

		protected void AssertColumnType<TExpectedType>(RateEntryCollectionGUIInfo info, string columnName)
			where TExpectedType : ZGridColumnInfo
		{
			AssertNotNull($"The {columnName} column is presented as a {typeof(TExpectedType).Name}",
				info.Columns
				.OfType<TExpectedType>()
				.Single(c => c.ColumnName == columnName));
		}

		protected void AssertContractNumberColumnStyle<T>(RateEntryCollectionGUIInfo info)
			where T : ZGridColumnInfo
		{
			AssertColumnType<T>(info, AutoRateEntry.Schema.TI_ContractNumber);
		}

		protected void TestContractNumberColumnStyle(RateEntryCollectionGUIInfo info)
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			if (IsWinzor)
			{
				AssertContractNumberColumnStyle<ZTextBoxColumnStyleInfo>(info);
			}
			else
			{
				AssertContractNumberColumnStyle<ContractNumberFindBoxColumnStyleInfo>(info);
			}

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			if (IsWinzor && !info.IsCosting)
			{
				AssertContractNumberColumnStyle<ZTextBoxColumnStyleInfo>(info);
			}
			else
			{
				AssertContractNumberColumnStyle<ContractNumberFindBoxColumnStyleInfo>(info);
			}

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertContractNumberColumnStyle<ZTextBoxColumnStyleInfo>(info);
		}

#if WINZOR
		protected bool IsWinzor => true;
#else
		protected bool IsWinzor => false;
#endif
	}
}
