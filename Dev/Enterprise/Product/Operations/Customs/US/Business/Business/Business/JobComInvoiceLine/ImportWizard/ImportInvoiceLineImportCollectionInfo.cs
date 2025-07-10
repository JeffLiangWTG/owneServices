using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.US.Business
{
	class ImportInvoiceLineImportCollectionInfo : IImportCollectionInfo
	{
		public ImportInvoiceLineImportCollectionInfo(IImportCollectionInfo collectionInfo)
		{
			this.collectionInfo = Argument.NotNull(collectionInfo, nameof(collectionInfo));
		}

		readonly IImportCollectionInfo collectionInfo;

		#region IImportCollectionInfo Members

		IBusinessObjectCollection IImportCollectionInfo.Collection => collectionInfo.Collection;

		IEnumerable<IImportPropertyInfo> IImportCollectionInfo.Properties
		{
			get
			{
				if (properties == null)
				{
					properties = new List<IImportPropertyInfo>(collectionInfo.Properties);
					if (!properties.Any(x => x.HeaderText == ManufacturerMID && x.MappingName == JobComInvoiceLine.Schema.ManufacturerMID))
					{
						var midProperty = new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.ManufacturerMID);
						midProperty.HeaderText = ManufacturerMID;
						properties.Add(midProperty);
					}
				}

				return properties;
			}
		}
		List<IImportPropertyInfo> properties;
		const string ManufacturerMID = "Manufacturer MID";
		bool IImportCollectionInfo.ValidateAndSave => collectionInfo.ValidateAndSave;

		void IImportCollectionInfo.OnImportStarted()
		{
			collectionInfo.OnImportStarted();
		}

		void IImportCollectionInfo.OnImportCompleted(bool success)
		{
			collectionInfo.OnImportCompleted(success);
		}
		#endregion
	}
}
