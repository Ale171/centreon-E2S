using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Centreon_EventLog_2_Syslog
{
    class SyslogServer
    {
        private String _ServerAddress;
        private String _Protocol = "udp";
        private int _ServerPort = 514;
        private int _FileBufferMaxSizeInMB = 10;
        private int _MemoryBufferMaxSize = 200;

        /// <summary>
        /// Definition of distant syslog server. This constructor set UDP protocol.
        /// </summary>
        /// <param name="serverAddress">IP address or DNS</param>
        public SyslogServer(String serverAddress)
        {
            SetServerAddress(serverAddress);
        }

        /// <summary>
        /// Definition of distant syslog server
        /// </summary>
        /// <param name="serverAddress">IP address or DNS</param>
        /// <param name="protocol">Protocol: UDP or TCP</param>
        public SyslogServer(String serverAddress, String protocol)
        {
            SetServerAddress(serverAddress);
            SetProtocol(protocol);
        }

        /// <summary>
        /// Definition of distant syslog server
        /// </summary>
        /// <param name="serverAddress">IP address or DNS</param>
        /// <param name="protocol">Protocol: UDP or TCP</param>
        /// <param name="port">Integer between 1 to 65535</param>
        public SyslogServer(String serverAddress, String protocol, int port)
        {
            SetServerAddress(serverAddress);
            SetPort(port);
            SetProtocol(protocol.ToLower());
        }

        /// <summary>
        /// Definition of distant syslog server
        /// </summary>
        /// <param name="serverAddress">IP address or DNS</param>
        /// <param name="protocol">Protocol: UDP or TCP</param>
        /// <param name="port">Integer between 1 to 65535</param>
        /// <param name="memoryBufferMaxSize">Memory buffer size. Only used for TCP protocol.</param>
        public SyslogServer(String serverAddress, String protocol, int port, int memoryBufferMaxSize)
        {
            SetServerAddress(serverAddress);
            SetPort(port);
            SetProtocol(protocol.ToLower());
            this._MemoryBufferMaxSize = memoryBufferMaxSize;
        }

        /// <summary>
        /// Definition of distant syslog server
        /// </summary>
        /// <param name="serverAddress">IP address or DNS</param>
        /// <param name="protocol">Protocol: UDP or TCP</param>
        /// <param name="port">Integer between 1 to 65535</param>
        /// <param name="memoryBufferMaxSize">Memory buffer size. Only used for TCP protocol.</param>
        /// <param name="fileBufferMaxSizeInMB">File buffer max size in MB. Only used for TCP protocol.</param>
        public SyslogServer(String serverAddress, String protocol, int port, int memoryBufferMaxSize, int fileBufferMaxSizeInMB)
        {
            SetServerAddress(serverAddress);
            SetPort(port);
            SetProtocol(protocol.ToLower());
            this._MemoryBufferMaxSize = memoryBufferMaxSize;
            this._FileBufferMaxSizeInMB = fileBufferMaxSizeInMB;
        }

        /// <summary>
        /// Set and control port number
        /// </summary>
        /// <param name="port">Integer between 1 to 65535</param>
        private void SetPort(int port)
        {
            if ((port == 0) || (port > 65535))
            {
                ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("Argument is out of bounds, please use integer into range [1-65535]");
                throw ex;

            }
            this._ServerPort = port;
        }

        /// <summary>
        /// Set and control protocol
        /// </summary>
        /// <param name="protocol">Protocol: UDP or TCP</param>
        private void SetProtocol(String protocol)
        {
            if ((protocol.ToLower().CompareTo("udp") != 0) || (protocol.ToLower().CompareTo("tcp") != 0))
            {
                ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("Argument must be UDP or TCP");
                throw ex;

            }
            this._Protocol = protocol;
        }

        /// <summary>
        /// Set and control IP address or DNS
        /// </summary>
        /// <param name="server">IP address or DNS</param>
        private void SetServerAddress(String server)
        {
            String validIpAddressRegex = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            String validHostnameRegex = "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\\-]*[A-Za-z0-9])$";

            Regex re = new Regex(validIpAddressRegex);

            if (re.IsMatch(server, 0))
            {
                this._ServerAddress = server;
            }
            else
            {
                re = new Regex(validHostnameRegex);

                if (re.IsMatch(server, 0))
                {
                    this._ServerAddress = server;
                }
                else
                {
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("Address IP or DNS is not correct.");
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Send event to syslog server
        /// </summary>
        /// <param name="eventLogEntry"></param>
        /// <param name="debug"></param>
        public void SendEvent(EventLogEntry eventLogEntry, Debug debug)
        {
        }

        /// <summary>
        /// Obtain all properties of this object
        /// </summary>
        /// <returns>String included all properties of this object.</returns>
        new public String ToString()
        {
            String temp = "ServerAddress: " + this._ServerAddress + ", Port: " + this._ServerPort + ", Protocol: " + this._Protocol;
            
            if (this._Protocol.CompareTo("tcp") == 0)
            {
                temp += " , FileBufferMaxSize: " + this._FileBufferMaxSizeInMB + " MB, MemoryBufferMaxSize: " + this._MemoryBufferMaxSize;
            }

            return temp;
        }
    }
}
