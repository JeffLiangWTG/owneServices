using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	public class SourceDataTemplate : HtmlTemplate<SourceData>
	{
		public override string GetBody(IEnumerable<SourceData> data)
		{
			var result = FormattableString.Invariant($@"
<table border='1'>
	<thead>
		<tr>
			<th>
				{nameof(SourceData.SDA_PK)}
			</th>
			<th>
				{nameof(SourceData.SDA_Source)}
			</th>
			<th>
				{nameof(SourceData.SDA_Filename)}
			</th>
			<th>
				{nameof(SourceData.SDA_Filetype)}
			</th>
			<th>
				{nameof(SourceData.SDA_SubSource)}
			</th>
			<th>
				{nameof(SourceData.SDA_Status)}
			</th>
		</tr>
	</thead>");

			foreach (var sourceData in data)
			{
				result += FormattableString.Invariant($@"
	<tbody>
		<tr>
			<td>
				{sourceData.SDA_PK}
			</td>
			<td>
				{HtmlSanitizer.Sanitize(sourceData.SDA_Source)}
			</td>
			<td>
				{HtmlSanitizer.Sanitize(sourceData.SDA_Filename)}
			</td>
			<td>
				{HtmlSanitizer.Sanitize(sourceData.SDA_Filetype)}
			</td>
			<td>
				{HtmlSanitizer.Sanitize(sourceData.SDA_SubSource)}
			</td>
			<td>
				{HtmlSanitizer.Sanitize(sourceData.SDA_Status)}
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
