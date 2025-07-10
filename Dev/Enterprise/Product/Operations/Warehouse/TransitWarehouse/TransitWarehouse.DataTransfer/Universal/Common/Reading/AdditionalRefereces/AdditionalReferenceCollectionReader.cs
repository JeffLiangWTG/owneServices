using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class AdditionalReferenceCollectionReader : DataObjectCollectionReader<AdditionalReference, CusEntryNumber>, IAdditionalReferenceCollectionReader
	{
		public AdditionalReferenceCollectionReader(AdditionalReference[] references, IHaveCusEntryNumReferences cusEntryNumReferenceParent, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(references)
		{
			CusEntryNumReferenceParent = cusEntryNumReferenceParent;
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
		}
		readonly IHaveCusEntryNumReferences CusEntryNumReferenceParent;
		protected readonly IXmlImportLogger logger;
		protected readonly UniversalObjectFactory factory;

		protected override CusEntryNumber[] BusinessObjects
		{
			get
			{
				if (bizOs == null)
				{
					bizOs = CusEntryNumReferenceParent.CusEntryNumReferences.Cast<CusEntryNumber>().ToArray();
				}
				return bizOs;
			}
		}
		CusEntryNumber[] bizOs;

		protected override void AddToCollection(CusEntryNumber businessObject)
		{
			CusEntryNumReferenceParent.CusEntryNumReferences.Add(businessObject);
		}

		protected override void RemoveFromCollection(CusEntryNumber businessObject)
		{
			businessObject.Delete();
		}

		protected override CusEntryNumber FindMatchingBusinessObject(AdditionalReference dataObject)
		{
			var finder = new AdditionalReferenceBusinessObjectFinder<CusEntryNumber>(dataObject);
			return finder.Find(CusEntryNumReferenceParent);
		}

		protected override CusEntryNumber ReadIntoBusinessObject(AdditionalReference dataObject, CusEntryNumber businessObject)
		{
			var reader = new AdditionalReferenceReader<CusEntryNumber>(dataObject, CusEntryNumReferenceParent, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
