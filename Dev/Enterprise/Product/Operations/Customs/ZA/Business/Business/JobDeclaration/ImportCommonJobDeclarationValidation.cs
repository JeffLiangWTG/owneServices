using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	class ImportCommonJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportCommonJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		#region Implementation

		/// <Incident> I00025735 </Incident>
		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			OrgHeader importer = Parent.Importer;
			if (importer == null)
			{
				Parent.JE_OH_ImporterInfo.AddMessageError(ImporterMandatory);
			}
			else
			{
				ZString importerCode = importer.LocalCustomsClientCode;
				if (importerCode.IsEmpty)
				{
					Parent.JE_OH_ImporterInfo.AddMessageError(CustomsImporterCodeMessageError);
				}
				else if (!new SouthAfricanOrganisationCodeValidator().IsValid(importerCode))
				{
					Parent.JE_OH_ImporterInfo.AddMessageError(CustomsImporterCodeInvalid);
				}
				else if (importerCode.Equals(ValidationConstants.Declaration.UnregisteredTraderCustomsCode))
				{
					CheckUnregisteredTrader(importer, Parent.JE_OH_ImporterInfo, "Importer");
				}
			}
		}

		protected override void ValidateSupplierForWarehouseTransactions()
		{
		}

		public static string ImporterMandatory
		{
			get { return Res.GetString("024bbd1e-cc6c-40c7-87ac-690d5dcf5d62", "Please enter an Importer"); }
		}
		public static string CustomsImporterCodeMessageError
		{
			get { return Res.GetString("adfea5c1-6fb9-4699-8f9d-507293345b24", "Importer does not have a customs code setup in Organization>Config"); }
		}
		public static string CustomsImporterCodeInvalid
		{
			get { return Res.GetString("e28aaed8-df0b-4deb-8b2f-3add012aecbd", "The currently selected importer has a Customs Importer code that is not valid. Please confirm that it has been entered correctly."); }
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			ListValidation.WarnIfInvalidCode(Parent.JE_RL_NKPortOfLoadingInfo, Parent.Lookups.PortOfLoadings);
		}

		protected override void CheckJE_OH_AgentOverride()
		{
			base.CheckJE_OH_AgentOverride();

			var targetInfo = Parent.JE_OH_AgentOverrideInfo;
			var agent = Parent.AgentOverride;
			if (agent != null)
			{
				var target = Parent.JE_OH_AgentOverride;
				var importerOrg = Parent.JE_OH_Importer;
				if (importerOrg != target)
				{
					var customOffice = Parent.JE_CustomsOffice;
					var financialAccountNumberPortMaps = Parent.FinancialAccountNumberPortMappings.Cast<FinancialAccountNumberPortMap>();
					if (financialAccountNumberPortMaps.Any(x => x.OrganizationPK == target && x.CustomsOfficeCode == customOffice && x.ImporterPays))
					{
						targetInfo.AddError("The Importer & Agent fields must have the same organization codes if the organization in the Agent field has been flagged as an Importer Pays Deferment.");
					}
				}
			}
		}

		protected override void CheckJE_BOESightDate()
		{
			base.CheckJE_BOESightDate();
			if (!Parent.JE_BOESightNumber.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JE_BOESightDateInfo);
			}
		}

		#endregion
	}
}
