using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ICustomLabelsConfigOrgProvider
	{
		OrgHeader ConfigOrg { get; }
		event EventHandler ConfigOrgChanged;
		BusinessObjectFactory Factory { get; }
	}
}
