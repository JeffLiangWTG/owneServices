using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public abstract class RatingHeaderProcessTask<T> : ProcessTask
		where T : RatingHeader
	{
		protected RatingHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		protected override Type ParentType
		{
			get { return typeof(T); }
		}

		public new T Parent
		{
			get { return (T)base.Parent; }
		}

		#endregion

		#region ProcessTask Overrides

		protected override ProcessTasksValidation GetNewValidation()
		{
			return IsTask ? new RatingHeaderProcessTaskValidation(this) : base.GetNewValidation();
		}

		public override ZGuid P9_ParentID
		{
			get { return base.P9_ParentID; }
			set
			{
				base.P9_ParentID = value;

				if (Parent != null && Parent.Header != null)
				{
					OrganisationPK = Parent.Header.PK;
				}
			}
		}

		#endregion

		#region Loggin

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
