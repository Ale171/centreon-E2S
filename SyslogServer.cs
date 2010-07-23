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

        private Hashtable Level = new Hashtable();
        private Hashtable Facility = new Hashtable();

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
        /// <param name="fileBufferMaxSizeInMB">File buffer max size in MB. Only used for TCP protocol.</param>
        public SyslogServer(String serverAddress, String protocol, int port, int memoryBufferMaxSize, int fileBufferMaxSizeInMB)
        {
            SetServerAddress(serverAddress);
            SetPort(port);
            SetProtocol(protocol.ToLower());
            this._MemoryBufferMaxSize = memoryBufferMaxSize;
            this._FileBufferMaxSizeInMB = fileBufferMaxSizeInMB;
            SetSyslogLevelAndFacility();
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

        /// <summary>
        /// Send event to syslog server
        /// </summary>
        /// <param name="evebntLogName">EventLog name</param>
        /// <param name="eventLogEntry">Event to transfert to syslog server</param>
        /// <param name="debug">Debug object</param>
        public void SendEvent(String evebntLogName, EventLogEntry eventLogEntry, ref Debug debug)
        {
            String MESSAGE = PrepareSyslogEvent(evebntLogName, eventLogEntry, ref debug);
            debug.Write("Syslog Server", "Event to send: " + MESSAGE, DateTime.Now);
        }

        /// <summary>
        /// Add syslog facilities and levels into Hashtable
        /// </summary>
        private void SetSyslogLevelAndFacility()
        {
            Level.Add("Emergency", 0);
            Level.Add("Alert", 1);
            Level.Add("Critical", 2);
            Level.Add("Error", 3);
            Level.Add("Warning", 4);
            Level.Add("Notice", 5);
            Level.Add("Informational", 6);
            Level.Add("Debug", 7);

            Facility.Add("Kern", 0);
            Facility.Add("User", 1);
            Facility.Add("Mail", 2);
            Facility.Add("Daemon", 3);
            Facility.Add("Auth", 4);
            Facility.Add("Syslog", 5);
            Facility.Add("LPR", 6);
            Facility.Add("News", 7);
            Facility.Add("UUCP", 8);
            Facility.Add("Cron", 9);
            Facility.Add("AuthPriv", 10);
            Facility.Add("FTP", 11);
            Facility.Add("NTP", 12);
            Facility.Add("Audit", 13);
            Facility.Add("Audit2", 14);
            Facility.Add("CRON2", 15);
            Facility.Add("Local0", 16);
            Facility.Add("Local1", 17);
            Facility.Add("Local2", 18);
            Facility.Add("Local3", 19);
            Facility.Add("Local4", 20);
            Facility.Add("Local5", 21);
            Facility.Add("Local6", 22);
            Facility.Add("Local7", 23);
        }

        /// <summary>
        /// Transform EventLogEntry to String
        /// </summary>
        /// <param name="evebntLogName">EventLog name</param>
        /// <param name="eventLogEntry">Event to transfert to syslog server</param>
        /// <param name="debug">Debug object</param>
        /// <returns>String of syslog event to transfert</returns>
        private String PrepareSyslogEvent(String evebntLogName, EventLogEntry eventLogEntry, ref Debug debug)
        {
            // Prepare message will sent to syslog server
            String body = evebntLogName + " Source: " + eventLogEntry.Source + " Type: " + eventLogEntry.EntryType.ToString();

            if (eventLogEntry.Category != null)
                body = body + " Category: " + eventLogEntry.Category;

            if (eventLogEntry.EventID != 0)
                body = body + " Event ID: " + eventLogEntry.EventID;

            if (eventLogEntry.UserName != null)
                body = body + " User: " + eventLogEntry.UserName;

            if (eventLogEntry.MachineName != null)
                body = body + " Computer: " + eventLogEntry.MachineName;

            body = body + " DateTime: " + eventLogEntry.TimeWritten.ToString() + " Description: " + eventLogEntry.Message;

            return null;
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
