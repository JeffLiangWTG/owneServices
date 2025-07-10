using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentStmNote))]
	sealed class ForwardingShipmentStmNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestStmNoteWithEmptyTextIsDeletedWhenSaving()
		{
			var detailedGoodsDescriptionNote1 = CreateStmNote(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, string.Empty);
			var marksAndNumbersNote1 = CreateStmNote(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, string.Empty);
			var invoiceDetailsNote1 = CreateStmNote(PredefinedNoteTypes.Instance.InvoiceDetails.Description, string.Empty);

			var detailedGoodsDescriptionNote2 = CreateStmNote(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Detailed Goods Description Note Text");
			var marksAndNumbersNote2 = CreateStmNote(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Marks And Numbers Note Text");
			var invoiceDetailsNote2 = CreateStmNote(PredefinedNoteTypes.Instance.InvoiceDetails.Description, "Invoice Details Note Text");

			Factory.Save();

			Assert(detailedGoodsDescriptionNote1.IsDeleted);
			Assert(marksAndNumbersNote1.IsDeleted);
			Assert(!invoiceDetailsNote1.IsDeleted);

			Assert(!detailedGoodsDescriptionNote2.IsDeleted);
			Assert(!marksAndNumbersNote2.IsDeleted);
			Assert(!invoiceDetailsNote2.IsDeleted);
		}

		public void TestValidation()
		{
			var note = Factory.New<ForwardingShipmentStmNote>();
			note.ST_Table = Shipment.TableName;
			note.ST_ParentID = Shipment.PK;

			AssertEquals(typeof(ForwardingShipmentStmNoteValidation), note.Validation.GetType());
		}

		public void TestST_NoteText_ReadOnly_ShouldBeChangedDueToPhase_WhenItisGoodDescription()
		{
			var security = CreatePhaseSecurity();
			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, security))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_Phase = "PH1";

				shipment.DetailedGoodsDescriptionNoteText = "Sample";
				var goodDescriptionNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description)[0] as ForwardingShipmentStmNote;
				goodDescriptionNote.ReadOnly = false;
				Assert(goodDescriptionNote.ST_NoteTextInfo.ReadOnly);

				shipment.JS_MarksAndNumbers = "Marks and numbers";
				var marksNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description)[0] as ForwardingShipmentStmNote;
				marksNote.ReadOnly = false;
				Assert(marksNote.ST_NoteTextInfo.ReadOnly);

				shipment.JS_Phase = PhaseConstants.Phase.ALL;

				Assert(!goodDescriptionNote.ST_NoteTextInfo.ReadOnly);
				Assert(!marksNote.ST_NoteTextInfo.ReadOnly);
			}
		}

		public void TestConstructor_WhenDescriptionIsOrderHistoryUpdateCode_ThenReportError()
		{
			Factory.NameForDebugging = "StubFactory";

			var note = Factory.New<ForwardingShipmentStmNoteForTest>();

			Assert(ErrorReporter.LastKeyReported.Equals("ForwardingShipmentStmNote_ST_Description_Is_OrderUpdateHistoryCode"));
			Assert(ErrorReporter.LastMessageReported.Contains("ForwardingShipmentStmNote constructed from an OrderUpdateHistoryStmNote's DataRow."));
			Assert(ErrorReporter.LastMessageReported.Contains("Factory Name: StubFactory"));
			Assert(ErrorReporter.LastMessageReported.Contains($"Row Data: {note.RowData}"));

			ErrorReporter.Clear();
		}

		public void TestOriginalBillNotesReadOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var note = shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			note.ST_NoteText = "test";

			Factory.Save();

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.OriginalBillNotes.Description).FirstOrDefault();
				var forwardingShipmentStmNoteForTest = new ForwardingShipmentStmNoteForBillNotesTest(note.Factory, ((IBusinessObjectInternals)note).Row);
				AssertEquals(true, forwardingShipmentStmNoteForTest.ST_NoteTextInfo.ReadOnly);
			}
		}

		class ForwardingShipmentStmNoteForTest : ForwardingShipmentStmNote
		{
			public ForwardingShipmentStmNoteForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				RowData = string.Join(";", row.ItemArray);
			}

			public string RowData;

			protected override void SetUpForTest()
			{
				base.SetUpForTest();
				ST_Description = PredefinedNoteTypes.Instance.OrderUpdateHistory.Code;
			}
		}

		class ForwardingShipmentStmNoteForBillNotesTest : ForwardingShipmentStmNote
		{
			public ForwardingShipmentStmNoteForBillNotesTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new bool ST_NoteText_ReadOnly
			{
				get
				{
					return base.ST_NoteText_ReadOnly;
				}
			}
		}

		#region Implementation

		IPhaseSecurity CreatePhaseSecurity()
		{
			var security = new PhaseSecurity();
			security.IsEnabled = true;
			security.RuleLocations.AddPair(PhaseConstants.Locations.Shipment.LoadPort, PhaseConstants.Locations.Shipment.LoadPort);

			var phase = security.Phases.AddNew();
			phase.Code = "PH1";
			phase.Description = (NoResString)"Hello";

			var rule = phase.Rules.AddNew();
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule.Location = PhaseConstants.Locations.Shipment.LoadPort;

			rule.Dependants.Add(new PhaseDependant()
			{
				Name = Enterprise.Freight.Business.CommonShipment.Schema.DetailedGoodsDescriptionNoteText,
				IsReadOnly = true
			});

			rule.Dependants.Add(new PhaseDependant()
			{
				Name = Enterprise.Freight.Business.CommonShipment.Schema.JS_MarksAndNumbers,
				IsReadOnly = true
			});

			return security;
		}

		ForwardingShipmentStmNote CreateStmNote(string description, string noteText)
		{
			var note = Factory.NewWithValidTestData<ForwardingShipmentStmNote>();
			note.ST_Table = Shipment.TableName;
			note.ST_ParentID = Shipment.PK;
			note.ST_Description = description;
			note.ST_NoteText = noteText;

			return note;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var note = Factory.NewWithValidTestData<ForwardingShipmentStmNote>();
			note.ST_Table = Shipment.TableName;
			note.ST_ParentID = Shipment.PK;

			return note;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var note = factory.NewWithValidTestData<ForwardingShipmentStmNote>();
			note.ST_Table = Shipment.TableName;
			note.ST_ParentID = Shipment.PK;

			return note;
		}

		ForwardingShipment Shipment => shipment ?? (shipment = Factory.New<ForwardingShipment>());
		ForwardingShipment shipment;

		#endregion
	}
}
