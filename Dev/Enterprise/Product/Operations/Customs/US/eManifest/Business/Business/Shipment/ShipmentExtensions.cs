using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public static class ShipmentExtensions
	{
		public static JobDeclaration CreateCustomsDeclaration(this Shipment shipmentBO, Trip tripBO)
		{
			JobDeclaration result = null;
			if (shipmentBO != null)
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var writer = ShipmentDataObjectWriter.New(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, shipmentBO)), shipmentBO, tripBO);
				var shipment = writer.GetDataObject(shipmentBO);
				shipment.DataContext = dataContext;

				var factory = new BusinessObjectFactory { NameForDebugging = "Shipment Extensions" };
				using (factory.AddDisposableService())
				{
					var events = UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(factory, shipmentBO, shipment, EDIMessageSubTypeList.Codes.XmlUniversalShipment);

					if (events != null && events.Length > 0)
					{
						var universalResult = PublishToUniversalResult.New(events, DataContextType.CustomsDeclaration, ZString.Empty);
						var basedec = universalResult.FindJobIfExists() as BaseJobDeclaration;
						if (basedec != null)
						{
							var declaration = factory.Load<JobDeclaration>(basedec.PK);
							declaration.Logs.AddNew(Events.TransferFromManifestToCustoms, "US eManifest: " + shipmentBO.B0_ReferenceID);

							CopyeDocs(tripBO, declaration);
							result = declaration;

							try
							{
								factory.Save();
								tripBO.BH_JobReferenceInfo.RefreshBinding();
							}
							catch (ZSaveException ex)
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}
					}
				}
			}
			return result;
		}

		public static void CopyeDocs(Trip tripBO, JobDeclaration declaration)
		{
			var docManagerInfo = ((IDocManagerSupport)tripBO).DocManagerInfo;
			docManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			var eDocs = docManagerInfo.AllEDocs;
			declaration.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			declaration.ShouldSaveEDocsMasterFactoryTogether = eDocs != null && eDocs.Count > 0;
			foreach (MasterFiles.Integration.IeDoc document in eDocs)
			{
				declaration.DocManagerInfo.AddFileOrDocument(document.ImageData, document.FileName, document.DocType);
			}
		}
	}
}
