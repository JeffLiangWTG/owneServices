using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ShipmentTypeUserControlTest : TestCase
	{
		public void TestApplicationCodeDropEditControl()
		{
			AssertType<ZDropEdit>(control.ApplicationCodeDropEdit);
		}

		public void TestServiceLevelCodeFindBoxControl()
		{
			AssertType<ZCodeFindBox>(control.ServiceLevelCodeFindBox);
		}

		public void TestMessageSubTypeDropEditControl()
		{
			AssertType<ZDropEdit>(control.MessageSubTypeDropEdit);
		}

		public void TestMessageTypeDropEditControl()
		{
			AssertType<ZDropEdit>(control.MessageTypeDropEdit);
		}

		public void TestTransportModeDropEditControl()
		{
			AssertType<ZDropEdit>(control.TransportModeDropEdit);
		}

		public void TestContainerModeDropEditControl()
		{
			AssertType<ZDropEdit>(control.ContainerModeDropEdit);
		}

		public void TestInlandModeOfTransportDropEditControl()
		{
			AssertType<ZDropEdit>(control.InlandModeOfTransportDropEdit);
		}

		public void TestDeclarantTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.DeclarantTypeDropEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentTypeUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentTypeUserControl control;
	}
}
