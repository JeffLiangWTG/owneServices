using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbBranch)]
	public class GlbBranchDependentCollection : ActiveBusinessObjectCollection<GlbBranch>
	{
		public GlbBranchDependentCollection(GlbCompany parent, BusinessObjectFactory factory) : base(factory, parent)
		{
			this.Parent = parent;
		}

		/// <summary>
		/// This is collection of branches that defaults to the current company. Pass another Company if branches for other companies are required
		/// </summary>
		public GlbBranchDependentCollection(BusinessObjectFactory factory) : this(GlbCompany.CurrentCompany, factory)
		{
		}

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		#region Overrides

		protected override void SetDefaultsForNewElementCore(GlbBranch branch)
		{
			base.SetDefaultsForNewElementCore(branch);
			if (branch != null && !branch.GB_OH_OrgProxy.IsValid && Parent != null)
			{
				branch.GB_OH_OrgProxy = Parent.GC_OH_OrgProxy;
			}
		}

		public override void Delete(GlbBranch businessObject)
		{
			GlbBranch branch = businessObject;

			if (branch.PK == GlbBranch.CurrentBranch.PK && OnAttemptedToDeleteCurrentBranch != null)
			{
				OnAttemptedToDeleteCurrentBranch(this, EventArgs.Empty);
			}
			else
			{
				base.Delete(businessObject);
			}
		}

		#endregion

		#region Implementation

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest] // Is used in base collection state (master in relationship)
#endif
		protected GlbCompany Parent;

		public event EventHandler OnAttemptedToDeleteCurrentBranch;

		#endregion
	}
}
