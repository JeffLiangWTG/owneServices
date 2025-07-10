using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class EntryTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		EntryTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(SubmissionTypeList.Codes.ISF5, nameof(Xsd.ISFSubmissionType.ISF5));
			yield return new Mapping(SubmissionTypeList.Codes.ISF10, nameof(Xsd.ISFSubmissionType.ISF10));
			yield return new Mapping(SubmissionTypeList.Codes.ISF10ToISF5, nameof(Xsd.ISFSubmissionType.ISF10ToISF5));
			yield return new Mapping(SubmissionTypeList.Codes.ISF5ToISF10, nameof(Xsd.ISFSubmissionType.ISF5ToISF10));
			yield return new Mapping(SubmissionTypeList.Codes.LateISF10, nameof(Xsd.ISFSubmissionType.LateISF10));
			yield return new Mapping(SubmissionTypeList.Codes.LateISF5, nameof(Xsd.ISFSubmissionType.LateISF5));
		}

		public static readonly EntryTypeToXmlCodeMappings Instance = new EntryTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Entry Type"; }
		}

		public new Xsd.ISFSubmissionType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFSubmissionType.ISF10, errorContext, notify);
		}
	}
}
