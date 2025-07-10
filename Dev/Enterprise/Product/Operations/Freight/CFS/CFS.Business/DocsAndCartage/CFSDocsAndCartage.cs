using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSDocsAndCartage : JobDocsAndCartage
	{
		public CFSDocsAndCartage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ParentType = typeof(CFSShipment);
		}

		#region Related Business Objects

		[ChildEditable(true)]
		public new CFSServiceDependentCollection Services
		{
			get { return (CFSServiceDependentCollection)base.Services; }
		}

		protected override JobServiceDependentCollection GetNewServiceCollection()
		{
			return new CFSServiceDependentCollection(this, Factory);
		}

		[ChildEditable(true)]
		public new CFSRequiredDocumentDependentCollection RequiredDocuments
		{
			get { return (CFSRequiredDocumentDependentCollection)base.RequiredDocuments; }
		}

		protected override JobRequiredDocumentDependentCollection GetNewRequiredDocumentCollection()
		{
			return new CFSRequiredDocumentDependentCollection(this, Factory);
		}

		#endregion

		#region Properties

		#region JP_LCLAvailable

		[BusinessObjectTestExclude]
		public override ZDateTime JP_LCLAvailable
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JP_LCLAvailable; }
			set
			{
				base.JP_LCLAvailable = value;

				if (JP_LCLAvailable.IsValid)
				{
					JP_LCLStorageCommences = LCLStorageStartDate(value);
				}
			}
		}

		#endregion

		#region UpdateLCLAvailableDate

		public void UpdateLCLAvailableDate(ZDateTime unPackDate)
		{
			if (unPackDate.IsValid)
			{
				JP_LCLAvailable = NextAvailableDate(unPackDate);
			}
		}

		public ZGuid ClientForStorage;

		#endregion

		#endregion

		#region Validation Override

		protected override JobDocsAndCartageValidation GetNewValidation()
		{
			return new CFSDocsAndCartageValidation(this);
		}

		public new CFSDocsAndCartageValidation Validation
		{
			get { return (CFSDocsAndCartageValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		ZDateTime NextAvailableDate(ZDateTime unPackDate)
		{
			if (unPackDate.IsValidSmallDateTime && Parent != null && Parent.Shipment != null)
			{
				var dateCalculator = new DateCalculator(Factory, GlbDepartment.CurrentDepartment.PK, GlbBranch.CurrentBranch.PK);
				string transportMode = Parent.Shipment.JS_TransportMode;
				bool hasDangerousGoods = Parent.Shipment.OuterPackLines.HasDangerousGoods;

				return dateCalculator.CalculateDate(unPackDate.ToDateTime(), DatesToCalculate.Available, transportMode, hasDangerousGoods);
			}

			return ZDateTime.Empty;
		}

		ZDateTime LCLStorageStartDate(ZDateTime availableDate)
		{
			ZDateTime result = ZDateTime.Empty;

			if (availableDate.IsValidSmallDateTime && Parent != null && Parent.Shipment != null)
			{
				var dateCalculator = new DateCalculator(Factory, GlbDepartment.CurrentDepartment.PK, GlbBranch.CurrentBranch.PK);
				string transportMode = Parent.Shipment.JS_TransportMode;
				bool hasDangerousGoods = Parent.Shipment.OuterPackLines.HasDangerousGoods;
				int freeDays = 0;
				var consignee = Parent.Shipment.Consignee;

				if (consignee != null)
				{
					freeDays = GetFreeDaysFromOrgByTransportMode(consignee, transportMode);
				}

				if (freeDays == 0 && !ClientForStorage.IsEmpty)
				{
					OrgHeader client = Factory.Load<OrgHeader>(ClientForStorage);

					if (client != null)
					{
						freeDays = GetFreeDaysFromOrgByTransportMode(client, transportMode);
					}
				}

				result = dateCalculator.CalculateDate(availableDate.ToDateTime(), DatesToCalculate.Storage, transportMode, hasDangerousGoods, freeDays);
			}

			return result;
		}

		int GetFreeDaysFromOrgByTransportMode(OrgHeader org, string mode)
		{
			return mode == Constants.TransportModes.Air ? org.MiscServ.OM_IMAirDepotFreeDays : org.MiscServ.OM_IMSeaDepotFreeDays;
		}

		#endregion
	}
}
