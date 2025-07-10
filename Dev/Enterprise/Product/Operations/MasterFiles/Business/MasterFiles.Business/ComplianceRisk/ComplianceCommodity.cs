using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceCommodity : IComplianceCommodity
	{
		public ComplianceCommodity(ZString harmonizedCode, ZString groupingOrCountry, ZString source, ZGuid parentJobID, ZString origin, ZString commoditySource, ZString goodsDescription)
		{
			if (harmonizedCode.IsEmpty)
			{
				throw new ArgumentException(null, nameof(harmonizedCode));
			}

			if (groupingOrCountry.IsEmpty)
			{
				throw new ArgumentException(null, nameof(groupingOrCountry));
			}

			if (!parentJobID.IsValid)
			{
				throw new ArgumentException(null, nameof(parentJobID));
			}

			HarmonizedCode = harmonizedCode;
			GroupingOrCountry = groupingOrCountry;
			Source = source;
			ParentJobID = parentJobID;
			Origin = origin;
			CommoditySource = commoditySource;
			GoodsDescription = goodsDescription.SubstringSafe(0, ComplianceCommodityDetailSchema.CCD_Description.MaxLength);
			DateAddedUtc = ZDateTime.UtcNow;
		}

		public ComplianceCommodity(ZString harmonizedCode, ZString groupingOrCountry, ZString source, ZGuid parentJobID, ZString origin, ZString commoditySource, ZString goodsDescription, ZString riskStatus, ZString assessmentNotes, ZDateTime dateAddedUtc) : this(harmonizedCode, groupingOrCountry, source, parentJobID, origin, commoditySource, goodsDescription)
		{
			if (riskStatus.IsEmpty)
			{
				throw new ArgumentException(null, nameof(riskStatus));
			}

			RiskStatus = riskStatus;
			AssessmentNotes = assessmentNotes;
			DateAddedUtc = dateAddedUtc;
		}

		public ComplianceCommodity(ZString groupingOrCountry, ZString source, ZGuid parentJobID, ZString commoditySource, DynamicBusinessObject rawDbLine) : this(harmonizedCode: (ZString)rawDbLine[JobComInvoiceLineSchema.JI_Tariff], groupingOrCountry, source, parentJobID, origin: (ZString)rawDbLine[JobComInvoiceLineSchema.JI_CountryOfOrigin], commoditySource, goodsDescription: GetCommercialInvoiceLineDescription(rawDbLine))
		{
		}

		public ComplianceCommodity(ZString groupingOrCountry, ZString source, ZGuid parentJobID, ZString commoditySource, IBaseJobComInvoiceLine line) : this(harmonizedCode: line.JI_TariffForComplianceWise, groupingOrCountry, source, parentJobID, origin: line.JI_CountryOfOrigin, commoditySource, goodsDescription: line.JI_Description.IsEmpty ? line.JI_NDescription : line.JI_Description)
		{
		}

		static ZString GetCommercialInvoiceLineDescription(DynamicBusinessObject rawDbLine)
		{
			var description = (ZString)rawDbLine[JobComInvoiceLineSchema.JI_Description];
			return description.IsEmpty ? (ZString)rawDbLine[JobComInvoiceLineSchema.JI_NDescription] : description;
		}

		public ZString HarmonizedCode { get; }

		public ZString GroupingOrCountry { get; }

		public ZString Source { get; }

		public ZString CommoditySource { get; set; }

		public ZString GoodsDescription { get; }

		public ZGuid ParentJobID { get; }

		public ZString? RiskStatus { get; }

		public ZString AssessmentNotes { get; }

		public ZString Origin { get; set; }

		public ZDateTime DateAddedUtc { get; set; }
	}
}
