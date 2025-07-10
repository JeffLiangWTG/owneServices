using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.GUI.Testing
{
	[TestedType(typeof(HVLVISFMetaHeaderForm))]
	public class HVLVISFMetaHeaderFormTest : ZFormBasherTest
	{
		public void TestPerformValidation_OnlyValidateTheFirstHeader()
		{
			var shipment = CreateTestShipment();
			Factory.Save();

			Factory.SuspendValidation();
			var creator = new ISFFromHVLVShipmentCreator(shipment);
			creator.Create(Factory);
			var metaHeader = new HVLVISFMetaHeader(shipment.Factory, shipment.PK);

			AssertEquals("Precondition : 2 ISFHeader was created", 2, metaHeader.RelatedJobs.Count);

			var firstHeader = metaHeader.FirstImporterSecurityFilingJob;
			Assert("Precondition : first Header doesn't has message error", !firstHeader.HasMessageErrors);

			var secondHeader = metaHeader.RelatedJobs.Last();
			Assert("Precondition : second Header doesn't has message error", !secondHeader.HasMessageErrors);

			using (var form = new HVLVISFMetaHeaderForm(shipment.Factory, shipment.PK))
			{
				Factory.ResumeValidation();
				form.FireSaveButton();

				Assert(firstHeader.HasMessageErrors);
				Assert(!secondHeader.HasMessageErrors);
			}
		}

		public void TestShipmentConstructor()
		{
			var shipment = CreateTestShipment();
			var creator = new ISFFromHVLVShipmentCreator(shipment);
			creator.Create(Factory);
			Factory.Save();

			using (var form = new HVLVISFMetaHeaderForm(shipment.Factory, shipment.PK))
			{
				form.Show();
				var menu = GetMenuItem(form, "Messaging");

				CombineAssertions("Form is constructed properly", () =>
				{
					AssertNotNull("Messaging Menu item is added", menu);
					AssertType<HVLVISFMetaHeader>(form.BusinessEntity);
				});
			}
		}

		public void TestRelatedJobsConstructor()
		{
			var shipment = CreateTestShipment();
			var creator = new ISFFromHVLVShipmentCreator(shipment);
			var relatedJobs = creator.CreateHeaders();
			Factory.Save();

			using (var form = new HVLVISFMetaHeaderForm(relatedJobs))
			{
				form.Show();
				var menu = GetMenuItem(form, "Messaging");

				CombineAssertions("Form is constructed properly", () =>
				{
					AssertNotNull("Messaging Menu item is added", menu);
					AssertType<HVLVISFMetaHeader>(form.BusinessEntity);
				});
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var shipment = CreateTestShipment();
			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();
			}

			return new HVLVISFMetaHeaderForm(shipment.Factory, shipment.PK);
		}

		#endregion

		ForwardingShipment CreateTestShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			return shipment;
		}

		MenuItem GetMenuItem(ZForm form, string caption)
		{
			var menuItems = form.Menu.MenuItems.Cast<MenuItem>();
			return menuItems.FirstOrDefault(item => item.Text == caption);
		}
	}

	class HVLVISFMetaHeaderFormForTest : HVLVISFMetaHeaderForm
	{
		internal HVLVISFMetaHeaderFormForTest(BusinessObjectFactory factory, ZGuid shipmentPK) : base(factory , shipmentPK)
		{
		}

		internal HVLVISFMetaHeader MetaHeader => BusinessEntity as HVLVISFMetaHeader;
	}
}
