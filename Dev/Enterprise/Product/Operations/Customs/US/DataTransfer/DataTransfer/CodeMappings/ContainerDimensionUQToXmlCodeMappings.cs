using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class ContainerDimensionUQToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ContainerDimensionUQToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(FDAMeasurementUnitList.Codes.Centimeters, nameof(Xsd.USFDAInnermostContainerDimensionUQ.C));
			yield return new Mapping(FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals, nameof(Xsd.USFDAInnermostContainerDimensionUQ.I));
			yield return new Mapping(FDAMeasurementUnitList.Codes.InchesWithOneTenthDecimals, nameof(Xsd.USFDAInnermostContainerDimensionUQ.D));
		}

		public static readonly ContainerDimensionUQToXmlCodeMappings Instance = new ContainerDimensionUQToXmlCodeMappings();

		protected override string Name
		{
			get { return "Export InBond Type"; }
		}

		public new Xsd.USFDAInnermostContainerDimensionUQ GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USFDAInnermostContainerDimensionUQ.C, errorContext, notify);
		}
	}
}
