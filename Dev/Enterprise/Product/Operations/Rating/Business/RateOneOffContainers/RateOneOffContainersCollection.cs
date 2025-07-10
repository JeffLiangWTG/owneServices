using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateOneOffContainersCollection : RateOneOffContainersCollection<RateOneOffContainers>
	{
		public RateOneOffContainersCollection(RateOneOffShipment master, bool isLooseCargo)
			: base(master, isLooseCargo)
		{
		}
	}

	public abstract class RateOneOffContainersCollection<T> : DependentBusinessObjectCollection<T, RateOneOffShipment>
		where T : RateOneOffContainers
	{
		public RateOneOffContainersCollection(RateOneOffShipment master, bool isLooseCargo)
			: base(master)
		{
		}

		#region Delete

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			// We set the parent here for event logging purposes
			((RateOneOffContainers)elementToDelete).Parent = Master;
			base.RemoveAndDelete(elementToDelete);
		}

		#endregion
	}
}

