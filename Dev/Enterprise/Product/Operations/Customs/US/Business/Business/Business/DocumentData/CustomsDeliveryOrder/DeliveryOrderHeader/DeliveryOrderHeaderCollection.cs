using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderHeaderCollection : DependentCusAddInfoCollection<DeliveryOrderHeader, JobDeclaration>
	{
		public DeliveryOrderHeaderCollection(JobDeclaration master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDeliveryOrderHeader)
		{
		}

		public bool HasAtLeastOneSelectedForPrinting
		{
			get
			{
				foreach (DeliveryOrderHeader header in this)
				{
					if (header.US_ShouldPrint)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected override void SetDefaultsForNewChild(CargoWise.EntityFramework.BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (DeliveryOrderHeader)child;
			if (Count == 0)
			{
				CustomsDeliveryOrderDefaulter.Defaulter(Master, newElement);
			}
			CustomsDeliveryOrderDefaulter.DefaultOrderLine(Master, newElement);
		}
	}
}
