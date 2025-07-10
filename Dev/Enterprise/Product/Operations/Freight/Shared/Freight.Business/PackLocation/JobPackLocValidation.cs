using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.Business
{
	public class JobPackLocValidation : AutoJobPackLocValidation
	{
		public JobPackLocValidation(AutoJobPackLoc parent)
			: base(parent)
		{
		}

		new PackLocation Parent
		{
			get { return base.Parent as PackLocation; }
		}

		protected override void CheckJQ_NoPackages()
		{
			base.CheckJQ_NoPackages();
			CompareValidation.CheckNumberNotNegative(Parent.JQ_NoPackagesInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLocationWhsGuid();
			ValidateLocationString();
		}

		public void ValidateLocationWhsGuid()
		{
			ValidateCalculatedProperty(Parent.LocationWhsGuidInfo);
		}

		protected void CheckLocationWhsGuid()
		{
			if (WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value && !Parent.LocationWhsGuid.IsEmpty && !Parent.LocationWhsGuid.IsValid)
			{
				Parent.LocationWhsGuidInfo.AddError(ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		public void ValidateLocationString()
		{
			ValidateCalculatedProperty(Parent.LocationStringInfo);
		}

		protected void CheckLocationString()
		{
			if (WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value && (!Parent.LocationString.IsEmpty || Parent.LocationWhsGuid.IsValid))
			{
				var helper = ObjectFactory.New<IWhsLocationDataHelper>();
				helper.ValidateLocation(Parent, Parent.LocationStringInfo);
			}
		}
	}
}
