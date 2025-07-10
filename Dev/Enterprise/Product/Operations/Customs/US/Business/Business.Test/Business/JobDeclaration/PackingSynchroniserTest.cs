using System;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class PackingSynchroniserTest : Customs.Business.Testing.PackingSynchroniserTest<JobDeclaration>
	{
		public override void TestSychroniseCore()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";//US syncs to MB as 12345678
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX123456";

			consol.Shipments.Add(shipment);

			declaration.CusContainers.AddNew().CO_ContainerNumber = container.JC_ContainerNum;

			PackLine containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 10;
			containerised.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			containerised.JL_JC = container.PK;
			containerised.JL_MarksAndNumbers = "MarksAndNumbers1";

			containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 15;
			containerised.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			containerised.JL_JC = container.PK;
			containerised.JL_MarksAndNumbers = "MarksAndNumbers2";

			PackLine nonContainerised = shipment.OuterPackLines.AddNew();
			nonContainerised.JL_PackageCount = 20;
			nonContainerised.JL_F3_NKPackType = Core.Constants.PkgUnit.Carton;
			nonContainerised.JL_JC = ZGuid.Empty;
			nonContainerised.JL_Calc_ContainerNumber = "";
			nonContainerised.JL_MarksAndNumbers = "MarksAndNumbers3";

			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("There should be three rows", 3, declaration.Packages.Count);

			Package package = declaration.Packages[0];
			AssertEquals(10, package.CW_PackQty);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Pallet, package.CW_PackType);
			AssertEquals(declaration.CusContainers[0], package.PackingGroup.Container);
			AssertEquals("MarksAndNumbers1", package.CW_MarksAndNos);

			package = declaration.Packages[1];
			AssertEquals(15, package.CW_PackQty);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Bag, package.CW_PackType);
			AssertEquals(declaration.CusContainers[0], package.PackingGroup.Container);
			AssertEquals("MarksAndNumbers2", package.CW_MarksAndNos);

			package = declaration.Packages[2];
			AssertEquals(20, package.CW_PackQty);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Carton, package.CW_PackType);
			AssertEquals(null, package.PackingGroup.Container);
			AssertEquals("MarksAndNumbers3", package.CW_MarksAndNos);
		}

		#region TestGetConvertedPackUQ

		[ExpectNoExceptions]
		public void TestGetConvertedPackUQ()
		{
			ErrorReporter.Clear();
			try
			{
				var mappings = USCustomsDataRegistry.Instance.USPackageTypesMapping.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				USCustomsDataRegistry.Instance.USPackageTypesMapping.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, mappings);

				var task1 = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection = Db.NewExtraConnectionToMainDb())
					{
						var factory = new BusinessObjectFactory(connection);
						var declaration = factory.New<JobDeclaration>();
						var shipment = factory.New<ForwardingShipment>();
						declaration.JE_JS = shipment.PK;
						for (int i = 0; i < 200; i++)
						{
							var mapping = USCustomsDataRegistry.Instance.USPackageTypesMapping.GetValueWithoutFallback(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty);
							var item = mapping[1];
							item.SuspendValidation();
							item.CustomsPackageType = "TTT";
						}
					}
				});

				var task2 = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection = Db.NewExtraConnectionToMainDb())
					{
						var factory = new BusinessObjectFactory(connection);
						var declaration = factory.New<JobDeclaration>();
						var shipment = factory.New<ForwardingShipment>();
						declaration.JE_JS = shipment.PK;
						var synchroniser = new PackingSynchroniserForTest(new JobDeclarationSynchroniser(declaration), declaration);
						for (int i = 0; i < 200; i++)
						{
							synchroniser.GetConvertedPackUQ_Exposed("SSS");
						}
					}
				});

				Task.WaitAll(task1, task2);
			}
			finally
			{
				AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
			}
		}

		class PackingSynchroniserForTest : PackingSynchroniser
		{
			public PackingSynchroniserForTest(JobDeclarationSynchroniser parentSynchroniser, Customs.Business.BaseJobDeclaration declaration) : base(parentSynchroniser, declaration)
			{
			}

			public ZString GetConvertedPackUQ_Exposed(ZString freightPackType)
			{
				return base.GetConvertedPackUQ(freightPackType);
			}
		}

		#endregion

		protected override JobDeclaration GetDeclarationPackingRelevant()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			return result;
		}

		protected override JobDeclaration GetDeclarationPackingNotRelevant()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			return result;
		}
	}
}
