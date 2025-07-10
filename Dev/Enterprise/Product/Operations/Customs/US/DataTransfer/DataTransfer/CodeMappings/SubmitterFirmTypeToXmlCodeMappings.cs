using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class SubmitterFirmTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		SubmitterFirmTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(SubmitterFirmTypeList.Codes.Carrier, nameof(Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType.C));
			yield return new Mapping(SubmitterFirmTypeList.Codes.F, nameof(Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType.F));
			yield return new Mapping(SubmitterFirmTypeList.Codes.I, nameof(Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType.I));
			yield return new Mapping(SubmitterFirmTypeList.Codes.M, nameof(Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType.M));
			yield return new Mapping(SubmitterFirmTypeList.Codes.S, nameof(Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType.S));
			yield return new Mapping(SubmitterFirmTypeList.Codes.U, nameof(Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType.U));
		}

		public static readonly SubmitterFirmTypeToXmlCodeMappings Instance = new SubmitterFirmTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "FDA Submitter Firm Type"; }
		}

		public new Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType.C, errorContext, notify);
		}
	}
}
