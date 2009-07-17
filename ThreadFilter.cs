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
//# Last modification date : 2009.02.24
//#
//# Dependent plugin : .NET Framework
//# Dependent plugin version : 2.0.x
//#
//####################################################################################
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace centreon_eventLog_syslog
{
    class ThreadFilter
    {
        private String _LogName = null;
        private SyslogServer _Syslog = null;
        private Debug _Debug = null;
        private DateTime _LastExecTime;
        private DateTime _MaxExecTime;
        private ArrayList _RulesList = null;
        private ManualResetEvent _DoneEvent;
        private EventLogEntry eventLogEntry = null;

        /// <summary>
        /// Complex constructor for filter thread
        /// </summary>
        /// <param name="logName">EventLog name</param>
        /// <param name="syslog">Syslog server, protocol and port definition</param>
        /// <param name="rulesList">Rules for find eventLog</param>
        /// <param name="debug">Permit to write debug line</param>
        /// <param name="lastExecTime">Min time research</param>
        /// <param name="maxExecTime">Mas time research</param>
        /// <param name="doneEvent">ManualResetEvent object to know if process if complit</param>
        public ThreadFilter(String logName, SyslogServer syslog, ArrayList rulesList, Debug debug,
            DateTime lastExecTime, DateTime maxExecTime, ManualResetEvent doneEvent)
        {
            _LogName = logName;
            _Syslog = syslog;
            _Debug = debug;
            _LastExecTime = lastExecTime;
            _MaxExecTime = maxExecTime;
            _RulesList = rulesList;
            _DoneEvent = doneEvent;
        }

        /// <summary>
        /// Scan Windows eventLog for find event like rules and send it to a syslog server
        /// </summary>
        /// <param name="threadContext">ManualResetEvent object</param>
        public void ThreadLoop(Object threadContext)
        {
            EventLog eventLog = null;
            EventLogEntryCollection eventLogEntryCollection = null;

            try
            {
                // Get EventLog entries
                eventLog = new EventLog();
                eventLog.Log = _LogName;
                eventLog.MachineName = ".";
                eventLogEntryCollection = eventLog.Entries;
            }
            catch (System.Exception e)
            {
                if (_Debug.isValid)
                {
                    Boolean ok = false;

                    do
                    {
                        if (_Debug.isActive == false)
                        {
                            _Debug.log("[FILTER " + _LogName + "] \tUnable to load eventLog: \"" + _LogName + "\" entries");
                            ok = true;
                        }
                        else
                            Thread.Sleep(500);
                    }
                    while(ok == false);
                }
            }

            try
            {
                int NbLogEntries = eventLogEntryCollection.Count;

                if (_Debug.isValid)
                {
                    Boolean ok = false;
                    do
                    {
                        if (_Debug.isActive == false)
                        {
                            if (_Debug.isValid) _Debug.log("[FILTER " + _LogName + "] \tStart at " + DateTime.Now);
                            ok = true;
                        }
                        else
                            Thread.Sleep(500);
                    }
                    while (ok == false);
                }

                for (int i=NbLogEntries-1; i> 0; i--)
                {
                    eventLogEntry = eventLogEntryCollection[i];
                    
                    long elapsedTimeMin = eventLogEntry.TimeWritten.Ticks - _LastExecTime.Ticks;
                    long elapsedTimeMax = eventLogEntry.TimeWritten.Ticks - _MaxExecTime.Ticks;

                    // If event are too old (more than last check) break loop
                    if (elapsedTimeMin < 0) break;

                    // While event is contained between last check and program time execution
                    if ((elapsedTimeMin >= 0) && (elapsedTimeMax < 0))
                    {
                        foreach (Rule rule in _RulesList)
                        {
                            Boolean bEventLogsources = false;
                            Boolean bEventLogID = false;
                            Boolean bUser = false;
                            Boolean bComputer = false;
                            Boolean bEventLogType = false;
                            Boolean bEventLogDescriptions = false;

                            if (rule.EventLogSources == null)
                            {
                                bEventLogsources = true;
                            }
                            else
                            {
                                foreach (string source in rule.EventLogSources)
                                {
                                    if ((source.CompareTo("*") == 0) || (source.CompareTo(eventLogEntry.Source) == 0))
                                    {
                                        bEventLogsources = true;
                                        break;
                                    }
                                }
                            }

                            if (rule.EventLogID == null)
                            {
                                bEventLogID = true;
                            }
                            else
                            {
                                foreach (string id in rule.EventLogID)
                                {
                                    if ((id.CompareTo("*") == 0) || (id.CompareTo(eventLogEntry.EventID.ToString()) == 0))
                                    {
                                        bEventLogID = true;
                                        break;
                                    }
                                }
                            }

                            if (rule.User == null)
                            {
                                bUser = true;
                            }
                            else
                            {
                                foreach (string user in rule.User)
                                {
                                    if ((user.CompareTo("*") == 0) || (user.CompareTo(eventLogEntry.UserName) == 0))
                                    {
                                        bUser = true;
                                        break;
                                    }
                                }
                            }

                            if (rule.Computer == null)
                            {
                                bComputer = true;
                            }
                            else
                            {
                                foreach (string computer in rule.Computer)
                                {
                                    if ((computer.CompareTo("*") == 0) || (computer.CompareTo(eventLogEntry.MachineName) == 0))
                                    {
                                        bComputer = true;
                                        break;
                                    }
                                }
                            }

                            if (rule.EventLogType == null)
                            {
                                bEventLogType = true;
                            }
                            else
                            {
                                foreach (string eventType in rule.EventLogType)
                                {
                                    if ((eventType.CompareTo("*") == 0) || (eventType.CompareTo(eventLogEntry.EntryType.ToString()) == 0))
                                    {
                                        bEventLogType = true;
                                        break;
                                    }
                                }
                            }

                            if (rule.EventLogDescriptions == null)
                            {
                                bEventLogDescriptions = true;
                            }
                            else
                            {
                                foreach (string description in rule.EventLogDescriptions)
                                {
                                    if ((description.CompareTo("*") == 0) || (description.CompareTo(eventLogEntry.Message) == 0))
                                    {
                                        bEventLogDescriptions = true;
                                        break;
                                    }
                                }
                            }

                            // If event pass all conditions, send it to syslog server.
                            if (bEventLogsources && bEventLogID && bUser && bComputer && bEventLogType && bEventLogDescriptions)
                            {
                                _Syslog.SendMessage(rule, eventLogEntry, _Debug);
                                if (_Debug.isValid)
                                {
                                    if (_Debug.isValid)
                                    {
                                        Boolean ok = false;

                                        do
                                        {
                                            if (_Debug.isActive == false)
                                            {
                                                _Debug.log("[FILTER " + _LogName + "]\tThe following message have passed filters and will sent to syslog server");
                                                _Debug.log("[FILTER " + _LogName + "]\tID: " + eventLogEntry.EventID.ToString()
                                                    + "\n\t\tType: " + eventLogEntry.EntryType.ToString()
                                                    + "\n\t\tMessage:" + eventLogEntry.Message.ToString() + "\n");
                                                ok = true;
                                            }
                                            else
                                                Thread.Sleep(500);
                                        }
                                        while(ok == false);
                                    }
                                    
                                }
                            }
                        }
                    }
                }
                // Desctruct all
                eventLog = null;
                eventLogEntry = null;
                eventLogEntryCollection = null;

                _DoneEvent.Set();

                if (_Debug.isValid)
                {
                    Boolean ok = false;
                    do
                    {
                        if (_Debug.isActive == false)
                        {
                            if (_Debug.isValid) _Debug.log("[FILTER " + _LogName + "] \tFinish at " + DateTime.Now);
                            ok = true;
                        }
                        else
                            Thread.Sleep(500);
                    }
                    while (ok == false);
                }
            }
            catch (System.Exception e)
            {
                if (_Debug.isValid)
                {
                    Boolean ok = false;

                    do
                    {
                        if (_Debug.isActive == false)
                        {
                            if (_Debug.isValid) _Debug.log("[FILTER " + _LogName + "] \tProblem during research");
                            ok = true;
                        }
                        else
                            Thread.Sleep(500);
                    }
                    while (ok == false);
                }
            }

        }
    }
}
