namespace CargoWise.RefDbRepo.SEReferenceData.Services.IncrementalExport
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.arcticgroup.se/tariff/arctictariff/export")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.arcticgroup.se/tariff/arctictariff/export", IsNullable = false)]
	public partial class export
	{

		/// <remarks/>
		public string id;

		/// <remarks/>
		public string exportType;

		/// <remarks/>
		public parameters parameters;

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("record", IsNullable = false)]
		public IncrementalExport.record[] items;

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string[] Text;
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.arcticgroup.se/tariff/arctictariff/export")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.arcticgroup.se/tariff/arctictariff/export", IsNullable = false)]
#pragma warning disable CS8860 // Types and aliases should not be named 'record'.
	public partial class record
#pragma warning restore CS8860 // Types and aliases should not be named 'record'.
	{

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("additionalCode", typeof(additionalCode))]
		[System.Xml.Serialization.XmlElementAttribute("baseRegulation", typeof(baseRegulation))]
		[System.Xml.Serialization.XmlElementAttribute("certificate", typeof(certificate))]
		[System.Xml.Serialization.XmlElementAttribute("exportRefundNomenclature", typeof(exportRefundNomenclature))]
		[System.Xml.Serialization.XmlElementAttribute("footnote", typeof(footnote))]
		[System.Xml.Serialization.XmlElementAttribute("fullTemporaryStopRegulation", typeof(fullTemporaryStopRegulation))]
		[System.Xml.Serialization.XmlElementAttribute("geographicalArea", typeof(geographicalArea))]
		[System.Xml.Serialization.XmlElementAttribute("goodsNomenclature", typeof(goodsNomenclature))]
		[System.Xml.Serialization.XmlElementAttribute("goodsNomenclatureGroup", typeof(goodsNomenclatureGroup))]
		[System.Xml.Serialization.XmlElementAttribute("lookupTable", typeof(lookupTable))]
		[System.Xml.Serialization.XmlElementAttribute("measure", typeof(measure))]
		[System.Xml.Serialization.XmlElementAttribute("measureAction", typeof(measureAction))]
		[System.Xml.Serialization.XmlElementAttribute("measureConditionCode", typeof(measureConditionCode))]
		[System.Xml.Serialization.XmlElementAttribute("measureType", typeof(measureType1))]
		[System.Xml.Serialization.XmlElementAttribute("measurement", typeof(measurement))]
		[System.Xml.Serialization.XmlElementAttribute("measurementUnit", typeof(measurementUnit))]
		[System.Xml.Serialization.XmlElementAttribute("measurementUnitQualifier", typeof(measurementUnitQualifier))]
		[System.Xml.Serialization.XmlElementAttribute("meursingAdditionalCode", typeof(meursingAdditionalCode))]
		[System.Xml.Serialization.XmlElementAttribute("meursingHeading", typeof(meursingHeading))]
		[System.Xml.Serialization.XmlElementAttribute("meursingSubheading", typeof(meursingSubheading))]
		[System.Xml.Serialization.XmlElementAttribute("meursingTablePlan", typeof(meursingTablePlan))]
		[System.Xml.Serialization.XmlElementAttribute("modificationRegulation", typeof(modificationRegulation))]
		[System.Xml.Serialization.XmlElementAttribute("monetaryExchangePeriod", typeof(monetaryExchangePeriod))]
		[System.Xml.Serialization.XmlElementAttribute("preferenceCode", typeof(preferenceCode))]
		[System.Xml.Serialization.XmlElementAttribute("quotaBalanceEvent", typeof(quotaBalanceEvent))]
		[System.Xml.Serialization.XmlElementAttribute("quotaClosedAndBalanceTransferredEvent", typeof(quotaClosedAndBalanceTransferredEvent))]
		[System.Xml.Serialization.XmlElementAttribute("quotaCriticalEvent", typeof(quotaCriticalEvent))]
		[System.Xml.Serialization.XmlElementAttribute("quotaDefinition", typeof(quotaDefinition))]
		[System.Xml.Serialization.XmlElementAttribute("quotaExhaustionEvent", typeof(quotaExhaustionEvent))]
		[System.Xml.Serialization.XmlElementAttribute("quotaOrderNumber", typeof(quotaOrderNumber1))]
		[System.Xml.Serialization.XmlElementAttribute("quotaReopeningEvent", typeof(quotaReopeningEvent))]
		[System.Xml.Serialization.XmlElementAttribute("quotaUnblockingEvent", typeof(quotaUnblockingEvent))]
		[System.Xml.Serialization.XmlElementAttribute("quotaUnsuspensionEvent", typeof(quotaUnsuspensionEvent))]
		[System.Xml.Serialization.XmlElementAttribute("reliefCode", typeof(reliefCode))]
		[System.Xml.Serialization.XmlElementAttribute("taxCode", typeof(taxCode))]
		public object Item;

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string[] Text;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public long recordId;

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool recordIdSpecified;
	}
}
