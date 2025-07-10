using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		protected override JobDeclaration GetNewJobDeclaration()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ClusterKey = 1;
			return (JobDeclaration)declaration;
		}

		protected override DocDeclaration CreateDeclarationWrapper(JobDeclaration declaration)
		{
			var result = DocDeclaration.New(declaration, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>());
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		public override void TestTransportModeDescription()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TransportModeDescription", "4 - Air", DeclarationWrapper.TransportModeDescription);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportModeDescription", "1 - Sea", DeclarationWrapper.TransportModeDescription);
		}

		public override void TestHeadingTransportModeWithPackingMode()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_ContainerMode = Declaration.TransportModeAirCodeForTesting;

			AssertEquals("No packing mode in heading", "1 - Sea", DeclarationWrapper.HeadingTransportMode);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			AssertEquals("Packing mode in heading", "FCL 1 - Sea", DeclarationWrapper.HeadingTransportMode);
		}

		public override void TestHeadingTransportMode()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("4 - Air", DeclarationWrapper.HeadingTransportMode);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("1 - Sea", DeclarationWrapper.HeadingTransportMode);
		}

		public override void TestShipperDepartureNoticeDocumentHeader()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "4 - Air Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "1 - Sea Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "FCL 1 - Sea Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "LCL 1 - Sea Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);
		}

		protected override string MessageTypeCodeForImports => MessageTypeCodeList.Codes.IPT;

		protected override string MessageTypeCodeForExports => MessageTypeCodeList.Codes.OUT;

		protected override string TestingCountry => Core.Constants.CountryCodes.Singapore;
	}
}
