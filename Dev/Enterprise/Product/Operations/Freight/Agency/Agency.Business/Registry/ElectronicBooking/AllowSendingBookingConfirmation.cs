using System;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public class AllowSendingBookingConfirmation : AutoAllowSendingBookingConfirmation
	{
		public AllowSendingBookingConfirmation() { }

		public AllowSendingBookingConfirmation(FallbackLevel fallbackLevel) : base(fallbackLevel) { }

		public AllowSendingBookingConfirmation(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory) { }

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AllowSendingBookingConfirmation(fallbackLevel, factory);
		}

		#endregion

		#region Validation

		public override void ValidatePrincipalPK()
		{
			base.ValidatePrincipalPK();

			if (!PrincipalPK.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(PrincipalPKInfo, Principals, ResString.GetMultilingualString("2A65E9C7-A32F-451B-9ABD-312D1F701396", "Please enter a valid Principal Org."));
			}

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(PrincipalPKInfo, checkEmptyValues: true);
			}
		}

		public override void ValidateEnabled()
		{
			base.ValidateEnabled();
			ValidatePrincipalPK();
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
