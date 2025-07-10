using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobScheduleChange : AutoJobScheduleChange
	{
		public JobScheduleChange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public JobScheduleChange LoadOrCreate(BusinessObject parent, ZString dateType)
			{
				return LoadOrCreate(parent, dateType, GlbStaff.CurrentUser.GS_Code);
			}

			public JobScheduleChange LoadOrCreate(BusinessObject parent, ZString dateType, ZString userInitials)
			{
				JobScheduleChange result = Load(parent.PK, dateType, userInitials);
				if (result == null)
				{
					result = Factory.New<JobScheduleChange>();
					result.E7_ParentID = parent.PK;
					result.E7_ParentTableCode = parent.TablePrefix;
					result.E7_DateType = dateType;
					result.E7_GS_NKChangedBy = userInitials;
				}
				return result;
			}

			public JobScheduleChange Load(ZGuid parentID, ZString dateType)
			{
				return Load(parentID, dateType, GlbStaff.CurrentUser.GS_Code);
			}

			public JobScheduleChange Load(ZGuid parentID, ZString dateType, ZString userInitials)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(JobScheduleChangeSchema.E7_ParentID, parentID);
				query.AddToFilter(JobScheduleChangeSchema.E7_DateType, dateType);
				query.AddToFilter(JobScheduleChangeSchema.E7_GS_NKChangedBy, GlbStaff.CurrentUser.GS_Code);
				return Factory.LoadTop1<JobScheduleChange>(query);
			}

			#region Implementation

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(JobScheduleChange);
			}

			#endregion
		}

		#endregion

		#region Saving

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				Delete();
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (E7_PreviousValue == E7_UpdatedValue)
			{
				Delete();
			}
		}

		#endregion

		#region Related Business Objects

		public IScheduleChangeParent Parent
		{
			get
			{
				switch (E7_ParentTableCode)
				{
					case JobVoyOriginSchema.Constants.Prefix:
						return Factory.Load<VoyageOrigin>(E7_ParentID);
					case JobVoyDestinationSchema.Constants.Prefix:
						return Factory.Load<VoyageDestination>(E7_ParentID);
					case JobSailingSchema.Constants.Prefix:
						return Factory.Load<JobSailing>(E7_ParentID);
					default:
						return null;
				}
			}
		}

		public JobVoyage Voyage
		{
			get { return Parent == null ? null : Parent.Voyage; }
		}

		#endregion

		#region Properties

		public ZString DateTypeDescription
		{
			get { return new ScheduleDateTypes().GetDescriptionFromCode(E7_DateType); }
		}

		#endregion
	}
}
