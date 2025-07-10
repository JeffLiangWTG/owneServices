using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACSDrawbackAddInfoJobComInvoiceLineValidation : CommonDrawbackAddInfoJobComInvoiceLineValidation
	{
		public ACSDrawbackAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		ZBool Is7552
		{
			get
			{
				var header = InvoiceLine.Declaration;
				return header == null ? ZBool.False : header.Is7552;
			}
		}

		protected override void CheckUS_ImportEntryNo()
		{
			base.CheckUS_ImportEntryNo();
			if (Parent.US_DRWIsForImportSection)
			{
				if ((InvoiceLine.US_ImportEntryNo.IsEmpty && InvoiceLine.US_DRWCertOfManufacture.IsEmpty) ||
					!(InvoiceLine.US_ImportEntryNo.IsEmpty || InvoiceLine.US_DRWCertOfManufacture.IsEmpty))
				{
					InvoiceLine.US_ImportEntryNoInfo.AddMessageError(ImportEntryOrCMEntered);
				}
			}
		}
		internal const string ImportEntryOrCMEntered = "You must enter either an Import Entry Number or a Certificate of Manufacture, but not both";

		protected override void CheckUS_DRWPort()
		{
			base.CheckUS_DRWPort();
			if (Parent.US_DRWIsForImportSection)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(InvoiceLine.US_DRWPortInfo, InvoiceLine.AddInfoLookups.RegionDistrictPorts);
				if (!InvoiceLine.US_DRWCertOfManufacture.IsEmpty && !InvoiceLine.Lookups.ValidCMPorts.Contains(InvoiceLine.US_DRWPort))
				{
					InvoiceLine.US_DRWPortInfo.AddMessageError(InvalidCMPort);
				}
			}
		}
		internal const string InvalidCMPort = "Invalid port for CM. Valid codes are 0401, 1001, 2002, 3901, 5201, 5301, 2704, or 2809";

		protected override void CheckUS_DRWCMCDIndicator()
		{
			if (Parent.US_DRWIsForImportSection)
			{
				base.CheckUS_DRWCMCDIndicator();
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(InvoiceLine.US_DRWCMCDIndicatorInfo, InvoiceLine.Lookups.DRWCMCDIndicatorCodeList);
			}
		}

		protected override void CheckUS_DRWCertOfManufacture()
		{
			base.CheckUS_DRWCertOfManufacture();
			if (Parent.US_DRWIsForImportSection)
			{
				if (!InvoiceLine.US_DRWCertOfManufacture.IsEmpty && !Regex.IsMatch(InvoiceLine.US_DRWCertOfManufacture, @"^CM\d{6}$"))
				{
					InvoiceLine.US_DRWCertOfManufactureInfo.AddMessageError(CertificateOfManufactureInvalid);
				}
				ValidateUS_ImportEntryNo();
			}
		}
		internal const string CertificateOfManufactureInvalid = "The Certificate of Manufacture should be 8 characters long and start with 'CM'";

		void DateRangeIsConsistent(ZDateTime dateFrom, ZDateTime dateTo, ZPropertyInfo dateInfoToHaveNotification)
		{
			if (!dateFrom.IsEmpty || !dateTo.IsEmpty)
			{
				if (Parent.Declaration != null && !Parent.Declaration.US_DRWSection.ToLower().Replace(" ", "").Contains("1313(b)", System.StringComparison.OrdinalIgnoreCase))
				{
					dateInfoToHaveNotification.AddMessageError(DatesMayNotBeEntered);
				}
				if ((dateFrom.IsEmpty && !dateTo.IsEmpty) || (!dateFrom.IsEmpty && !dateTo.IsEmpty && dateTo < dateFrom))
				{
					dateInfoToHaveNotification.AddMessageError(ToDateBeforeFromDate);
				}
			}
		}
		internal const string ToDateBeforeFromDate = "If the 'To' date is entered then the 'From' date must also be entered, and the 'To' date may not be before the 'From' date";
		internal const string DatesMayNotBeEntered = "This date is only required if you are using 1313(b)";

		protected override void CheckUS_DRWDateRcvFrom()
		{
			base.CheckUS_DRWDateRcvFrom();
			if (Parent.US_DRWIsForImportSection)
			{
				DateRangeIsConsistent(Parent.US_DRWDateRcvFrom, Parent.US_DRWDateRcvTo, Parent.US_DRWDateRcvFromInfo);
				ValidateUS_DRWDateRcvTo();
			}
		}

		protected override void CheckUS_DRWDateRcvTo()
		{
			base.CheckUS_DRWDateRcvTo();
			if (Parent.US_DRWIsForImportSection)
			{
				DateRangeIsConsistent(Parent.US_DRWDateRcvFrom, Parent.US_DRWDateRcvTo, Parent.US_DRWDateRcvToInfo);
				ValidateUS_DRWDateRcvFrom();
			}
		}

		protected override void CheckUS_DRWDateDelFrom()
		{
			if (Is7552)
			{
				base.CheckUS_DRWDateDelFrom();
				DateRangeIsConsistent(Parent.US_DRWDateDelFrom, Parent.US_DRWDateDelTo, Parent.US_DRWDateDelFromInfo);
				ValidateUS_DRWDateDelTo();
			}
		}

		protected override void CheckUS_DRWDateDelTo()
		{
			if (Is7552)
			{
				base.CheckUS_DRWDateDelTo();
				DateRangeIsConsistent(Parent.US_DRWDateDelFrom, Parent.US_DRWDateDelTo, Parent.US_DRWDateDelToInfo);
				ValidateUS_DRWDateDelFrom();
			}
		}

		protected override void CheckUS_DRWCDUse()
		{
			base.CheckUS_DRWCDUse();
			if (Is7552)
			{
				var header = Parent.Declaration;
				var lookups = (header == null) ? null : header.AddInfoLookups;
				if (lookups != null)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWCDUseInfo, lookups.US_DRWCDUseCodeList);
				}
			}
		}

		protected override void CheckUS_DRWDateUsedFrom()
		{
			base.CheckUS_DRWDateUsedFrom();
			if (Parent.US_DRWIsForImportSection)
			{
				DateRangeIsConsistent(Parent.US_DRWDateUsedFrom, Parent.US_DRWDateUsedTo, Parent.US_DRWDateUsedFromInfo);
				ValidateUS_DRWDateUsedTo();
			}
		}

		protected override void CheckUS_DRWDateUsedTo()
		{
			base.CheckUS_DRWDateUsedTo();
			if (Parent.US_DRWIsForImportSection)
			{
				DateRangeIsConsistent(Parent.US_DRWDateUsedFrom, Parent.US_DRWDateUsedTo, Parent.US_DRWDateUsedToInfo);
				ValidateUS_DRWDateUsedFrom();
			}
		}

		protected override void CheckUS_DRWExportID()
		{
			base.CheckUS_DRWExportID();
			if (Parent.US_DRWIsForExportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWExportIDInfo);
			}
		}

		protected override void CheckUS_DRWExportDest()
		{
			base.CheckUS_DRWExportDest();
			if (Parent.US_DRWIsForExportSection)
			{
				if (Parent.US_DRWExportAction != "D")
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWExportDestInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(InvoiceLine.US_DRWExportDestInfo);
			}
		}

		protected override void CheckUS_DRWImportEntryLine()
		{
			base.CheckUS_DRWImportEntryLine();

			if (Parent.US_DRWIsForImportSection && (Parent.US_DRWImportEntryLine < 1 || Parent.US_DRWImportEntryLine > short.MaxValue))
			{
				Parent.US_DRWImportEntryLineInfo.AddError(DRWImportEntryLineLength);
			}
		}
		internal static readonly string DRWImportEntryLineLength = "A valid Entry Line Number is required for Drawback purposes. Valid line number should be in the range of 1 through " + short.MaxValue;
	}
}
