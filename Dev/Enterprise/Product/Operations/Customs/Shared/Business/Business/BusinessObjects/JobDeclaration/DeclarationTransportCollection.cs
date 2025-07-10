using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business
{
	public class DeclarationTransportCollection : TransportCollection
	{
		public DeclarationTransportCollection(BaseJobDeclaration parent)
		: base(parent)
		{
			this.declaration = parent;
		}
		readonly BaseJobDeclaration declaration;

		#region Overrides

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			var transport = (Transport)bizO;
			declaration?.OnTransportRemoved(transport);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			Transport transport = (Transport)child;
			base.SetDefaultsForNewChild(transport);

			int nextLeg = 1;
			foreach (Transport existingTransport in this)
			{
				if (existingTransport.JW_LegOrder >= nextLeg)
				{
					nextLeg = existingTransport.JW_LegOrder + 1;
				}
			}

			transport.JW_LegOrder = (ZByte)((short)nextLeg);
		}

		#endregion
	}
}
