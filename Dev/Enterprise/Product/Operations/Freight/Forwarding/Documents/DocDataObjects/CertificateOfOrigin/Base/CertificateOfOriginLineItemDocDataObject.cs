using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base
{
	public abstract class CertificateOfOriginLineItemDocDataObject : DocDataObject
	{
		protected CertificateOfOriginLineItemDocDataObject(object id) : base(id)
		{
		}

		#region IsMarksAndNumbersEditable

		public ZBool IsMarksAndNumbersEditable
		{
			get => isMarksAndNumbersEditable;
			set
			{
				if(SetNonPersistentPropertyValue(IsMarksAndNumbersEditableInfo, ref isMarksAndNumbersEditable, value))
				{
				}
			}
		}

		ZBool isMarksAndNumbersEditable;

		public ZPropertyInfo IsMarksAndNumbersEditableInfo => GetZPropertyInfo(nameof(IsMarksAndNumbersEditable));

		#endregion

		#region ItemNumber

		public ZShort ItemNumber
		{
			get => itemNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ItemNumberInfo, ref itemNumber, value))
				{
					Validate(ItemNumberInfo);
				}
			}
		}

		ZShort itemNumber;

		public ZPropertyInfo ItemNumberInfo => GetZPropertyInfo(nameof(ItemNumber));

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get => marksAndNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersInfo, ref marksAndNumbers, value))
				{
					Validate(MarksAndNumbersInfo);
				}
			}
		}

		ZString marksAndNumbers;

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}

		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		#region PackageCount

		public ZInt PackageCount
		{
			get => packageCount;
			set
			{
				if (SetNonPersistentPropertyValue(PackageCountInfo, ref packageCount, value))
				{
					Validate(PackageCountInfo);
				}
			}
		}

		ZInt packageCount;

		public ZPropertyInfo PackageCountInfo => GetZPropertyInfo(nameof(PackageCount));

		#endregion

		#region PackageType

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}

		ICodeDescription packageType;

		#endregion

		#region HarmonizedCode

		public HarmonizedCode HarmonizedCode
		{
			get => harmonizedCode;
			set => harmonizedCode = SetChild(harmonizedCode, value);
		}

		HarmonizedCode harmonizedCode;

		#endregion

		#region OriginCriterion

		public ICodeDescription OriginCriterion
		{
			get => originCriterion;
			set => originCriterion = SetChild(originCriterion, value);
		}

		ICodeDescription originCriterion;

		#endregion

		#region OriginCode

		public ZString OriginCode
		{
			get => originCode;
			set
			{
				if (SetNonPersistentPropertyValue(OriginCodeInfo, ref originCode, value))
				{
					Validate(OriginCodeInfo);
				}
			}
		}

		ZString originCode;

		public ZPropertyInfo OriginCodeInfo => GetZPropertyInfo(nameof(OriginCode));

		#endregion

		#region Origin

		public ZString Origin
		{
			get => origin;
			set
			{
				if (SetNonPersistentPropertyValue(OriginInfo, ref origin, value))
				{
					Validate(OriginInfo);
				}
			}
		}

		ZString origin;

		public ZPropertyInfo OriginInfo => GetZPropertyInfo(nameof(Origin));

		#endregion

		#region Quantity

		public IMeasurement Quantity
		{
			get => quantity;
			set
			{
				quantity = SetChild(quantity, value);
			}
		}

		IMeasurement quantity;

		#endregion

		#region QuantityNet

		public Measurement QuantityNet
		{
			get => quantityNet;
			set
			{
				quantityNet = SetChild(quantityNet, value);
			}
		}

		Measurement quantityNet;

		#endregion

		#region Invoice

		public Invoice Invoice
		{
			get => invoice;
			set => invoice = SetChild(invoice, value);
		}

		Invoice invoice;

		#endregion
	}
}
