using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Rating;

public class JobRatingPreference(BusinessObjectFactory factory, DataRow row) : AutoJobRatingPreference(factory, row)
{
	public static JobRatingPreference Create(BusinessObject parent, ZGuid? companyPk = null, ZDateTime? autoratingDate = null, ZBool? isDateOverridden = null)
	{
		var preference = parent.Factory.New<JobRatingPreference>();
		using var suspendSettingHasChanges = preference.SuspendSettingHasChanges();

		preference.JRP_ParentTableCode = parent.TablePrefix;
		preference.JRP_ParentID = parent.PK;

		if (companyPk.HasValue)
		{
			preference.JRP_GC_Company = companyPk.Value;
		}

		if (autoratingDate.HasValue)
		{
			preference.JRP_AutoratingDate = autoratingDate.Value;
		}

		if (isDateOverridden.HasValue)
		{
			preference.JRP_IsDateOverridden = isDateOverridden.Value;
		}

		return preference;
	}

	public static JobRatingPreference Load(BusinessObject parent, ZGuid? companyPk = null)
	{
		var query = new ZQuery();
		query.AddToFilter(JobRatingPreferenceSchema.JRP_ParentTableCode, parent.TablePrefix);
		query.AddToFilter(JobRatingPreferenceSchema.JRP_ParentID, parent.PK);

		if (companyPk.HasValue)
		{
			query.AddToFilter(JobRatingPreferenceSchema.JRP_GC_Company, companyPk);
		}
		else
		{
			query.AddToFilter(JobRatingPreferenceSchema.JRP_GC_Company, DBNull.Value);
		}

		return parent.Factory.LoadTop1<JobRatingPreference>(query);
	}
}
