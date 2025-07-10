using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class PowerOfAttorneyValidator : Customs.Business.AuthorityToActValidator
	{
		public void Validate(CusISFHeader header, OrgHeader organisationBeingValidated, ZPropertyInfo propertyBeingValidated)
		{
			if (organisationBeingValidated != null)
			{
				bool hasPowerOfAttorney = false;
				foreach (string code in new string[] { Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms })
				{
					JobRequiredDocument poa = GetPowerOfAttorney(organisationBeingValidated.RequiredDocuments, IsPOAForImport, code);
					if (poa != null)
					{
						hasPowerOfAttorney = ValidatePowerOfAttorneyDocumentDates(propertyBeingValidated, poa, "organization");
					}
				}

				if (!hasPowerOfAttorney)
				{
					JobRequiredDocument poa = GetPowerOfAttorney(header.RequiredDocuments, IsPOAForImport, Core.Constants.RefDocTypes.PowerOfAttorney) ?? GetPowerOfAttorney(header.RequiredDocuments, IsPOAForImport, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
					if (poa == null)
					{
						var notificationType = CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType(header.RegistryCompanyPK);
						propertyBeingValidated.AddNotification(notificationType, GetNoPOADocumentForImporterString(CountrySpecificNameForPOA));
					}
					else
					{
						ValidatePowerOfAttorneyDocumentDates(propertyBeingValidated, poa, "ISF");
					}
				}
			}
		}

		bool IsPOAForImport(JobRequiredDocument doc)
		{
			return !doc.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.Direction) || doc.Attributes[JobRequiredDocAttribTypeList.Codes.Direction, ImportExportCodeList.Codes.Import] != null
				|| doc.Attributes[JobRequiredDocAttribTypeList.Codes.Direction, ImportExportCodeList.Codes.ImportISF] != null;
		}

		JobRequiredDocument GetPowerOfAttorney(JobRequiredDocumentDependentCollection requiredDocuments, Predicate<JobRequiredDocument> extraMatch, string code)
		{
			JobRequiredDocument poa = requiredDocuments.GetDocByType(code, Core.Constants.CountryCodes.UnitedStates, new Predicate<JobRequiredDocument>(x => (x.EQ_ValidToDate >= ZDateTime.Today || x.EQ_ValidToDate.IsEmpty) && x.Attributes.Count == 0));
			if (poa == null)
			{
				poa = requiredDocuments.GetDocByType(code, Core.Constants.CountryCodes.UnitedStates, extraMatch);
				if (poa == null)
				{
					poa = requiredDocuments.GetDocByType(code, Core.Constants.CountryCodes.UnitedStates, new Predicate<JobRequiredDocument>(x => x.Attributes.Count == 0));
				}
			}
			return poa;
		}
	}
}
