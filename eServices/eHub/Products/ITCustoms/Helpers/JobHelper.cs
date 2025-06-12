using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.eHubTransactions;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace CargoWise.eHub.Products.ITCustoms.Helpers
{
    public static class JobHelper
	{
		public static string Base36Encode(int input)
		{
			if (input < 0) throw new ArgumentOutOfRangeException("input cannot be negative: " + input);
			if (input > 1295) throw new ArgumentOutOfRangeException("input cannot be over 1295: " + input);

			var clistarr = Base36CharList.ToCharArray();
			var result = new Stack<char>();
			while (input != 0)
			{
				result.Push(clistarr[input % 36]);
				input /= 36;
			}
			return new string(result.ToArray()).PadLeft(2, '0');
		}

		public static int Base36Decode(string input)
		{
			if (input.Length > 2) throw new ArgumentOutOfRangeException("input is limited to two characters: " + input);
			var reversed = input.ToUpper().Reverse();
			var result = 0;
			var pos = 0;
			foreach (var c in reversed)
			{
				result += Base36CharList.IndexOf(c) * (int)Math.Pow(36, pos);
				if (result < 0) throw new ArgumentOutOfRangeException("input must be alphanumeric: " + input);
				pos++;
			}
			return result;
		}

		public static string GetFileNamePrefix(string node, string fileType, ILog logger)
		{
			var date = GetItalyCurrentMonthAndDate();
			var prefix = node + date + "." + fileType;
			return prefix;
		}

		public static long GetCurrentMaxFileNumber(string prefix, ILog logger)
		{
			using (var context = GetContext())
			{
				var transformation = context.eHubTransformationSets.Where(x => x.TS_Name == TransformationNameMessageCounter).FirstOrDefault();
				var counter =
					context.eHubInterfaceCounters.Where(x => x.CT_Name == prefix && x.eHubTransformationSet.TS_Name == TransformationNameMessageCounter)
						.FirstOrDefault();

				if (counter == null)
				{
					context.eHubInterfaceCounters.Add(
						new eHubInterfaceCounter { CT_Name = prefix, eHubTransformationSet = transformation, CT_LastUpdateUTC = DateTime.UtcNow, CT_Cleanup = true });
					context.SaveChanges();
				}
				return counter == null ? -1 : counter.CT_Value;
			}
		}

		public static void CheckFileNumberIsMaximum(string prefix, ILog logger)
		{
			if (GetCurrentMaxFileNumber(prefix, logger) >= 1295)
				throw new FatalMessageProcessingException("Max Daily File Limit Reached.");
		}

		public static DateTime GetItalyTime(DateTime time, string timeZoneId = "UTC")
		{
			return TimeZoneInfo.ConvertTime(time, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId),
				TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
		}

		public static string GetItalyCurrentMonthAndDate()
		{
			var currentDateAndMonth = GetItalyTime(DateTime.UtcNow);
			return currentDateAndMonth.ToString("MMdd");
		}

		public static string GenerateFileName(string prefix, long fileNumber, ILog logger)
		{
			return prefix + Base36Encode((int)fileNumber);
		}

		public static void UpdateFileCounter(string prefix, long fileNumber, ILog logger)
		{
			using (var context = GetContext())
			{
				var counter =
					context.eHubInterfaceCounters.Where(x => x.CT_Name == prefix &&
																									 x.eHubTransformationSet.TS_Name == TransformationNameMessageCounter)
						.FirstOrDefault();
				counter.CT_Value = fileNumber;
				counter.CT_LastUpdateUTC = DateTime.UtcNow;
				context.SaveChanges();
			}
		}

		public static string GenerateHeader(string fileName, string officeCode, string accountNumber,
			int numberOfLines, ILog logger)
		{
			var accountSplit = accountNumber.Split('-');
			var header = fileName.Substring(0, 4) + new string(' ', 12) + fileName +
									 new string(' ', 12) + officeCode + new string(' ', 4) + accountSplit[0] + new string(' ', 5) + accountSplit[1] +
									 new string(' ', 1) +
									 numberOfLines.ToString().PadLeft(5, '0') + Environment.NewLine;
			return header;
		}

		public static int GetLineCount(string message)
		{
			return message.Trim().Split('\n').Length;
		}

		public static string TryGetErrorDescriptionsFromAnswerFile(XmlDocument answerXml)
		{
			var results = answerXml.GetElementsByTagName("DetailOrQueue");
			var sb = new StringBuilder();
			for (int i = 0; i < results.Count; i++)
			{
				var errorCode = results[i].InnerText.Substring(16, 1);
				if (Char.IsDigit(results[i].InnerText[0]) && errorCode != "0")
				{
					sb.AppendLine(GetErrorDescriptionFromCodeMapping(errorCode));
				}
			}
			return sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
		}

		private static string GetErrorDescriptionFromCodeMapping(string errorCode)
		{
			using (var context = GetContext())
			{
				var codeMapValue = context.eHubCodeMapValues
					.Single(x => x.eHubCodeMapKey.eHubCodeSet.eHubTransformationSet.TS_Name ==
											 TransformationNameMessageErrorCode &&
											 x.eHubCodeMapKey.eHubCodeSet.CS_Name == CodeSetNameErrorCode &&
											 x.eHubCodeMapKey.CK_Key1Value == errorCode);
				if (codeMapValue == null || string.IsNullOrWhiteSpace(codeMapValue.CV_OutputCode))
					throw new InvalidOperationException("Could not find ITCustoms answer file error description using error code: " + errorCode);
				return codeMapValue.CV_OutputCode;
			}
		}

		public static NextJobStatusAndPollStartUTC GetNextJobStatusAndPollStartUTC(string lastStatus, 
			string transformationNameJobStatus = TransformationNameJobStatus)
		{
			using (var context = GetContext())
			{
                string nextStatus;
                eHubCodeMapKey nextCodeMapKey;
                Regex regex = new Regex(@"^[A-Za-z]\d{2}$");

                if (!string.IsNullOrWhiteSpace(lastStatus) && regex.IsMatch(lastStatus))
                {
                    var currentStatusKey = lastStatus[0].ToString();
                    var currentRepeatTimes = int.Parse(lastStatus.Substring(1)) + 1;

                    var currentCodeMapKey = context.eHubCodeMapKeys
                        .Single(x => x.eHubCodeSet.eHubTransformationSet.TS_Name == transformationNameJobStatus
								&& x.eHubCodeSet.CS_Name == CodeSetNameJobStatus
                                && x.CK_Key1Value == currentStatusKey);

                    var currentRepeatTimesSetting = int.Parse(context.eHubCodeMapValues
                        .Single(x => x.CV_CK == currentCodeMapKey.CK_PK && x.eHubCodeSetResult.CR_Name == CodeSetResultNameRepeatTimes)
                        .CV_OutputCode);

                    if(currentRepeatTimes < currentRepeatTimesSetting)
                    {
                        nextStatus = currentStatusKey + currentRepeatTimes.ToString("D2");
                        nextCodeMapKey = currentCodeMapKey;
                    }
                    else
                    {
                        nextCodeMapKey = context.eHubCodeMapKeys
                            .Single(x => x.eHubCodeSet.eHubTransformationSet.TS_Name == transformationNameJobStatus
									&& x.eHubCodeSet.CS_Name == CodeSetNameJobStatus
                                    && x.CK_Order == currentCodeMapKey.CK_Order + 1);

                        nextStatus = nextCodeMapKey.CK_Key1Value.ToUpper() != "MIA"
                                    ? nextCodeMapKey.CK_Key1Value + "00"
                                    : nextCodeMapKey.CK_Key1Value;
                    }
                }
				else
				{
					nextCodeMapKey = context.eHubCodeMapKeys
						.Single(x => x.eHubCodeSet.eHubTransformationSet.TS_Name == transformationNameJobStatus
								&& x.eHubCodeSet.CS_Name == CodeSetNameJobStatus
                                && x.CK_Order == 1);

                    nextStatus = nextCodeMapKey.CK_Key1Value + "00";
                }

                var nextPollStartUTC = DateTime.UtcNow.AddMinutes(Convert.ToDouble(context.eHubCodeMapValues
                    .Single(x => x.CV_CK == nextCodeMapKey.CK_PK && x.eHubCodeSetResult.CR_Name == CodeSetResultNamePollInterval)
                    .CV_OutputCode));

                return new NextJobStatusAndPollStartUTC(nextStatus, XmlConvert.ToString(nextPollStartUTC, XmlDateTimeSerializationMode.Unspecified));
			}
		}

        public static string GetNewFilenameByType(string oldName, string newType)
        {
            if(oldName.Length != 12)
            {
                throw new FatalMessageProcessingException("Invalid filename: "+ oldName);
            }

            var newName = oldName.Substring(0, 9) + newType + oldName.Substring(10, 2);
            return newName;
        }

		internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();
		private const string Base36CharList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private const string TransformationNameMessageCounter = "ITCustoms Outbound Message Counter";
		private const string TransformationNameJobStatus = "ITCustoms Response Message Job Status";
		private const string CodeSetNameJobStatus = "JobStatus";
		private const string TransformationNameMessageErrorCode = "ITCustoms Response Message Error Code";
		private const string CodeSetNameErrorCode = "AnswerFileError";
		private const string CodeSetResultNamePollInterval = "Poll Interval";
		private const string CodeSetResultNameRepeatTimes = "Repeat Times";

        [Serializable]
		public struct NextJobStatusAndPollStartUTC
		{
			public NextJobStatusAndPollStartUTC(string status, string pollStartUTCString)
				: this()
			{
				Status = status;
				PollStartUTCString = pollStartUTCString;
			}
			public string Status { get; private set; }
			public string PollStartUTCString { get; private set; }
		}
	}
}
