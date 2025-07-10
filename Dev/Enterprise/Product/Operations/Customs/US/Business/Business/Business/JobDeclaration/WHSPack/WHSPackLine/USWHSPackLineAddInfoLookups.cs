//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSWHSPackLineAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSWHSPackLineAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
namespace Enterprise.Customs.US.Business
{
	public class USWHSPackLineAddInfoLookups : AutoUSWHSPackLineAddInfoLookups
	{
		public USWHSPackLineAddInfoLookups(AutoUSWHSPackLineAddInfo parent)
			: base(parent)
		{
		}

		protected new USWHSPackLineAddInfo Parent
		{
			get { return (USWHSPackLineAddInfo)base.Parent; }
		}

		WHSPackLine PackLine
		{
			get { return Parent.Parent; }
		}

		JobDeclaration Declaration
		{
			get
			{
				var packLine = PackLine;
				return packLine == null ? null : packLine.Parent;
			}
		}

		public IBusinessObjectCollection InvoiceLineList
		{
			get
			{
				IBusinessObjectCollection result = null;
				var declaration = Declaration;
				if (declaration != null)
				{
					result = declaration.PackableInvoiceLines;
				}
				return result;
			}
		}

		public ICodeDescriptionPairList WHSPackList
		{
			get
			{
				ICodeDescriptionPairList result = null;
				var declaration = Declaration;
				if (declaration != null)
				{
					result = declaration.WHSPacks.UniquePackageReferenceList;
				}
				return result;
			}
		}
	}
}
