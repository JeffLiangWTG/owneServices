using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocExchangeControlDec : FormatterForBillTypeLayout
	{
		public DocExchangeControlDec(DocJobComInvoiceHeader invoiceHeader)
			: base()
		{
			fInvoiceHeader = invoiceHeader;
		}

		public override ZString UnformattedGoodsDescription
		{
			get
			{
				ZString result = ZString.Empty;

				if (fInvoiceHeader.Declaration != null)
				{
					if (!fInvoiceHeader.Declaration.DetailedGoodsDescription.IsEmpty)
					{
						result = fInvoiceHeader.Declaration.DetailedGoodsDescription;
					}
					else
					{
						result = fInvoiceHeader.Declaration.GoodsDescription;
					}
				}

				return result;
			}
		}

		public override ZString UnformattedMarksAndNumbers
		{
			get
			{
				ZString result = ZString.Empty;

				if (fInvoiceHeader.Declaration != null)
				{
					result = fInvoiceHeader.Declaration.MarksAndNumbers;
				}

				return result;
			}
		}

		public override ZString PackageTypeDescription
		{
			get
			{
				ZString result = ZString.Empty;

				if (fInvoiceHeader.Declaration != null)
				{
					result = fInvoiceHeader.Declaration.PackTypeDescription;
				}

				return result;
			}
		}

		public override ZInt PackCount
		{
			get
			{
				ZInt result = 0;

				if (fInvoiceHeader.Declaration != null)
				{
					result = fInvoiceHeader.Declaration.TotalNoOfPacks;
				}

				return result;
			}
		}

		public override IDocSimpleContainerCollection Containers
		{
			get
			{
				IDocSimpleContainerCollection containers = new IDocSimpleContainerCollection(Factory);

				if (fInvoiceHeader.Declaration != null)
				{
					foreach (DocCusContainer container in fInvoiceHeader.Declaration.Containers)
					{
						containers.Add(container);
					}
				}

				return containers;
			}
		}

		public override ZString ContainerSectionHeading
		{
			get { return (NoResString)"CONTAINER        SEAL                     TYPE         "; }
		}

		public override ZBool ShowContainerAdditionalDetails
		{
			get { return ZBool.False; }
		}

		#region Constants

		public override ZInt MarksAndNumbersAndDescriptionRowHeight
		{
			get { return fInvoiceHeader.MarksAndNumbsAndDescHeight; }
		}

		public override ZInt MarksAndNumbersWidth
		{
			get { return fInvoiceHeader.MarksAndNumbersWidth; }
		}
		public override ZInt DescriptionWidth
		{
			get { return fInvoiceHeader.GoodsDescWidth; }
		}

		public override ZInt ContainerRowHeight
		{
			get { return fInvoiceHeader.NoOfContainerRows; }
		}

		#endregion

		protected DocJobComInvoiceHeader fInvoiceHeader;
	}
}
