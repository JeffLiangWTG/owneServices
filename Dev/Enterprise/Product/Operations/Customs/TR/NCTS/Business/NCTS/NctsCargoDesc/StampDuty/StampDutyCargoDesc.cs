using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	[DependentBusinessObject(typeof(NctsHeader), "StampDutyFees")]
	public class StampDutyCargoDesc : CusInBondCargoDesc
	{
		public StampDutyCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new class Schema : CusInBondCargoDesc.Schema
		{
			public const string StampDuty = nameof(StampDuty);
			public const string StampDutyStatus = nameof(StampDutyStatus);
			public const string RegistrationDate = nameof(RegistrationDate);
		}

		protected override Type FeeTypeCore => typeof(StampDutyFee);

		public NctsHeader NctsHeader => Factory.Load<NctsHeader>(BY_ParentID);

		#region Stamp Duty Fee

		[ChildEditable(true)]
		public StampDutyFeeCollection StampDutyFees
		{
			get
			{
				if (fStampDutyFees == null)
				{
					fStampDutyFees = new StampDutyFeeCollection(this);
					RegisterEditableChildObject(fStampDutyFees);
				}
				return fStampDutyFees;
			}
		}
		StampDutyFeeCollection fStampDutyFees;

		StampDutyFee SingleStampDutyFee
		{
			get
			{
				if (fSingleStampDutyFee == null || fSingleStampDutyFee.IsDeleted)
				{
					fSingleStampDutyFee = StampDutyFees.FirstOrDefault(e => e != null && !e.IsDeleted);
				}
				return fSingleStampDutyFee;
			}
		}
		StampDutyFee fSingleStampDutyFee;

		StampDutyFee CreateStampDutyFee()
		{
			using (SuspendSettingHasChanges())
			using (SuspendMarkingAsNeedingValidation())
			{
				return StampDutyFees.AddNew();
			}
		}

		public ZDecimal StampDuty
		{
			get => SingleStampDutyFee?.BFE_ChargeAmount ?? ZDecimal.Zero;
			set
			{
				var oldValue = StampDuty;
				if (oldValue != value)
				{
					var singleStampDutyFee = SingleStampDutyFee ?? CreateStampDutyFee();
					singleStampDutyFee.BFE_ChargeAmount = value;

					StampDutyInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo StampDutyInfo => GetZPropertyInfo(Schema.StampDuty);

		[MaxLength(1)]
		public ZString StampDutyStatus
		{
			get => SingleStampDutyFee?.BFE_MethodOfCalculation ?? ZString.Empty;
			set
			{
				var oldValue = StampDutyStatus;
				if (oldValue != value)
				{
					CheckMaximumLength(StampDutyStatusInfo, value);

					var singleStampDutyFee = SingleStampDutyFee ?? CreateStampDutyFee();
					singleStampDutyFee.BFE_MethodOfCalculation = value;

					StampDutyStatusInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo StampDutyStatusInfo => GetZPropertyInfo(Schema.StampDutyStatus);

		public ZDate RegistrationDate
		{
			get => SingleStampDutyFee?.RegistrationDate ?? ZDate.Today;
		}

		public ZPropertyInfo RegistrationDateInfo => GetZPropertyInfo(Schema.RegistrationDate);

		public override void OnSaving()
		{
			if (this.StampDuty.IsEmpty && this.StampDutyStatus.IsEmpty)
			{
				this.Delete();
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			StampDutyFees.DeleteAll();
			base.Delete();
		}

		#endregion
	}
}
