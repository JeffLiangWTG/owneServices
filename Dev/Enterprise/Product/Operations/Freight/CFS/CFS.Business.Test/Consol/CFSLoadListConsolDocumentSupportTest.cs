using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolDocumentSupporter))]
	class CFSLoadListConsolDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage_NoContainers()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_UniqueConsignRef = "C00001";

			consol.Containers.RemoveAndDeleteAll();

			var contextArray = new[]
			{
				Constants.DataContext.ERA,
				Constants.DataContext.IMO,
				Constants.DataContext.Container,
				Constants.DataContext.PackUnpackContainerRego
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "No containers are entered for Load List C00001.";

			AssertNotFoundMessage(consol, menu, contextArray, true, expectedMessage);

			consol.Containers.AddNew();

			AssertNotFoundMessage(consol, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_GenericFreightJobByPackages()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_UniqueConsignRef = "C00001";

			var contextArray = new[]
			{
				Constants.DataContext.GenericFreightJobByPackages
			};

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = LabelsName.ImportLabel;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var expectedMessage = "A Shipment attached to this Load List must have at least one packline.";

			AssertNotFoundMessage(consol, menu, contextArray, true, expectedMessage);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;

			AssertNotFoundMessage(consol, menu, contextArray, false, ZString.Empty);
		}

		protected override bool ShouldSkipWithContextAndMenu(Constants.DataContext context, IStmMenuItem menu)
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var supporter = consol.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get { return new[] { Factory.New<CFSLoadListConsol>() }; }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.Containers.AddNew();
			Factory.Save();

			return consol;
		}

		public void TestGetDocBusinessObjectDefaultsConsolInfo()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var queryProvider = new Mock<ICommonConsolDocumentSupporterQueryProvider>();

			Factory.SetValue(() => queryProvider.Object);
			queryProvider
				.Setup(m => m.GetConsolToPrint(It.Is<DocumentCommonConsol>(p => p.IncludeAllShipments)))
				.Returns(new DocumentCommonConsol(consol, Core.Constants.DataContext.LoadListDocument));

			DocumentWrapper[] wrappers = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LoadListDocument, null);
			AssertNotNull(wrappers);
			queryProvider
				.Verify(m => m.GetConsolToPrint(It.Is<DocumentCommonConsol>(p => p.IncludeAllShipments)), Times.Once);
		}

		public void TestGetGenericWrapperForOuterPacks()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = LabelsName.ImportLabel;

			var consol = Factory.New<CFSLoadListConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "USCHI";
			transport.JW_RL_NKDiscPort = "AUSYD";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "USCHI";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_F3_NKPackType = "PLT";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "USCHI";
			shipment2.JS_RL_NKDestination = "AUSYD";
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = "PLT";

			Factory.Save();

			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByPackages, menuItem);
			AssertEquals("wrappers should be created for both shipments", 5, wrapper.Length);

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			Factory.Save();

			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByPackages, menuItem);
			AssertEquals("wrappers should be created for STD Type shipmnent only", 3, wrapper.Length);

			shipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Factory.Save();

			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByPackages, menuItem);
			AssertEquals("no wrappers created", 0, wrapper.Length);
		}

		public void TestGetGenericWrapperForOuterPacksMatchImportLabelPrefix()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = LabelsName.ImportLabel + "XXX";

			var consol = Factory.New<CFSLoadListConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "USCHI";
			transport.JW_RL_NKDiscPort = "AUSYD";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "USCHI";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_F3_NKPackType = "PLT";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "USCHI";
			shipment2.JS_RL_NKDestination = "AUSYD";
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = "PLT";

			Factory.Save();

			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByPackages, menuItem);
			AssertEquals("wrappers should be created for both shipments", 5, wrapper.Length);

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			Factory.Save();

			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByPackages, menuItem);
			AssertEquals("wrappers should be created for STD Type shipmnent only", 3, wrapper.Length);

			shipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Factory.Save();

			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByPackages, menuItem);
			AssertEquals("no wrappers created", 0, wrapper.Length);
		}

		public void TestGetIMOWrappers()
		{
			var consol = Factory.New<CFSLoadListConsol>();

			var queryProvider = new Mock<ICommonConsolDocumentSupporterQueryProvider>();

			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.Setup(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false))
				.Returns(new ContainersToPrintOptions());

			var command = Factory.New<StmMenuItem>();
			DocumentWrapper[] wrappers = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.IMO, command);
			AssertNull(wrappers);
			queryProvider
				.Verify(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false),
					Times.Once);

			CFSContainer container = consol.Containers.AddNew();
			CFSShipment shipment = consol.Shipments.AddNew();

			CFSPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			container.PackLines.Add(packLine1);

			CFSPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			container.PackLines.Add(packLine2);

			Factory.Save();

			queryProvider
				.Setup(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false))
				.Returns(new ContainersToPrintOptions { ContainersToPrint = new CommonContainer[] { container } });

			wrappers = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.IMO, command);
			AssertNotNull(wrappers);
			queryProvider
				.Verify(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false),
					Times.Exactly(2));

			container.PackLines.Remove(packLine2);

			queryProvider
				.Setup(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false))
				.Returns(new ContainersToPrintOptions { ContainersToPrint = new CommonContainer[] { container } });

			wrappers = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.IMO, command);
			AssertNotNull(wrappers);

			queryProvider
				.Verify(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false),
					Times.Exactly(3));
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable businessObject)
		{
			if (command.SU_MenuName == "Import Label")
			{
				var consol = (CFSLoadListConsol)businessObject;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "ASDF0000909";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 4;
				packLine.JL_JC = container.PK;
			}

			if (command.SU_MenuName == "On Forwarding Label")
			{
				var consol = (CFSLoadListConsol)businessObject;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "ASDF0000909";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUMEL";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 4;
				packLine.JL_JC = container.PK;
			}

			if (command.SU_MenuName == "Transhipment Label")
			{
				var consol = (CFSLoadListConsol)businessObject;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "ASDF0000909";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USCHI";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 4;
				packLine.JL_JC = container.PK;
			}
		}
	}
}
