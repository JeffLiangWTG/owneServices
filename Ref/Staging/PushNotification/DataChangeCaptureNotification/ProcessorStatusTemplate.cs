using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	public class ProcessorStatusTemplate : HtmlTemplate<ProcessorStatus>
	{
		public override string GetBody(IEnumerable<ProcessorStatus> data)
		{
			var result = FormattableString.Invariant($@"
<table border='1'>
	<thead>
		<tr>
			<th>
				{nameof(ProcessorStatus.PRC_PK)}
			</th>
			<th>
				{nameof(ProcessorStatus.PRC_JobName)}
			</th>
			<th>
				{nameof(ProcessorStatus.PRC_JobGroup)}
			</th>
			<th>
				{nameof(ProcessorStatus.PRC_LastRunTime)}
			</th>
			<th>
				{nameof(ProcessorStatus.PRC_LastSuccessRunTime)}
			</th>
			<th>
				{nameof(ProcessorStatus.PRC_LastSuccessRecordUpdatedCount)}
			</th>
			<th>
				{nameof(ProcessorStatus.PRC_LastDataSetUpdatedTime)}
			</th>
			<th>
				{nameof(ProcessorStatus.PRC_Status)}
			</th>
		</tr>
	</thead>");

			foreach (var processor in data)
			{
				result += FormattableString.Invariant($@"
	<tbody>
		<tr>
			<td>
				{processor.PRC_PK}
			</td>
			<td>
				{HtmlSanitizer.Sanitize(processor.PRC_JobName)}
			</td>
			<td>
				{HtmlSanitizer.Sanitize(processor.PRC_JobGroup)}
			</td>
			<td>
				{processor.PRC_LastRunTime}
			</td>
			<td>
				{processor.PRC_LastSuccessRunTime}
			</td>
			<td>
				{processor.PRC_LastSuccessRecordUpdatedCount}
			</td>
			<td>
				{processor.PRC_LastDataSetUpdatedTime}
			</td>
			<td>
				{HtmlSanitizer.Sanitize(processor.PRC_Status)}
			</td>
		</tr>
	</tbody>
");
			}
			result += "</table>";

			return result;
		}
	}
}
