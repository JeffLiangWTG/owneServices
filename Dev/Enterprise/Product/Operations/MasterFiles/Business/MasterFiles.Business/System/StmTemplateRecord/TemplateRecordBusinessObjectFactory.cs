using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateRecordBusinessObjectFactory : BusinessObjectFactory
	{
		public TemplateRecordBusinessObjectFactory()
		{
			if (DataRegistry.Instance.TemplateRecordValidation == RawDataRegistry.TemplateRecordValidationCodes.NoValidation)
			{
				SuspendValidation();
			}

			RefreshEnabled = false;
			TemplateRecordFactory = new BusinessObjectFactory();
			ChildFactories.Add(TemplateRecordFactory);
		}

		public BusinessObjectFactory TemplateRecordFactory { get; }

		public ITemplateRecordProvider TemplateRecordProvider { get; set; }

		protected override IChangedTableNames SaveInTransactionCore()
		{
			var bizo = TemplateRecordProvider as IBusiness;
			if (bizo == null || bizo.HasChanges)
			{
				TemplateRecordProvider?.SaveToTemplateRecord();
			}

			BusinessObjectsInLastSaveOrder = null; // Initialize list of saved bizos
			return new ChangedTableNames(new List<string>());
		}

		public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			return new BusinessObjectFactory();
		}
	}
}
