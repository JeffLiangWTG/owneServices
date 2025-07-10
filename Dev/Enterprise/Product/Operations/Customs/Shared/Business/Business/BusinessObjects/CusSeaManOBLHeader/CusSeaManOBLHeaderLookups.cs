//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManOBLHeaderLookups
//
//    This class should be used for overriding collections in AutoCusSeaManOBLHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLHeaderLookups : AutoCusSeaManOBLHeaderLookups
	{
		public CusSeaManOBLHeaderLookups(AutoCusSeaManOBLHeader parent) : base(parent)
		{
		}

		#region Properties

		#region Consignees

		public override OrgHeaderCollection Consignees
		{
			get { return new CusSeaManOBLHeaderConsigneeCollection(Factory, Parent); }
		}

		#endregion

		#region Consignors

		public override OrgHeaderCollection Consignors
		{
			get { return new CusSeaManOBLHeaderConsignorCollection(Factory, Parent); }
		}

		#endregion

		#region MethodsOfPayment

		protected virtual CodeDescriptionPairList GetNewMethodsOfPayment()
		{
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList MethodsOfPayment
		{
			get
			{
				if (fMethodsOfPayment == null)
				{
					fMethodsOfPayment = GetNewMethodsOfPayment();
				}

				return fMethodsOfPayment;
			}
		}
		CodeDescriptionPairList fMethodsOfPayment;

		#endregion

		#region CargoCodes

		protected virtual CodeDescriptionPairList GetNewCargoCodes()
		{
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList CargoCodes
		{
			get
			{
				if (fCargoCodes == null)
				{
					fCargoCodes = GetNewCargoCodes();
				}

				return fCargoCodes;
			}
		}
		CodeDescriptionPairList fCargoCodes;

		#endregion

		#endregion

		#region Implementation

		public new AutoCusSeaManOBLHeader Parent
		{
			get { return (AutoCusSeaManOBLHeader)base.Parent; }
		}

		#endregion
	}
}
