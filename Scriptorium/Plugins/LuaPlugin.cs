using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;
using Scriptorium;

namespace Scriptorium.Plugins
{
    public class LuaPlugin : Plugin
    {
        string scriptName;

        Lua state;

        public LuaPlugin(string scriptName, bool addPath = true)
        {
            this.scriptName = scriptName;
            fileName = scriptName;

            state = new Lua();

            path = (addPath ? "Plugins/" : "") + scriptName;

            Action<string> LogAction = (message) => { Log(message); };
            state["Log"] = LogAction;
            ClearLog();

            Action<string, string> SaveFileAction = (fileType, contents) => { SaveFile(fileType, contents); };
            state["SaveFile"] = SaveFileAction;

            Action GetDataAction = () => { GetData(); };
            state["GetData"] = GetDataAction;

            try
            {
                state.DoFile(path);
            } catch(NLua.Exceptions.LuaScriptException exception)
            {
                Log($"ERROR {exception.Message}");
            }

            if (state["ShouldRun"] != null)
            {
                shouldRun = (bool)state["ShouldRun"];
            }

            string customName = (string)state["DebugName"];
            menuPath = (string)state["MenuPath"];

            if (!string.IsNullOrEmpty(customName))
            {
                fileName = customName;
            }
        }

        public override void OnRun()
        {
            try
            {
                state.DoFile(path);
                LuaFunction scriptFunc = state["OnRun"] as LuaFunction;

                if (scriptFunc != null)
                {
                    scriptFunc.Call();
                }
            } catch (NLua.Exceptions.LuaScriptException exception)
            {
                Log($"ERROR {exception.Message}");
            }
        }
    }
}
