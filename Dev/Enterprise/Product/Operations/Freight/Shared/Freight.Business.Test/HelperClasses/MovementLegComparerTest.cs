using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MovementLegComparerTest : BaseFreightTest
	{
		public void TestFirstLastLegsForTransportMode()
		{
			var legs = new[]
						{
							NewLeg("AUBNE", "SGSIN", new ZDateTime(2008, 5, 4), ZDateTime.Empty, "AIR"),
							NewLeg("CNSHA", "NLAMS", ZDateTime.Empty, ZDateTime.Empty, "SEA"),
							NewLeg("NLAMS", "GBLON", ZDateTime.Empty, new ZDateTime(2008, 6, 5), "SEA"),
							NewLeg("NZAKL", "AUBNE", ZDateTime.Empty, ZDateTime.Empty, "RAI"),
							NewLeg("SGSIN", "HKHKG", ZDateTime.Empty, ZDateTime.Empty, "ROA"),
						};

			AssertEquals("AUBNE", MovementLegComparer.FirstOrDefaultLegForTransportMode(legs, "AIR").Load);
			AssertEquals("CNSHA", MovementLegComparer.FirstOrDefaultLegForTransportMode(legs, "SEA").Load);
			AssertEquals("NZAKL", MovementLegComparer.FirstOrDefaultLegForTransportMode(legs, "RAI").Load);
			AssertEquals("SGSIN", MovementLegComparer.FirstOrDefaultLegForTransportMode(legs, "ROA").Load);
			AssertEquals("NZAKL", MovementLegComparer.FirstOrDefaultLegForTransportMode(legs, "COU").Load);

			AssertEquals("AUBNE", MovementLegComparer.LastOrDefaultLegForTransportMode(legs, "AIR").Load);
			AssertEquals("NLAMS", MovementLegComparer.LastOrDefaultLegForTransportMode(legs, "SEA").Load);
			AssertEquals("NZAKL", MovementLegComparer.LastOrDefaultLegForTransportMode(legs, "RAI").Load);
			AssertEquals("SGSIN", MovementLegComparer.LastOrDefaultLegForTransportMode(legs, "ROA").Load);
			AssertEquals("NLAMS", MovementLegComparer.LastOrDefaultLegForTransportMode(legs, "COU").Load);
		}

		public void TestSortMovementLegsByPorts()
		{
			IMovementLeg[] legs = new IMovementLeg[]
			{
				NewLeg("NLAMS", "NLAMS", new ZDateTime(2008, 6, 2), new ZDateTime(2008, 6, 3)),
				NewLeg("AUBNE", "AUBNE", new ZDateTime(2008, 5, 1), ZDateTime.Empty),
				NewLeg("SGSIN", "SGSIN", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("AUBNE", "AUBNE", new ZDateTime(2008, 5, 4), ZDateTime.Empty),
				NewLeg("NLAMS", "NLAMS", ZDateTime.Empty, new ZDateTime(2008, 6, 4)),
				NewLeg("SGSIN", "SGSIN", ZDateTime.Invalid, ZDateTime.Empty),
				NewLeg("NLAMS", "NLAMS", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("NLAMS", "NLAMS", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("AUBNE", "SGSIN", new ZDateTime(2008, 5, 4), ZDateTime.Empty),
				NewLeg("CNSHA", "NLAMS", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("NLAMS", "GBLON", ZDateTime.Empty, new ZDateTime(2008, 6, 5)),
				NewLeg("NZAKL", "AUBNE", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("SGSIN", "HKHKG", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("SGSIN", "SGSIN", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("AUBNE", "AUBNE", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("SGSIN", "SGSIN", ZDateTime.Empty, new ZDateTime(2007, 1, 1)),
				NewLeg("AUBNE", "AUBNE", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("NLAMS", "NLAMS", new ZDateTime(2008, 6, 1), ZDateTime.Empty),

				NewLeg("AUBNE", "", new ZDateTime(2008, 4, 30), ZDateTime.Empty),
				NewLeg("SGSIN", "", new ZDateTime(2008, 5, 5), ZDateTime.Empty),
				NewLeg("", "AUBNE", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("", "AUBNE", new ZDateTime(2008, 5, 2), ZDateTime.Empty),
				NewLeg("", "SGSIN", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("AUBNE", "", ZDateTime.Empty, ZDateTime.Empty),
			};

			MovementLegComparer.SortMovementLegsByPorts(legs);

			string expectedValue = @"NZAKL() -> AUBNE()
AUBNE(2008-04-30) -> ()
AUBNE(2008-05-01) -> AUBNE()
(2008-05-02) -> AUBNE()
AUBNE(2008-05-04) -> AUBNE()
AUBNE() -> AUBNE()
AUBNE() -> AUBNE()
() -> AUBNE()
AUBNE() -> ()
AUBNE(2008-05-04) -> SGSIN()
SGSIN() -> SGSIN(2007-01-01)
SGSIN(2008-05-05) -> ()
SGSIN() -> SGSIN()
SGSIN() -> SGSIN()
SGSIN() -> SGSIN()
() -> SGSIN()
SGSIN() -> HKHKG()
CNSHA() -> NLAMS()
NLAMS(2008-06-01) -> NLAMS()
NLAMS(2008-06-02) -> NLAMS(2008-06-03)
NLAMS() -> NLAMS(2008-06-04)
NLAMS() -> NLAMS()
NLAMS() -> NLAMS()
NLAMS() -> GBLON(2008-06-05)";

			StringBuilder builder = new StringBuilder();
			foreach (IMovementLeg leg in legs)
			{
				builder.AppendLine(FormatLeg(leg));
			}

			AssertMultilineASCIIEquals("", expectedValue, builder.ToString());
		}

		[ExpectNoExceptions]
		public void TestSortMovementLegsByPorts_Loop()
		{
			IMovementLeg[] legs = new IMovementLeg[]
			{
				NewLeg("NZAKL", "AUBNE", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("AUBNE", "AUSIN", ZDateTime.Empty, ZDateTime.Empty),
				NewLeg("AUSIN", "NZAKL", ZDateTime.Empty, ZDateTime.Empty),
			};

			MovementLegComparer.SortMovementLegsByPorts(legs);
		}

		public void TestDatesBasedComparer()
		{
			ZDateTime today = ZDateTime.Today;

			IMovementLeg leg1 = NewLeg("", "", ZDateTime.Empty, today.AddDays(5));
			IMovementLeg leg2 = NewLeg("", "", today.AddDays(6), ZDateTime.Empty);
			IMovementLeg leg3 = NewLeg("", "", today.AddDays(11), today.AddDays(15));
			IMovementLeg leg4 = NewLeg("", "", ZDateTime.Empty, ZDateTime.Empty);
			IMovementLeg leg5 = NewLeg("", "", ZDateTime.Empty, ZDateTime.Empty);

			AssertOrder(MovementLegComparer.DatesBased, leg1, leg2);
			AssertOrder(MovementLegComparer.DatesBased, leg2, leg3);
			AssertOrder(MovementLegComparer.DatesBased, leg1, leg3);
			AssertOrder(MovementLegComparer.DatesBased, leg1, leg4);
			AssertNoOrder(MovementLegComparer.DatesBased, leg4, leg5);
			AssertNoOrder(MovementLegComparer.DatesBased, leg3, leg3);
		}

		public void TestDatesBasedComparer_WithOverlappingDates()
		{
			IMovementLeg leg1 = NewLeg("", "", new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 4));
			IMovementLeg leg2 = NewLeg("", "", new ZDateTime(2004, 1, 2), new ZDateTime(2004, 1, 5));

			AssertOrder(MovementLegComparer.DatesBased, leg1, leg2);

			IMovementLeg leg3 = NewLeg("", "", new ZDateTime(2004, 1, 2), new ZDateTime(2004, 1, 4));
			IMovementLeg leg4 = NewLeg("", "", new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 5));

			AssertNoOrder(MovementLegComparer.DatesBased, leg3, leg4);
		}

		public void TestPortsAndDatesBasedComparer()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport leg3 = consol.Transports[0];
			Transport leg2 = consol.Transports.AddNew();
			Transport leg1 = consol.Transports.AddNew();

			leg1.JW_RL_NKLoadPort = "AUBNE";
			leg1.JW_RL_NKDiscPort = "SGSIN";

			leg2.JW_RL_NKLoadPort = "SGSIN";
			leg2.JW_RL_NKDiscPort = "MYBAG";

			leg3.JW_RL_NKLoadPort = "MYBAG";
			leg3.JW_RL_NKDiscPort = "NLAMS";

			MovementLegComparer comparer = MovementLegComparer.PortsAndDatesBased(consol.Transports);

			AssertOrder(comparer, leg1, leg2);
			AssertOrder(comparer, leg1, leg3);
			AssertOrder(comparer, leg2, leg3);
			AssertNoOrder(comparer, leg2, leg2);

			Transport leg4 = consol.Transports.AddNew();
			leg4.JW_RL_NKLoadPort = "NLAMS";

			AssertOrder(comparer, leg2, leg4);

			leg4.JW_RL_NKLoadPort = "";
			leg4.JW_RL_NKDiscPort = "AUBNE";
			AssertOrder(comparer, leg4, leg2);

			Transport leg5 = consol.Transports.AddNew();
			leg5.JW_RL_NKLoadPort = "SGSIN";
			leg5.JW_RL_NKDiscPort = "SGSIN";

			AssertOrder(comparer, leg1, leg5);
			AssertOrder(comparer, leg5, leg2);

			const string expectedExceptionMessage =
				"attempting to compare an unknown element\r\n" +
				"\r\n" +
				"SGSIN()->MYBAG()\r\n" +
				"()->()\r\n" +
				"\r\n" +
				"MYBAG()->NLAMS()\r\n" +
				"SGSIN()->MYBAG()\r\n" +
				"AUBNE()->SGSIN()\r\n" +
				"()->AUBNE()\r\n" +
				"SGSIN()->SGSIN()\r\n" +
				"";

			Transport unrelatedLeg = Factory.New<CommonConsol>().Transports[0];
			AssertExceptionThrown(typeof(InvalidOperationException), expectedExceptionMessage, delegate
			{ comparer.Compare(leg2, unrelatedLeg); });
		}

		[ExpectNoExceptions]
		public void TestComparer()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "CNSZX";
			transport1.JW_RL_NKDiscPort = "USLGB";
			transport1.JW_ETD = new ZDateTime(2009, 6, 12);
			transport1.JW_ETA = new ZDateTime(2009, 6, 24);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USLGB";
			transport2.JW_RL_NKDiscPort = "USCHI";
			transport2.JW_ETD = ZDateTime.Empty;
			transport2.JW_ETA = ZDateTime.Empty;

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "";
			transport3.JW_RL_NKDiscPort = "USGEN";
			transport3.JW_ETD = ZDateTime.Empty;
			transport3.JW_ETA = ZDateTime.Empty;

			MovementLegComparer comparer = MovementLegComparer.PortsAndDatesBased(consol.Transports);
			consol.Transports.Sort(comparer);
		}

		#region Implementation

		void AssertOrder(IComparer<IMovementLeg> comparer, IMovementLeg leg1, IMovementLeg leg2)
		{
			AssertEquals(FormatLegs(leg1, leg2) + "comparer.Compare(leg1, leg2)<0", true, comparer.Compare(leg1, leg2) < 0);
			AssertEquals(FormatLegs(leg2, leg1) + "comparer.Compare(leg1, leg2)>0", true, comparer.Compare(leg2, leg1) > 0);
		}

		void AssertNoOrder(IComparer<IMovementLeg> comparer, IMovementLeg leg1, IMovementLeg leg2)
		{
			AssertEquals(FormatLegs(leg1, leg2), 0, comparer.Compare(leg1, leg2));
			AssertEquals(FormatLegs(leg1, leg2), 0, comparer.Compare(leg2, leg1));
		}

		string FormatLeg(IMovementLeg leg)
		{
			return string.Format("{0}({1:yyyy-MM-dd}) -> {2}({3:yyyy-MM-dd})",
				leg.Load, leg.DepartureDate,
				leg.Discharge, leg.ArrivalDate
				);
		}
		string FormatLegs(IMovementLeg leg1, IMovementLeg leg2)
		{
			return string.Format("{0}({1:yyyy-MM-dd}) -> {2}({3:yyyy-MM-dd}):{4}({5:yyyy-MM-dd}) -> {6}({7:yyyy-MM-dd})",
				leg1.Load, leg1.DepartureDate,
				leg1.Discharge, leg1.ArrivalDate,
				leg2.Load, leg2.DepartureDate,
				leg2.Discharge, leg2.ArrivalDate
				);
		}

		IMovementLeg NewLeg(ZString load, ZString discharge, ZDateTime atd, ZDateTime ata, string transportMode = "")
		{
			var mock = new Mock<IMovementLeg>();
			mock.Setup(m => m.Load).Returns(load);
			mock.Setup(m => m.Discharge).Returns(discharge);
			mock.Setup(m => m.ArrivalDate).Returns(ata);
			mock.Setup(m => m.DepartureDate).Returns(atd);
			mock.Setup(m => m.TransportMode).Returns((ZString)transportMode);

			return mock.Object;
		}

		#endregion
	}
}
