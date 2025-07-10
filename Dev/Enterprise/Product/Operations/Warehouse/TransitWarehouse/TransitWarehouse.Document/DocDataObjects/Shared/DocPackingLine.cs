using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects
{
	public class DocPackingLine : DocDataObject
	{
		public DocPackingLine(object identifier) : base(identifier)
		{
		}

		#region RefType

		public ICodeDescription RefType
		{
			get => refType;
			set => refType = SetChild(refType, value);
		}

		ICodeDescription refType;

		#endregion

		#region RefCode

		public ZString RefCode
		{
			get => refCode;
			set
			{
				value = value.RemoveHyphen();
				if (SetNonPersistentPropertyValue(RefCodeInfo, ref refCode, value))
				{
					Validate(RefCodeInfo);
				}
			}
		}

		ZString refCode;

		public ZPropertyInfo RefCodeInfo => GetZPropertyInfo(nameof(RefCode));

		#endregion

		#region SourceType

		public ZString SourceType
		{
			get => sourceType;
			set => sourceType = value;
		}

		ZString sourceType;

		#endregion

		#region SourceID

		public ZString SourceID
		{
			get => sourceID;
			set => sourceID = value;
		}

		ZString sourceID;

		#endregion

		#region AmountQuantity

		public ZInt AmountQuantity
		{
			get => amountQuantity;
			set
			{
				if (SetNonPersistentPropertyValue(AmountQuantityInfo, ref amountQuantity, value) || value == 0)
				{
					Validate(AmountQuantityInfo);
					Validate(AmountWeightInfo);
				}
			}
		}
		ZInt amountQuantity;
		public ZPropertyInfo AmountQuantityInfo => GetZPropertyInfo(nameof(AmountQuantity));

		#endregion

		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
					Validate(DescriptionInfo);
				}
			}
		}
		ZString description;
		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region AmountWeight

		public ZDecimal AmountWeight
		{
			get => amountWeight;
			set
			{
				var validWeight = value.RoundTo3Digits();
				if (SetNonPersistentPropertyValue(AmountWeightInfo, ref amountWeight, validWeight) || validWeight == 0m)
				{
					Validate(AmountWeightInfo);
					Validate(AmountQuantityInfo);
				}
			}
		}
		ZDecimal amountWeight;
		public ZPropertyInfo AmountWeightInfo => GetZPropertyInfo(nameof(AmountWeight));

		#endregion

		#region AccompanyDocumentType

		public ZString AccompanyDocumentType
		{
			get => accompanyDocumentType;
			set
			{
				if (SetNonPersistentPropertyValue(AccompanyDocumentTypeInfo, ref accompanyDocumentType, value))
				{
					Validate(AccompanyDocumentTypeInfo);
				}
			}
		}
		ZString accompanyDocumentType;
		public ZPropertyInfo AccompanyDocumentTypeInfo => GetZPropertyInfo(nameof(AccompanyDocumentType));

		#endregion

		#region AccompanyDocumentRef

		public ZString AccompanyDocumentRef
		{
			get => accompanyDocumentRef;
			set
			{
				if (SetNonPersistentPropertyValue(AccompanyDocumentRefInfo, ref accompanyDocumentRef, value))
				{
					Validate(AccompanyDocumentRefInfo);
				}
			}
		}
		ZString accompanyDocumentRef;
		public ZPropertyInfo AccompanyDocumentRefInfo => GetZPropertyInfo(nameof(AccompanyDocumentRef));

		#endregion

		#region TemporaryStorageDeclaration

		public ZString TemporaryStorageDeclaration
		{
			get => temporaryStorageDeclaration;
			set
			{
				if (SetNonPersistentPropertyValue(TemporaryStorageDeclarationInfo, ref temporaryStorageDeclaration, value))
				{
					Validate(TemporaryStorageDeclarationInfo);
				}
			}
		}
		ZString temporaryStorageDeclaration;
		public ZPropertyInfo TemporaryStorageDeclarationInfo => GetZPropertyInfo(nameof(TemporaryStorageDeclaration));

		#endregion

		#region SourceReceiveConsignment

		public WhsItemReceiveConsignment SourceReceiveConsignment { get; set; }

		#endregion

		#region MessageID

		public ZString DeconsMessageID
		{
			get => deconsMessageID;
			set => deconsMessageID = value;
		}

		ZString deconsMessageID;

		#endregion
	}
}
