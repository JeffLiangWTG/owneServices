using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class PortReferenceCollectionReader : DataObjectCollectionReader<PortReference, CusEntryNumber>, IPortReferenceCollectionReader
	{
		public PortReferenceCollectionReader(PortReference[] references, IHavePortReferences portReferenceParent, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(references)
		{
			PortReferenceParent = portReferenceParent;
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
		}
		readonly IHavePortReferences PortReferenceParent;
		protected readonly IXmlImportLogger logger;
		protected readonly UniversalObjectFactory factory;

		protected override CusEntryNumber[] BusinessObjects
		{
			get
			{
				if (bizOs == null)
				{
					bizOs = PortReferenceParent.PortReferences.Cast<CusEntryNumber>().ToArray();
				}
				return bizOs;
			}
		}
		CusEntryNumber[] bizOs;

		protected override void AddToCollection(CusEntryNumber businessObject)
		{
			PortReferenceParent.PortReferences.Add(businessObject);
		}

		protected override void RemoveFromCollection(CusEntryNumber businessObject)
		{
			businessObject.Delete();
		}

		protected override CusEntryNumber FindMatchingBusinessObject(PortReference dataObject)
		{
			var finder = new PortReferenceBusinessObjectFinder<CusEntryNumber>(dataObject);
			return finder.Find(PortReferenceParent);
		}

		protected override CusEntryNumber ReadIntoBusinessObject(PortReference dataObject, CusEntryNumber businessObject)
		{
			var reader = new PortReferenceReader<CusEntryNumber>(dataObject, PortReferenceParent, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
