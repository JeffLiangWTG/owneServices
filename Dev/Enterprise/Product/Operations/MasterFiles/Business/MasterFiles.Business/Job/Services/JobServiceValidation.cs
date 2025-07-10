//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobServiceValidation
//
//    This class should be used for overriding validation in AutoJobServiceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceValidation : AutoJobServiceValidation
	{
		public JobServiceValidation(AutoJobService parent)
			: base(parent)
		{
			Service = (JobService)parent;
		}

		readonly JobService Service;

		#region CheckES_ServiceCode

		protected override void CheckES_ServiceCode()
		{
			base.CheckES_ServiceCode();
			MandatoryValidation.CheckEntered(Service.ES_ServiceCodeInfo);

			if (Service.Parent != null)
			{
				ListValidation.ErrorIfInvalidCode(Service.ES_ServiceCodeInfo, Service.Lookups.JobServiceType_List);
			}

			if (!Service.ES_ServiceCodeInfo.HasErrors() && IsDuplicateCode)
			{
				Service.ES_ServiceCodeInfo.AddError(Res.GetString("7155b1fd-d892-4fd9-a0cb-de901c1a4fca", "Can't have more than one {0} type", Service.ES_ServiceCode));
			}
		}

		bool IsDuplicateCode =>
			!Service.HaveServiceId &&
			Service.ServiceTypeNeedsToBeUnique &&
			Service.Parent != null &&
			Service.Parent.Services.Cast<JobService>().Any(otherService => otherService != Service && !otherService.HaveServiceId && otherService.ES_ServiceCode == Service.ES_ServiceCode);

		#endregion

		#region ES_ServiceRate

		protected override void CheckES_ServiceRate()
		{
			base.CheckES_ServiceRate();

			ValidateES_RX_NKServiceRateCurrency();
			ValidateES_MeasurementBasis();
		}

		#endregion

		#region ES_RX_NKServiceRateCurrency

		protected override void CheckES_RX_NKServiceRateCurrency()
		{
			base.CheckES_RX_NKServiceRateCurrency();

			ListValidation.ErrorIfInvalidCode(Service.ES_RX_NKServiceRateCurrencyInfo, Service.Lookups.ServiceRateCurrencies);
			if (!Service.ES_ServiceRate.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Service.ES_RX_NKServiceRateCurrencyInfo);
			}
		}

		#endregion

		#region ES_MeasurementBasis

		protected override void CheckES_MeasurementBasis()
		{
			base.CheckES_MeasurementBasis();

			ListValidation.ErrorIfInvalidCode(Service.ES_MeasurementBasisInfo, Service.Lookups.MeasurementBasisList);
			if (!Service.ES_ServiceRate.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Service.ES_MeasurementBasisInfo);
			}
		}

		#endregion
	}
}
