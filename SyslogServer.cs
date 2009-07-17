//##################################################################################
//# Copyright 2005-2009 MERETHIS
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
//# Last modification by : Laurent Pinsivy
//# Last modification date : 2009.02.27
//#
//# Dependent plugin : .NET Framework
//# Dependent plugin version : 2.0.x
//#
//####################################################################################
using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Globalization; 

namespace centreon_eventLog_syslog
{
    class SyslogServer
    {
        private string _Server = null;
        private string _Protocol = null;
        private static ASCIIEncoding ascii = new ASCIIEncoding();
        private int _Port;
        public Hashtable Level = new Hashtable();
        public Hashtable Facility = new Hashtable();

        /// <summary>
        /// Constructor
        /// Filled Level and Facility array with syslog definition
        /// </summary>
        public SyslogServer()
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
        /// Get or set Syslog Server
        /// </summary>
        public string Server
        {
            get
            {
                return _Server;
            }
            set
            {
                _Server = value;
            }
        }

        /// <summary>
        /// Get or set Syslog Protocol
        /// </summary>
        public string Protocol
        {
            get
            {
                return _Protocol;
            }
            set
            {
                _Protocol = value;
            }
        }

        /// <summary>
        /// Get or set Syslog Port
        /// </summary>
        public int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = value;
            }
        }

        /// <summary>
        /// Determine if SyslogServer object is valid or not
        /// </summary>
        /// <returns>Return true if SyslogServer object is valid</returns>
        public Boolean isValid()
        {
            Boolean valid = true;
            if (_Server == null) valid = false;
            if (_Protocol == null) valid = false;
            if (_Port == 0) valid = false;

            return valid;
        }

        /// <summary>
        /// Delete all diacritics from string entry
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(string inputString)
        {
            String normalizedString = inputString.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Send syslog message to syslog server
        /// </summary>
        /// <param name="facility">syslog facility</param>
        /// <param name="level">syslog level</param>
        /// <param name="text">syslog message</param>
        /// <param name="debug">Debug objet instance</param>
        public void SendMessage(Rule rule, EventLogEntry eventLog, Debug debug)
        {
            // Prepare message will sent to syslog server
            string body = rule.EventLogName + " Source: " + eventLog.Source + " Type: " + eventLog.EntryType.ToString();

            if (eventLog.Category != null)
                body = body + " Category: " + eventLog.Category;

            if (eventLog.EventID != 0)
                body = body + " Event ID: " + eventLog.EventID;

            if (eventLog.UserName != null)
                body = body + " User: " + eventLog.UserName;

            if (eventLog.MachineName != null)
                body = body + " Computer: " + eventLog.MachineName;

            body = body + " DateTime: " + eventLog.TimeWritten.ToString() + " Description: " + eventLog.Message;

            try
            {
                _Server = Dns.Resolve(_Server).HostName;

                byte[] rawMsg;

                // Create syslog tag and remove syslog message accents
                string[] strParams = { "<" + ((int)Facility[rule.SyslogFacility] * 8 + (int)Level[rule.SyslogLevel]) + ">", 
                                             eventLog.Source + " ", RemoveDiacritics(body)};

                // Convert final message in bytes
                rawMsg = ascii.GetBytes(string.Concat(strParams));

                if (_Protocol.CompareTo("UDP") == 0)
                {
                    UdpClient udp = new UdpClient(_Server, _Port);

                    udp.Send(rawMsg, rawMsg.Length);

                    udp.Close();
                    udp = null;
                }
                else if (_Protocol.CompareTo("TCP") == 0)
                {
                    TcpClient tcp = new TcpClient(_Server, _Port);

                    NetworkStream flux = tcp.GetStream();

                    flux.Write(rawMsg, 0, rawMsg.Length);

                    flux.Close();
                    tcp.Close();
                    tcp = null;
                }
            }
            catch (System.Exception e)
            {
                String error = "[ERROR]\tUnable to send syslog message \"" + body + "\" to " + _Server + ":" + _Port + " (" + _Protocol + ")";
                if (debug.isValid)
                {
                    debug.log(error);
                    debug.log(e.Message);
                }

            }
        }
    }
}
