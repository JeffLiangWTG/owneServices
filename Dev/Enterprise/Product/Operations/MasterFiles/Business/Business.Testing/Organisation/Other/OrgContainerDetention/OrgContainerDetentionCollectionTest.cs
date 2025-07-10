using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContainerDetentionCollection))]
	sealed class OrgContainerDetentionCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgContainerDetentionCollection>
	{
		public void TestRelationships()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var carrierPenaltyCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Carrier, ZString.Empty, Constants.ContainerPenaltyCreditorType.Codes.Carrier);
			var consignorCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Consignor, ZString.Empty, ZString.Empty);
			var consigneeCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Consignee, ZString.Empty, ZString.Empty);
			var consigneeCTOStorageCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Consignee, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			var consignorCTOStorageCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Consignor, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			var serviceImpCTOStorageCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.ServiceIMP, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			var serviceExpCTOStorageCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.ServiceEXP, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);

			var carrierPenalty = carrierPenaltyCollection.AddNew();
			var consignor = consignorCollection.AddNew();
			var consignee = consigneeCollection.AddNew();
			var consigneeCTOStorage = consigneeCTOStorageCollection.AddNew();
			var consignorCTOStorage = consignorCTOStorageCollection.AddNew();
			var serviceImpCTOStorage = serviceImpCTOStorageCollection.AddNew();
			var serviceExpCTOStorage = serviceExpCTOStorageCollection.AddNew();

			AssertDetention("carrierPenalty", carrierPenalty, parent, null, null, ZString.Empty, ZString.Empty, Constants.ContainerPenaltyCreditorType.Codes.Carrier);
			AssertDetention("consignor", consignor, null, parent, null, Constants.ContainerDetentionDirection.Export, ZString.Empty, ZString.Empty);
			AssertDetention("consignee", consignee, null, parent, null, Constants.ContainerDetentionDirection.Import, ZString.Empty, ZString.Empty);

			AssertDetention("consigneeCTOStorage", consigneeCTOStorage, null, parent, null, Constants.ContainerDetentionDirection.Import, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			AssertDetention("consignorCTOStorage", consignorCTOStorage, null, parent, null, Constants.ContainerDetentionDirection.Export, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			AssertDetention("serviceImpCTOStorage", serviceImpCTOStorage, null, null, parent, Constants.ContainerDetentionDirection.Import, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			AssertDetention("serviceExpCTOStorage", serviceExpCTOStorage, null, null, parent, Constants.ContainerDetentionDirection.Export, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);

			AssertContainsExactElementsInAnyOrder("carrierPenalty", new OrgContainerDetention[] { carrierPenalty }, carrierPenaltyCollection);
			AssertContainsExactElementsInAnyOrder("consignor", new OrgContainerDetention[] { consignor }, consignorCollection);
			AssertContainsExactElementsInAnyOrder("consignee", new OrgContainerDetention[] { consignee }, consigneeCollection);

			AssertContainsExactElementsInAnyOrder("consigneeCTOStorage", new OrgContainerDetention[] { consigneeCTOStorage }, consigneeCTOStorageCollection);
			AssertContainsExactElementsInAnyOrder("consignorCTOStorage", new OrgContainerDetention[] { consignorCTOStorage }, consignorCTOStorageCollection);
			AssertContainsExactElementsInAnyOrder("serviceImpCTOStorage", new OrgContainerDetention[] { serviceImpCTOStorage }, serviceImpCTOStorageCollection);
			AssertContainsExactElementsInAnyOrder("serviceExpCTOStorage", new OrgContainerDetention[] { serviceExpCTOStorage }, serviceExpCTOStorageCollection);
		}

		public void TestCreditorTypeDefaulting()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var consignorCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Consignor, ZString.Empty, ZString.Empty);
			var consigneeCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Consignee, ZString.Empty, ZString.Empty);
			var consigneeCTOStorageCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Consignee, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			var consignorCTOStorageCollection = new OrgContainerDetentionCollection(parent, OrgContainerDetentionCollection.ParentType.Consignor, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);

			var consignor = consignorCollection.AddNew();
			var consignee = consigneeCollection.AddNew();
			var consignorCTOStorage = consignorCTOStorageCollection.AddNew();
			var consigneeCTOStorage = consigneeCTOStorageCollection.AddNew();

			consignor.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.Carrier, consignor.PD_CreditorType);

			consignor.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.Carrier, consignor.PD_CreditorType);

			consignee.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.Carrier, consignee.PD_CreditorType);

			consignee.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.Carrier, consignee.PD_CreditorType);

			AssertEquals(Core.Constants.ContainerDetentionPenaltyType.STO, consignorCTOStorage.PD_PenaltyType);
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.CTO, consignorCTOStorage.PD_CreditorType);

			AssertEquals(Core.Constants.ContainerDetentionPenaltyType.STO, consigneeCTOStorage.PD_PenaltyType);
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.CTO, consigneeCTOStorage.PD_CreditorType);
		}

		#region Implemention

		void AssertDetention(string message, OrgContainerDetention detention, OrgHeader carrier, OrgHeader client, OrgHeader cto, ZString direction, ZString penaltyType, ZString creditorType)
		{
			AssertEquals(message + ": carrier", carrier == null ? ZGuid.Empty : carrier.PK, detention.PD_OH_Carrier);
			AssertEquals(message + ": client", client == null ? ZGuid.Empty : client.PK, detention.PD_OH_Client);
			AssertEquals(message + ": direction", direction, detention.PD_Direction);
			AssertEquals(message + ": penaltyType", penaltyType, detention.PD_PenaltyType);
			AssertEquals(message + ": creditorType", creditorType, detention.PD_CreditorType);
		}

		#endregion
	}
}
