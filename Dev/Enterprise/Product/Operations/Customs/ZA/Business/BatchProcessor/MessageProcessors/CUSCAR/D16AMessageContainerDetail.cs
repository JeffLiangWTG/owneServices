using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class D16AMessageContainerDetail
	{
		public D16AMessageContainerDetail(SegmentGroup5 grp5)
		{
			this.grp5 = grp5;
		}

		readonly SegmentGroup5 grp5;

		public ZString SealNumber => grp5.SEL[0]?.TransportUnitSealIdentifier ?? ZString.Empty; // supports up to 4 ???

		public ZString ContainerNumber
		{
			get
			{
				if (containerNumber == null)
				{
					var eqdSegment = grp5.EQD.Cast<EQDSegment>()
										 .FirstOrDefault(eqd => eqd.EquipmentTypeCodeQualifier == EquipmentTypeCodeQualifierList.Container);

					if (eqdSegment != null)
					{
						containerNumber = eqdSegment.EquipmentIdentification.EquipmentIdentifier;
					}
				}
				return containerNumber;
			}
		}
		string containerNumber;

		public ZString GoodsWeight
		{
			get
			{
				if (!goodsWeight.HasValue)
				{
					GetGoodsWeight();
				}
				return (ZString)goodsWeight;
			}
		}
		ZString? goodsWeight;

		public ZString GoodsWeightUQ
		{
			get
			{
				if (!goodsWeightUQ.HasValue)
				{
					GetGoodsWeight();
				}
				return (ZString)goodsWeightUQ;
			}
		}
		ZString? goodsWeightUQ;

		void GetGoodsWeight()
		{
			var meaSegment = grp5.MEA.Cast<MEASegment>()
								 .FirstOrDefault(mea => mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.Measurement
													 && mea.MeasurementDetails.MeasuredAttributeCode == MeasuredAttributeCodeList.TransportEquipmentVerifiedGrossMassWeight);

			goodsWeight = meaSegment?.ValueRange.Measure ?? ZString.Empty;
			goodsWeightUQ = meaSegment?.ValueRange.MeasurementUnitCode ?? ZString.Empty;
		}
	}
}

