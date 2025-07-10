using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public class ProjectDataContextManager : ActivityDataContextManager<Project>
	{
		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(WorkProjectSchema.WKP_ProjectNumber, matchingValues.Key);
		}

		public override DataContextType DataContextType => DataContextType.Project;
		public override string DefaultOutputDirectory => string.Empty;

		public override ITopLevelDataObjectWriter GetActivityDataObjectWriter(IDataWritingManager writeManager, bool shouldOnlyIncludeBasicDetails)
		{
			return new ProjectDataObjectWriter(writeManager, shouldOnlyIncludeBasicDetails);
		}

		public override ITopLevelDataObjectReader GetActivityDataObjectReader(ITopLevelDataObject activity, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			return new ProjectDataObjectReader((Activity)activity, logger, (UniversalObjectFactory)factory);
		}
	}
}
