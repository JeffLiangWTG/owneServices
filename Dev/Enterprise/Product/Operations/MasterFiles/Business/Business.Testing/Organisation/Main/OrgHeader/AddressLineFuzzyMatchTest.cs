using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AddressLineFuzzyMatchTest : TestCase
	{
		public void TestFuzzyMatch()
		{
			ZString s1 = AddressLineFuzzyMatch.GetStringForFuzzyComparing("1/ 25 Victoria Rd");
			ZString s2 = AddressLineFuzzyMatch.GetStringForFuzzyComparing(" 1 /25 VicTORia");
			Assert("Strings should match", s1.CompareTo(s2) == 0);

			Assert(AddressLineFuzzyMatch.GetStringForFuzzyComparing(" 1 /25 ").IsEmpty);
			AssertEquals("WETHERILLPARK", AddressLineFuzzyMatch.GetStringForFuzzyComparing("WETHERILL PARK D.C., NSW"));
		}

		public void TestFuzzyMatchAddresses()
		{
			AssertEquals("34,6DUTRUC", AddressLineFuzzyMatch.GetStringForFuzzyComparing("FL 34, 6 DUTRUC STREET"));
			AssertEquals("129APT1BOTANY", AddressLineFuzzyMatch.GetStringForFuzzyComparing("129 APT 1 BOTANY ROAD NSW"));
			AssertEquals("1/25VICTORIA", AddressLineFuzzyMatch.GetStringForFuzzyComparing("1/ 25 Victoria Rd"));
			AssertEquals("1103-4,11/F100HOWMING", AddressLineFuzzyMatch.GetStringForFuzzyComparing("Suites 1103-4, 11/F, One Landmark East, 100 How Ming Street, Kwun Tong, Kowloon, Hong Kong"));
			AssertEquals("255,EDF.CHINA7ANDAR,MACAU", AddressLineFuzzyMatch.GetStringForFuzzyComparing("Alameda Dr. Carlos d'Assumpcao, 255, Edf. China Civil Plaza, 7 Andar, Macau"));
			AssertEquals("12/F,ONE1WANGYUEN", AddressLineFuzzyMatch.GetStringForFuzzyComparing("12/F, One Kowloon, 1 Wang Yuen Street, Kowloon Bay, Hong Kong"));
			AssertEquals("6,8/F3-5WANG", AddressLineFuzzyMatch.GetStringForFuzzyComparing("Flat 6, 8/F, Block A,Hoplite Industrial Centre, 3-5 Wang Tai Road, Kowloon Bay, Kowloon"));
			AssertEquals("331CORPORATECIRCLE", AddressLineFuzzyMatch.GetStringForFuzzyComparing("331 Corporate Circle, Suite J"));
			AssertEquals("600PETERJEFFERSON310", AddressLineFuzzyMatch.GetStringForFuzzyComparing("600 Peter Jefferson Pkwy, Suite 310"));
			AssertEquals("895SIVERTDR", AddressLineFuzzyMatch.GetStringForFuzzyComparing("895 Sivert Dr"));
			AssertEquals("8578NW23RD", AddressLineFuzzyMatch.GetStringForFuzzyComparing("8578 NW 23rd St"));
			AssertEquals("177LOCKWOODLANE", AddressLineFuzzyMatch.GetStringForFuzzyComparing("177 Lockwood Lane"));
			AssertEquals("103-4,1/F608CASTLEPEAK", AddressLineFuzzyMatch.GetStringForFuzzyComparing("Unit 103-4, 1/F., Block A,Wing Kut Industrial Blg., 608 Castle Peak Road,Cheung Sha Wan"));
			AssertEquals("7A,EE10256TALLINN", AddressLineFuzzyMatch.GetStringForFuzzyComparing("Jaan Poska 7A, EE-10256 Tallinn"));
			AssertEquals("RUPNIECIBAS13-1", AddressLineFuzzyMatch.GetStringForFuzzyComparing("Rupniecibas 13-1"));
			AssertEquals("3-7-4,KYOBASHI", AddressLineFuzzyMatch.GetStringForFuzzyComparing("3-7-4, Kyobashi Chuo-Ku"));
			AssertEquals("111SOLEDAD,SUITE300", AddressLineFuzzyMatch.GetStringForFuzzyComparing("111 Soledad, Suite 300"));
			AssertEquals("PODVYSOTSKOGO6,109", AddressLineFuzzyMatch.GetStringForFuzzyComparing("Podvysotskogo 6, 109"));
		}

		public void TestWithoutNumbers()
		{
			AssertEquals("HELLOWORLD", AddressLineFuzzyMatch.GetStringForFuzzyComparing("HELLO WORLD"));
			AssertEquals("HELLOCSHARP", AddressLineFuzzyMatch.GetStringForFuzzyComparing("HELLO CSHARP WORLD"));
			AssertEquals("HELLO", AddressLineFuzzyMatch.GetStringForFuzzyComparing(" HELLO"));
			AssertEquals("HELLO", AddressLineFuzzyMatch.GetStringForFuzzyComparing("HELLO "));
			AssertEquals("HELLO", AddressLineFuzzyMatch.GetStringForFuzzyComparing("  HELLO"));
			AssertEquals("HELLO", AddressLineFuzzyMatch.GetStringForFuzzyComparing("HELLO  "));
			AssertEquals("HELLO", AddressLineFuzzyMatch.GetStringForFuzzyComparing("HELLO"));
		}
	}
}
