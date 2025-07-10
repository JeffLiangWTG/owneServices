using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class D16AMessagePackLineDetail
	{
		public D16AMessagePackLineDetail(SegmentGroup14 grp14)
		{
			this.grp14 = grp14;
		}

		readonly SegmentGroup14 grp14;

		public ZInt GoodsLineNumber => ZInt.ParseSafe(GIDSegment?.GoodsItemNumber, 0);

		public ZString NumberOfPackages => GIDSegment?.NumberAndTypeOfPackages1.PackageQuantity ?? ZString.Empty;

		public ZString TypeOfPackages => GIDSegment?.NumberAndTypeOfPackages1.PackageTypeDescriptionCode ?? ZString.Empty;

		GIDSegment GIDSegment => gidSegment ?? (gidSegment = grp14.GID[0]);
		GIDSegment gidSegment;

		public ZString ContainerPackageLink => grp14.SGP[0]?.EquipmentIdentification.EquipmentIdentifier ?? ZString.Empty;

		public ZString GoodsDescription
		{
			get
			{
				if (goodsDescription == null)
				{
					var ftxSegment = grp14.FTX.Cast<FTXSegment>()
										  .FirstOrDefault(ftx => ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GoodsItemDescription);

					if (ftxSegment != null)
					{
						var text = ftxSegment.TextLiteral;
						goodsDescription = text.FreeText1 + text.FreeText2 + text.FreeText3 + text.FreeText4 + text.FreeText5;
					}
				}

				return (ZString)goodsDescription;
			}
		}
		string goodsDescription;

		public ZString MarksAndNumbers
		{
			get
			{
				if (marksAndNumbers == null)
				{
					var fullText = new StringBuilder();

					var pciSegment = grp14.PCI.Cast<PCISegment>().FirstOrDefault(pci => pci.MarkingInstructionsCode == MarkingInstructionsCodeList.ShipperAssigned);
					if (pciSegment != null)
					{
						var labels = pciSegment.MarksLabels;
						fullText.Append(labels.ShippingMarksDescription1);
						fullText.Append(labels.ShippingMarksDescription2);
						fullText.Append(labels.ShippingMarksDescription3);
						fullText.Append(labels.ShippingMarksDescription4);
						fullText.Append(labels.ShippingMarksDescription5);
						fullText.Append(labels.ShippingMarksDescription6);
						fullText.Append(labels.ShippingMarksDescription7);
						fullText.Append(labels.ShippingMarksDescription8);
						fullText.Append(labels.ShippingMarksDescription9);
						fullText.Append(labels.ShippingMarksDescription10);
					}

					marksAndNumbers = fullText.ToString();
				}

				return marksAndNumbers;
			}
		}
		string marksAndNumbers;

		public ZString PackWeight
		{
			get
			{
				if (!packWeight.HasValue)
				{
					GetPackWeight();
				}
				return (ZString)packWeight;
			}
		}
		ZString? packWeight;

		public ZString PackWeightUQ
		{
			get
			{
				if (!packWeightUQ.HasValue)
				{
					GetPackWeight();
				}
				return (ZString)packWeightUQ;
			}
		}
		ZString? packWeightUQ;

		void GetPackWeight()
		{
			var meaSegment = grp14.MEA.Cast<MEASegment>()
								  .FirstOrDefault(mea => mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.Measurement
													  && mea.MeasurementDetails.MeasuredAttributeCode == MeasuredAttributeCodeList.GoodsItemGrossWeight);

			packWeight = meaSegment?.ValueRange.Measure ?? ZString.Empty;
			packWeightUQ = meaSegment?.ValueRange.MeasurementUnitCode ?? ZString.Empty;
		}
	}
}

