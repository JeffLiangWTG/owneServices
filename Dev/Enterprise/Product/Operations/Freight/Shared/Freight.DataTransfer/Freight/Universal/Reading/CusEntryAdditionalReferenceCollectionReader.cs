using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class CusEntryAdditionalReferenceCollectionReader<T> : AdditionalReferenceCollectionReader<CusEntryNumber, T>
		where T : BusinessObject, IStmNoteParent
	{
		public CusEntryAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, T parent)
			: base(additionalReferenceDataObjects, logger, factory, parent)
		{
		}

		#region IsInvalidAdditionalReferenceType

		protected override bool IsInvalidAdditionalReferenceType(AdditionalReference dataObject)
		{
			var type = dataObject.Type.GetCodeAsUpperCase();
			var customsAdditionalReferenceTypes = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(Parent, Factory.BOFactory, CusEntryNumber.Categories.AdditionalReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var transitAdditionalReferenceTypes = new ShipmentNonCustomsAdditionalReferenceCodesCodeList();

			return (type != GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber) && !customsAdditionalReferenceTypes.ContainsCode(type) && !transitAdditionalReferenceTypes.ContainsCode(type);
		}

		#endregion

		#region Matching

		protected override CusEntryNumber FindMatchingBusinessObject(AdditionalReference dataObject)
		{
			return new AdditionalReferenceBusinessObjectFinder(dataObject).Find(BusinessObjects);
		}

		protected override CusEntryNumber[] BusinessObjects
		{
			get { return Numbers.Cast<CusEntryNumber>().ToArray(); }
		}

		protected abstract CusEntryNumAdditionalReferenceCollection Numbers { get; }

		#endregion

		#region Creating/Updating

		protected override void AddToCollection(CusEntryNumber additionalRefNumber)
		{
			Numbers.Add(additionalRefNumber);
		}

		protected override void RemoveFromCollection(CusEntryNumber additionalRefNumber)
		{
			Numbers.RemoveAndDelete(additionalRefNumber);
		}

		protected override CusEntryNumber ReadIntoBusinessObject(AdditionalReference dataObject, CusEntryNumber additionalRefNumber)
		{
			var reader = new AdditionalReferenceDataObjectReader(dataObject,
				Logger, Factory, additionalReferenceData => additionalRefNumber);

			return reader.ReadIntoBusinessObject();
		}

		#endregion
	}
}
