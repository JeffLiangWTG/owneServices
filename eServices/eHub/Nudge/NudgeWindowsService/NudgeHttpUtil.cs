using System;
using System.Net;
using Common.Logging;

namespace CargoWise.eHub.Nudge
{
	public class NudgeHttpUtil
	{
		public virtual int MakeHttpRequest(string url)
		{
			var status = -1;

			try
			{
			    var request = (HttpWebRequest)WebRequest.Create(url);

				using (var response = (HttpWebResponse)request.GetResponse())
				{
					status = (int)response.StatusCode;
				}
			}
			catch (WebException e)
			{
				if (e.Response != null)
				{
					status = (int)((HttpWebResponse)e.Response).StatusCode;
				}
				Logger.Warn("MakeHttpRequest failed with exception ", e);
			}
			catch (Exception e)
			{
				Logger.Warn("MakeHttpRequest failed with exception ", e);
			}

			return status;
		}

		public static NudgeHttpUtil Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new NudgeHttpUtil();
				}
				return instance;
			}
			set
			{
				instance = value;
			}
		}

		static NudgeHttpUtil instance;
		static readonly ILog Logger = LogManager.GetLogger(typeof(NudgeHttpUtil));
	}
}
