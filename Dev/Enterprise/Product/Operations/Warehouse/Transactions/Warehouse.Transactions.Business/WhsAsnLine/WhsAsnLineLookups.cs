//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsAsnLineLookups
//
//    This class should be used for overriding collections in AutoWhsAsnLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAsnLineLookups : AutoWhsAsnLineLookups
	{
		#region Constructors

		public WhsAsnLineLookups(AutoWhsAsnLine parent) : base(parent)
		{
		}

		#endregion

		#region Parent

		protected new WhsAsnLine Parent
		{
			get { return (WhsAsnLine)base.Parent; }
		}

		#endregion

		#region Product UQ List

		public CodeDescriptionPairList ProductUQList
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		#region Supplier Parts

		public override OrgSupplierPartCollection SupplierParts
		{
			get
			{
				WhsOrgSupplierPartCollection result;
				if (Parent.Docket != null)
				{
					result = new WhsOrgSupplierPartCollection(Factory, null, Parent.Docket.Client, false);
					if (Parent.Docket.Client != null)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", Parent.Docket.WD_OH_Client));
					}
				}
				else
				{
					result = new WhsOrgSupplierPartCollection(Factory);
				}
				return result;
				}
		}

		#endregion
	}
}
