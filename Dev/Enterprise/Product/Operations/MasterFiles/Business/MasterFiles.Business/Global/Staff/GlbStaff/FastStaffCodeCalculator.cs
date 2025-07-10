using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class FastStaffCodeCalculator : StaffCodeCalculator
	{
		public FastStaffCodeCalculator(BusinessObjectFactory factory) : base(factory)
		{
		}

		public HashSet<string> AvailableStaffCodes
		{
			get
			{
				var codes = MemoryCache.Default.Get("AvailableStaffCodes") as HashSet<string>;

				if (codes == null || codes.Count == 0)
				{
					var policy = new CacheItemPolicy();
					policy.AbsoluteExpiration = new DateTimeOffset(GetCacheExpiry());
					codes = GetAvailableCodes();
					MemoryCache.Default.Add("AvailableStaffCodes", codes, policy);
				}

				return codes;
			}
		}

		protected override string ExtraCharacters { get { return Env.Registry.ScimCodeGenerationCharacters; } }

		protected virtual DateTime GetCacheExpiry()
		{
			return ZDateTime.UtcNow.AddHours(1).ToDateTime();
		}

		protected virtual HashSet<string> GetAvailableCodes()
		{
			var usedCodes = new List<string>();
			using (var reader = Db.Connection.Command($"select GS_Code from dbo.GlbStaff").ExecuteReader())
			{
				while (reader.Read())
				{
					usedCodes.Add(reader.GetString(0));
				}
			}

			var availableStaffCodes = new HashSet<string>();
			for (int first = 0; first < AllCharacters.Length; first++)
			{
				for (int second = 0; second < AllCharacters.Length; second++)
				{
					for (int third = 0; third < AllCharacters.Length; third++)
					{
						var newCode = string.Concat(new char[] { AllCharacters[first], AllCharacters[second], AllCharacters[third] });

						if (!usedCodes.Contains(newCode))
						{
							availableStaffCodes.Add(newCode);
						}
					}
				}
			}

			return availableStaffCodes;
		}

		protected override bool CodeExists(string staffCode)
		{
			return !AvailableStaffCodes.Contains(staffCode);
		}

		protected override void OnInitialsFound(string proposedInitial)
		{
			base.OnInitialsFound(proposedInitial);

			if (!string.IsNullOrEmpty(proposedInitial))
			{
				AvailableStaffCodes.Remove(proposedInitial);
			}
		}

		protected override string GetNextCode(string characters, string initialCode)
		{
			var proposedCode = new StringBuilder();

			int index1 = 0;
			int index2 = 0;
			int index3 = 0;

			if (!string.IsNullOrEmpty(initialCode))
			{
				index1 = characters.IndexOf(initialCode[0]);
				index2 = characters.IndexOf(initialCode[1]);
				index3 = characters.IndexOf(initialCode[2]);
			}

			for (int i = index1; i < characters.Length; i++)
			{
				for (int j = index2; j < characters.Length; j++)
				{
					for (int k = index3; k < characters.Length; k++)
					{
						proposedCode.Append(characters[i]);
						proposedCode.Append(characters[j]);
						proposedCode.Append(characters[k]);
						var code = proposedCode.ToString();
						if (AvailableStaffCodes.Contains(code))
						{
							return code;
						}

						proposedCode.Clear();
					}
				}
			}

			return string.Empty;
		}

		public string GetBestCode(GlbStaff staff)
		{
			string code = string.Empty;

			try
			{
				bool isSafe = false;

				for (int i = 0; i < 5; i++)
				{
					isSafe = mutex.WaitOne();
					if (isSafe)
					{
						break;
					}

					Thread.Sleep(100);
				}

				if (isSafe)
				{
					if (staff == null)
					{
						return string.Empty;
					}

					if (!staff.GS_FriendlyName.IsEmpty)
					{
						code = GetMostAppropriateStaffInitial(staff.GS_FriendlyName);
					}

					if (string.IsNullOrEmpty(code) && !staff.GS_FullName.IsEmpty)
					{
						code = GetMostAppropriateStaffInitial(staff.GS_FullName);
					}

					if (string.IsNullOrEmpty(code))
					{
						code = GetNextAvailableUniqueCode();
					}
				}
			}
			finally
			{
				mutex.ReleaseMutex();
			}

			return code;
		}

		static readonly Mutex mutex = new Mutex();
	}
}
