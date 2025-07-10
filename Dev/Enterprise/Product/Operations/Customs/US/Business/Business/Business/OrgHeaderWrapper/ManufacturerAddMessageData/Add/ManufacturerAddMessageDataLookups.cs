using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ManufacturerAddMessageDataLookups : ZLookups
	{
		public ManufacturerAddMessageDataLookups(ManufacturerAddMessageData parent)
			: base(parent) { }

		public USCCountryCollection CountryList
		{
			get { return new USCCountryCollection(Factory); }
		}

		public OrgAddressDependentCollection Addresses
		{
			get { return Parent.wrapper.organisation.Addresses; }
		}

		#region Implementation

		protected new ManufacturerAddMessageData Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ManufacturerAddMessageData)base.Parent; }
		}

		#endregion
	}
}
