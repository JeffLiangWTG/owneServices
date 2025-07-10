using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public static class ContainerYardUniversalHelper
	{
		public static IOrgHeader GetClient(UniversalObjectFactory factory, IXmlImportLogger logger, Shipment dataObject)
		{
			var dataContext = logger.TopLevelDataContext;
			var client = GetClient(dataObject, logger, factory);
			client ??= dataContext.GetSourceOrganisation(logger, factory.BOFactory);
			if (client == null && dataContext.IsFromSameSystem())
			{
				client = GlbBranch.CurrentBranch.OrgProxy;
			}

			return client;
		}

		public static string GetBookingConfirmationReference(Shipment subShipment)
		{
			return subShipment.BookingConfirmationReference;
		}

		public static string GetTransportReference(Shipment subShipment)
		{
			return subShipment.AdditionalReferenceCollection?
				.Where(reference => reference.Type.Code.Equals(AdditionalReferenceTypes.Codes.TransportReference))
				.Select(reference => reference.ReferenceNumber)
				.FirstOrDefault();
		}

		public static WhsWarehouse GetYard(Shipment sourceDO, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var yard = YardMatchingHelper.GetYard(sourceDO, factory, logger);

			return yard is not null ?
				factory.Load<WhsWarehouse>(yard.GetValue(WhsWarehouseSchema.PK)) :
				throw new DataObjectReadFailureException(Res.GetString("3278198F-2459-420F-AE98-95FBD306F171", "Yard is not found."));
		}

		static IOrgHeader GetClient(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			OrgHeader client = null;
			if (dataObject != null && dataObject.OrganizationAddressCollection != null)
			{
				var organizationAddress = dataObject.OrganizationAddressCollection.Find(a => a.AddressType.GetValueOrDefault().EqualsIgnoringCase(nameof(DocAddressType.BookingPartyDocumentaryAddress)));
				if (organizationAddress != null)
				{
					var reader = new OrganisationDataObjectReader(organizationAddress, logger, factory);
					var orgAddress = reader.GetMatched();
					client = orgAddress?.Header;

					if (orgAddress == null)
					{
						logger.Log(LogType.Warning, $"Could not find Sending Party Organisation with Client ID '{organizationAddress.OrganizationCode}'.");
					}
				}
			}

			return client;
		}

		public static void AddStmALog(UniversalObjectFactory factory, IXmlImportLogger logger, Guid parentPK, string parentTableName, string reference, string eventCode)
		{
			var row = factory.RowFactory.NewRowWithPK(StmALogSchema.Instance);
			row.SetValue(StmALogSchema.SL_EventTime, ZDateTime.Now, logger);
			row.SetValue(StmALogSchema.SL_EventTimeUtc, ZDateTime.UtcNow, logger);
			row.SetValue(StmALogSchema.SL_FireWorkflow, ZBool.True, logger);
			row.SetValue(StmALogSchema.SL_GB_NKBranch, Env.Instance.CurrentBranch.Code, logger);
			row.SetValue(StmALogSchema.SL_GE_NKDepartment, Env.Instance.CurrentDepartment.Code, logger);
			row.SetValue(StmALogSchema.SL_GS_NKUser, (NoResString)"~AD", logger); // User code for service task
			row.SetValue(StmALogSchema.SL_Parent, parentPK, logger);
			row.SetValue(StmALogSchema.SL_PostedTimeUtc, ZDateTime.UtcNow, logger);
			row.SetValue(StmALogSchema.SL_SE_NKEvent, eventCode, logger);
			row.SetValue(StmALogSchema.SL_Table, parentTableName, logger);
			row.SetValue(StmALogSchema.SL_Reference, reference, logger);
		}
	}
}
