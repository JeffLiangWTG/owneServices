using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[CodeProperty(JobSundryChargesSchema.Constants.D4_JobNumber)]
	[DescriptionProperty(JobSundryChargesSchema.Constants.D4_SundriesDescription)]
	public partial class SundryCharges : AutoJobSundryCharges
	{
		public SundryCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Bound Properties

		[ReadOnly(true)]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString D4_JobNumber
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.D4_JobNumber; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.D4_JobNumber = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member HasPostedCharges")]
		[ReadOnlyMember("HasPostedCharges")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZDateTime D4_FromDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.D4_FromDate; }
			set
			{
				base.D4_FromDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateD4_ToDate();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member HasPostedCharges")]
		[ReadOnlyMember("HasPostedCharges")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZDateTime D4_ToDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.D4_ToDate; }
			set
			{
				base.D4_ToDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateD4_FromDate();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member HasPostedCharges")]
		[ReadOnlyMember("HasPostedCharges")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid D4_OH_BillToParty
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.D4_OH_BillToParty; }
			set
			{
				base.D4_OH_BillToParty = value;

				if (Job != null && Job.JH_OA_LocalChargesAddr.IsEmpty && BillToParty != null)
				{
					OrgAddress address = BillToParty.GetAddressWithFallback(AddressType.ARM);

					if (address != null)
					{
						Job.JH_OA_LocalChargesAddr = address.PK;
					}
				}
			}
		}

		[List("Lookups.Types")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString D4_SundriesJobType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.D4_SundriesJobType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.D4_SundriesJobType = value; }
		}

		[List("Lookups.Modes")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString D4_SundryJobMode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.D4_SundryJobMode; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.D4_SundryJobMode = value; }
		}

		[List("Lookups.Activities")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString D4_SundryJobActivity
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.D4_SundryJobActivity; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.D4_SundryJobActivity = value; }
		}

		#endregion

		#region Related BusinessObjects

		public JobHeader Job
		{
			get { return job ?? (job = new JobHeader.Loader(this).Load()); }
		}
		JobHeader job;

		#endregion

		#region Overlapping Jobs

		public ReadOnlyCollection<OverlappingJob> OverlappingJobs
		{
			get
			{
				if (overlappingJobs == null)
				{
					overlappingJobs = new CachedProperty<ReadOnlyCollection<OverlappingJob>>(Factory, delegate
					{
						OverlappingJob[] list = OverlappingJob.Load(this);
						return Array.AsReadOnly(list ?? Array.Empty<OverlappingJob>());
					});
				}
				return overlappingJobs.Value;
			}
		}
		public void ForceResetOverlappingJobs()
		{
			overlappingJobs = null;
		}
		CachedProperty<ReadOnlyCollection<OverlappingJob>> overlappingJobs;

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			D4_SundriesJobType = Lookups.Types.DefaultCode;
			D4_SundryJobMode = Lookups.Modes.DefaultCode;
			D4_SundryJobActivity = Lookups.Activities.DefaultCode;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			SetJobNumberIfNeeded();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("11e522b2-849f-4b25-976d-00effa235685", "Sundry Charges {0}", D4_JobNumber); }
		}

		#endregion
		#region Implementation

		void SetJobNumberIfNeeded()
		{
			if (D4_JobNumber.IsEmpty)
			{
				SundryChargesJobNumberTarget target = new SundryChargesJobNumberTarget();

				NumberGenerator generator = new NumberGenerator();
				generator.Factory = Factory;
				generator.Context = new NumberGeneratorContext();
				generator.BaseFountain = Env.NumberFountains.SundryChargesNumber;
				generator.CompanyFountainGetter = Env.NumberFountains.GetSundryChargesGeneratorFountain;
				generator.PrimaryTarget = target;
				generator.ValueProviders.AddRange(new StandardValueSource());
				generator.ValueProviders.AddRange(new SundryChargesValueSource(this));

				generator.Generate();
				generator.EnforceMaxLengths();

				D4_JobNumber = target.Value;
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new SundryChargesFetchStrategy(this);
		}

		#endregion
	}
}


