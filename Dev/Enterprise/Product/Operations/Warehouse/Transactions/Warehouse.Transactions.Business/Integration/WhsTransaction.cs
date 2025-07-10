using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWarehouseTransaction : IWhsWarehouseTransaction
	{
		#region Properties

		public OrgHeader Client { get; set; }

		public ZDateTime Date { get; set; }

		public ZGuid ExternalPK { get; set; }

		public ZString Reference { get; set; }

		public OrgHeader TransportCompany { get; set; }

		protected IWhsWarehouseTransactionLineCollection lines;
		public IWhsWarehouseTransactionLineCollection Lines
		{
			get { return lines ?? (lines = new WhsWarehouseTransactionLineCollection()); }
			set { lines = value; }
		}

		NotificationCollection problems;
		public NotificationCollection Problems
		{
			get { return problems ?? (problems = new NotificationCollection()); }
		}

		public bool HasErrors
		{
			get
			{
				bool result = Problems.HasErrors;
				if (!result && Lines != null)
				{
					foreach (IWhsWarehouseTransactionLine line in Lines)
					{
						if (line.HasErrors)
						{
							return true;
						}
					}
				}
				return result;
			}
		}

		public bool HasWarnings
		{
			get
			{
				bool result = Problems.HasWarnings;
				if (!result && Lines != null)
				{
					foreach (IWhsWarehouseTransactionLine line in Lines)
					{
						if (line.HasWarnings)
						{
							return true;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region Clear Notifications

		public void ClearAllNotifications()
		{
			Problems.ErrorList.Clear();
			Problems.WarningList.Clear();

			if (Lines != null)
			{
				foreach (IWhsWarehouseTransactionLine line in Lines)
				{
					((WhsWarehouseTransactionLine)line).ClearAllNotifications();
				}
			}
		}

		#endregion

		#region IWhsWarehouseTransaction Members

		IOrgHeader IWhsWarehouseTransaction.Client
		{
			get { return Client; }
		}

		ZDateTime IWhsWarehouseTransaction.Date
		{
			get { return Date; }
		}

		ZGuid IWhsWarehouseTransaction.ExternalPK
		{
			get { return ExternalPK; }
		}

		bool IWhsWarehouseTransaction.HasErrors
		{
			get { return HasErrors; }
		}

		bool IWhsWarehouseTransaction.HasWarnings
		{
			get { return HasWarnings; }
		}

		IWhsWarehouseTransactionLineCollection IWhsWarehouseTransaction.Lines
		{
			get { return Lines; }
		}

		NotificationCollection IWhsWarehouseTransaction.Problems
		{
			get { return Problems; }
		}

		ZString IWhsWarehouseTransaction.Reference
		{
			get { return Reference; }
		}

		IOrgHeader IWhsWarehouseTransaction.TransportCompany
		{
			get { return TransportCompany; }
		}

		#endregion
	}
}
