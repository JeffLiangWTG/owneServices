using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public abstract class CusInBondMoveDetail : Customs.Business.CusInBondMoveDetail
	{
		protected CusInBondMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static new readonly TypeDecider TypeDecider = new Customs.Business.CusInBondMoveDetailTypeDecider();

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.CusInBondMoveDetail.Schema
		{
			public const string B9_CustomsStatusDescription = "B9_CustomsStatusDescription";
			public const string B9_MessageStatusDescription = "B9_MessageStatusDescription";
		}

		#region Override Properties

		#region CustomsStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.CustomsStatusList))]
		public override ZString B9_CustomsStatus
		{
			get { return base.B9_CustomsStatus; }
			set { base.B9_CustomsStatus = value; }
		}

		public ZString B9_CustomsStatusDescription
		{
			get { return Lookups.CustomsStatusList.GetDescriptionFromCode(B9_CustomsStatus) ?? ZString.Empty; }
		}

		public ZPropertyInfo B9_CustomsStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B9_CustomsStatusDescription); }
		}

		#endregion

		#region B9_MessageStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.MessageStatusList))]
		public override ZString B9_MessageStatus
		{
			get { return base.B9_MessageStatus; }
			set { base.B9_MessageStatus = value; }
		}

		public ZString B9_MessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(B9_MessageStatus) ?? ZString.Empty; }
		}

		public ZPropertyInfo B9_MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B9_MessageStatusDescription); }
		}

		#endregion

		#region B9_ExportLadenOn

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.ConveyanceList))]
		public override ZString B9_ExportLadenOn
		{
			get { return base.B9_ExportLadenOn; }
			set { base.B9_ExportLadenOn = value; }
		}

		#endregion

		#region B9_ForeignDestPortKCode

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.ForeignPorts))]
		public override ZString B9_ForeignDestPortKCode
		{
			get { return base.B9_ForeignDestPortKCode; }
			set { base.B9_ForeignDestPortKCode = value; }
		}

		public override void OnSaving()
		{
			if (B9_B0.IsEmpty)
			{
				throw new ZSaveException(new ZDataException(new Exception(stackTraceWhenB9_B0SetToEmpty + Res.GetString("4EFE2361-52A7-4B22-87BC-6D06F7C6379B", "Save failed. No Bill Number set.")), null, null), Factory);
			}
			base.OnSaving();
		}

		#endregion

		#region B9_B0

		public override ZGuid B9_B0
		{
			get => base.B9_B0;
			set
			{
				base.B9_B0 = value;
				if (B9_B0.IsEmpty)
				{
					stackTraceWhenB9_B0SetToEmpty = new StackTrace().ToString();
				}
			}
		}

		string stackTraceWhenB9_B0SetToEmpty;

		#endregion

		public new CusInBondMoveDetailLookups Lookups
		{
			get { return (CusInBondMoveDetailLookups)base.Lookups; }
		}

		public new CusInBondMoveHeader MoveHeader
		{
			get { return (CusInBondMoveHeader)base.MoveHeader; }
		}

		#endregion

		#region New Properties

		public ZString MasterBillNumber
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_MasterBillNumber;
			}
		}

		#endregion

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var moveHeader = MoveHeader;
				var bill = Bill;
				var inBondNumber = moveHeader == null ? ZString.Empty : moveHeader.InBondNumber;
				var billNumber = bill == null ? ZString.Empty : bill.B0_MasterBillNumber;
				return (!inBondNumber.IsEmpty ? inBondNumber + " / " : "") + billNumber;
			}
		}

		#endregion
	}
}
