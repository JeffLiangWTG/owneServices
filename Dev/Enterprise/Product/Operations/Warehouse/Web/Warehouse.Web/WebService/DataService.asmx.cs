using System;
using System.ComponentModel;
using System.Web.Services;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	/// <summary>
	/// Summary description for DataService
	/// </summary>
	[WebService(Namespace = "http://cargowise.com/WarehouseRF/")]
	[ToolboxItem(false)]
	public class DataService : BaseService
	{
		[WebMethod(Description = "System Version")]
		public String SystemVersion()
		{
			return WinCEWebServiceVersion;
		}

		[WebMethod(Description = "CW1 Version")]
		public String CW1Version()
		{
			EnterpriseInformationRetriever retriever = new EnterpriseInformationRetriever();
			return retriever.VersionNumber;
		}

		[WebMethod(Description = "Retrieve available departments")]
		public DepartmentInfoCollection GetAvailableDepartments()
		{
			var result = new DepartmentInfoCollection();
			try
			{
				result = LoadAvailableDepartments();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);
			}
			return result;
		}

		[WebMethod(Description = "Retrieve available branches")]
		public BranchInfoCollection GetAvailableBranches()
		{
			var result = new BranchInfoCollection();
			try
			{
				result = LoadAvailableBranches();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);
			}
			return result;
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DepartmentInfoCollection LoadAvailableDepartments()
		{
			var result = new DepartmentInfoCollection();
			using (Db.DisposableActionForDbConnection())
			using (var reader = Db.Connection.Command("select GE_PK, GE_Code, GE_Desc from dbo.GlbDepartment where GE_IsActive = 1 order by GE_Desc").ExecuteReader())      // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				while (reader.Read())
				{
					var department = new DepartmentInfo();
					department.Code = (string)reader[GlbDepartmentSchema.GE_Code.Name];
					department.Description = (string)reader[GlbDepartmentSchema.GE_Desc.Name];
					department.PK = (Guid)reader[GlbDepartmentSchema.PK.Name];
					result.Add(department);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		BranchInfoCollection LoadAvailableBranches()
		{
			var result = new BranchInfoCollection();
			using (Db.DisposableActionForDbConnection())
			using (var reader = Db.Connection.Command("select GB_PK, GB_Code, GB_BranchName, GC_Name from dbo.GlbBranch join dbo.GlbCompany on GB_GC = GC_PK where GB_IsActive = 1 order by GB_BranchName").ExecuteReader())        // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				while (reader.Read())
				{
					var branch = new BranchInfo();
					branch.Code = (string)reader[GlbBranchSchema.GB_Code.Name];
					branch.Name = (string)reader[GlbBranchSchema.GB_BranchName.Name];
					branch.PK = (Guid)reader[GlbBranchSchema.PK.Name];
					result.Add(branch);
				}
			}
			return result;
		}

		#endregion
	}
}
