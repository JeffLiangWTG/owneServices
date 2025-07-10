using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[PreventDelete(false)]
	public class AccOrgTaxConfigurationTemplate : AutoAccOrgTaxConfigurationTemplate, ITemplateCopyable, IDocManagerSupport, IEDocsParsingSupport
	{
		public AccOrgTaxConfigurationTemplate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ReadOnly(true)]
		public override ZBool OCT_IsReceivable
		{
			get { return base.OCT_IsReceivable; }
			set
			{
				if (value != OCT_IsReceivable)
				{
					base.OCT_IsReceivable = value;

					if (accOrgTaxConfigurations != null)
					{
						AccOrgTaxConfigurations.DeleteAll();
						accOrgTaxConfigurations = null;
					}
				}
			}
		}

		[ReadOnly(true)]
		public ZBool OCT_IsPayable => !OCT_IsReceivable;

		public ZPropertyInfo OCT_IsPayableInfo
		{
			get { return GetZPropertyInfo(nameof(OCT_IsPayable)); }
		}

		public ZString TemplateType => new ZString(OCT_IsReceivable ?
			AccountingMasterFilesConstants.AccOrgTaxConfigurationTemplateTypes.ReceivablesOrganizationsTemplate.CodeAndDescription :
			AccountingMasterFilesConstants.AccOrgTaxConfigurationTemplateTypes.PayablesOrganizationsTemplate.CodeAndDescription);

		protected override ZString HumanReadableNameCore => OCT_IsReceivable ?
			AccountingMasterFilesConstants.AccOrgTaxConfigurationTemplateTypes.ReceivablesOrganizationsTemplate.Description :
			AccountingMasterFilesConstants.AccOrgTaxConfigurationTemplateTypes.PayablesOrganizationsTemplate.Description;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			OCT_GC_Company = GlbCompany.CurrentCompany.PK;
		}

		public string Ledger => OCT_IsReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;

		public IBusiness TemplateCopy()
		{
			var result = (AccOrgTaxConfigurationTemplate)Clone();
			using (result.GetValidationSuspender())
			{
				foreach (var sourceTaxConfig in AccOrgTaxConfigurations)
				{
					var targetTaxConfig = result.AccOrgTaxConfigurations.AddNew();
					targetTaxConfig.CopyEditableColumns(sourceTaxConfig);
					targetTaxConfig.OTC_ETC = sourceTaxConfig.OTC_ETC;
				}
			}

			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ChildEditable(true)]
		public AccOrgTaxConfigurationTemplateItemCollection AccOrgTaxConfigurations
		{
			get
			{
				if (accOrgTaxConfigurations == null)
				{
					accOrgTaxConfigurations = new AccOrgTaxConfigurationTemplateItemCollection(this);
					RegisterEditableChildObject(accOrgTaxConfigurations);
				}

				return accOrgTaxConfigurations;
			}
		}
		AccOrgTaxConfigurationTemplateItemCollection accOrgTaxConfigurations;

		public override void Delete()
		{
			ClearLinkedOrganizationForeionKey();
			AccOrgTaxConfigurations.DeleteAll();
			base.Delete();
		}

		void ClearLinkedOrganizationForeionKey()
		{
			var columnName = OCT_IsReceivable
				? OrgCompanyDataSchema.OB_OCT_ARTaxTemplate.Name
				: OrgCompanyDataSchema.OB_OCT_APTaxTemplate.Name;

			var sql = $@"UPDATE {OrgCompanyDataSchema.Constants.SqlSchemaName}.{OrgCompanyDataSchema.Constants.TableName}
SET
	{columnName} = null
WHERE
	{columnName} = @TemplatePK";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@TemplatePK", SqlDbType.UniqueIdentifier, PK.ToGuid());
				command.ExecuteNonQuery();
			}
		}

		[ChildEditable(false)]
		public AccOrgTaxConfigurationTemplateLinkedOrganisationCollection LinkedOrganisations
		{
			get
			{
				if (linkedOrganisations == null)
				{
					linkedOrganisations = new AccOrgTaxConfigurationTemplateLinkedOrganisationCollection(this);
					RegisterEditableChildObject(linkedOrganisations);
				}
				return linkedOrganisations;
			}
		}
		AccOrgTaxConfigurationTemplateLinkedOrganisationCollection linkedOrganisations;

		public OrgHeaderCollection FindBoxCollection
		{
			get
			{
				if (findBoxCollection == null)
				{
					if (OCT_IsReceivable)
					{
						findBoxCollection = new DebtorCollection(Factory);
					}
					else
					{
						findBoxCollection = new CreditorCollection(Factory);
					}
				}
				return findBoxCollection;
			}
		}
		OrgHeaderCollection findBoxCollection;

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.TaxConfigurationTemplate);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
	}
}
