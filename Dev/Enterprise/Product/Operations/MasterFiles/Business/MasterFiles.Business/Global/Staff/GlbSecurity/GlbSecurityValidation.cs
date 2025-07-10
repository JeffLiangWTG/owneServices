using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbSecurityValidation : AutoGlbSecurityValidation
	{
		internal static string CannotGrantOthersRightsYouDoNotHaveYourself(GlbSecurity parent)
		{
			ISecurityCheckpoint checkpoint = parent.Checkpoint;
			return Res.GetString("344afee2-2fd3-4b65-b99f-5d0235b34640", "You do not have security rights to this item yourself, so you cannot grant it to others. Security Checkpoint: {0}.", checkpoint != null ? checkpoint.DisplayTextPathToSecurityRight : (string)parent.GU_SecurityRight);
		}

		public GlbSecurityValidation(AutoGlbSecurity parent)
			: base(parent)
		{
		}

		public new GlbSecurity Parent
		{
			get { return (GlbSecurity)base.Parent; }
		}

		#region DepartmentCode

		public void ValidateDepartmentCode()
		{
			ValidateCalculatedProperty(Parent.DepartmentCodeInfo);
		}

		protected void CheckDepartmentCode()
		{
			ISecurityCheckpoint checkpoint = null;
			if ((!Parent.IsInDatabase || Parent.GU_GEInfo.HasChanges) && RowIsDuplicate)
			{
				checkpoint = Parent.Checkpoint;
				Parent.DepartmentCodeInfo.AddError(Res.GetString("97664f79-72b4-490e-82f7-4924ed0afc5c", "A Security Permissions row must be unique. Security Checkpoint: {0}.", checkpoint != null ? checkpoint.DisplayTextPathToSecurityRight : (string)Parent.GU_SecurityRight));
			}

			if (!Parent.GU_GE.IsEmpty && Parent.GU_GC.IsEmpty && Parent.GU_GB.IsEmpty)
			{
				checkpoint = Parent.Checkpoint;
				Parent.DepartmentCodeInfo.AddError(Res.GetString("56b86b9c-d6b6-405e-a9f9-179deee110b1", "When entering a Department, you must also enter a Company or Branch. Security Checkpoint: {0}.", checkpoint != null ? checkpoint.DisplayTextPathToSecurityRight : (string)Parent.GU_SecurityRight));
			}
			if (Parent.DepartmentCode == "*" || Parent.DepartmentCode.IsEmpty)
			{
				return;
			}

			if (checkpoint == null)
			{ checkpoint = Parent.Checkpoint; }
			ListValidation.ErrorIfInvalidCode(Parent.DepartmentCodeInfo, ResString.GetMultilingualString("3f7a44a4-c840-4acf-a21f-f7e0a52302fe", "Department. Security Checkpoint: {0}", checkpoint != null ? checkpoint.DisplayTextPathToSecurityRight : (string)Parent.GU_SecurityRight));

			ValidateGU_SecurityItemIsAllowed();
		}

		#endregion

		#region CompanyCode

		public void ValidateCompanyCode()
		{
			ValidateCalculatedProperty(Parent.CompanyCodeInfo);
			ValidateDepartmentCode();
		}

		protected void CheckCompanyCode()
		{
			ISecurityCheckpoint checkpoint = null;
			if ((!Parent.IsInDatabase || Parent.GU_GCInfo.HasChanges) && RowIsDuplicate)
			{
				checkpoint = Parent.Checkpoint;
				Parent.CompanyCodeInfo.AddError(Res.GetString("97664f79-72b4-490e-82f7-4924ed0afc5c", "A Security Permissions row must be unique. Security Checkpoint: {0}.", checkpoint != null ? checkpoint.DisplayTextPathToSecurityRight : (string)Parent.GU_SecurityRight));
			}

			if (Parent.CompanyCode != "*" && !Parent.CompanyCode.IsEmpty)
			{
				if (checkpoint == null)
				{ checkpoint = Parent.Checkpoint; }
				ListValidation.ErrorIfInvalidCode(Parent.CompanyCodeInfo, ResString.GetMultilingualString("cbfa6dd1-9000-4871-a910-409e4e31cde8", "Company. Security Checkpoint: {0}", checkpoint != null ? checkpoint.DisplayTextPathToSecurityRight : (string)Parent.GU_SecurityRight));
			}
			ValidateGU_SecurityItemIsAllowed();
		}

		#endregion

		#region BranchCode

		public void ValidateBranchCode()
		{
			ValidateCalculatedProperty(Parent.BranchCodeInfo);
			ValidateDepartmentCode();
		}

		protected void CheckBranchCode()
		{
			ISecurityCheckpoint checkpoint = null;
			if ((!Parent.IsInDatabase || Parent.GU_GBInfo.HasChanges) && RowIsDuplicate)
			{
				checkpoint = Parent.Checkpoint;
				Parent.BranchCodeInfo.AddError(Res.GetString("97664f79-72b4-490e-82f7-4924ed0afc5c", "A Security Permissions row must be unique. Security Checkpoint: {0}.", checkpoint != null ? checkpoint.DisplayTextPathToSecurityRight : (string)Parent.GU_SecurityRight));
			}

			if (Parent.BranchCode != "*" && !Parent.BranchCode.IsEmpty)
			{
				if (checkpoint == null)
				{ checkpoint = Parent.Checkpoint; }
				ListValidation.ErrorIfInvalidCode(Parent.BranchCodeInfo, ResString.GetMultilingualString("5f30d48e-cca7-4196-bb58-80f9e8aed676", "Branch. Security Checkpoint: {0}", checkpoint != null ? checkpoint.DisplayTextPathToSecurityRight : (string)Parent.GU_SecurityRight));
			}
			ValidateGU_SecurityItemIsAllowed();
		}

		#endregion

		#region Row Is Duplicate

		ZBool RowIsDuplicate
		{
			get
			{
				BusinessObjectCollection[] parentCollections = ((IBusinessObjectInternals)Parent).ParentCollections;
				if (parentCollections == null || parentCollections.Length == 0)
				{
					return false;
				}

				return parentCollections[0].Any(x => x is GlbSecurity security && DuplicateItemFilter(security));
			}
		}

		bool DuplicateItemFilter(GlbSecurity x)
		{
			return x != null
				&& x.PK != Parent.PK
				&& x.GU_SecurityRight == Parent.GU_SecurityRight
				&& x.GU_GG == Parent.GU_GG
				&& x.GU_GS == Parent.GU_GS
				&& x.GU_GC == Parent.GU_GC
				&& x.GU_GE == Parent.GU_GE
				&& x.GU_GB == Parent.GU_GB
				&& x.GU_GG == Parent.GU_GG
				&& x.GU_ItemGUID == Parent.GU_ItemGUID;
		}

		#endregion

		#region Has Rights to Change Others

		bool CalculateHasRightsToChangeOthers()
		{
			bool result = true;
			if (SecurityHasChanged)
			{
				if (Parent.GU_SecurityRight == GlbSecurity.GroupOwnerSecurityRightName)
				{
					return result;
				}

				// The GUIs are responsible for only letting people get to the form if they have some sort of rights. (That's how all security in Enterprise works.)
				// For validation, we only need to determine what sort of rights they have - unrestricted, or only able to grant rights they hold themselves.
				bool currentUserCanOnlyGrantRightsTheyHaveThemselves = Env.CurrentUser != null &&
																								(Parent.GU_GG.IsValid || Parent.GU_GS.IsValid) &&
																								!Env.CurrentUser.IsController &&
																								Env.CurrentUser.IsOperational;

				if (currentUserCanOnlyGrantRightsTheyHaveThemselves)
				{
					result = false;
					var lookupKey = Parent.GetCheckpointLookupKey_ForValidation();
					if (!lookupKey.IsEmpty)
					{
						var checkpoint = Env.Security.FindCheckPoint(lookupKey);
						if (checkpoint == null && lookupKey.Code.StartsWith("RegCat", StringComparison.Ordinal))
						{
							Env.Security.LoadRegistrySecurityCheckPoints();
							checkpoint = Env.Security.FindCheckPoint(lookupKey);
						}
						else if (checkpoint == null && lookupKey.Code.StartsWith("StmPrintQueue", StringComparison.Ordinal))
						{
							//Maybe new security checkpoint (e.g. new print queue) was added. Re-initialize and try a second time.
							Env.Security.ReloadPrintCheckpoints();
							checkpoint = Env.Security.FindCheckPoint(lookupKey);
						}
						else if (checkpoint == null && (lookupKey.Code.StartsWith("eDocsUploadDocumentType", StringComparison.Ordinal) || lookupKey.Code.StartsWith((NoResString)"Doc", StringComparison.Ordinal)))
						{
							//Maybe new security checkpoint (e.g. new document) was added. Re-initialize and try a second time.
							Env.Security.ReloadDocumentCheckpoints();
							checkpoint = Env.Security.FindCheckPoint(lookupKey);
						}
						if (checkpoint != null)
						{
							IGlbSecurityCollectionWithSecurity parentCollection = null;
							if (((IBusinessObjectInternals)Parent).ParentCollections.Length > 0 && (((IBusinessObjectInternals)Parent).ParentCollections.Length > 0))
							{
								parentCollection = ((IBusinessObjectInternals)Parent).ParentCollections[0] as IGlbSecurityCollectionWithSecurity;
								if (parentCollection != null)
								{
									var factory = parentCollection.ReadOnlyFactory;
									var securities = new GlbSecurityCollection(factory);
									securities.Load(GetFilterForCurrentUserSecurities());
									result = parentCollection.SecurityMap.HasAccessForAllBranchesAndDepartments(factory, securities, Env.Security, GlbStaff.CurrentUser, Parent, lookupKey);
								}
							}
						}
					}
				}
			}

			return result;
		}

		ZQuery GetFilterForCurrentUserSecurities()
		{
			ZQuery result = new ZQuery(GlbSecuritySchema.GU_GS, GlbStaff.CurrentUser.PK);
			result.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GG, GlbStaff.CurrentUser.Groups.GetPKs());
			return result;
		}

		bool SecurityHasChanged
		{
			get
			{
				return !Parent.ReadOnly &&
					(!Parent.IsInDatabase ||
					!Parent.GU_GC.Equals(Parent.GU_GCInfo.OriginalValue) ||
					!Parent.GU_GB.Equals(Parent.GU_GBInfo.OriginalValue) ||
					!Parent.GU_GE.Equals(Parent.GU_GEInfo.OriginalValue) ||
					!Parent.GU_SecurityItemIsAllowed.Equals(Parent.GU_SecurityItemIsAllowedInfo.OriginalValue));
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCompanyCode();
			ValidateBranchCode();
			ValidateDepartmentCode();
		}

		protected override void CheckGU_GB()
		{
			base.CheckGU_GB();
			ValidateGU_SecurityItemIsAllowed();
		}

		protected override void CheckGU_GC()
		{
			base.CheckGU_GC();
			ValidateGU_SecurityItemIsAllowed();
		}

		protected override void CheckGU_GG()
		{
			base.CheckGU_GG();
			ValidateGU_SecurityItemIsAllowed();
		}

		protected override void CheckGU_GE()
		{
			base.CheckGU_GE();
			ValidateGU_SecurityItemIsAllowed();
		}

		protected override void CheckGU_ItemGUID()
		{
			base.CheckGU_ItemGUID();
			ValidateGU_SecurityItemIsAllowed();
		}

		protected override void CheckGU_SecurityItemIsAllowed()
		{
			base.CheckGU_SecurityItemIsAllowed();

			if (Parent.ChangeOthersRightsNeedsValidation)
			{
				Parent.HasRightsToChangeOthers = CalculateHasRightsToChangeOthers();
				Parent.ChangeOthersRightsNeedsValidation = false;
			}
			if (!Parent.HasRightsToChangeOthers)
			{
				Parent.GU_SecurityItemIsAllowedInfo.AddError(CannotGrantOthersRightsYouDoNotHaveYourself(Parent));
			}
		}

		protected override void CheckGU_SecurityRight()
		{
			base.CheckGU_SecurityRight();
			ValidateGU_SecurityItemIsAllowed();
		}
	}
}
