using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StaffCodeCalculator
	{
		public StaffCodeCalculator(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			this.factory = factory;
		}

		ZString[] allLetterSets;
		ZString[] firstLastLetterSets;

		public string GetMostAppropriateStaffInitial(string staffName)
		{
			var proposedInitial = GetMostAppropriateStaffInitialCore(staffName);
			OnInitialsFound(proposedInitial);
			return proposedInitial;
		}

		protected virtual void OnInitialsFound(string proposedInitial)
		{
		}

		string GetMostAppropriateStaffInitialCore(string staffName)
		{
			staffName = staffName.ToUpper();
			if (CalculateLetterSets(staffName))
			{
				var potentialInitials = new List<string>();

				if (allLetterSets.Length == 3)
				{
					var initials = allLetterSets[0].Left(1) + allLetterSets[1].Left(1) + allLetterSets[2].Left(1);
					potentialInitials.Add(initials);
				}

				AddInitials(potentialInitials, new[] { new[] { 0 }, new[] { 0 } });
				AddInitials(potentialInitials, new[] { new[] { 0 }, new[] { 0, 1 } });
				AddInitials(potentialInitials, new[] { new[] { 0 }, new[] { 0, 2 } });
				AddInitials(potentialInitials, new[] { new[] { 0, 1 }, new[] { 0 } });
				AddInitials(potentialInitials, new[] { new[] { 0, 2 }, new[] { 0 } });

				foreach (var initial in potentialInitials)
				{
					if (!CodeExists(initial))
					{
						return initial;
					}
				}

				// Start to fall back to numbers
				// First - try 2 initials and the numbers 1 - 0
				string firstInitial = firstLastLetterSets[0].Left(1);
				string lastInitial = firstLastLetterSets[1].Left(1);
				var prefix = firstInitial + lastInitial;
				for (var counter = 1; counter <= 10; counter++)
				{
					var initial = prefix + (counter % 10).ToString();
					if (!CodeExists(initial))
					{
						return initial;
					}
				}

				// Second - then try 1 initial and the numbers 01-00
				prefix = firstInitial;
				for (var counter = 1; counter <= 100; counter++)
				{
					var initial = prefix + (counter % 100).ToString().PadLeft(2, '0');
					if (!CodeExists(initial))
					{
						return initial;
					}
				}
			}
			return string.Empty;
		}

		readonly BusinessObjectFactory factory;

		/// <summary>
		/// calculates the set of letters (either two or three) and returns true if suitable letters have been extracted
		/// </summary>
		/// <param name="staffName"></param>
		/// <returns></returns>
		bool CalculateLetterSets(string staffName)
		{
			var all = new List<ZString>();
			foreach (var word in staffName.Split(' '))
			{
				var letterSet = GetLetterSet(word);
				if (letterSet.Length > 0)
				{
					all.Add(letterSet);
				}
			}
			while (all.Count > 3)
			{
				all.RemoveAt(3);
			}
			allLetterSets = all.ToArray();

			if (allLetterSets.Length >= 2)
			{
				firstLastLetterSets = new[] { allLetterSets[0], allLetterSets[allLetterSets.Length - 1] };
			}
			return allLetterSets.Length >= 2;
		}

		/// <summary>
		/// Letter sets consist of the first letter, first consonant thereafter and the last consonant
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>
		static string GetLetterSet(string word)
		{
			var letterSet = string.Empty;
			for (var i = 0; i < word.Length; i++)
			{
				var letter = word[i];
				// y is considered a vowel unless it is the first letter
				if ("BCDFGHJKLMNPQRSTVWXZ".IndexOf(letter) > -1 || (i == 0 && "AEIOUY".IndexOf(letter) > -1))
				{
					letterSet += letter;
				}
			}
			return letterSet.Length > 3 ? letterSet.Remove(2, letterSet.Length - 3) : letterSet;
		}

		protected virtual bool CodeExists(string staffCode)
		{
			return factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode) != null;
		}

		void AddInitials(ICollection<string> potentialInitials, int[][] positionsInEachWord)
		{
			var initial = string.Empty;
			for (var wordNumber = 0; wordNumber <= positionsInEachWord.GetLength(0); wordNumber++)
			{
				if (wordNumber >= firstLastLetterSets.Length)
				{
					continue;
				}

				var letterSet = firstLastLetterSets[wordNumber];
				foreach (var letterPosition in positionsInEachWord[wordNumber])
				{
					if (letterPosition < letterSet.Length)
					{
						initial += letterSet[letterPosition];
					}
				}
			}

			if (initial.Length > 0 && !potentialInitials.Contains(initial))
			{
				potentialInitials.Add(initial);
			}
		}

		#region GetFirstAvailableUniqueCode

		const string ReadableCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		protected virtual string ExtraCharacters { get { return "0123456789!\"#$%&()*+,-./:;<=>@[\\]^_`{|}~"; } } // All eligible symbols for code (33-127 except single quote (') and question (?))
		internal string AllCharacters { get { return ReadableCharacters + ExtraCharacters; } }

		public ZString GetNextAvailableUniqueCode(string initialCode = "")
		{
			string code = ExtraCharacters.IndexOfAny(initialCode.ToCharArray()) < 0
							? GetNextCode(ReadableCharacters, initialCode)
							: string.Empty;

			if (string.IsNullOrEmpty(code))
			{
				code = GetNextCode(AllCharacters, initialCode);
			}
			if (!string.IsNullOrEmpty(code) && CodeExists(code))
			{
				code = GetNextAvailableUniqueCode(code);
			}

			OnInitialsFound(code);

			return new ZString(code);
		}

		protected virtual string GetNextCode(string characters, string initialCode)
		{
			const string sql = "select dbo.GetNextAvailableUniqueStaffCode(@Characters, @InitialCode)";

			string result;
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@Characters", System.Data.SqlDbType.VarChar, characters);
				cmd.AddParameter("@InitialCode", System.Data.SqlDbType.Char, initialCode);
				result = cmd.ExecuteScalar().ToString().Trim();
			}

			return result;
		}

		#endregion
	}
}
