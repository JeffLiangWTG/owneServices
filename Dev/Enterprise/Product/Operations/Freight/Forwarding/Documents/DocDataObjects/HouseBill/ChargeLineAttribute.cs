using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class ChargeLineAttribute : DocDataObject, IChargeLineAttribute
	{
		#region Name

		public ZString Name
		{
			get => name;
			set
			{
				if (SetNonPersistentPropertyValue(NameInfo, ref name, value))
				{
				}
			}
		}

		ZString name;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

		#endregion

		#region Value

		public ZString Value
		{
			get => value;
			set
			{
				if (SetNonPersistentPropertyValue(ValueInfo, ref this.value, value))
				{
				}
			}
		}

		ZString value;

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));

		#endregion

		#region Amount

		public ZDecimal Amount
		{
			get => amount;
			set
			{
				if (SetNonPersistentPropertyValue(AmountInfo, ref amount, value))
				{
				}
			}
		}

		ZDecimal amount;

		public ZPropertyInfo AmountInfo => GetZPropertyInfo(nameof(Amount));

		#endregion
	}
}
