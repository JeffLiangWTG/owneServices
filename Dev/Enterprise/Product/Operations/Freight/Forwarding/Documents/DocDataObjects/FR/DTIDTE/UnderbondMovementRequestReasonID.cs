using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	internal class UnderbondMovementRequestReasonID : DocDataObject
	{
		public UnderbondMovementRequestReasonID()
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

		#region IsDE

		public ZBool IsDE
		{
			get => ObjectValue == Codes.DE;
			set
			{
				if (value)
				{
					ObjectValue = Codes.DE;
				}
				else if (IsDE)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsDEInfo => GetZPropertyInfo(nameof(IsDE));

		#endregion

		#region IsCV

		public ZBool IsCV
		{
			get => ObjectValue == Codes.CV;
			set
			{
				if (value)
				{
					ObjectValue = Codes.CV;
				}
				else if (IsCV)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsCVInfo => GetZPropertyInfo(nameof(IsCV));

		#endregion

		#region IsPH

		public ZBool IsPH
		{
			get => ObjectValue == Codes.PH;
			set
			{
				if (value)
				{
					ObjectValue = Codes.PH;
				}
				else if (IsPH)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsPHInfo => GetZPropertyInfo(nameof(IsPH));

		#endregion

		#region IsST

		public ZBool IsST
		{
			get => ObjectValue == Codes.ST;
			set
			{
				if (value)
				{
					ObjectValue = Codes.ST;
				}
				else if (IsST)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsSTInfo => GetZPropertyInfo(nameof(IsST));

		#endregion

		#region IsTR

		public ZBool IsTR
		{
			get => ObjectValue == Codes.TR;
			set
			{
				if (value)
				{
					ObjectValue = Codes.TR;
				}
				else if (IsTR)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsTRInfo => GetZPropertyInfo(nameof(IsTR));

		#endregion

		#region IsCD

		public ZBool IsCD
		{
			get => ObjectValue == Codes.CD;
			set
			{
				if (value)
				{
					ObjectValue = Codes.CD;
				}
				else if (IsCD)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsCDInfo => GetZPropertyInfo(nameof(IsCD));

		#endregion

		#region IsCS

		public ZBool IsCS
		{
			get => ObjectValue == Codes.CS;
			set
			{
				if (value)
				{
					ObjectValue = Codes.CS;
				}
				else if (IsCS)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsCSInfo => GetZPropertyInfo(nameof(IsCS));

		#endregion

		#region IsIZ

		public ZBool IsIZ
		{
			get => ObjectValue == Codes.IZ;
			set
			{
				if (value)
				{
					ObjectValue = Codes.IZ;
				}
				else if (IsIZ)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsIZInfo => GetZPropertyInfo(nameof(IsIZ));

		#endregion

		void ObjectValueRefreshBinding()
		{
			ObjectValueInfo.RefreshBinding();

			IsDEInfo.RefreshBinding();
			IsCVInfo.RefreshBinding();
			IsPHInfo.RefreshBinding();
			IsSTInfo.RefreshBinding();
			IsTRInfo.RefreshBinding();
			IsCDInfo.RefreshBinding();
			IsCSInfo.RefreshBinding();
			IsIZInfo.RefreshBinding();
		}

		public static class Codes
		{
			public const string DE = "DE";
			public const string CV = "CV";
			public const string PH = "PH";
			public const string ST = "ST";
			public const string TR = "TR";
			public const string CS = "CS";
			public const string CD = "CD";
			public const string IZ = "IZ";
		}
	}
}
