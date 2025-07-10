using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	class PowerOfAttorneyValidator : AuthorityToActValidator
	{
		public PowerOfAttorneyValidator(string noPOADocumentForImporterOverrideString) : base("Power of Attorney", noPOADocumentForImporterOverrideString)
		{
		}

		public void ValidatePOADates(BaseJobDeclaration declaration, OrgHeader organisationBeingValidated, ZPropertyInfo propertyBeingValidated, string powerOfAttorneyCode, string powerOfAttorneyCustoms, string powerOfAttorneyForwarding, Predicate<JobRequiredDocument> extraMatch)
		{
			var powerOfAttorneyCodes = new string[] { powerOfAttorneyCode, powerOfAttorneyCustoms, powerOfAttorneyForwarding };
			var poasMatchExtraCriteria = base.GetAllPOAMatchingExtraCriteria(declaration, organisationBeingValidated, powerOfAttorneyCodes, extraMatch);
			var poaFromCartageAndDocs = base.GetAllPOAFromCartageAndDocs(declaration, powerOfAttorneyCodes);
			base.ValidatePOADates(propertyBeingValidated, poasMatchExtraCriteria, poaFromCartageAndDocs);
		}

		public static List<(Predicate<JobRequiredDocument>, string)> ExtraMatchingConditionForExportDirection()
		{
			return new List<(Predicate<JobRequiredDocument>, string)>()
			{
				HasNoDirectionAttributeOrHasMatchedDirectionAttribute(ImportExportCodeList.Codes.Export)
			};
		}

		public static List<(Predicate<JobRequiredDocument>, string)> ExtraMatchingConditionForImportDirectionAndPortOfEntry(string portOfEntry)
		{
			return new List<(Predicate<JobRequiredDocument>, string)>()
			{
				HasNoDirectionAttributeOrHasMatchedDirectionAttribute(ImportExportCodeList.Codes.Import),
				HasNoPortOfEntryAttributeOrHasMatchedPortOfEntryAttribute(portOfEntry)
			};
		}

		public const string DirectionOrPortOfEntryNotMatchForReconAndDrawback = "US and/or this direction";
	}
}
