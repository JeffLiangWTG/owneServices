using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineRelatedControllingMsgHeadersGenPivot : CustomsGenPivot, Integration.Customs.TW.IInvoiceLineRelatedCAHeadersGenPivot
	{
		public InvoiceLineRelatedControllingMsgHeadersGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = GenPivotTypeDecider.Types.InvoiceLineRelatedControllingMessageHeaderPivot;
			XX_Relation1TableCode = JobComInvoiceLineSchema.Constants.Prefix;
			XX_Relation2TableCode = CusTWControllingMessageHeaderSchema.Constants.Prefix;
		}

		JobComInvoiceLine InvoiceLine => Relation1Object as JobComInvoiceLine;

		CusTWControllingMessageHeader ControllingMessageHeader => Relation2Object as CusTWControllingMessageHeader;

		public override void Delete()
		{
			var oldInvoiceLine = IsDeleted ? null : InvoiceLine;
			var oldControllingMessageHeader = IsDeleted ? null : ControllingMessageHeader;

			base.Delete();
			RefreshLinksIfNeeded(oldInvoiceLine, oldControllingMessageHeader, false);
		}

		void RefreshLinksIfNeeded(JobComInvoiceLine invoiceLine, CusTWControllingMessageHeader header, bool isLinked)
		{
			if (invoiceLine != null && header != null)
			{
				invoiceLine.RefreshInvoiceLineLinkControllingMsgHeaderLinkIfNeeded(header.PK, isLinked);
				header.RefreshControllingMessageHeaderLinkInvoiceLineLinkIfNeeded(invoiceLine.PK, isLinked);
			}
		}

		public override ZGuid XX_Relation1ID
		{
			get => base.XX_Relation1ID;
			set
			{
				var oldInvoiceLine = InvoiceLine;
				base.XX_Relation1ID = value;
				if (oldInvoiceLine != InvoiceLine)
				{
					RefreshLinksIfNeeded(oldInvoiceLine, ControllingMessageHeader, false);
					RefreshLinksIfNeeded(InvoiceLine, ControllingMessageHeader, true);
				}
			}
		}

		public override ZGuid XX_Relation2ID
		{
			get => base.XX_Relation2ID;
			set
			{
				var oldControllingMessageHeader = ControllingMessageHeader;
				base.XX_Relation2ID = value;
				if (oldControllingMessageHeader != ControllingMessageHeader)
				{
					RefreshLinksIfNeeded(InvoiceLine, oldControllingMessageHeader, false);
					RefreshLinksIfNeeded(InvoiceLine, ControllingMessageHeader, true);
				}
			}
		}
	}
}
