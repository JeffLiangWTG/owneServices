using System.Xml.Serialization;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

public partial class additionalCode : IDataPoint;
public partial class additionalCodeType : IDataPoint;
public partial class baseRegulation : IDataPoint;
public partial class ceiling : IDataPoint;
public partial class certificate : IDataPoint;
public partial class certificateType : IDataPoint;
public partial class completeAbrogationRegulation : IDataPoint;
public partial class dutyExpression : IDataPoint;
public partial class explicitAbrogationRegulation : IDataPoint;
public partial class exportRefundNomenclature : IDataPoint;
public partial class footnote : IDataPoint;
public partial class footnoteType : IDataPoint;
public partial class fullTemporaryStopRegulation : IDataPoint;
public partial class geographicalArea : IDataPoint;
public partial class goodsNomenclature : IDataPoint;
public partial class goodsNomenclatureGroup : IDataPoint;
public partial class language : IDataPoint;
public partial class measureAction : IDataPoint;
public partial class measure : IDataPoint;
public partial class measureConditionCode : IDataPoint;
public partial class measureType : IDataPoint;
public partial class measureTypeSeries : IDataPoint;
public partial class measurementUnit : IDataPoint;
public partial class measurementUnitQualifier : IDataPoint;
public partial class meursingAdditionalCode : IDataPoint;
public partial class meursingTablePlan : IDataPoint;
public partial class modificationRegulation : IDataPoint;
public partial class monetaryExchangePeriod : IDataPoint;
public partial class monetaryPlaceOfPublication : IDataPoint;
public partial class monetaryUnit : IDataPoint;
public partial class prorogationRegulation : IDataPoint;
public partial class publicationSigle : IDataPoint;
public partial class quotaDefinition : IDataPoint;
public partial class quotaOrderNumber : IDataPoint;
public partial class regulationGroup : IDataPoint;
public partial class regulationReplacement : IDataPoint;
public partial class regulationRoleType : IDataPoint;
public partial class sensitiveGood : IDataPoint;

public partial class findAdditionalCodeByDatesResponse : IDataGroup<additionalCode>
{
	[XmlIgnore]
	public additionalCode[] DataPoints { get => AdditionalCode; set => AdditionalCode = value; }
}

public partial class findAdditionalCodeTypeByDatesResponse : IDataGroup<additionalCodeType>
{
	[XmlIgnore]
	public additionalCodeType[] DataPoints { get => AdditionalCodeType; set => AdditionalCodeType = value; }
}

public partial class findBaseRegulationByDatesResponseHistory : IDataGroup<baseRegulation>
{
	[XmlIgnore]
	public baseRegulation[] DataPoints { get => BaseRegulation; set => BaseRegulation = value; }
}

public partial class findCeilingByDatesResponseHistory : IDataGroup<ceiling>
{
	[XmlIgnore]
	public ceiling[] DataPoints { get => Ceiling; set => Ceiling = value; }
}

public partial class findCertificateByDatesResponse : IDataGroup<certificate>
{
	[XmlIgnore]
	public certificate[] DataPoints { get => Certificate; set => Certificate = value; }
}

public partial class findCertificateTypeByDatesResponse : IDataGroup<certificateType>
{
	[XmlIgnore]
	public certificateType[] DataPoints { get => CertificateType; set => CertificateType = value; }
}

public partial class findCompleteAbrogationRegulationByDatesResponseHistory : IDataGroup<completeAbrogationRegulation>
{
	[XmlIgnore]
	public completeAbrogationRegulation[] DataPoints { get => CompleteAbrogationRegulation; set => CompleteAbrogationRegulation = value; }
}

public partial class findDutyExpressionByDatesResponse : IDataGroup<dutyExpression>
{
	[XmlIgnore]
	public dutyExpression[] DataPoints { get => DutyExpression; set => DutyExpression = value; }
}

public partial class findExplicitAbrogationRegulationByDatesResponseHistory : IDataGroup<explicitAbrogationRegulation>
{
	[XmlIgnore]
	public explicitAbrogationRegulation[] DataPoints { get => ExplicitAbrogationRegulation; set => ExplicitAbrogationRegulation = value; }
}

public partial class findExportRefundNomenclatureByDatesResponse : IDataGroup<exportRefundNomenclature>
{
	[XmlIgnore]
	public exportRefundNomenclature[] DataPoints { get => ExportRefundNomenclature; set => ExportRefundNomenclature = value; }
}

public partial class findFootnoteByDatesResponse : IDataGroup<footnote>
{
	[XmlIgnore]
	public footnote[] DataPoints { get => Footnote; set => Footnote = value; }
}

public partial class findFootnoteTypeByDatesResponse : IDataGroup<footnoteType>
{
	[XmlIgnore]
	public footnoteType[] DataPoints { get => FootnoteType; set => FootnoteType = value; }
}

public partial class findFullTemporaryStopRegulationByDatesResponseHistory : IDataGroup<fullTemporaryStopRegulation>
{
	[XmlIgnore]
	public fullTemporaryStopRegulation[] DataPoints { get => FullTemporaryStopRegulation; set => FullTemporaryStopRegulation = value; }
}

public partial class findGeographicalAreaByDatesResponse : IDataGroup<geographicalArea>
{
	[XmlIgnore]
	public geographicalArea[] DataPoints { get => GeographicalArea; set => GeographicalArea = value; }
}

public partial class findGoodsNomenclatureByDatesResponse : IDataGroup<goodsNomenclature>
{
	[XmlIgnore]
	public goodsNomenclature[] DataPoints { get => GoodsNomenclature; set => GoodsNomenclature = value; }
}

