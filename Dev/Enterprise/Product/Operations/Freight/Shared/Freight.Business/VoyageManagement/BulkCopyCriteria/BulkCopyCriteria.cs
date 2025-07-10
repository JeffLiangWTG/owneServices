using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public abstract class BulkCopyCriteria : NonPersistentBusinessObject, IObsoleteValidation, IDisposable
	{
		protected BulkCopyCriteria(ZGuid sailingPK)
			: this(sailingPK, false)
		{
		}

		protected BulkCopyCriteria(ZGuid sailingPK, bool onlyThisSailing)
			: base(new BusinessObjectFactory())
		{
			sailing = Factory.Load<BaseJobSailing>(sailingPK);
			schedules = new BaseJobSailingCollection(Factory);

			if (onlyThisSailing && sailing != null)
			{
				FilteredOrigin = sailing.JX_JA_RL_NKPortOfLoading;
				FilteredDestination = sailing.JX_JB_RL_NKPortOfDischarge;
			}
		}

		#region Related Business Objects

		public BaseJobSailing Sailing
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return sailing; }
		}

		public BaseJobSailingCollection Schedules
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return schedules; }
		}

		public BulkSailingConsolGenerator ConsolDetails
		{
			get
			{
				if (consolDetails == null)
				{
					consolDetails = GetNewSailingConsolGenerator();
					RegisterEditableChildObject(consolDetails);
				}

				return consolDetails;
			}
		}
		protected BulkSailingConsolGenerator consolDetails;
		protected abstract BulkSailingConsolGenerator GetNewSailingConsolGenerator();

		public RepititionSelection RepititionSelection
		{
			get
			{
				if (repititionSelection == null)
				{
					repititionSelection = new RepititionSelection();
					RegisterEditableChildObject(repititionSelection);
				}

				return repititionSelection;
			}
		}
		RepititionSelection repititionSelection;

		#endregion

		public void Generate()
		{
			BuildBulkJobSailingHelper helper = new BuildBulkJobSailingHelper(this);
			helper.FilteredOrigin = FilteredOrigin;
			helper.FilteredDestination = FilteredDestination;
			helper.BulkCopySchedule();

			if (ConsolDetails.CreateConsol)
			{
				ConsolDetails.GenerateConsols(Schedules);
			}
		}

		internal ZString FilteredOrigin { get; private set; }
		internal ZString FilteredDestination { get; private set; }

		#region Implementation

		readonly BaseJobSailing sailing;
		readonly BaseJobSailingCollection schedules;

		#endregion

		void IDisposable.Dispose()
		{
			Dispose();
		}

		protected virtual void Dispose()
		{
		}
	}
}
