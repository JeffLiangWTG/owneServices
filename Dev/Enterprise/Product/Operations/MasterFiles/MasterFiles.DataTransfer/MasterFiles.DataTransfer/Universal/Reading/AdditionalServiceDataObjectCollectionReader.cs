using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AdditionalServiceDataObjectCollectionReader : DataObjectCollectionReader<AdditionalService, JobService>
	{
		public AdditionalServiceDataObjectCollectionReader(DataObjectList<AdditionalService> services, IXmlImportLogger logger, UniversalObjectFactory factory, IHaveServices parent)
			: base(services)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.parent = Argument.NotNull(parent, "parent");
		}

		protected override CollectionContent DefaultCollectionContent => CollectionContent.Partial;

		#region Factory

		protected UniversalObjectFactory Factory
		{
			get { return factory; }
		}

		readonly UniversalObjectFactory factory;

		#endregion

		#region Logger

		protected IXmlImportLogger Logger
		{
			get { return logger; }
		}

		readonly IXmlImportLogger logger;

		#endregion

		#region Parent

		protected IHaveServices Parent
		{
			get { return parent; }
		}

		readonly IHaveServices parent;

		protected virtual JobServiceDependentCollection ParentServices
		{
			get { return Parent.Services; }
		}

		#endregion

		#region Implementation

		protected override JobService[] BusinessObjects => ParentServices.Cast<JobService>().ToArray();

		protected override JobService FindMatchingBusinessObject(AdditionalService dataObject)
		{
			return new AdditionalServiceBusinessObjectFinder<JobService>(dataObject).Find(ParentServices?.Cast<JobService>());
		}

		protected override void AddToCollection(JobService businessObject)
		{
			ParentServices.Add(businessObject);
		}

		protected override void RemoveFromCollection(JobService businessObject)
		{
			ParentServices.RemoveAndDelete(businessObject);
		}

		protected override JobService ReadIntoBusinessObject(AdditionalService dataObject, JobService businessObject)
		{
			return new AdditionalServiceDataObjectReader(dataObject, logger, factory, parent).ReadIntoBusinessObject();
		}

		#endregion
	}
}
