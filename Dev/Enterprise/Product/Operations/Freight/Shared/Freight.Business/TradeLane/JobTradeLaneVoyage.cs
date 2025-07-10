using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	[DependentBusinessObject(typeof(JobVoyage), "TradeLanes")]
	[System.Diagnostics.DebuggerDisplay("Principal = {Header.OH_Code}, TradeLane = {TradeLane.EJ_Code}")]
	public class JobTradeLaneVoyage : AutoJobTradeLaneVoyage
	{
		public JobTradeLaneVoyage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region NB_EJ

		[RelatedBusinessObject("TradeLane")]
		[List("Lookups.JobTradeLaneList")]
		public override ZGuid NB_EJ
		{
			get { return base.NB_EJ; }
			set
			{
				base.NB_EJ = value;
				if (TradeLane != null && NB_OH.IsEmpty)
				{
					NB_OH = TradeLane.EJ_OH_RelatedOrg;
				}

				if (Voyage != null)
				{
					Voyage.Origins.MarkAsNeedingValidation();
					Voyage.Destinations.MarkAsNeedingValidation();
				}
				Validation.ValidateNB_OH();
			}
		}

		#endregion

		#endregion

		#region Business Object Overrides

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new JobTradeLaneVoyageUniqueIndexFailureHandler(this); }
		}

		#endregion

		#region Implementation

		public JobVoyage Voyage
		{
			get { return Factory.Load<JobVoyage>(NB_JV); }
		}

		public JobTradeLane TradeLane
		{
			get { return Factory.Load<JobTradeLane>(NB_EJ); }
		}

		public OrgHeader Principal
		{
			get { return Factory.Load<OrgHeader>(NB_OH); }
		}

		#endregion

	}
}
