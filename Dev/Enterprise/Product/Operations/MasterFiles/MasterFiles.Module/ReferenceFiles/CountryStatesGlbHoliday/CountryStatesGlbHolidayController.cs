using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CountryStatesGlbHolidayController : ZController
	{
		public override ControllerID ID => ControllerIDs.CountryStatesGlbHoliday;

		public override ModuleIdentifier ModuleID => ModuleIDs.CountryStatesGlbHoliday;

		public override Type TypeOfTopLevelBusinessObject => typeof(CountryStatesGlbHolidayBizo);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CountryStatesGlbHolidayView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CountryStatesGlbHolidayNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CountryStatesGlbHolidayModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CountryStatesGlbHolidayDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CountryStatesGlbHolidayForm((CountryStatesGlbHolidayBizo)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInFactory(BusinessObjectFactory factory)
		{
			return new CountryStatesGlbHolidayBizo(factory);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new CountryStatesGlbHolidayBizo(Factory);
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var glbHoliday = factory.Load<GlbHoliday>(sourceEntityPK);
			return glbHoliday != null ? new CountryStatesGlbHolidayBizo(factory, glbHoliday) : null;
		}

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			var glbHolidays = new List<GlbHoliday>();
			foreach (CountryStatesGlbHolidayBizo holidayBizo in selectedBusinessObjects)
			{
				if (holidayBizo.GHC_IsStateSpecific)
				{
					var applicableStates = holidayBizo.GHC_CountryStates.Where(x => x.IsChecked && x.HolidayPK.HasValue);
					var glbHolidayOnStates = Factory.Load<GlbHoliday>(new ZQuery(GlbHolidaySchema.PK, applicableStates.Select(x => x.HolidayPK.Value)));
					glbHolidays.AddRange(glbHolidayOnStates);
				}
				else
				{
					var glbHoliday = Factory.Load<GlbHoliday>(holidayBizo.GHC_PK);
					glbHolidays.Add(glbHoliday);
				}
			}
			base.DeleteMultipleCore(glbHolidays.ToArray());
		}
	}
}
