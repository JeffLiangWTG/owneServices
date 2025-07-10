using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaPack : ASYCUDA.Business.AsycudaPack, Integration.Customs.ASYCUDA.USExportManifest.IAsycudaPack
	{
		public USExportAsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaPack.Schema
		{
			public const string PSN = "PSN";
			public const string ContactPK = "ContactPK";
			public const string ContactPhone = "ContactPhone";
			public const string FlashpointTemperatureC = "FlashpointTemperatureC";
			public const string FlashpointTemperatureF = "FlashpointTemperatureF";
		}

		public new USExportAsycudaBill Bill => (USExportAsycudaBill)base.Bill;

		public ZString PSN
		{
			get
			{
				var result = ZString.Empty;
				var undgDGRelatedBO = UNDGDGManager.RelatedBusinessObject;
				if (undgDGRelatedBO != null)
				{
					result = undgDGRelatedBO.DG_PSN;
				}
				return result;
			}
		}

		[List(nameof(UNDGs) + "+" + nameof(UNDGDataItemCollection<UNDGDataItem>.Contacts))]
		public ZGuid ContactPK
		{
			get { return ContactManager.Value; }
			set { ContactManager.Value = value; }
		}

		public ZString ContactPhone
		{
			get
			{
				var result = ZString.Empty;
				var undgContactRelatedBO = ContactManager.RelatedBusinessObject;
				if (undgContactRelatedBO != null)
				{
					result = undgContactRelatedBO.OC_Phone;
				}
				return result;
			}
		}

		public ZDecimal FlashpointTemperatureC
		{
			get
			{
				ZDecimal.TryParse(FlashPointManager.Value, out ZDecimal result);
				return result;
			}
			set
			{
				fFlashpointTemperatureF = ZDecimal.Zero;
				FlashPointManager.Value = value.ToString();
			}
		}

		public ZDecimal FlashpointTemperatureF
		{
			get
			{
				var tempF = ZDecimal.Zero;
				if (!fFlashpointTemperatureF.IsEmpty)
				{
					tempF = fFlashpointTemperatureF;
				}
				else
				{
					var tempC = FlashpointTemperatureC;
					tempF = (tempC / 0.5556m) + 32m;
				}
				return tempF;
			}
			set
			{
				fFlashpointTemperatureF = value;
				var ftoC = (value - 32m) * 0.5556m;
				FlashPointManager.Value = ftoC.ToString();
			}
		}

		ZDecimal fFlashpointTemperatureF
		{
			get;
			set;
		}

		public MultipleItemManager FlashPointManager
		{
			get
			{
				return flashPointManager ?? (flashPointManager = UNDGs.UNDGFlashPointManager);
			}
		}
		MultipleItemManager flashPointManager;

		public MultipleItemManagerGUID<OrgContact> ContactManager
		{
			get
			{
				return contactManager ?? (contactManager = UNDGs.UNDGContactManager);
			}
		}
		MultipleItemManagerGUID<OrgContact> contactManager;

		public MultipleItemManagerGUID<UNDGSubstance> UNDGDGManager
		{
			get
			{
				return undgDGManager ?? (undgDGManager = UNDGs.UNDGDGManager);
			}
		}
		MultipleItemManagerGUID<UNDGSubstance> undgDGManager;
	}
}
