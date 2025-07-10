using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class SundryChargesJobNumberTarget : NumberGeneratorTarget
	{
		protected override int GetMaxLengthCore()
		{
			return JobSundryChargesSchema.D4_JobNumber.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("a129ab65-61d0-4f99-8196-094033f2311d", "job number");
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(AgencyRegistry.Instance.SundryJobNumberCustomisation);
		}

		public override string NumberCustomisationLocation
		{
			get
			{
				return (NoResString)"Liner & Agency -> Sundry Charges -> " + // points to non-localised registry entry
				Res.GetString("70E84CC0-1B00-45fe-871B-19C24830E20B", "Job Number Customization");
			}
		}
	}
}
