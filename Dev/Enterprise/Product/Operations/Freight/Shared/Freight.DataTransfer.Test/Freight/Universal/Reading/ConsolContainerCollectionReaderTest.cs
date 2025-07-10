using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ConsolContainerCollectionReader<CommonContainer, CommonConsol>))]
	sealed class ConsolContainerCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var ref40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var ref20RE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");

			containerCollection.Content = CollectionContent.Complete;

			AddContainer(1, "CON0001", ref20GP);
			AddContainer(3, "CON0002", ref40GP);
			AddContainer(3, "", ref40GP);

			AddContainerDataObject(1, "CON0001", "20RE");
			AddContainerDataObject(3, "", "40GP");
			AddContainerDataObject(1, "CON0003", "20GP");

			var reader = new ConsolContainerCollectionReader<CommonContainer, CommonConsol>(containerCollection, logger, Factory, consol, new ContainerLinkManager<CommonConsol>(null));
			reader.ReadIntoCollection();

			AssertEquals(3, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref20RE.PK, 1);
			AssertContainer(consol.Containers[1], "", ref40GP.PK, 3);
			AssertContainer(consol.Containers[2], "CON0003", ref20GP.PK, 1);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.", logger.Logs);
		}

		public void TestReadIntoCollection_NoExceptionWithIllegalData()
		{
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			containerCollection.Content = CollectionContent.Partial;
			var firstContainer = AddContainer(1, ZString.Empty, ref20GP);
			firstContainer.JC_RC = ZGuid.Empty;
			AddContainerDataObject(1, "CON0001", "");

			var reader = new ConsolContainerCollectionReader<CommonContainer, CommonConsol>(containerCollection, logger, Factory, consol, new ContainerLinkManager<CommonConsol>(null));
			AssertNoExceptionThrown(reader.ReadIntoCollection);
		}

		public void TestReadContainerCollection_Partial()
		{
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var ref40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var ref20RE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");

			containerCollection.Content = CollectionContent.Partial;
			Func<ConsolContainerCollectionReader<CommonContainer, CommonConsol>> getReader = () => new ConsolContainerCollectionReader<CommonContainer, CommonConsol>(containerCollection, logger, Factory, consol, new ContainerLinkManager<CommonConsol>(null));

			//test1a: match single container, same container type
			AddContainer(1, "", ref20GP);
			AddContainerDataObject(1, "CON0001", "20GP");

			var reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(1, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref20GP.PK, 1, true);

			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.", logger.Logs);

			Reset();

			//test1b: match single container, same container type but inactive. Should match but set type to blank with warning
			AddContainer(1, "", ref20GP);
			AddContainerDataObject(1, "CON0001", "20GP");
			ref20GP.RC_IsActive = false;

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(1, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ZGuid.Empty, 1, true);
			AssertEquals($@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '{ref20GP.RC_Code}' is inactive so it will not be used, matched container's type '{ref20GP.RC_Code}' is also inactive so it will not be used. Please go to the consol and choose a container type.
Information - Container number advised: CON0001.", logger.Logs);

			ref20GP.RC_IsActive = true;
			Reset();

			//test2a: match single container, different container type. Both types are active.
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(1, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.", logger.Logs);

			Reset();
			//test2b: match single container, different container type. Existing consol container type is active, but import type is inactive. Should not update type
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "20GP");
			ref20GP.RC_IsActive = false;

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(1, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref40GP.PK, 1, true);
			AssertEquals($@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '{ref20GP.RC_Code}' is inactive so it will not be used, using the type from the matched container.
Information - Container number advised: CON0001.", logger.Logs);

			ref20GP.RC_IsActive = true;
			Reset();

			//test2c: match single container, different container type. Existing consol container type is inactive, import type is active. Should update with imported type
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "20GP");
			ref40GP.RC_IsActive = false;

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(1, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.", logger.Logs);

			ref40GP.RC_IsActive = true;
			Reset();

			//test2d: match single container, different container type. Both types are inactive. Should set type to blank
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "20GP");
			ref20GP.RC_IsActive = false;
			ref40GP.RC_IsActive = false;

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(1, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ZGuid.Empty, 1, true);
			AssertEquals($@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '{ref20GP.RC_Code}' is inactive so it will not be used, matched container's type '40GP' is also inactive so it will not be used. Please go to the consol and choose a container type.
Information - Container number advised: CON0001.", logger.Logs);

			ref20GP.RC_IsActive = true;
			ref40GP.RC_IsActive = true;
			Reset();

			//test3: match single container, picks one with same container type
			AddContainer(1, "", ref20GP);
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(2, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref20GP.PK, 1, true);
			AssertContainer(consol.Containers[1], "", ref40GP.PK, 1);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.", logger.Logs);

			Reset();

			//test4a: match multi-container, same container type
			AddContainer(4, "", ref20GP);
			AddContainerDataObject(1, "CON0001", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(2, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref20GP.PK, 3);
			AssertContainer(consol.Containers[1], "CON0001", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.", logger.Logs);

			Reset();

			//test4b: match multi-container, same inactive container type. Should show warning about inactive type and give existing and new containers empty type.
			AddContainer(4, "", ref20GP);
			AddContainerDataObject(1, "CON0001", "20GP");
			ref20GP.RC_IsActive = false;

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(2, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ZGuid.Empty, 3);
			AssertContainer(consol.Containers[1], "CON0001", ZGuid.Empty, 1, true);
			AssertEquals($@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '{ref20GP.RC_Code}' is inactive so it will not be used, matched container's type '{ref20GP.RC_Code}' is also inactive so it will not be used. Please go to the consol and choose a container type.
Warning - Matched container's Type has also been unset since it was inactive. Please go to the consol and choose a container type.
Information - Container number advised: CON0001.", logger.Logs);

			ref20GP.RC_IsActive = true;
			Reset();

			//test5: match multi-container, same container type, reduce twice
			AddContainer(4, "", ref20GP);
			AddContainerDataObject(1, "CON0001", "20GP");
			AddContainerDataObject(1, "CON0002", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(3, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref20GP.PK, 2);
			AssertContainer(consol.Containers[1], "CON0001", ref20GP.PK, 1, true);
			AssertContainer(consol.Containers[2], "CON0002", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0002.", logger.Logs);

			Reset();

			//test6: match multi-container, same container type, decrease container count to 1
			AddContainer(3, "", ref20GP);
			AddContainerDataObject(1, "CON0001", "20GP");
			AddContainerDataObject(1, "CON0002", "20GP");
			AddContainerDataObject(1, "CON0003", "20GP");
			AddContainerDataObject(1, "CON0004", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(4, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0003", ref20GP.PK, 1, true);
			AssertContainer(consol.Containers[1], "CON0001", ref20GP.PK, 1, true);
			AssertContainer(consol.Containers[2], "CON0002", ref20GP.PK, 1, true);
			AssertContainer(consol.Containers[3], "CON0004", ref20GP.PK, 1);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0002.
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0003.
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.", logger.Logs);

			Reset();

			//test7:Not match because there is only 1 multi-container with non-matching container type
			AddContainer(4, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(2, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref40GP.PK, 4);
			AssertContainer(consol.Containers[1], "CON0001", ref20GP.PK, 1);
			AssertEquals(@"Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.", logger.Logs);

			Reset();

			//test8: match multi-container, picks one with same container type
			AddContainer(4, "", ref40GP);
			AddContainer(4, "", ref20GP);

			AddContainerDataObject(1, "CON0001", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(3, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref40GP.PK, 4);
			AssertContainer(consol.Containers[1], "", ref20GP.PK, 3);
			AssertContainer(consol.Containers[2], "CON0001", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.", logger.Logs);

			Reset();

			//test9: match single container, no container type in USXML
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(1, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref40GP.PK, 1, true);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '' is invalid, using the type from the matched container.
Information - Container number advised: CON0001.", logger.Logs);

			Reset();

			//test10: match single container, container type is invalid in USXML
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "1234");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(1, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref40GP.PK, 1, true);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '1234' is invalid, using the type from the matched container.
Information - Container number advised: CON0001.", logger.Logs);

			Reset();

			//test11: match multi-container, no container type in USXML
			AddContainer(3, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "123");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(2, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref40GP.PK, 2);
			AssertContainer(consol.Containers[1], "CON0001", ref40GP.PK, 1, true);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '123' is invalid, using the type from the matched container.
Information - Container number advised: CON0001.", logger.Logs);

			Reset();

			//test12: does not match because there is no container type in USXML and there is more than one container entry
			AddContainer(1, "", ref20GP);
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(3, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref20GP.PK, 1);
			AssertContainer(consol.Containers[1], "", ref40GP.PK, 1);
			AssertContainer(consol.Containers[2], "CON0001", ZGuid.Empty, 1);
			AssertEquals(@"Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '' is invalid.", logger.Logs);

			Reset();

			//test13: does not match because container type is invalid in USXML and there is more than one container entry
			AddContainer(1, "", ref20GP);
			AddContainer(1, "", ref40GP);
			AddContainerDataObject(1, "CON0001", "1234");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(3, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref20GP.PK, 1);
			AssertContainer(consol.Containers[1], "", ref40GP.PK, 1);
			AssertContainer(consol.Containers[2], "CON0001", ZGuid.Empty, 1);
			AssertEquals(@"Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '1234' is invalid.", logger.Logs);

			Reset();

			//test14: keep existing unmatched containers
			AddContainer(1, "CON0001", ref20GP);
			AddContainer(3, "CON0002", ref40GP);
			AddContainer(3, "", ref40GP);

			AddContainerDataObject(1, "CON0001", "20RE");
			AddContainerDataObject(3, "", "40GP");
			AddContainerDataObject(1, "CON0003", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(4, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "CON0001", ref20RE.PK, 1);
			AssertContainer(consol.Containers[1], "CON0002", ref40GP.PK, 3); //unmatched one
			AssertContainer(consol.Containers[2], "", ref40GP.PK, 3);
			AssertContainer(consol.Containers[3], "CON0003", ref20GP.PK, 1);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.", logger.Logs);

			Reset();

			//test15: should match universal shipment xml container with container number and some container type to consol's container without container number and different container type
			//only if there is only one container in xml and one container on consol (with container count equal to 1)
			AddContainer(1, "", ref20GP);
			AddContainer(1, "", ref40GP);

			AddContainerDataObject(1, "CON0001", "40GP");
			AddContainerDataObject(1, "CON0002", "40GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(3, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref20GP.PK, 1);
			AssertContainer(consol.Containers[1], "CON0001", ref40GP.PK, 1);
			AssertContainer(consol.Containers[2], "CON0002", ref40GP.PK, 1);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.", logger.Logs);

			Reset();

			//test16: multi-matches, container count is 1
			AddContainer(1, "", ref40GP);
			AddContainer(1, "", ref20GP);
			AddContainer(1, "", ref20GP);

			AddContainerDataObject(1, "CON0001", "20GP");
			AddContainerDataObject(1, "CON0002", "20GP");
			AddContainerDataObject(1, "CON0003", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(4, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref40GP.PK, 1);
			AssertContainer(consol.Containers[1], "CON0001", ref20GP.PK, 1);
			AssertContainer(consol.Containers[2], "CON0002", ref20GP.PK, 1);
			AssertContainer(consol.Containers[3], "CON0003", ref20GP.PK, 1);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0002.
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.", logger.Logs);

			Reset();

			//test17: multi-matches, container count is greater than 1
			AddContainer(1, "", ref40GP);
			AddContainer(2, "", ref20GP);
			AddContainer(2, "", ref20GP);

			AddContainerDataObject(1, "CON0001", "20GP");
			AddContainerDataObject(1, "CON0002", "20GP");
			AddContainerDataObject(1, "CON0003", "20GP");

			reader = getReader();
			reader.ReadIntoCollection();

			AssertEquals(5, consol.Containers.Count);
			AssertContainer(consol.Containers[0], "", ref40GP.PK, 1);
			AssertContainer(consol.Containers[1], "CON0002", ref20GP.PK, 1);
			AssertContainer(consol.Containers[2], "", ref20GP.PK, 1);
			AssertContainer(consol.Containers[3], "CON0001", ref20GP.PK, 1);
			AssertContainer(consol.Containers[4], "CON0003", ref20GP.PK, 1);
			AssertEquals(@"Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0001.
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0002.
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: CON0003.", logger.Logs);

			Reset();
		}

		CommonContainer AddContainer(ZShort count, ZString containerNumber, RefContainer containerType)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = count;
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = containerType.PK;

			return container;
		}

		Container AddContainerDataObject(ZInt count, ZString containerNumber, ZString containerType)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			container.ContainerCount = count;
			container.ContainerNumber = containerNumber;
			container.ContainerType = new ContainerType { Code = containerType };

			containerCollection.Add(container);

			return container;
		}

		void Reset()
		{
			consol.Containers.RemoveAndDeleteAll();
			containerCollection.Clear();
			logger.ClearLogs();
		}

		void AssertContainer(CommonContainer container, string expectNumber, ZGuid expectTypePK, short expectCount, bool shouldCheckCIDLog = false)
		{
			AssertEquals(expectNumber, container.JC_ContainerNum);
			AssertEquals(expectTypePK, container.JC_RC);
			AssertEquals(expectCount, container.JC_ContainerCount);

			if (shouldCheckCIDLog)
			{
				var parameters = new[]
				{
					Params.New.AsKeyFor(expectNumber),
					Params.Type.AsKeyFor(Constants.EventReferenceParameterTypes.ContainerID),
					Params.Reason.AsKeyFor(Constants.EventReferenceParameterReasons.ContainerNumberAdvised)
				};

				var logs = container.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ChangeOfIdentifierCode))
					.Where(x => x.SL_Reference == StmALog.GenerateEventReference("", parameters));
				AssertEquals(1, logs.Count());
			}
		}

		TestErrorLogger logger;
		CommonConsol consol;
		DataObjectList<Container> containerCollection;

		protected override void SetUp()
		{
			logger = new TestErrorLogger();
			consol = Factory.New<CommonConsol>();
			containerCollection = new DataObjectList<Container>();

			base.SetUp();
		}
	}
}
