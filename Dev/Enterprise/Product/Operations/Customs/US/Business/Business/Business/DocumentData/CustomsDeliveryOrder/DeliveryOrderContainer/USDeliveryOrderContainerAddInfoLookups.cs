//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDeliveryOrderContainerAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSDeliveryOrderContainerAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USDeliveryOrderContainerAddInfoLookups : AutoUSDeliveryOrderContainerAddInfoLookups
	{
		public USDeliveryOrderContainerAddInfoLookups(AutoUSDeliveryOrderContainerAddInfo parent)
			: base(parent)
		{
		}

		protected new USDeliveryOrderContainerAddInfo Parent
		{
			get { return (USDeliveryOrderContainerAddInfo)base.Parent; }
		}

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				return Factory.GetCachedValue("USContainerModeList", delegate
				{
					CusContainer container = Factory.GetNull<CusContainer>();
					return container.Lookups.CO_FCL_LCL_NCT_List;
				});
			}
		}

		public RefContainerCollection ContainerTypeList
		{
			get { return new RefContainerCollection(Factory, Core.Constants.TransportModes.Sea); }
		}

		public CodeDescriptionPairList WeightUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList ContainerList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				DeliveryOrderContainer container = Parent.Parent;
				DeliveryOrderHeader header = container == null ? null : container.Parent;
				JobDeclaration declaration = header == null ? null : header.Parent;
				if (declaration != null)
				{
					foreach (CusContainer cusContainer in declaration.CusContainers)
					{
						ZString containerNumber = cusContainer.CO_ContainerNumber;
						if (!list.ContainsCode(containerNumber))
						{
							list.AddPair(containerNumber, containerNumber);
						}
					}

					foreach (DeliveryOrderHeader existingHeader in declaration.DeliveryOrderHeaders)
					{
						foreach (DeliveryOrderContainer existingContainer in existingHeader.DeliveryOrderContainers)
						{
							if (existingContainer != container && list.ContainsCode(existingContainer.US_ContainerNumber))
							{
								list.RemoveCode(existingContainer.US_ContainerNumber);
							}
						}
					}
				}
				return list;
			}
		}

		public ShippingOrPackingingUnitList PackageTypeList
		{
			get { return Factory.GetCachedValue<ShippingOrPackingingUnitList>(); }
		}
	}
}
