using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class LineBreakerTest : TestCaseWithFactory
	{
		public void TestMaxLengthOnTargetLines()
		{
			const string sourceString = "Hello, I'm Leonard Nimoy. The following tale of alien encounters is true, and by true I mean false. It's all lies. But they're entertaining lies. And in the end, isn't that the real truth? The answer is no.";

			var lineBreaker = new LineBreaker(30);
			lineBreaker.SetTargetLineMaxLengths(new Dictionary<int, int> { { 4, 10 }, { 5, 10 } });
			var lines = lineBreaker.GetBrokenLinesList(sourceString, 0, 2);

			AssertEquals("This is target line 2 - 30 char max", "Hello, I'm Leonard Nimoy. The", lines[0]);
			AssertEquals("This is target line 3 - 30 char max", "following tale of alien", lines[1]);
			AssertEquals("This is target line 4 - 10 char max", "encounters", lines[2]);
			AssertEquals("This is target line 5 - 10 char max", "is true,", lines[3]);
			AssertEquals("This is target line 6 - 30 char max", "and by true I mean false. It's", lines[4]);
			AssertEquals("This is target line 7 - 30 char max", "all lies. But they're", lines[5]);
			AssertEquals("This is target line 8 - 30 char max", "entertaining lies. And in the", lines[6]);
			AssertEquals("This is target line 8 - 30 char max", "end, isn't that the real", lines[7]);
			AssertEquals("This is target line 8 - 30 char max", "truth? The answer is no.", lines[8]);
		}

		public void TestIsThereSpaceForNextCharacter()
		{
			String str = "Mohsen";
			var arialFont = new Font("Arial", 9);

			int width = (new TextSizeCalculator(arialFont)).GetLengthInMillimeter(str);

			AssertEquals(true, new LineBreaker(0, str.Length, width, arialFont).IsThereSpaceForNextString(""));
			AssertEquals(true, new LineBreaker(0, str.Length, width, arialFont).IsThereSpaceForNextString(" "));
			AssertEquals(true, new LineBreaker(0, str.Length, width, arialFont).IsThereSpaceForNextString(str));
			AssertEquals(false, new LineBreaker(0, str.Length, width - 1, arialFont).IsThereSpaceForNextString(str));
			AssertEquals(false, new LineBreaker(0, str.Length - 1, width, arialFont).IsThereSpaceForNextString(str));
			AssertEquals(false, new LineBreaker(0, str.Length - 1, width - 1, arialFont).IsThereSpaceForNextString(str));
		}

		public void TestGetBrokenLinesWithJustANumberOfCharacters()
		{
			const string sourceString = @"the only thing im using is the intake manifolds and TB, everything else that came with my longblock is up for grabs. anybody want any of it? i'll pretty much take any reasonable offers just to make a tiny bit of my money back and get some of this crap out of my garage.
block is a late a series, low mileage, supposedly the importer saw it run in japan. looks real clean anyway. all the cast and platic stuff has no cracks or anything.
sorry guys, guess i wasnt too clear, what i meant by everything that came with my longblock was everything EXCEPT the actual block. as in manifold, turbos, mounts, piping, ac/ps, waterpump, belts + pulleys, flexplate, crap like that. the block, counterweight, and intake manifold are not for sale.
Posts: 5,656  how much for the whole twin turbo setup with the charge pipes and all that?  
i have no idea what its worth, make me an offer. otherwise i'll look around and see what kind of price i would want. all that stuff looks to be in excellent shape by the way. i can measure shaft play in the turbos if you want, but IIRC it was almost undetectable.";

			var lineBreaker = new LineBreaker(100);
			ZString result = lineBreaker.GetBrokenLines(sourceString);

			//  123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789!
			AssertMultilineASCIIEquals("Should be word broker into 100 char lines.", @"
the only thing im using is the intake manifolds and TB, everything else that came with my longblock
is up for grabs. anybody want any of it? i'll pretty much take any reasonable offers just to make a
tiny bit of my money back and get some of this crap out of my garage.
block is a late a series, low mileage, supposedly the importer saw it run in japan. looks real clean
anyway. all the cast and platic stuff has no cracks or anything.
sorry guys, guess i wasnt too clear, what i meant by everything that came with my longblock was
everything EXCEPT the actual block. as in manifold, turbos, mounts, piping, ac/ps, waterpump, belts
+ pulleys, flexplate, crap like that. the block, counterweight, and intake manifold are not for
sale.
Posts: 5,656  how much for the whole twin turbo setup with the charge pipes and all that?
i have no idea what its worth, make me an offer. otherwise i'll look around and see what kind of
price i would want. all that stuff looks to be in excellent shape by the way. i can measure shaft
play in the turbos if you want, but IIRC it was almost undetectable.
".Trim(), result);
		}
	}
}
