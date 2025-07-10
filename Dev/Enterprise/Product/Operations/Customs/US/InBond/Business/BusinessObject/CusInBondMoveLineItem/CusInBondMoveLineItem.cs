using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using ISequenceNumberLine = Enterprise.Customs.Business.ISequenceNumberLine;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveLineItem : Customs.Business.CusInBondMoveLineItem, IShortSequenceNumberLine
	{
		public CusInBondMoveLineItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		public ZWeight Weight
		{
			get { return new ZWeight(BI_Weight, BI_WeightUnit); }
		}

		#endregion

		#region Override Properties

		#region BI_B9

		[RelatedBusinessObject("MoveDetail")]
		public override ZGuid BI_B9
		{
			get { return base.BI_B9; }
			set
			{
				var oldMoveDetail = MoveDetail;
				var oldValue = BI_B9;
				base.BI_B9 = value;
				if (!IsCopying && oldValue != BI_B9)
				{
					SetSequenceNoOnSettingBI_B9(oldMoveDetail);
				}
			}
		}

		public CusInBondMoveDetail MoveDetail
		{
			get { return Factory.Load<CusInBondMoveDetail>(BI_B9); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveLineItemLookups.WeightUnitList))]
		public override ZString BI_WeightUnit
		{
			get { return base.BI_WeightUnit; }
			set { base.BI_WeightUnit = value; }
		}

		[MeasureUnit(Schema.BI_WeightUnit, MeasureUnitType.Weight)]
		public override ZDecimal BI_Weight
		{
			get { return base.BI_Weight; }
			set { base.BI_Weight = value; }
		}

		public new CusInBondMoveLineItemLookups Lookups
		{
			get { return (CusInBondMoveLineItemLookups)base.Lookups; }
		}

		public new CusInBondMoveLineItemValidation Validation
		{
			get { return (CusInBondMoveLineItemValidation)base.Validation; }
		}

		[ResourceStringData("36CA7E2E-E3E2-43C7-B6A3-877DAD8A4AC0", Caption = "Description and Quantity of Merchandise", ShortCaption = "Desc.")]
		public override ZString BI_Description
		{
			get { return base.BI_Description; }
			set { base.BI_Description = value; }
		}

		[BusinessObjectTestExclude]
		public override ZShort BI_PrintingSequenceNo
		{
			get { return base.BI_PrintingSequenceNo; }
			set
			{
				if (value > 0)
				{
					ZShort oldValue = BI_PrintingSequenceNo;

					base.BI_PrintingSequenceNo = value;
					if (!IsCopying && oldValue != BI_PrintingSequenceNo)
					{
						var moveDetail = MoveDetail;
						if (moveDetail != null)
						{
							moveDetail.PrintingSequenceNoGenerator.RecalculateWhenRenumbered(this, oldValue);
						}
					}
				}
			}
		}

		#endregion

		#region Related Objects

		public CusInBondHeader Header
		{
			get
			{
				CusInBondMoveHeader moveHeader = MoveHeader;
				return moveHeader == null ? null : moveHeader.Header;
			}
		}

		public CusInBondMoveHeader MoveHeader
		{
			get
			{
				CusInBondMoveDetail moveDeatil = MoveDetail;
				return moveDeatil == null ? null : moveDeatil.MoveHeader;
			}
		}

		#endregion

		#region Implementation

		void SetSequenceNoOnSettingBI_B9(CusInBondMoveDetail oldMoveDetail)
		{
			if (oldMoveDetail != null)
			{
				oldMoveDetail.PrintingSequenceNoGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			var moveDetail = MoveDetail;
			if (moveDetail != null)
			{
				moveDetail.PrintingSequenceNoGenerator.RecalculateWhenAdded(this);
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override Customs.Business.CusInBondMoveLineItemLookups GetNewLookups()
		{
			return new CusInBondMoveLineItemLookups(this);
		}

		protected override Customs.Business.CusInBondMoveLineItemValidation GetNewValidation()
		{
			return new CusInBondMoveLineItemValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BI_MarksAndNumbers = Core.Constants.ContainerMarking.NoMarks;
		}

		#endregion

		#region IShortSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => BI_B9;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => BI_PrintingSequenceNo;
			set => base.BI_PrintingSequenceNo = value;
		}

		#endregion
	}
}
