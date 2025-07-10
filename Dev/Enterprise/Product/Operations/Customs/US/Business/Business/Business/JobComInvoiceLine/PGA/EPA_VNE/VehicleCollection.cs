using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class VehicleCollection : DependentCusAddInfoCollection<Vehicle, BusinessObject>, IPGADataCorrectionCollection
	{
		public VehicleCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USPGAVehicle)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = child as Vehicle;

			var invoiceLine = Master as JobComInvoiceLine;
			if (newElement != null && invoiceLine != null)
			{
				if (invoiceLine.Importer != null)
				{
					newElement.US_OA_Owner = invoiceLine.Importer.MainAddress.PK;
				}

				if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					if (newElement != null && invoiceLine != null && invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
					{
						Vehicle previousVNELine = this[Count - 1];
						newElement.CopyPersistentValuesFrom(previousVNELine);
						newElement.US_VNEElectronicImage = CargoWise.Types.ZBool.False;
					}
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return AllowAddNewPGALines && base.AllowNewCore; }
		}

		public bool AllowAddNewPGALines
		{
			get
			{
				if (!fAllowAddNewPGALines.HasValue)
				{
					var invoiceLine = Master as JobComInvoiceLine;
					fAllowAddNewPGALines = invoiceLine?.AllowAddNewLineToPGACollection() ?? true;
				}
				return fAllowAddNewPGALines.Value;
			}
			set
			{
				fAllowAddNewPGALines = value;
				if (value)
				{
					var invoiceLine = Master as JobComInvoiceLine;
					invoiceLine?.Declaration?.UpdatePGADataReplacementUpdateRequired();
				}
			}
		}
		bool? fAllowAddNewPGALines;

		System.Collections.Generic.IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();
	}
}
