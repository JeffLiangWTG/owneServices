using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ContactItemProxyLookups : ZLookups
	{
		public ContactItemProxyLookups(ContactItemProxy parent)
			: base(parent)
		{
		}

		#region Contacts

		public virtual OrgContactCollection Contacts
		{
			get { return new OrgContactCollection(Factory); }
		}

		#endregion

		public virtual CodeDescriptionPairList DescriptionList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList SelectableDescriptionList
		{
			get { return DescriptionList; }
		}

		public CodeDescriptionPairList InverseDescriptionList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (ICodeDescription description in SelectableDescriptionList)
				{
					result.AddPair(description.Description, description.Code);
				}

				return result;
			}
		}
	}
}
