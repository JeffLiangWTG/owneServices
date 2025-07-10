using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSContainer))]
	public class CFSContainerBusinessObjectTest : CFSBusinessObjectTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			CFSContainer res = Factory.New<CFSContainer>();
			return res;
		}

		#endregion
		#region Property Tests

		public void TestHasChangesOnCreate()
		{
			CFSContainer newContainer = Factory.New<CFSContainer>();
			AssertEquals("should be no changes on create", false, newContainer.HasChanges);
		}

		public void TestHasChangesOnLoad()
		{
			CFSContainer newContainer = Factory.New<CFSContainer>();
			newContainer.OnLoaded();
			AssertEquals("should be no changes on create", false, newContainer.HasChanges);
		}

		#region TestAllowSurplusPacks

		public void TestAllowSurplusPacks()
		{
			AssertEquals("no surplus", false, Container.PackUnpackShipments.AllowSurplusPacks);
		}

		#endregion

		#region JC_OH_CFSClient

		public void TestJC_OH_CFSClient()
		{
			ZGuid testGuid = ZGuid.NewZGuid();
			Container.JC_OH_CFSClient = testGuid;
			AssertEquals("JC_OH_CFSClient", testGuid, Container.JC_OH_CFSClient);

			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_OH_Forwarder = ZGuid.NewZGuid();
			loadList.Containers.Add(Container);
			AssertEquals("JC_JK", loadList.PK, Container.JC_JK);
			AssertEquals("JC_OH_CFSClient", loadList.JK_OH_Forwarder, Container.JC_OH_CFSClient);

			Container.JC_OH_CFSClient = testGuid;
			AssertEquals("JC_OH_CFSClient", testGuid, Container.JC_OH_CFSClient);
			AssertEquals("JC_JK", ZGuid.Empty, Container.JC_JK);
		}

		#endregion

		public void TestJC_Calc_ConsolEntryNumber()
		{
			AssertEquals("Container shuld not have a consol by default", ZGuid.Empty, Container.JC_JK);
			AssertEquals("Calculated entry number should be blank", ZString.Empty, Container.JC_Calc_ConsolEntryNumber);
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_CustomsReference = "qwerts";
			AssertEquals("Calculated entry number should be blank", ZString.Empty, Container.JC_Calc_ConsolEntryNumber);
			Container.JC_JK = consol.PK;
			AssertEquals("Calculated entry number should inherit from consol", "qwerts", Container.JC_Calc_ConsolEntryNumber);
		}

		#endregion

		#region Validation

		#region JC_RC

		public void TestValidateJC_RC()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			CodeDescriptionPairList containerModes = container.JC_ContainerMode_List;

			foreach (CodeDescriptionPair pair in containerModes)
			{
				container.JC_ContainerMode = pair.Code;

				container.JC_RC = ZGuid.Empty;

				if (container.JC_ContainerMode == Constants.ContainerModes.Bulk
				|| container.JC_ContainerMode == Constants.ContainerModes.Liquid
				|| container.JC_ContainerMode == Constants.ContainerModes.ULD
				|| container.JC_ContainerMode == Constants.ContainerModes.BreakBulk
				|| container.JC_ContainerMode == Constants.ContainerModes.AIR)
				{
					Assert("container type is not mandatory for BLK, BBK, AIR, ULD", !container.JC_RCInfo.HasNotifications());
				}
				else
				{
					Assert("Container type is mandatory, expecting errors", container.JC_RCInfo.HasErrors());

					var rC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
					container.JC_RC = rC.PK;
					Assert("Container type is mandatory and entered, not expecting errors", !container.JC_RCInfo.HasErrors());
				}
			}
		}

		#endregion

		#region JC_ContainerNum

		public void TestValidateJC_ContainerNum()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			ContainerModeCodeDescriptionPairList containerModes = new ContainerModeCodeDescriptionPairList(container.JC_TransportMode);

			foreach (CodeDescriptionPair pair in containerModes)
			{
				container.JC_ContainerMode = pair.Code;
				container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, "SEA")).PK;

				container.JC_ContainerNum = "ABCD1111117";
				AssertNoNotifications("never a problem", container.JC_RCInfo);

				container.JC_ContainerNum = "";
				AssertNoNotifications("never a problem", container.JC_ContainerNumInfo);

				container.JC_ContainerNum = "1111";
				AssertEquals("warning expected", true, container.JC_ContainerNumInfo.HasWarnings());
			}
		}

		#endregion

		#region TestValidateTotalsAgainstPackLines

		public void TestValidateTotalsAgainstPackLines()
		{
			AssertEquals("no totals validation", false, Container.PackUnpackShipments.ValidateTotalsAgainstPackLines);
		}

		#endregion

		#endregion

		#region Services

		#region TestServicesCannotBeAddedOrCancelledByUser

		public void TestServicesCannotBeAddedOrCancelledByUser()
		{
			Event[] eventsToExclude =
			{
				Events.ServiceRequested,
				Events.ServiceCompleted,
				Events.QuarantineRequired,
				Events.QuarantineComplete,
				Events.CustomsImpedimentReceived,
				Events.CustomsCleared,
				Events.ExportCustomsCleared,
				Events.InspectionComplete,
			};

			CFSContainer container = Factory.New<CFSContainer>();
			foreach (Event item in eventsToExclude)
			{
				AssertEquals(item.Code, true, container.Logs.EventsThatCannotBeAdded.Contains(item));
				AssertEquals(item.Code, true, container.Logs.EventsThatCannotBeCancelled.Contains(item));
			}
		}

		#endregion

		#endregion

		public void TestAddAndEditEvents()
		{
			AssertEquals("precondition", 0, Container.Logs.GetAllLogs().Count);
			Container.OrgDebtor_List.Load();
			Container.JC_OH_CFSClient = Container.OrgDebtor_List[0].PK;
			Container.JC_ContainerNum = "x";
			Container.JC_RC = Container.RefContainer_List[0].PK;
			AssertEquals("should be no created event yet", null, Container.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem));
			Factory.Save();
			Assert("should have added a created event", Container.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem) != null);
			Container.JC_ContainerNum = "y";
			AssertEquals("should be no edited event yet", null, Container.Logs.MostRecentLogByEventTime(Events.EditedARecord));
			Factory.Save();
			Assert("should have added an edited event", Container.Logs.MostRecentLogByEventTime(Events.EditedARecord) != null);
		}

		public void TestNoteTypesCanBeAccessed()
		{
			AssertNotNull(Container.NoteTypes);
		}

		public void TestBreakBulkContainerMode()
		{
			Container.JC_ContainerMode = Constants.ContainerModes.FCL;
			Container.JC_RC = ZGuid.NewZGuid();
			Assert("Container Type should not be readonly", !Container.JC_RCInfo.ReadOnly);

			Container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			Assert("Container Type should be readonly", Container.JC_RCInfo.ReadOnly);
			AssertEquals("Container Type should be ZGuid.Empty", ZGuid.Empty, Container.JC_RC);
		}

		public void TestBreakBulkAvailableForNonAir()
		{
			CodeDescriptionPairList list;

			Container.JC_TransportMode = Constants.TransportModes.Air;
			list = Container.JC_ContainerMode_List;
			Assert("Break Bulk is not a viable option for Air transport", !list.ContainsCode(Constants.ContainerModes.BreakBulk));

			Container.JC_TransportMode = Constants.TransportModes.Sea;
			list = Container.JC_ContainerMode_List;
			Assert("Break Bulk is a viable option for Sea transport", list.ContainsCode(Constants.ContainerModes.BreakBulk));
		}

		public void TestContainerMode_List_Air()
		{
			Container.JC_TransportMode = Constants.TransportModes.Air;
			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.ContainerModes.ULD }, Container.JC_ContainerMode_List.GetAllCodes());
		}

		#region Implementation

		protected CFSContainer Container;
		protected ZDateTime TestTime;

		protected override void SetUp()
		{
			base.SetUp();
			TestTime = ZDateTime.Now.AddDays(10);
			new ConstantsAndReusables(Factory).EnsureCurrentCompanyMatchesCurrentBranch();
			Container = Factory.New<CFSContainer>();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			Container = Factory.New<CFSContainer>();
			//Container.CreateDefaultArrivalDepartureLegs();
			//AssertNotNull(Container.CFSArrival);
			//AssertNotNull(Container.CFSDispatch);
			return Container;
		}
		#endregion
	}
}
