using System;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(DetentionAdviceDeliveryRegistryItemEditor))]
	internal class DetentionAdviceDeliveryRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DetentionAdviceDeliveryRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DetentionAdviceDeliveryRegistryItemEditor(new DetentionAdviceDeliveryRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DetentionAdviceDeliveryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			StmPrintQueue printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_AllowPrinting = true;
			printQueue.SQ_IsPrivate = false;
			printQueue.SQ_QueueName = "Blaticus";
			printQueue.SQ_DisplayName = "Blaticus";
			DetentionAdviceDelivery auto = new DetentionAdviceDelivery(Factory);
			auto.Mode = DetentionAdviceDeliveryMode.Codes.Auto;
			auto.Printer = printQueue.PK;
			DetentionAdviceDelivery print = new DetentionAdviceDelivery(Factory);
			print.Mode = DetentionAdviceDeliveryMode.Codes.Print;
			print.Printer = printQueue.PK;
			DetentionAdviceDelivery notify = new DetentionAdviceDelivery(Factory);
			notify.Mode = DetentionAdviceDeliveryMode.Codes.Notify;
			notify.NotificationGroup = Constants.Groups.AllPK;
			return new object[] { auto, print, notify, };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			RegistryZUserControl control = (RegistryZUserControl)editorPane;
			return !control.ReadOnly;
		}
		#endregion
	}
}
