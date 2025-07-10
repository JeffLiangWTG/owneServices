using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class CarrierMessageDataLookups
	{
		public CarrierMessageDataLookups(OrgHeader carrier)
		{
			this.carrier = carrier;
		}

		readonly OrgHeader carrier;

		public CodeDescriptionPairList EBLProviderList
		{
			get
			{
				if (eblProviderList == null)
				{
					eblProviderList = new CodeDescriptionPairList();
					eblProviderList.AddPair(EBLProviderConstants.Codes.NotListed);

					var availableShippingLineEBLProviders = carrier?.ShippingLine?.ShippingLineEBLProviders?.OfType<RefShippingLineEBLProvider>().Where(x => x.RSE_IsAvailable);
					if (availableShippingLineEBLProviders != null)
					{
						foreach (var eblProvider in availableShippingLineEBLProviders)
						{
							eblProviderList.AddPair(eblProvider.RSE_Name);
						}
					}
				}

				return eblProviderList;
			}
		}
		CodeDescriptionPairList eblProviderList;

		public CodeDescriptionPairList BRWoodenPackageProcessTypes
		{
			get
			{
				if (brWoodenPackageProcessTypes == null)
				{
					brWoodenPackageProcessTypes = new CodeDescriptionPairList();
					brWoodenPackageProcessTypes.AddPair(DocDataConstants.WoodenPackageProcessTypes.NotApplicable);
					brWoodenPackageProcessTypes.AddPair(DocDataConstants.WoodenPackageProcessTypes.NotTreatedAndNotCertified);
					brWoodenPackageProcessTypes.AddPair(DocDataConstants.WoodenPackageProcessTypes.Processed);
					brWoodenPackageProcessTypes.AddPair(DocDataConstants.WoodenPackageProcessTypes.TreatedAndCertified);
				}

				return brWoodenPackageProcessTypes;
			}
		}
		CodeDescriptionPairList brWoodenPackageProcessTypes;
	}
}
