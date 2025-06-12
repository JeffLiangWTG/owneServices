using System;
using System.IO;

namespace Hawking.Xslt.Compiler.ScriptsCompiler
{
    public class CompilationEmit : IDisposable
    {
        public CompilationEmit(string csharpCode)
        {
            CSharpCode = csharpCode;
            DllMemoryStream = new MemoryStream();
            PdbMemoryStream = new MemoryStream();
        }

        public MemoryStream DllMemoryStream { get; }
        public MemoryStream PdbMemoryStream { get; }
        public string CSharpCode { get; }
        public bool EmitSuccess { get; set; }

        public void SeekOrigin()
        {
            DllMemoryStream?.Seek(0L, System.IO.SeekOrigin.Begin);
            PdbMemoryStream?.Seek(0L, System.IO.SeekOrigin.Begin);
        }

        #region IDisposable

        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeInternal();
            }

            disposed = true;
        }

        void DisposeInternal()
        {
            DllMemoryStream?.Dispose();
            PdbMemoryStream?.Dispose();
        }

        ~CompilationEmit()
        {
            Dispose(true);
        }

        #endregion
    }
}
