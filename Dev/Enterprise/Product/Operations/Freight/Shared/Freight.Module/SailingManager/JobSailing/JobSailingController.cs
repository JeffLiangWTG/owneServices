using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public abstract class JobSailingController : ZController, IJobSailingSchedule
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public bool ScheduleCreateFromJob { get; set; }

		protected abstract ZString TransportMode { get; }

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			IBusiness result = Factory.Load(sourceEntity.GetType(), sourceEntity.Identifier);
			BaseJobSailing sailing = result as BaseJobSailing;
			if (sailing != null)
			{
				result = sailing.GetVoyageBusinessObject();
			}
			return result;
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobVoyage); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			JobVoyage voyage = (JobVoyage)businessEntity;
			if (!voyage.IsInDatabase)
			{
				voyage.JV_AirSeaRoad = TransportMode;
				voyage.HasChanges = false;
			}
			return new ZJobVoyageForm(voyage);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			var sailing = sourceEntity as BaseJobSailing;
			if (sailing != null)
			{
				var voyage = sailing.Voyage;
				if (voyage != null && !voyage.CanDelete)
				{
					string caption = Res.GetString("c9d63a5b-352c-4fd2-a353-ae0905976959", "Cannot Delete {0}", voyage.HumanReadableName);
					Globals.Message.ShowWarning(voyage.ReasonForNotAbleToDelete, caption);

					return null;
				}
			}

			return base.ShowDeleteForm(sourceEntity);
		}

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Length > 0)
			{
				List<JobVoyage> voyages = new List<JobVoyage>();

				foreach (BaseJobSailing sailing in selectedBusinessObjects)
				{
					if (sailing.Voyage != null && !voyages.Contains(sailing.Voyage))
					{
						voyages.Add(sailing.Voyage);
					}
				}

				if (voyages.Count == 1)
				{
					ShowDeleteForm(selectedBusinessObjects[0]);
				}
				else
				{
					base.DeleteMultipleCore(voyages.ToArray());
				}
			}
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			if (ScheduleCreateFromJob && !CheckPointForCreateFromJob.IsAllowed)
			{
				Globals.Message.ShowError(CheckPointForCreateFromJob.ErrorMessageForNotAllowed);
				return null;
			}

			return base.ShowFormForNewEntityCore(businessEntity);
		}

		#region Security

		protected virtual SecurityCheckpoint CheckPointForCreateFromJob
		{
			get { return Env.Security.SailingScheduleCreateFromJob; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SailingSchedule; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SailingScheduleNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SailingScheduleEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.SailingScheduleDelete; }
		}

		#endregion
	}
}
