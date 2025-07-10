using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.ProcessManagement.Business
{
	public abstract class WorkItemCommon : AutoWorkItem, IWorkItem, IJobHeaderParent, IJobInvoicingPlugIn, IHaveServices, IWorkItemLookupsParent, ILocation
	{
		protected WorkItemCommon(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region TypeDecider

		public static readonly WorkItemTypeDecider TypeDecider = new WorkItemTypeDecider();

		#endregion

		#region Schema

		public new abstract class Schema : AutoWorkItem.Schema
		{
			public const string WorkItemTypeDescription = "WorkItemTypeDescription";
			public const string AreaDescription = "AreaDescription";
			public const string ActivityTypeDescription = "ActivityTypeDescription";
			public const string ActivitySubtypeDescription = "ActivitySubtypeDescription";
			public const string PriorityDescription = "PriorityDescription";
			public const string CreatedTimeAsText = "CreatedTimeAsText";
			public const string ReleaseSequenceName = "ReleaseSequenceName";
			public const string ReleaseSequencePosition = "ReleaseSequencePosition";
			public const string ReleaseSequenceInvestment = "ReleaseSequenceInvestment";
			public const string ReleaseSequenceValue = "ReleaseSequenceValue";
			public const string ReleaseSequenceDateAsText = "ReleaseSequenceDateAsText";
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			SetJobNumberIfRequired();
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					WKI_WorkItemNumber = "";
				}
			}

			base.OnSaved(saveSucceeded);
		}

		public void SetJobNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(WKI_WorkItemNumberInfo, JobNumberFountain);
			}
		}

		#endregion

		#region Related BO

		[ActionFieldFollow(false)]
		public JobHeader Job
		{
			get { return new JobHeader.Loader(this).Load(); }
		}

		#endregion

		#region Properties

		#region WKI_Details_HTML

		public ZBlob WKI_Details_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(WKI_Details);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.WKI_Details = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		#endregion

		#region WKI_WorkItemNumber

		[ActionField(ReadOnly = true)]
		public override ZString WKI_WorkItemNumber
		{
			get { return base.WKI_WorkItemNumber; }
			set { base.WKI_WorkItemNumber = value; }
		}

		#endregion

		#region PortOrCountry

		[List("Lookups.Locations")]
		public override ZString WKI_PortOrCountry
		{
			get { return base.WKI_PortOrCountry; }
			set
			{
				base.WKI_PortOrCountry = value;
				portOrCountry = null;
			}
		}

		#endregion

		#region Created By / Time

		[List("Lookups.Staff")]
		public override ZString WKI_SystemCreateUser
		{
			get { return base.WKI_SystemCreateUser; }
			set
			{
				base.WKI_SystemCreateUser = value;
				if (!IsValidationSuspended)
				{
					((WorkItemCommonValidation)Validation).ValidateWKI_SystemCreateUser();
				}
			}
		}

		public bool WKI_SystemCreateUser_ReadOnly
		{
			get { return IsInDatabase && !Env.Security.WorkItemEditModifyStaffAssignment.IsAllowed; }
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public ZString CreatedTimeAsText
		{
			get { return WKI_SystemCreateTimeUtc.IsValid ? Env.Time.GetLocalTimeFromUtc(WKI_SystemCreateTimeUtc.ToDateTime()).ToString(DateTimeFormatStrings.LongTimeFormat, CultureInfo.CurrentCulture) : string.Empty; }
		}

		public ZPropertyInfo CreatedTimeAsTextInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CreatedTimeAsText, x => WKI_SystemCreateTimeUtcInfo); }
		}

		#endregion

		#endregion

		#region Validation

		protected override sealed WorkItemValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		protected abstract WorkItemCommonValidation GetNewValidationCore();

		#endregion

		#region Lookups

		protected override sealed WorkItemLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		protected abstract WorkItemLookups GetNewLookupsCore();

		#endregion

		#region JobNumberFountain

		protected virtual INumberFountainProxy JobNumberFountain
		{
			get { return Env.NumberFountains.WorkItemNo; }
		}

		#endregion

		#region IJobHeaderParent

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return AllowInvoiceDeletionCore; }
		}

		protected virtual bool AllowInvoiceDeletionCore { get { return true; } }

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			SetJobNumberIfRequired();
		}

		public string JobNumber
		{
			get { return WKI_WorkItemNumber; }
		}

		#endregion

		#region IJobInvoicingPlugIn

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get
			{
				return invoicingSupporter ?? (invoicingSupporter = GetInvoicingSupporterCore());
			}
		}
		IJobInvoicingSupporter invoicingSupporter;

		protected virtual IJobInvoicingSupporter GetInvoicingSupporterCore()
		{
			return new WorkItemInvoicingSupporter(this);
		}

		#endregion

		#region IHaveServices Member

		[ChildEditable]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = GetJobServicesDependentCollection(this, Factory);
					services.Load();
					RegisterEditableChildObject(services);
					AfterServiceLoaded();
				}
				return services;
			}
		}
		JobServiceDependentCollection services;

		protected abstract JobServiceDependentCollection GetJobServicesDependentCollection(WorkItemCommon workItemCommon, BusinessObjectFactory factory);

		protected virtual void AfterServiceLoaded()
		{
		}

		public ZString ContainerMode
		{
			get { return ""; }
		}

		public IHaveServices[] DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		public BusinessObject ServiceParent
		{
			get { return this; }
		}

		public ZString TableCode
		{
			get { return WorkItemSchema.Constants.Prefix; }
		}

		public ZString TransportMode
		{
			get { return ""; }
		}

		public bool NeedsServiceEvents
		{
			get { return false; }
		}

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Factory.Load<GlbBranch>(WorkItemServiceBranchPK);

		protected abstract ZGuid WorkItemServiceBranchPK { get; }

		#endregion

		#region ILocation

		ZString ILocation.Code => PortOrCountry?.Code ?? ZString.Empty;

		ZString ILocation.Description => PortOrCountry?.Description ?? ZString.Empty;

		ZBool ILocation.IsActive => PortOrCountry?.IsActive ?? false;

		RefCityTown ILocation.CityTown => PortOrCountry?.CityTown;

		RefCountry ILocation.Country => PortOrCountry?.Country;

		RefCountryStates ILocation.State => PortOrCountry?.State;

		RefUNLOCO ILocation.UNLOCO => PortOrCountry?.UNLOCO;

		IATACityCode ILocation.IATACityCode => PortOrCountry?.IATACityCode;

		RefZoneHeader[] ILocation.Zones => PortOrCountry?.Zones ?? Array.Empty<RefZoneHeader>();

		bool ILocationReference.IsLocalInRelationTo(ZString code) => PortOrCountry?.IsLocalInRelationTo(code) ?? false;

		#endregion

		ILocation PortOrCountry => portOrCountry ?? LocationHelper.GetLocationFromString(WKI_PortOrCountry, Factory);

		ILocation portOrCountry;
	}
}

