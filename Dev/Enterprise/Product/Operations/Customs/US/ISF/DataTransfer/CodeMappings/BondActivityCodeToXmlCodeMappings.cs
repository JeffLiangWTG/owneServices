using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class BondActivityCodeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		BondActivityCodeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ISFBondActivityCodeList.Codes.ImporterOrBroker, nameof(Xsd.ISFBondActivityCode.Item01));
			yield return new Mapping(ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise, nameof(Xsd.ISFBondActivityCode.Item02));
			yield return new Mapping(ISFBondActivityCodeList.Codes.InternationalCarrier, nameof(Xsd.ISFBondActivityCode.Item03));
			yield return new Mapping(ISFBondActivityCodeList.Codes.ForeignTradeZoneOperator, nameof(Xsd.ISFBondActivityCode.Item04));
			yield return new Mapping(ISFBondActivityCodeList.Codes.ISFBond16, nameof(Xsd.ISFBondActivityCode.Item16));
			yield return new Mapping(ISFBondActivityCodeList.Codes.ISFBond99, nameof(Xsd.ISFBondActivityCode.Item99));
		}

		public static readonly BondActivityCodeToXmlCodeMappings Instance = new BondActivityCodeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Bond Activity Code"; }
		}

		public new Xsd.ISFBondActivityCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFBondActivityCode.Item01, errorContext, notify);
		}
	}
}
