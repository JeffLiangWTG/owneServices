using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using RefServiceLevel = WiseRates.Api.Model.RefServiceLevel;

namespace Enterprise.Rating.GUI
{
	public class OrganizationFormForCarrierServiceLevelsMapping
	{
		public OrganizationFormForCarrierServiceLevelsMapping(
			ZForm parentForm,
			OrgHeader carrier,
			RefServiceLevel[] universalServiceLevels,
			IEnumerable<UnmappedForeignCode> unmappedCarrierServiceLevels)
		{
			ParentForm = parentForm;
			Carrier = carrier;
			UniversalServiceLevels = universalServiceLevels;
			UnmappedCarrierServiceLevels = unmappedCarrierServiceLevels;
		}

		public bool ShowModal()
		{
			if (Carrier == null)
			{
				return false;
			}

			var organizationController = ZControllerFactory.Create(ControllerIDs.Organisation);
			organizationController.SetFormsModalTo(ParentForm);
			organizationController.ShowChildrenAsDialog = true;

			ZFormModaliser.SetDelegateToCallOnFormShown(PresetMappings);
			((IOrganisationController)organizationController).ShowForm(Carrier, OrganisationTabPages.ServiceLevel, FormAction.Edit);
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();

			Carrier.MiscServ.CarrierServiceLevels.Reload(false);
			var converter = new WiseRatesConverter(Carrier.Factory, new ElementaryLogger());
			var allServiceLevelsMapped = UnmappedCarrierServiceLevels.All(l =>
				string.IsNullOrEmpty(converter.ConvertServiceLevel(l.ForeignCode, null, Carrier).error));

			return allServiceLevelsMapped;
		}

		void PresetMappings(object form)
		{
			if (!(form is IZForm organizationForm))
			{
				return;
			}

			var orgInForm = organizationForm.BusinessEntityForPersistingForm as OrgHeader;
			if (orgInForm == null)
			{
				return;
			}

			if (UnmappedCarrierServiceLevels == null)
			{
				return;
			}

			var serviceLevelsCollection = orgInForm.MiscServ.CarrierServiceLevels;

			foreach (var unmappedCode in UnmappedCarrierServiceLevels)
			{
				var serviceLevelMapping = serviceLevelsCollection.AddNew();
				serviceLevelMapping.PL_Code = unmappedCode.ForeignCode;
				serviceLevelMapping.PL_CarrierServiceCode = unmappedCode.ForeignCode;

				var description = string.Empty;
				if (UniversalServiceLevels != null)
				{
					var universalServiceLevel = UniversalServiceLevels.FirstOrDefault(s => s.Code == unmappedCode.ForeignCode);
					description = universalServiceLevel?.Description ?? unmappedCode.ForeignCode;
				}

				var maxLength = OrgCarrierServiceLevelSchema.PL_CarrierServiceLevelDescription.MaxLength;
				serviceLevelMapping.PL_CarrierServiceLevelDescription = description.Substring(0, Math.Min(description.Length, maxLength));
			}
		}

		readonly ZForm ParentForm;
		readonly OrgHeader Carrier;
		readonly RefServiceLevel[] UniversalServiceLevels;
		readonly IEnumerable<UnmappedForeignCode> UnmappedCarrierServiceLevels;
	}
}
