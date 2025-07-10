using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class GovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
	{
		public GovernmentAgencyGoodsItem(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		public CusEntryLine EntryLine { get; }

		public JobComInvoiceLine InvoiceLine { get; }

		public virtual ZInt SequenceNumeric => EntryLine.CL_LineNumber;

		public ICommodity Commodity => commodity ?? (commodity = CommodityCore);
		ICommodity commodity;

		protected virtual ICommodity CommodityCore => new Commodity(EntryLine, InvoiceLine);

		public virtual IEnumerable<IAdditionalDocument> AdditionalDocuments
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					foreach (var additionalDocument in GetAdditionalDocuments(invoiceLine))
					{
						yield return additionalDocument;
					}
				}
			}
		}

		protected IEnumerable<IAdditionalDocument> GetAdditionalDocuments(JobComInvoiceLine invoiceLine)
		{
			foreach (AssignedJobComInvLineRefs assignedJobComInvLineRefs in invoiceLine.AssignedJobComInvLineRefsCollection)
			{
				yield return new AdditionalDocumentWrapper(assignedJobComInvLineRefs.JG_ReferenceNumber, 0);
			}
		}

		public IEnumerable<IAdditionalInformation> AdditionalInformations => InvoiceLine.GetAdditionalInformations();

		public virtual IGoodsMeasure GoodsMeasure => new GoodsMeasure(EntryLine);

		public IPartyDetails Manufacturer => new GovernmentAgencyGoodsItemManufacturer(InvoiceLine.ManufacturerDocAddress);

		public IOrigin Origin => new Origin(InvoiceLine);

		public IPackaging Packaging
		{
			get
			{
				var packagingQTY = InvoiceLine.JI_PackagingQTY;
				return packagingQTY.IsEmpty ? null : new PackingWarpper(ZDate.Empty, InvoiceLine.JI_PackagingUQ, packagingQTY);
			}
		}

		public IPreviousDocument PreviousDocument => InvoiceLine.GetPreviousDocument();

		public ILPCODetail ApprovalDocument => null;

		public ICommoditySpecification CommoditySpecification => null;

		public IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasure => null;

		public virtual IGoodsStatisticalMeasure GoodsStatisticalMeasure => new GoodsStatisticalMeasure(EntryLine);

		public ILPCODetail MedicalInstrument => null;

		public IPreviousDocument PreBondedDocument => InvoiceLine.GetPreBondedDocument();

		public IEnumerable<IShippingIdentification> ShippingIdentifications => null;

		public IGovernmentProcedure GovernmentProcedure => new GovernmentProcedure(InvoiceLine);

		public ZDateTime ControlInspectionStartDateTime => ZDateTime.Empty;

		public ZString ExaminationPlace => ZString.Empty;

		public IEnumerable<ITransportEquipment> TransportEquipments => null;

		public IAdditionalDeclaration AdditionalDeclaration => null;

		#region Properties For Document

		public virtual ZString EntryLineGroupForDocument => EntryLine?.CL_Calc_EntryLineGroup ?? ZString.Empty;
		public virtual ZString CommodityDescriptionForDocument => EntryLine?.CL_Calc_GoodsDescriptionWithoutGrouping ?? ZString.Empty;
		#endregion

		public ZString CriteriaCode => null;

		public ZString PreferentialCriteria => null;

		public ZString ProducerCode => null;

		public ZString OtherCriteria => null;
	}
}
