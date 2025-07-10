using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class Money : DocDataObject, IMoney
	{
		#region Constructor

		public Money()
		{
		}

		public Money(IContext context)
		{
			Currency = new CodeDescription((context?.Currencies as IFindBoxListProvider));
		}

		#endregion

		#region Amount

		public ZDecimal Amount
		{
			get => amount;
			set
			{
				if (SetNonPersistentPropertyValue(AmountInfo, ref amount, value))
				{
					Validate(AmountInfo);
				}
			}
		}

		ZDecimal amount;

		public ZPropertyInfo AmountInfo => GetZPropertyInfo(nameof(Amount));

		#endregion

		#region Currency

		public ICodeDescription Currency
		{
			get => currency;
			set => currency = SetChild(currency, value);
		}

		ICodeDescription currency;

		#endregion
	}
}
