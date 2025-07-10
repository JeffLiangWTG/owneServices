using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class EntryNumberCollectionReader<T> : DataObjectCollectionReader<EntryNumber, T> where T : CusEntryNumber
	{
		public EntryNumberCollectionReader(EntryNumber[] entryNumbers, IXmlImportLogger logger, UniversalObjectFactory factory, BusinessObject numbersParent, IBusinessObjectCollection numbersCollection)
			: base(entryNumbers)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.numbersCollection = Argument.NotNull(numbersCollection, "numbersCollection");
			this.numbersParent = Argument.NotNull(numbersParent, "numbersParent");
		}

		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly IBusinessObjectCollection numbersCollection;
		readonly BusinessObject numbersParent;

		#region Implementation

		protected override T[] BusinessObjects
		{
			get { return entryNumbers ?? (entryNumbers = numbersCollection.ToArray<T>()); }
		}

		T[] entryNumbers;

		protected override T FindMatchingBusinessObject(EntryNumber dataObject)
		{
			var finder = new EntryNumberBusinessObjectFinder(dataObject);
			return (T)finder.Find(BusinessObjects);
		}

		protected override T ReadIntoBusinessObject(EntryNumber dataObject, T entryNumber)
		{
			var reader = new EntryNumberDataObjectReader(dataObject, logger, factory, numbersParent, (dataObj) => entryNumber);
			return (T)reader.ReadIntoBusinessObject();
		}

		protected override void AddToCollection(T entryNumber)
		{
			numbersCollection.Add(entryNumber);
		}

		protected sealed override void RemoveFromCollection(T entryNumber)
		{
			// we're not removing mismatched entry numbers
		}

		#endregion
	}
}
