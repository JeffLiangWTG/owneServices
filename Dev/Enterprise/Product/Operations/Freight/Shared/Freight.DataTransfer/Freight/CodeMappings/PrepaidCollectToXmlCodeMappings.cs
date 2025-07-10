using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[Immutable]
	public class PrepaidCollectToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		PrepaidCollectToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.PaymentType.Prepaid, nameof(Xsd.PaymentType.PPD));
			yield return new Mapping(Core.Constants.PaymentType.Collect, nameof(Xsd.PaymentType.CCX));
		}

		public static readonly PrepaidCollectToXmlCodeMappings Instance = new PrepaidCollectToXmlCodeMappings();

		protected override string Name
		{
			get { return "Prepaid/Collect"; }
		}

		public new Xsd.PaymentType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.PaymentType.PPD, errorContext, notify);
		}
	}
}
