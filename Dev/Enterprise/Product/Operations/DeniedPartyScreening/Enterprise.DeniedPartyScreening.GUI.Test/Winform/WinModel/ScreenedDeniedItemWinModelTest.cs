using System;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class ScreenedDeniedItemWinModelTest : TransactionedTestCase
	{
		public void TestConstructor_FourParameters()
		{
			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
				var model = new ScreenedDeniedItemWinModel("李霞", "王芳", "59%", ScoreGrades.Low, 59);
				AssertScreenedDeniedItemWinModel("李霞", "王芳", "59%", ScoreGrades.Low, 59, model);

				model = new ScreenedDeniedItemWinModel("李霞", "李磊", "80%", ScoreGrades.Medium, 80);
				AssertScreenedDeniedItemWinModel("李霞", "李磊", "80%", ScoreGrades.Medium, 80, model);

				model = new ScreenedDeniedItemWinModel("李霞", "李夏", "91%", ScoreGrades.High, 91);
				AssertScreenedDeniedItemWinModel("李霞", "李夏", "91%", ScoreGrades.High, 91, model);
			}
		}

		public void TestConstructor_OneParameter()
		{
			AssertScreenedDeniedItemWinModel(string.Empty, "李霞", string.Empty, ScoreGrades.Low, 0, new ScreenedDeniedItemWinModel("李霞"));
		}

		public static void AssertScreenedDeniedItemWinModel(string expectedScreenedParty, string expectedDeniedParty, string expectedDisplayScore, ScoreGrades expectedScoreGrade, int expectedScore, ScreenedDeniedItemWinModel model)
		{
			AssertEquals(expectedScreenedParty, model.ScreenedParty);
			AssertEquals(expectedDeniedParty, model.DeniedParty);
			AssertEquals(expectedDisplayScore, model.DisplayScore);
			AssertEquals(expectedScoreGrade, model.ScoreGrade);
			AssertEquals(expectedScore, model.Score);
		}

		public void TestConstructor_ArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => _ = new ScreenedDeniedItemWinModel(string.Empty));
			AssertExceptionThrown<ArgumentException>(() => _ = new ScreenedDeniedItemWinModel("1234", string.Empty, string.Empty, ScoreGrades.Low));
			AssertExceptionThrown<ArgumentException>(() => _ = new ScreenedDeniedItemWinModel(string.Empty, "1234", string.Empty, ScoreGrades.Low));
		}
	}
}
