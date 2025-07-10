using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using static System.FormattableString;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public static class PackageParentJobMapper
	{
		public static DataObjectWriter<PkgPackage, UniversalShipment> GetWriter(ParentJobType parentType, IDataWritingManager writeManager)
		{
			DataObjectWriter<PkgPackage, UniversalShipment> result = null;

			if (parentType == ParentJobType.None)
			{
				result = new DefaultWriter(writeManager);
			}
			else
			{
				var packageExporters = ObjectFactory.Get<Hashtable>("UniversalPackingExporters");
				var parentJobType = parentType.ToString();
				if (packageExporters.ContainsKey(parentJobType))
				{
					var handle = (ObjectHandle)packageExporters[parentJobType];
					result = (DataObjectWriter<PkgPackage, UniversalShipment>)handle?.GetObject(writeManager);
				}

				if (result == null)
				{
					throw new InvalidOperationException(
						Invariant($"All ParentJobTypes must be mapped to UniversalPackingExporters in our Application Configuration. Missing Mapping for ParentJobType: {parentType}."));
				}
			}

			return result;
		}

		class DefaultWriter : TopLevelDataObjectWriter<PkgPackage, UniversalShipment>
		{
			public DefaultWriter(IDataWritingManager writeManager)
				: base(writeManager)
			{
			}

			protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			protected override DataContextType GetTopLevelDataContextType() => DataContextType.PkgPackage;

			protected override void PopulateDataObject(PkgPackage sourceBO, UniversalShipment dataObject)
			{
			}
		}
	}
}
