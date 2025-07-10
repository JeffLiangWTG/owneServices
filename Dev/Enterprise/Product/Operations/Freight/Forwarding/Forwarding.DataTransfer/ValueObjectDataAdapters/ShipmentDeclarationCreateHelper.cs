using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentDeclarationCreateHelper
	{
		public ShipmentDeclarationCreateHelper(bool isManualImport, BusinessObjectFactory factory)
		{
			IsManualImport = isManualImport;
			Factory = factory;
		}

		readonly bool IsManualImport;
		readonly BusinessObjectFactory Factory;

		public bool ShouldImportJobDeclaration(GlbBranch orgProxybranch)
		{
			return AutomaticCreateJobDecRegistry(orgProxybranch.Company.PK.ToGuid()) &&
				(!IsManualImport || Env.CurrentBranch.Code == orgProxybranch.GB_Code);
		}

		public bool ShouldCreateJobDecIrrespectiveOfCompanies
		{
			get { return !IsManualImport && ShouldLookForBranchesOnSystemLevel; }
		}

		public UserContext GetUserContextForBranch(GlbBranch branch)
		{
			UserContext result = null;

			GlbDepartment department = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, DefaultDepartmentCode);

			if (branch != null && department != null)
			{
				result = new UserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
			}
			return result;
		}

		public GlbBranch GetBranchWithSamePortCode(RefUNLOCO unloco, OrgHeader broker)
		{
			GlbBranch targetBranch = null;

			ZQuery query = BranchQuery(true, broker.PK, unloco.RL_Code);
			GlbBranch[] candidatebranches = Factory.Load<GlbBranch>(query);

			if (candidatebranches.Length > 0)
			{
				targetBranch = candidatebranches[0];
			}
			else
			{
				query = BranchQuery(false, broker.PK, unloco.RL_Code);
				candidatebranches = Factory.Load<GlbBranch>(query);

				foreach (GlbBranch current in candidatebranches)
				{
					if (FoundMatchingBranch(unloco.RL_Code, current))
					{
						targetBranch = current;
						break;
					}
				}
			}

			return targetBranch;
		}

		bool ShouldLookForBranchesOnSystemLevel
		{
			get
			{
				bool result = true;

				ZString systemDefinedRegValue = GetConsolImportDirRegistryValueFallBackToAllLevels(Guid.Empty);

				foreach (GlbCompany current in ActiveCompanies)
				{
					if (GetConsolImportDirRegistryValueFallBackToAllLevels(current.PK.ToGuid()) != systemDefinedRegValue)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		ZString GetConsolImportDirRegistryValueFallBackToAllLevels(Guid companyPK)
		{
			return SystemDataRegistry.Instance.ConsolsDataImportDirectory.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
		}

		bool AutomaticCreateJobDecRegistry(Guid companyPK)
		{
			return SystemDataRegistry.Instance.AutoCreateDeclarationWithinShipment.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
		}

		bool FoundMatchingBranch(ZString portCode, GlbBranch branch)
		{
			bool result = false;

			foreach (GlbBranchExtraPorts currentPort in branch.ExtraPorts)
			{
				if (portCode == currentPort.GY_RL_NKAdditionalBranchRelatedPort)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		ZQuery BranchQuery(bool filterByBrachHomePort, ZGuid brokerPK, ZString portCode)
		{
			ZQuery query = ActiveBranchQuery;
			query.AddToFilter(GlbBranchSchema.GB_OH_OrgProxy, brokerPK);

			if (filterByBrachHomePort)
			{
				query.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, portCode);
			}

			if (IsManualImport)
			{
				query.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			}

			return query;
		}

		ZQuery ActiveBranchQuery
		{
			get { return new ZQuery(GlbBranchSchema.GB_IsActive, true); }
		}

		GlbCompanyCollection ActiveCompanies
		{
			get
			{
				if (activeCompanies == null)
				{
					ZQuery filter = new ZQuery(GlbCompanySchema.GC_IsActive, true);
					activeCompanies = new GlbCompanyCollection(Factory, filter);
				}
				return activeCompanies;
			}
		}
		GlbCompanyCollection activeCompanies;

		readonly ZString DefaultDepartmentCode = "BRN";
	}
}
