using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.DocumentWrappers
{
	[CodeAlive("This is used by reflection code in DocumentWrapper.")]
	public class FreightWrapperFromReconDeclaration : FreightWrapper
	{
		public FreightWrapperFromReconDeclaration(ReconDeclaration declarationBO, BusinessObjectFactory factory)
			: base(declarationBO, factory)
		{
			this.declarationBO = declarationBO;
		}

		public static FreightWrapperFromReconDeclaration New(ReconDeclaration declarationBO, BusinessObjectFactory factory)
		{
			if (declarationBO != null)
			{
				return new FreightWrapperFromReconDeclaration(declarationBO, factory);
			}

			return null;
		}

		readonly ReconDeclaration declarationBO;

		protected override Integration.Customs.US.IReconDeclaration GetReconDeclaration()
		{
			return declarationBO;
		}

		#region Related Business Objects

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			return new CodeAndDescriptionWrapper(Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon, new Enterprise.Customs.Common.US.USJobMessageTypeList(), Factory);
		}

		#endregion

		#region Organisations

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, declarationBO.Importer, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			return new OrganisationWrapper(OrganisationUsageType.ImportBroker, declarationBO.Branch.OrgProxy, ContactType.All, Factory);
		}

		#endregion

		#region MoneyWrappers

		protected override MoneyWrapper GetGoodsValue()
		{
			var totalOfInvoiceAmounts = Money.Empty;
			foreach (var invoiceHeader in declarationBO.Invoices)
			{
				var invoiceAmount = new Money(invoiceHeader.JZ_InvoiceAmount, invoiceHeader.Invoice_Currency);
				totalOfInvoiceAmounts = invoiceHeader.CurrencyConverter.Add(totalOfInvoiceAmounts, invoiceAmount);
			}
			return new MoneyWrapper(totalOfInvoiceAmounts, Factory);
		}

		#endregion

		#region General Freight References

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("DA4E0FF2-2A59-4CBF-845C-C6426175FF2A", "Declaration");
		}

		protected override ZString GetJobNumber()
		{
			return declarationBO.JE_DeclarationReference;
		}

		#endregion

		#region TrackingUrl

		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.Declaration;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return declarationBO.PK;
		}

		#endregion

		#region Branding

		protected override Image JobHeaderBranchLogo
		{
			get
			{
				Image result = base.JobHeaderBranchLogo;
				if (result == null)
				{
					DocBranch branch = DocBranch.New(declarationBO.Branch, Factory);
					return branch != null ? branch.Logo : null;
				}
				return result;
			}
		}

		#endregion

		protected override BusinessObject GetParentBOForNoteStorageEDocsAndDocData()
		{
			return declarationBO;
		}
	}
}
