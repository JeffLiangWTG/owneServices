using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ShipmentForm))]
	public class ShipmentFormBasherTest : ZFormBasherTest
	{
		[MemoryTestRetryCount(4)]
		public override void TestBashingForm()
		{
			base.TestBashingForm();
		}

		[ExpectNoExceptions]
		public void TestEditFormBashing()
		{
			using (var testForm = GetEditFormToBash())
			{
				testForm.Show();
				ExposeAllTabPages(testForm);

				Application.DoEvents();

				ThrowFailureException();
			}
		}

		public void TestFormIsGarbageCollected()
		{
			var formReference = CreateShipmentShowFormAndCloseFormAndGetWeakReference();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertEquals("IsAlive", false, formReference.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference CreateShipmentShowFormAndCloseFormAndGetWeakReference()
		{
			var cleanFactory = new BusinessObjectFactory();
			var bO = cleanFactory.New<ForwardingShipment>();
			ChildEditableService.SetState(bO.Factory, ChildEditableServiceStates.Shipment);
			var form = new ShipmentForm(bO);
			var formReference = new WeakReference(form, false);

			form.Show();
			form.Close();
			Application.DoEvents();

			return formReference;
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			var line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 10;
			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew();

			shipment.PickupConfirms.AddNew();
			shipment.DeliveryConfirms.AddNew();

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.JE_OverrideFreightDefaults] = true;
			declaration[JobDeclarationSchema.JE_MessageType] = "IMP";

			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			var result = new ShipmentForm(shipment);
			result.ControllerID = ControllerIDs.JobShipment;
			return result;
		}

		Form GetEditFormToBash()
		{
			var factory = new BusinessObjectFactory();
			var bO = factory.New<ForwardingShipment>();
			bO.Consols.AddNew();
			var dec = (BusinessObject)factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			dec[JobDeclarationSchema.JE_JS] = bO.PK;
			dec[JobDeclarationSchema.JE_OverrideFreightDefaults] = true;

			factory.Save();
			ChildEditableService.SetState(bO.Factory, ChildEditableServiceStates.Shipment);
			var result = new ShipmentForm(bO);

			if (result is ZForm zForm)
			{
				zForm.IsFormToBash_ForTestOnly = true;
			}

			result.ControllerID = ControllerIDs.JobShipment;
			return result;
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}

		#endregion
	}
}
