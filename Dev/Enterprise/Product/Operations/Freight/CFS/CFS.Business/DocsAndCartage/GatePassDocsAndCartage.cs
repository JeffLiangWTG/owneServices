using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassDocsAndCartage : CFSDocsAndCartage
	{
		public GatePassDocsAndCartage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ParentType = typeof(GatePassShipment);
		}

		public event EventHandler IsContingencyReleaseChecked;

		#region Related Business Objects

		public new GatePassServiceDependentCollection Services
		{
			get { return (GatePassServiceDependentCollection)base.Services; }
		}

		protected override JobServiceDependentCollection GetNewServiceCollection()
		{
			return new GatePassServiceDependentCollection(this, Factory);
		}

		public new GatePassRequiredDocumentDependentCollection RequiredDocuments
		{
			get { return (GatePassRequiredDocumentDependentCollection)base.RequiredDocuments; }
		}

		protected override JobRequiredDocumentDependentCollection GetNewRequiredDocumentCollection()
		{
			return new GatePassRequiredDocumentDependentCollection(this, Factory);
		}

		#endregion

		#region Business Object Overrides

		public override ZBool JP_LCLDatesOverrideConsol
		{
			get { return base.JP_LCLDatesOverrideConsol; }
			set
			{
				bool isDiff = base.JP_LCLDatesOverrideConsol != value;
				base.JP_LCLDatesOverrideConsol = value;
				if (isDiff)
				{
					BusinessObject parent = Parent as BusinessObject;
					if (parent != null)
					{
						parent.MarkAsNeedingValidation();
					}
				}
			}
		}
		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			CreateContingencyReleaseEvent();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (JP_IsContingencyRelease)
			{
				StmALog log = ((IStmALogParent)Parent).Logs.MostRecentLogByEventTime(Events.CustomsContingencyRelease);
				ReasonForContingencyRelease = log != null ? log.SL_Reference : ZString.Empty;
			}

			HasChanges = false;
		}

		#endregion

		#region Property Overrides

		public override ZBool JP_IsContingencyRelease
		{
			get { return base.JP_IsContingencyRelease; }
			set
			{
				base.JP_IsContingencyRelease = value;
				((BusinessObject)Parent).RefreshBinding();

				if (value)
				{
					OnIsContingencyReleaseChecked(EventArgs.Empty);
				}
				else
				{
					ReasonForContingencyRelease = ZString.Empty;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateReasonForContingencyRelease();
				}

				NeedToAddContingencyReleaseLog = value;
			}
		}

		#endregion

		#region Non-Persistent Properties

		protected ZString fReasonForContingencyRelease;
		[MaxLength(255)]
		public ZString ReasonForContingencyRelease
		{
			get
			{
				return fReasonForContingencyRelease;
			}
			set
			{
				if (fReasonForContingencyRelease != value)
				{
					CheckMaximumLength(ReasonForContingencyReleaseInfo, value);
					fReasonForContingencyRelease = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateReasonForContingencyRelease();
					}
				}
				ReasonForContingencyReleaseInfo.RefreshBinding();
				((BusinessObject)Parent).RefreshBinding();
			}
		}

		public ZPropertyInfo ReasonForContingencyReleaseInfo
		{
			get { return GetZPropertyInfo(nameof(ReasonForContingencyRelease)); }
		}

		protected bool ReasonForContingencyRelease_ReadOnly
		{
			get { return !JP_IsContingencyRelease; }
		}

		#endregion

		#region Validation

		protected override JobDocsAndCartageValidation GetNewValidation()
		{
			return new GatePassDocsAndCartageValidation(this);
		}

		public new GatePassDocsAndCartageValidation Validation
		{
			get { return (GatePassDocsAndCartageValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		bool NeedToAddContingencyReleaseLog;

		protected virtual void OnIsContingencyReleaseChecked(EventArgs e)
		{
			if (IsContingencyReleaseChecked != null)
			{
				IsContingencyReleaseChecked(this, e);
			}
		}

		protected void CreateContingencyReleaseEvent()
		{
			if (NeedToAddContingencyReleaseLog)
			{
				((IStmALogParent)Parent).Logs.AddNew(Events.CustomsContingencyRelease, ReasonForContingencyRelease);
				NeedToAddContingencyReleaseLog = false;
			}
		}

		#endregion
	}
}
