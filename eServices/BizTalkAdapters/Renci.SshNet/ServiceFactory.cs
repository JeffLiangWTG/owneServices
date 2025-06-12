using System;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System.Diagnostics;

namespace Renci.SshNet
{
    /// <summary>
    /// Basic factory for creating new services.
    /// </summary>
    internal partial class ServiceFactory : IServiceFactory
    {
        /// <summary>
        /// Creates a new <see cref="ISession"/> with the specified <see cref="ConnectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="ConnectionInfo"/> to use for creating a new session.</param>
        /// <returns>
        /// An <see cref="ISession"/> for the specified <see cref="ConnectionInfo"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        public ISession CreateSession(ConnectionInfo connectionInfo)
        {
            return new Session(connectionInfo);
        }

        /// <summary>
        /// Creates a new <see cref="ISftpSession"/> in a given <see cref="ISession"/> and with
        /// the specified operation timeout and encoding.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> to create the <see cref="ISftpSession"/> in.</param>
        /// <param name="operationTimeout">The operation timeout.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="traceLog">A <see cref="TraceListener"/>.</param>
        /// <returns>
        /// An <see cref="ISftpSession"/>.
        /// </returns>
		/// <customization>
		/// Added 'traceLog' parameter.
		/// </customization>
		public ISftpSession CreateSftpSession(ISession session, TimeSpan operationTimeout, Encoding encoding, TraceListener traceLog)
        {
            return new SftpSession(session, operationTimeout, encoding, traceLog);
        }

        /// <summary>
        /// Create a new <see cref="PipeStream"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="PipeStream"/>.
        /// </returns>
        public PipeStream CreatePipeStream()
        {
            return new PipeStream();
        }
    }
}
