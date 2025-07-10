using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class DispositionCodeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		DispositionCodeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(DispositionCodeList.Codes._2O, nameof(Xsd.ISFDispositionCode.Item2O));
			yield return new Mapping(DispositionCodeList.Codes._2P, nameof(Xsd.ISFDispositionCode.Item2P));
			yield return new Mapping(DispositionCodeList.Codes._2Q, nameof(Xsd.ISFDispositionCode.Item2Q));
			yield return new Mapping(DispositionCodeList.Codes._2R, nameof(Xsd.ISFDispositionCode.Item2R));
			yield return new Mapping(DispositionCodeList.Codes._4O, nameof(Xsd.ISFDispositionCode.Item4O));
			yield return new Mapping(DispositionCodeList.Codes._4P, nameof(Xsd.ISFDispositionCode.Item4P));
			yield return new Mapping(DispositionCodeList.Codes._4Q, nameof(Xsd.ISFDispositionCode.Item4Q));
			yield return new Mapping(DispositionCodeList.Codes._4R, nameof(Xsd.ISFDispositionCode.Item4R));
			yield return new Mapping(DispositionCodeList.Codes.S1, nameof(Xsd.ISFDispositionCode.S1));
			yield return new Mapping(DispositionCodeList.Codes.S2, nameof(Xsd.ISFDispositionCode.S2));
			yield return new Mapping(DispositionCodeList.Codes.S3, nameof(Xsd.ISFDispositionCode.S3));
			yield return new Mapping(DispositionCodeList.Codes.S4, nameof(Xsd.ISFDispositionCode.S4));
			yield return new Mapping(DispositionCodeList.Codes.S5, nameof(Xsd.ISFDispositionCode.S5));
			yield return new Mapping(DispositionCodeList.Codes.S6, nameof(Xsd.ISFDispositionCode.S6));
			yield return new Mapping(DispositionCodeList.Codes.S7, nameof(Xsd.ISFDispositionCode.S7));
			yield return new Mapping(DispositionCodeList.Codes.SA, nameof(Xsd.ISFDispositionCode.SA));
			yield return new Mapping(DispositionCodeList.Codes.SB, nameof(Xsd.ISFDispositionCode.SB));
			yield return new Mapping(DispositionCodeList.Codes.SC, nameof(Xsd.ISFDispositionCode.SC));
			yield return new Mapping(DispositionCodeList.Codes.XX, nameof(Xsd.ISFDispositionCode.XX));
		}

		public static readonly DispositionCodeToXmlCodeMappings Instance = new DispositionCodeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Disposition Code"; }
		}

		public new Xsd.ISFDispositionCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFDispositionCode.S1, errorContext, notify);
		}
	}
}
