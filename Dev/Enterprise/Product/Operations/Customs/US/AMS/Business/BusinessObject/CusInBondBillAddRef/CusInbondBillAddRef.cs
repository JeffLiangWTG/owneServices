using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusInbondBillAddRef : Customs.Business.CusInbondBillAddRef,
		IShipmentReferenceDetail,
		ICanDelete,
		Customs.Business.ISynchroniserReadOnlyMembersProvider,
		Integration.Customs.US.USAMS.ICusInbondBillAddRef
	{
		public CusInbondBillAddRef(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		public ZString BR_QualifierDescription
		{
			get { return Lookups.ReferenceList.GetDescriptionFromCode(BR_Qualifier) ?? ZString.Empty; }
		}

		public ZBool IsOceanBill
		{
			get { return BR_Qualifier == BillReferenceList.Codes.OB; }
		}

		#endregion

		#region Override Properties

		#region BR_B0

		[RelatedBusinessObject("Bill")]
		public override ZGuid BR_B0
		{
			get { return base.BR_B0; }
			set { base.BR_B0 = value; }
		}

		public CusInBondBill Bill
		{
			get { return Factory.Load<CusInBondBill>(BR_B0); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusInbondBillAddRefLookups.ReferenceList))]
		public override ZString BR_Qualifier
		{
			get { return base.BR_Qualifier; }
			set
			{
				var oldValue = BR_Qualifier;
				base.BR_Qualifier = value;
				if (!IsCopying && oldValue != BR_Qualifier)
				{
					var info = GetBillInfoMatchingQualifier();
					if (info != null)
					{
						var maxLength = info.MaxLength;
						if (maxLength > 0 && BR_ReferenceNum.Length > maxLength)
						{
							BR_ReferenceNum = BR_ReferenceNum.Left(maxLength);
						}
					}
					UpdateRefreshBillData(BR_ReferenceNum);

					var bill = Bill;
					if (bill != null)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString BR_ReferenceNum
		{
			get { return base.BR_ReferenceNum; }
			set
			{
				var oldValue = BR_ReferenceNum;
				if (!IsCopying)
				{
					var info = GetBillInfoMatchingQualifier();
					if (info != null)
					{
						var maxLength = info.MaxLength;
						if (maxLength > 0 && value.Length > maxLength)
						{
							value = value.Left(maxLength);
						}
					}
				}
				base.BR_ReferenceNum = value;
				if (!IsCopying && oldValue != BR_ReferenceNum)
				{
					UpdateRefreshBillData(oldValue);
				}
			}
		}

		public new CusInbondBillAddRefLookups Lookups
		{
			get { return (CusInbondBillAddRefLookups)base.Lookups; }
		}

		public new CusInbondBillAddRefValidation Validation
		{
			get { return (CusInbondBillAddRefValidation)base.Validation; }
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region Related Objects

		public CusInBondHeader Header
		{
			get
			{
				var bill = Bill;
				return bill == null ? null : bill.Header;
			}
		}

		#endregion

		#region Implementation

		void UpdateRefreshBillData(ZString oldValue)
		{
			var bill = Bill;
			if (bill != null)
			{
				var info = GetBillInfoMatchingQualifier();
				if (info != null)
				{
					if (oldValue != BR_ReferenceNum)
					{
						info.RefreshBinding(oldValue);
					}
					else
					{
						info.RefreshBinding();
					}
					bill.RefreshBinding();
				}
			}
		}

		ZPropertyInfo GetBillInfoMatchingQualifier()
		{
			ZPropertyInfo info = null;
			var bill = Bill;
			if (bill != null)
			{
				switch (BR_Qualifier)
				{
					case BillReferenceList.Codes.ULC:
						info = bill.B0_PlaceOfDeliveryInfo;
						break;
					case BillReferenceList.Codes.CSK:
						info = bill.B0_ForeignPortOfUnladingKCodeInfo;
						break;
				}
			}
			return info;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override Customs.Business.CusInbondBillAddRefLookups GetNewLookups()
		{
			return new CusInbondBillAddRefLookups(this);
		}

		protected override Customs.Business.CusInbondBillAddRefValidation GetNewValidation()
		{
			return new CusInbondBillAddRefValidation(this);
		}

		#endregion

		#region IShipmentReferenceDetail Members

		ZString IShipmentReferenceDetail.Qualifier
		{
			get { return BR_Qualifier; }
		}

		ZString IShipmentReferenceDetail.ReferenceIdentifier
		{
			get { return BR_ReferenceNum; }
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get
			{
				var header = Header;
				return header != null && (!IsOceanBill || (IsOceanBill && !BR_QualifierInfo.ReadOnly) || !header.ShouldSynchronise);
			}
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ReasonForCannotDeleteOceanBillOfLading; }
		}

		internal static MultilingualString ReasonForCannotDeleteOceanBillOfLading
		{
			get { return ResString.GetMultilingualString("AMS|CusInbondBillAddRef|D97A7B1B-EC8C-4AC8-9FAF-DF64FC40357E", "Ocean Bill Of Lading values are copied from consol. If you want to delete this record, please do it in consol. Or you should tick 'Override Freight Defaults'."); }
		}

		#endregion

	}
}
