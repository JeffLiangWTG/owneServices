using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	internal class UnderbondMovementRequestAuthorizationResponseType : DocDataObject
	{
		public UnderbondMovementRequestAuthorizationResponseType()
		{
		}

		#region ObjectValue

		public ZString ObjectValue
		{
			get => objectValue;
			internal set
			{
				if (SetNonPersistentPropertyValue(ObjectValueInfo, ref objectValue, value))
				{
					Validate(ObjectValueInfo);
				}
			}
		}
		ZString objectValue;
		public ZPropertyInfo ObjectValueInfo => GetZPropertyInfo(nameof(ObjectValue));

		#endregion

		#region IsEmpty

		public ZBool IsEmpty
		{
			get => ObjectValue.IsEmpty;
		}

		#endregion

		#region IsAgreement

		public ZBool IsAgreement
		{
			get => ObjectValue == Codes.Agreement;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Agreement;
				}
				else if (IsAgreement)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsAgreementInfo => GetZPropertyInfo(nameof(IsAgreement));

		#endregion

		#region IsAgreementWithReservations

		public ZBool IsAgreementWithReservations
		{
			get => ObjectValue == Codes.AgreementWithReservations;
			set
			{
				if (value)
				{
					ObjectValue = Codes.AgreementWithReservations;
				}
				else if (IsAgreementWithReservations)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsAgreementWithReservationsInfo => GetZPropertyInfo(nameof(IsAgreementWithReservations));

		#endregion

		#region IsNoResponse

		public ZBool IsNoResponse
		{
			get => ObjectValue == Codes.NoResponse;
			set
			{
				if (value)
				{
					ObjectValue = Codes.NoResponse;
				}
				else if (IsNoResponse)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsNoResponseInfo => GetZPropertyInfo(nameof(IsNoResponse));

		#endregion

		#region IsRefused

		public ZBool IsRefused
		{
			get => ObjectValue == Codes.Refused;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Refused;
				}
				else if (IsRefused)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsRefusedInfo => GetZPropertyInfo(nameof(IsRefused));

		#endregion

		void ObjectValueRefreshBinding()
		{
			ObjectValueInfo.RefreshBinding();

			IsAgreementInfo.RefreshBinding();
			IsAgreementWithReservationsInfo.RefreshBinding();
			IsNoResponseInfo.RefreshBinding();
			IsRefusedInfo.RefreshBinding();
		}

		public static class Codes
		{
			public const string Agreement = "AC";
			public const string AgreementWithReservations = "PAR";
			public const string NoResponse = "NO";
			public const string Refused = "DE";
		}
	}
}
