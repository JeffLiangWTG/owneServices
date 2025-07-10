using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	internal class ACEStandAlonePriorNoticeWrapper
	{
		internal ACEStandAlonePriorNoticeWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
			fPriorNoticeHeaders = new List<IStandAlonePriorNoticeHeader>();
			GeneratePriorNoticeHeaders();
		}
		readonly JobDeclaration declaration;
		readonly List<IStandAlonePriorNoticeHeader> fPriorNoticeHeaders;

		void GeneratePriorNoticeHeaders()
		{
			if (declaration.IsACEENTStandAlonePriorNotice || declaration.IsFTZFTZStandAlonePriorNotice)
			{
				fPriorNoticeHeaders.Add(new ACEPriorNoticeEntryNumberWrapper(declaration));
			}
			else if (declaration.IsACEBLNStandAlonePriorNotice || declaration.IsFTZBLNStandAlonePriorNotice)
			{
				foreach (Bill bill in declaration.Bills)
				{
					if (bill.IsMasterBill)
					{
						fPriorNoticeHeaders.Add(new ACEPriorNoticeBillWrapper(bill, declaration));
					}
				}
			}
		}

		public List<IStandAlonePriorNoticeHeader> PriorNoticeHeaders
		{
			get { return fPriorNoticeHeaders; }
		}
	}

	abstract class ACEPriorNoticeDeclarationWrapper : IStandAlonePriorNoticeHeader
	{
		internal ACEPriorNoticeDeclarationWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		protected readonly JobDeclaration declaration;

		#region IStandAlonePriorNoticeHeader Members

		ZString IStandAlonePriorNoticeHeader.HumanReadableName
		{
			get { return GetHumanReadableName(); }
		}

		protected virtual ZString GetHumanReadableName()
		{
			return declaration.HumanReadableName;
		}

		ZString IStandAlonePriorNoticeHeader.ReferenceQualifierCode
		{
			get { return ReferenceQualifierCode; }
		}

		protected ZString ReferenceQualifierCode
		{
			get
			{
				var result = ZString.Empty;

				switch (declaration.US_SPNIDType)
				{
					case StandAlonePriorNoticeIDTypeList.Codes.ENT:
						result = PriorNoticeReferenceQualifierCodeList.Codes.ENT;
						break;
					case StandAlonePriorNoticeIDTypeList.Codes.BLN:
						result = declaration.IsAir ? PriorNoticeReferenceQualifierCodeList.Codes.AWB : PriorNoticeReferenceQualifierCodeList.Codes.BOL;
						break;
					case StandAlonePriorNoticeIDTypeList.Codes.FTZ:
						result = PriorNoticeReferenceQualifierCodeList.Codes.FTZ;
						break;
				}

				return result;
			}
		}

		ZString IStandAlonePriorNoticeHeader.FilerOrIssuerCode
		{
			get { return GetFilerOrIssuerCode(); }
		}

		protected abstract ZString GetFilerOrIssuerCode();

		ZString IStandAlonePriorNoticeHeader.ReferenceIdentifierNumber
		{
			get { return GetReferenceIdentifierNumber(); }
		}

		protected abstract ZString GetReferenceIdentifierNumber();

		ZString IStandAlonePriorNoticeHeader.BillTypeIndicator
		{
			get { return GetBillTypeIndicator(); }
		}

		protected virtual ZString GetBillTypeIndicator()
		{
			return ZString.Empty;
		}

		ZString IStandAlonePriorNoticeHeader.ImportingCarrierSCAC
		{
			get { return declaration.US_UI_NKCarrierSCAC; }
		}

		ZString IStandAlonePriorNoticeHeader.EntryType
		{
			get { return !declaration.IsFTZAdmission ? (string)declaration.US_EntryType : "81"; }
		}

		BusinessObjectFactory IStandAlonePriorNoticeHeader.Factory
		{
			get { return declaration.Factory; }
		}

		CBPEDIMessageCollection IStandAlonePriorNoticeHeader.Messages
		{
			get { return declaration.Messages; }
		}

		ZString IStandAlonePriorNoticeHeader.ModeOfTransportationCode
		{
			get { return declaration.JE_Calc_USTransportMode; }
		}

		ZBool IStandAlonePriorNoticeHeader.RelatedBillRequired
		{
			get { return (declaration.IsACEBLNStandAlonePriorNotice || declaration.IsFTZBLNStandAlonePriorNotice) && declaration.Bills.NumberOfMasterBill > 1; }
		}

		IEnumerable<IBillOfLadingDetail> IStandAlonePriorNoticeHeader.Bills
		{
			get { return GetChildBills(); }
		}

		protected virtual IEnumerable<IBillOfLadingDetail> GetChildBills()
		{
			return System.Array.Empty<IBillOfLadingDetail>();
		}

		IEnumerable<IACEPriorNoticeLine> IStandAlonePriorNoticeHeader.ACEStandalonePriorNoticeLines
		{
			get { return GetACEStandalonePriorNoticeLines(); }
		}

		protected abstract IEnumerable<IACEPriorNoticeLine> GetACEStandalonePriorNoticeLines();

		protected IEnumerable<IACEPriorNoticeLine> GetPriorNoticeLines(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			var sortedLines = new List<JobComInvoiceLine>(invoiceLines);
			sortedLines.Sort(new InvoiceLineComparer());

			foreach (JobComInvoiceLine invoiceLine in sortedLines)
			{
				var orderedFDALines = invoiceLine.ACE_FDALines.Cast<ACEFDA>().OrderBy(x => x.US_LineNo);

				foreach (ACEFDA fda in orderedFDALines)
				{
					if (fda.US_PNC.IsEmpty && !fda.US_PND)
					{
						if (fda.US_FDAForcePN || invoiceLine.HasPriorNoticeRequirements)
						{
							yield return fda;
						}
					}
				}
			}
		}

		#endregion
	}

	class ACEPriorNoticeBillWrapper : ACEPriorNoticeDeclarationWrapper
	{
		internal ACEPriorNoticeBillWrapper(Bill masterBill, JobDeclaration declaration)
			: base(declaration)
		{
			this.masterBill = Argument.NotNull(masterBill, "masterBill");
		}
		readonly Bill masterBill;

		protected override ZString GetHumanReadableName()
		{
			return "Master Bill(" + masterBill.SCACAndBillNumber + ")";
		}

		protected override ZString GetFilerOrIssuerCode()
		{
			return ReferenceQualifierCode == PriorNoticeReferenceQualifierCodeList.Codes.AWB ? ZString.Empty : masterBill.US_UI_NKBillIssuerSCAC;
		}

		protected override ZString GetReferenceIdentifierNumber()
		{
			return masterBill.CU_BillNum.KeepAlphanumericCharacters();
		}

		protected override ZString GetBillTypeIndicator()
		{
			return masterBill.IsLowestBill ? SEBillTypesList.Codes.RegularBill : SEBillTypesList.Codes.MasterBill;
		}

		protected override IEnumerable<IBillOfLadingDetail> GetChildBills()
		{
			return masterBill.ChildBills.Cast<IBillOfLadingDetail>();
		}

		protected override IEnumerable<IACEPriorNoticeLine> GetACEStandalonePriorNoticeLines()
		{
			var multipleMBs = declaration.Bills.Cast<Bill>().Where(x => x.IsMasterBill).IsCountMoreThan(1);
			var invoiceLines = multipleMBs ? declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.InvoiceHeader != null && BelongsToThisMasterBill(x.InvoiceHeader, masterBill)) : declaration.InvoiceLines.Cast<JobComInvoiceLine>();

			return GetPriorNoticeLines(invoiceLines);
		}

		static bool BelongsToThisMasterBill(JobComInvoiceHeader invoice, Bill masterBill)
		{
			Bill relatedMasterBill = null;
			var relatedBill = invoice.Bill;
			if (relatedBill != null)
			{
				if (relatedBill.IsMasterBill)
				{
					relatedMasterBill = relatedBill;
				}
				else if (relatedBill.ParentBill != null && relatedBill.ParentBill.IsMasterBill)
				{
					relatedMasterBill = relatedBill.ParentBill;
				}
			}

			return relatedMasterBill == masterBill;
		}
	}

	class ACEPriorNoticeEntryNumberWrapper : ACEPriorNoticeDeclarationWrapper
	{
		internal ACEPriorNoticeEntryNumberWrapper(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZString GetHumanReadableName()
		{
			if (declaration.IsFTZAdmission)
			{
				return "FTZ Admission Number(" + declaration.FTZAdmissionNumberFormatted + ")";
			}
			else
			{
				return "Entry Number(" + declaration.US_EntryFilerCode + declaration.ImportEntryNumber + ")";
			}
		}

		protected override ZString GetFilerOrIssuerCode()
		{
			return !declaration.IsFTZAdmission ? declaration.US_EntryFilerCode : ZString.Empty;
		}

		protected override ZString GetReferenceIdentifierNumber()
		{
			return declaration.IsFTZAdmission ? declaration.FTZAdmissionNumberFormatted : declaration.ImportEntryNumber;
		}

		protected override ZString GetBillTypeIndicator()
		{
			return ZString.Empty;
		}

		protected override IEnumerable<IBillOfLadingDetail> GetChildBills()
		{
			return declaration.Bills.Cast<Bill>().Where(x => x.IsMasterBill).Cast<IBillOfLadingDetail>();
		}

		protected override IEnumerable<IACEPriorNoticeLine> GetACEStandalonePriorNoticeLines()
		{
			return GetPriorNoticeLines(declaration.InvoiceLines.Cast<JobComInvoiceLine>());
		}
	}
}
