using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingMaintenanceUtilities<T> where T : BusinessObject, IPatternMatchingBusinessObjects
	{
		public PatternMatchingMaintenanceUtilities(BusinessObject bizO, ZBool shouldSuspendDeletion, ZInt hashedValue, ZGuid orgHeaderPk, ZGuid personPk, ZString countryCode)
		{
			this.bizO = bizO;
			this.shouldSuspendDeletion = shouldSuspendDeletion;
			this.hashedValue = hashedValue;
			this.orgHeaderPk = orgHeaderPk;
			this.personPk = personPk;
			this.countryCode = countryCode;
		}

		readonly BusinessObject bizO;
		readonly ZGuid orgHeaderPk;
		readonly ZGuid personPk;
		readonly ZString countryCode;
		readonly ZInt hashedValue;
		readonly ZBool shouldSuspendDeletion;

		public bool CreateOrUpdatePatternRecord(SchemaGuidColumn parentIdColumn, ZGuid parentPk)
		{
			T patternMatchingObject = null;
			if (!shouldSuspendDeletion)
			{
				var matchingPatterns = bizO.Factory.Load<T>(new ZQuery(parentIdColumn, parentPk));
				if (matchingPatterns.Length > 0)
				{
					patternMatchingObject = matchingPatterns[0];
					for (int i = 1; i < matchingPatterns.Length; i++)
					{
						matchingPatterns[i].Delete();
					}
				}
			}

			if (hashedValue != 0)
			{
				if (patternMatchingObject == null)
				{
					patternMatchingObject = bizO.Factory.New<T>();
				}

				if (!orgHeaderPk.IsEmpty)
				{
					patternMatchingObject.OrganisationPK = orgHeaderPk;
				}

				if (!personPk.IsEmpty)
				{
					patternMatchingObject.PersonPK = personPk;
				}

				patternMatchingObject.HashedValue = hashedValue;
				patternMatchingObject.ParentTableCode = bizO.TablePrefix;
				patternMatchingObject.ParentId = bizO.PK;
				patternMatchingObject.PatternMatchingCountryCode = countryCode;
				patternMatchingObject.IsActive = true;
			}

			return hashedValue != 0;
		}
	}
}
