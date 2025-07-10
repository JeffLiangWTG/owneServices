using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCollectionCall : Autovw_OrgCollectionCall
	{
		public OrgCollectionCall(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		#region Header

		OrgHeader fHeader;
		public OrgHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, CC_OH));
					if (fHeader != null)
					{
						fHeader.IsLoadedFromCollectionCall = ZBool.True;
					}
				}
				return fHeader;
			}
		}

		#endregion

		#endregion

		#region Properties

		[List("Lookups.Branches")]
		public override ZGuid CC_OB_GB_ControllingBranch
		{
			get { return base.CC_OB_GB_ControllingBranch; }
			set { base.CC_OB_GB_ControllingBranch = value; }
		}

		[List("Lookups.DebtorGroups")]
		public override ZGuid CC_OB_OJ_ARDebtorGroup
		{
			get { return base.CC_OB_OJ_ARDebtorGroup; }
			set { base.CC_OB_OJ_ARDebtorGroup = value; }
		}

		[List("Lookups.Contacts")]
		public override ZGuid CC_OC
		{
			get { return base.CC_OC; }
			set { base.CC_OC = value; }
		}

		[EmailAddress]
		public override ZString CC_OC_Email
		{
			get
			{
				return base.CC_OC_Email;
			}

			set
			{
				base.CC_OC_Email = value;
			}
		}

		#region CC_AvgDaysOverdue

		public ZInt CC_AvgDaysOverdue
		{
			get
			{
				if (!CC_AvgDueDate.IsEmpty)
				{
					TimeSpan diff = ZDateTime.Now.ToDateTime().Subtract(CC_AvgDueDate.ToDateTime());
					return ((ZInt)diff.Days) > 0 ? (ZInt)diff.Days : ZInt.Zero;
				}
				else
				{
					return ZInt.Zero;
				}
			}
		}

		public ZPropertyInfo CC_AvgDaysOverdueInfo
		{
			get { return GetZPropertyInfo(nameof(CC_AvgDaysOverdue)); }
		}

		#endregion

		#region CC_MaxDaysOverdue

		public ZInt CC_MaxDaysOverdue
		{
			get
			{
				if (!CC_OldestDueDate.IsEmpty)
				{
					TimeSpan diff = ZDateTime.Now.ToDateTime().Subtract(CC_OldestDueDate.ToDateTime());
					return ((ZInt)diff.Days) > 0 ? (ZInt)diff.Days : ZInt.Zero;
				}
				else
				{
					return ZInt.Zero;
				}
			}
		}

		public ZPropertyInfo CC_MaxDaysOverdueInfo
		{
			get { return GetZPropertyInfo(nameof(CC_MaxDaysOverdue)); }
		}

		#endregion

		public ZString AdditionalCompanyName => Header.GetAdditionalCompanyName(OrgAddressType.Receivables);

		#region Collection Notes

		public OrgCollectionNoteCollection CollectionNotes
		{
			get
			{
				if (Header != null)
				{
					Header.LoadCollectionNotesWithFiltering();
					return Header.CollectionNotes;
				}
				else
				{
					if (fCollectionNotes == null)
					{
						fCollectionNotes = new OrgCollectionNoteCollection(Factory);
					}
					return fCollectionNotes;
				}
			}
		}
		OrgCollectionNoteCollection fCollectionNotes;

		#endregion

		#endregion

		#region Decimal Places

		public int DecimalPlaces => GlbCompany.CurrentCompany.GetLocalDecimals();

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_AvailableCredit
		{
			get
			{
				return base.CC_AvailableCredit;
			}
			set
			{
				base.CC_AvailableCredit = value;
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_CreditLimit
		{
			get
			{
				return base.CC_CreditLimit;
			}
			set
			{
				base.CC_CreditLimit = value;
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_DisbursementOutstandingAmount
		{
			get
			{
				return base.CC_DisbursementOutstandingAmount;
			}
			set
			{
				base.CC_DisbursementOutstandingAmount = value;
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_DisbursementOverdueAmount
		{
			get
			{
				return base.CC_DisbursementOverdueAmount;
			}
			set
			{
				base.CC_DisbursementOverdueAmount = value;
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_LastReceiptAmount
		{
			get
			{
				return base.CC_LastReceiptAmount;
			}
			set
			{
				base.CC_LastReceiptAmount = value;
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_StandardOutstandingAmount
		{
			get
			{
				return base.CC_StandardOutstandingAmount;
			}
			set
			{
				base.CC_StandardOutstandingAmount = value;
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_StandardOverdueAmount
		{
			get
			{
				return base.CC_StandardOverdueAmount;
			}
			set
			{
				base.CC_StandardOverdueAmount = value;
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_TotalOutstandingAmount
		{
			get
			{
				return base.CC_TotalOutstandingAmount;
			}
			set
			{
				base.CC_TotalOutstandingAmount = value;
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal CC_TotalOverdueAmount
		{
			get
			{
				return base.CC_TotalOverdueAmount;
			}
			set
			{
				base.CC_TotalOverdueAmount = value;
			}
		}

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				return Header.GetHumanReadableShortcutName();
			}
		}

		#endregion
	}
}
