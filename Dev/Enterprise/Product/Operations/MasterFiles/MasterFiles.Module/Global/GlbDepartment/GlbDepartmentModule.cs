using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GlbDepartmentModule : ZFilterGridModule
	{
		public GlbDepartmentModule()
		{
		}

		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbDepartment; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbDepartment);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbDepartmentFilterControl(GridCollection, (GlbDepartmentFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			//TODO: Return a BusinessObjectCollection to be used for the MainForm grid.
			return new GlbDepartmentCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbDepartmentFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Departments; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override bool AllowDefaultActivateDeactivate
		{
			get
			{
				return true;
			}
		}

		protected override BusinessObjectActivator GetNewBusinessObjectActivator()
		{
			return new DepartmentActivator();
		}

		#endregion

		class DepartmentActivator : BusinessObjectActivator
		{
			protected override string ProvideFinalActivateDeactivateCheckBeforeSaving(ActivationResult activationProcessResult)
			{
				string result = base.ProvideFinalActivateDeactivateCheckBeforeSaving(activationProcessResult);
				if (!activationProcessResult.Activation)
				{
					ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
					query.AddToFilter(JoinCondition.And, GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, activationProcessResult.ProcessedObjects.Select(obj => obj.PK));
					if (!Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(GlbDepartment)), query))
					{
						result = Res.GetString("203768a0-00e9-4f5e-a1be-4b264c0d7d2f", "You cannot deactivate all Departments as no one will be able to login after that. Please leave at least one active Department.");
					}
				}
				return result;
			}
		}
	}
}
