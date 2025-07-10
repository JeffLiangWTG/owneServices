using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Module
{
	public class PortFilterSecurityValidator
	{
		public PortFilterSecurityValidator(BusinessObjectFactory factory, SecurityCheckpoint securityCheckpoint)
		{
			this.Factory = factory;
			this.SecurityCheckpoint = securityCheckpoint;
		}

		readonly BusinessObjectFactory Factory;
		readonly SecurityCheckpoint SecurityCheckpoint;

		public void ValidatePort(ZPropertyInfo currentPortInfo, ZPropertyInfo otherPortInfo, ModuleFilterCollection allFilters, params ZString[] relatedFilterDescriptions)
		{
			if (!SecurityCheckpoint.IsAllowed && !Globals.IsWeb)
			{
				FilterOrCategory currentFilterOrCaterogy = ((ModuleFilter)currentPortInfo.BizObj).OrCategory;
				var allLocationFiltersInCurrentOrCategory = allFilters
					.Where(filter => filter is ModuleLocationFilter
						&& filter.IsActive
						&& filter.OrCategory == currentFilterOrCaterogy);

				bool allowedPortFoundInCategory = false;
				foreach (ModuleLocationFilter locationFilter in allLocationFiltersInCurrentOrCategory)
				{
					foreach (ZString description in relatedFilterDescriptions)
					{
						if (locationFilter.Description.StartsWith(description, StringComparison.Ordinal))   // To handle duplicated filters, that would have desription like "Origin (1)"
						{
							if (AllowablePorts.Contains(locationFilter.Property1) || AllowablePorts.Contains(locationFilter.Property2))
							{
								allowedPortFoundInCategory = true;
								break;
							}
						}
					}
				}

				ZString currentPort = (ZString)currentPortInfo.Value;
				ZString otherPort = (ZString)otherPortInfo.Value;
				if (currentFilterOrCaterogy != FilterOrCategory.None && !currentPort.IsEmpty && !AllowablePorts.Contains(currentPort) && !AllowablePorts.Contains(otherPort))
				{
					allowedPortFoundInCategory = false;
				}

				if (!allowedPortFoundInCategory)
				{
					currentPortInfo.AddError(ErrorMessage);
				}
			}
		}

		public void ValidatePort(DateLocationFilter filter)
		{
			if (!SecurityCheckpoint.IsAllowed && !Globals.IsWeb && !AllowablePorts.Contains(filter.Property3))
			{
				filter.Property3Info.AddError(ErrorMessage);
			}
		}

		ZString[] AllowablePorts
		{
			get
			{
				if (allowablePorts == null)
				{
					SecurityLocator locator = new SecurityLocator(GlbStaff.CurrentUser, Factory);
					GlbBranch[] branches = locator.GetAllBranchesThatStaffHasPermissionsFor(Env.Security.Login);

					List<ZString> portCodesAllBranches = new List<ZString>();
					foreach (GlbBranch branch in branches)
					{
						if (!portCodesAllBranches.Contains(branch.GB_RL_NKHomePort))
						{
							portCodesAllBranches.Add(branch.GB_RL_NKHomePort);
						}

						foreach (GlbBranchExtraPorts extraPort in branch.ExtraPorts)
						{
							if (!portCodesAllBranches.Contains(extraPort.GY_RL_NKAdditionalBranchRelatedPort))
							{
								portCodesAllBranches.Add(extraPort.GY_RL_NKAdditionalBranchRelatedPort);
							}
						}
					}

					portCodesAllBranches.Sort();
					allowablePorts = portCodesAllBranches.ToArray();
				}

				return allowablePorts;
			}
		}
		ZString[] allowablePorts;

		string ErrorMessage
		{
			get
			{
				if (errorMessage == null)
				{
					string commaDelimitedPorts = string.Join(", ", AllowablePorts);
					errorMessage = Res.GetString("cd7fd4f4-6768-4679-a54d-aba51afd3812", "Please set at least one location filter to one of the following ports:") + " " + commaDelimitedPorts.TrimEnd(',', ' ') +
						"\n" + Res.GetString("746ff4e1-a241-4197-aedf-1612d57450eb", "If you require access to results for all locations, please ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n{0}", SecurityCheckpoint.DisplayTextPathToSecurityRight);
				}
				return errorMessage;
			}
		}

		string errorMessage;
	}
}
