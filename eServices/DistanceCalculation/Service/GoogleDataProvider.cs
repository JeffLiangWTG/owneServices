using System;
using System.IO;
using System.Net;

namespace Enterprise.Freight.DistanceCalculation.Service
{
	public class GoogleDataProvider : IDisposable
	{
		readonly string url;
		HttpWebResponse response;

		public GoogleDataProvider(string url)
		{
			this.url = url;
		}

		public Stream GetData()
		{
			var request = (HttpWebRequest)HttpWebRequest.Create(url);
			response = (HttpWebResponse)request.GetResponse();
			return response.GetResponseStream();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~GoogleDataProvider()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (response != null)
					{
						try
						{
							response.Close();
						}
						catch { }
						response = null;
					}
				}
			}
			disposed = true;
		}

		bool disposed;
	}
}