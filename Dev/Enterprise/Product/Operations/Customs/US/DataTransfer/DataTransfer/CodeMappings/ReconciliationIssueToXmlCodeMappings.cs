using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class ReconciliationIssueToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ReconciliationIssueToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ReconIssueCodeList.Codes.ValueClass9802Recon, nameof(Xsd.USReconciliationIssue.AL));
			yield return new Mapping(ReconIssueCodeList.Codes.Class9802Recon, nameof(Xsd.USReconciliationIssue.C9));
			yield return new Mapping(ReconIssueCodeList.Codes.ClassRecon, nameof(Xsd.USReconciliationIssue.CL));
			yield return new Mapping(ReconIssueCodeList.Codes._9802Recon, nameof(Xsd.USReconciliationIssue.Item98));
			yield return new Mapping(ReconIssueCodeList.Codes.Value9802Recon, nameof(Xsd.USReconciliationIssue.V9));
			yield return new Mapping(ReconIssueCodeList.Codes.ValueClassRecon, nameof(Xsd.USReconciliationIssue.VC));
			yield return new Mapping(ReconIssueCodeList.Codes.ValueRecon, nameof(Xsd.USReconciliationIssue.VL));
			yield return new Mapping(ReconIssueCodeList.Codes.NotApplicable, nameof(Xsd.USReconciliationIssue.NA));
		}

		public static readonly ReconciliationIssueToXmlCodeMappings Instance = new ReconciliationIssueToXmlCodeMappings();

		protected override string Name
		{
			get { return "Reconciliation Issue"; }
		}

		public new Xsd.USReconciliationIssue GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USReconciliationIssue.AL, errorContext, notify);
		}
	}
}
