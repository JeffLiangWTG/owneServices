using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[CodeProperty(JobVoyAccountSchema.Constants.NA_JobNumber)]
	[System.Diagnostics.DebuggerDisplay("Voyage Account = {NA_JobNumber}")]
	public partial class VoyageAccount : AutoJobVoyAccount, Integration.Agency.IVoyageAccount
	{
		#region Schema

		public new class Schema : AutoJobVoyAccount.Schema
		{
			public const string NA_Calc_Vessel = "NA_Calc_Vessel";
			public const string NA_Calc_Voyage = "NA_Calc_Voyage";
			public const string NA_Calc_Description = "NA_Calc_Description";
		}

		#endregion

		public VoyageAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Related BusinessObjects

		public JobVoyage Voyage
		{
			get { return Factory.Load<JobVoyage>(NA_JV); }
		}

		public OrgHeader Principal
		{
			get { return Factory.Load<OrgHeader>(NA_OH); }
		}

		public JobHeader Job
		{
			get { return job ?? (job = new JobHeader.Loader(this).Load()); }
		}
		JobHeader job;

		#endregion

		#region Bound Properties

		[ReadOnly(true)]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString NA_JobNumber
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.NA_JobNumber; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.NA_JobNumber = value; }
		}

		[ReadOnlyMember(nameof(HasPostedCharges))]
		[RelatedBusinessObject("Voyage")]
		public override ZGuid NA_JV
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.NA_JV; }
			set
			{
				base.NA_JV = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateNA_Calc_Vessel();
					Validation.ValidateNA_Calc_Voyage();
					Validation.ValidateNA_OH();
				}
			}
		}

		[ReadOnlyMember(nameof(HasPostedCharges))]
		[RelatedBusinessObject("Principal")]
		public override ZGuid NA_OH
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.NA_OH; }
			set
			{
				base.NA_OH = value;
				if (Job != null && Job.JH_OA_LocalChargesAddr.IsEmpty)
				{
					OrgAddress address = Header.GetAddressWithFallback(AddressType.ARM);
					if (address != null)
					{
						Job.JH_OA_LocalChargesAddr = address.PK;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateNA_Calc_Vessel();
					Validation.ValidateNA_Calc_Voyage();
					Validation.ValidateNA_OH();
				}
			}
		}

		[ResourceStringData("JobVoyAccount|NA_Calc_Vessel", Caption = "Vessel", FullDescription = "The vessel this job relates to.")]
		public ZString NA_Calc_Vessel
		{
			get
			{
				JobVoyage voyage = Voyage;
				return voyage == null ? ZString.Empty : voyage.JV_RV_NKVessel;
			}
		}
		public ZPropertyInfo NA_Calc_VesselInfo
		{
			get { return GetZPropertyInfo(Schema.NA_Calc_Vessel); }
		}

		[ResourceStringData("JobVoyAccount|NA_Calc_Voyage", Caption = "Voyage", FullDescription = "The voyage this job relates to.")]
		public ZString NA_Calc_Voyage
		{
			get
			{
				JobVoyage voyage = Voyage;
				return voyage == null ? ZString.Empty : voyage.JV_VoyageFlight;
			}
		}
		public ZPropertyInfo NA_Calc_VoyageInfo
		{
			get { return GetZPropertyInfo(Schema.NA_Calc_Voyage); }
		}

		public ZString NA_Calc_Description
		{
			get
			{
				JobVoyage voyage = Voyage;
				OrgHeader header = Header;

				return string.Format(CultureInfo.InvariantCulture, "{0}/{1} ({2})",
					voyage == null ? null : voyage.JV_RV_NKVessel,
					voyage == null ? null : voyage.JV_VoyageFlight,
					header == null ? null : header.OH_Code);
			}
		}
		public ZPropertyInfo NA_Calc_DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.NA_Calc_Description); }
		}

		#endregion

		#region Non Bound Properties

		public bool IsDistinct
		{
			get
			{
				if (isDistinct == null)
				{
					isDistinct = new CachedProperty<bool>(Factory, () => Factory.LoadTop1<VoyageAccount>(IsDistinctQuery) == null);
				}

				return isDistinct.Value;
			}
		}
		CachedProperty<bool> isDistinct;

		internal ZQuery IsDistinctQuery
		{
			get
			{
				var filter = new ZQuery(JobVoyAccountSchema.PK, SQLComparisonOperator.NotEqual, PK);
				filter.AddToFilter(JobVoyAccountSchema.NA_GC, NA_GC);
				filter.AddToFilter(JobVoyAccountSchema.NA_JV, NA_JV);
				filter.AddToFilter(JobVoyAccountSchema.NA_OH, NA_OH);

				return filter;
			}
		}

		internal string IsNotDistinctErrorMessage
		{
			get { return Res.GetString("09baa139-7145-47a8-aa84-8ab4f107735b", "This combination of schedule + principal already exists in the database."); }
		}

		#endregion

		#region BusinessObject Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			SetJobNumberIfNotSet();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				NA_JobNumber = "";
			}
			base.OnSaved(saveSucceeded);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			NA_GC = GlbCompany.CurrentCompany.PK;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("fa6755ca-8fc5-41d9-9a8d-f2f97e35eee8", "Voyage Accounting Job {0}", NA_JobNumber); }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void RunPreSaveValidationCore()
		{
			isDistinct = null;
			base.RunPreSaveValidationCore();
		}

		public override bool CanDelete
		{
			get { return Job == null || Job.CanDelete; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;
				if (!CanDelete)
				{
					result = ResString.GetMultilingualString("56758537-0859-43f2-b682-117513cf8b5f", "{0}: {1}", HumanReadableName, Job.ReasonForNotAbleToDelete);
				}

				return result;
			}
		}

		public override void Delete()
		{
			if (Job != null && Job.CanDelete)
			{
				Job.Delete();
			}

			base.Delete();
		}

		#endregion

		#region Implementation

		void SetJobNumberIfNotSet()
		{
			if (NA_JobNumber.IsEmpty)
			{
				NA_JobNumber = Env.NumberFountains.VoyageAccountingNumber(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(Factory);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for tests VoyagePrincipalReadOnly (VoyageAccountTest) and VoyageButton_Click (VoyageAccountingFormTest)")]
		bool HasPostedCharges
		{
			get
			{
				if (Job != null)
				{
					JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, Job.PK));
					foreach (JobCharge charge in charges)
					{
						if (charge.IsCostPosted || charge.IsRevenuePosted)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new ScheduleAndPrincipalUniqueIndexFailureHandler(this)); }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new VoyageAccountFetchStrategy(this);
		}

		#endregion
	}
}


