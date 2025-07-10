using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgMatchApprovalModule : ZFilterGridModule, IOrgMatchApprovalModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OrgMatchApproval; }
		}
		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (EditMenuItem != null)
			{
				EditMenuItem.Text = Res.GetString("11827e78-11c6-4faa-b04f-4bef0eaef47b", "R&eview");
			}
			return result.ToArray();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override SortInfo DefaultSortOrder
		{
			get { return null; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.OrgMatchApproval);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgMatchApprovalFilterControl(GridCollection, FilterBusinessObject);
		}

		public new OrgMatchApprovalCollection GridCollection
		{
			get { return (OrgMatchApprovalCollection)base.GridCollection; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ModuleOrgMatchApprovalCollection(Factory);
		}

		public class ModuleOrgMatchApprovalCollection : OrgMatchApprovalCollection
		{
			public ModuleOrgMatchApprovalCollection(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override bool AllowSort
			{
				get { return false; }
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgMatchApprovalFilterBusinessObject();
		}

		protected new OrgMatchApprovalFilterBusinessObject FilterBusinessObject
		{
			get { return (OrgMatchApprovalFilterBusinessObject)base.FilterBusinessObject; }
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.OrgMatchApproval; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#endregion

		#region IOrgMatchApprovalModule

		public OrgMatchApprovalForm ShowEditForm(OrgMatchApproval selectedBusinessObject)
		{
			return (OrgMatchApprovalForm)ShowEditForm((BusinessObject)selectedBusinessObject);
		}

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			OrgMatchApprovalForm result = null;

			OrgMatchApproval selectedMatchApproval = (OrgMatchApproval)selectedBusinessObject;
			if (selectedMatchApproval.Parent == null)
			{
				Globals.Message.ShowError(Res.GetString("7e002ca4-c321-4abe-8a69-06541f3d418f", "Deleted"), Res.GetString("3ceb85d6-e0a6-4eb4-b722-f5eac355c794", "The record associated with this match approval has been deleted by an administrator. This match approval will now be deleted."));
				DeleteMatchApprovalInOtherFactory(selectedMatchApproval);
			}
			else
			{
				result = (OrgMatchApprovalForm)base.ShowEditForm(selectedBusinessObject);
			}
			return result;
		}

		void DeleteMatchApprovalInOtherFactory(OrgMatchApproval matchApproval)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgMatchApproval matchApprovalInOtherFactory = newFactory.Load<OrgMatchApproval>(matchApproval.PK);
			matchApprovalInOtherFactory.Delete();
			newFactory.Save();
		}

		public bool IsUnmatchForCurrentUserOnly
		{
			get { return FilterBusinessObject.IsUnmatchForCurrentUserOnly; }
		}

		public void RevertFilterToUnmatchedByCurrentUser()
		{
			FilterBusinessObject.RevertFilterToUnmatchedByCurrentUser();
			PerformSearch();
		}

		public bool RevertFilterToUnmatchedByCurrentUserAfterWarningUser()
		{
			bool result = true;
			if (!FilterBusinessObject.IsUnmatchForCurrentUserOnly)
			{
				DialogResult dialogResult = Globals.Message.Show(Res.GetString("b4176086-1b72-4e04-a1fb-b9f79eae1139", "This will reset your current module filter to 'Items unmatched by current user'. Proceed?"), Res.GetString("de3dbcd2-aacd-461a-ac47-2b4b014cb77e", "Reset Filter"), MessageBoxButtons.YesNo, DialogResult.Yes);
				result = (dialogResult == DialogResult.Yes);
				if (result)
				{
					RevertFilterToUnmatchedByCurrentUser();
				}
			}
			return result;
		}

		public void PerformSearch()
		{
			base.PerformSearch();
		}

		#endregion
	}
}
