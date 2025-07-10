using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	public class WhsBondedWarehouseTransaction : WhsWarehouseTransaction, IWhsBondedWarehouseTransaction
	{
		#region Copy

		public static WhsBondedWarehouseTransaction Copy(IWhsBondedWarehouseTransaction header)
		{
			// problems are not copied
			WhsBondedWarehouseTransaction copyTo = CopyExceptLines(header);
			copyTo.Lines = header.Lines.Clone();
			return copyTo;
		}

		public static WhsBondedWarehouseTransaction CopyExceptLines(IWhsBondedWarehouseTransaction header)
		{
			// problems are not copied
			var copyTo = new WhsBondedWarehouseTransaction();
			copyTo.AdditionalReferences = header.AdditionalReferences;
			copyTo.Date = header.Date;
			copyTo.Client = (OrgHeader)header.Client;
			copyTo.IsWarehousedByExternalAgent = header.IsWarehousedByExternalAgent;
			copyTo.TransportCompany = (OrgHeader)header.TransportCompany;
			copyTo.Warehouse = (OrgAddress)header.Warehouse;
			copyTo.Reference = header.Reference;
			return copyTo;
		}

		#endregion

		public OrgAddress Warehouse { get; set; }

		public IEnumerable<AdditionalReference> AdditionalReferences { get; set; }

		public bool IsWarehousedByExternalAgent { get; set; }

		public new IWhsBondedWarehouseTransactionLineCollection Lines
		{
			get
			{
				WhsBondedWarehouseTransactionLineCollection result;

				if (lines == null)
				{
					result = new WhsBondedWarehouseTransactionLineCollection();
				}
				else
				{
					result = (lines as WhsBondedWarehouseTransactionLineCollection);
					if (result == null)
					{
						result = new WhsBondedWarehouseTransactionLineCollection();
						foreach (IWhsWarehouseTransactionLine line1 in lines)
						{
							IWhsBondedWarehouseTransactionLine line = line1 as IWhsBondedWarehouseTransactionLine;
							if (line != null)
							{
								result.Add(line);
							}
						}
					}
				}
				lines = result;
				return result;
			}
			set
			{
				lines = value;
			}
		}

		#region IWhsBondedWarehouseTransaction Members

		IEnumerable<AdditionalReference> IWhsBondedWarehouseTransaction.AdditionalReferences
		{
			get { return AdditionalReferences; }
		}

		bool IWhsBondedWarehouseTransaction.IsWarehousedByExternalAgent
		{
			get { return IsWarehousedByExternalAgent; }
		}

		IWhsBondedWarehouseTransactionLineCollection IWhsBondedWarehouseTransaction.Lines
		{
			get { return Lines; }
		}

		IOrgAddress IWhsBondedWarehouseTransaction.Warehouse
		{
			get { return Warehouse; }
		}

		#endregion
	}
}
