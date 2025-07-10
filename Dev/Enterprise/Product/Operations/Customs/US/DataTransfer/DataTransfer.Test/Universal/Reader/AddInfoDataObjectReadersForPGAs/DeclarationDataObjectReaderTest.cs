using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest
	{
		public void TestImportingVehicleOrganisations()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var owner = CreateOrganisation("OWNER", "ABC#@1");
			var storage = CreateOrganisation("LOCATION", "ABC#@2");
			var addInfos = new Dictionary<ZString, ZString>();
			var dataObject = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			UpdateAddInfo(addInfos, USVehicleAddInfoSchema.US_BodyDescription, (ZString)"TEST");
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			dataObject.AddInfoCollection = AddInfoCollectionCreator.CreateCollection(builder.ToString());
			dataObject.Type = new CodeDescriptionPair()
			{ Code = CusAddInfoTypeAttribute.Codes.USPGAVehicle };
			dataObject.AddOrgAddress(writeManager, owner, Constants.AddressType.Owner);
			dataObject.AddOrgAddress(writeManager, storage, nameof(DocAddressType.Location));
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair()
				{
					Code = JobMessageTypeList.Codes.Import
				},
				MessagingApplicationCode = new CodeDescriptionPair()
				{
					Code = JobApplicationCodeList.Codes.ACE
				},
				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										dataObject
									})
								}
							})))
					})
				}
			};
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo()
				{
					Key = "EnableENS",
					Value = "Y"
				},
				new AddInfo()
				{
					Key = "CargoReleaseType",
					Value = "ACE"
				}
			});
			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "OW234#$#"));
			AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			AssertEquals("Vehicle Lines count", 1, invoiceLine.VehicleLines.Count);
			var line = invoiceLine.VehicleLines[0];
			Assert(line.US_OA_Owner == owner.MainAddress.PK);
			Assert(line.US_OA_StorageLocation == storage.MainAddress.PK);
		}
	}
}
