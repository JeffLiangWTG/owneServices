using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class DOTVIN : AutoDOTVIN,
		IDOTVIN
	{
		public DOTVIN(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "DOT Vehicle Details"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();
			return base.CloneInternal(args);
		}

		#region IDOTVIN Members

		ZString IDOTVIN.MakeOfVehicle
		{
			get { return US_DOTMake; }
		}

		ZString IDOTVIN.Model
		{
			get { return US_DOTModel; }
		}

		ZInt IDOTVIN.Year
		{
			get { return US_DOTYear; }
		}

		ZString IDOTVIN.VehicleIdentificationNumber
		{
			get { return US_DOTVIN; }
		}

		ZString IDOTVIN.NHTSARegisteredImporterRINumber
		{
			get { return US_DOTRINo; }
		}

		ZString IDOTVIN.VehicleEligibilityNumber
		{
			get { return US_DOTVEN; }
		}

		#endregion
	}
}
