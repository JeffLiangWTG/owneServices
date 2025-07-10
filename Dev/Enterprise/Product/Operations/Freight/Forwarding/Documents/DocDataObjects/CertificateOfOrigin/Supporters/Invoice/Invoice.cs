using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice
{
	public class Invoice : DocDataObject
	{
		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if(SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}

		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region Date

		public ZDateTime Date
		{
			get => date;
			set
			{
				if(SetNonPersistentPropertyValue(DateInfo, ref date, value))
				{
					Validate(DateInfo);
				}
			}
		}

		ZDateTime date;

		public ZPropertyInfo DateInfo => GetZPropertyInfo(nameof(Date));

		#endregion

		#region Amount

		public Money Amount
		{
			get => amount;
			set => amount = SetChild(amount, value);
		}

		Money amount;

		#endregion
	}
}
