using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerBusinessObjectFinder<T> : MatchingBusinessObjectFinder<Container, T> where T : CommonContainer
	{
		public ContainerBusinessObjectFinder(Container dataObject)
			: base(dataObject)
		{
		}

		protected override T FindCore(IEnumerable<T> businessObjects)
		{
			T result = null;

			var containerNumber = dataObject.ContainerNumber.GetValueOrDefault();

			if (!containerNumber.IsEmpty)
			{
				result = businessObjects.FirstOrDefault(container => container.JC_ContainerNum.ToUpper() == containerNumber.ToUpper());
			}

			return result;
		}
	}
}
