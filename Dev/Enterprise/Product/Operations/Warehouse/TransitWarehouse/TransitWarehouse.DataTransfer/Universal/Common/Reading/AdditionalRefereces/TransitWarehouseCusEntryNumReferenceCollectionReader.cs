using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business.Common;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitWarehouseCusEntryNumReferenceCollectionReader : DataObjectCollectionReader<TransitAdditionalReferenceInfo, CusEntryNumber>, ICusEntryNumReferenceCollectionReader
	{
		public TransitWarehouseCusEntryNumReferenceCollectionReader(TransitAdditionalReferenceInfo[] additionalReferencesToReadIn, IHaveCusEntryNumReferences cusEntryNumReferenceParent, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(additionalReferencesToReadIn)
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

		protected override CusEntryNumber FindMatchingBusinessObject(TransitAdditionalReferenceInfo dataObject)
		{
			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(dataObject);
			return finder.Find(CusEntryNumReferenceParent);
		}

		protected override CusEntryNumber ReadIntoBusinessObject(TransitAdditionalReferenceInfo dataObject, CusEntryNumber businessObject)
		{
			var reader = new TransitWarehouseAdditionalReferenceReader<CusEntryNumber>(dataObject, CusEntryNumReferenceParent, logger, factory);
			var entryNumber = reader.ReadIntoBusinessObject();
			if (!string.IsNullOrEmpty(dataObject.SourceType))
			{
				var addOnValue = entryNumber.GetAddOnValues(a => a.XV_Name == "SourceType").FirstOrDefault();
				if (addOnValue != null)
				{
					addOnValue.XV_Data = dataObject.SourceType.Value;
				}
				else
				{
					entryNumber.PopulateAddOnValue("SourceType", "STR", dataObject.SourceType.Value);
				}
			}
			return entryNumber;
		}
	}
}
