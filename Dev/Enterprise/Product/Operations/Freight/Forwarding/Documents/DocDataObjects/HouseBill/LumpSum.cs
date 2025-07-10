using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class LumpSum : DocDataObject, IMoney
	{
		#region Ctor

		public LumpSum(Money money)
		{
			this.money = SetChild(null, money ?? new Money());
		}

		readonly Money money;

		#endregion

		public ZDecimal Amount
		{
			get => money.Amount;
			set => money.Amount = value;
		}

		public ZPropertyInfo AmountInfo => GetWrappedZPropertyInfo(nameof(Amount), _ => money.AmountInfo);

		public ICodeDescription Currency => money.Currency;

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"FREIGHT LUMP SUM: {0:0.00} {1}", Amount, Currency?.Code); // no need for translation as HBLs are english only
		}
	}
}
