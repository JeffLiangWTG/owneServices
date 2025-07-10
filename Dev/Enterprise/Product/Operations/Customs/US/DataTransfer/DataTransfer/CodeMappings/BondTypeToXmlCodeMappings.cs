using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class BondTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		BondTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(BondTypeList.Codes.NoBondRequired, nameof(Xsd.USDeclarationBondType.Item0));
			yield return new Mapping(BondTypeList.Codes.ContinuousBond, nameof(Xsd.USDeclarationBondType.Item8));
			yield return new Mapping(BondTypeList.Codes.SingleTransactionBond, nameof(Xsd.USDeclarationBondType.Item9));
		}

		public static readonly BondTypeToXmlCodeMappings Instance = new BondTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "Declaration Bond Type"; }
		}

		public new Xsd.USDeclarationBondType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USDeclarationBondType.Item0, errorContext, notify);
		}
	}
}
