using System;
using System.IO;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class XmlExamQuestionsImporterTest : ExamQuestionsImporterTestCase
	{
		public void TestImportQuestions()
		{
			string xmlFile1 = Environment.Env.GetTempFileName();
			string xmlFile2 = Environment.Env.GetTempFileName();
			ExamQuestionsImporter importer = GetExamQuestionsImporter();

			try
			{
				NotificationBuffer notifications = new NotificationBuffer();

				using (StreamWriter writer = File.CreateText(xmlFile1))
				{
					writer.Write(XmlData_1);
				}

				importer.importerBizO.FileLocation = xmlFile1;
				AssertEquals("Number of notifications", 3, importer.ImportQuestions(notifications));
				AssertEquals("Events", 0, notifications.Events.Length);
				AssertEquals("Questions added", 3, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[0], @"Question 1:&lt;script src=&quot;&quot; media=&#39;&#39;&gt;<pre class=""codeblock"">&lt;div&gt;<b><font color=""#FF00FF"">&quot;A&quot;</font></b> &amp;&amp; <i><font color=""#FF00FF"">&#39;B&#39;</font></i>&lt;/div&gt;</pre>", true, string.Empty, 4, 1, "Option A");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[1], "Question 2:", true, string.Empty, 0, 1);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[2], @"Question 3:&lt;script src=&quot;&quot; media=&#39;&#39;&gt;<pre class=""codeblock"">&lt;div&gt;<b>&quot;A&quot;</b> &amp;&amp; <i><font color=""#FF00FF"">&#39;B&#39;</font></i>&lt;/div&gt;</pre>", false, string.Empty, 4, 2, "Option A", "Option D");

				using (StreamWriter writer = File.CreateText(xmlFile2))
				{
					writer.Write(XmlData_2);
				}

				importer.importerBizO.FileLocation = xmlFile2;
				importer.importerBizO.ShouldClearExistingQuestions = true;
				AssertEquals("Number of notifications", 2, importer.ImportQuestions(notifications));
				AssertEquals("Events", 0, notifications.Events.Length);
				AssertEquals("Questions added", 2, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[0], "Question 2.1:", true, string.Empty, 2, 1, "Option 2.1A");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[1], "Question 2.2:", false, string.Empty, 3, 2, "Option 2.2A", "Option 2.2C");
			}
			finally
			{
				File.Delete(xmlFile1);
				File.Delete(xmlFile2);
			}
		}

		public void TestImportQuestionsWithComments()
		{
			var xmlFile4 = Environment.Env.GetTempFileName();
			var importer = GetExamQuestionsImporter();

			try
			{
				NotificationBuffer notifications = new NotificationBuffer();

				using (StreamWriter writer = File.CreateText(xmlFile4))
				{
					writer.Write(XmlData_4);
				}

				importer.importerBizO.FileLocation = xmlFile4;
				AssertEquals("Number of notifications", 3, importer.ImportQuestions(notifications));
				AssertEquals("Events", 0, notifications.Events.Length);
				AssertEquals("Questions added", 3, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[0], @"Question 1:&lt;script src=&quot;&quot; media=&#39;&#39;&gt;<pre class=""codeblock"">&lt;div&gt;<b><font color=""#FF00FF"">&quot;A&quot;</font></b> &amp;&amp; <i><font color=""#FF00FF"">&#39;B&#39;</font></i>&lt;/div&gt;</pre>", true, "This is a great question", 4, 1, "Option A");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[1], "Question 2:", true, "This question is not so good", 0, 1);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[2], @"Question 3:&lt;script src=&quot;&quot; media=&#39;&#39;&gt;<pre class=""codeblock"">&lt;div&gt;<b>&quot;A&quot;</b> &amp;&amp; <i><font color=""#FF00FF"">&#39;B&#39;</font></i>&lt;/div&gt;</pre>", false,
					"This is a super long comment that will hopefully get cut offThis is a super long comment that will hopefully get cut offThis is a super long comment that will hopefully get cut offThis is a super long comment that will hopefully get cut offThis is a super",
					4, 2, "Option A", "Option D");
			}
			finally
			{
				File.Delete(xmlFile4);
			}
		}

		void AssertImportedQuestion(VoteExamSurveyQuestion question, string questionText, bool isQuestionRandomisable, string comment, int optionsCount, int maxChosenOptionsCount, params string[] correctOptions)
		{
			AssertContains("Question", questionText, question.HY_Question);
			AssertEquals("Comment", comment, question.HY_Comment);
			AssertEquals("Options Count", optionsCount, question.SubQuestions.Count);
			if (correctOptions != null && correctOptions.Length > 0)
			{
				foreach (LearningCentreQuestion subQuestion in question.SubQuestions)
				{
					bool isInCorrectSet = false;
					foreach (ZString correctOption in correctOptions)
					{
						if (subQuestion.HY_Question == correctOption)
						{
							isInCorrectSet = true;
							break;
						}
					}
					AssertEquals("Correct question option", isInCorrectSet, subQuestion.CorrectAnswerAsBool);
				}
			}
			AssertEquals("Max options can be chosen", maxChosenOptionsCount, question.HY_Max);
			AssertEquals("Is Question Randomisable", isQuestionRandomisable, question.HY_IsRandomisable);
		}

		#region Implementation

		protected override ExamQuestionsImporter GetExamQuestionsImporter()
		{
			ExamSurveyQuestionImporterBizO importerBizO = new ExamSurveyQuestionImporterBizO(Factory.NewWithValidTestData<LearningCentreCampaign>(), new FileMapperForTest());
			importerBizO.FileLocation = "import.xml";
			return ExamQuestionsImporter.New(importerBizO);
		}

		protected override string CreateValidTestFileWithTwoQuestions()
		{
			string xmlFile = Environment.Env.GetTempFileName();

			using (StreamWriter writer = File.CreateText(xmlFile))
			{
				writer.Write(XmlData_3);
			}

			return xmlFile;
		}

		protected override Tuple<string, string>[] InvalidTestFileContentsAndExpectedErrorMessage()
		{
			return new[]
				{
Tuple.Create(
	"Not a valid XML file",
	"Error: The XML file contains invalid content: Data at the root level is invalid. Line 1, position 1."),

Tuple.Create(
	@"<?xml version=""1.0"" encoding=""utf-8"" ?> 
	<GlbCompanyCampaign>
		<CampaignName>.NET Framework</CampaignName>
		<CampaignComments>.NET Framework</CampaignComments>
		<CampaignManager>MS</CampaignManager>
		<CampaignCoordinator>MS</CampaignCoordinator>
		<ExamExpiryTimeInMinutes>1 hour</ExamExpiryTimeInMinutes>
		<VoteExamSurveyQuestionCollection>
			<VoteExamSurveyQuestion>
				<QuestionOrder>Don't know</QuestionOrder>
				<Question>Question 1:</Question>
				<AnswerType>MUL</AnswerType>
				<IsRandomisable>Sure</IsRandomisable>
				<IsValid>Nah</IsValid>
				<IsActive>Y</IsActive>
				<OptionalAnswerExplanation>Question 1 Explanation</OptionalAnswerExplanation>
				<AnswerCollection>
					<VoteExamSurveyQuestion>
						<QuestionOrder>1</QuestionOrder>
						<SubQuestionOrder>1</SubQuestionOrder>
						<Question>Option A</Question>
						<AnswerType>MCP</AnswerType>
						<ExamCorrectAnswer>Y</ExamCorrectAnswer>
						<IsRandomisable>Y</IsRandomisable>
						<IsValid>Y</IsValid>
						<IsActive>Y</IsActive>
					</VoteExamSurveyQuestion>
				</AnswerCollection>
			</VoteExamSurveyQuestion>
		</VoteExamSurveyQuestionCollection>
	</GlbCompanyCampaign>",
	"Error: The XML file contains invalid content: Cannot initialise a CargoWise.Types.ZBool with <Sure> (System.String).")
			};
		}

		#region Test Data 1 For TestImportQuestion

		const string XmlData_1 =
					@"<?xml version=""1.0"" encoding=""utf-8"" ?> 
					<GlbCompanyCampaign>
						<CampaignName>.NET Framework</CampaignName>
						<CampaignComments>.NET Framework</CampaignComments>
						<CampaignManager>MS</CampaignManager>
						<CampaignCoordinator>MS</CampaignCoordinator>
						<ExamExpiryTimeInMinutes>120</ExamExpiryTimeInMinutes>
						<VoteExamSurveyQuestionCollection>
							<VoteExamSurveyQuestion>
								<QuestionOrder>1</QuestionOrder>
								<Question>Question 1:&lt;script src=&quot;&quot; media=''&gt;[code:csharp]&lt;div&gt;[b]""A""[/b] &amp;&amp; [i]'B'[/i]&lt;/div&gt;[/code]</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>Y</IsRandomisable>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 1 Explanation</OptionalAnswerExplanation>
								<AnswerCollection>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>1</SubQuestionOrder>
										<Question>Option A</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>2</SubQuestionOrder>
										<Question>Option B</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>3</SubQuestionOrder>
										<Question>Option C</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>4</SubQuestionOrder>
										<Question>Option D</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
								</AnswerCollection>
							</VoteExamSurveyQuestion>
							<VoteExamSurveyQuestion>
								<QuestionOrder>2</QuestionOrder>
								<Question>Question 2:</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>Y</IsRandomisable>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 2 Explanation</OptionalAnswerExplanation>
								<AnswerCollection />
							</VoteExamSurveyQuestion>
							<VoteExamSurveyQuestion>
								<QuestionOrder>3</QuestionOrder>
								<Question>Question 3:&lt;script src=&quot;&quot; media=''&gt;[code:tsql]&lt;div&gt;[b]""A""[/b] &amp;&amp; [i]'B'[/i]&lt;/div&gt;[/code]</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>N</IsRandomisable>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 3 Explanation</OptionalAnswerExplanation>
								<AnswerCollection>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>1</SubQuestionOrder>
										<Question>Option A</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>2</SubQuestionOrder>
										<Question>Option B</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>3</SubQuestionOrder>
										<Question>Option C</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>4</SubQuestionOrder>
										<Question>Option D</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
								</AnswerCollection>
							</VoteExamSurveyQuestion>
						</VoteExamSurveyQuestionCollection>
					</GlbCompanyCampaign>";

		#endregion

		#region Test Data 2 For TestImportQuestion

		const string XmlData_2 =
					@"<?xml version=""1.0"" encoding=""utf-8"" ?> 
					<GlbCompanyCampaign>
						<CampaignName>.NET Framework</CampaignName>
						<CampaignComments>.NET Framework</CampaignComments>
						<CampaignManager>MS</CampaignManager>
						<CampaignCoordinator>MS</CampaignCoordinator>
						<ExamExpiryTimeInMinutes>120</ExamExpiryTimeInMinutes>
						<VoteExamSurveyQuestionCollection>
							<VoteExamSurveyQuestion>
								<QuestionOrder>1</QuestionOrder>
								<Question>Question 2.1:</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>Y</IsRandomisable>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 2.1 Explanation</OptionalAnswerExplanation>
								<AnswerCollection>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>1</SubQuestionOrder>
										<Question>Option 2.1A</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>2</SubQuestionOrder>
										<Question>Option 2.1B</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
								</AnswerCollection>
							</VoteExamSurveyQuestion>
							<VoteExamSurveyQuestion>
								<QuestionOrder>3</QuestionOrder>
								<Question>Question 2.2:</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>N</IsRandomisable>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 2.2 Explanation</OptionalAnswerExplanation>
								<AnswerCollection>
									<VoteExamSurveyQuestion>
										<QuestionOrder>2</QuestionOrder>
										<SubQuestionOrder>1</SubQuestionOrder>
										<Question>Option 2.2A</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>2</SubQuestionOrder>
										<Question>Option 2.2B</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>3</SubQuestionOrder>
										<Question>Option 2.2C</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
								</AnswerCollection>
							</VoteExamSurveyQuestion>
						</VoteExamSurveyQuestionCollection>
					</GlbCompanyCampaign>";

		#endregion

		#region Test Data For CreateValidTestFile

		const string XmlData_3 =
					@"<?xml version=""1.0"" encoding=""utf-8"" ?> 
					<GlbCompanyCampaign>
						<CampaignName>.NET Framework</CampaignName>
						<CampaignComments>.NET Framework</CampaignComments>
						<CampaignManager>MS</CampaignManager>
						<CampaignCoordinator>MS</CampaignCoordinator>
						<ExamExpiryTimeInMinutes>120</ExamExpiryTimeInMinutes>
						<VoteExamSurveyQuestionCollection>
							<VoteExamSurveyQuestion>
								<QuestionOrder>1</QuestionOrder>
								<Question>Question 1:</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>Y</IsRandomisable>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 1 Explanation</OptionalAnswerExplanation>
								<AnswerCollection>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>1</SubQuestionOrder>
										<Question>Option A</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>2</SubQuestionOrder>
										<Question>Option B</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>3</SubQuestionOrder>
										<Question>Option C</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>4</SubQuestionOrder>
										<Question>Option D</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
								</AnswerCollection>
							</VoteExamSurveyQuestion>
							<VoteExamSurveyQuestion>
								<QuestionOrder>2</QuestionOrder>
								<Question>Question 2:</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>Y</IsRandomisable>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 2 Explanation</OptionalAnswerExplanation>
								<AnswerCollection />
							</VoteExamSurveyQuestion>
						</VoteExamSurveyQuestionCollection>
					</GlbCompanyCampaign>";

		#endregion

		#region Test Data For TestImportQuestionWithComments

		const string XmlData_4 =

			@"<?xml version=""1.0"" encoding=""utf-8"" ?> 
					<GlbCompanyCampaign>
						<CampaignName>.NET Framework</CampaignName>
						<CampaignComments>.NET Framework</CampaignComments>
						<CampaignManager>MS</CampaignManager>
						<CampaignCoordinator>MS</CampaignCoordinator>
						<ExamExpiryTimeInMinutes>120</ExamExpiryTimeInMinutes>
						<VoteExamSurveyQuestionCollection>
							<VoteExamSurveyQuestion>
								<QuestionOrder>1</QuestionOrder>
								<Question>Question 1:&lt;script src=&quot;&quot; media=''&gt;[code:csharp]&lt;div&gt;[b]""A""[/b] &amp;&amp; [i]'B'[/i]&lt;/div&gt;[/code]</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>Y</IsRandomisable>
								<Comment>This is a great question</Comment>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 1 Explanation</OptionalAnswerExplanation>
								<AnswerCollection>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>1</SubQuestionOrder>
										<Question>Option A</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>2</SubQuestionOrder>
										<Question>Option B</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>3</SubQuestionOrder>
										<Question>Option C</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<SubQuestionOrder>4</SubQuestionOrder>
										<Question>Option D</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
								</AnswerCollection>
							</VoteExamSurveyQuestion>
							<VoteExamSurveyQuestion>
								<QuestionOrder>2</QuestionOrder>
								<Question>Question 2:</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>Y</IsRandomisable>
								<Comment>This question is not so good</Comment>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 2 Explanation</OptionalAnswerExplanation>
								<AnswerCollection />
							</VoteExamSurveyQuestion>
							<VoteExamSurveyQuestion>
								<QuestionOrder>3</QuestionOrder>
								<Question>Question 3:&lt;script src=&quot;&quot; media=''&gt;[code:tsql]&lt;div&gt;[b]""A""[/b] &amp;&amp; [i]'B'[/i]&lt;/div&gt;[/code]</Question>
								<AnswerType>MUL</AnswerType>
								<IsRandomisable>N</IsRandomisable>
								<Comment>This is a super long comment that will hopefully get cut offThis is a super long comment that will hopefully get cut offThis is a super long comment that will hopefully get cut offThis is a super long comment that will hopefully get cut offThis is a super long comment that will hopefully get cut offThis is a super long comment that will hopefully get cut off</Comment>
								<IsValid>Y</IsValid>
								<IsActive>Y</IsActive>
								<OptionalAnswerExplanation>Question 3 Explanation</OptionalAnswerExplanation>
								<AnswerCollection>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>1</SubQuestionOrder>
										<Question>Option A</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>2</SubQuestionOrder>
										<Question>Option B</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>3</SubQuestionOrder>
										<Question>Option C</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>N</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
									<VoteExamSurveyQuestion>
										<QuestionOrder>3</QuestionOrder>
										<SubQuestionOrder>4</SubQuestionOrder>
										<Question>Option D</Question>
										<AnswerType>MCP</AnswerType>
										<ExamCorrectAnswer>Y</ExamCorrectAnswer>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
									</VoteExamSurveyQuestion>
								</AnswerCollection>
							</VoteExamSurveyQuestion>
						</VoteExamSurveyQuestionCollection>
					</GlbCompanyCampaign>";

		#endregion

		#endregion
	}
}
