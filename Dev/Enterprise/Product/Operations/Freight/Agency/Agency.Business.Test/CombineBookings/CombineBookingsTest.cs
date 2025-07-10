using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(CombineBookings))]
	internal class CombineBookingsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMasterBooking()
		{
			AssertEquals("Should be the correct booking", MasterBooking.PK, Combine.MasterBooking.PK);
			AssertEquals("Should be readonly", true, Combine.MasterBooking.ReadOnly);
		}

		public void TestOtherBookings()
		{
			AssertEquals("Should not be readonly", false, Combine.OtherBookings.ReadOnly);
			AssertEquals("Should not allow new", false, ((IBindingList)Combine.OtherBookings).AllowNew);
		}

		public void TestCombine_CancelOtherBookings()
		{
			Combine.OtherBookings.Add(OtherBooking1);
			Combine.OtherBookings.Add(OtherBooking2);
			Factory.Save();
			bool factorySaved = false;
			BusinessObjectFactory combineFactory = new BusinessObjectFactory();
			combineFactory.Saving += delegate
			{
				factorySaved = true;
			};
			AssertEquals("precondition: saving should not be called", false, factorySaved);
			Combine.Combine(combineFactory);
			AssertEquals("Combine() should not have saved the factory", false, factorySaved);
			AgencyBooking cMasterBooking = combineFactory.Load<AgencyBooking>(masterBooking.PK);
			AgencyBooking cOtherBooking1 = combineFactory.Load<AgencyBooking>(otherBooking1.PK);
			AgencyBooking cOtherBooking2 = combineFactory.Load<AgencyBooking>(otherBooking2.PK);
			AssertEquals("MasterBooking should not be canceled", false, cMasterBooking.JS_IsCancelled);
			AssertEquals("OtherBooking1 should be canceled", true, cOtherBooking1.JS_IsCancelled);
			AssertEquals("OtherBooking2 should be canceled", true, cOtherBooking2.JS_IsCancelled);
			combineFactory.Save();
		}

		public void TestCombine_CancelOtherBookingsAndWorkflowItems()
		{
			var assignedTask = OtherBooking1.WorkflowItems.Tasks.AddNew();
			assignedTask.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			assignedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var closedTask = OtherBooking1.WorkflowItems.Tasks.AddNew();
			closedTask.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			closedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var incompleteMilestone = OtherBooking1.WorkflowItems.Milestones.AddNew();
			incompleteMilestone.P9_Type = Core.Constants.Workflow.MilestoneType;
			incompleteMilestone.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var completeMilestone = OtherBooking1.WorkflowItems.Milestones.AddNew();
			completeMilestone.P9_Type = Core.Constants.Workflow.MilestoneType;
			completeMilestone.SetMilestoneActualDateForTest(ZDateTime.Today);
			Combine.OtherBookings.Add(OtherBooking1);
			Factory.Save();
			bool factorySaved = false;
			var combineFactory = new BusinessObjectFactory();
			combineFactory.Saving += delegate
			{
				factorySaved = true;
			};
			AssertEquals(false, factorySaved);
			Combine.Combine(combineFactory);
			AssertEquals(false, factorySaved);
			var cMasterBooking = combineFactory.Load<AgencyBooking>(masterBooking.PK);
			var cOtherBooking1 = combineFactory.Load<AgencyBooking>(otherBooking1.PK);
			AssertEquals(true, cMasterBooking.HasChanges);
			combineFactory.Save();
			AssertEquals(false, cMasterBooking.JS_IsCancelled);
			AssertEquals(true, cOtherBooking1.JS_IsCancelled);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, assignedTask.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, closedTask.P9_Status);
			AssertEquals(true, incompleteMilestone.IsDeleted);
			AssertEquals(ProcessTask.LastCompletedStatusCode, completeMilestone.P9_Status);
		}

		public void TestCombine_ContainersAndPackLines()
		{
			RefContainer rc20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer rc20RE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			RefContainer rc40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			RefContainer rc40RE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE");
			Combine.OtherBookings.Add(OtherBooking1);
			Combine.OtherBookings.Add(OtherBooking2);
			AgencyBookingContainer container1 = MasterBooking.BookedContainers.AddNew();
			container1.JC_RC = rc20GP.PK;
			AgencyBookingContainer container2 = MasterBooking.BookedContainers.AddNew();
			container2.JC_RC = rc20RE.PK;
			AgencyBookingPackLine packline1 = MasterBooking.OuterPackLines.AddNew();
			packline1.JL_JC = container1.PK;
			packline1.JL_Description = "packline1";
			AgencyBookingPackLine packline2 = MasterBooking.OuterPackLines.AddNew();
			packline2.JL_Description = "packline2";
			AgencyBookingContainer container3 = OtherBooking1.BookedContainers.AddNew();
			container3.JC_RC = rc40GP.PK;
			AgencyBookingPackLine packline3 = OtherBooking1.OuterPackLines.AddNew();
			packline3.JL_JC = container3.PK;
			packline3.JL_Description = "packline3";
			AgencyBookingContainer container4 = OtherBooking2.BookedContainers.AddNew();
			container4.JC_RC = rc40RE.PK;
			AgencyBookingPackLine packline4 = OtherBooking2.OuterPackLines.AddNew();
			packline4.JL_Description = "packline4";
			Factory.Save();
			bool factorySaved = false;
			BusinessObjectFactory combineFactory = new BusinessObjectFactory();
			combineFactory.Saving += delegate
			{
				factorySaved = true;
			};
			AssertEquals("precondition: saving should not be called", false, factorySaved);
			Combine.Combine(combineFactory);
			AssertEquals("Combine() should not have saved the factory", false, factorySaved);
			AgencyBooking cMasterBooking = combineFactory.Load<AgencyBooking>(masterBooking.PK);
			AgencyBooking cOtherBooking1 = combineFactory.Load<AgencyBooking>(otherBooking1.PK);
			AgencyBooking cOtherBooking2 = combineFactory.Load<AgencyBooking>(otherBooking2.PK);
			AssertContainsExactElementsInAnyOrder("MasterBooking should have all the containers", BusinessObjectEqualityComparer<RefContainer>.IgnoreFactoryComparer, (c) => c.RC_Code, new RefContainer[] { rc20GP, rc20RE, rc40GP, rc40RE }, Array.ConvertAll(cMasterBooking.BookedContainers.ToArray<AgencyBookingContainer>(), (c) => c.Container));
			AssertContainsExactElementsInAnyOrder("MasterBooking should have all the packlines", new string[] { "packline1", "packline2", "packline3", "packline4" }, Array.ConvertAll(cMasterBooking.OuterPackLines.ToArray<AgencyBookingPackLine>(), (p) => p.JL_Description.ToString()));
			AssertContainsExactElementsInAnyOrder("OtherBooking1 should keep it's containers", BusinessObjectEqualityComparer<AgencyBookingContainer>.IgnoreFactoryComparer, (c) => c.Container.RC_Code, new AgencyBookingContainer[] { container3 }, cOtherBooking1.BookedContainers.ToArray<AgencyBookingContainer>());
			AssertContainsExactElementsInAnyOrder("OtherBooking1 should keep it's packlines", BusinessObjectEqualityComparer<AgencyBookingPackLine>.IgnoreFactoryComparer, (p) => p.JL_Description, new AgencyBookingPackLine[] { packline3 }, cOtherBooking1.OuterPackLines.ToArray<AgencyBookingPackLine>());
			AssertContainsExactElementsInAnyOrder("OtherBooking2 should keep it's containers", BusinessObjectEqualityComparer<AgencyBookingContainer>.IgnoreFactoryComparer, (c) => c.Container.RC_Code, new AgencyBookingContainer[] { container4 }, cOtherBooking2.BookedContainers.ToArray<AgencyBookingContainer>());
			AssertContainsExactElementsInAnyOrder("OrderBooking2 should keep it's packlines", BusinessObjectEqualityComparer<AgencyBookingPackLine>.IgnoreFactoryComparer, (p) => p.JL_Description, new AgencyBookingPackLine[] { packline4 }, cOtherBooking2.OuterPackLines.ToArray<AgencyBookingPackLine>());
			AgencyBookingPackLine cPackline1 = FindPackline(cMasterBooking.OuterPackLines, "packline1");
			AgencyBookingPackLine cPackline2 = FindPackline(cMasterBooking.OuterPackLines, "packline2");
			AgencyBookingPackLine cPackline3 = FindPackline(cMasterBooking.OuterPackLines, "packline3");
			AgencyBookingPackLine cPackline4 = FindPackline(cMasterBooking.OuterPackLines, "packline4");
			AgencyBookingContainer cContainer1 = FindContainer(cMasterBooking.BookedContainers, rc20GP.PK);
			AgencyBookingContainer cContainer3 = FindContainer(cMasterBooking.BookedContainers, rc40GP.PK);
			AssertEquals("packline1.JL_JC", cPackline1.JL_JC, cContainer1.PK);
			AssertEquals("packline2.JL_JC", cPackline2.JL_JC, ZGuid.Empty);
			AssertEquals("packline3.JL_JC", cPackline3.JL_JC, cContainer3.PK);
			AssertEquals("packline4.JL_JC", cPackline4.JL_JC, ZGuid.Empty);
			combineFactory.Save();
		}

		public void TestCombine_Notes()
		{
			Combine.OtherBookings.Add(OtherBooking1);
			Combine.OtherBookings.Add(OtherBooking2);
			StmNote note1 = MasterBooking.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "note1");
			StmNote note2 = MasterBooking.Notes.AddNew(false, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "note2");
			StmNote note3 = OtherBooking1.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "note3");
			StmNote note4 = OtherBooking2.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "note4");
			Factory.Save();
			bool factorySaved = false;
			BusinessObjectFactory combineFactory = new BusinessObjectFactory();
			combineFactory.Saving += delegate
			{
				factorySaved = true;
			};
			AssertEquals("precondition: saving should not be called", false, factorySaved);
			Combine.Combine(combineFactory);
			AssertEquals("Combine() should not have saved the factory", false, factorySaved);
			AgencyBooking cMasterBooking = combineFactory.Load<AgencyBooking>(masterBooking.PK);
			AgencyBooking cOtherBooking1 = combineFactory.Load<AgencyBooking>(otherBooking1.PK);
			AgencyBooking cOtherBooking2 = combineFactory.Load<AgencyBooking>(otherBooking2.PK);
			AssertContainsExactElementsInAnyOrder("MasterBooking should have a note of each type.", new string[] { PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description + " - note1", PredefinedNoteTypes.Instance.MarksAndNumbers.Description + " - note2", PredefinedNoteTypes.Instance.HandlingInstructions.Description + " - note4", }, Array.ConvertAll(cMasterBooking.Notes.GetAllNotes().ToArray<StmNote>(), (s) => s.ST_Description + " - " + s.ST_NoteText));
			AssertContainsExactElementsInAnyOrder("OtherBooking1 should keep it's notes", BusinessObjectEqualityComparer<StmNote>.IgnoreFactoryComparer, (s) => s.ST_Description + " - " + s.ST_NoteText, new StmNote[] { note3 }, cOtherBooking1.Notes.GetAllNotes().ToArray<StmNote>());
			AssertContainsExactElementsInAnyOrder("OtherBooking2 should keep it's notes", BusinessObjectEqualityComparer<StmNote>.IgnoreFactoryComparer, (s) => s.ST_Description + " - " + s.ST_NoteText, new StmNote[] { note4 }, cOtherBooking2.Notes.GetAllNotes().ToArray<StmNote>());
			combineFactory.Save();
		}

		public void TestCombine_TotalWeightAndVolume()
		{
			Combine.OtherBookings.Add(OtherBooking1);
			Combine.OtherBookings.Add(OtherBooking2);
			MasterBooking.JS_ActualWeight = 18000;
			MasterBooking.JS_UnitOfWeight = Constants.Weight.Kilograms;
			MasterBooking.JS_ActualVolume = 12;
			MasterBooking.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			OtherBooking1.JS_ActualWeight = 20;
			OtherBooking1.JS_UnitOfWeight = Constants.Weight.Tonnes;
			OtherBooking1.JS_ActualVolume = 10;
			OtherBooking1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			OtherBooking2.JS_ActualWeight = 25000;
			OtherBooking2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			OtherBooking2.JS_ActualVolume = 0.015;
			OtherBooking2.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			Factory.Save();
			bool factorySaved = false;
			BusinessObjectFactory combineFactory = new BusinessObjectFactory();
			combineFactory.Saving += delegate
			{
				factorySaved = true;
			};
			AssertEquals("precondition: saving should not be called", false, factorySaved);
			Combine.Combine(combineFactory);
			AssertEquals("Combine() should not have saved the factory", false, factorySaved);
			AgencyBooking cMasterBooking = combineFactory.Load<AgencyBooking>(masterBooking.PK);
			AssertEquals("MasterBooking Weight", 63000m, cMasterBooking.JS_ActualWeight);
			AssertEquals("MasterBooking Weight Unit", Constants.Weight.Kilograms, cMasterBooking.JS_UnitOfWeight);
			AssertEquals("MasterBooking Volume", 37m, cMasterBooking.JS_ActualVolume);
			AssertEquals("MasterBooking Volume Unit", Constants.Volume.CubicMetres, cMasterBooking.JS_UnitOfVolume);
			combineFactory.Save();
		}

		public void TestCombine_RealContainers()
		{
			RefContainer rc20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer rc40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			RefContainer rc40GP1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			Combine.OtherBookings.Add(OtherBooking1);
			Combine.OtherBookings.Add(OtherBooking2);
			AgencyBookingContainer container1 = MasterBooking.BookedContainers.AddNew();
			container1.JC_RC = rc20GP.PK;
			AgencyBookingContainer container2 = OtherBooking1.BookedContainers.AddNew();
			container2.JC_RC = rc40GP.PK;
			AgencyBookingContainer container3 = OtherBooking1.BookedContainers.AddNew();
			container3.JC_RC = rc40GP1.PK;
			AgencyBookingContainer realContainer1 = MasterBooking.RealContainers.AddNew();
			realContainer1.JC_RC = rc20GP.PK;
			realContainer1.JC_ContainerNum = "AAAA";
			AgencyBookingContainer realContainer2 = OtherBooking1.RealContainers.AddNew();
			realContainer2.JC_RC = rc40GP.PK;
			realContainer2.JC_ContainerNum = "BBBB";
			AgencyBookingContainer realContainer3 = OtherBooking1.RealContainers.AddNew();
			realContainer3.JC_RC = rc40GP1.PK;
			realContainer3.JC_ContainerNum = "CCCC";
			Factory.Save();
			bool factorySaved = false;
			BusinessObjectFactory combineFactory = new BusinessObjectFactory();
			combineFactory.Saving += delegate
			{
				factorySaved = true;
			};
			AssertEquals("precondition: saving should not be called", false, factorySaved);
			Combine.Combine(combineFactory);
			AssertEquals("Combine() should not have saved the factory", false, factorySaved);
			AgencyBooking cMasterBooking = combineFactory.Load<AgencyBooking>(masterBooking.PK);
			AgencyBooking cOtherBooking1 = combineFactory.Load<AgencyBooking>(otherBooking1.PK);
			AssertContainsExactElementsInAnyOrder("MasterBooking should have all the booked containers", new[] { "|20GP", "|40GP", "|40GP" }, FormatContainers(cMasterBooking.BookedContainers.ToArray<AgencyBookingContainer>()));
			AssertContainsExactElementsInAnyOrder("MasterBooking should have all the real containers", new[] { "AAAA|20GP", "BBBB|40GP", "CCCC|40GP" }, FormatContainers(cMasterBooking.RealContainers.ToArray<AgencyBookingContainer>()));
			AssertContainsExactElementsInAnyOrder("OtherBooking1 should keep it's containers", new[] { "BBBB|40GP", "CCCC|40GP" }, FormatContainers(cOtherBooking1.RealContainers.ToArray<AgencyBookingContainer>()));
			AssertNoExceptionThrown("Saved master booking and other bookings", combineFactory.Save);
		}

		string[] FormatContainers(IEnumerable<AgencyBookingContainer> containers)
		{
			return containers.Select(c => string.Format("{0}|{1}", c.JC_ContainerNum, c.Container.RC_Code)).ToArray();
		}

		#region Implementation
		static AgencyBookingPackLine FindPackline(IBusinessObjectCollection collection, string description)
		{
			BusinessObject[] result = collection.Find(new ZQuery(JobPackLinesSchema.JL_Description, description));
			return (AgencyBookingPackLine)(result.Length > 0 ? result[0] : null);
		}

		static AgencyBookingContainer FindContainer(IBusinessObjectCollection collection, ZGuid containerTypePK)
		{
			BusinessObject[] result = collection.Find(new ZQuery(JobContainerSchema.JC_RC, containerTypePK));
			return (AgencyBookingContainer)(result.Length > 0 ? result[0] : null);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CombineBookings(MasterBooking);
		}

		CombineBookings Combine
		{
			get
			{
				return combine ?? (combine = new CombineBookings(MasterBooking));
			}
		}

		CombineBookings combine;
		AgencyBooking MasterBooking
		{
			get
			{
				return masterBooking ?? (masterBooking = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking masterBooking;
		AgencyBooking OtherBooking1
		{
			get
			{
				return otherBooking1 ?? (otherBooking1 = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking otherBooking1;
		AgencyBooking OtherBooking2
		{
			get
			{
				return otherBooking2 ?? (otherBooking2 = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking otherBooking2;
		#endregion
	}
}
