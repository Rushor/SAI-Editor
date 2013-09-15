﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAI_Editor.Database.Classes;
using SAI_Editor.Properties;

namespace SAI_Editor
{
    public struct TimedActionListOrEntries
    {
        public List<string> entries;
        public SourceTypes sourceTypeOfEntry;
    };

    class SAI_Editor_Manager
    {
        public WorldDatabase worldDatabase { get; set; }
        public SQLiteDatabase sqliteDatabase { get; set; }

        private static object _lock = new object();
        private static SAI_Editor_Manager _instance;

        public static SAI_Editor_Manager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new SAI_Editor_Manager();

                    return _instance;
                }
            }
        }

        public SAI_Editor_Manager()
        {
            worldDatabase = new WorldDatabase(Settings.Default.Host, Settings.Default.Port, Settings.Default.User, Settings.Default.Password, Settings.Default.Database);
            sqliteDatabase = new SQLiteDatabase("parameter_info.db");
        }

        public bool IsNumericString(string str)
        {
            try
            {
                Int32 strInt = Int32.Parse(str);
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        public async Task<TimedActionListOrEntries> GetTimedActionlistsOrEntries(SmartScript smartScript, SourceTypes sourceType)
        {
            TimedActionListOrEntries timedActionListOrEntries = new TimedActionListOrEntries();
            timedActionListOrEntries.entries = new List<string>();
            timedActionListOrEntries.sourceTypeOfEntry = SourceTypes.SourceTypeScriptedActionlist;

            if (sourceType == SourceTypes.SourceTypeScriptedActionlist)
            {
                List<SmartScript> smartScriptsCallingActionlist = await worldDatabase.GetSmartScriptsCallingActionLists();

                if (smartScriptsCallingActionlist != null)
                {
                    foreach (SmartScript _smartScript in smartScriptsCallingActionlist)
                    {
                        switch ((SmartAction)_smartScript.action_type)
                        {
                            case SmartAction.SMART_ACTION_CALL_TIMED_ACTIONLIST:
                            case SmartAction.SMART_ACTION_CALL_RANDOM_TIMED_ACTIONLIST:
                                if (_smartScript.action_param1 == smartScript.entryorguid ||
                                    _smartScript.action_param2 == smartScript.entryorguid ||
                                    _smartScript.action_param3 == smartScript.entryorguid ||
                                    _smartScript.action_param4 == smartScript.entryorguid ||
                                    _smartScript.action_param5 == smartScript.entryorguid ||
                                    _smartScript.action_param6 == smartScript.entryorguid)
                                {
                                    timedActionListOrEntries.entries.Add(_smartScript.entryorguid.ToString());
                                    timedActionListOrEntries.sourceTypeOfEntry = (SourceTypes)_smartScript.source_type;
                                }

                                break;
                            case SmartAction.SMART_ACTION_CALL_RANDOM_RANGE_TIMED_ACTIONLIST:
                                break; // param1 tot param2
                        }
                    }
                }
            }
            else
            {
                switch ((SmartAction)smartScript.action_type)
                {
                    case SmartAction.SMART_ACTION_CALL_TIMED_ACTIONLIST:
                        timedActionListOrEntries.entries.Add(smartScript.action_param1.ToString());
                        break;
                    case SmartAction.SMART_ACTION_CALL_RANDOM_TIMED_ACTIONLIST:
                        timedActionListOrEntries.entries.Add(smartScript.action_param1.ToString());
                        timedActionListOrEntries.entries.Add(smartScript.action_param2.ToString());

                        if (smartScript.action_param3 > 0)
                            timedActionListOrEntries.entries.Add(smartScript.action_param3.ToString());

                        if (smartScript.action_param4 > 0)
                            timedActionListOrEntries.entries.Add(smartScript.action_param4.ToString());

                        if (smartScript.action_param5 > 0)
                            timedActionListOrEntries.entries.Add(smartScript.action_param5.ToString());

                        if (smartScript.action_param6 > 0)
                            timedActionListOrEntries.entries.Add(smartScript.action_param6.ToString());

                        break;
                    case SmartAction.SMART_ACTION_CALL_RANDOM_RANGE_TIMED_ACTIONLIST:
                        for (int i = smartScript.action_param1; i <= smartScript.action_param2; ++i)
                            timedActionListOrEntries.entries.Add(i.ToString());
                        break;
                }
            }

            return timedActionListOrEntries;
        }
    }
}