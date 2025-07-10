using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Module
{
	public class AuditClassificationLinesMethodApplicator : OperationalActionMethodApplicator
	{
		public AuditClassificationLinesMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var orgSupplierParts = targets.OfType<OrgSupplierPart>().ToArray();
			log.SetSectionProgressMax(orgSupplierParts.Length);
			AuditClassificationLines(log, orgSupplierParts);
		}

		protected void AuditClassificationLines(IOperationalActionSectionLog log, OrgSupplierPart[] orgSuppliers, bool skipDialogBoxForm = false)
		{
			var defaultReference = ZString.Empty;
			foreach (var orgSupplier in orgSuppliers)
			{
				foreach (var classPartPivot in GetPivots(orgSupplier))
				{
					Security.SecurityCheckpoint security = null;
					if (classPartPivot.IsHTB)
					{
						security = Env.Security.CustomsSupplierPartAuditImport.IsAllowed ? Env.Security.CustomsSupplierPartAuditExport : Env.Security.CustomsSupplierPartAuditImport;
					}
					else if (classPartPivot.IsImportClassification)
					{
						security = Env.Security.CustomsSupplierPartAuditImport;
					}
					else if (classPartPivot.IsExportClassification)
					{
						security = Env.Security.CustomsSupplierPartAuditExport;
					}

					if (security != null)
					{
						if (!security.IsAllowed)
						{
							log.Notify(OperationalActionLogErrorLevel.Warning,
								Res.GetString("764ceda7-30f6-454d-a6fa-aa3a1a0b35a6",
								"Failed to audit Classification Line. Details: Product Code: '{0}', {1}. Reason: {2}.",
								orgSupplier.OP_PartNum,
								((IBusiness)classPartPivot).HumanReadableName,
								security.ErrorMessageForNotAllowed));
						}
						else
						{
							var caption = Res.GetString("f63beb28-39d4-4cfb-a36d-5ed9551e745c", "Audit Classification Lookup");
							var recordTypeName = Res.GetString("69087d6f-2090-4a89-9f01-d8fc2fc2c62f", "Classification Pivot");
							defaultReference = WriteToLogFormInvoker.ShowWriteToLogForm(orgSupplier, (EnterpriseBusinessObject)classPartPivot, recordTypeName, security, caption, defaultReference, new BusinessObjectLoggerOptions(), Globals.Message.ShowInformation, x => skipDialogBoxForm = x, skipDialogBoxForm);

							if (skipDialogBoxForm)
							{
								log.Notify(OperationalActionLogErrorLevel.Informational,
									Res.GetString("b0da37e7-6f59-4c3e-9dc9-0aafd71d6489",
									"Audit Classification Line successfully. Details: Product Code: '{0}', {1}.",
									orgSupplier.OP_PartNum,
									((IBusiness)classPartPivot).HumanReadableName));
							}
							else
							{
								log.Notify(OperationalActionLogErrorLevel.Warning,
								Res.GetString("7d6f4cf1-0db3-4399-b9d3-0056bfaa99fb",
								"Failed to audit Classification Line. Details: Product Code: '{0}', {1}. Reason: Failed to log.",
								orgSupplier.OP_PartNum,
								((IBusiness)classPartPivot).HumanReadableName));
							}
						}
					}
				}
				log.BumpSectionProgress();
			}
		}

		IBaseCusClassPartPivot[] GetPivots(OrgSupplierPart orgSupplier)
		{
			var partPivotFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, orgSupplier.PK);
			partPivotFilter.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			partPivotFilter.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			return orgSupplier.Factory.Load<IBaseCusClassPartPivot>(partPivotFilter);
		}
	}
}
