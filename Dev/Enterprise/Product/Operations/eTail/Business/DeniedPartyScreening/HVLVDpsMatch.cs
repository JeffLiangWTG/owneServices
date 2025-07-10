using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class HVLVDpsMatch : NonPersistentBusinessObject
	{
		public HVLVDpsMatch(DpsResponseWithScreeningParty dpsResponseWithScreeningParty, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(dpsResponseWithScreeningParty, nameof(dpsResponseWithScreeningParty));
			Argument.NotNull(factory, nameof(factory));

			DpsResponseWithScreeningParty = dpsResponseWithScreeningParty;
			ProfileHeaderCollection = new ProfileHeaderCollection(dpsResponseWithScreeningParty.Response, Factory);
		}

		public ProfileHeaderCollection ProfileHeaderCollection { get; }

		public DpsResponseWithScreeningParty DpsResponseWithScreeningParty { get; }

		public ZString PartyName => DpsResponseWithScreeningParty.ScreeningParty.Code;

		public ZString PartyCode => DpsResponseWithScreeningParty.ScreeningParty.OrgCode;

		public ZString Status => newStatus ?? CurrentStatus;
		ZString? newStatus;

		CodeDescriptionPairList fScreeningStatusesList;
		CodeDescriptionPairList cachedStatusesList => fScreeningStatusesList ?? (fScreeningStatusesList = new ScreeningStatusesList());

		public ZString StatusDescription => cachedStatusesList.GetDescriptionFromCode(Status);

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));

		public ZString CurrentStatus => DpsResponseWithScreeningParty.ScreeningParty.CurrentScreeningStatus;

		public ZString Description => DpsResponseWithScreeningParty.ScreeningParty.ParentsDescription;

		public ZInt HighestConfidenceScore => ProfileHeaderCollection.Select(x => x.Score).DefaultIfEmpty(0).Max();

		public ZString CountryCode => DpsResponseWithScreeningParty.ScreeningParty.Header?.CountryCode ?? DpsResponseWithScreeningParty.ScreeningParty.NaturalPerson?.Country;

		public ZBool Cleared
		{
			get => Status == ScreeningStatusesList.Codes.Clear;
			set
			{
				newStatus = ScreeningStatusesList.Codes.Clear;
				StatusInfo.RefreshBinding();
			}
		}

		public ZBool Matched
		{
			get => Status == ScreeningStatusesList.Codes.Matched;
			set
			{
				newStatus = ScreeningStatusesList.Codes.Matched;
				StatusInfo.RefreshBinding();
			}
		}

		public ZInt PotentialMatchesCount => ProfileHeaderCollection.Count;

		public BusinessObject ParentBusinessObject => DpsResponseWithScreeningParty.ScreeningParty.Parent;

		#region Clearing Reason

		ZString clearingReasonTitle;

		public ZString ClearingReasonTitle
		{
			get
			{
				return clearingReasonTitle;
			}
			set
			{
				clearingReasonTitle = value;
				ClearingReasonTitleInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ClearingReasonTitleInfo => GetZPropertyInfo(nameof(ClearingReasonTitle));

		public ZString ClearingReason { get; set; }

		public ZString ClearingReasonText { get; set; }

		#endregion
	}
}
