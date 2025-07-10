using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;

namespace Enterprise.Freight.Forwarding.Business
{
	public class OrdersOnDeclarationLimitHelper : CollectionLimitHelperForPotentialHVLV
	{
		public OrdersOnDeclarationLimitHelper(IBaseJobDeclaration declaration)
			: base((BusinessObject)declaration)
		{
			this.declaration = declaration;
		}

		readonly IBaseJobDeclaration declaration;

		protected override IList Collection
		{
			get { return ((IAttachOrders)declaration).AttachedOrders; }
		}

		protected override int Limit
		{
			get { return (int)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().OrdersPerDeclarationLimit.Value; }
		}

		protected override DateTime LimitIntroductionTimeUtc
		{
			get { return (DateTime)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().OrdersPerDeclarationLimitIntroductionTimeUTC.Value; }
		}

		protected override ZDateTime ParentCreationTimeUtc
		{
			get { return (ZDateTime)((BusinessObject)declaration)[JobDeclarationSchema.JE_SystemCreateTimeUtc]; }
		}

		protected override string ChildPlural
		{
			get { return Res.GetString("95c04e5c-2c70-44e4-9099-44c79e7eb19e", "Orders"); }
		}

		protected override string ChildSingular
		{
			get { return Res.GetString("fcd13da5-5195-4637-8449-25619c5b4626", "Order"); }
		}

		protected override string ParentSingular
		{
			get { return Res.GetString("bc9387b4-502a-4b6a-a705-08d03d5b65a2", "Declaration"); }
		}
	}
}
