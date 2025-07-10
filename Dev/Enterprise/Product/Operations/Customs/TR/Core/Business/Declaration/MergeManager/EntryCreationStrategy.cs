using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class EntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
		{
			var line = (JobComInvoiceLine)invoiceLine;
			var result = base.GetKeyForLine(line);

			result.Add(line.JI_ZZF_NKTaxType);
			result.Add(GetUnitQuantityKey(line, line.JI_CustomsFifthUnitQty));
			result.Add(line.ZG_ReturnToOrigin);
			result.Add(line.ZG_SecondaryTreatedProduct);
			result.Add(line.ZG_InwardProcessingLicenseLineNumber);

			if (GetMergeBy() == OrgConstants.MergeInvoiceLines.TariffAndDescription)
			{
				result.Add(line.JI_NDescription);
			}

			return result;
		}

		protected override bool DocumentForMerge(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument doc)
		{
			return false;
		}

		protected override ZString GetTariffAndDescriptionKey(BaseJobComInvoiceLine baseInvoiceLine) => baseInvoiceLine.JI_NDescription.IsEmpty ? baseInvoiceLine.JI_Description : baseInvoiceLine.JI_NDescription;
	}
}
