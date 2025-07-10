using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FastStaffCodeCalculatorTest : StaffCodeCalculatorTest
	{
		public void TestAvailableStaffCodes1()
		{
			AssertCodeGeneration("1234567890@.-_");
		}

		public void TestAvailableStaffCodes2()
		{
			AssertCodeGeneration("123");
		}

		public void TestAvailableStaffCodes3()
		{
			AssertCodeGeneration("@^&(");
		}

		public void TestAvailableStaffCodes4()
		{
			AssertCodeGeneration("");
		}

		void AssertCodeGeneration(string chars)
		{
			Env.Registry.ScimCodeGenerationCharacters = chars;
			var calc = new FastStaffCodeCalculator(Factory);
			var length = calc.AllCharacters.Length;
			AssertEquals(length * length * length, calc.AvailableStaffCodes.Count);

			var allCharacters = "0123456789!\"#$%&()*+,-./:;<=>@[\\]^_`{|}~";
			var badCharacters = allCharacters.Except(chars);

			int total = 26 + chars.Length;
			AssertEquals(total * total * total, calc.AvailableStaffCodes.Count);

			var invalidCodes = calc.AvailableStaffCodes.Where(code => badCharacters.Any(badChar => code.Contains(badChar))).ToList();

			AssertEquals("Should have no codes with disallowed characters. Codes: " + string.Join(" ", invalidCodes.ToArray()), 0, invalidCodes.Count);
		}

		[SnailTest]
		public void TestPerformanceDoesNotDegradeMuch()
		{
			const int num = 500;

			var names = GenerateRandomNames(num);
			var friendlyNames = GenerateRandomNames(num);

			var sw = new Stopwatch();

			var calc = new FastStaffCodeCalculator(Factory);
			sw.Start();

			for (int i = 0; i < num; i++)
			{
				var code1 = calc.GetMostAppropriateStaffInitial(names[i]);
				var code2 = calc.GetMostAppropriateStaffInitial(friendlyNames[i]);
				var code3 = calc.GetNextAvailableUniqueCode();
			}

			sw.Stop();

			var totalInitial = sw.Elapsed.TotalMilliseconds;

			for (int i = 0; i < num; i++)
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_FullName = names[i];
				staff.GS_FriendlyName = friendlyNames[i];
				staff.GS_LoginName = "login" + i;
				staff.AllowEmptyPasswordForNewRecord();
			}

			Factory.Save();

			sw.Reset();

			calc = new FastStaffCodeCalculator(Factory);
			sw.Start();

			for (int i = 0; i < num; i++)
			{
				calc.GetMostAppropriateStaffInitial(names[i]);
				calc.GetMostAppropriateStaffInitial(friendlyNames[i]);
				calc.GetNextAvailableUniqueCode();
			}
			sw.Stop();

			var totalFinal = sw.Elapsed.TotalMilliseconds;

			var diff = totalFinal * 100 / totalInitial;

			AssertLessThan("Should not be slower by more than 20%", diff, 120);
		}

		public void TestMutex()
		{
			string name = "Anton Gorlin";

			var calc = new FastStaffCodeCalculatorForMutexTest(Factory);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = name;
			staff1.GS_LoginName = "xxlogin aaa";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = name;
			staff2.GS_LoginName = "xxlogin bbb";

			string firstCode = string.Empty;
			string secondCode = string.Empty;

			Task task = null;
			calc.InitialsFound += delegate
			{
				task = Task.Run(delegate
				{
					secondCode = calc.GetBestCode(staff2);
				});
			};

			firstCode = calc.GetBestCode(staff1);

			if (task != null)
			{
				task.Wait();
			}

			AssertNotEquals("", firstCode);
			AssertNotEquals("", secondCode);
			AssertNotEquals(secondCode, firstCode);
		}

		class FastStaffCodeCalculatorForMutexTest : FastStaffCodeCalculator
		{
			public FastStaffCodeCalculatorForMutexTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public EventHandler InitialsFound;
			bool eventFired;

			protected override void OnInitialsFound(string proposedInitial)
			{
				if (InitialsFound != null && !eventFired)
				{
					eventFired = true;
					InitialsFound(this, new EventArgs());
				}

				Thread.Sleep(300);
				base.OnInitialsFound(proposedInitial);
			}
		}

		public void TestSameCode()
		{
			string name = "Anton Gorlin";
			var calc = new FastStaffCodeCalculator(Factory);

			var code1 = calc.GetMostAppropriateStaffInitial(name);
			var code2 = calc.GetMostAppropriateStaffInitial(name);

			AssertNotEquals(code1, code2);

			code1 = calc.GetNextAvailableUniqueCode();
			code2 = calc.GetNextAvailableUniqueCode();

			AssertNotEquals(code1, code2);

			code1 = calc.GetNextAvailableUniqueCode("ABC");
			code2 = calc.GetNextAvailableUniqueCode("ABC");

			AssertNotEquals(code1, code2);
		}

		public void TestGetBestCode()
		{
			var calc = new FastStaffCodeCalculator(Factory);

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Anton Gorlin";
			staff.GS_FriendlyName = "Don Antonio";

			var code = calc.GetBestCode(staff);
			AssertEquals("Friendly name goes first", "DAN", code);

			staff.GS_FriendlyName = "";
			code = calc.GetBestCode(staff);
			AssertEquals("Full name goes second", "AGR", code);

			staff.GS_FullName = "";
			code = calc.GetBestCode(staff);
			AssertEquals("Random goes third", "AAA", code);
		}

		public void TestDbHits()
		{
			const int num = 100;

			var names = GenerateRandomNames(num);
			var friendlyNames = GenerateRandomNames(num);
			var calc = new FastStaffCodeCalculator(Factory);

			for (int i = 0; i < num; i++)
			{
				calc.GetMostAppropriateStaffInitial(names[i]);
				calc.GetMostAppropriateStaffInitial(friendlyNames[i]);
				calc.GetNextAvailableUniqueCode();
			}

			AssertEquals(0, Factory.DatabaseLoadCount);
		}

		string[] GenerateRandomNames(int num)
		{
			var names = new string[num];
			const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var rand = new Random();
			int length1, length2;
			for (int i = 0; i < num; i++)
			{
				length1 = rand.Next(3, 8);
				length2 = rand.Next(3, 8);
				var word1 = new StringBuilder();
				var word2 = new StringBuilder();

				for (int j = 0; j < length1; j++)
				{
					word1.Append(letters[rand.Next(letters.Length)]);
				}

				for (int j = 0; j < length2; j++)
				{
					word2.Append(letters[rand.Next(letters.Length)]);
				}

				names[i] = word1.ToString() + " " + word2.ToString();
			}

			return names;
		}

		public void TestCacheWorks()
		{
			var calc = new FastStaffCodeCalculatorForTest(Factory);

			AssertEquals(0, calc.GetAvailableCodesCounter);

			calc.GetMostAppropriateStaffInitial("albus dumbledore");
			AssertEquals(1, calc.GetAvailableCodesCounter);

			calc.GetMostAppropriateStaffInitial("severus snape");
			AssertEquals(1, calc.GetAvailableCodesCounter);

			calc.GetMostAppropriateStaffInitial("harry potter");
			AssertEquals(1, calc.GetAvailableCodesCounter);

			Thread.Sleep(5000);
			calc.GetMostAppropriateStaffInitial("ron weasley");
			AssertEquals(2, calc.GetAvailableCodesCounter);
		}

		class FastStaffCodeCalculatorForTest : FastStaffCodeCalculator
		{
			protected override DateTime GetCacheExpiry()
			{
				return ZDateTime.UtcNow.AddSeconds(5).ToDateTime();
			}

			public int GetAvailableCodesCounter;

			public FastStaffCodeCalculatorForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override HashSet<string> GetAvailableCodes()
			{
				GetAvailableCodesCounter++;
				return base.GetAvailableCodes();
			}
		}
	}
}
