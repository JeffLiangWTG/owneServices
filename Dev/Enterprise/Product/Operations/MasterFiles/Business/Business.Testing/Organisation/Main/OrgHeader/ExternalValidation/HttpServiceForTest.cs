using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class HttpServiceForTest
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

				cts = new CancellationTokenSource();
				processRequestTask = Task.Run(() => ProcessRequest(cts.Token),cts.Token);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
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
				cts.Cancel();
				serviceListener.Stop();
				processRequestTask.Wait();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
			finally
			{
				cts?.Dispose();
			}
		}

		public static int GetFreeTcpPort()
		{
			TcpListener l = new TcpListener(IPAddress.Loopback, 0);
			l.Start();
			int port = ((IPEndPoint)l.LocalEndpoint).Port;
			l.Stop();
			return port;
		}

		#endregion

		#region Fields

		Task processRequestTask;
		CancellationTokenSource cts;
		HttpListener serviceListener;

		#endregion

		#region Private/Protected Members

		void ProcessRequest(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					var context = serviceListener.GetContext();
					ThreadPool.QueueUserWorkItem(_ => ProcessRequestCore(context));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					break;
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
					string responseText = String.Empty;
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
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		#endregion
	}
}
