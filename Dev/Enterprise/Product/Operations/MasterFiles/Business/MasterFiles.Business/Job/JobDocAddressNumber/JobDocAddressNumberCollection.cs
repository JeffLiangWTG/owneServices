using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressNumberCollection : DependentBusinessObjectCollection<JobDocAddressNumber, JobDocAddress>
	{
		public JobDocAddressNumberCollection(JobDocAddress master) : base(master) { }

		protected override bool AllowNewCore => Master.SupportsDocAddressNumbers;

		public JobDocAddressNumber Find(ZString numberType, ZString countryCode)
		{
			var findResult = Find(number => number.E2N_NumberType == numberType && number.E2N_RN_NKCountryCode == countryCode);
			return findResult.FirstOrDefault();
		}

		public JobDocAddressNumber FindOrCreate(ZString numberType, ZString countryCode)
		{
			return Find(numberType, countryCode) ?? AddNew(numberType, countryCode);
		}

		public JobDocAddressNumber AddNew(ZString numberType, ZString countryCode)
		{
			JobDocAddressNumber result = null;
			if (!numberType.IsEmpty)
			{
				result = AddNew();
				using (result.SuspendSettingHasChanges())
				{
					result.E2N_NumberType = numberType;
					result.E2N_RN_NKCountryCode = countryCode;
				}
			}
			return result;
		}

		public JobDocAddressNumber FindFirstByNumberType(ZString type)
		{
			return Find(number => number.E2N_NumberType == type).FirstOrDefault();
		}
	}
}
