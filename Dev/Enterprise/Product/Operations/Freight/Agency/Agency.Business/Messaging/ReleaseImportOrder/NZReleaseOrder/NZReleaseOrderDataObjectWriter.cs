using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.Business
{
	class NZReleaseOrderDataObjectWriter : ITopLevelDataObjectWriter
	{
		public NZReleaseOrderDataObjectWriter(IDataWritingManager manager, ZString senderID, ZString port, ZString vesselLloydsImo, ZString messagePurpose)
		{
			this.agencyShipmentContainerDataObjectWriter = ((IShipmentDataContextManager)typeof(AgencyShipmentContainer).GetUniversalDataContextManager()).GetShipmentDataObjectWriter(manager);
			this.senderID = senderID;
			this.port = port;
			this.vesselLloydsImo = vesselLloydsImo;
			this.messagePurpose = messagePurpose;
		}
		readonly ITopLevelDataObjectWriter agencyShipmentContainerDataObjectWriter;
		readonly ZString senderID;
		readonly ZString port;
		readonly ZString vesselLloydsImo;
		readonly ZString messagePurpose;

		public DataContextType TopLevelDataContextType => DataContextType.AgencyShipmentContainer;

		public ZString EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		public ZString RootElementName => typeof(AgencyShipmentContainer).GetAttribute<RootElementAttribute>().RootElementName;

		public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var dataObject = (UniversalShipment)agencyShipmentContainerDataObjectWriter.GetDataObject(sourceBO);

			AddAddInfos(dataObject, sourceBO.Factory);
			AddVesselLloydsIMO(dataObject);
			AddDocumentaryOverridePurpose(dataObject);

			return dataObject;
		}

		void AddAddInfos(UniversalShipment dataObject, BusinessObjectFactory factory)
		{
			var addInfos = new List<AddInfo>();
			if (dataObject.AddInfoCollection != null)
			{
				addInfos.AddRange(dataObject.AddInfoCollection);
			}

			addInfos.Add(
				new AddInfo()
				{
					Key = "SenderID",
					Value = senderID.IsEmpty ? (NoResString)"CargoWise client Enterprise" : senderID.ToString()
				}
			);

			if (!port.IsEmpty)
			{
				addInfos.Add(
					new AddInfo()
					{
						Key = "OperationalPort_Code",
						Value = port
					}
				);
				addInfos.Add(
					new AddInfo()
					{
						Key = "OperationalPort_Name",
						Value = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, port).RL_PortName
					}
				);
			}

			dataObject.SetAddInfoCollection(() =>
			{
				return addInfos.Any() ? addInfos : null;
			});
		}

		void AddVesselLloydsIMO(UniversalShipment dataObject)
		{
			if (string.IsNullOrEmpty(dataObject.LloydsIMO) && !string.IsNullOrEmpty(vesselLloydsImo))
			{
				dataObject.LloydsIMO = vesselLloydsImo;
			}
		}

		void AddDocumentaryOverridePurpose(UniversalShipment dataObject)
		{
			if (!string.IsNullOrEmpty(messagePurpose))
			{
				dataObject.DataContext.SetDocumentaryOverride((NoResString)"Import Release Order", messagePurpose, null, true, 1, 1);
			}
		}
	}
}
