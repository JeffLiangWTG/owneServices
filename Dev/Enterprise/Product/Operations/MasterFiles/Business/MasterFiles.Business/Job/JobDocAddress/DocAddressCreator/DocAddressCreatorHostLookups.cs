using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DocAddressCreatorHostLookups : ZLookups
	{
		public DocAddressCreatorHostLookups(DocAddressCreatorHost parent)
			: base(parent)
		{
		}

		#region AddressTypes

		public CodeDescriptionPairList AddressTypes
		{
			get
			{
				if (addressTypes == null)
				{
					addressTypes = new CodeDescriptionPairList();

					if (Parent.DocAddressParent != null && Parent.CodeFormatter != null)
					{
						foreach (DocAddressType addrType in Parent.DocAddressParent.SupportedAddressTypes)
						{
							addressTypes.AddPair(Parent.CodeFormatter(addrType), DocAddressTypes.GetDescription(Factory, addrType));
						}
					}
				}
				return addressTypes;
			}
		}
		CodeDescriptionPairList addressTypes;

		#endregion

		#region Organisations

		public OrgHeaderCollection Organisations
		{
			get { return Parent.DocAddress != null ? Parent.DocAddress.Lookups.OrgHeader_List : new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Implementation

		protected new DocAddressCreatorHost Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DocAddressCreatorHost)base.Parent; }
		}

		#endregion
	}
}
