using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class CusWHSOperatorTransactionNettOffProcedure
	{
		public const string QualifiedName = "dbo.CusWHSOperatorTransactionNettOff";

		class Parameters
		{
			public const string Company = "@Company";
			public const string WarehouseAddress = "@WarehouseAddress";
			public const string ProductOwner = "@ProductOwner";
			public const string ExportType = "@ExportType";
			public const string OwnerReferences = "@OwnerReferences";
			public const string SortAscending = "@SortAscending";
			public const string CurrentUser = "@CurrentUser";
			public const string CurrentTimeUtc = "@CurrentTimeUtc";
		}

		class Columns
		{
			public const string PK = "WOL_PK";
			public const string Order = "WOL_WOT_WHSOperatorTransactionOrder";
			public const string Receipt = "WOL_WOT_WHSOperatorTransactionReceipt";
			public const string Quantity = "WOL_Quantity";
			public const string EntryLineNo = "WOL_CustomsEntryLineNo";
		}

		public class TransactionLine
		{
			public TransactionLine(Guid pk, Guid orderPK, Guid receiptPK, decimal quantity, short entryLineNo)
			{
				PK = pk;
				OrderPK = orderPK;
				ReceiptPK = receiptPK;
				Quantity = quantity;
				EntryLineNo = entryLineNo;
			}

			public Guid PK { get; }
			public Guid OrderPK { get; }
			public Guid ReceiptPK { get; }
			public decimal Quantity { get; }
			public short EntryLineNo { get; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Calling a stored proc which does not return a business object")]
		public static IReadOnlyCollection<TransactionLine> Execute(BusinessObjectFactory factory, Guid company, Guid warehouseAddress,
			Guid productOwner, string exportType, IEnumerable<string> ownerReferences, bool sortAscending, string userCode)
		{
			var results = new List<TransactionLine>();
			var sql = FormattableString.Invariant($"EXEC {QualifiedName} {Parameters.Company}, {Parameters.WarehouseAddress}, {Parameters.ProductOwner}, {Parameters.ExportType}, {Parameters.OwnerReferences}, {Parameters.SortAscending}, {Parameters.CurrentUser}, {Parameters.CurrentTimeUtc}");
			using (var cmd = ((IDbConnected)factory).Connection.Command(sql))
			{
				cmd.AddParameter(Parameters.Company, SqlDbType.UniqueIdentifier, 0, company);
				cmd.AddParameter(Parameters.WarehouseAddress, SqlDbType.UniqueIdentifier, 0, warehouseAddress);
				cmd.AddParameter(Parameters.ProductOwner, SqlDbType.UniqueIdentifier, 0, productOwner);
				cmd.AddParameter(Parameters.ExportType, SqlDbType.VarChar, 3, exportType);
				cmd.AddTableValuedParameter(Parameters.OwnerReferences, "TVP_Varchar_35", ownerReferences.Distinct());
				cmd.AddParameter(Parameters.SortAscending, SqlDbType.Bit, 1, sortAscending);
				cmd.AddParameter(Parameters.CurrentUser, SqlDbType.VarChar, userCode);
				cmd.AddParameter(Parameters.CurrentTimeUtc, SqlDbType.SmallDateTime, ZDateTime.Now);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader[Columns.PK];
						var order = (Guid)reader[Columns.Order];
						var receipt = (Guid)reader[Columns.Receipt];
						var qty = (decimal)reader[Columns.Quantity];
						var entryLineNo = (short)reader[Columns.EntryLineNo];

						results.Add(new TransactionLine(pk, order, receipt, qty, entryLineNo));
					}
				}
			}

			return results;
		}
	}
}
