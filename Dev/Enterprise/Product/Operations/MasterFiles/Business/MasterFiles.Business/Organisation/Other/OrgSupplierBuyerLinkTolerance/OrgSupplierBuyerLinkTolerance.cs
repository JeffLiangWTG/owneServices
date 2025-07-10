using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierBuyerLinkTolerance : AutoOrgSupplierBuyerLinkTolerance, IOrgSupplierBuyerLinkTolerance
	{
		public OrgSupplierBuyerLinkTolerance(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OLT_TransportMode = Constants.TransportModes.All;
			OLT_PartNumber = ZString.Empty;
		}

		#endregion

		#region Property Overrides

		#region TransportMode

		[List("Lookups.TransportModes")]
		public override ZString OLT_TransportMode
		{
			get => base.OLT_TransportMode;
			set => base.OLT_TransportMode = value;
		}

		#endregion

		#region PartNumber

		[List("Lookups.SupplierParts")]
		public override ZString OLT_PartNumber
		{
			get => base.OLT_PartNumber;
			set => base.OLT_PartNumber = value;
		}

		#endregion

		#endregion

		#region GetExistingOrgSupplierBuyerLinkTolerance

		public static IOrgSupplierBuyerLinkTolerance GetExistingOrgSupplierBuyerLinkTolerance(OrgSupplierBuyerLink supplierBuyerLink, ZString transportMode, ZString partNumber)
		{
			var supplierBuyerLinkTolerances = GetExistingOrgSupplierBuyerLinkTolerances(supplierBuyerLink, transportMode);

			if (supplierBuyerLinkTolerances == null)
			{
				return null;
			}

			var supplierBuyerLinkTolerance = supplierBuyerLinkTolerances.FirstOrDefault(t => t.OLT_TransportMode == transportMode && t.OLT_PartNumber == partNumber);
			if (supplierBuyerLinkTolerance != null)
			{
				return supplierBuyerLinkTolerance;
			}

			if (transportMode != Constants.TransportModes.All)
			{
				supplierBuyerLinkTolerance = supplierBuyerLinkTolerances.FirstOrDefault(t => t.OLT_TransportMode == Constants.TransportModes.All && t.OLT_PartNumber == partNumber);
				if (supplierBuyerLinkTolerance != null)
				{
					return supplierBuyerLinkTolerance;
				}
			}

			if (partNumber != ZString.Empty)
			{
				supplierBuyerLinkTolerance = supplierBuyerLinkTolerances.FirstOrDefault(t => t.OLT_TransportMode == transportMode && t.OLT_PartNumber == ZString.Empty);
				if (supplierBuyerLinkTolerance != null)
				{
					return supplierBuyerLinkTolerance;
				}
			}

			return transportMode == Constants.TransportModes.All || partNumber == ZString.Empty
				? null
				: supplierBuyerLinkTolerances.FirstOrDefault(t => t.OLT_TransportMode == Constants.TransportModes.All && t.OLT_PartNumber == ZString.Empty);
		}

		static OrgSupplierBuyerLinkTolerance[] GetExistingOrgSupplierBuyerLinkTolerances(OrgSupplierBuyerLink supplierBuyerLink, ZString transportMode)
		{
			if (supplierBuyerLink == null)
			{
				return null;
			}

			var transportModeQuery = new ZQuery(OrgSupplierBuyerLinkToleranceSchema.OLT_TransportMode, Constants.TransportModes.All);
			if (transportMode != Constants.TransportModes.All)
			{
				transportModeQuery.AddToFilter(JoinCondition.Or, OrgSupplierBuyerLinkToleranceSchema.OLT_TransportMode, transportMode);
			}

			var query = new ZQuery(OrgSupplierBuyerLinkToleranceSchema.OLT_OL_SupplierBuyerLink, supplierBuyerLink.PK);
			query.AddToFilter(transportModeQuery);
			return supplierBuyerLink.Factory.Load<OrgSupplierBuyerLinkTolerance>(query);
		}

		#endregion
	}
}
