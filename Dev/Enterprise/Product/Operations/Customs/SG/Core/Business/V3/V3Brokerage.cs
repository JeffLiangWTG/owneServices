using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V3.Business
{
	public class V3Brokerage : NonPersistentBusinessObject, IObsoleteValidation
	{
		public V3Brokerage(ForwardingShipment shipment, BusinessObjectFactory factory)
			: base(factory)
		{
			this.shipment = shipment;
		}

		public V3Brokerage(ZGuid pK, BusinessObjectFactory factory)
			: base(factory)
		{
			this._pK = pK;
		}

		readonly ForwardingShipment shipment;
		readonly ZGuid _pK;

		public V3JobDeclarationCollection Declarations
		{
			get
			{
				if (declarations == null)
				{
					declarations = new V3JobDeclarationCollection(Factory);

					if (shipment != null)
					{
						declarations.AdditionalFilter = new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK);
					}
					else
					{
						declarations.AdditionalFilter = new ZQuery(JobDeclarationSchema.PK, _pK);
					}

					foreach (V3JobDeclaration dec in declarations)
					{
						dec.SuspendValidation();
					}
				}
				return declarations;
			}
		}
		V3JobDeclarationCollection declarations;
	}
}
