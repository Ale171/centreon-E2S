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
using System.Collections.Generic;
using System.Text;

namespace Centreon_EventLog_2_Syslog
{
    /// <summary>
    /// Get informations about debogging option
    /// </summary>
    class DebugInformations
    {
        private Int32 _Level;
        private Int32 _Versobe;
        private Int32 _MaxSize;
        private Int32 _FileNumber;
        private Boolean _DateTimeInName;

        /// <summary>
        /// Simple Constructor, set default value: level=0, verbose=0, maxSize=10, fileNumber=5, dateTimeInName=true
        /// </summary>
        public DebugInformations()
        {
            this._Level = 0;
            this._Versobe = 0;
            this._MaxSize = 10;
            this._FileNumber = 5;
            this._DateTimeInName = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level">1 to active debug</param>
        /// <param name="verbose">[0-2] verbose option</param>
        /// <param name="maxSize">Max size of log file before rotation</param>
        /// <param name="fileNumber">Number of file keep</param>
        /// <param name="dateTimeInName">Include datetime in log file name</param>
        public DebugInformations(Int32 level, Int32 verbose, Int32 maxSize, Int32 fileNumber, Boolean dateTimeInName)
        {
            if (level < 0)
            {
                this._Level = 0;
            }
            else
            {
                this._Level = level;
            }

            if (verbose < 0)
            {
                this._Versobe = 0;
            }
            else
            {
                this._Versobe = verbose;
            }

            if (maxSize < 0)
            {
                this._MaxSize = 0;
            }
            else
            {
                this._MaxSize = maxSize;
            }

            if (fileNumber < 0)
            {
                this._FileNumber = 0;
            }
            else
            {
                this._FileNumber = fileNumber;
            }

            this._DateTimeInName = dateTimeInName;
        }

        /// <summary>
        /// Get or set Level value
        /// </summary>
        public Int32 Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                this._Level = value;
            }
        }

        /// <summary>
        /// Get or set Versobe value
        /// </summary>
        public Int32 Versobe
        {
            get
            {
                return this._Versobe;
            }
            set
            {
                this._Versobe = value;
            }
        }

        /// <summary>
        /// Get or set MaxSize value
        /// </summary>
        public Int32 MaxSize
        {
            get
            {
                return this._MaxSize;
            }
            set
            {
                this._MaxSize = value;
            }
        }

        /// <summary>
        /// Get or set FileNumber value
        /// </summary>
        public Int32 FileNumber
        {
            get
            {
                return this._FileNumber;
            }
            set
            {
                this._FileNumber = value;
            }
        }

        /// <summary>
        /// Get or set DateTimeInName value
        /// </summary>
        public Boolean DateTimeInName
        {
            get
            {
                return this._DateTimeInName;
            }
            set
            {
                this._DateTimeInName = value;
            }
        }
    }
}
