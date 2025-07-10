using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GlobalCreditControlledDocumentsApprovalMissingExRateEmail : AccountingEmailDef
	{
		public GlobalCreditControlledDocumentsApprovalMissingExRateEmail(OrgHeader orgHeader, BusinessObject businessObject, ZGuid menuItemPK)
			: base()
		{
			ContentType = EmailContentTypes.HTML;
			subject = GetSubjectCore(orgHeader);
			body = GetBodyCore(orgHeader, businessObject, menuItemPK);
		}

		readonly string subject;
		readonly string body;

		protected override GuidRegistryItem Recipient => AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup;

		protected override string GetBody()
		{
			return body;
		}

		string GetBodyCore(OrgHeader org, BusinessObject businessObject, ZGuid menuItemPK)
		{
			var documentName = org.Factory.Load<StmMenuItem>(menuItemPK)?.SU_MenuName ?? string.Empty;
			var jobNumber = (businessObject as ICreditControlledDocumentDelivery)?.JobNumber.FirstOrDefault() ?? string.Empty;

			return String.Format(CultureInfo.InvariantCulture, @"{0}<br />
<br />
{1}",
				org.InvalidGlobalCreditCurrencyOrMissingExRateMessage.Replace(System.Environment.NewLine, "<br />"),
				Res.GetString("8c045608-1257-4c9f-bae4-d540d64cb07e", "System could not determine the Global Credit Limit for {0} while delivering the document: {1} for {2}.", org.OH_Code, documentName, jobNumber));
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(OrgHeader org)
		{
			return Res.GetString("7cbf98e4-836e-4cdd-b939-71d5c74f224e", "Missing Exchange Rate while determining Global Credit Limit for {0}", org.OH_Code);
		}
	}
}
