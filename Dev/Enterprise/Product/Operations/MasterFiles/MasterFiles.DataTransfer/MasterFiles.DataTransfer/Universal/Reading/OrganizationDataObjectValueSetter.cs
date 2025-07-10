using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class OrganizationDataObjectValueSetter : ValueSetter
	{
		public OrganizationDataObjectValueSetter(IDocAddresses addressParent, OrganizationAddress addressData, IXmlImportLogger logger, UniversalObjectFactory factory, DocAddressType? docAddressOverride = null)
			: base(() => addressData, logger)
		{
			this.addressParent = Argument.NotNull(addressParent, "addressParent");
			this.factory = Argument.NotNull(factory, "factory");
			this.docAddressOverride = docAddressOverride;
		}
		readonly IDocAddresses addressParent;
		readonly UniversalObjectFactory factory;
		readonly DocAddressType? docAddressOverride;

		public static ZString GetKey(ZGuid pk, ZString addressType)
		{
			return pk.ToStringKey() + addressType;
		}

		protected OrganizationAddress addressData
		{
			get { return (OrganizationAddress)value; }
		}

		protected sealed override ZString MatchingKeyCore
		{
			get { return GetKey(addressParent.DocAddresses.Master.PK, docAddressOverride.HasValue ? (ZString)docAddressOverride.ToString() : addressData.AddressType.GetValueOrDefault()); }
		}

		protected sealed override void SetValueCore()
		{
			new OrganisationDataObjectReader(addressData, logger, factory).GetMatchedOrNew(addressParent, docAddressOverride);
		}
	}
}
