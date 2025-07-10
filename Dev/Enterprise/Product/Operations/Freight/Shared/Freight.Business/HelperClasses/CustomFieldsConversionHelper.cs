using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public static class CustomFieldsConversionHelper
	{
		public static bool MoveCustomFields(BusinessObject fromBizObj, BusinessObject toBizObj)
		{
			return fromBizObj != null && toBizObj != null && MoveCustomFields(toBizObj.Factory, fromBizObj.TablePrefix, fromBizObj.PK, toBizObj.TablePrefix, toBizObj.PK);
		}

		public static bool MoveCustomFields(BusinessObjectFactory factory, ZString fromTablePrefix, ZGuid fromBizoPK, ZString toTablePrefix, ZGuid toBizoPK)
		{
			if (factory == null || fromBizoPK.IsEmpty || toBizoPK.IsEmpty)
			{
				return false;
			}

			var succeeded = false;

			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentTableCode, fromTablePrefix);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentID, fromBizoPK);

			foreach (GenCustomAddOnValue customField in factory.Load<GenCustomAddOnValue>(query))
			{
				customField.XV_ParentID = toBizoPK;
				customField.XV_ParentTableCode = toTablePrefix;
				succeeded = true;
			}

			return succeeded;
		}

		public static void CopyCustomFields(BusinessObject fromBizObj, BusinessObject toBizObj)
		{
			if (fromBizObj != null && toBizObj != null)
			{
				CopyCustomFields(toBizObj.Factory, fromBizObj.TablePrefix, fromBizObj.PK, toBizObj.TablePrefix, toBizObj.PK);
			}
		}

		public static void CopyCustomFields(BusinessObjectFactory factory, ZString fromTablePrefix, ZGuid fromBizoPK, ZString toTablePrefix, ZGuid toBizoPK)
		{
			if (factory == null || fromBizoPK.IsEmpty || toBizoPK.IsEmpty)
			{
				return;
			}

			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentTableCode, new[] { fromTablePrefix, toTablePrefix });
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentID, new[] { toBizoPK, fromBizoPK });
			var customFields = factory.Load<GenCustomAddOnValue>(query);

			var fromBizoCustomFields = customFields.Where(t => t.XV_ParentID == fromBizoPK && t.XV_ParentTableCode == fromTablePrefix).ToList();
			var toBizoCustomFields = customFields.Where(t => t.XV_ParentID == toBizoPK && t.XV_ParentTableCode == toTablePrefix).ToList();

			var existingFields = toBizoCustomFields
				.Select(x => (x.XV_Name, x.XV_Type))
				.ToHashSet();

			foreach (var customField in fromBizoCustomFields)
			{
				if (existingFields.Contains((customField.XV_Name, customField.XV_Type)))
				{
					continue;
				}

				var customFieldClone = (GenCustomAddOnValue)customField.Clone();
				customFieldClone.XV_ParentID = toBizoPK;
				customFieldClone.XV_ParentTableCode = toTablePrefix;
			}
		}
	}
}
