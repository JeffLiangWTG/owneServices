using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public eManifestModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.US.eManifest; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.US.eManifest);
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.eManifestWorkflowDescriptorCode; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new[] { BusinessContext.eManifest }; }
		}

		#region Filter

		protected override IFilterControl GetNewFilterControl()
		{
			return new eManifestFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return GetNewGridCollection(Factory);
		}

		internal static IBusinessObjectCollection GetNewGridCollection(BusinessObjectFactory factory)
		{
			return new ActiveBusinessObjectCollection<Trip>(factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new eManifestFilterStrip();
		}

		#endregion

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.USeManifest; }
		}

		#endregion

		#region IOperationalActionSupportable

		public OperationalActionSupporter OperationalActionSupporter => new eManifestOperationalActionSupporter();

		#endregion

		#region Test Helpers

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (Env.CurrentUser.IsDeveloper && Env.Registry.EnableCustomsDiagnostics)
			{
				result.Add(new ZMenuItem("-"));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Interchange Composer Service Task", (sender, e) => RunOneServiceTaskCycle("InterchangeComposerServiceTask")));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Interchange Processor Service Task", (sender, e) => RunOneServiceTaskCycle("InterchangeProcessorServiceTask")));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Message Processor Service Task", (sender, e) => RunOneServiceTaskCycle("MessageProcessorServiceTask")));
			}

			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				result.Add(new ZMenuItem("-"));
				result.Add(new ZMenuItem(CreateNewDeclarationMenuName, OnCreateNewDeclaration_Click));
			}

			return result.ToArray();
		}
		internal const string CreateNewDeclarationMenuName = "Create A New Declaration";

		void OnCreateNewDeclaration_Click(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(SelectAtLeastOneeManifest);
			}
			else
			{
				foreach (Trip selectedTrip in Grid.SelectedElements)
				{
					ManifestFormhelper.CreateANewDeclaration(selectedTrip);
				}
			}
		}
		internal const string SelectAtLeastOneeManifest = "Please select at least one eManifest";

		static void RunOneServiceTaskCycle(string serviceTaskName)
		{
			var type = Type.GetType(string.Format("Enterprise.Customs.US.eManifest.ServiceTasks.{0}, Enterprise.Customs.US.eManifest.ServiceTasks", serviceTaskName));
			if (type != null)
			{
				var logger = new Enterprise.Messaging.Business.Logger();
				var obj = Activator.CreateInstance(type, Array.Empty<object>());
				var property = type.GetProperty("ServiceLogger", BindingFlags.Public | BindingFlags.Instance);
				property.SetValue(obj, logger, null);
				var methodInfo = type.GetMethod("RunTask", BindingFlags.Public | BindingFlags.Instance);
				methodInfo.Invoke(obj, Array.Empty<object>());
				Globals.Message.Show(logger.ToString());
			}
		}
		#endregion
	}
}
