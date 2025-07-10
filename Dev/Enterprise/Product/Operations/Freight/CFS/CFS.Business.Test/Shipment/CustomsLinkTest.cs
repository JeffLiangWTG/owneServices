using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business
{
	public class CustomsLinkTest : TestCaseWithFactory
	{
		public void TestTotalOuterPackages()
		{
			CFSLoadListConsol loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();
			loadList.JK_RL_NKDischargePort = "AUSYD";
			loadList.JK_RL_NKLoadPort = "NZAKA";
			TallyContainer container = Factory.NewWithValidTestData<TallyContainer>();
			loadList.Containers.Add(container);
			PackUnpackShipment shipment = Factory.NewWithValidTestData<PackUnpackShipment>();
			shipment.JS_RL_NKDestination = "NZAKA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			loadList.Shipments.Add(shipment);
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 100;
			packLine.JL_Outturn = 100;
			packLine.JL_Pillaged = 10;
			packLine.JL_Damaged = 15;
			Factory.Save();
			container.PackLines.Add(packLine);
			Factory.Save();
			Type tallyOutturnType = Type.GetType("Enterprise.Customs.AU.Declaration.Business.TallyOutturn, Enterprise.Customs.AU.Declaration.Business", true);
			BusinessObject tallyOutturn = Factory.New(tallyOutturnType);
			Type tallyCustomsListenerType = Type.GetType("Enterprise.Customs.AU.Declaration.Business.TallyCustomsListener, Enterprise.Customs.AU.Declaration.Business", true);
			IOutturnLink tallyCustomsListener = (IOutturnLink)Activator.CreateInstance(tallyCustomsListenerType, new object[] { tallyOutturn });
			((IOutturnLinkable)container).SetOutturnLink(tallyCustomsListener);
			shipment.ParentContainerRegistration = container;
			tallyOutturn[CusOutturnSchema.Constants.C5_ParentID] = shipment.PK;
			tallyOutturn[CusOutturnSchema.Constants.C5_ParentTableCode] = JobShipmentSchema.Constants.Prefix;
			shipment.NotifyCustomsListener();
			MethodInfo getPackagesOutturnedMethodInfo = shipment.CustomsOutturnListener.GetType().GetMethod("GetPackagesOutturned", BindingFlags.Public | BindingFlags.Instance);
			AssertEquals("PackagesOutturned on CustomsOutternListener", 100, getPackagesOutturnedMethodInfo.Invoke(shipment.CustomsOutturnListener, null));
		}

		public void TestOuterPacks()
		{
			linkOutturnMock.Reset();
			linkOutturnMock.Setup(m => m.NumberOfPackages).Returns(new ZInt(7));
			linkOutturnMock.Verify(m => m.NumberOfPackages, Times.AtMost(2));
			shipment.JS_OuterPacks = 5;
			linkOutturnMock.VerifyAll();
			AssertEquals(5, shipment.JS_OuterPacks);

			linkOutturnMock.Reset();
			linkOutturnMock.Setup(m => m.NumberOfPackages).Returns(new ZInt(7));
			linkOutturnMock.Verify(m => m.NumberOfPackages, Times.AtMost(2));
			shipment.JS_OuterPacks = 0;
			linkOutturnMock.VerifyAll();
			AssertEquals(7, shipment.JS_OuterPacks);

			linkOutturnMock.Reset();
			linkOutturnMock.Setup(m => m.NumberOfPackages).Returns(new ZInt(7));
			linkOutturnMock.Verify(m => m.NumberOfPackages, Times.AtMost(2));
			shipment.JS_OuterPacks = 3;
			linkOutturnMock.VerifyAll();
			AssertEquals(3, shipment.JS_OuterPacks);
		}

		[ExpectNoExceptions()]
		public void TestOuterPacksWhenLinkNull()
		{
			shipment.customsOutturnListener = null;
			AssertNull(shipment.CustomsOutturnListener);
			//shipment.OnLoaded();
			shipment.ParentContainerRegistration = null;
			shipment.customsOutturnListener = null;
			AssertNull(shipment.CustomsOutturnListener);
			shipment.JS_OuterPacks = 0;
		}

		public void TestOuterPacksValidation()
		{
			const string warningText = "The package count is different from the outturn's that this pack line is linked to. The outturn pack count is 3.";
			linkOutturnMock.Setup(m => m.NumberOfPackages).Returns(new ZInt(3));
			shipment.JS_OuterPacks = 3;
			AssertNoWarning(shipment.JS_OuterPacksInfo, warningText);
			shipment.JS_OuterPacks = 2;
			AssertHasWarning(shipment.JS_OuterPacksInfo, warningText);
		}

		[ExpectNoExceptions]
		public void TestOutturnedPacks()
		{
			// If you caused this test to fail and you have no idea why, try fiddling with the number on the next line.
			int callsDuringSetDefaultsInCollection = 1;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			PackLine line1 = null;
			CheckMockExpectations(0, false, false, "", callsDuringSetDefaultsInCollection, delegate
			{ line1 = shipment.OuterPackLines.AddNew(); });

			CheckMockExpectations(5, false, false, "PKG", delegate
			{ line1.JL_Outturn = 5; });

			PackLine line2 = null;
			CheckMockExpectations(5, false, false, "PKG", callsDuringSetDefaultsInCollection, delegate
			{ line2 = shipment.OuterPackLines.AddNew(); });

			CheckMockExpectations(8, false, false, "PKG", delegate
			{ line2.JL_Outturn = 3; });
			CheckMockExpectations(8, true, false, "PKG", delegate
			{ line2.JL_Pillaged = 1; });
			CheckMockExpectations(5, true, false, "PKG", delegate
			{ line2.JL_Outturn = 0; });
			CheckMockExpectations(5, true, false, "PKG", delegate
			{ line1.JL_Pillaged = 4; });
			CheckMockExpectations(5, true, true, "PKG", delegate
			{ line2.JL_Damaged = 7; });
			CheckMockExpectations(5, true, true, "PKG", delegate
			{ line1.JL_Pillaged = 0; });
			CheckMockExpectations(5, false, true, "PKG", delegate
			{ line2.JL_Pillaged = 0; });
			shipment.OuterPackLines.RemoveAndDelete(line1);
			CheckMockExpectations(0, false, true, "BAG", delegate
			{ line2.JL_F3_NKPackType = "BAG"; });
		}

		public void TestOutturnDeletedWhileLinked()
		{
			// TODO: Do a test on the other side in the stand alone outturn code for the other direction.
			AssertNotNull("precondition", shipment.CustomsOutturnListener);
			linkOutturnMock.Reset();
			linkOutturnMock.Setup(m => m.IsDeleted).Returns(ZBool.True);
			AssertNull(shipment.CustomsOutturnListener);
			linkOutturnMock.VerifyAll();
		}

		#region Implementation

		delegate void DoStuff();

		void CheckMockExpectations(ZInt outturned, ZBool pillaged, ZBool damaged, ZString packageType, DoStuff doStuff)
		{
			CheckMockExpectations(outturned, pillaged, damaged, packageType, 1, doStuff);
		}

		void CheckMockExpectations(ZInt outturned, ZBool pillaged, ZBool damaged, ZString packageType, int expectedCalls, DoStuff doStuff)
		{
			for (int i = 0; i < expectedCalls; i++)
			{
				linkOutturnMock.Setup(m => m.SetPackagesOutturned(outturned));
				linkOutturnMock.Setup(m => m.SetPillaged(pillaged));
				linkOutturnMock.Setup(m => m.SetDamaged(damaged));
				linkOutturnMock.Setup(m => m.SetPackageType(packageType));
			}
			doStuff();
			linkOutturnMock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();

			container = Factory.New<TallyContainer>();
			shipment = container.PackUnpackShipments.AddNew();
			linkOutturnMock = new Mock<IOutturn>();
			shipment.customsOutturnListener = linkOutturnMock.Object;
			linkOutturnMock.Setup(m => m.IsDeleted).Returns(ZBool.False);
		}

		TallyContainer container;
		PackUnpackShipment shipment;
		Mock<IOutturn> linkOutturnMock;

		#endregion

	}
}
