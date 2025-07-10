using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class NHTSAHeaderCollection : DependentCusAddInfoCollection<NHTSAHeader, BusinessObject>, IPGADataCorrectionCollection
	{
		public NHTSAHeaderCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USNHTSAHeader)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var header = child as NHTSAHeader;
			if (header != null && header.InvoiceLine != null)
			{
				header.US_NHTFabricatingMFRAddress = header.InvoiceLine.JI_OA_ManufacturerAddress;
				header.US_OA_NHTRetailer = header.InvoiceLine.JI_OA_SoldToPartyAddress;
				if (header.InvoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					NHTSAHeader previousHeader = this[Count - 1];
					header.CopyPersistentValuesFrom(previousHeader);
					header.US_NHTElectronicImage = ZBool.False;
					header.US_NHTFabricatingMFROrgPK = previousHeader.US_NHTFabricatingMFROrgPK;
					header.US_NHTFabricatingMFRAddress = previousHeader.US_NHTFabricatingMFRAddress;
					header.US_OA_NHTRetailer = previousHeader.US_OA_NHTRetailer;

					foreach (NHTSADetails detail in previousHeader.NHTSADetails)
					{
						header.NHTSADetails.Add(detail.Clone());
					}

					header.NHTSADocuments.RemoveAndDeleteAll();
					foreach (NHTSADocument document in previousHeader.NHTSADocuments)
					{
						header.NHTSADocuments.Add(document.Clone());
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
