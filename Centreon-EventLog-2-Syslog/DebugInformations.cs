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
