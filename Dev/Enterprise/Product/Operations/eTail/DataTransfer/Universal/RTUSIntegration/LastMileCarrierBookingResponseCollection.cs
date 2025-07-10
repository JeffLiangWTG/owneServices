using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.eTail.Integration;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class LastMileCarrierBookingResponseCollection : List<ILastMileCarrierBookingResponse>, ILastMileCarrierBookingResponseCollection
	{
		public bool HasError => this.Any(i => !i.Successful);

		public string ErrorMessage
		{
			get
			{
				var failedResponses = this.Where(i => !i.Successful);
				var sb = new StringBuilder();

				failedResponses?.ForEach((r) =>
				{
					if (!string.IsNullOrWhiteSpace(r.ErrorMessage))
					{
						sb.AppendLine(r.ErrorMessage.Trim());
					}
				});

				return sb.ToString().Trim();
			}
		}
	}
}
