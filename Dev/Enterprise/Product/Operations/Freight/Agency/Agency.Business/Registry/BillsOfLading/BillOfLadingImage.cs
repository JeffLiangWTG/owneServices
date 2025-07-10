using System;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public class BillOfLadingImage : AutoBillOfLadingImage
	{
		public BillOfLadingImage() { }

		public BillOfLadingImage(FallbackLevel fallbackLevel) : base(fallbackLevel) { }

		public BillOfLadingImage(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory) { }

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillOfLadingImage(fallbackLevel, factory);
		}

		#endregion

		#region Validation

		protected override void CheckImage()
		{
			base.CheckImage();

			if (Image == null)
			{
				AddRowError(Res.GetString("6f63d08a-80ab-4bdf-b25c-9f70236e6515", "Please select an Image."));
			}
		}

		public override void ValidatePrincipalPK()
		{
			base.ValidatePrincipalPK();

			MandatoryValidation.CheckEntered(PrincipalPKInfo, Res.GetString("92c466d0-fe89-4ab3-8e60-277e3888db4c", "Principal Org."));
			ListValidation.ErrorIfInvalidPK(PrincipalPKInfo, Principals, ResString.GetMultilingualString("bad5759d-aac5-467a-8409-26f8a594e5a0", "Please enter a valid principal Org.."));

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(PrincipalPKInfo, Res.GetString("95325f1a-c649-4017-89b9-e817f53eb030", "A principal org. may only appear once in this list."));
			}
		}

		public override void ValidateEnabled()
		{
			base.ValidateEnabled();
			ValidatePrincipalPK();
			CheckImage();
		}

		#endregion

		#region Principals

		public BusinessObjectCollection Principals
		{
			get
			{
				if (principals == null)
				{
					principals = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IShipsAgencyPrincipalCollection>(), CurrentFactory);
				}

				return principals;
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		BusinessObjectCollection principals;

		#endregion
	}
}
