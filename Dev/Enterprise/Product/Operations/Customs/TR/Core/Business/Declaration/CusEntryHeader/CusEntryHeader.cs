using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class CusEntryHeader : EU.Business.Declaration.CusEntryHeader, Integration.Customs.TR.ICusEntryHeader, IMessageAttachee, ICusEntryCPDecParent
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class Constants
		{
			public const string ExporterUnion = "EXU";
		}

		public new Customs.Business.CusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.CusEntryLineCollection<CusEntryLine>)base.MergedLines;
		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

		public CusEUEntryHeader TREntryHeader => (CusEUEntryHeader)AddInfoChild;

		protected override Customs.Business.ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new EU.Business.Declaration.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		[ChildEditable(true)]
		public new EU.Business.Declaration.CusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (EU.Business.Declaration.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		public CusEntryHeaderCharges ExportUnionCharges
		{
			get
			{
				if (!IsDeleted && fExportUnionCharges == null)
				{
					fExportUnionCharges = Charges.GetChargeWithThisCode(Constants.ExporterUnion);
					if (fExportUnionCharges != null)
					{
						RegisterEditableChildObject(fExportUnionCharges);
					}
				}
				return fExportUnionCharges;
			}
		}
		CusEntryHeaderCharges fExportUnionCharges;

		public CusEntryPayInfo ExportUnionPayInfo
		{
			get
			{
				if (!IsDeleted && fExportUnionPayInfo == null)
				{
					fExportUnionPayInfo = EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(x => x.C9_PaymentParty == Constants.ExporterUnion);
					if (fExportUnionPayInfo != null)
					{
						RegisterEditableChildObject(fExportUnionCharges);
					}
				}
				return fExportUnionPayInfo;
			}
		}
		CusEntryPayInfo fExportUnionPayInfo;

		[ChildEditable(true)]
		public CusEntryCPDecCollection CPDecCollection
		{
			get
			{
				if (cpDecs == null)
				{
					cpDecs = new CusEntryCPDecCollection(this);
					cpDecs.Load();
					RegisterEditableChildObject(cpDecs);
				}
				return cpDecs;
			}
		}
		CusEntryCPDecCollection cpDecs;

		ZString ICusEntryCPDecParent.Prefix => CusEntryHeaderSchema.Constants.Prefix;

		protected override ZString EntryNumberType => IsExport ? new ZString(Common.CusEntryNumberTypes.Standard.MovementReferenceNumber) : base.EntryNumberType;

		public void RegistrationSetter(ZString registrationNo, ZDateTime registrationDate) => SetEntryNumber(EntryNumberType, registrationNo, registrationDate);

		#region IMessageAttachee
		ZString IMessageAttachee.MessageStatus
		{
			get => CH_Status;
			set => CH_Status = value;
		}
		ZString IMessageAttachee.CustomsStatus
		{
			get => CH_EntryStatus;
			set => CH_EntryStatus = value;
		}

		ZString IMessageAttachee.JobReference => CH_BGMReference;

		ZGuid IMessageAttachee.GlobalBranchPK => Declaration.JE_GB;

		IBusinessObjectCollection IMessageAttachee.Messages => Messages;

		#endregion

		public override ZString CH_EntryStatus
		{
			get => base.CH_EntryStatus;
			set
			{
				var currentValue = base.CH_EntryStatus;
				base.CH_EntryStatus = value;

				if (currentValue != value)
				{
					Declaration?.JE_EntryStatusInfo?.RefreshBinding();
				}
			}
		}

		public void CreateStampDutyIfApplicable()
		{
			if (IsInDatabase)
			{
				return;
			}

			var stampDutyDuty = Charges.GetChargeWithThisCode(DeclarationHelper.StampDutyConstants.ChargeType);

			if (stampDutyDuty == null)
			{
				var isExemptFromStampDuty = Declaration.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().IsExemptFromStampDuty;

				if (!isExemptFromStampDuty)
				{
					var stampDutyTax = new Universal.RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Turkey, DeclarationHelper.StampDutyConstants.ChargeType, ZDateTime.Today);

					var charges = Charges.AddNew();
					charges.C1_ChargeType = DeclarationHelper.StampDutyConstants.ChargeType;
					charges.C1_ChargeAmount = stampDutyTax?.ZZF_Value ?? 0m;
					charges.C1_MethodOfPayment = DeclarationHelper.StampDutyConstants.MethodOfPayment;
					charges.C1_RateOverrideReasonCode = DeclarationHelper.StampDutyConstants.RateOverrideReasonCode;
					charges.C1_Source = DeclarationHelper.StampDutyConstants.Source;
				}
			}
		}
	}
}
