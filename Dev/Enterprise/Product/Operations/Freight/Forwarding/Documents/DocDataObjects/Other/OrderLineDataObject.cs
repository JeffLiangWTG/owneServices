using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class OrderLineDataObject : DocDataObject
	{
		public OrderLineDataObject(object identifier) : base(identifier) { }

		#region LineNumber

		public ZInt LineNumber
		{
			get => lineNumber;
			set
			{
				if (SetNonPersistentPropertyValue(LineNumberInfo, ref lineNumber, value))
				{
					Validate();
				}
			}
		}

		ZInt lineNumber;

		public ZPropertyInfo LineNumberInfo => GetZPropertyInfo(nameof(LineNumber));

		#endregion

		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
					Validate();
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region OuterPacks

		public ZDecimal OuterPacks
		{
			get => outerPacks;
			set
			{
				if (SetNonPersistentPropertyValue(OuterPacksInfo, ref outerPacks, value))
				{
					Validate();
				}
			}
		}

		ZDecimal outerPacks;

		public ZPropertyInfo OuterPacksInfo => GetZPropertyInfo(nameof(OuterPacks));

		#endregion

		#region InnerPacks

		public ZDecimal InnerPacks
		{
			get => innerPacks;
			set
			{
				if (SetNonPersistentPropertyValue(InnerPacksInfo, ref innerPacks, value))
				{
					Validate();
				}
			}
		}

		ZDecimal innerPacks;

		public ZPropertyInfo InnerPacksInfo => GetZPropertyInfo(nameof(InnerPacks));

		#endregion

		#region TotalInnerPacks

		public ZDecimal TotalInnerPacks
		{
			get => totalInnerPacks;
			set
			{
				if (SetNonPersistentPropertyValue(TotalInnerPacksInfo, ref totalInnerPacks, value))
				{
					Validate();
				}
			}
		}

		ZDecimal totalInnerPacks;

		public ZPropertyInfo TotalInnerPacksInfo => GetZPropertyInfo(nameof(TotalInnerPacks));

		#endregion

		#region QuantityOrdered

		public Measurement QuantityOrdered
		{
			get => quantityOrdered;
			set => quantityOrdered = SetChild(quantityOrdered, value);
		}

		Measurement quantityOrdered;

		#endregion

		#region QuantityInvoiced

		public Measurement QuantityInvoiced
		{
			get => quantityInvoiced;
			set => quantityInvoiced = SetChild(quantityInvoiced, value);
		}

		Measurement quantityInvoiced;

		#endregion

		#region QuantityReceived

		public Measurement QuantityReceived
		{
			get => quantityReceived;
			set => quantityReceived = SetChild(quantityReceived, value);
		}

		Measurement quantityReceived;

		#endregion

		#region QuantityRemaining

		public Measurement QuantityRemaining
		{
			get => quantityRemaining;
			set => quantityRemaining = SetChild(quantityRemaining, value);
		}

		Measurement quantityRemaining;

		#endregion

		#region ItemPrice

		public Money ItemPrice
		{
			get => itemPrice;
			set => itemPrice = SetChild(itemPrice, value);
		}

		Money itemPrice;

		#endregion

		#region TotalLinePrice

		public Money TotalLinePrice
		{
			get => totalLinePrice;
			set => totalLinePrice = SetChild(totalLinePrice, value);
		}

		Money totalLinePrice;

		#endregion

		#region RequiredDate

		public ZDateTime RequiredDate
		{
			get => requiredDate;
			set
			{
				if (SetNonPersistentPropertyValue(RequiredDateInfo, ref requiredDate, value))
				{
					Validate();
				}
			}
		}

		ZDateTime requiredDate;

		public ZPropertyInfo RequiredDateInfo => GetZPropertyInfo(nameof(RequiredDate));

		#endregion

		#region Status

		public CodeDescription Status
		{
			get => status;
			set => status = SetChild(status, value);
		}

		CodeDescription status;

		#endregion

		#region Product

		public CodeDescription Product
		{
			get => product;
			set => product = SetChild(product, value);
		}

		CodeDescription product;

		#endregion

		#region OrderNumber

		public ZString OrderNumber
		{
			get => orderNumber;
			set
			{
				if (SetNonPersistentPropertyValue(OrderNumberInfo, ref orderNumber, value))
				{
					Validate();
				}
			}
		}

		ZString orderNumber;

		public ZPropertyInfo OrderNumberInfo => GetZPropertyInfo(nameof(OrderNumber));

		#endregion
	}
}
