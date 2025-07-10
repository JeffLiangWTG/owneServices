using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DataTransfer
{
	class DocAddressValueObjectHelper : Enterprise.DataTransfer.DataAdapters.DocAddressValueObjectHelper
	{
		internal DocAddressValueObjectHelper(string errorContext)
			: base(errorContext)
		{
		}

		#region ImportFromValueObjectCollectionCore

		protected override void ImportFromValueObjectCollectionCore(DocAddressCollection docAddressValueCollection, JobDocAddressDependentCollection docAddresses, IValueObjectImportContext context)
		{
			if (docAddresses.Master is BaseJobDeclaration && ((BaseJobDeclaration)docAddresses.Master).IsStandAlone)
			{
				foreach (DocAddress docAddressValue in docAddressValueCollection)
				{
					var docAddressType = RemappingDictionary.ContainsKey(docAddressValue.AddressType)
										 && docAddressValueCollection.GetAddressByType(RemappingDictionary[docAddressValue.AddressType]) == null
											? RemappingDictionary[docAddressValue.AddressType] : docAddressValue.AddressType;

					CreateOrUpdateFromValueObject(docAddresses, docAddressType, docAddressValue, context);
				}
			}
			else
			{
				base.ImportFromValueObjectCollectionCore(docAddressValueCollection, docAddresses, context);
			}
		}

		void CreateOrUpdateFromValueObject(JobDocAddressDependentCollection docAddresses, DocAddressAddressType docAddressType, DocAddress docAddressValue, IValueObjectImportContext context)
		{
			var addressType = DocAddressTypes.GetDocAddressTypeFromCode(context.Factory, docAddressType.ToString());
			var docAddressToUpdate = docAddresses.FindOrCreateWithDocAddressType(addressType);
			ImportFromValueObject(docAddressValue, docAddressToUpdate, context);
		}

		#region RemappingDictionary

		Dictionary<DocAddressAddressType, DocAddressAddressType> RemappingDictionary
		{
			get
			{
				return remappingDictionary
					   ?? (remappingDictionary
						   = new Dictionary<DocAddressAddressType, DocAddressAddressType>
								{
									{ DocAddressAddressType.CED, DocAddressAddressType.IMD }, //Consignee Documentary Address -> Importer Documentary Address
									{ DocAddressAddressType.CEG, DocAddressAddressType.IMG }, //Consignee Pickup/Delivery Address -> Importer Pickup/Delivery Address
									{ DocAddressAddressType.CRD, DocAddressAddressType.SUD }, //Consignor Documentary Address -> Supplier Documentary Address
									{ DocAddressAddressType.CRG, DocAddressAddressType.SUG } //Consignor Pickup/Delivery Address -> Supplier Pickup/Delivery Address
								});
			}
		}

		Dictionary<DocAddressAddressType, DocAddressAddressType> remappingDictionary;

		#endregion

		#endregion
	}
}
