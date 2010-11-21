//##################################################################################
//# Copyright 2005-2010 MERETHIS
//# Centreon is developped by : Julien Mathis and Romain Le Merlus under
//# GPL Licence 2.0.
//# 
//# This program is free software; you can redistribute it and/or modify it under 
//# the terms of the GNU General Public License as published by the Free Software 
//# Foundation ; either version 2 of the License.
//# 
//# This program is distributed in the hope that it will be useful, but WITHOUT ANY
//# WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A 
//# PARTICULAR PURPOSE. See the GNU General Public License for more details.
//# 
//# You should have received a copy of the GNU General Public License along with 
//# this program; if not, see <http://www.gnu.org/licenses>.
//# 
//# Linking this program statically or dynamically with other modules is making a 
//# combined work based on this program. Thus, the terms and conditions of the GNU 
//# General Public License cover the whole combination.
//# 
//# As a special exception, the copyright holders of this program give MERETHIS 
//# permission to link this program with independent modules to produce an executable, 
//# regardless of the license terms of these independent modules, and to copy and 
//# distribute the resulting executable under terms of MERETHIS choice, provided that 
//# MERETHIS also meet, for each linked independent module, the terms  and conditions 
//# of the license of that module. An independent module is a module which is not 
//# derived from this program. If you modify this program, you may extend this 
//# exception to your version of the program, but you are not obliged to do so. If you
//# do not wish to do so, delete this exception statement from your version.
//# 
//# For more information : contact@centreon.com
//# 
//# SVN : $URL
//# SVN : $Id :
//#
//####################################################################################
//#
//# Dependent plugin : .NET Framework
//# Dependent plugin version : 2.0.x
//#
//####################################################################################
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Centreon_EventLog_2_Syslog
{
    class SyslogServer
    {
        private String _ServerAddress;
        private String _Protocol = "udp";
        private int _ServerPort = 514; private int _MemoryBufferMaxSize = 200;

        private Hashtable Level = new Hashtable();
        private Hashtable Facility = new Hashtable();

        private Queue SendQueue = new Queue();
        public Thread Sending_thread = null;

        public bool connected = false;
        public bool sending = false;

        private Debug _Debug;

        #region Constructors

        /// <summary>
        /// Definition of distant syslog server. This constructor set UDP protocol.
        /// </summary>
        /// <param name="serverAddress">IP address or DNS</param>
        public SyslogServer(String serverAddress)
        {
            SetServerAddress(serverAddress);
            SetSyslogLevelAndFacility();
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
            SetSyslogLevelAndFacility();
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
            SetSyslogLevelAndFacility();
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
            SetSyslogLevelAndFacility();
        }

        /// <summary>
        /// Definition of distant syslog server
        /// </summary>
        /// <param name="serverAddress">IP address or DNS</param>
        /// <param name="protocol">Protocol: UDP or TCP</param>
        /// <param name="port">Integer between 1 to 65535</param>
        /// <param name="memoryBufferMaxSize">Memory buffer size. Only used for TCP protocol.</param>
        /// <param name="debug">Debug object</param>
        public SyslogServer(String serverAddress, String protocol, int port, int memoryBufferMaxSize, ref Debug debug)
        {
            SetServerAddress(serverAddress);
            SetPort(port);
            SetProtocol(protocol.ToLower());
            this._MemoryBufferMaxSize = memoryBufferMaxSize;
            SetSyslogLevelAndFacility();
            this._Debug = debug;
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
            if ((protocol.ToLower().CompareTo("udp") == 0) || (protocol.ToLower().CompareTo("tcp") == 0))
            {
                this._Protocol = protocol;
            }
            else
            {
                ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("Argument must be UDP or TCP");
                throw ex;
            }
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

        public void SetSyslogServer(String ServerAddress, int ServerPort)
        {
            this._ServerAddress = ServerAddress;
            this._ServerPort = ServerPort;
        }

        #endregion

        /// <summary>
        /// Send event to syslog server using UDP protocol
        /// </summary>
        /// <param name="evebntLogName">EventLog name</param>
        /// <param name="eventLogEntry">Event to transfert to syslog server</param>
        /// <param name="filter">Filter with Syslog facility and level</param>
        /// <param name="debug">Debug object</param>
        public void SendEvent(String eventLogName, EventLogEntry eventLogEntry, Filter filter)
        {
            String message = PrepareSyslogEvent(eventLogName, eventLogEntry);
            if (_Protocol.CompareTo("udp") == 0)
            {
                SendEventByUDP(message, eventLogName, eventLogEntry, filter);
            }
            else if (_Protocol.CompareTo("tcp") == 0)
            {
                SendEventByTCP(message, eventLogName, eventLogEntry, filter);
            }
        }

        /// <summary>
        /// Send event to syslog server
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="evebntLogName">EventLog name</param>
        /// <param name="eventLogEntry">Event to transfert to syslog server</param>
        /// <param name="filter">Filter with Syslog facility and level</param>
        /// <param name="debug">Debug object</param>
        /// <returns>True if any error appear</returns>
        private Boolean SendEventByUDP(String message, String eventLogName, EventLogEntry eventLogEntry, Filter filter)
        {
            //ASCIIEncoding ascii = new ASCIIEncoding();
            IPAddress[] ServersAddress;

            // Create syslog tag and remove syslog message accents
            Int32 pri = (int)Facility[filter.SyslogFacility.ToLower()] * 8 + (int)Level[filter.SyslogLevel.ToLower()];
            String body = "<" + pri + ">" + eventLogEntry.MachineName + " " + message;

            //string[] strParams = { "<" + ((int)Facility[filter.SyslogFacility] * 8 + (int)Level[filter.SyslogLevel]) + ">", eventLogName + " ", message};

            // Convert final message in bytes
            //byte[] rawMsg = Encoding.ASCII.GetBytes(string.Concat(strParams));
            byte[] rawMsg = Encoding.Default.GetBytes(body);

            try
            {
                ServersAddress = Dns.GetHostAddresses(this._ServerAddress);

                //UdpClient udp = new UdpClient(this._ServerAddress, this._ServerPort);
                String temp = ServersAddress.GetValue(0).ToString();

                for (int i = 0; i < ServersAddress.Length; i++)
                {
                    UdpClient udp = new UdpClient(ServersAddress.GetValue(i).ToString(), this._ServerPort);

                    udp.Send(rawMsg, rawMsg.Length);
                    this._Debug.Write("Syslog Server", "Event send to: " + ServersAddress.GetValue(i).ToString() + " with message: " + message, DateTime.Now);
                    udp.Close();
                    udp = null;
                }
            }
            catch (SocketException e)
            {
                this._Debug.Write("Syslog Server", "SocketException caught because: " + e.Message, DateTime.Now);
                return false;
            }
            catch (ArgumentNullException e)
            {
                this._Debug.Write("Syslog Server", "ArgumentNullException caught because: " + e.Message, DateTime.Now);
                return false;
            }
            catch (ArgumentOutOfRangeException e)
            {
                this._Debug.Write("Syslog Server", "ArgumentOutOfRangeException caught because: " + e.Message, DateTime.Now);
                return false;
            }
            catch (ObjectDisposedException e)
            {
                this._Debug.Write("Syslog Server", "ObjectDisposedException caught because: " + e.Message, DateTime.Now);
                return false;
            }
            catch (InvalidOperationException e)
            {
                this._Debug.Write("Syslog Server", "InvalidOperationException caught because: " + e.Message, DateTime.Now);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Send event to syslog server using TCP protocol
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="evebntLogName">EventLog name</param>
        /// <param name="eventLogEntry">Event to transfert to syslog server</param>
        /// <param name="filter">Filter with Syslog facility and level</param>
        /// <param name="debug">Debug object</param>
        /// <returns>True if any error appear</returns>
        private Boolean SendEventByTCP(String message, String eventLogName, EventLogEntry eventLogEntry, Filter filter)
        {
            IPAddress[] ServersAddress;

            // Create syslog tag and remove syslog message accents
            Int32 pri = (int)Facility[filter.SyslogFacility.ToLower()] * 8 + (int)Level[filter.SyslogLevel.ToLower()];

            string month = eventLogEntry.TimeWritten.ToString("MMM", DateTimeFormatInfo.InvariantInfo);
            string day = eventLogEntry.TimeWritten.ToString("dd", DateTimeFormatInfo.InvariantInfo);
            if (Convert.ToInt32(day) < 10)
            {
                day = " " + Convert.ToInt32(day);
            }
            string date = month + " " + day + " " + eventLogEntry.TimeWritten.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo);

            String body = "<" + pri + ">" + date + " " + eventLogEntry.MachineName + " " + message + "\n";

            // Convert final message in bytes
            byte[] rawMsg = Encoding.Default.GetBytes(body);

            try
            {
                ServersAddress = Dns.GetHostAddresses(this._ServerAddress);

                //String temp = ServersAddress.GetValue(0).ToString();

                for (int i = 0; i < ServersAddress.Length; i++)
                {
                    //Try to send message by TCP
                    TcpClient tcp;
                    NetworkStream flux;
                    try
                    {
                        tcp = new TcpClient(ServersAddress.GetValue(i).ToString(), this._ServerPort);
                        if (tcp.Connected)
                        {
                            flux = tcp.GetStream();
                            if (flux.CanWrite)
                            {
                                flux.Write(rawMsg, 0, rawMsg.Length);

                                this._Debug.Write("Syslog Server", "Event send to: " + ServersAddress.GetValue(i).ToString() + " with message: " + message, DateTime.Now);
                                flux.Close();
                                tcp.Close();
                                tcp = null;
                            }
                            else
                            {
                                SetMessageInBuffer(body);
                            }
                        }
                        else
                        {
                            SetMessageInBuffer(body);
                        }
                    }
                    catch (SocketException e)
                    {
                        //debug.Write("Syslog Server", "SocketException caught because: " + e.Message, DateTime.Now);
                        //return false;
                        SetMessageInBuffer(body);
                    }
                    catch (ArgumentNullException e)
                    {
                        this._Debug.Write("Syslog Server", "ArgumentNullException caught because: " + e.Message, DateTime.Now);
                        return false;
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        this._Debug.Write("Syslog Server", "ArgumentOutOfRangeException caught because: " + e.Message, DateTime.Now);
                        return false;
                    }
                    catch (ObjectDisposedException e)
                    {
                        this._Debug.Write("Syslog Server", "ObjectDisposedException caught because: " + e.Message, DateTime.Now);
                        return false;
                    }
                    catch (System.IO.IOException e)
                    {
                        //debug.Write("Syslog Server", "IOException caught because: " + e.Message, DateTime.Now);
                        //return false;
                        SetMessageInBuffer(body);
                    }
                    // If it's impossible add message into FIFO stack
                    // Start process to try to send later messages in FIFO stack
                    // If FIFO stack is full, write messages into buffer file
                    // Start process to try to send later messages in buffer file
                    // If buffer file is full, delete old messages


                }
            }
            catch (SocketException e)
            {
                this._Debug.Write("Syslog Server", "SocketException caught because: " + e.Message, DateTime.Now);
                return false;
            }
            catch (ArgumentNullException e)
            {
                this._Debug.Write("Syslog Server", "ArgumentNullException caught because: " + e.Message, DateTime.Now);
                return false;
            }
            catch (ArgumentOutOfRangeException e)
            {
                this._Debug.Write("Syslog Server", "ArgumentOutOfRangeException caught because: " + e.Message, DateTime.Now);
                return false;
            }
            catch (ObjectDisposedException e)
            {
                this._Debug.Write("Syslog Server", "ObjectDisposedException caught because: " + e.Message, DateTime.Now);
                return false;
            }
            catch (InvalidOperationException e)
            {
                this._Debug.Write("Syslog Server", "InvalidOperationException caught because: " + e.Message, DateTime.Now);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Add syslog facilities and levels into Hashtable
        /// </summary>
        private void SetSyslogLevelAndFacility()
        {
            Level.Add("emergency", 0);
            Level.Add("emerg", 0);
            Level.Add("panic", 0);
            Level.Add("alert", 1);
            Level.Add("critical", 2);
            Level.Add("crit", 2);
            Level.Add("error", 3);
            Level.Add("err", 3);
            Level.Add("warning", 4);
            Level.Add("warn", 4);
            Level.Add("notice", 5);
            Level.Add("informational", 6);
            Level.Add("info", 6);
            Level.Add("debug", 7);

            Facility.Add("kern", 0);
            Facility.Add("user", 1);
            Facility.Add("mail", 2);
            Facility.Add("daemon", 3);
            Facility.Add("auth", 4);
            Facility.Add("syslog", 5);
            Facility.Add("lpr", 6);
            Facility.Add("news", 7);
            Facility.Add("uucp", 8);
            Facility.Add("cron", 9);
            Facility.Add("authPriv", 10);
            Facility.Add("ftp", 11);
            Facility.Add("ntp", 12);
            Facility.Add("audit", 13);
            Facility.Add("audit2", 14);
            Facility.Add("cron2", 15);
            Facility.Add("local0", 16);
            Facility.Add("local1", 17);
            Facility.Add("local2", 18);
            Facility.Add("local3", 19);
            Facility.Add("local4", 20);
            Facility.Add("local5", 21);
            Facility.Add("local6", 22);
            Facility.Add("local7", 23);
        }

        /// <summary>
        /// Transform EventLogEntry to String
        /// </summary>
        /// <param name="evebntLogName">EventLog name</param>
        /// <param name="eventLogEntry">Event to transfert to syslog server</param>
        /// <param name="debug">Debug object</param>
        /// <returns>String of syslog event to transfert</returns>
        private String PrepareSyslogEvent(String evebntLogName, EventLogEntry eventLogEntry)
        {
            // Prepare message will sent to syslog server
            String body = eventLogEntry.Source.Replace(" ", "_") + " Type: " + eventLogEntry.EntryType.ToString();

            if (eventLogEntry.Category != null)
                body = body + ", Category: " + eventLogEntry.Category;

            if (eventLogEntry.EventID != 0)
                body = body + ", Event ID: " + eventLogEntry.EventID;

            if (eventLogEntry.UserName != null)
                body = body + ", User: " + eventLogEntry.UserName;

            body = body + ", Description: " + eventLogEntry.Message;

            body = body.Replace('\r', ' ');
            body = body.Replace('\n', ' ');

            return body;
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
                temp += " MB, MemoryBufferMaxSize: " + this._MemoryBufferMaxSize;
            }

            return temp;
        }

        #region TCP communication

        /// <summary>
        /// Thread to send syslog event to TCP syslog server
        /// </summary>
        private void SendThread()
        {
            do
            {
                // Send the object
                try
                {
                    String message = (String)SendQueue.Peek();
                    byte[] rawMsg = Encoding.Default.GetBytes(message);

                    IPAddress[] ServersAddress = Dns.GetHostAddresses(this._ServerAddress);

                    //String temp = ServersAddress.GetValue(0).ToString();

                    for (int i = 0; i < ServersAddress.Length; i++)
                    {
                        //Try to send message by TCP
                        TcpClient tcp;
                        NetworkStream flux;

                        tcp = new TcpClient(ServersAddress.GetValue(i).ToString(), this._ServerPort);
                        flux = tcp.GetStream();
                        flux.Write(rawMsg, 0, rawMsg.Length);
                        flux.Close();
                        tcp.Close();
                        tcp = null;
                        this._Debug.Write("Syslog Server", "Event send to: " + this._ServerAddress + " with message: " + message, DateTime.Now);
                        SendQueue.Dequeue();

                    }
                }
                catch
                {
                    this._Debug.Write("Syslog Server", "Erreur dans la queue de tranfere", DateTime.Now);
                }

                if (SendQueue.Count == 0) Thread.Sleep(100);

            } while (SendQueue.Count != 0);

            sending = false;
        }

        /// <summary>
        /// Add syslog event in Queue if can't join TCP syslog server
        /// </summary>
        /// <param name="message">syslog event to add in queue</param>
        private void Send(String message)
        {
            this._Debug.Write("Syslog Server", "Add message in queue", DateTime.Now);
            SendQueue.Enqueue(message);

            // Check to not start a second sending loop
            if (sending == true) return;
            sending = true;
            Sending_thread = new Thread(new ThreadStart(SendThread));
            Sending_thread.Start();
        }

        /// <summary>
        /// Set syslog event into memory buffer to try to sent it later
        /// </summary>
        /// <param name="message">syslog event</param>
        private void SetMessageInBuffer(String message)
        {
            if (this.SendQueue == null)
            {
                this.SendQueue = new Queue(this._MemoryBufferMaxSize);
            }

            if (this.SendQueue.Count < this._MemoryBufferMaxSize)
            {
                Send(message);
            }
            else
            {
                this._Debug.Write("Syslog Server", "Memory stack full", DateTime.Now);
            }
        }

        #endregion
    }
}
