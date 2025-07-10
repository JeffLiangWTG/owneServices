using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class PowerOfAttorneyValidator : Customs.Business.AuthorityToActValidator
	{
		public void Validate(CusInBondHeader cusInBondHeader, OrgHeader organisationBeingValidated, ZPropertyInfo propertyBeingValidated)
		{
			try
			{
				organisation = organisationBeingValidated;
				if (organisation != null)
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
						var header = cusInBondHeader;
						JobRequiredDocument poa = null;
						if (header != null)
						{
							poa = GetPowerOfAttorney(header.RequiredDocuments, IsPOAForImport, Core.Constants.RefDocTypes.PowerOfAttorney);
							if (poa == null)
							{
								poa = GetPowerOfAttorney(header.RequiredDocuments, IsPOAForImport, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
							}
						}
						if (poa == null)
						{
							var notificationType = CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType();
							propertyBeingValidated.AddNotification(notificationType, GetNoPOADocumentForImporterString(CountrySpecificNameForPOA));
						}
						else
						{
							ValidatePowerOfAttorneyDocumentDates(propertyBeingValidated, poa, "In-Bond");
						}
					}
				}
			}
			finally
			{
				organisation = null;
			}
		}

		public void Validate(CusInBondMoveHeader moveHeader, OrgHeader organisationBeingValidated, ZPropertyInfo propertyBeingValidated)
		{
			Validate(moveHeader.Header, organisationBeingValidated, propertyBeingValidated);
		}
		OrgHeader organisation;

		bool IsPOAForImport(JobRequiredDocument doc)
		{
			bool result = doc.EQ_OH_DocumentOwner.IsEmpty || doc.EQ_OH_DocumentOwner == organisation.PK;
			if (result)
			{
				result = !doc.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.Direction) || doc.Attributes[JobRequiredDocAttribTypeList.Codes.Direction, ImportExportCodeList.Codes.Import] != null;
			}
			return result;
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
