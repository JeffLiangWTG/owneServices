using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusVehicleCollection<out TCusVehicle, out TJobComInvoiceLine> : IDependentBusinessObjectCollection
		where TCusVehicle : CusVehicle
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		new TJobComInvoiceLine Master { get; }
		void Load();
		void RemoveAndDelete(BusinessObject elementToDelete);
		void RemoveAndDeleteAll();
		TCusVehicle AddNew(Type bizOType);
		new TCusVehicle AddNew();
		new TCusVehicle this[int index] { get; }
		void MarkAsNeedingValidation();
		event CollectionCountChangedEventHandler CountChanged;
	}

	public class CusVehicleCollection<TCusVehicle, TJobComInvoiceLine> : DependentBusinessObjectCollection<TCusVehicle, TJobComInvoiceLine>, ICusVehicleCollection<TCusVehicle, TJobComInvoiceLine>
		where TCusVehicle : CusVehicle
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		public CusVehicleCollection(TJobComInvoiceLine master) : base(master)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			if (Master.VehicleRelationship == VehicleRelationshipType.None)
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusVehicleSchema.CVH_ParentID; }
		}

		protected override bool AllowNewCore => Master.VehicleRelationship switch
		{
			VehicleRelationshipType.One => Count == 0,
			VehicleRelationshipType.Many => true,
			_ => false
		};
	}
}
