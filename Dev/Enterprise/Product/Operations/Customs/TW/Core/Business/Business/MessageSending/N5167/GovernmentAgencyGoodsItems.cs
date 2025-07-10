using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5167
{
	public class GovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
	{
		public GovernmentAgencyGoodsItem(CusEntryHeader header)
		{
			this.header = header;
		}

		readonly CusEntryHeader header;

		public ZInt SequenceNumeric => ZInt.Zero;

		public ICommodity Commodity => null;

		public IEnumerable<IAdditionalDocument> AdditionalDocuments => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

		public IGoodsMeasure GoodsMeasure => null;

		public IPartyDetails Manufacturer => null;

		public IOrigin Origin => null;

		public IPackaging Packaging => null;

		public IPreviousDocument PreviousDocument => null;

		public ILPCODetail ApprovalDocument => null;

		public ICommoditySpecification CommoditySpecification => null;

		public IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasure => null;

		public IGoodsStatisticalMeasure GoodsStatisticalMeasure => null;

		public ILPCODetail MedicalInstrument => null;

		public IPreviousDocument PreBondedDocument => null;

		public IEnumerable<IShippingIdentification> ShippingIdentifications => null;

		public IGovernmentProcedure GovernmentProcedure => null;

		public ZDateTime ControlInspectionStartDateTime => header.EntryInstruction?.TW_ICIExamTime ?? ZDateTime.Empty;

		public ZString ExaminationPlace => header.EntryInstruction?.TW_ICIExamLocation ?? ZString.Empty;

		public IEnumerable<ITransportEquipment> TransportEquipments
		{
			get
			{
				foreach (CusContainer cusContainer in header.Declaration.CusContainers)
				{
					yield return new TransportEquipmentWrapper(cusContainer);
				}
			}
		}

		public IAdditionalDeclaration AdditionalDeclaration => new AdditionalDeclaration(header.EntryNumberForSendingObject);

		public ZString CriteriaCode => null;

		public ZString PreferentialCriteria => null;

		public ZString ProducerCode => null;

		public ZString OtherCriteria => null;
	}
}
