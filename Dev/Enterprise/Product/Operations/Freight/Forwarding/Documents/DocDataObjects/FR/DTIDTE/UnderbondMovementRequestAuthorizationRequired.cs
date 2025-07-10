using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	internal class UnderbondMovementRequestAuthorizationRequired : DocDataObject
	{
		public UnderbondMovementRequestAuthorizationRequired()
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

		#region IsVeterinarian

		public ZBool IsVeterinarian
		{
			get => ObjectValue == Codes.Veterinarian;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Veterinarian;
				}
				else if (IsVeterinarian)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsVeterinarianInfo => GetZPropertyInfo(nameof(IsVeterinarian));

		#endregion

		#region IsPhytosanitary

		public ZBool IsPhytosanitary
		{
			get => ObjectValue == Codes.Phytosanitary;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Phytosanitary;
				}
				else if (IsPhytosanitary)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsPhytosanitaryInfo => GetZPropertyInfo(nameof(IsPhytosanitary));

		#endregion

		void ObjectValueRefreshBinding()
		{
			ObjectValueInfo.RefreshBinding();

			IsVeterinarianInfo.RefreshBinding();
			IsPhytosanitaryInfo.RefreshBinding();
		}

		public static class Codes
		{
			public const string Veterinarian = "V";
			public const string Phytosanitary = "P";
		}
	}
}
