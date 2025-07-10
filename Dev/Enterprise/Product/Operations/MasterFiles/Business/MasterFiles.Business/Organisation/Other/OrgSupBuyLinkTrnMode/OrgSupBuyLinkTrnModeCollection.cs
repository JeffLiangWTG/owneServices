using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupBuyLinkTrnModeCollection : BusinessObjectCollection<OrgSupBuyLinkTrnMode>
	{
		public OrgSupBuyLinkTrnModeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}

	public class OrgSupBuyLinkTrnModeDependentCollection : DependentBusinessObjectCollection<OrgSupBuyLinkTrnMode, OrgSupplierBuyerLink>
	{
		public OrgSupBuyLinkTrnModeDependentCollection(OrgSupplierBuyerLink supplierBuyerLink, BusinessObjectFactory factory)
			: base(supplierBuyerLink, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgSupBuyLinkTrnModeSchema.PF_OL; }
		}

		#region Find

		public OrgSupBuyLinkTrnMode Find(ZString transportMode, ZString containerMode)
		{
			OrgSupBuyLinkTrnMode result = null;
			int overallMatchLevel = 0;

			foreach (OrgSupBuyLinkTrnMode linkMode in Elements)
			{
				int curMatchLevel = 0;

				if (linkMode.PF_TransportMode == transportMode)
				{
					curMatchLevel = (linkMode.PF_ContainerMode == containerMode) ? 4 : 2;
				}
				else if (linkMode.PF_TransportMode == Core.Constants.TransportModes.All)
				{
					curMatchLevel = 1;
				}

				if (curMatchLevel > overallMatchLevel)
				{
					overallMatchLevel = curMatchLevel;
					result = linkMode;
				}
			}

			return result;
		}

		#endregion
	}
}
