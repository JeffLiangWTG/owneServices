using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class StmTemplateRecordBizoStrategy : IBusinessObjectStrategy
	{
		public void BeforeSuccessfulDelete(BusinessObject businessObject) { }

		public DeleteDetails DeleteDetails(BusinessObject businessObject)
		{
			var templateRecord = businessObject as StmTemplateRecord;
			if (templateRecord == null)
			{
				return null;
			}

			if (templateRecord.STR_ModuleID.IsEmpty)
			{
				return new DeleteDetails.Allow();
			}

			var contextKey = templateRecord.STR_ModuleID + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			var copyTemplates = businessObject.Factory.Load<IUniversalCopyTemplate>(new ZQuery(StmModuleFilterSchema.S9_ModuleID, contextKey));
			var linkedCopyTemplates = copyTemplates.Where(copyTemplate => copyTemplate.ConfigurationSource == "NOR" && copyTemplate.NominatedRecordPk == businessObject.PK).ToList();
			if (linkedCopyTemplates.Count > 0)
			{
				return new DeleteDetails.Disallow(ResString.GetMultilingualString("c2089e24-3754-43a4-aa88-48d67fafd7cc", "This record is selected as a Nominated Record in following Copy Template(s): {0}",
					System.Environment.NewLine + string.Join(System.Environment.NewLine, linkedCopyTemplates.Select(template => template.ConfigurationName))));
			}

			return new DeleteDetails.Allow();
		}

		public void OnDelete(BusinessObject businessObject) { }

		public void OnSaving(BusinessObject businessObject) { }

		public void OnSaved(BusinessObject businessObject, bool saveSucceeded) { }

		public void OnFactorySaving(BusinessObject businessObject) { }

		public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded) { }

		public void OnSaveRollback(BusinessObject businessObject) { }

		public void FetchForLoad(BusinessObject businessObject) { }

		public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject) { }
	}
}