public partial class findGoodsNomenclatureGroupByDatesResponse : IDataGroup<goodsNomenclatureGroup>
{
	[XmlIgnore]
	public goodsNomenclatureGroup[] DataPoints { get => GoodsNomenclatureGroup; set => GoodsNomenclatureGroup = value; }
}

public partial class findLanguageByDatesResponseHistory : IDataGroup<language>
{
	[XmlIgnore]
	public language[] DataPoints { get => Language; set => Language = value; }
}

public partial class findMeasureActionByDatesResponse : IDataGroup<measureAction>
{
	[XmlIgnore]
	public measureAction[] DataPoints { get => MeasureAction; set => MeasureAction = value; }
}

public partial class findMeasureByDatesResponseHistory : IDataGroup<measure>
{
	[XmlIgnore]
	public measure[] DataPoints { get => Measure; set => Measure = value; }
}

public partial class findMeasureConditionCodeByDatesResponse : IDataGroup<measureConditionCode>
{
	[XmlIgnore]
	public measureConditionCode[] DataPoints { get => MeasureConditionCode; set => MeasureConditionCode = value; }
}

public partial class findMeasureTypeByDatesResponse : IDataGroup<measureType>
{
	[XmlIgnore]
	public measureType[] DataPoints { get => MeasureType; set => MeasureType = value; }
}

public partial class findMeasureTypeSeriesByDatesResponse : IDataGroup<measureTypeSeries>
{
	[XmlIgnore]
	public measureTypeSeries[] DataPoints { get => MeasureTypeSeries; set => MeasureTypeSeries = value; }
}

public partial class findMeasurementUnitByDatesResponse : IDataGroup<measurementUnit>
{
	[XmlIgnore]
	public measurementUnit[] DataPoints { get => MeasurementUnit; set => MeasurementUnit = value; }
}

public partial class findMeasurementUnitQualifierByDatesResponse : IDataGroup<measurementUnitQualifier>
{
	[XmlIgnore]
	public measurementUnitQualifier[] DataPoints { get => MeasurementUnitQualifier; set => MeasurementUnitQualifier = value; }
}

public partial class findMeursingAdditionalCodeByDatesResponseHistory : IDataGroup<meursingAdditionalCode>
{
	[XmlIgnore]
	public meursingAdditionalCode[] DataPoints { get => MeursingAdditionalCode; set => MeursingAdditionalCode = value; }
}

public partial class findMeursingTablePlanByDatesResponseHistory : IDataGroup<meursingTablePlan>
{
	[XmlIgnore]
	public meursingTablePlan[] DataPoints { get => MeursingTablePlan; set => MeursingTablePlan = value; }
}

public partial class findModificationRegulationByDatesResponseHistory : IDataGroup<modificationRegulation>
{
	[XmlIgnore]
	public modificationRegulation[] DataPoints { get => ModificationRegulation; set => ModificationRegulation = value; }
}

public partial class findMonetaryExchangePeriodByDatesResponseHistory : IDataGroup<monetaryExchangePeriod>
{
	[XmlIgnore]
	public monetaryExchangePeriod[] DataPoints { get => MonetaryExchangePeriod; set => MonetaryExchangePeriod = value; }
}

public partial class findMonetaryPlaceOfPublicationByDatesResponseHistory : IDataGroup<monetaryPlaceOfPublication>
{
	[XmlIgnore]
	public monetaryPlaceOfPublication[] DataPoints { get => MonetaryPlaceOfPublication; set => MonetaryPlaceOfPublication = value; }
}

public partial class findMonetaryUnitByDatesResponse : IDataGroup<monetaryUnit>
{
	[XmlIgnore]
	public monetaryUnit[] DataPoints { get => MonetaryUnit; set => MonetaryUnit = value; }
}

public partial class findProrogationRegulationByDatesResponseHistory : IDataGroup<prorogationRegulation>
{
	[XmlIgnore]
	public prorogationRegulation[] DataPoints { get => ProrogationRegulation; set => ProrogationRegulation = value; }
}

public partial class findPublicationSigleByDatesResponseHistory : IDataGroup<publicationSigle>
{
	[XmlIgnore]
	public publicationSigle[] DataPoints { get => PublicationSigle; set => PublicationSigle = value; }
}

public partial class findQuotaDefinitionByDatesResponseHistory : IDataGroup<quotaDefinition>
{
	[XmlIgnore]
	public quotaDefinition[] DataPoints { get => QuotaDefinition; set => QuotaDefinition = value; }
}

public partial class findQuotaOrderNumberByDatesResponseHistory : IDataGroup<quotaOrderNumber>
{
	[XmlIgnore]
	public quotaOrderNumber[] DataPoints { get => QuotaOrderNumber; set => QuotaOrderNumber = value; }
}

public partial class findRegulationGroupByDatesResponseHistory : IDataGroup<regulationGroup>
{
	[XmlIgnore]
	public regulationGroup[] DataPoints { get => RegulationGroup; set => RegulationGroup = value; }
}

public partial class findRegulationReplacementByDatesResponseHistory : IDataGroup<regulationReplacement>
{
	[XmlIgnore]
	public regulationReplacement[] DataPoints { get => RegulationReplacement; set => RegulationReplacement = value; }
}

public partial class findRegulationRoleTypeByDatesResponseHistory : IDataGroup<regulationRoleType>
{
	[XmlIgnore]
	public regulationRoleType[] DataPoints { get => RegulationRoleType; set => RegulationRoleType = value; }
}

public partial class findSensitiveGoodByDatesResponseHistory : IDataGroup<sensitiveGood>
{
	[XmlIgnore]
	public sensitiveGood[] DataPoints { get => SensitiveGood; set => SensitiveGood = value; }
}
