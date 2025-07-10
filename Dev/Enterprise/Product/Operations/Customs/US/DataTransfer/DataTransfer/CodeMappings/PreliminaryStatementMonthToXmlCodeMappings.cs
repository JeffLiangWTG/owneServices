using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	class PreliminaryStatementMonthToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		PreliminaryStatementMonthToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(MonthList.Codes._01, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item01));
			yield return new Mapping(MonthList.Codes._02, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item02));
			yield return new Mapping(MonthList.Codes._03, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item03));
			yield return new Mapping(MonthList.Codes._04, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item04));
			yield return new Mapping(MonthList.Codes._05, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item05));
			yield return new Mapping(MonthList.Codes._06, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item06));
			yield return new Mapping(MonthList.Codes._07, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item07));
			yield return new Mapping(MonthList.Codes._08, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item08));
			yield return new Mapping(MonthList.Codes._09, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item09));
			yield return new Mapping(MonthList.Codes._10, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item10));
			yield return new Mapping(MonthList.Codes._11, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item11));
			yield return new Mapping(MonthList.Codes._12, nameof(Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item12));
		}

		public static readonly PreliminaryStatementMonthToXmlCodeMappings Instance = new PreliminaryStatementMonthToXmlCodeMappings();

		protected override string Name
		{
			get { return "Declaration Preliminary Statement Month"; }
		}

		public new Xsd.USDeclarationPaymentPreliminaryStatementMonth GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USDeclarationPaymentPreliminaryStatementMonth.Item01, errorContext, notify);
		}
	}
}
