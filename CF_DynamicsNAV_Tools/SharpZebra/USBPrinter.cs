﻿#region License
/* ---------------------------------------------------------------------------
 * Creative Commons License
 * http://creativecommons.org/licenses/by/2.5/au/
 *
 * Attribution 2.5 Australia
 *
 * You are free:
 *
 * - to copy, distribute, display, and perform the work 
 * - to make derivative works 
 * - to make commercial use of the work 
 *
 * Under the following conditions:
 *
 * Attribution: You must attribute the work in the manner specified by the
 *              author or licensor. 
 *
 * For any reuse or distribution, you must make clear to others the license
 * terms of this work.  Any of these conditions can be waived if you get
 * permission from the copyright holder.  Your fair use and other rights
 * are in no way affected by the above.
 *
 * This is a human-readable summary of the Legal Code (the full license). 
 * http://creativecommons.org/licenses/by/2.5/au/legalcode
 * ------------------------------------------------------------------------ */
#endregion License

using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Com.SharpZebra.Printing
{
    public class USBPrinter: IZebraPrinter
    {
        public PrinterSettings Settings { get; set; }

        public USBPrinter(PrinterSettings settings)
        {
            Settings = settings;
        }

        public void Print(byte[] data)
        {
            UsbPrinterConnector connector = new UsbPrinterConnector(Settings.PrinterName);
            connector.BeginSend();          
            connector.Send(data, 0, data.Length);
        }
    }

    public class UsbPrinterConnector
    {
        public static readonly int DefaultReadTimeout = 200;
        public static readonly int DefaultWriteTimeout = 200;
        private int readTimeout = DefaultReadTimeout;
        public int ReadTimeout
        {
            get { return readTimeout; }
            set { readTimeout = value; }
        }

        private int writeTimeout = DefaultWriteTimeout;

        public int WriteTimeout
        {
            get { return writeTimeout; }
            set { writeTimeout = value; }
        }

        #region EnumDevices

        static Guid GUID_DEVICEINTERFACE_USBPRINT = new Guid(
                0x28d78fad, 0x5a12, 0x11D1,
                0xae, 0x5b, 0x00, 0x00, 0xf8, 0x03, 0xa8, 0xc2);
        
        public static Dictionary<string, string> EnumDevices()
        {
            RegistryKey regKey, subKey;
            string portValue, path;
            int portNumber;
            Dictionary<string, string> printers = new Dictionary<string, string>();
            Dictionary<int, string> printerNames = new Dictionary<int, string>();
            try
            {
                RegistryKey nameKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Print\\Printers");
                foreach (string printer in nameKey.GetSubKeyNames())
                {
                    subKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Print\\Printers\\" + printer);
                    portValue = subKey.GetValue("Port").ToString();
                    if (portValue.Substring(0, 3) == "USB")
                    {
                        if (int.TryParse(portValue.Substring(3, portValue.Length - 3), out portNumber))
                        {
                            if (!printerNames.ContainsKey(portNumber))
                                printerNames.Add(portNumber, printer);
                        }
                    }
                    subKey.Close();
                }
                nameKey.Close();
            }
            catch
            {
                //do nothing (no printers?)
            }
            try
            {
                regKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\DeviceClasses\\" +
                    GUID_DEVICEINTERFACE_USBPRINT.ToString("B"));
                try
                {
                    foreach (string sub in regKey.GetSubKeyNames())
                    {
                        if (sub.Substring(0, 16).ToUpper() == "##?#USB#VID_0A5F")
                        {
                            subKey = Registry.LocalMachine.OpenSubKey(
                                string.Format("SYSTEM\\CurrentControlSet\\Control\\DeviceClasses\\{0:B}\\{1}\\#",
                                GUID_DEVICEINTERFACE_USBPRINT, sub));
                            path = subKey.GetValue("SymbolicLink").ToString();
                            subKey.Close();

                            subKey = Registry.LocalMachine.OpenSubKey(
                                string.Format("SYSTEM\\CurrentControlSet\\Control\\DeviceClasses\\{0:B}\\{1}\\#\\Device Parameters",
                                GUID_DEVICEINTERFACE_USBPRINT, sub));                            
                            if (int.TryParse(subKey.GetValue("Port Number").ToString(), out portNumber))
                            {
                                if (printerNames.ContainsKey(portNumber))
                                    printers.Add(printerNames[portNumber], path);
                            }
                            subKey.Close();
                        }
                    }
                }
                finally
                {
                    regKey.Close();
                }
            }
            catch
            {
                // do nothing
            }
            return printers;
        }

        #endregion EnumDevices

        private string interfaceName;

        private IntPtr usbHandle = IntPtr.Zero;

        public static readonly uint ReadBufferSize = 512;

        private byte[] readBuffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="InterfaceName"></param>
        public UsbPrinterConnector(string PrinterName)
            : base()
        {
            
            Dictionary<string, string> plist = EnumDevices();
            if (plist.ContainsKey(PrinterName))
                this.interfaceName = plist[PrinterName];
            else
                throw new Exception("Cannot locate USB device");            
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~UsbPrinterConnector()
        {
            SetConnected(false);
        }

        protected void SetConnected(bool value)
        {
            if (value)
            {
                if ((int)usbHandle > 0)
                    SetConnected(false);

                /* C++ Decl.
                usbHandle = CreateFile(
                    interfacename, 
                    GENERIC_WRITE, 
                    FILE_SHARE_READ,
                    NULL, 
                    OPEN_ALWAYS, 
                    FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, 
                    NULL);
                */

                usbHandle = FileIO.CreateFile(
                    interfaceName,
                    FileIO.FileAccess.GENERIC_WRITE | FileIO.FileAccess.GENERIC_READ,
                    FileIO.FileShareMode.FILE_SHARE_READ,
                    IntPtr.Zero,
                    FileIO.FileCreationDisposition.OPEN_ALWAYS,
                    FileIO.FileAttributes.FILE_ATTRIBUTE_NORMAL |
                        FileIO.FileAttributes.FILE_FLAG_SEQUENTIAL_SCAN |
                        FileIO.FileAttributes.FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);
                if ((int)usbHandle <= 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
                if ((int)usbHandle > 0)
                {
                    FileIO.CloseHandle(usbHandle);
                    usbHandle = IntPtr.Zero;
                }
        }

        protected bool GetConnected()
        {
            try
            {
                SetConnected(true);
                return ((int)usbHandle > 0);
            }
            catch (Win32Exception)  //printer is not online
            {
                return false;
            }            
        }

        public bool BeginSend()
        {
            return GetConnected();
        }

        public int Send(byte[] buffer, int offset, int count)
        {
            // USB 1.1 WriteFile maximum block size is 4096
            uint size;
            byte[] bytes;

            if (!GetConnected())
                throw new ApplicationException("Not connected");

            if (count > 4096)
            {
                int current = 0;
                int total = 0;
                while (current < count)
                {
                    int newcount = current + 4096 > count ? count - current : 4096;
                    total += Send(buffer, current, newcount);
                    current += 4096;
                }
                return total;
                //throw new NotImplementedException();  // TODO: Copy byte array loop
            }
            else
            {
                bytes = new byte[count];
                Array.Copy(buffer, offset, bytes, 0, count);
                ManualResetEvent wo = new ManualResetEvent(false);
                NativeOverlapped ov = new NativeOverlapped();
                // ov.OffsetLow = 0; ov.OffsetHigh = 0;
                ov.EventHandle = wo.SafeWaitHandle.DangerousGetHandle();
                if (!FileIO.WriteFile(usbHandle, bytes, (uint)count, out size, ref ov))
                {
                    if (Marshal.GetLastWin32Error() == FileIO.ERROR_IO_PENDING)
                        wo.WaitOne(WriteTimeout, false);
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                FileIO.GetOverlappedResult(usbHandle, ref ov, out size, true);
                return (int)size;
            }
        }

        public int Read(out byte[] buffer)
        {
            // USB 1.1 ReadFile in block chunks of 64 bytes
            // USB 2.0 ReadFile in block chunks of 512 bytes
            uint read;

            if (readBuffer == null)
                readBuffer = new byte[ReadBufferSize];

            AutoResetEvent sg = new AutoResetEvent(false);
            NativeOverlapped ov = new NativeOverlapped();
            ov.OffsetLow = 0;
            ov.OffsetHigh = 0;
            ov.EventHandle = sg.SafeWaitHandle.DangerousGetHandle();

            if (!FileIO.ReadFile(usbHandle, readBuffer, ReadBufferSize, out read, ref ov))
            {
                if (Marshal.GetLastWin32Error() == FileIO.ERROR_IO_PENDING)
                    sg.WaitOne(ReadTimeout, false);
                else
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            FileIO.GetOverlappedResult(usbHandle, ref ov, out read, true);
            buffer = new byte[read];
            Array.Copy(readBuffer, buffer, read);
            return (int)read;
        }
    }
        
    
    internal class FileIO
    {

        internal const int INVALID_HANDLE_VALUE = -1;

        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_INVALID_NAME = 123;
        internal const int ERROR_ACCESS_DENIED = 5;
        internal const int ERROR_IO_PENDING = 997;
        internal const int ERROR_IO_INCOMPLETE = 996;

        internal class NullClass
        {
            public NullClass()
            {
                throw new Exception("Cannot create instance of NullClass");
            }
        }

        #region CreateFile

        [Flags]
        internal enum FileAccess : uint  // from winnt.h
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000
        }

        [Flags]
        internal enum FileShareMode : uint  // from winnt.h
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x00000002,
            FILE_SHARE_DELETE = 0x00000004
        }

        internal enum FileCreationDisposition : uint  // from winbase.h
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5
        }

        [Flags]
        internal enum FileAttributes : uint  // from winnt.h
        {
            FILE_ATTRIBUTE_READONLY = 0x00000001,
            FILE_ATTRIBUTE_HIDDEN = 0x00000002,
            FILE_ATTRIBUTE_SYSTEM = 0x00000004,
            FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
            FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
            FILE_ATTRIBUTE_DEVICE = 0x00000040,
            FILE_ATTRIBUTE_NORMAL = 0x00000080,
            FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
            FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,
            FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,
            FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
            FILE_ATTRIBUTE_OFFLINE = 0x00001000,
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
            FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,

            // from winbase.h
            FILE_FLAG_WRITE_THROUGH = 0x80000000,
            FILE_FLAG_OVERLAPPED = 0x40000000,
            FILE_FLAG_NO_BUFFERING = 0x20000000,
            FILE_FLAG_RANDOM_ACCESS = 0x10000000,
            FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
            FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
            FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
            FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
            FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
            FILE_FLAG_OPEN_NO_RECALL = 0x00100000,
            FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FileShareMode dwShareMode,
            IntPtr lpSecurityAttributes,
            FileCreationDisposition dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        #endregion CreateFile

        #region CloseHandle

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        #endregion CloseHandle

        #region GetOverlappedResult

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetOverlappedResult(
            IntPtr hFile,
            /* IntPtr */ ref System.Threading.NativeOverlapped lpOverlapped,
            out uint nNumberOfBytesTransferred,
            bool bWait);

        #endregion GetOverlappedResult

        #region WriteFile

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "WriteFile")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WriteFile0(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPArray)]
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            NullClass lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WriteFile(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            [In] ref System.Threading.NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int WriteFileEx(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer,
            int nNumberOfBytesToWrite,
            [In] ref System.Threading.NativeOverlapped lpOverlapped,
            [MarshalAs(UnmanagedType.FunctionPtr)] IOCompletionCallback callback
        );

        #endregion WriteFile

        #region ReadFile

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadFile(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            [In] ref System.Threading.NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "ReadFile")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadFile0(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            NullClass lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int ReadFileEx(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer,
            int nNumberOfBytesToRead,
            [In] ref System.Threading.NativeOverlapped lpOverlapped,
            [MarshalAs(UnmanagedType.FunctionPtr)] IOCompletionCallback callback);

        #endregion ReadFile

    }

}
