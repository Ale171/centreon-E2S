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
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace Centreon_EventLog_2_Syslog
{
    class ThreadFilter
    {
        private String _LogName = null;
        private SyslogServer _SyslogServer = null;
        private Debug _Debug = null;
        private DateTime _LastExecTime;
        private DateTime _MaxExecTime;
        private ArrayList _iFilters = null;
        private ArrayList _eFilters = null;
        private ManualResetEvent _DoneEvent;

        /// <summary>
        /// Complex constructor for filter thread
        /// </summary>
        /// <param name="logName">EventLog name</param>
        /// <param name="syslog">Syslog server, protocol and port definition</param>
        /// <param name="rulesList">Rules for find eventLog</param>
        /// <param name="debug">Permit to write debug lines</param>
        /// <param name="lastExecTime">Min time search</param>
        /// <param name="maxExecTime">Mas time search</param>
        /// <param name="doneEvent">ManualResetEvent object to know if process if complit</param>
        public ThreadFilter(String logName, SyslogServer syslogServer, ArrayList iFilters, ArrayList eFilters, ref Debug debug,
            DateTime lastExecTime, DateTime maxExecTime, ManualResetEvent doneEvent)
        {
            _LogName = logName;
            _SyslogServer = syslogServer;
            _Debug = debug;
            _LastExecTime = lastExecTime;
            _MaxExecTime = maxExecTime;
            _iFilters = iFilters;
            _eFilters = eFilters;
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
            EventLogEntry eventLogEntry = null;

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
                this._Debug.Write("Thread " + _LogName, "Unable to load eventLog: \"" + _LogName + "\" entries because : " + e.Message, DateTime.Now);
            }

            try
            {
                int NbLogEntries = eventLogEntryCollection.Count;

                this._Debug.Write("Thread " + _LogName, "Start events control from: " + _LogName, DateTime.Now);
                
                for (int i = NbLogEntries - 1; i > 0; i--)
                {
                    eventLogEntry = eventLogEntryCollection[i];

                    long elapsedTimeMin = eventLogEntry.TimeWritten.Ticks - _LastExecTime.Ticks;
                    long elapsedTimeMax = eventLogEntry.TimeWritten.Ticks - _MaxExecTime.Ticks;

                    // If event are too old (more than last check) break loop
                    if (elapsedTimeMin < 0) break;

                    // While event is contained between last check and program time execution
                    if ((elapsedTimeMin >= 0) && (elapsedTimeMax < 0))
                    {
                        Boolean isExclude = false;
                        Boolean isInclude = false;

                        isExclude = TestEvent(eventLogEntry, this._eFilters);

                        if (!isExclude)
                        {
                            isInclude = TestEvent(eventLogEntry, this._iFilters);
                        }

                        if (isInclude)
                        {
                            this._SyslogServer.SendEvent(eventLogEntry, this._Debug);
                        }
                    }
                    else if (elapsedTimeMin >= 0)
                    {
                        break;
                    }
                }

                // Desctruct all
                eventLog = null;
                eventLogEntry = null;
                eventLogEntryCollection = null;

                this._Debug.Write("Thread " + _LogName, "Finish events control", DateTime.Now);
                _DoneEvent.Set();
            }
            catch (System.Exception e)
            {
                this._Debug.Write("Thread " + _LogName, "Problem during research due to: " + e.Message, DateTime.Now);
                _DoneEvent.Set();
            }
        }

        /// <summary>
        /// Control if event corresponds to a filter
        /// </summary>
        /// <param name="actualEventLog">Event from eventLog</param>
        /// <param name="filters">List of filters</param>
        /// <returns>True if a correspondence is found</returns>
        private Boolean TestEvent(EventLogEntry actualEventLog, ArrayList filters)
        {
            Boolean bEventLogsources = false;
            Boolean bEventLogID = false;
            Boolean bUser = false;
            Boolean bComputer = false;
            Boolean bEventLogType = false;
            Boolean bEventLogDescriptions = false;

            foreach (Filter filter in filters)
            {
                // Check MachineName
                if (filter.Computer == null)
                {
                    bComputer = true;
                }
                else
                {
                    foreach (String Computer in filter.Computer)
                    {
                        if ((Computer.CompareTo("*") == 0) || (Computer.CompareTo(actualEventLog.MachineName) == 0))
                        {
                            bComputer = true;
                            break;
                        }
                    }
                }

                // Check Message
                if (filter.EventLogDescriptions == null)
                {
                    bEventLogDescriptions = true;
                }
                else
                {
                    foreach (String Description in filter.EventLogDescriptions)
                    {
                        if ((Description.CompareTo("*") == 0) || (Description.CompareTo(actualEventLog.Message) == 0))
                        {
                            bEventLogDescriptions = true;
                            break;
                        }
                    }
                }

                // Check EventID
                if (filter.EventLogID == null)
                {
                    bEventLogID = true;
                }
                else
                {
                    foreach (String ID in filter.EventLogID)
                    {
                        if ((ID.CompareTo("*") == 0) || (ID.CompareTo(actualEventLog.EventID.ToString()) == 0))
                        {
                            bEventLogID = true;
                            break;
                        }
                    }
                }

                // Check Source
                if (filter.EventLogSources == null)
                {
                    bEventLogsources = true;
                }
                else
                {
                    foreach (String Source in filter.EventLogSources)
                    {
                        if ((Source.CompareTo("*") == 0) || (Source.CompareTo(actualEventLog.Source) == 0))
                        {
                            bEventLogsources = true;
                            break;
                        }
                    }
                }

                // Check EntryType
                if (filter.EventLogType == null)
                {
                    bEventLogType = true;
                }
                else
                {
                    foreach (String Type in filter.EventLogType)
                    {
                        if ((Type.CompareTo("*") == 0) || (Type.CompareTo(actualEventLog.EntryType.ToString()) == 0))
                        {
                            bEventLogType = true;
                            break;
                        }
                    }
                }

                // Check UserName
                if (filter.User == null)
                {
                    bUser = true;
                }
                else
                {
                    foreach (String User in filter.User)
                    {
                        if ((User.CompareTo("*") == 0) || (User.CompareTo(actualEventLog.UserName) == 0))
                        {
                            bUser = true;
                            break;
                        }
                    }
                }

                if (bEventLogsources && bEventLogID && bUser && bComputer && bEventLogType && bEventLogDescriptions)
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
