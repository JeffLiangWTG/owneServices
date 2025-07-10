using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class DangerousGoodsManifestDataObjectWriter : ITopLevelDataObjectWriter
	{
		public DangerousGoodsManifestDataObjectWriter(IDataWritingManager manager, ZString senderID, ZString portCode, ZString direction, ZString messagePurpose)
		{
			this.billOfLadingDataObjectWriter = ((IShipmentDataContextManager)typeof(BillOfLading).GetUniversalDataContextManager()).GetShipmentDataObjectWriter(manager);
			this.senderID = senderID;
			this.portCode = portCode;
			this.direction = direction;
			this.messagePurpose = messagePurpose;
		}
		readonly ITopLevelDataObjectWriter billOfLadingDataObjectWriter;
		readonly ZString senderID;
		readonly ZString portCode;
		readonly ZString direction;
		readonly ZString messagePurpose;

		public DataContextType TopLevelDataContextType => DataContextType.BillOfLading;

		public ZString EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		public ZString RootElementName => typeof(BillOfLading).GetAttribute<RootElementAttribute>().RootElementName;

		public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var dataObject = (UniversalShipment)billOfLadingDataObjectWriter.GetDataObject(sourceBO);
			var vessel = (sourceBO as BillOfLading)?.Sailing.Vessel;

			RemoveNonUNDGPacklinesAndContainers(dataObject);

			AddAddInfos(dataObject, sourceBO.Factory, vessel);

			if (!messagePurpose.IsEmpty)
			{
				dataObject.DataContext.SetDocumentaryOverride((NoResString)"Dangerous Goods Manifest", messagePurpose, null, true, 1, 1);
			}

			if (!string.IsNullOrEmpty(vessel?.RV_LloydsNumber))
			{
				dataObject.LloydsIMO = vessel.RV_LloydsNumber;
			}

			return dataObject;
		}

		void RemoveNonUNDGPacklinesAndContainers(UniversalShipment dataObject)
		{
			foreach (var p in dataObject.PackingLineCollection.Reverse())
			{
				if (p.UNDGCollection == null)
				{
					var containerNumber = p.ContainerNumber;
					dataObject.PackingLineCollection.Remove(p);

					if (!dataObject.PackingLineCollection.Any(pl => pl.ContainerNumber == containerNumber))
					{
						dataObject.ContainerCollection.Remove(dataObject.ContainerCollection.FirstOrDefault(c => c.ContainerNumber == containerNumber));
					}
				}
			}
		}

		void AddAddInfos(UniversalShipment dataObject, BusinessObjectFactory factory, RefVessel vessel)
		{
			var addInfos = new List<AddInfo>()
			{
				new AddInfo()
				{
					Key = "SenderID",
					Value = senderID
				},
				new AddInfo()
				{
					Key = "OperationalPort_Code",
					Value = portCode
				},
				new AddInfo()
				{
					Key = "OperationalPort_Name",
					Value = factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, portCode)?.RL_PortName ?? ZString.Empty
				},
				new AddInfo()
				{
					Key = (NoResString)"Direction",
					Value = direction
				},
				new AddInfo()
				{
					Key = "VesselType",
					Value = vessel?.RV_VesselType ?? ZString.Empty
				},
				new AddInfo()
				{
					Key = "VesselCountryOfReg",
					Value = vessel?.RV_RN_NKCountryOfReg ?? ZString.Empty
				}
			};

			if (dataObject.AddInfoCollection != null)
			{
				dataObject.AddInfoCollection.AddRange(addInfos);
			}
			else
			{
				dataObject.SetAddInfoCollection(() => addInfos);
			}
		}
	}
}
