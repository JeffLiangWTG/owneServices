using System;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentCustomsEntryNumberForTest : ShipmentCustomsEntryNumber
	{
		public ShipmentCustomsEntryNumberForTest(CommonShipment shipment)
			: base(shipment)
		{
		}

		public Func<ZString> GetEntryTypeImplementation { get; set; }
		protected override ZString GetEntryType()
		{
			return GetEntryTypeImplementation != null ? GetEntryTypeImplementation() : (Shipment.CusEntryNumbers.Count > 0 ? Shipment.CusEntryNumbers[0].CE_EntryType : customsEntryNumberType);
		}

		public Action<ZString> SetEntryTypeImplementation { get; set; }
		protected override void SetEntryType(ZString value)
		{
			if (SetEntryTypeImplementation != null)
			{
				SetEntryTypeImplementation(value);
			}
			else
			{
				base.SetEntryType(value);
			}
		}

		public Func<bool> EntryType_ReadOnlyImplementation { get; set; }
		protected override bool EntryType_ReadOnly
		{
			get { return EntryType_ReadOnlyImplementation != null ? EntryType_ReadOnlyImplementation() : base.EntryNumber_ReadOnly; }
		}

		public Func<ZString> GetEntryNumberImplementation { get; set; }
		protected override ZString GetEntryNumber()
		{
			return GetEntryNumberImplementation != null ? GetEntryNumberImplementation() : (Shipment.CusEntryNumbers.Count > 0 ? Shipment.CusEntryNumbers[0].CE_EntryNum : ZString.Empty);
		}

		public Action<ZString> SetEntryNumberImplementation { get; set; }
		protected override void SetEntryNumber(ZString value)
		{
			if (SetEntryNumberImplementation != null)
			{
				SetEntryNumberImplementation(value);
			}
			else
			{
				base.SetEntryNumber(value);
			}
		}

		public Func<ZDateTime> GetIssueDateImplementation { get; set; }
		protected override ZDateTime GetIssueDate()
		{
			return GetIssueDateImplementation != null ? GetIssueDateImplementation() : (Shipment.CusEntryNumbers.Count > 0 ? Shipment.CusEntryNumbers[0].CE_IssueDate : ZDateTime.Empty);
		}

		public Action<ZDateTime> SetIssueDateImplementation { get; set; }
		protected override void SetIssueDate(ZDateTime value)
		{
			if (SetIssueDateImplementation != null)
			{
				SetIssueDateImplementation(value);
			}
			else
			{
				base.SetIssueDate(value);
			}
		}

		public Func<ZDateTime> GetExpiryDateImplementation { get; set; }
		protected override ZDateTime GetExpiryDate()
		{
			return GetExpiryDateImplementation != null ? GetExpiryDateImplementation() : (Shipment.CusEntryNumbers.Count > 0 ? Shipment.CusEntryNumbers[0].CE_ExpiryDate : ZDateTime.Empty);
		}

		public Action<ZDateTime> SetExpiryDateImplementation { get; set; }
		protected override void SetExpiryDate(ZDateTime value)
		{
			if (SetExpiryDateImplementation != null)
			{
				SetExpiryDateImplementation(value);
			}
			else
			{
				base.SetExpiryDate(value);
			}
		}

		public Func<CusEntryNumber> GetCusEntryNumberImplementation { get; set; }
		protected override CusEntryNumber GetCusEntryNumber()
		{
			return GetCusEntryNumberImplementation != null ? GetCusEntryNumberImplementation() : (Shipment.CusEntryNumbers.Count > 0 ? Shipment.CusEntryNumbers[0] : Shipment.CusEntryNumbers.AddNew());
		}

		public new bool IsEntryNumberInAustralia
		{
			get { return base.IsEntryNumberInAustralia; }
		}
	}
}
