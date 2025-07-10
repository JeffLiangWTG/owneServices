using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class LumpSum : DocDataObject, IMoney
	{
		#region Constructor

		public LumpSum(Money money)
		{
			this.money = SetChild(null, money ?? new Money());
		}

		readonly Money money;

		#endregion

		#region Amount

		public ZDecimal Amount
		{
			get => money.Amount;
			set => money.Amount = value;
		}

		public ZPropertyInfo AmountInfo => GetWrappedZPropertyInfo(nameof(Amount), _ => money.AmountInfo);

		#endregion

		#region Currency

		public ICodeDescription Currency => money.Currency;

		#endregion

		#region ToString

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"FREIGHT LUMP SUM: {0:0.00} {1}", Amount, Currency?.Code);
		}

		#endregion
	}
}
