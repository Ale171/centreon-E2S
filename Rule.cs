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
using System.Collections.Generic;
using System.Text;

namespace centreon_eventLog_syslog
{
    class Rule
    {
        /// <summary>
        /// Get or set EventLog Sources
        /// </summary>
        public string[] EventLogSources
        {
            get
            {
                return _EventLogSources;
            }
            set
            {
                _EventLogSources = value;
            }
        }

        /// <summary>
        /// Get or set EventLog IDs
        /// </summary>
        public string[] EventLogID
        {
            get
            {
                return _EventLogID;
            }
            set
            {
                _EventLogID = value;
            }
        }

        /// <summary>
        /// Get or set EventLog User
        /// </summary>
        public string[] User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
            }
        }

        /// <summary>
        /// Get or set EventLog computer that generate event
        /// </summary>
        public string[] Computer
        {
            get
            {
                return _Computer;
            }
            set
            {
                _Computer = value;
            }
        }

        /// <summary>
        /// Get or set EventLog type
        /// </summary>
        public string[] EventLogType
        {
            get
            {
                return _EventLogType;
            }
            set
            {
                _EventLogType = value;
            }
        }

        /// <summary>
        /// Get or set EventLog Descriptions
        /// </summary>
        public string[] EventLogDescriptions
        {
            get
            {
                return _EventLogDescriptions;
            }
            set
            {
                _EventLogDescriptions = value;
            }
        }

        /// <summary>
        /// Get or set EventLog name
        /// </summary>
        public string EventLogName
        {
            get
            {
                return _EventLogName;
            }
            set
            {
                _EventLogName = value;
            }
        }

        /// <summary>
        /// Get or set syslog level for forward eventLog
        /// </summary>
        public string SyslogLevel
        {
            get
            {
                return _SyslogLevel;
            }
            set
            {
                _SyslogLevel = value;
            }
        }

        /// <summary>
        /// Get or set syslog facility for forward eventLog
        /// </summary>
        public string SyslogFacility
        {
            get
            {
                return _SyslogFacility;
            }
            set
            {
                _SyslogFacility = value;
            }
        }

        /// <summary>
        /// Print all infomration not null
        /// </summary>
        /// <returns>String with all informations</returns>
        override public String ToString()
        {
            String rule = "";

            if (_EventLogSources != null)
            {
                rule = rule + "EventLogSources: ";
                foreach (String value in _EventLogSources)
                {
                    rule = rule + value + ", ";
                }
            }
            if (_EventLogID != null)
            {
                rule = rule + "EventLogID: ";
                foreach (String value in _EventLogID)
                {
                    rule = rule + value + ", ";
                }
            }
            if (_User != null)
            {
                rule = rule + "User: ";
                foreach (String value in _User)
                {
                    rule = rule + value + ", ";
                }
            }
            if (_Computer != null)
            {
                rule = rule + "Computer: ";
                foreach (String value in _Computer)
                {
                    rule = rule + value + ", ";
                }
            }
            if (_EventLogType != null)
            {
                rule = rule + "EventLogType: ";
                foreach (String value in _EventLogType)
                {
                    rule = rule + value + ", ";
                }
            }
            if (_EventLogDescriptions != null)
            {
                rule = rule + "EventLogDescriptions: ";
                foreach (String value in EventLogDescriptions)
                {
                    rule = rule + value + ", ";
                }
            }
            if (_EventLogName != null)
                rule = rule + "EventLogName: " + _EventLogName + ", ";

            if (_SyslogLevel != null)
                rule = rule + "SyslogLevel: " + _SyslogLevel + ", ";

            if (_SyslogFacility != null)
                rule = rule + "SyslogFacility: " + _SyslogFacility + ", ";

            return rule;
        }

        /// <summary>
        /// to know if this object is empty or not
        /// </summary>
        public Boolean isEmpty
        {
            get
            {
                if (_EventLogSources != null)
                    return false;
                else if (_EventLogID != null)
                    return false;
                else if (_User != null)
                    return false;
                else if (_Computer != null)
                    return false;
                else if (_EventLogType != null)
                    return false;
                else if (_EventLogDescriptions != null)
                    return false;
                else if (_EventLogName != null)
                    return false;
                else
                    return true;
            }
            set
            {
                this._isEmpty = value;
            }
        }

        private string[] _EventLogSources = null;
        private string[] _EventLogID = null;
        private string[] _User = null;
        private string[] _Computer = null;
        private string[] _EventLogType = null;
        private string[] _EventLogDescriptions = null;
        private string _EventLogName = null;
        private string _SyslogLevel = null;
        private string _SyslogFacility = null;
        private Boolean _isEmpty;
    }
}
