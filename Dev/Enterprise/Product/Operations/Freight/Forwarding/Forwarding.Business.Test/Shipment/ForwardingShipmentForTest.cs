using System.Data;
using CargoWise.EntityFramework;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentForTest : ForwardingShipment
	{
		public ForwardingShipmentForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IBaseJobDeclaration[] DeclarationsForTest { get; set; }

		public override IBaseJobDeclaration[] Declarations
		{
			get
			{
				if (DeclarationsForTest == null)
				{
					return base.Declarations;
				}

				return DeclarationsForTest;
			}
		}
	}
}
