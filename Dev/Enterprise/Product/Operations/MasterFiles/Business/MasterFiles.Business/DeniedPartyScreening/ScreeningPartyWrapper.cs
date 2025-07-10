using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ScreeningPartyWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ScreeningPartyWrapper(ScreeningParty screeningParty)
			: base()
		{
			WrappedScreeningParty = screeningParty;
		}

		public readonly ScreeningParty WrappedScreeningParty;

		public ZGuid Key
		{
			get { return WrappedScreeningParty.Key; }
		}

		#region Properties

		public ZString ScreeningStatus
		{
			get { return WrappedScreeningParty.CurrentScreeningStatus; }
		}
		public ZPropertyInfo ScreeningStatusInfo { get { return GetZPropertyInfo(nameof(ScreeningStatus)); } }

		public ZString ScreeningStatusDescription
		{
			get { return ScreeningStatusesList.GetDescriptionFromCode(WrappedScreeningParty.CurrentScreeningStatus); }
		}
		public ZPropertyInfo ScreeningStatusDescriptionInfo { get { return GetZPropertyInfo(nameof(ScreeningStatusDescription)); } }

		public ZString Code
		{
			get { return WrappedScreeningParty.Code; }
		}
		public ZPropertyInfo CodeInfo { get { return GetZPropertyInfo(nameof(Code)); } }

		public ZString OrgCode
		{
			get { return WrappedScreeningParty.OrgCode; }
		}
		public ZPropertyInfo OrgCodeInfo { get { return GetZPropertyInfo(nameof(OrgCode)); } }

		public ZString ParentsDescription
		{
			get { return WrappedScreeningParty.ParentsDescription; }
		}
		public ZPropertyInfo ParentsDescriptionInfo { get { return GetZPropertyInfo(nameof(ParentsDescription)); } }

		#endregion

		#region Implementation

		public CodeDescriptionPairList ScreeningStatusesList
		{
			get { return fScreeningStatusesList ?? (fScreeningStatusesList = new ScreeningStatusesList()); }
		}
		CodeDescriptionPairList fScreeningStatusesList;

		#endregion

	}
}
