using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CargoWise.RefDbRepo.ComplianceCommodityAlertListReferenceData.Test
{
	public class HttpServiceForTest : IDisposable
	{
		#region Properties

		/// <summary>
		/// Gets/Sets the service delay in milliseconds (for simulating service timeout).
		/// </summary>
		public int Delay { get; set; }

		/// <summary>
		/// Gets whether the service has started.
		/// </summary>
		public bool IsStarted { get; private set; }

		/// <summary>
		/// Gets/Sets the service methods.
		/// </summary>
		public string[] Methods { get; set; }

		/// <summary>
		/// Gets/Sets the response ContentType
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// Gets/Sets the processing logics of the service.
		/// </summary>
		public Func<Uri, string, Tuple<int, string>> Processor { get; set; }

		/// <summary>
		/// Gets/Sets the service URI.
		/// </summary>
		public Uri Uri { get; set; }

		/// <summary>
		/// Gets the request headers
		/// </summary>
		public NameValueCollection Headers { get; private set; }

		#endregion

		#region Public/Internal Instance Methods

		/// <summary>
		/// Starts the service.
		/// </summary>
		public void Start()
		{
			if (IsStarted)
			{
				return;
			}
			try
			{
				IsStarted = true;
				serviceListener = new HttpListener { AuthenticationSchemes = AuthenticationSchemes.Anonymous };
				serviceListener.Prefixes.Add(Uri.AbsoluteUri);
				serviceListener.Start();
				cancellationTokenSource = new CancellationTokenSource();
				processRequestThread = new Thread(new ParameterizedThreadStart(ProcessRequest));
				processRequestThread.Start(cancellationTokenSource);
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		/// Stops the service.
		/// </summary>
		public void Stop()
		{
			if (!IsStarted)
			{
				return;
			}
			IsStarted = false;
			try
			{
				cancellationTokenSource.Cancel();
				cancellationTokenSource.Dispose();
				cancellationTokenSource = null;
				serviceListener.Stop();
			}
			catch (Exception)
			{
			}
		}

		public static int GetFreeTcpPort()
		{
			using (TcpListener l = new TcpListener(IPAddress.Loopback, 0))
			{
				l.Start();
				int port = ((IPEndPoint)l.LocalEndpoint).Port;
				l.Stop();
				return port;
			}
		}

		#endregion

		#region Fields

		Thread processRequestThread;
		HttpListener serviceListener;
		CancellationTokenSource cancellationTokenSource;

		#endregion

		#region Private/Protected Members

		void ProcessRequest(object cancellationTokenSource)
		{
			var tokenSource = (CancellationTokenSource)cancellationTokenSource;
			using (tokenSource)
			{
				while (!tokenSource.IsCancellationRequested)
				{
					try
					{
						var context = serviceListener.GetContext();
						ThreadPool.QueueUserWorkItem(_ => ProcessRequestCore(context));
					}
					catch (Exception)
					{
						break;
					}
				}
			}
		}

		void ProcessRequestCore(HttpListenerContext context)
		{
			try
			{
				HttpListenerRequest request = context.Request;
				Headers = request.Headers;
				HttpListenerResponse response = context.Response;
				response.ContentType = ContentType;
				if (Methods != null && !Methods.Contains(request.HttpMethod, StringComparer.OrdinalIgnoreCase))
				{
					response.StatusCode = 405;
					response.Close();
					return;
				}
				if (Delay > 0)
				{
					Thread.Sleep(Delay);
				}
				using (StreamReader requestReader = new StreamReader(request.InputStream))
				{
					string requestText = requestReader.ReadToEnd();
					string responseText = string.Empty;
					if (Processor != null)
					{
						var responseMessage = Processor(request.Url, requestText);
						response.StatusCode = responseMessage.Item1;
						responseText = responseMessage.Item2;
					}
					using (StreamWriter responseWriter = new StreamWriter(response.OutputStream))
					{
						responseWriter.Write(responseText);
					}
					response.Close();
				}
			}
			catch (Exception)
			{
			}
		}

		public void Dispose()
		{
			((IDisposable)serviceListener)?.Dispose();
			cancellationTokenSource?.Dispose();
		}

		#endregion
	}
}
