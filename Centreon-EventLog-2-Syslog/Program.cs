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
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace Centreon_EventLog_2_Syslog
{
    class Program
    {
        private static String ConfigurationFile = "Configuration.xml";
        private static DebugInformations debInf = new DebugInformations();
        private static Debug deb;
        private static Hashtable iFilters = null;
        private static Hashtable eFilters = null;
        private static Int32 refreshIntervalle = 0;
        private static Boolean _isActive = true;
        private static SyslogServer syslogServer = null;

        public static DateTime lastExecTime;
        public static DateTime maxExecTime;
        public static DateTime nextCheck;

        private static ManualResetEvent[] doneEvents = null;

        /// <summary>
        /// Simple constructor
        /// </summary>
        public Program()
        {
        }

        /// <summary>
        /// Main load XML configuration file
        /// call sub load XML configuration file functions
        /// </summary>
        static void LoadConfiguration()
        {
            ConfigurationFile = "Configuration.xml";
            String exepath = Environment.GetCommandLineArgs()[0];
            String exedir = exepath.Substring(0, exepath.LastIndexOf('\\'));
            ConfigurationFile = exedir + "\\" + ConfigurationFile;

            XmlDocument xDoc = new XmlDocument();

            debInf.Level = 1;
            debInf.Versobe = 1;
            deb = new Debug("Debug.log", ref debInf);
            deb.Write("Main Program", "Load configuration", DateTime.Now);

            try
            {
                xDoc.Load(@ConfigurationFile);

                XmlNode rootNode = xDoc.ChildNodes[1];

                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    if (node.Name.CompareTo("program") == 0)
                    {
                        LoadConfigurationProgram(node);
                    }
                    else if (node.Name.CompareTo("syslog_server") == 0)
                    {
                        LoadSyslogConfiguration(node);

                        if (debInf.Versobe == 2)
                        {
                            deb.Write("Load Syslog Server Configuration", "Syslog server configuration: " + syslogServer.ToString(), DateTime.Now);
                        }
                    }
                    else if (node.Name.CompareTo("filters") == 0)
                    {
                        LoadFilters(node);

                        if (debInf.Versobe == 2)
                        {
                            ArrayList list = new ArrayList(iFilters.Keys);
                            String[] eventLogNames = (String[])list.ToArray(typeof(string));

                            foreach (String eventLogName in eventLogNames)
                            {
                                ArrayList itemp = (ArrayList)iFilters[eventLogName];
                                ArrayList etemp = (ArrayList)eFilters[eventLogName];

                                int iItems = 0;
                                int eItems = 0;

                                if (itemp != null)
                                {
                                    iItems = itemp.Count;
                                }
                                if (etemp != null)
                                {
                                    eItems = etemp.Count;
                                }

                                deb.Write("Load configuration", iItems + " include filter(s) and " + eItems + " exclude filter(s) loaded for eventLog : " + eventLogName, DateTime.Now);
                                itemp = null;
                                etemp = null;
                            }
                        }
                    }
                }
            }
            catch (XmlException xe)
            {
                deb.Write("Load configuration", "101 - Load XML configuration - " + xe.Message, DateTime.Now);
                deb.Write("Load configuration", "Program stop", DateTime.Now);
                System.Environment.Exit(-1);
            }
            catch (NotSupportedException nse)
            {
                deb.Write("Load configuration", "102 - Load XML configuration - " + nse.Message, DateTime.Now);
                deb.Write("Load configuration", "Program stop", DateTime.Now);
                System.Environment.Exit(-1);
            }
            catch (Exception e)
            {
                deb.Write("Load configuration", "103 - Load XML configuration - " + e.Message, DateTime.Now);
                deb.Write("Load configuration", "Program stop", DateTime.Now);
                System.Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Load program configuration from specific XML node
        /// </summary>
        /// <param name="node">specific XML including program configuration</param>
        static void LoadConfigurationProgram(XmlNode node)
        {
            foreach (XmlNode childnode in node.ChildNodes)
            {
                if (childnode.Name.CompareTo("debug") == 0)
                {
                    foreach (XmlNode paramNode in childnode.ChildNodes)
                    {
                        if (paramNode.Name.CompareTo("level") == 0)
                        {
                            try
                            {
                                debInf.Level = Convert.ToInt32(paramNode.InnerText);
                                if (debInf.Versobe == 2)
                                {
                                    deb.Write("Load program configuration", "Set level " + debInf.Level, DateTime.Now);
                                }
                            }
                            catch (FormatException fe)
                            {
                                debInf.Level = 1;
                                DateTime temp = DateTime.Now;
                                deb.Write("Load program configuration", "201 - Get level value - " + fe.Message, temp);
                                deb.Write("Load program configuration", "201 - Debug level value is set to \"1\"", temp);
                            }
                        }
                        else if (paramNode.Name.CompareTo("verbose") == 0)
                        {
                            try
                            {
                                debInf.Versobe = Convert.ToInt32(paramNode.InnerText);
                                if (debInf.Versobe == 2)
                                {
                                    deb.Write("Load program configuration", "Set Verbose " + debInf.Level, DateTime.Now);
                                }
                            }
                            catch (FormatException fe)
                            {
                                deb.Write("Load program configuration", "202 - Get verbose value - " + fe.Message, DateTime.Now);
                            }
                        }
                        else if (paramNode.Name.CompareTo("max_size") == 0)
                        {
                            try
                            {
                                debInf.MaxSize = Convert.ToInt32(paramNode.InnerText);
                                if (debInf.Versobe == 2)
                                {
                                    deb.Write("Load program configuration", "Set Debug file max size " + debInf.MaxSize + " MB", DateTime.Now);
                                }
                            }
                            catch (FormatException fe)
                            {
                                deb.Write("Load program configuration", "203 - Get max size value - " + fe.Message, DateTime.Now);
                            }
                        }
                        else if (paramNode.Name.CompareTo("file_number") == 0)
                        {
                            try
                            {
                                debInf.FileNumber = Convert.ToInt32(paramNode.InnerText);
                                if (debInf.Versobe == 2)
                                {
                                    deb.Write("Load program configuration", "Set Debug max number files " + debInf.FileNumber, DateTime.Now);
                                }
                            }
                            catch (FormatException fe)
                            {
                                deb.Write("Load program configuration", "204 - Get file number value - " + fe.Message, DateTime.Now);
                            }
                        }
                    }
                }
                else if (childnode.Name.CompareTo("refresh_intervalle") == 0)
                {
                    try
                    {
                        refreshIntervalle = Convert.ToInt32(childnode.InnerText);
                        if (debInf.Versobe == 2)
                        {
                            deb.Write("Load program configuration", "Set Refresh intervalle " + refreshIntervalle + " minutes", DateTime.Now);
                        }
                    }
                    catch (FormatException fe)
                    {
                        deb.Write("Load program configuration", "205 - Getrefresh intervalle value - " + fe.Message, DateTime.Now);
                    }
                }
            }
        }

        /// <summary>
        /// Load filters to find in event log
        /// </summary>
        /// <param name="node">specific XML including filter parameters</param>
        static void LoadFilters(XmlNode node)
        {
            String patternSyslogLevel = "Emergency|Alert|Critical|Error|Warning|Notice|Informational|Debug";
            Regex rSyslogLevel = new Regex(patternSyslogLevel, RegexOptions.IgnoreCase);

            String patternSyslogFacility = "Kern|User|Mail|Daemon|Auth|Syslog|LPR|News|UUCP|Cron|AuthPriv|FTP|NTP|Audit|Audit2|CRON2|Local0|Local1|Local2|Local3|Local4|Local5|Local6|Local7";
            Regex rSyslogFacility = new Regex(patternSyslogFacility, RegexOptions.IgnoreCase);

            String[] eventLogName = null;
            Filter iFilter = null;
            Filter eFilter = null;

            foreach (XmlNode childnode in node.ChildNodes)
            {
                eventLogName = null;
                iFilter = new Filter();
                eFilter = new Filter();

                foreach (XmlNode cnode in childnode.ChildNodes)
                {
                    if (cnode.Name.ToLower().CompareTo("event") == 0)
                    {
                        foreach (XmlNode paramNode in cnode.ChildNodes)
                        {
                            if (paramNode.Name.ToLower().CompareTo("eventlogname") == 0)
                            {
                                ArrayList temp = new ArrayList();
                                foreach (XmlNode element in paramNode.ChildNodes)
                                {
                                    if (element.Name.IndexOf("#comment") < 0)
                                    {
                                        temp.Add(element.InnerText);
                                    }
                                }
                                eventLogName = new String[temp.Count];
                                int i = 0;
                                foreach (String item in temp)
                                {
                                    eventLogName.SetValue(item, i);
                                    i++;
                                }
                            }
                            else if (paramNode.Name.ToLower().CompareTo("sources") == 0)
                            {
                                ArrayList itemp = new ArrayList();
                                ArrayList etemp = new ArrayList();
                                foreach (XmlNode element in paramNode.ChildNodes)
                                {
                                    if (element.Name.IndexOf("include") >= 0)
                                    {
                                        itemp.Add(element.InnerText);
                                    }
                                    else if (element.Name.IndexOf("exclude") >= 0)
                                    {
                                        etemp.Add(element.InnerText);
                                    }
                                }

                                if (itemp.Count > 0)
                                {
                                    String[] strTemp = new String[itemp.Count];
                                    int i = 0;
                                    foreach (String item in itemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    iFilter.EventLogSources = strTemp;
                                }

                                if (etemp.Count > 0)
                                {
                                    String[] strTemp = new String[etemp.Count];
                                    int i = 0;
                                    foreach (String item in etemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    eFilter.EventLogSources = strTemp;
                                }


                            }
                            else if (paramNode.Name.ToLower().CompareTo("id") == 0)
                            {
                                ArrayList itemp = new ArrayList();
                                ArrayList etemp = new ArrayList();
                                foreach (XmlNode element in paramNode.ChildNodes)
                                {
                                    if (element.Name.IndexOf("include") >= 0)
                                    {
                                        itemp.Add(element.InnerText);
                                    }
                                    else if (element.Name.IndexOf("exclude") >= 0)
                                    {
                                        etemp.Add(element.InnerText);
                                    }
                                }

                                if (itemp.Count > 0)
                                {
                                    String[] strTemp = new String[itemp.Count];
                                    int i = 0;
                                    foreach (String item in itemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    iFilter.EventLogID = strTemp;
                                }

                                if (etemp.Count > 0)
                                {
                                    String[] strTemp = new String[etemp.Count];
                                    int i = 0;
                                    foreach (String item in etemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    eFilter.EventLogID = strTemp;
                                }
                            }
                            else if (paramNode.Name.ToLower().CompareTo("users") == 0)
                            {
                                ArrayList itemp = new ArrayList();
                                ArrayList etemp = new ArrayList();
                                foreach (XmlNode element in paramNode.ChildNodes)
                                {
                                    if (element.Name.IndexOf("include") >= 0)
                                    {
                                        itemp.Add(element.InnerText);
                                    }
                                    else if (element.Name.IndexOf("exclude") >= 0)
                                    {
                                        etemp.Add(element.InnerText);
                                    }
                                }

                                if (itemp.Count > 0)
                                {
                                    String[] strTemp = new String[itemp.Count];
                                    int i = 0;
                                    foreach (String item in itemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    iFilter.User = strTemp;
                                }

                                if (etemp.Count > 0)
                                {
                                    String[] strTemp = new String[etemp.Count];
                                    int i = 0;
                                    foreach (String item in etemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    eFilter.User = strTemp;
                                }
                            }
                            else if (paramNode.Name.ToLower().CompareTo("computers") == 0)
                            {
                                ArrayList itemp = new ArrayList();
                                ArrayList etemp = new ArrayList();
                                foreach (XmlNode element in paramNode.ChildNodes)
                                {
                                    if (element.Name.IndexOf("include") >= 0)
                                    {
                                        itemp.Add(element.InnerText);
                                    }
                                    else if (element.Name.IndexOf("exclude") >= 0)
                                    {
                                        etemp.Add(element.InnerText);
                                    }
                                }

                                if (itemp.Count > 0)
                                {
                                    String[] strTemp = new String[itemp.Count];
                                    int i = 0;
                                    foreach (String item in itemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    iFilter.Computer = strTemp;
                                }

                                if (etemp.Count > 0)
                                {
                                    String[] strTemp = new String[etemp.Count];
                                    int i = 0;
                                    foreach (String item in etemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    eFilter.Computer = strTemp;
                                }
                            }
                            else if (paramNode.Name.ToLower().CompareTo("type") == 0)
                            {
                                ArrayList itemp = new ArrayList();
                                ArrayList etemp = new ArrayList();
                                foreach (XmlNode element in paramNode.ChildNodes)
                                {
                                    if (element.Name.IndexOf("include") >= 0)
                                    {
                                        itemp.Add(element.InnerText);
                                    }
                                    else if (element.Name.IndexOf("exclude") >= 0)
                                    {
                                        etemp.Add(element.InnerText);
                                    }
                                }

                                if (itemp.Count > 0)
                                {
                                    String[] strTemp = new String[itemp.Count];
                                    int i = 0;
                                    foreach (String item in itemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    iFilter.EventLogType = strTemp;
                                }

                                if (etemp.Count > 0)
                                {
                                    String[] strTemp = new String[etemp.Count];
                                    int i = 0;
                                    foreach (String item in etemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    eFilter.EventLogType = strTemp;
                                }
                            }
                            else if (paramNode.Name.ToLower().CompareTo("descriptions") == 0)
                            {
                                ArrayList itemp = new ArrayList();
                                ArrayList etemp = new ArrayList();
                                foreach (XmlNode element in paramNode.ChildNodes)
                                {
                                    if (element.Name.IndexOf("include") >= 0)
                                    {
                                        itemp.Add(element.InnerText);
                                    }
                                    else if (element.Name.IndexOf("exclude") >= 0)
                                    {
                                        etemp.Add(element.InnerText);
                                    }
                                }

                                if (itemp.Count > 0)
                                {
                                    String[] strTemp = new String[itemp.Count];
                                    int i = 0;
                                    foreach (String item in itemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    iFilter.EventLogDescriptions = strTemp;
                                }

                                if (etemp.Count > 0)
                                {
                                    String[] strTemp = new String[etemp.Count];
                                    int i = 0;
                                    foreach (String item in etemp)
                                    {
                                        strTemp.SetValue(item, i);
                                        i++;
                                    }
                                    eFilter.EventLogDescriptions = strTemp;
                                }
                            }
                        }
                    }
                    else if (cnode.Name.ToLower().CompareTo("syslog") == 0)
                    {
                        foreach (XmlNode paramNode in cnode.ChildNodes)
                        {
                            if (paramNode.Name.ToLower().CompareTo("level") == 0)
                            {
                                if (rSyslogLevel.IsMatch(paramNode.InnerText))
                                {
                                    iFilter.SyslogLevel = paramNode.InnerText;
                                    eFilter.SyslogLevel = paramNode.InnerText;
                                }
                                else
                                {
                                    deb.Write("Load filters configuration", "301 - Uncorrect syslog level : \"" + paramNode.InnerText + "\"", DateTime.Now);
                                }
                            }
                            else if (paramNode.Name.ToLower().CompareTo("facility") == 0)
                            {
                                if (rSyslogFacility.IsMatch(paramNode.InnerText))
                                {
                                    iFilter.SyslogFacility = paramNode.InnerText;
                                    eFilter.SyslogFacility = paramNode.InnerText;
                                }
                                else
                                {
                                    deb.Write("Load filters configuration", "301 - Uncorrect syslog facility : \"" + paramNode.InnerText + "\"", DateTime.Now);
                                }
                            }
                        }
                    }
                }

                if (eventLogName != null)
                {
                    foreach (String element in eventLogName)
                    {
                        ArrayList itemp = null;
                        itemp = (ArrayList)iFilters[element];
                        ArrayList etemp = null;
                        etemp = (ArrayList)eFilters[element];

                        if ((itemp != null) && !iFilter.IsEmpty())
                        {
                            itemp.Add(iFilter);
                            if (debInf.Versobe == 2)
                            {
                                deb.Write("Load filters configuration", "Add to filter list for event log " + element + " evement " + iFilter.ToString(), DateTime.Now);
                            }
                            iFilters[element] = itemp;
                        }
                        else if ((itemp == null) && !iFilter.IsEmpty())
                        {
                            itemp = new ArrayList();
                            itemp.Add(iFilter);
                            if (debInf.Versobe == 2)
                            {
                                deb.Write("Load filters configuration", "Add to filter list for event log " + element + " evement " + iFilter.ToString(), DateTime.Now);
                            }
                            iFilters[element] = itemp;
                        }

                        if ((etemp != null) && !eFilter.IsEmpty())
                        {
                            etemp.Add(eFilter);
                            if (debInf.Versobe == 2)
                            {
                                deb.Write("Load filters configuration", "Add to exclude filter list for event log " + element + " evement " + iFilter.ToString(), DateTime.Now);
                            }
                            eFilters[element] = etemp;
                        }
                        else if ((etemp == null) && !eFilter.IsEmpty())
                        {
                            etemp = new ArrayList();
                            etemp.Add(eFilter);
                            if (debInf.Versobe == 2)
                            {
                                deb.Write("Load filters configuration", "Add to exclude filter list for event log " + element + " evement " + iFilter.ToString(), DateTime.Now);
                            }
                            eFilters[element] = etemp;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load Syslog server parameters
        /// </summary>
        /// <param name="node">specific XML including Syslog server parameters</param>
        static void LoadSyslogConfiguration(XmlNode node)
        {
            // TODO : Load Syslog server parameters
            foreach (XmlNode childnode in node.ChildNodes)
            {
                if (childnode.Name.CompareTo("server") == 0)
                {
                    String address = null;
                    String protocol = null;
                    int port = 514;
                    int memory_buffer = 200;


                    foreach (XmlNode paramNode in childnode.ChildNodes)
                    {
                        if (paramNode.Name.CompareTo("address") == 0)
                        {
                            String validIpAddressRegex = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]):[0-9]{2,5}$";
                            String validHostnameRegex = "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\\-]*[A-Za-z0-9]):[0-9]{2,5}$";
                            String validIpAddressRegexWithoutPort = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
                            String validHostnameRegexWithoutPort = "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\\-]*[A-Za-z0-9])$";

                            Regex re = new Regex(validIpAddressRegex);
                            Regex re2 = new Regex(validHostnameRegex);
                            Regex re3 = new Regex(validIpAddressRegexWithoutPort);
                            Regex re4 = new Regex(validHostnameRegexWithoutPort);

                            if (re.IsMatch(paramNode.InnerText, 0) || re2.IsMatch(paramNode.InnerText, 0))
                            {
                                String[] temp = paramNode.InnerText.ToString().Split(':');
                                address = temp[0];

                                try
                                {
                                    port = Convert.ToInt32(temp[1]);
                                    deb.Write("Load syslog configuration", "Set port " + port, DateTime.Now);
                                }
                                catch (SystemException se)
                                {
                                    deb.Write("Load Syslog Server Configuration", "Set port = 514 because: " + se.Message, DateTime.Now);
                                    port = 514;
                                }
                            }
                            else if (re3.IsMatch(paramNode.InnerText, 0) || re4.IsMatch(paramNode.InnerText, 0))
                            {
                                address = paramNode.InnerText;
                                deb.Write("Load syslog configuration", "Set address " + address, DateTime.Now);
                            }
                        }
                        else if (paramNode.Name.CompareTo("port") == 0)
                        {
                            try
                            {
                                port = Convert.ToInt32(paramNode.InnerText);
                                deb.Write("Load syslog configuration", "Set port " + port, DateTime.Now);
                            }
                            catch (SystemException se)
                            {
                                deb.Write("Load Syslog Server Configuration", "Set port = 514 because: " + se.Message, DateTime.Now);
                                port = 514;
                            }
                        }
                        else if (paramNode.Name.CompareTo("protocole") == 0)
                        {
                            protocol = paramNode.InnerText;
                            deb.Write("Load syslog configuration", "Set protocole " + protocol, DateTime.Now);
                        }
                        else if (paramNode.Name.CompareTo("memory_buffer") == 0)
                        {
                            try
                            {
                                memory_buffer = Convert.ToInt32(paramNode.InnerText);
                                deb.Write("Load syslog configuration", "Set memory buffer " + memory_buffer, DateTime.Now);
                            }
                            catch (SystemException se)
                            {
                                deb.Write("Load Syslog Server Configuration", "Set memory_buffer = 200 because: " + se.Message, DateTime.Now);
                                memory_buffer = 200;
                            }
                        }
                    }

                    try
                    {
                        if (syslogServer == null)
                        {
                            syslogServer = new SyslogServer(address, protocol, port, memory_buffer, ref deb);
                        }
                        else
                        {
                            syslogServer.SetSyslogServer(address, port);
                        }
                    }
                    catch (Exception e)
                    {
                        deb.Write("Load Syslog Server Configuration", "Configuration of syslog server is not correct: " + e.Message, DateTime.Now);
                        deb.Write("Load Syslog Server Configuration", "Program abort", DateTime.Now);
                        System.Environment.Exit(-1);
                    }
                }
            }
        }

        /// <summary>
        /// Start process:
        /// 1 - Control if process already exist and quit if exist
        /// 2 - Loop while process not receive order to stop it
        /// 3 - Load configuration
        /// 4 - Launch thread to parse event log
        /// 5 - Wait x seconds berore goto 3
        /// </summary>
        public void Start()
        {
            if (ProcessAlreadyExist() == true)
            {
                System.Environment.Exit(-1);
            }

            lastExecTime = DateTime.Now.AddMinutes(-120);
            maxExecTime = DateTime.Now;

            do
            {
                iFilters = new Hashtable();
                eFilters = new Hashtable();

                LoadConfiguration();
                StartThread();
                WaitHandle.WaitAll(doneEvents);

                iFilters = null;
                eFilters = null;

                nextCheck = maxExecTime.AddMinutes(refreshIntervalle);

                DateTime dtNow = DateTime.Now;
                int now = dtNow.Hour * 3600 + dtNow.Minute * 60 + dtNow.Second;
                int sleep = ((nextCheck.Hour * 3600 + nextCheck.Minute * 60 + nextCheck.Second) - now);

                if (debInf.Versobe == 2)
                {
                    deb.Write("Main program", "Sleep for " + sleep + " seconds", DateTime.Now);
                }

                do
                {
                    Thread.Sleep(1000);
                    if (isActive == false)
                        break;
                    sleep--;
                }
                while (sleep > 0);

                lastExecTime = maxExecTime;
                maxExecTime = DateTime.Now;

            }
            while (isActive);

            deb.Write("Main program", "Program stop", DateTime.Now);
            System.Environment.Exit(0);
        }

        /// <summary>
        /// Start one thread ThreadFilter by event log name
        /// </summary>
        private void StartThread()
        {
            deb.Write("Preparation of Threads", "Start threads", DateTime.Now);

            ArrayList list = new ArrayList(iFilters.Keys);
            String[] eventLogNames = (String[])list.ToArray(typeof(string));

            doneEvents = new ManualResetEvent[eventLogNames.Length];

            try
            {
                int i = 0;
                foreach (String eventLog in eventLogNames)
                {
                    try
                    {
                        ThreadFilter[] filterArray = new ThreadFilter[eventLogNames.Length];

                        doneEvents[i] = new ManualResetEvent(false);
                        ThreadFilter tf = new ThreadFilter(eventLog, ref syslogServer, (ArrayList)iFilters[eventLog], (ArrayList)eFilters[eventLog], ref deb, lastExecTime, maxExecTime, doneEvents[i]);
                        filterArray[i] = tf;

                        ThreadPool.QueueUserWorkItem(tf.ThreadLoop, i);
                    }
                    catch (System.NullReferenceException e)
                    {
                        deb.Write("Preparation of Threads", "401 - Problem starting thread for " + eventLog + "package rules due to: " + e.Message, DateTime.Now);
                    }
                    i++;
                }
            }
            catch (System.NullReferenceException e)
            {
                deb.Write("Preparation of Threads", "402 - Problem starting threads due to: " + e.Message, DateTime.Now);
                System.Environment.Exit(-1);
            }

            deb.Write("Preparation of Threads", "End of starting threads", DateTime.Now);
        }

        /// <summary>
        /// Test if exist process exist
        /// </summary>
        /// <returns>True if process exist</returns>
        private bool ProcessAlreadyExist()
        {
            Process currentProcess = Process.GetCurrentProcess();

            foreach (Process p in Process.GetProcesses())
            {
                if (p.Id != currentProcess.Id && p.ProcessName.Equals(currentProcess.ProcessName) == true)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get or set isActive value
        /// If value is false, principal process run by Start method finish
        /// </summary>
        public Boolean isActive
        {
            get
            {
                return _isActive;
            }

            set
            {
                _isActive = value;
            }
        }
    }
}