using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	[Immutable]
	public class CostDistributionMechanismListToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		CostDistributionMechanismListToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(CostDistributionMechanismList.Codes.Actual, nameof(Xsd.LandedCostDistributionCode.AWV));
			yield return new Mapping(CostDistributionMechanismList.Codes.ActualVolume, nameof(Xsd.LandedCostDistributionCode.VOL));
			yield return new Mapping(CostDistributionMechanismList.Codes.ActualWeight, nameof(Xsd.LandedCostDistributionCode.WGT));
			yield return new Mapping(CostDistributionMechanismList.Codes.Item, nameof(Xsd.LandedCostDistributionCode.ITM));
			yield return new Mapping(CostDistributionMechanismList.Codes.LineValue, nameof(Xsd.LandedCostDistributionCode.VAV));
		}

		public static readonly CostDistributionMechanismListToXmlCodeMappings Instance = new CostDistributionMechanismListToXmlCodeMappings();

		public new Xsd.LandedCostDistributionCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.LandedCostDistributionCode.AWV, errorContext, notifications);
		}

		protected override string Name
		{
			get { return Res.GetString("3cb31b55-2a82-40ec-ab7b-24f9aef5a869", "Landed Costing Distribution Code"); }
		}
	}
}
