using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	public abstract class VesselRoutingVoyagesFilterTestBase : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestAllPorts();
		public abstract void TestPerformSearch();
		public abstract void TestE9_Carrier();
		public abstract void TestE9_Voyage();
		public abstract void TestE9_DataProvider();
		public abstract void TestFilterByImportExport();
		public abstract void TestLloydsNumber_MatchOnVesselName();
		public abstract void TestValidateE9_Port();
		public abstract void TestLoadDischargePorts();
		public abstract void TestPerformSearch_ValidatesLloydsNumbers();
		public abstract void TestVesselName_MatchOnRefVesselVesselName();
		protected abstract string DataProvider { get; }
		protected abstract string[] ExpectedVoyages_TestE9_LineOperator { get; }
		protected abstract void AssertDateFilter(ZString dateFilterType, SchemaDateTimeColumn jobVesselScheduleDateProperty);

		public void TestPerformSearch_SwapsFactoryInVoyageCollection()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Voyage");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(2), "Voyage");
			Factory.Save();

			BusinessObjectFactory oldFactory = FilterBusinessObject.Voyages.Factory;
			FilterBusinessObject.E9_Voyage = "Voyage";
			FilterBusinessObject.PerformSearch();
			AssertNotEquals(
				"Should swap the collection's factory to prevent memory leaks from re-searching from an sql view that returns newid() as the PK",
				FilterBusinessObject.Voyages.Factory != oldFactory);
		}

		#region Overrides

		[TestDate(2000, 1, 1)]
		public void TestSetDefaultValues()
		{
			PopulateFilterBizOWithDummyData();

			FilterBusinessObject.ResetToDefaultValues();
			foreach (ZPropertyInfo property in FilterBusinessObject.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				if (property != FilterBusinessObject.E9_DateTypeInfo &&
					property != FilterBusinessObject.E9_DateFromInfo)
				{
					AssertEquals("Property " + property.Name + " should be reset to an empty value", true, property.Value.IsEmpty);
				}
			}
		}

		void PopulateFilterBizOWithDummyData()
		{
			foreach (ZPropertyInfo property in FilterBusinessObject.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				if (property.PropertyType == typeof(ZString))
				{
					property.Value = (ZString)"x";
				}
				else if (property.PropertyType == typeof(ZDateTime))
				{
					property.Value = ZDateTime.Now;
				}
				else if (property.PropertyType == typeof(ZBool))
				{
					property.Value = ZBool.True;
				}
			}
		}

		public void TestRunPreSaveValidation()
		{
			FilterBusinessObject.E9_DateType = "XXX";
			FilterBusinessObject.E9_DateFrom = ZDateTime.Invalid;
			FilterBusinessObject.E9_DateTo = ZDateTime.Invalid;
			FilterBusinessObject.E9_Port = "XXXXX";

			FilterBusinessObject.ClearAllNotifications();
			AssertNoErrors("No errors initially for E9_DateType", FilterBusinessObject.E9_DateTypeInfo);
			AssertNoErrors("No errors initially for E9_DateFrom", FilterBusinessObject.E9_DateFromInfo);
			AssertNoErrors("No errors initially for E9_DateTo", FilterBusinessObject.E9_DateToInfo);
			AssertNoErrors("No errors initially for E9_Port", FilterBusinessObject.E9_PortInfo);

			FilterBusinessObject.RunPreSaveValidation();
			AssertHasErrors("Has errors after RunPreSaveValidation() for E9_DateType", FilterBusinessObject.E9_DateTypeInfo);
			AssertHasErrors("Has errors after RunPreSaveValidation() for E9_DateFrom", FilterBusinessObject.E9_DateFromInfo);
			AssertHasErrors("Has errors after RunPreSaveValidation() for E9_DateTo", FilterBusinessObject.E9_DateToInfo);
			AssertHasErrors("Has errors after RunPreSaveValidation() for E9_Port", FilterBusinessObject.E9_PortInfo);
		}

		#endregion

		#region Date Filters

		public void TestE9_DateType_Default()
		{
			FilterBusinessObject.ResetToDefaultValues();
			AssertEquals(VesselRoutingVoyagesFilter.Constants.Dates.All, FilterBusinessObject.E9_DateType);
		}

		public void TestValidateE9_DateFrom()
		{
			FilterBusinessObject.E9_DateFrom = ZDateTime.Empty;
			AssertHasErrors("Error when no from date entered", FilterBusinessObject.E9_DateFromInfo);

			FilterBusinessObject.E9_DateFrom = ZDateTime.Now.AddMonths(-6).AddDays(-6);
			AssertNoErrors("No error when from date is after 6 months ago (and 7 days just in case they leave the form open for a long time)", FilterBusinessObject.E9_DateFromInfo);

			FilterBusinessObject.E9_DateFrom = ZDateTime.Now.AddMonths(-6).AddDays(-8);
			AssertHasErrors("Error when from date is before 6 months ago (and 7 days just in case they leave the form open for a long time)", FilterBusinessObject.E9_DateFromInfo);
		}

		public void TestEstimatedDepartureDate()
		{
			AssertDateFilter(VesselRoutingVoyagesFilter.Constants.Dates.EstimatedDeparture, JobVesselScheduleSchema.EV_ETD);
		}

		public void TestEstimatedArrivalDate()
		{
			AssertDateFilter(VesselRoutingVoyagesFilter.Constants.Dates.EstimatedArrival, JobVesselScheduleSchema.EV_ETA);
		}

		public void TestActualDepartureDate()
		{
			AssertDateFilter(VesselRoutingVoyagesFilter.Constants.Dates.ActualDeparture, JobVesselScheduleSchema.EV_ActualDeparture);
		}

		public void TestActualArrivalDate()
		{
			AssertDateFilter(VesselRoutingVoyagesFilter.Constants.Dates.ActualArrival, JobVesselScheduleSchema.EV_ActualArrival);
		}

		#endregion

		#region Vessel Name and Lloyds Number

		public void TestE9_RV_NKVesselNameInfo_MaxLength()
		{
			AssertEquals("E9_RV_NKVesselNameInfo.MaxLength. If this changes, remember to change the filter control layout", 35, FilterBusinessObject.E9_RV_NKVesselNameInfo.MaxLength);
		}

		public void TestLloydsNumber_PopulatedFromVesselName()
		{
			RefVessel refVessel = this.RefVessel;
			FilterBusinessObject.E9_RV_NKVesselName = "VesselName";
			AssertEquals("Lloyds number should be populated", "Lloyds", FilterBusinessObject.E9_LloydsNumber);
		}

		public void TestVesselName_PopulatedFromLloydsNumber()
		{
			RefVessel refVessel = this.RefVessel;
			FilterBusinessObject.E9_LloydsNumber = "Lloyds";
			AssertEquals("Vessel name should be populated", "VesselName", FilterBusinessObject.E9_RV_NKVesselName);
		}

		#endregion

		#region Port Pair Types

		public void TestPortPairTypes_DefaultValue()
		{
			VesselRoutingVoyagesFilter filterBusinessObject = new VesselRoutingVoyagesFilter(Factory);
			AssertEquals(PortPairTypes.All, filterBusinessObject.PortPairTypes);
		}

		#endregion

		#region Carrier

		public void TestE9_CarrierMaxLength()
		{
			AssertEquals(ViewVesselRoutingVoyagesSchema.E8_OperatorsDescription.MaxLength, FilterBusinessObject.E9_CarrierInfo.MaxLength);
		}

		#endregion

		#region Implementation

		protected void AssertFilterMatches(string message, params string[] expectedVoyageMatches)
		{
			VesselRoutingVoyage[] actualMatches = Factory.Load<VesselRoutingVoyage>(FilterBusinessObject.Filter);
			AssertEquals(message + "; Count", expectedVoyageMatches.Length, actualMatches.Length);
			foreach (string voyage in expectedVoyageMatches)
			{
				AssertEquals(message, true, ContainsVoyage(actualMatches, voyage));
			}
		}

		protected void AssertFilterMatches(string message, params ZGuid[] expectedJobVesselScheduleMatches)
		{
			VesselRoutingVoyage[] actualMatches = Factory.Load<VesselRoutingVoyage>(FilterBusinessObject.Filter);
			AssertEquals(message + "; Count", expectedJobVesselScheduleMatches.Length, actualMatches.Length);
			foreach (ZGuid schedulePK in expectedJobVesselScheduleMatches)
			{
				AssertEquals(message, true, ContainsPK(actualMatches, schedulePK));
			}
		}

		bool ContainsVoyage(VesselRoutingVoyage[] voyages, string voyageToCheck)
		{
			foreach (VesselRoutingVoyage voyage in voyages)
			{
				if (voyage.E8_Voyage == voyageToCheck)
				{
					return true;
				}
			}
			return false;
		}

		bool ContainsPK(VesselRoutingVoyage[] voyages, ZGuid schedulePK)
		{
			foreach (VesselRoutingVoyage voyage in voyages)
			{
				if (voyage.E8_EV_PK == schedulePK)
				{
					return true;
				}
			}
			return false;
		}

		protected JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZDateTime eTDandETA, ZString voyage)
		{
			return NewJobVesselSchedule(portCode, eTDandETA, "Lloyds", voyage, "", DataProvider);
		}

		protected JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZDateTime eTDandETA, ZString lloyds, ZString voyage)
		{
			return NewJobVesselSchedule(portCode, eTDandETA, lloyds, voyage, "", DataProvider);
		}

		protected JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZDateTime eTDandETA, ZString lloyds, ZString voyage, ZString dataProvider)
		{
			return NewJobVesselSchedule(portCode, eTDandETA, lloyds, voyage, "", dataProvider);
		}

		protected JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZDateTime eTDandETA, ZString lloyds, ZString voyage, ZString dataProviderReference, ZString dataProvider)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portCode;
			result.EV_ETD = eTDandETA;
			result.EV_ETA = eTDandETA;
			result.EV_IMOLloydsNumber = lloyds;
			result.EV_ShipOperatorVoyageIn = voyage;
			result.EV_ShipOperatorVoyageOut = voyage;
			result.EV_DataProvider = dataProvider;
			result.EV_DataProviderReference = dataProviderReference;
			return result;
		}

		protected JobVesselRouting NewJobVesselRouting(ZString portCode, ZString voyage)
		{
			return NewJobVesselRouting(portCode, "Lloyds", voyage);
		}

		protected JobVesselRouting NewJobVesselRouting(ZString portCode, ZString lloyds, ZString voyage)
		{
			return NewJobVesselRouting(portCode, lloyds, voyage, ZDateTime.Today.AddDays(5), ZGuid.Empty);
		}

		protected JobVesselRouting NewJobVesselRouting(ZString portCode, ZString lloyds, ZString voyage, ZGuid schedulePK)
		{
			return NewJobVesselRouting(portCode, lloyds, voyage, ZDateTime.Today.AddDays(5), schedulePK);
		}

		protected JobVesselRouting NewJobVesselRouting(ZString portCode, ZString lloyds, ZString voyage, ZDateTime eTAandETD, ZGuid schedulePk)
		{
			JobVesselRouting result = Factory.New<JobVesselRouting>();
			result.E1_RL_NKDischargePortCode = portCode;
			result.E1_LloydsID = lloyds;
			result.E1_VoyageNumber = voyage;
			result.E1_EV = schedulePk;
			result.E1_ETA = eTAandETD;
			result.E1_ETD = eTAandETD;
			return result;
		}

		protected RefVessel RefVessel
		{
			get
			{
				if (vessel == null)
				{
					vessel = Factory.New<RefVessel>();
					vessel.RV_Name = "VesselName";
					vessel.RV_LloydsNumber = "Lloyds";
					Factory.Save();
				}
				return vessel;
			}
		}
		RefVessel vessel;

		protected VesselRoutingVoyagesFilter FilterBusinessObject
		{
			get
			{
				if (filterBusinessObject == null)
				{
					filterBusinessObject = new VesselRoutingVoyagesFilter(Factory);
				}
				return filterBusinessObject;
			}
		}
		VesselRoutingVoyagesFilter filterBusinessObject;

		protected override void SetUp()
		{
			FilterBusinessObject.E9_DateType = VesselRoutingVoyagesFilter.Constants.Dates.All;
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new VesselRoutingVoyagesFilter(Factory);
		}

		#endregion
	}
}
