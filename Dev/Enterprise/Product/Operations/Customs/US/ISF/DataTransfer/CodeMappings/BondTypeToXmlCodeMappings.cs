using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class BondTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		BondTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ImporterBondTypeList.Codes.ContinuousBond, nameof(Xsd.ISFBondType.Item8));
			yield return new Mapping(ImporterBondTypeList.Codes.SingleTransactionBond, nameof(Xsd.ISFBondType.Item9));
		}

		public static readonly BondTypeToXmlCodeMappings Instance = new BondTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Bond Type"; }
		}

		public new Xsd.ISFBondType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFBondType.Item8, errorContext, notify);
		}
	}
}
