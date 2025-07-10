using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingExportPreAdviceDataObjectWriter : ITopLevelDataObjectWriter
	{
		public AgencyBookingExportPreAdviceDataObjectWriter(ReleaseHeader header, IDataWritingManager writeManager, string documentName, string purposeCode)
		{
			writer = ((IShipmentDataContextManager)header.Shipment.GetUniversalDataContextManager()).GetShipmentDataObjectWriter(writeManager);
			this.includeMessage = header.IncludeMessage;
			this.documentName = documentName;
			this.purposeCode = purposeCode;
		}

		readonly ITopLevelDataObjectWriter writer;
		readonly bool includeMessage;
		readonly string documentName;
		readonly string purposeCode;

		public DataContextType TopLevelDataContextType => writer.TopLevelDataContextType;

		public ZString EDIMessageSubType => writer.EDIMessageSubType;

		public ZString RootElementName => writer.RootElementName;

		public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var dataObject = writer.GetDataObject(sourceBO) as UniversalShipment;
			var sourceObject = sourceBO as AgencyBooking;
			var portSenderID = AgencyRegistry.Instance.ContainerExportPreAdvicePorts.FindByPortAndPrincipalWithFallback(sourceObject.JS_NKLoadPort, sourceObject.JS_OH_DeliveryAgent)?.SenderID ?? ZString.Empty;

			if (includeMessage && !portSenderID.IsEmpty)
			{
				dataObject.DataContext.SetDocumentaryOverride(documentName, purposeCode, null, true, 1, 1);
				dataObject.LloydsIMO = RefVessel.LookupVesselByFK(sourceObject.Sailing.JX_JV_NKVessel, sourceObject.Factory)?.RV_LloydsNumber ?? ZString.Empty;

				var addInfoCollection = new List<AddInfo>()
				{
					new AddInfo()
					{
						Key = "SenderID",
						Value = portSenderID
					},
					new AddInfo()
					{
						Key = "OperationalPort_Code",
						Value = sourceObject.JS_NKLoadPort
					},
					new AddInfo()
					{
						Key = "OperationalPort_Name",
						Value = sourceObject.Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, sourceObject.JS_NKLoadPort).RL_PortName
					}
				};

				if (dataObject.AddInfoCollection == null)
				{
					dataObject.SetAddInfoCollection(() => addInfoCollection);
				}
				else
				{
					dataObject.AddInfoCollection.AddRange(addInfoCollection);
				}
			}

			return dataObject;
		}
	}
}
