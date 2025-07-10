using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class ModuleToDeclarationModuleSender : IDeclarationModuleToModuleSender
	{
		public PublishToUniversalResult CreateJob<T>(T businessEntity)
			where T : BusinessObject, IWorkflowProvider, IModuleToModule
		{
			return ModuleToDeclarationModuleSender<T>.CreateJob(businessEntity);
		}
	}

	public class ModuleToDeclarationModuleSender<T> : ModuleToModuleSender<T>
		where T : BusinessObject, IWorkflowProvider, IModuleToModule
	{
		ModuleToDeclarationModuleSender()
		{
		}

		internal static PublishToUniversalResult CreateJob(BusinessObject businessEntity)
		{
			return new ModuleToDeclarationModuleSender<T>().CreateNewEntityFromParent(businessEntity as T);
		}

		protected override DataContextType EntityTypeToLoad
		{
			get { return DataContextType.CustomsDeclaration; }
		}

		protected override ZString ErrorPrefix
		{
			get { return Res.GetString("866E0C72-1607-4532-8ABC-312FF0D4A1AF", "Failed to create a Job:"); }
		}

		protected override UniversalDataBuss.DataObjects.Universal.Event[] GetUniversalEvents(T parentEntity)
		{
			var factory = new BusinessObjectFactory();
			UniversalDataBuss.DataObjects.Universal.Event[] events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, parentEntity.RecipientOrganisation as OrgHeader, new RecipientRoleType[1] { RecipientRoleType.BRO }, parentEntity).ToArray();
				factory.Save();
			}
			return events;
		}
	}
}
