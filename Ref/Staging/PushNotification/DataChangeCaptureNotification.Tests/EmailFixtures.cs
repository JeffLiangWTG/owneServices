using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification.Tests
{
	[TestFixture]
	public class EmailFixtures
	{
		[Test]
		public void SourceDataTemplateIsMatching()
		{
			var sourceDataList = new List<SourceData>()
			{
				new SourceData()
				{
					SDA_PK = new Guid("26D3D966-28D2-4A70-AB9D-6697DAA22579"),
					SDA_Source = "INT",
					SDA_Filename = "any name",
					SDA_Filetype = "XML",
					SDA_SubSource = "any",
					SDA_Status = "ERR"
				}
			};

			var template = new SourceDataTemplate();

			var header = template.GetHeader();
			var body = template.GetBody(sourceDataList);
			var footer = template.GetFooter();
			var title = template.GetTitle("any message");

			AssertHeader(header);
			AssertBodySourceData(body);
			AssertFooter(footer);
			AssertTitle(title);
		}

		[Test]
		public void ProcessorStatusTemplateIsMatching()
		{
			var processorStatusList = new List<ProcessorStatus>()
			{
				new ProcessorStatus()
				{
					PRC_PK = new Guid("26D3D966-28D2-4A70-AB9D-6697DAA22579"),
					PRC_JobGroup = "AU",
					PRC_LastDataSetUpdatedTime = DateTime.UtcNow,
					PRC_LastRunTime = DateTime.UtcNow,
					PRC_LastSuccessRecordUpdatedCount = 1,
					PRC_LastSuccessRunTime = DateTime.UtcNow,
					PRC_JobName = "Proc1"
				}
			};

			var template = new ProcessorStatusTemplate();

			var header = template.GetHeader();
			var body = template.GetBody(processorStatusList);
			var footer = template.GetFooter();
			var title = template.GetTitle("any message");

			AssertHeader(header);
			AssertBodyProcessorStatus(body);
			AssertFooter(footer);
			AssertTitle(title);
		}

		void AssertHeader(string header)
		{
			var expected = @"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
  <title>Error found during processing of data</title>
  <style type='text/css'>
	
td.content table tr td,
td.content table tr th {
	padding:8px;
}
</style>
</head>
<body>
  <table>
	<tr>
	  <td class='content'>
".Replace("'", "\"");

			Assert.That(header == expected);
		}

		void AssertBodySourceData(string body)
		{
			var expected = FormattableString.Invariant($@"
<table border='1'>
	<thead>
		<tr>
			<th>
				SDA_PK
			</th>
			<th>
				SDA_Source
			</th>
			<th>
				SDA_Filename
			</th>
			<th>
				SDA_Filetype
			</th>
			<th>
				SDA_SubSource
			</th>
			<th>
				SDA_Status
			</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>
				2839CC86-6D0A-44B1-B9DB-DB1005D96402
			</td>
			<td>
				INT
			</td>
			<td>
				any name
			</td>
			<td>
				XML
			</td>
			<td>
				any
			</td>
			<td>
				ERR
			</td>
		</tr>
	</tbody>
</table>
");
		}

		void AssertBodyProcessorStatus(string body)
		{
			var expected = FormattableString.Invariant($@"
<table border='1' class='t1'>
	<thead>
		<tr>
			<th>
				PRC_PK
			</th>
			<th>
				PRC_Processor
			</th>
			<th>
				PRC_Country
			</th>
			<th>
				PRC_LasRunTime
			</th>
			<th>
				PRC_LastSuccessRunTime
			</th>
			<th>
				PRC_LastSuccessRecordUpdatedCount
			</th>
			<th>
				PRC_LastDataSetUpdatedTime
			</th>
			<th>
				PRC_Status
			</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>
				2839CC86-6D0A-44B1-B9DB-DB1005D96402
			</td>
			<td>
				Proc1
			</td>
			<td>
				AU
			</td>
			<td>
				{DateTime.UtcNow:s}
			</td>
			<td>
				{DateTime.UtcNow:s}
			</td>
			<td>
				1
			</td>
			<td>
				{DateTime.UtcNow:s}
			</td>
			<td>
				ERR
			</td>
		</tr>
	</tbody>
</table>
");
		}

		void AssertFooter(string footer)
		{
			var expected = @"		</td>
	</tr>
	<tr style='padding:15px'>
	  <td><small>These notifications are generated automatically, please do not reply to this e-mail</small></td>
	</tr>
  </table>
</body>
</html>
".Replace("'", "\"");

			Assert.That(footer == expected);
		}

		void AssertTitle(string title)
		{
			var expected = FormattableString.Invariant($"<br /><br /><h2>any message</h2><br /><br />");

			Assert.That(title == expected);
		}
	}
}
