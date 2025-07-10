using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContainerISOType))]
	sealed class ContainerISOTypeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultContainerSizing()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ISOType = "44NJ";
			ContainerISOType.DefaultSizing(refContainer);
			AssertEquals(40m, refContainer.RC_Length);
			AssertEquals(9m, refContainer.RC_Height);
			AssertEquals(8m, refContainer.RC_Width);
			refContainer.RC_ISOType = "45G9";
			ContainerISOType.DefaultSizing(refContainer);
			AssertEquals(40m, refContainer.RC_Length);
			AssertEquals(9.5m, refContainer.RC_Height);
			AssertEquals(8m, refContainer.RC_Width);
			refContainer.RC_ISOType = "EDN7";
			ContainerISOType.DefaultSizing(refContainer);
			AssertEquals(7.820m, Utilities.Round(refContainer.LengthMetres, 3));
			AssertEquals(9m, refContainer.RC_Height);
			AssertEquals(0m, refContainer.RC_Width);
		}

		#region Other

		public void TestEmpty()
		{
			ISOType.ISOCode = "";
			AssertEquals("", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(false, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		public void TestUnknownISOCode()
		{
			ISOType.ISOCode = "9999";
			AssertEquals("", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(false, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);

			ISOType.ISOCode = "42XX";
			AssertEquals("", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(false, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		public void TestLMS2()
		{
			ISOType.ISOCode = "LMS2";
			AssertEquals("[45' Long, 9' High, >2500mm Overall Wide] : Named Cargo, Live fish carrier", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		#endregion

		#region General Purpose

		public void Test45W0()
		{
			ISOType.ISOCode = "45W0";
			AssertEquals("[40' Long, 9'6 High, 8' Wide] : Foldable general purpose containers, Container folding on their base structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void Test45W1()
		{
			ISOType.ISOCode = "45W1";
			AssertEquals("[40' Long, 9'6 High, 8' Wide] : Foldable general purpose containers, Container folding on their side structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void Test42W0()
		{
			ISOType.ISOCode = "42W0";
			AssertEquals("[40' Long, 8'6 High, 8' Wide] : Foldable general purpose containers, Container folding on their base structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void Test42W1()
		{
			ISOType.ISOCode = "42W1";
			AssertEquals("[40' Long, 8'6 High, 8' Wide] : Foldable general purpose containers, Container folding on their side structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void Test54W0()
		{
			ISOType.ISOCode = "54W0";
			AssertEquals("[45' Long, 9' High, 8' Wide] : Foldable general purpose containers, Container folding on their base structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void TestP5W1()
		{
			ISOType.ISOCode = "P5W1";
			AssertEquals("[53' Long, 9'6 High, 8' Wide] : Foldable general purpose containers, Container folding on their side structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void TestP6W1()
		{
			ISOType.ISOCode = "P6W1";
			AssertEquals("[53' Long, >9'6 High, 8' Wide] : Foldable general purpose containers, Container folding on their side structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void TestP7W1()
		{
			ISOType.ISOCode = "P7W1";
			AssertEquals("[53' Long, 8>h>4 High, 8' Wide] : Foldable general purpose containers, Container folding on their side structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void Test22W0()
		{
			ISOType.ISOCode = "22W0";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Foldable general purpose containers, Container folding on their base structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void Test22W1()
		{
			ISOType.ISOCode = "22W1";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Foldable general purpose containers, Container folding on their side structure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void Test22G0()
		{
			ISOType.ISOCode = "22G0";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : General Purpose Container Without Ventilation, Openings at one end or both ends", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void Test42G0()
		{
			ISOType.ISOCode = "42G0";
			AssertEquals("[40' Long, 8'6 High, 8' Wide] : General Purpose Container Without Ventilation, Openings at one end or both ends", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);

			ISOType.ISOCode = "42GA";
			AssertEquals("[40' Long, 8'6 High, 8' Wide] : General Purpose Container Without Ventilation, Openings at one end or both ends", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		public void TestGFV4()
		{
			ISOType.ISOCode = "GFV4";
			AssertEquals("[41' Long, >9'6 High, >2438mm & <=2500mm Overall Wide] : General Purpose Container With Ventilation, Mechanical ventilation system, located externally", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);

			ISOType.ISOCode = "GFVJ";
			AssertEquals("[41' Long, >9'6 High, >2438mm & <=2500mm Overall Wide] : General Purpose Container With Ventilation, Mechanical ventilation system, located externally", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.GeneralPurpose, ISOType.Category);
		}

		#endregion

		#region Open Top

		public void TestFLU4()
		{
			ISOType.ISOCode = "FLU4";
			AssertEquals("[8100mm Long, 8'6 High, >2500mm Overall Wide] : Open-top Container, Opening(s) at one or both ends, plus partial opening on one side and full opening on the other side", ISOType.DimensionsAndDescription);
			AssertEquals(true, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.OpenTop, ISOType.Category);
		}

		public void TestFLU6()
		{
			ISOType.ISOCode = "FLU6";
			AssertEquals("[8100mm Long, 8'6 High, >2500mm Overall Wide] : Open-top Container, Open topped container with removable hard top", ISOType.DimensionsAndDescription);
			AssertEquals(true, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.OpenTop, ISOType.Category);
		}

		#endregion

		#region Refrigerated / Thermal containers

		public void Test22R2()
		{
			ISOType.ISOCode = "22R2";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container, Self-Powered, Mechanically refrigerated", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);

			ISOType.ISOCode = "22RD";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container, Self-Powered, Mechanically refrigerated", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);
		}
		public void Test22H1()
		{
			ISOType.ISOCode = "22H1";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Refrigerated and/or Heated With Removable Equipment, Refrigerated and/or heated with removable equipment located internally", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);

			ISOType.ISOCode = "22HB";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Refrigerated and/or Heated With Removable Equipment, Refrigerated and/or heated with removable equipment located internally", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);
		}

		public void Test22H2()
		{
			ISOType.ISOCode = "22H2";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Refrigerated and/or Heated With Removable Equipment, Refrigerated and/or heated with removable equipment located externally, heat transfer coefficient K = 0,7 W/(M2-K)", ISOType.DimensionsAndDescription);
			AssertEquals(true, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);

			ISOType.ISOCode = "22HD";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Refrigerated and/or Heated With Removable Equipment, Refrigerated and/or heated with removable equipment located externally, heat transfer coefficient K = 0,7 W/(M2-K)", ISOType.DimensionsAndDescription);
			AssertEquals(true, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);
		}

		public void Test22H5()
		{
			ISOType.ISOCode = "22H5";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Insulated, Insulated; heat transfer coefficient K = 0,4 W/(M2-K)", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);

			ISOType.ISOCode = "22HM";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Insulated, Insulated; heat transfer coefficient K = 0,4 W/(M2-K)", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);
		}

		public void Test22H6()
		{
			ISOType.ISOCode = "22H6";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Insulated, Insulated; heat transfer coefficient K = 0 W/(M2-K)", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);

			ISOType.ISOCode = "22HV";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Insulated, Insulated; heat transfer coefficient K = 0 W/(M2-K)", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);
		}

		public void Test22H8()
		{
			ISOType.ISOCode = "22H8";
			AssertEquals("[20' Long, 8'6 High, 8' Wide] : Thermal Container Eutectic, Eutectic, remote mechanical refrigeration", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Refrigerated, ISOType.Category);
		}

		#endregion

		#region Flat Rack

		public void Test42P1()
		{
			ISOType.ISOCode = "42P1";
			AssertEquals("[40' Long, 8'6 High, 8' Wide] : Platform (Container) Fixed, Two complete and fixed ends", ISOType.DimensionsAndDescription);
			AssertEquals(true, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.FlatRack, ISOType.Category);
		}

		public void Test42P6()
		{
			ISOType.ISOCode = "42P6";
			AssertEquals("[40' Long, 8'6 High, 8' Wide] : Platform (Container) Platform Based Container For Named Cargo, Ship's gear carrier", ISOType.DimensionsAndDescription);
			AssertEquals(true, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.FlatRack, ISOType.Category);
		}

		public void Test42P9()
		{
			ISOType.ISOCode = "42P9";
			AssertEquals("[40' Long, 8'6 High, 8' Wide] : Platform (Container) Platform Based Container For Named Cargo, Coil carrier", ISOType.DimensionsAndDescription);
			AssertEquals(true, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.FlatRack, ISOType.Category);
		}

		#endregion

		#region Tanks

		public void Test42K2()
		{
			ISOType.ISOCode = "42K2";
			AssertContains("Pressurized Tank Container (Liquids and Gases), Liquid tank dangerous goods > 2,65 bar and less than or equal to 10 bar pressure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);

			ISOType.ISOCode = "42KD";
			AssertContains("Pressurized Tank Container (Liquids and Gases), Liquid tank dangerous goods > 2,65 bar and less than or equal to 10 bar pressure", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		public void Test42K7()
		{
			ISOType.ISOCode = "42K7";
			AssertContains("Pressurized Tank Container (Liquids and Gases), Cryogenic tank", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);

			ISOType.ISOCode = "42KW";
			AssertContains("Pressurized Tank Container (Liquids and Gases), Cryogenic tank", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		public void Test42N0()
		{
			ISOType.ISOCode = "42N0";
			AssertContains("Hopper Tank Container (Dry), Hopper type vertical discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);

			ISOType.ISOCode = "42NA";
			AssertContains("Hopper Tank Container (Dry), Hopper type vertical discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		public void Test42N3()
		{
			ISOType.ISOCode = "42N3";
			AssertContains("Non-Pressurized Tank Container (Dry), Non-pressurized rear discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);

			ISOType.ISOCode = "42NG";
			AssertContains("Non-Pressurized Tank Container (Dry), Non-pressurized rear discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		public void Test42N5()
		{
			ISOType.ISOCode = "42N5";
			AssertContains("Non-Pressurized Tank Container (Dry), Non-pressurized tipping discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);

			ISOType.ISOCode = "42NM";
			AssertContains("Non-Pressurized Tank Container (Dry), Non-pressurized tipping discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		public void Test42N8()
		{
			ISOType.ISOCode = "42N8";
			AssertContains("Pressurized Tank Container (Dry), Pressurized side discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);

			ISOType.ISOCode = "42NX";
			AssertContains("Pressurized Tank Container (Dry), Pressurized side discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		public void Test42N9()
		{
			ISOType.ISOCode = "42N9";
			AssertContains("Pressurized Tank Container (Dry), Pressurized tipping discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);

			ISOType.ISOCode = "42NL";
			AssertContains("Pressurized Tank Container (Dry), Pressurized tipping discharge", ISOType.DimensionsAndDescription);
			AssertEquals(false, ISOType.OverDimensionAllowed);
			AssertEquals(true, ISOType.IsKnown);
			AssertEquals(ContainerISOType.Categories.Other, ISOType.Category);
		}

		#endregion

		#region Dimensions

		public void TestOverDimensionAllowed()
		{
			ISOType.ISOCode = "22G0";
			AssertEquals(false, ISOType.OverDimensionAllowed);

			ISOType.ISOCode = "11U0";
			AssertEquals(true, ISOType.OverDimensionAllowed);

			ISOType.ISOCode = "22R1";
			AssertEquals(false, ISOType.OverDimensionAllowed);

			ISOType.ISOCode = "22P5";
			AssertEquals(true, ISOType.OverDimensionAllowed);
		}

		#endregion

		#region Implementation

		ContainerISOType ISOType;

		protected override void SetUp()
		{
			base.SetUp();
			ISOType = new ContainerISOType();
		}

		#endregion
	}
}
