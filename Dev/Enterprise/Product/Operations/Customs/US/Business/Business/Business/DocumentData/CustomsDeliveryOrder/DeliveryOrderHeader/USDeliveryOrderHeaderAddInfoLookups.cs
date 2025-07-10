using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USDeliveryOrderHeaderAddInfoLookups : AutoUSDeliveryOrderHeaderAddInfoLookups
	{
		public USDeliveryOrderHeaderAddInfoLookups(AutoUSDeliveryOrderHeaderAddInfo parent)
			: base(parent)
		{
		}

		protected new USDeliveryOrderHeaderAddInfo Parent
		{
			get { return (USDeliveryOrderHeaderAddInfo)base.Parent; }
		}

		public DeliveryOrderPrepaidCollectTypeList PrepaidCollectList
		{
			get { return Factory.GetCachedValue<DeliveryOrderPrepaidCollectTypeList>(); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public LocalTransportCollection InlandCarriers
		{
			get { return new LocalTransportCollection(Factory); }
		}

		public USCarrierCombinedCollection USCarriers
		{
			get { return new USCarrierCombinedCollection(Factory); }
		}

		public OrgHeaderCollection BillToParties
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public CodeDescriptionPairList OrderReferenceList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				DeliveryOrderHeader header = Parent.Parent;
				JobDeclaration declaration = header != null ? header.Declaration : null;
				if (declaration != null)
				{
					if (!declaration.JE_OwnerRef.IsEmpty)
					{
						result.AddPair(declaration.JE_OwnerRef, declaration.JE_OwnerRef);
					}

					foreach (OrderItem order in declaration.DocsAndCartage.OrderItems)
					{
						result.AddPairIfNotExist(order.JT_OrderReference, order.JT_OrderReference);
					}

					foreach (var order in declaration.AttachedOrders)
					{
						result.AddPairIfNotExist(order.JD_OrderNumberAndSplit, order.JD_OrderNumberAndSplit);
					}
				}
				return result;
			}
		}
	}
}
