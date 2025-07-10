using System;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
[TransactionedTestCase]
class RefLanguageTextFixture
{
	string DbName { get; } = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);

	[TestCase("RN")]
	[TestCase("RW")]
	[TestCase("RX")]
	public void CreateRefLanguageText(string code)
	{
		var refLanguageText = new RefLanguageText
		{
			RLT_PK = Guid.NewGuid(),
			RLT_ParentTableCode = code,
			RLT_ParentId = Guid.NewGuid(),
			RLT_Language = "VI-VN",
			RLT_ColumnName = "RN_Desc",
			RLT_Text = "Nam Georgia và đảo Nam Sandwich"
		};

		using (var context1 = new StagingDbContext(TestConnectionString.GetAdmin(DbName)))
		{
			context1.RefLanguageTexts.Add(refLanguageText);
			context1.SaveChanges();
		}

		using (var context2 = new StagingDbContext(TestConnectionString.GetAdmin(DbName)))
		{
			var refLanguageTextFromDb = context2.RefLanguageTexts.Find(refLanguageText.RLT_PK);
			Assert.IsNotNull(refLanguageTextFromDb);
			Assert.AreEqual(refLanguageText.RLT_PK, refLanguageTextFromDb.RLT_PK);
			Assert.AreEqual(refLanguageText.RLT_ParentTableCode, refLanguageTextFromDb.RLT_ParentTableCode);
			Assert.AreEqual(refLanguageText.RLT_ParentId, refLanguageTextFromDb.RLT_ParentId);
			Assert.AreEqual(refLanguageText.RLT_Language, refLanguageTextFromDb.RLT_Language);
			Assert.AreEqual(refLanguageText.RLT_ColumnName, refLanguageTextFromDb.RLT_ColumnName);
			Assert.AreEqual(refLanguageText.RLT_Text, refLanguageTextFromDb.RLT_Text);
		}
	}

	[TestCase("RQ")]
	public void CreateRefLanguageTextWithError(string code)
	{
		var ex = Assert.Throws<DbUpdateException>(() =>
		{
			var refLanguageText = new RefLanguageText
			{
				RLT_PK = Guid.NewGuid(),
				RLT_ParentTableCode = code,
				RLT_ParentId = Guid.NewGuid(),
				RLT_Language = "VI-VN",
				RLT_ColumnName = "RN_Desc",
				RLT_Text = "Nam Georgia và đảo Nam Sandwich"
			};

			using var context = new StagingDbContext(TestConnectionString.GetAdmin(DbName));
			context.RefLanguageTexts.Add(refLanguageText);
			context.SaveChanges();
		});

		Assert.That(ex.InnerException.Message.Contains("The INSERT statement conflicted with the CHECK constraint \"CK_RefLanguageText_RLT_ParentTableCode\"."), Is.EqualTo(true));
	}
}
