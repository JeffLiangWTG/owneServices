using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[CodeProperty(JobContainerDetentionSchema.Constants.NC_JobNumber)]
	[DescriptionProperty(JobContainerDetentionSchema.Constants.NC_DetentionType)]
	public partial class ContainerDetention : AutoJobContainerDetention,
		Integration.Agency.IContainerDetention,
		IRatingSupporter
	{
		public new class Schema : AutoJobContainerDetention.Schema
		{
			public const string NC_Calc_Status = "NC_Calc_Status";
		}

		public ContainerDetention(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		[List("Lookups.DetentionTypes")]
		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZString NC_DetentionType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.NC_DetentionType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.NC_DetentionType = value; }
		}

		[ReadOnly(true)]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString NC_JobNumber
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.NC_JobNumber; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.NC_JobNumber = value; }
		}

		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZGuid NC_OH_Client
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.NC_OH_Client; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.NC_OH_Client = value; }
		}

		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZGuid NC_OH_Principal
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.NC_OH_Principal; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.NC_OH_Principal = value; }
		}

		[List("Lookups.DetentionStatus")]
		[MaxLength(3)]
		public ZString NC_Calc_Status
		{
			get
			{
				if (nc_Calc_Status == null)
				{
					nc_Calc_Status = new CachedProperty<ZString>(Factory, delegate
					{
						bool hasPostedCharge = false;
						bool hasNonPostedCharge = false;

						if (Job != null)
						{
							ZQuery filter = new ZQuery(JobChargeSchema.JR_JH, Job.PK);
							filter.FetchOnlyFromLocalCache = !Job.IsInDatabase;

							foreach (JobCharge charge in Factory.Load<JobCharge>(filter))
							{
								if (charge.IsRevenuePosted)
								{
									hasPostedCharge = true;
								}
								else
								{
									hasNonPostedCharge = true;
								}

								if (hasPostedCharge && hasNonPostedCharge)
								{
									break;
								}
							}
						}

						if (hasPostedCharge)
						{
							if (hasNonPostedCharge)
							{
								return DetentionInvoiceStatus.Codes.PartiallyPosted;
							}
							else
							{
								return DetentionInvoiceStatus.Codes.Posted;
							}
						}
						else
						{
							return DetentionInvoiceStatus.Codes.NotPosted;
						}
					});
				}
				return nc_Calc_Status.Value;
			}
		}
		public ZPropertyInfo NC_Calc_StatusInfo
		{
			get { return GetZPropertyInfo(Schema.NC_Calc_Status); }
		}
		CachedProperty<ZString> nc_Calc_Status;

		[ChildEditable(true)]
		public ContainerMovementCollection Movements
		{
			get
			{
				if (movements == null)
				{
					ICollectionRelationship relationship = new DependentRelationship(this, typeof(ContainerMovement));
					movements = new ContainerMovementCollection(Factory, false, relationship);
					RegisterEditableChildObject(movements);
					((IBindingList)movements).ListChanged += new ListChangedEventHandler(Movements_ListChanged);
				}
				return movements;
			}
		}
		ContainerMovementCollection movements;

		public JobHeader Job
		{
			get { return job ?? (job = new JobHeader.Loader(this).Load()); }
		}
		JobHeader job;

		public bool HasPostedCharges
		{
			get
			{
				if (hasPostedCharges == null)
				{
					hasPostedCharges = new CachedProperty<bool>(Factory, delegate
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
					});
				}

				return hasPostedCharges.Value;
			}
		}
		CachedProperty<bool> hasPostedCharges;

		public ContainerMovement[] FindRelatedMovements()
		{
			if (Client != null && Company != null)
			{
				switch (NC_DetentionType)
				{
					case DetentionInvoiceType.Codes.Export:
					case DetentionInvoiceType.Codes.Import:
						return DetentionableLoader.LoadDetentionableContainers(
							Factory,
							NC_GC,
							Client.PK,
							Principal.PK,
							NC_DetentionType,
							Company.GC_RN_NKCountryCode);

					default:
						return System.Array.Empty<ContainerMovement>();
				}
			}
			else
			{
				return System.Array.Empty<ContainerMovement>();
			}
		}

		#region Implementation

		public override void OnSaving()
		{
			base.OnSaving();
			SetJobNumberIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				NC_DetentionTypeInfo.RefreshBinding();
				NC_JobNumberInfo.RefreshBinding();
				NC_OH_ClientInfo.RefreshBinding();
				NC_OH_PrincipalInfo.RefreshBinding();
			}
			else if (!IsInDatabase)
			{
				NC_JobNumber = "";
			}
		}

		JobHeader jobForDeleteCheck;
		public override bool CanDelete
		{
			get
			{
				if (Job != null)
				{
					jobForDeleteCheck = Job;
					return Job.CanDelete;
				}
				else
				{
					jobForDeleteCheck = jobForDeleteCheck ?? (jobForDeleteCheck = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, PK)));
					return jobForDeleteCheck == null;
				}
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;
				if (!CanDelete)
				{
					result = MultilingualString.Join(System.Environment.NewLine, ResString.GetMultilingualString("07cf68b4-1cdf-4ae2-b2d8-5c34868137c1", "{0} cannot be deleted.", HumanReadableName), jobForDeleteCheck?.ReasonForNotAbleToDelete);
				}

				return result;
			}
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				// if we don't detach the movements then they will be cascade deleted.
				foreach (ContainerMovement movement in Movements.ToArray())
				{
					Movements.RemoveFromRelationship(movement);
				}
			}

			if (Job != null && !Job.IsDeleted)
			{
				Job.Delete();
			}

			base.Delete();
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection result = base.NoteTypesCore;
				result.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
				return result;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (IsInDatabase)
				{
					return Res.GetString("fd498b4a-427d-4540-a5e5-14b4373b711f", "Container Detention ({0})", NC_JobNumber);
				}
				else
				{
					return Res.GetString("36060e10-ae8e-4fe1-b210-d6828a0861b8", "Container Detention");
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			NC_GC = GlbCompany.CurrentCompany.PK;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		void SetJobNumberIfNeeded()
		{
			if (NC_JobNumber.IsEmpty)
			{
				NC_JobNumber = Env.NumberFountains.DetentionInvoiceNumbers(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(Factory);
			}
		}

		void Movements_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e != null && e.ListChangedType == ListChangedType.ItemDeleted && Movements.Count == 0 && this.Job != null && !this.Job.IsDeleted)
			{
				this.Job.JH_Status = JobHeaderStatus.Closed.Code;
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ContainerDetentionFetchStrategy(this);
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new ContainerDetentionRatingAdaptersProvider(this); }
		}

		#endregion
	}
}


