using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AutoRefCarrierConsortium.Schema.RG_Code), DescriptionProperty(AutoRefCarrierConsortium.Schema.RG_Code)]
	public class RefCarrierConsortium : AutoRefCarrierConsortium
	{
		public RefCarrierConsortium(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		#region RG_OH
		[List("Lookups.Headers")]
		public override ZGuid RG_OH
		{
			get
			{
				return base.RG_OH;
			}
			set
			{
				base.RG_OH = value;
			}
		}
		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("B52FBA57-82B2-4C0E-ADF5-72C924472312", "Vessel Consortium - {0}", CalculateShortcutName());

		#endregion

		#region Relcated Business Objects

		#region Organisations

		[ChildEditable(true)]
		public OrganisationManyToManyCollection OrgHeaders
		{
			get
			{
				if (fOrgHeaders == null)
				{
					fOrgHeaders = new OrganisationManyToManyCollection(this);
					RegisterEditableChildObject(fOrgHeaders);
				}
				return fOrgHeaders;
			}
		}

		OrganisationManyToManyCollection fOrgHeaders;

		#endregion

		#region Vessels

		[ChildEditable(true)]
		public RefCarrierConsortiumRefVesselDependentCollection Vessels
		{
			get
			{
				if (fVessels == null)
				{
					fVessels = new RefCarrierConsortiumRefVesselDependentCollection(this, Factory);
					fVessels.Load();
					RegisterEditableChildObject(fVessels);
				}

				return fVessels;
			}
		}
		RefCarrierConsortiumRefVesselDependentCollection fVessels;

		#endregion

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
