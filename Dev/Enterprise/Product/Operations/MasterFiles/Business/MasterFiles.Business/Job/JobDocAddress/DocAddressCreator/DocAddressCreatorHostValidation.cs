using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class DocAddressCreatorHostValidation : AutoDocAddressCreatorHostValidation
	{
		public DocAddressCreatorHostValidation(AutoDocAddressCreatorHost parent)
			: base(parent) { }

		#region CheckAddressTypeCode

		protected override void CheckAddressTypeCode()
		{
			base.CheckAddressTypeCode();
			MandatoryValidation.CheckEntered(Parent.AddressTypeCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AddressTypeCodeInfo, Parent.Lookups.AddressTypes);
		}

		#endregion

		#region Implementation

		public new DocAddressCreatorHost Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DocAddressCreatorHost)base.Parent; }
		}

		#endregion
	}
}
