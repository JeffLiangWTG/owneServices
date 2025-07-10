//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusDecHouseContainerPackLookups
//
//    This class should be used for overriding collections in AutoCusDecHouseContainerPackLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusDecHouseContainerPackLookups : AutoCusDecHouseContainerPackLookups
	{
		public CusDecHouseContainerPackLookups(AutoCusDecHouseContainerPack parent) : base(parent)
		{
		}

		new BasePackage Parent
		{
			get { return (BasePackage)base.Parent; }
		}

		public ILowestBillCollection<Bill, BaseJobDeclaration> LowestBills
		{
			get { return Parent.Declaration != null ? Parent.Declaration.LowestBills : null; }
		}

		public CodeDescriptionPairList IrrelevantPackages
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var declaration = Parent.Declaration;
				if (declaration != null)
				{
					result.AddRange(declaration.Packages.Cast<BasePackage>().Where(package => package.PK != Parent.PK && !package.Ancestors.Any(ancestor => ancestor.PK == Parent.PK)).ToArray());
				}
				return result;
			}
		}
	}
}
