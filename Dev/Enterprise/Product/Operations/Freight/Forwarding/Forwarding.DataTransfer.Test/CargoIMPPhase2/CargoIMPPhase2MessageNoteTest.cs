using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	internal class CargoIMPPhase2MessageNoteTest : TestCaseWithFactory
	{
		public void TestDocumentNoteRetrieveNoteIsNull()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			note.WriteToNote("asdasd");

			AssertNull("DocumentNote.RetrieveNote(shipment)", DocumentNote.RetrieveNote(shipment));
		}

		public void TestDescription()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			ZQuery query = new ZQuery(StmNoteSchema.ST_ParentID, shipment.PK);
			query.AddToFilter(StmNoteSchema.ST_Description, "CargoIMPPhase2MessageNote");
			query.AddToFilter(StmNoteSchema.ST_Table, shipment.TableName);
			AssertEquals(0, Factory.Load<StmNote>(query).Length);

			note.WriteToNote("asdasd");
			AssertEquals(1, Factory.Load<StmNote>(query).Length);
		}

		public void TestIsEmpty()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageNoteForTest note = new CargoIMPPhase2MessageNoteForTest(shipment);
			Assert(!note.HasNote);
			Assert(note.IsEmptyForTest);

			note.WriteToNote("");
			Assert(note.HasNote);
			Assert("No content", note.IsEmptyForTest);

			note.WriteToNote("asdasdfasd");
			Assert(!note.IsEmptyForTest);
		}

		public void TestWriteAndLoad()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			note.WriteToNote("");

			ZString str = note.LoadFromNote();
			AssertEquals("", str);

			note.WriteToNote("asdasda");
			str = note.LoadFromNote();
			AssertEquals("asdasda", str);
		}

		class CargoIMPPhase2MessageNoteForTest : CargoIMPPhase2MessageNote
		{
			public CargoIMPPhase2MessageNoteForTest(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			public bool IsEmptyForTest
			{
				get { return IsEmpty; }
			}
		}
	}
}
