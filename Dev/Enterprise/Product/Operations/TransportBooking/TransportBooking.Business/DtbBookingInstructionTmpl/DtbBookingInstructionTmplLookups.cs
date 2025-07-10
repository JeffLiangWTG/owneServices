using Enterprise.ZArchitecture.Core;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionTmplLookups : Common.DtbBookingInstructionTmplLookups
	{
		public DtbBookingInstructionTmplLookups(DtbBookingInstructionTmpl parent)
			: base(parent)
		{
		}

		public LocalCartageJobOrgTypeList OrganisationTypes
		{
			get { return LocalCartageJobOrgTypeList.Instance; }
		}

		public CodeDescriptionPairList InstructionTypes
		{
			get { return BindToLists.InstructionTypes; }
		}

		public CodeDescriptionPairList DropModes
		{
			get { return BindToLists.DropModes; }
		}

		public CodeDescriptionPairList DefaultPackageTypes
		{
			get { return BindToLists.DefaultPackageTypes; }
		}

		BindToLists BindToLists
		{
			get { return Factory.GetCachedValue("TransportBookings|BindToLists", () => new BindToLists(Factory)); }
		}
	}
}
