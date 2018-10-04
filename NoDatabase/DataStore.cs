using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

namespace NoDatabase
{
    public interface INoDatabaseEntity
    {
        object EntityKey { get; set; }
    }

    [Serializable]
    public abstract class NoDatabaseEntity : INoDatabaseEntity
    {
        public abstract object EntityKey { get; set; }
    }

    public class DataStore : IDisposable
    {
        private static object s_lockObject = new object();

        private IntPtr m_storage;

        private bool Create(string dbPath)
        {
            try
            {
                if (Directory.Exists(dbPath))
                    return true;

                Directory.CreateDirectory(dbPath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string m_dbPath;

        public DataStore(string dbPath)
        {
            if (Create(dbPath))
                m_dbPath = dbPath;
            else
                throw new DirectoryNotFoundException(string.Format("Unable to create or open database '{0}'", dbPath));
        }

        private string GetEncryptedString(string original)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(original);

            return Convert.ToBase64String(bytes);
        }

        /*
        public bool Insert<T>(T item) where T : class
        {
            lock(s_lockObject)
            {
                if (!(item is INoDatabaseEntity)) return false;

                try
                {
                    string keyField = "EntityKey";

                    Type t = typeof(T);

                    string className = t.Name;

                    PropertyInfo propInfo = t.GetProperty(keyField);

                    //string fileName = Path.Combine(m_dbPath, GetEncryptedString(className));
                    string fileName = Path.Combine(m_dbPath, className + ".stg");

                    //IntPtr storage = IntPtr.Zero;
                    if(m_storage == IntPtr.Zero)
                        NativeFunctions.OpenOrCreateStorage(fileName, out m_storage);

                    uint numStreams = NativeFunctions.GetNumStreams(m_storage);

                    TypeConverter tc = TypeDescriptor.GetConverter(propInfo.PropertyType);

                    propInfo.SetValue(item, tc.ConvertTo(numStreams, propInfo.PropertyType), null);

                    string streamName = string.Format("{0}_{1}", className, numStreams);

                    IntPtr stream;

                    NativeFunctions.OpenOrCreateStream(m_storage, streamName, out stream);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();

                        bf.Serialize(ms, item);

                        byte[] bytes = ms.GetBuffer();

                        NativeFunctions.UpdateStream(m_storage, stream, bytes, (uint)bytes.Length);
                    }
                }
                catch(Exception ex)
                {
                    return false;
                }

                return true;
            }
        }
        */

        public bool Save<T>(T item, object keyValue) where T : class
        {
            lock (s_lockObject)
            {
                if (!(item is INoDatabaseEntity)) return false;

                try
                {
                    Type t = typeof(T);

                    string className = t.Name;

                    PropertyInfo propInfo = t.GetProperty("EntityKey");

                    string fileName = Path.Combine(m_dbPath, className + ".stg");

                    if (m_storage == IntPtr.Zero)
                        NativeFunctions.OpenOrCreateStorage(fileName, out m_storage);

                    propInfo.SetValue(item, keyValue, null);

                    string streamName = string.Format("{0}_{1}", className, keyValue);

                    IntPtr stream;

                    NativeFunctions.OpenOrCreateStream(m_storage, streamName, out stream);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();

                        bf.Serialize(ms, item);

                        byte[] bytes = ms.GetBuffer();

                        uint numWritten = NativeFunctions.UpdateStream(m_storage, stream, bytes, (uint)bytes.Length);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            }
        }

        public bool Save<T>(T item, object keyValue, string keyField) where T : class
        {
            lock (s_lockObject)
            {
                if (string.IsNullOrWhiteSpace(keyField)) return false;

                try
                {
                    Type t = typeof(T);

                    string className = t.Name;

                    PropertyInfo propInfo = t.GetProperty(keyField);

                    string fileName = Path.Combine(m_dbPath, className + ".stg");

                    if (m_storage == IntPtr.Zero)
                        NativeFunctions.OpenOrCreateStorage(fileName, out m_storage);

                    propInfo.SetValue(item, keyValue, null);

                    string streamName = string.Format("{0}_{1}", className, keyValue);

                    IntPtr stream;

                    NativeFunctions.OpenOrCreateStream(m_storage, streamName, out stream);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();

                        bf.Serialize(ms, item);

                        byte[] bytes = ms.GetBuffer();

                        uint numWritten = NativeFunctions.UpdateStream(m_storage, stream, bytes, (uint)bytes.Length);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            }
        }
        
        public T Get<T>(object keyValue) where T : class, new()
        {
            Type t = typeof(T);

            if (t.GetInterface("INoDatabaseEntity") == null) return default(T);

            string className = t.Name;

            PropertyInfo propInfo = t.GetProperty("EntityKey");

            string fileName = Path.Combine(m_dbPath, className + ".stg");

            if (m_storage == IntPtr.Zero)
                NativeFunctions.OpenOrCreateStorage(fileName, out m_storage);

            string streamName = string.Format("{0}_{1}", className, keyValue);

            IntPtr stream;

            NativeFunctions.OpenOrCreateStream(m_storage, streamName, out stream);

            byte[] bytes;
            ulong numRead;

            NativeFunctions.ReadStream(stream, out bytes, out numRead);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                if (ms.Length > 0)
                {
                    BinaryFormatter bf = new BinaryFormatter();

                    T item = bf.Deserialize(ms) as T;

                    return item;
                }
            }

            return default(T);
        }

        public T Get<T>(object keyValue, string keyField) where T: class, new()
        {
            if (string.IsNullOrWhiteSpace(keyField)) return default(T);

            Type t = typeof(T);
            string className = t.Name;

            PropertyInfo propInfo = t.GetProperty(keyField);

            string fileName = Path.Combine(m_dbPath, className + ".stg");

            if (m_storage == IntPtr.Zero)
                NativeFunctions.OpenOrCreateStorage(fileName, out m_storage);

            string streamName = string.Format("{0}_{1}", className, keyValue);

            IntPtr stream;

            NativeFunctions.OpenOrCreateStream(m_storage, streamName, out stream);

            byte[] bytes;
            ulong numRead;

            NativeFunctions.ReadStream(stream, out bytes, out numRead);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                if (ms.Length > 0)
                {
                    BinaryFormatter bf = new BinaryFormatter();

                    T item = bf.Deserialize(ms) as T;

                    return item;
                }
            }

            return default(T);
        }

        public bool Delete<T>(T item) where T : class
        {
            try
            {
                Type t = typeof(T);

                string className = t.Name;

                string fileName = Path.Combine(m_dbPath, className + ".stg");

                if (m_storage == IntPtr.Zero)
                    NativeFunctions.OpenOrCreateStorage(fileName, out m_storage);

                PropertyInfo propInfo = t.GetProperty("EntityKey");

                object keyVal = propInfo.GetValue(item, null);

                string streamName = string.Format("{0}_{1}", className, keyVal);

                NativeFunctions.DeleteStream(m_storage, streamName);
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        public T[] GetAll<T>() where T : class, new()
        {
            List<T> retVal = new List<T>();

            Type t = typeof(T);

            string className = t.Name;

            string fileName = Path.Combine(m_dbPath, className + ".stg");

            if (!File.Exists(fileName))
                return retVal.ToArray();

            if (m_storage == IntPtr.Zero)
                NativeFunctions.OpenOrCreateStorage(fileName, out m_storage);

            StreamInfo[] streamNames;

            NativeFunctions.GetStreams(m_storage, out streamNames);

            foreach(StreamInfo streamName in streamNames)
            {
                IntPtr stream;

                NativeFunctions.OpenOrCreateStream(m_storage, streamName.StreamName, out stream);

                byte[] bytes;
                ulong numRead;

                NativeFunctions.ReadStream(stream, out bytes, out numRead);

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    if (ms.Length > 0)
                    {
                        BinaryFormatter bf = new BinaryFormatter();

                        T item = bf.Deserialize(ms) as T;

                        retVal.Add(item);
                    }
                }
            }

            return retVal.ToArray();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if(m_storage != null && m_storage != IntPtr.Zero)
                {
                    NativeFunctions.CloseStorage(m_storage);

                    m_storage = IntPtr.Zero;
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~DataStore()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
