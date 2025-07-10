using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public class Measure : BaseModel, IRegulationData, ICopyable<Measure>
	{
		public string RegulationId { get; set; }
		public string RegulationRoleTypeId { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string ItemId { get; set; }
		public string Suffix { get; set; }
		public string MeasureType { get; set; }
		public string GeographicalArea { get; set; }
		public string ExportItemId { get; set; }
		public string AdditionalCode { get; set; }
		public string AdditionalCodeType { get; set; }
		public string OrderNumber { get; set; }

		public IEnumerable<MeasureComponent> Components => components.Values;
		public IEnumerable<MeasureCondition> Conditions => conditions.Values.OrderBy(x => x.SequenceNumber);
		public IEnumerable<string> ExcludedGeographicalAreas => excludedGeographicalAreas.Values.Select(x => x.Value);
		public IEnumerable<string> Footnotes => footnotes.Values.Select(x => x.Value);

		// Calculated
		public string CleanId { get; set; }
		public string Description { get; set; }
		public string ExportDescription { get; set; }
		public string Formula { get; set; }
		public string CompositeKey { get; set; }
		public string ConditionClass { get; set; }
		public string MeasureTypeDescription { get; set; }
		public IEnumerable<string> TariffTypes { get; set; }
		public string MeasureTypeSeries { get; set; }
		public string RateType { get; set; }
		public string RateCode { get; set; }
		public IEnumerable<string> Preferences { get; set; }
		public string VatCode { get; set; }

		public DateTime? NomenclatureStartDate { get; set; }
		public DateTime? NomenclatureEndDate { get; set; }

		public override bool IsChapterSpecific => true;

		public override bool IsInChapter(string chapterFilter) => ItemId?.StartsWith(chapterFilter, StringComparison.Ordinal) ?? false;

		protected override bool IsValidCore(StringBuilder errorCollector, string source)
		{
			var valid = true;
			var validationErrors = new StringBuilder();

			if (ItemId?.Length != 10 || !long.TryParse(ItemId, out _))
			{
				validationErrors.Append("A valid ItemId is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(RegulationId))
			{
				validationErrors.Append("RegulationId is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(GeographicalArea))
			{
				validationErrors.Append("GeographicalArea is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(MeasureType))
			{
				validationErrors.Append("MeasureType is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"Measure validation error. Key: '{ItemId}' Errors: '{validationErrors}' Source: '{source}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		public DateTime CalcStartDate => CommonHelper.CalcMinDate(StartDate);
		public DateTime CalcEndDate => CommonHelper.CalcMaxDate(EndDate);
		public DateTime CalcNomenclatureStartDate => CommonHelper.CalcMinDate(NomenclatureStartDate);
		public DateTime CalcNomenclatureEndDate => CommonHelper.CalcMaxDate(NomenclatureEndDate);

		public bool IsAdditionalInfo => string.IsNullOrEmpty(Formula) && !(Conditions?.Any() ?? false);
		public bool IsSupplementaryUnit { get; set; }

		public Measure Copy()
		{
			var m = new Measure
			{
				ItemId = ItemId,
				Suffix = Suffix,
				HJID = HJID,
				OpType = OpType,
				OpDate = OpDate,
				RegulationId = RegulationId,
				RegulationRoleTypeId = RegulationRoleTypeId,
				StartDate = StartDate,
				EndDate = EndDate,
				MeasureType = MeasureType,
				GeographicalArea = GeographicalArea,
				ExportItemId = ExportItemId,
				AdditionalCode = AdditionalCode,
				AdditionalCodeType = AdditionalCodeType,
				OrderNumber = OrderNumber,

				CleanId = CleanId,
				Description = Description,
				ExportDescription = ExportDescription,
				Formula = Formula,
				CompositeKey = CompositeKey,
				ConditionClass = ConditionClass,
				MeasureTypeDescription = MeasureTypeDescription,
				TariffTypes = TariffTypes,
				MeasureTypeSeries = MeasureTypeSeries,
				RateType = RateType,
				RateCode = RateCode,
				Preferences = Preferences,
				VatCode = VatCode,
				IsSupplementaryUnit = IsSupplementaryUnit,

				NomenclatureStartDate = NomenclatureStartDate,
				NomenclatureEndDate = NomenclatureEndDate,
			};

			m.components.CopyFrom(components);
			m.conditions.CopyFrom(conditions);
			m.excludedGeographicalAreas.CopyFrom(excludedGeographicalAreas);
			m.footnotes.CopyFrom(footnotes);

			return m;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity")]
		protected override void UpdateFromXElement(XElement element)
		{
			RegulationId = element.Element("measureGeneratingRegulationId")?.Value ?? string.Empty;
			RegulationRoleTypeId = element.Element("regulationRoleType")?.Element("regulationRoleTypeId")?.Value ?? string.Empty;
			ItemId = element.Element("goodsNomenclature")?.Element("goodsNomenclatureItemId")?.Value ?? string.Empty;
			Suffix = element.Element("goodsNomenclature")?.Element("produclineSuffix")?.Value ?? string.Empty;
			ExportItemId = element.Element("exportRefundNomenclature")?.Element("goodsNomenclature")?.Element("goodsNomenclatureItemId")?.Value ?? string.Empty;
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityEndDate", x => EndDate = x);
			MeasureType = element.Element("measureType")?.Element("measureTypeId")?.Value ?? string.Empty;
			GeographicalArea = element.Element("geographicalArea")?.Element("geographicalAreaId")?.Value ?? string.Empty;
			OrderNumber = element.Element("ordernumber")?.Value ?? string.Empty;
			AdditionalCode = element.Element("additionalCode")?.Element("additionalCodeCode")?.Value ?? string.Empty;
			AdditionalCodeType = element.Element("additionalCode")?.Element("additionalCodeType")?.Element("additionalCodeTypeId")?.Value ?? string.Empty;

			foreach (var mcElement in element.Elements("measureComponent"))
			{
				var mcModel = new MeasureComponent();
				XmlElementReader.LoadMetaInfo(mcModel, mcElement);
				components.ProcessUpdate(mcModel, mcElement);
			}

			foreach (var mcElement in element.Elements("measureCondition"))
			{
				var mcModel = new MeasureCondition();
				XmlElementReader.LoadMetaInfo(mcModel, mcElement);
				conditions.ProcessUpdate(mcModel, mcElement);
			}

			foreach (var exElement in element.Elements("measureExcludedGeographicalArea"))
			{
				var exModel = new ExcludedGeographicalArea();
				XmlElementReader.LoadMetaInfo(exModel, exElement);
				excludedGeographicalAreas.ProcessUpdate(exModel, exElement);
			}

			foreach (var fElement in element.Elements("footnoteAssociationMeasure"))
			{
				var fModel = new Footnote();
				XmlElementReader.LoadMetaInfo(fModel, fElement);
				footnotes.ProcessUpdate(fModel, fElement);
			}
		}

		protected override void ProcessDelete()
		{
			base.ProcessDelete();
			components.Clear();
			conditions.Clear();
			excludedGeographicalAreas.Clear();
			footnotes.Clear();
		}

		internal Measure SetComponents(IEnumerable<MeasureComponent> components)
		{
			this.components.Add(components);
			return this;
		}

		internal Measure SetConditions(IEnumerable<MeasureCondition> conditions)
		{
			this.conditions.Add(conditions);
			return this;
		}

		internal Measure SetExcludedGeographicalAreas(IEnumerable<ExcludedGeographicalArea> excludedGeographicalAreas)
		{
			this.excludedGeographicalAreas.Add(excludedGeographicalAreas);
			return this;
		}

		internal Measure SetFootnotes(IEnumerable<Footnote> footnotes)
		{
			this.footnotes.Add(footnotes);
			return this;
		}

		readonly UpdatableElementList<MeasureComponent> components = new UpdatableElementList<MeasureComponent>();
		readonly UpdatableElementList<MeasureCondition> conditions = new UpdatableElementList<MeasureCondition>();
		readonly UpdatableElementList<ExcludedGeographicalArea> excludedGeographicalAreas = new UpdatableElementList<ExcludedGeographicalArea>();
		readonly UpdatableElementList<Footnote> footnotes = new UpdatableElementList<Footnote>();
	}

	public abstract class BaseTariffModel : BaseModel
	{
		public override bool IsChapterSpecific => false;
		public override bool IsInChapter(string chapterFilter) => true;
		protected override bool IsValidCore(StringBuilder errorCollector, string source) => true;
	}

	public abstract class CommonMeasurementElements : BaseTariffModel
	{
		public string MeasurementUnit { get; set; }
		public string MeasurementUnitQualifier { get; set; }
		public string MonetaryUnit { get; set; }
	}

	public class MeasureComponent : CommonMeasurementElements, ICopyable<MeasureComponent>
	{
		public decimal? DutyAmount { get; set; }
		public string DutyExpression { get; set; }

		public MeasureComponent Copy()
		{
			return new MeasureComponent
			{
				HJID = HJID,
				OpType = OpType,
				OpDate = OpDate,

				DutyAmount = DutyAmount,
				DutyExpression = DutyExpression,

				MeasurementUnit = MeasurementUnit,
				MeasurementUnitQualifier = MeasurementUnitQualifier,
				MonetaryUnit = MonetaryUnit
			};
		}

		protected override void UpdateFromXElement(XElement element)
		{
			XmlElementReader.UpdateDecimalFromElementIfPresent(element, "dutyAmount", x => DutyAmount = x);
			DutyExpression = element.Element("dutyExpression")?.Element("dutyExpressionId")?.Value ?? string.Empty;
			MeasurementUnit = element.Element("measurementUnit")?.Element("measurementUnitCode")?.Value ?? string.Empty;
			MeasurementUnitQualifier = element.Element("measurementUnitQualifier")?.Element("measurementUnitQualifierCode")?.Value ?? string.Empty;
			MonetaryUnit = element.Element("monetaryUnit")?.Element("monetaryUnitCode")?.Value ?? string.Empty;
		}
	}

	public class MeasureCondition : CommonMeasurementElements, ICopyable<MeasureCondition>
	{
		public int? SequenceNumber { get; set; }
		public decimal? DutyAmount { get; set; }
		public string CertificateCode { get; set; }
		public string CertificateTypeCode { get; set; }
		public string MeasureAction { get; set; }
		public string ConditionCode { get; set; }

		// Calculated / Loaded
		public string ConditionCodeDescription { get; set; }
		public bool IsRateFormulaCondition { get; set; }
		public bool IsCertificate { get; set; }
		public string Formula { get; set; }

		public IEnumerable<MeasureComponent> Components => components.Values;

		public MeasureCondition Copy()
		{
			var mc = new MeasureCondition
			{
				HJID = HJID,
				OpType = OpType,
				OpDate = OpDate,

				SequenceNumber = SequenceNumber,
				DutyAmount = DutyAmount,
				CertificateCode = CertificateCode,
				CertificateTypeCode = CertificateTypeCode,
				MeasureAction = MeasureAction,
				ConditionCode = ConditionCode,
				ConditionCodeDescription = ConditionCodeDescription,
				IsRateFormulaCondition = IsRateFormulaCondition,
				IsCertificate = IsCertificate,
				Formula = Formula,

				MeasurementUnit = MeasurementUnit,
				MeasurementUnitQualifier = MeasurementUnitQualifier,
				MonetaryUnit = MonetaryUnit,
			};
			mc.components.CopyFrom(components);

			return mc;
		}

		internal MeasureCondition SetComponents(IEnumerable<MeasureComponent> components)
		{
			this.components.Add(components);
			return this;
		}

		protected override void UpdateFromXElement(XElement element)
		{
			XmlElementReader.UpdateIntFromElementIfPresent(element, "conditionSequenceNumber", x => SequenceNumber = x);
			XmlElementReader.UpdateDecimalFromElementIfPresent(element, "conditionDutyAmount", x => DutyAmount = x);
			CertificateCode = element.Element("certificate")?.Element("certificateCode")?.Value ?? string.Empty;
			CertificateTypeCode = element.Element("certificate")?.Element("certificateType")?.Element("certificateTypeCode")?.Value ?? string.Empty;
			MeasureAction = element.Element("measureAction")?.Element("actionCode")?.Value ?? string.Empty;
			ConditionCode = element.Element("measureConditionCode")?.Element("conditionCode")?.Value ?? string.Empty;

			MeasurementUnit = element.Element("measurementUnit")?.Element("measurementUnitCode")?.Value ?? string.Empty;
			MeasurementUnitQualifier = element.Element("measurementUnitQualifier")?.Element("measurementUnitQualifierCode")?.Value ?? string.Empty;
			MonetaryUnit = element.Element("monetaryUnit")?.Element("monetaryUnitCode")?.Value ?? string.Empty;

			foreach (var mcElement in element.Elements("measureConditionComponent"))
			{
				var mcModel = new MeasureComponent();
				XmlElementReader.LoadMetaInfo(mcModel, mcElement);
				components.ProcessUpdate(mcModel, mcElement);
			}
		}

		readonly UpdatableElementList<MeasureComponent> components = new UpdatableElementList<MeasureComponent>();
	}

	public class ExcludedGeographicalArea : BaseTariffModel, ICopyable<ExcludedGeographicalArea>
	{
		public string Value { get; set; }

		public ExcludedGeographicalArea Copy()
		{
			return new ExcludedGeographicalArea
			{
				HJID = HJID,
				OpType = OpType,
				OpDate = OpDate,
				Value = Value,
			};
		}

		protected override void UpdateFromXElement(XElement element)
		{
			var exclusion = element.Element("geographicalArea")?.Element("geographicalAreaId")?.Value ?? string.Empty;
			if (!string.IsNullOrEmpty(exclusion))
			{
				Value = exclusion;
			}
		}
	}

	public class Footnote : BaseTariffModel, ICopyable<Footnote>
	{
		public string Value { get; set; }

		public Footnote Copy()
		{
			return new Footnote
			{
				HJID = HJID,
				OpType = OpType,
				OpDate = OpDate,
				Value = Value,
			};
		}

		protected override void UpdateFromXElement(XElement element)
		{
			var footnoteTypeId = element.Element("footnote")?.Element("footnoteType")?.Element("footnoteTypeId")?.Value ?? string.Empty;
			var footnoteId = element.Element("footnote")?.Element("footnoteId")?.Value ?? string.Empty;

			var footnote = $"{footnoteTypeId}{footnoteId}";
			if (!string.IsNullOrEmpty(footnote))
			{
				Value = footnote;
			}
		}
	}
}
